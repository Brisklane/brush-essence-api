using BrushEssence.Domain.Common;

namespace BrushEssence.Domain.Entities;

/// <summary>
/// A discount campaign. Applies to all paintings, one category, or a hand-picked
/// set, and is live only while active and within its optional date window.
/// </summary>
public class Promotion : AuditableEntity
{
    /// <summary>Display name (e.g. "Summer Sale").</summary>
    public required string Name { get; set; }

    public DiscountType DiscountType { get; set; }

    /// <summary>Percentage (0–100) or fixed amount, per <see cref="DiscountType"/>.</summary>
    public decimal Value { get; set; }

    public PromotionScope Scope { get; set; }

    /// <summary>Target category, when <see cref="Scope"/> is Category.</summary>
    public Guid? CategoryId { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Optional start of the active window (inclusive).</summary>
    public DateTimeOffset? StartsAt { get; set; }

    /// <summary>Optional end of the active window (inclusive).</summary>
    public DateTimeOffset? EndsAt { get; set; }

    // Navigation properties
    public Category? Category { get; set; }
    public ICollection<PromotionPainting> PromotionPaintings { get; set; } = [];
}
