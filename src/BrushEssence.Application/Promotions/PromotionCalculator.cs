using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Promotions;

/// <summary>
/// Pure pricing logic for promotions — the single source of truth used by the
/// catalogue, cart and checkout so a painting's discounted price is identical
/// everywhere. "Best discount wins" when several promotions apply.
/// </summary>
public static class PromotionCalculator
{
    /// <summary>True when a promotion is active and within its date window.</summary>
    public static bool IsLive(Promotion promotion, DateTimeOffset now)
        => promotion.IsActive
           && (promotion.StartsAt is null || now >= promotion.StartsAt)
           && (promotion.EndsAt is null || now <= promotion.EndsAt);

    /// <summary>True when a promotion targets the given painting.</summary>
    public static bool AppliesTo(Promotion promotion, Guid paintingId, Guid? categoryId)
        => promotion.Scope switch
        {
            PromotionScope.All => true,
            PromotionScope.Category => categoryId is { } c && promotion.CategoryId == c,
            PromotionScope.Paintings => promotion.PromotionPaintings.Any(pp => pp.PaintingId == paintingId),
            _ => false,
        };

    /// <summary>The amount a single promotion takes off the given price.</summary>
    public static decimal DiscountAmount(Promotion promotion, decimal price)
        => promotion.DiscountType == DiscountType.Percentage
            ? price * (promotion.Value / 100m)
            : Math.Min(promotion.Value, price);

    /// <summary>
    /// The best discounted price for a painting, or null when no live promotion
    /// applies. Rounded to a whole unit (the store charges whole PKR).
    /// </summary>
    public static decimal? DiscountedPrice(
        decimal price,
        Guid paintingId,
        Guid? categoryId,
        IReadOnlyList<Promotion> livePromotions)
    {
        decimal best = 0m;
        foreach (var promotion in livePromotions)
        {
            if (AppliesTo(promotion, paintingId, categoryId))
            {
                var discount = DiscountAmount(promotion, price);
                if (discount > best)
                {
                    best = discount;
                }
            }
        }

        if (best <= 0)
        {
            return null;
        }

        var final = Math.Round(price - best, 0, MidpointRounding.AwayFromZero);
        return final < 0 ? 0 : final;
    }

    /// <summary>The price actually charged: discounted when applicable, else original.</summary>
    public static decimal EffectivePrice(
        decimal price,
        Guid paintingId,
        Guid? categoryId,
        IReadOnlyList<Promotion> livePromotions)
        => DiscountedPrice(price, paintingId, categoryId, livePromotions) ?? price;
}
