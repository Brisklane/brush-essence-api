using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Promotions;

/// <summary>
/// Public, slim view of a currently-live promotion — just what the storefront
/// banner needs (name, discount and optional countdown end).
/// </summary>
public sealed class ActivePromotionDto
{
    public string Name { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal Value { get; set; }
    public PromotionScope Scope { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
}
