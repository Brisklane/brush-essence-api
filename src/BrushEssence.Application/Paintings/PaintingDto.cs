namespace BrushEssence.Application.Paintings;

/// <summary>Read model returned to API clients for a painting.</summary>
public class PaintingDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = "USD";

    public double WidthCm { get; set; }

    public double HeightCm { get; set; }

    public string? Medium { get; set; }

    public string? ImageUrl { get; set; }

    public int StockQuantity { get; set; }

    public bool IsPublished { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
