using BrushEssence.Application.Promotions;
using BrushEssence.Domain.Entities;

namespace BrushEssence.UnitTests.Promotions;

public class PromotionCalculatorTests
{
    private static readonly Guid PaintingId = Guid.NewGuid();
    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 6, 27, 12, 0, 0, TimeSpan.Zero);

    private static Promotion Promo(
        DiscountType type,
        decimal value,
        PromotionScope scope = PromotionScope.All,
        bool active = true,
        DateTimeOffset? starts = null,
        DateTimeOffset? ends = null,
        Guid? categoryId = null,
        Guid? paintingId = null)
    {
        var promotion = new Promotion
        {
            Name = "Test",
            DiscountType = type,
            Value = value,
            Scope = scope,
            IsActive = active,
            StartsAt = starts,
            EndsAt = ends,
            CategoryId = categoryId,
        };
        if (paintingId is { } id)
        {
            promotion.PromotionPaintings.Add(new PromotionPainting { PaintingId = id });
        }
        return promotion;
    }

    [Fact]
    public void Percentage_discount_is_applied()
    {
        var promos = new[] { Promo(DiscountType.Percentage, 20m) };
        var result = PromotionCalculator.DiscountedPrice(1000m, PaintingId, CategoryId, promos);
        Assert.Equal(800m, result);
    }

    [Fact]
    public void Fixed_discount_is_applied_and_never_negative()
    {
        var promos = new[] { Promo(DiscountType.FixedAmount, 5000m) };
        var result = PromotionCalculator.DiscountedPrice(1000m, PaintingId, CategoryId, promos);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void Best_discount_wins_when_several_apply()
    {
        var promos = new[]
        {
            Promo(DiscountType.Percentage, 10m),
            Promo(DiscountType.FixedAmount, 300m),
        };
        // 10% off 1000 = 900; 300 off = 700. Best (lowest) is 700.
        var result = PromotionCalculator.DiscountedPrice(1000m, PaintingId, CategoryId, promos);
        Assert.Equal(700m, result);
    }

    [Fact]
    public void Returns_null_when_no_promotion_applies()
    {
        var promos = new[]
        {
            Promo(DiscountType.Percentage, 20m, PromotionScope.Category, categoryId: Guid.NewGuid()),
        };
        var result = PromotionCalculator.DiscountedPrice(1000m, PaintingId, CategoryId, promos);
        Assert.Null(result);
    }

    [Fact]
    public void Category_scope_targets_only_matching_paintings()
    {
        var promo = Promo(DiscountType.Percentage, 50m, PromotionScope.Category, categoryId: CategoryId);
        Assert.True(PromotionCalculator.AppliesTo(promo, PaintingId, CategoryId));
        Assert.False(PromotionCalculator.AppliesTo(promo, PaintingId, Guid.NewGuid()));
    }

    [Fact]
    public void Paintings_scope_targets_only_listed_paintings()
    {
        var promo = Promo(DiscountType.Percentage, 50m, PromotionScope.Paintings, paintingId: PaintingId);
        Assert.True(PromotionCalculator.AppliesTo(promo, PaintingId, CategoryId));
        Assert.False(PromotionCalculator.AppliesTo(promo, Guid.NewGuid(), CategoryId));
    }

    [Theory]
    [InlineData(false, null, null, false)] // inactive
    [InlineData(true, null, null, true)] // active, no window
    [InlineData(true, -1, 1, true)] // within window (days relative to now)
    [InlineData(true, 1, 2, false)] // starts in the future
    [InlineData(true, -2, -1, false)] // already ended
    public void IsLive_respects_active_flag_and_window(
        bool active, int? startOffsetDays, int? endOffsetDays, bool expected)
    {
        var promo = Promo(
            DiscountType.Percentage, 10m,
            active: active,
            starts: startOffsetDays is { } s ? Now.AddDays(s) : null,
            ends: endOffsetDays is { } e ? Now.AddDays(e) : null);

        Assert.Equal(expected, PromotionCalculator.IsLive(promo, Now));
    }
}
