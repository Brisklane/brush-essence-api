using BrushEssence.Domain.Common;

namespace BrushEssence.Domain.Entities;

/// <summary>
/// A single line in a <see cref="Cart"/>: a painting and how many of it the
/// customer intends to buy. Price is intentionally not snapshotted here — the
/// cart always reflects the painting's current price, and the price is only
/// locked in when an order is placed.
/// </summary>
public class CartItem : AuditableEntity
{
    public Guid CartId { get; set; }

    public Guid PaintingId { get; set; }

    /// <summary>How many units of the painting are in the cart (always ≥ 1).</summary>
    public int Quantity { get; set; }

    // Navigation properties
    public Cart? Cart { get; set; }
    public Painting? Painting { get; set; }
}
