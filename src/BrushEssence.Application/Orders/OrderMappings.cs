using BrushEssence.Domain.Entities;
using BrushEssence.Domain.ValueObjects;

namespace BrushEssence.Application.Orders;

/// <summary>Explicit, dependency-free mapping for orders and their parts.</summary>
public static class OrderMappings
{
    public static OrderDto ToDto(this Order order) => new()
    {
        Id = order.Id,
        OrderNumber = order.OrderNumber,
        CustomerEmail = order.CustomerEmail,
        Status = order.Status,
        ShippingAddress = order.ShippingAddress.ToDto(),
        Currency = order.Currency,
        Subtotal = order.Subtotal,
        ShippingCost = order.ShippingCost,
        Total = order.Total,
        Items = order.Items
            .OrderBy(item => item.CreatedAt)
            .Select(ToItemDto)
            .ToList(),
        CreatedAt = order.CreatedAt,
    };

    public static OrderTrackingDto ToTrackingDto(this Order order) => new()
    {
        Id = order.Id,
        OrderNumber = order.OrderNumber,
        Status = order.Status,
        IsComplete = OrderStatusWorkflow.IsTerminal(order.Status),
        History = order.StatusHistory
            .OrderBy(e => e.CreatedAt)
            .Select(e => new OrderStatusEventDto
            {
                Status = e.Status,
                Note = e.Note,
                OccurredAt = e.CreatedAt,
            })
            .ToList(),
    };

    private static OrderItemDto ToItemDto(OrderItem item) => new()
    {
        Id = item.Id,
        PaintingId = item.PaintingId,
        Title = item.Title,
        ImageUrl = item.ImageUrl,
        UnitPrice = item.UnitPrice,
        Quantity = item.Quantity,
        LineTotal = item.LineTotal,
    };

    private static ShippingAddressDto ToDto(this ShippingAddress address) => new()
    {
        FullName = address.FullName,
        Line1 = address.Line1,
        Line2 = address.Line2,
        City = address.City,
        Region = address.Region,
        PostalCode = address.PostalCode,
        Country = address.Country,
        Phone = address.Phone,
    };

    public static ShippingAddress ToEntity(this ShippingAddressInput input) => new()
    {
        FullName = input.FullName.Trim(),
        Line1 = input.Line1.Trim(),
        Line2 = string.IsNullOrWhiteSpace(input.Line2) ? null : input.Line2.Trim(),
        City = input.City.Trim(),
        Region = string.IsNullOrWhiteSpace(input.Region) ? null : input.Region.Trim(),
        PostalCode = input.PostalCode.Trim(),
        Country = input.Country.Trim(),
        Phone = string.IsNullOrWhiteSpace(input.Phone) ? null : input.Phone.Trim(),
    };
}
