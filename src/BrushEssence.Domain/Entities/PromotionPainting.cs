namespace BrushEssence.Domain.Entities;

/// <summary>
/// Join entity linking a <see cref="Promotion"/> (scope Paintings) to the
/// specific paintings it targets. Composite key (PromotionId, PaintingId).
/// </summary>
public class PromotionPainting
{
    public Guid PromotionId { get; set; }
    public Guid PaintingId { get; set; }

    // Navigation properties
    public Promotion? Promotion { get; set; }
    public Painting? Painting { get; set; }
}
