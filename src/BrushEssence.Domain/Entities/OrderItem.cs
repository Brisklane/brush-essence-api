using BrushEssence.Domain.Common;

namespace BrushEssence.Domain.Entities;

/// <summary>
/// A purchased line on an <see cref="Order"/>. Product details (title, price,
/// image) are snapshotted at purchase time so the order is a faithful historical
/// record even if the painting is later edited or removed.
/// </summary>
public class OrderItem : AuditableEntity
{
    public Guid OrderId { get; set; }

    /// <summary>
    /// The painting bought. Nullable because a painting may be deleted later; the
    /// snapshot fields below preserve what was actually ordered regardless.
    /// </summary>
    public Guid? PaintingId { get; set; }

    /// <summary>Painting title at purchase time.</summary>
    public required string Title { get; set; }

    /// <summary>Painting image URL at purchase time.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Unit price locked in at purchase time.</summary>
    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    /// <summary><see cref="UnitPrice"/> × <see cref="Quantity"/>, locked in.</summary>
    public decimal LineTotal { get; set; }

    // Navigation properties
    public Order? Order { get; set; }
    public Painting? Painting { get; set; }
}
