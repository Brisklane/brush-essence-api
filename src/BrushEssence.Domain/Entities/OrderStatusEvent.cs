using BrushEssence.Domain.Common;

namespace BrushEssence.Domain.Entities;

/// <summary>
/// A single entry in an order's status timeline, recorded on every transition.
/// Powers order tracking — the customer sees when each milestone was reached
/// (<see cref="AuditableEntity.CreatedAt"/> is the time it happened).
/// </summary>
public class OrderStatusEvent : AuditableEntity
{
    public Guid OrderId { get; set; }

    public OrderStatus Status { get; set; }

    /// <summary>Optional human-readable note (e.g. a carrier tracking number).</summary>
    public string? Note { get; set; }

    // Navigation property
    public Order? Order { get; set; }
}
