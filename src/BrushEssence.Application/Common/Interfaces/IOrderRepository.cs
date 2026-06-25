using BrushEssence.Application.Orders;
using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Common.Interfaces;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a full order (items + status history) for reading or status updates.
    /// Tracked so the service can mutate it.
    /// </summary>
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Compact, no-tracking summaries for a user's order history (newest first).</summary>
    Task<IReadOnlyList<OrderSummaryDto>> GetSummariesByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>True when an order with the given number already exists.</summary>
    Task<bool> OrderNumberExistsAsync(string orderNumber, CancellationToken cancellationToken = default);
}
