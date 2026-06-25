namespace BrushEssence.Application.Orders;

/// <summary>
/// Checkout and order use cases. Read/create operations act on the authenticated
/// caller's own orders; status changes are an administrative operation.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Places an order from the current user's cart: validates stock, snapshots
    /// prices, decrements inventory, and empties the cart — all atomically.
    /// </summary>
    Task<OrderDto> CreateFromCartAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);

    /// <summary>The current user's orders, newest first (history list).</summary>
    Task<IReadOnlyList<OrderSummaryDto>> GetMyOrdersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// A single order's full details. Restricted to its owner (or an admin).
    /// </summary>
    Task<OrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>An order's tracking timeline. Restricted to its owner (or an admin).</summary>
    Task<OrderTrackingDto> GetTrackingAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Advances an order along its lifecycle (admin only). Enforces the allowed
    /// transitions and restores stock when an order is cancelled.
    /// </summary>
    Task<OrderDto> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);
}
