namespace BrushEssence.Domain.Entities;

/// <summary>What a promotion applies to. Stored as a string.</summary>
public enum PromotionScope
{
    /// <summary>Every painting.</summary>
    All = 0,

    /// <summary>Every painting in one category.</summary>
    Category = 1,

    /// <summary>A specific, hand-picked set of paintings.</summary>
    Paintings = 2,
}
