using BrushEssence.Application.Admin;
using BrushEssence.Application.Common.Exceptions;
using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Application.Common.Models;
using BrushEssence.Domain.Common;
using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Orders;

public sealed class OrderService(
    IOrderRepository orders,
    ICartRepository carts,
    ICurrentUser currentUser,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork) : IOrderService
{
    public async Task<OrderDto> CreateFromCartAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireUserId();

        var cart = await carts.GetByUserIdAsync(userId, cancellationToken);
        if (cart is null || cart.Items.Count == 0)
        {
            throw new BadRequestException("Your cart is empty.");
        }

        var now = timeProvider.GetUtcNow();
        var order = new Order
        {
            UserId = userId,
            CustomerEmail = currentUser.Email ?? string.Empty,
            OrderNumber = await GenerateUniqueOrderNumberAsync(now, cancellationToken),
            ShippingAddress = request.ShippingAddress.ToEntity(),
            Status = OrderStatus.Placed,
        };

        var currency = "USD";
        decimal subtotal = 0m;

        foreach (var cartItem in cart.Items)
        {
            var painting = cartItem.Painting;

            // Re-validate at checkout: the cart may have sat for a while.
            if (painting is null || !painting.IsPublished)
            {
                throw new BadRequestException(
                    "An item in your cart is no longer available. Please review your cart.");
            }

            if (painting.StockQuantity < cartItem.Quantity)
            {
                throw new BadRequestException(
                    $"Only {painting.StockQuantity} of \"{painting.Title}\" remain. Please update your cart.");
            }

            currency = painting.Currency;
            var lineTotal = painting.Price * cartItem.Quantity;
            subtotal += lineTotal;

            order.Items.Add(new OrderItem
            {
                PaintingId = painting.Id,
                Title = painting.Title,
                ImageUrl = painting.ImageUrl,
                UnitPrice = painting.Price,
                Quantity = cartItem.Quantity,
                LineTotal = lineTotal,
            });

            // Reserve the stock for this order.
            painting.StockQuantity -= cartItem.Quantity;
        }

        order.Currency = currency;
        order.Subtotal = subtotal;
        order.ShippingCost = 0m;
        order.Total = subtotal;
        order.StatusHistory.Add(new OrderStatusEvent
        {
            Status = OrderStatus.Placed,
            Note = "Order placed.",
        });

        await orders.AddAsync(order, cancellationToken);

        // Empty the cart now that its contents have become an order.
        cart.Items.Clear();

        // One SaveChanges → one transaction: order, stock, and cart commit together.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return order.ToDto();
    }

    public async Task<PagedResult<AdminOrderListItemDto>> GetAllAsync(
        AdminOrderQuery query,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await orders.GetPagedForAdminAsync(query, cancellationToken);

        return new PagedResult<AdminOrderListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<IReadOnlyList<OrderSummaryDto>> GetMyOrdersAsync(
        CancellationToken cancellationToken = default)
        => await orders.GetSummariesByUserAsync(RequireUserId(), cancellationToken);

    public async Task<OrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => (await LoadOwnedOrderAsync(id, cancellationToken)).ToDto();

    public async Task<OrderTrackingDto> GetTrackingAsync(Guid id, CancellationToken cancellationToken = default)
        => (await LoadOwnedOrderAsync(id, cancellationToken)).ToTrackingDto();

    public async Task<OrderDto> UpdateStatusAsync(
        Guid id,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await orders.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order not found.");

        if (!OrderStatusWorkflow.CanTransition(order.Status, request.Status))
        {
            throw new BadRequestException(
                $"An order cannot move from {order.Status} to {request.Status}.");
        }

        // Cancelling releases the reserved stock back to the catalogue.
        if (request.Status == OrderStatus.Cancelled)
        {
            RestoreStock(order);
        }

        order.TransitionTo(request.Status, request.Note);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return order.ToDto();
    }

    // ----- Helpers -----

    private Guid RequireUserId()
        => currentUser.UserId
            ?? throw new AuthenticationException("You must be signed in to manage orders.");

    private bool IsAdmin => currentUser.Roles.Contains(Roles.Admin);

    /// <summary>Loads an order and asserts the caller may see it (owner or admin).</summary>
    private async Task<Order> LoadOwnedOrderAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order not found.");

        if (order.UserId != RequireUserId() && !IsAdmin)
        {
            // Don't reveal that someone else's order exists.
            throw new NotFoundException("Order not found.");
        }

        return order;
    }

    private static void RestoreStock(Order order)
    {
        foreach (var item in order.Items)
        {
            if (item.Painting is not null)
            {
                item.Painting.StockQuantity += item.Quantity;
            }
        }
    }

    private async Task<string> GenerateUniqueOrderNumberAsync(
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var candidate = OrderNumberGenerator.Generate(now);
            if (!await orders.OrderNumberExistsAsync(candidate, cancellationToken))
            {
                return candidate;
            }
        }

        // Effectively unreachable; the unique index is the final backstop.
        throw new ConflictException("Could not allocate a unique order number. Please retry.");
    }
}
