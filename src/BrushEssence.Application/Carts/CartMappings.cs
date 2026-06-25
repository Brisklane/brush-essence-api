using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Carts;

/// <summary>
/// Explicit, dependency-free mapping from <see cref="Cart"/> aggregates to their
/// DTOs. Totals are derived from each painting's current price, so the cart
/// always reflects live catalogue pricing.
/// </summary>
public static class CartMappings
{
    /// <summary>
    /// Builds the read model. Lines whose painting has been removed from the
    /// catalogue are skipped defensively, though the service prunes them first.
    /// </summary>
    public static CartDto ToDto(this Cart cart)
    {
        var items = cart.Items
            .Where(item => item.Painting is not null)
            .OrderBy(item => item.CreatedAt)
            .Select(ToItemDto)
            .ToList();

        // Use the first line's currency as the cart currency; the store is
        // single-currency in practice, but this keeps the DTO self-describing.
        var currency = items.Count > 0 ? items[0].Currency : "USD";

        return new CartDto
        {
            Id = cart.Id,
            Items = items,
            TotalQuantity = items.Sum(item => item.Quantity),
            Subtotal = items.Sum(item => item.LineTotal),
            Currency = currency,
        };
    }

    private static CartItemDto ToItemDto(CartItem item)
    {
        var painting = item.Painting!;
        return new CartItemDto
        {
            Id = item.Id,
            PaintingId = item.PaintingId,
            Title = painting.Title,
            ImageUrl = painting.ImageUrl,
            UnitPrice = painting.Price,
            Currency = painting.Currency,
            Quantity = item.Quantity,
            StockQuantity = painting.StockQuantity,
            LineTotal = painting.Price * item.Quantity,
        };
    }
}
