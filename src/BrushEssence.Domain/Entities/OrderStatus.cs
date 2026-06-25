namespace BrushEssence.Domain.Entities;

/// <summary>
/// Lifecycle states an order moves through. Stored as a string in the database
/// (see configuration) so the values stay readable and stable across releases.
/// </summary>
public enum OrderStatus
{
    /// <summary>Order has been placed and paid for (or awaiting fulfilment).</summary>
    Placed = 0,

    /// <summary>Being prepared / packed.</summary>
    Processing = 1,

    /// <summary>Handed to the carrier and in transit.</summary>
    Shipped = 2,

    /// <summary>Received by the customer. Terminal (success).</summary>
    Delivered = 3,

    /// <summary>Cancelled before delivery. Terminal (stock is restored).</summary>
    Cancelled = 4,
}
