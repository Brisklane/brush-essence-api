namespace BrushEssence.Application.Carts;

/// <summary>Read model for a shopping cart returned to API clients.</summary>
public class CartDto
{
    public Guid Id { get; set; }

    public IReadOnlyList<CartItemDto> Items { get; set; } = [];

    /// <summary>Total number of units across all lines.</summary>
    public int TotalQuantity { get; set; }

    /// <summary>Sum of every line total (items only — no shipping or tax).</summary>
    public decimal Subtotal { get; set; }

    /// <summary>ISO 4217 currency the totals are expressed in.</summary>
    public string Currency { get; set; } = "USD";
}

/// <summary>Read model for a single cart line, enriched with painting details.</summary>
public class CartItemDto
{
    public Guid Id { get; set; }

    public Guid PaintingId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    /// <summary>Current unit price of the painting.</summary>
    public decimal UnitPrice { get; set; }

    public string Currency { get; set; } = "USD";

    public int Quantity { get; set; }

    /// <summary>Stock currently available for the painting.</summary>
    public int StockQuantity { get; set; }

    /// <summary><see cref="UnitPrice"/> × <see cref="Quantity"/>.</summary>
    public decimal LineTotal { get; set; }
}
