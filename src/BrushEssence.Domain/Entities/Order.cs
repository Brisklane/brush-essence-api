using BrushEssence.Domain.Common;
using BrushEssence.Domain.ValueObjects;

namespace BrushEssence.Domain.Entities;

/// <summary>
/// A customer order created from their cart at checkout. Line prices are
/// snapshotted onto <see cref="OrderItem"/>s, so an order is an immutable record
/// of what was bought and for how much, regardless of later catalogue changes.
/// </summary>
public class Order : AuditableEntity
{
    /// <summary>Owning customer.</summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Human-friendly, unique reference shown to customers (e.g. "BE-20260626-3F7A1C").
    /// </summary>
    public required string OrderNumber { get; set; }

    /// <summary>Email captured at checkout (snapshot of the user's email).</summary>
    public required string CustomerEmail { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Placed;

    public required ShippingAddress ShippingAddress { get; set; }

    /// <summary>ISO 4217 currency the monetary fields are expressed in.</summary>
    public string Currency { get; set; } = "USD";

    /// <summary>Sum of line totals at the time of purchase.</summary>
    public decimal Subtotal { get; set; }

    /// <summary>Shipping charge. Flat 0 for now; kept for future rate logic.</summary>
    public decimal ShippingCost { get; set; }

    /// <summary><see cref="Subtotal"/> + <see cref="ShippingCost"/>.</summary>
    public decimal Total { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public ICollection<OrderItem> Items { get; set; } = [];
    public ICollection<OrderStatusEvent> StatusHistory { get; set; } = [];

    /// <summary>
    /// Moves the order to <paramref name="next"/>, recording a history entry.
    /// Caller is responsible for validating the transition first
    /// (<see cref="OrderStatusWorkflow.CanTransition"/>).
    /// </summary>
    public void TransitionTo(OrderStatus next, string? note = null)
    {
        Status = next;
        StatusHistory.Add(new OrderStatusEvent { Status = next, Note = note });
    }
}
