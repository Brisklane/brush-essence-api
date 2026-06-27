using BrushEssence.Application.Promotions;
using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Carts;

/// <summary>
/// Explicit, dependency-free mapping from <see cref="Cart"/> aggregates to their
/// DTOs. Unit prices reflect each painting's current price *after* any live
/// promotion, so the cart shows exactly what the customer will be charged.
/// </summary>
public static class CartMappings
{
    /// <summary>
    /// Builds the read model. Lines whose painting has been removed from the
    /// catalogue are skipped defensively, though the service prunes them first.
    /// </summary>
    public static CartDto ToDto(this Cart cart, IReadOnlyList<Promotion> livePromotions)
    {
        var items = cart.Items
            .Where(item => item.Painting is not null)
            .OrderBy(item => item.CreatedAt)
            .Select(item => ToItemDto(item, livePromotions))
            .ToList();

        // Use the first line's currency as the cart currency; the store is
        // single-currency in practice, but this keeps the DTO self-describing.
        var currency = items.Count > 0 ? items[0].Currency : "PKR";

        return new CartDto
        {
            Id = cart.Id,
            Items = items,
            TotalQuantity = items.Sum(item => item.Quantity),
            Subtotal = items.Sum(item => item.LineTotal),
            Currency = currency,
        };
    }

    private static CartItemDto ToItemDto(CartItem item, IReadOnlyList<Promotion> livePromotions)
    {
        var painting = item.Painting!;
        var unitPrice = PromotionCalculator.EffectivePrice(
            painting.Price, painting.Id, painting.CategoryId, livePromotions);

        return new CartItemDto
        {
            Id = item.Id,
            PaintingId = item.PaintingId,
            Title = painting.Title,
            ImageUrl = painting.ImageUrl,
            UnitPrice = unitPrice,
            Currency = painting.Currency,
            Quantity = item.Quantity,
            StockQuantity = painting.StockQuantity,
            LineTotal = unitPrice * item.Quantity,
        };
    }
}
