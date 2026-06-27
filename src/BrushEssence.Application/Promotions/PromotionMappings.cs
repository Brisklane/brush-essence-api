using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Promotions;

public static class PromotionMappings
{
    public static PromotionDto ToDto(this Promotion promotion, DateTimeOffset now) => new()
    {
        Id = promotion.Id,
        Name = promotion.Name,
        DiscountType = promotion.DiscountType,
        Value = promotion.Value,
        Scope = promotion.Scope,
        CategoryId = promotion.CategoryId,
        CategoryName = promotion.Category?.Name,
        PaintingIds = promotion.PromotionPaintings.Select(pp => pp.PaintingId).ToList(),
        IsActive = promotion.IsActive,
        StartsAt = promotion.StartsAt,
        EndsAt = promotion.EndsAt,
        IsLive = PromotionCalculator.IsLive(promotion, now),
        CreatedAt = promotion.CreatedAt,
    };
}
