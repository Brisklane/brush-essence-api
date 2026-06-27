namespace BrushEssence.Application.Paintings;

/// <summary>Read model returned to API clients for a painting.</summary>
public class PaintingDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    /// <summary>Sale price when a promotion applies; null at full price.</summary>
    public decimal? DiscountedPrice { get; set; }

    public string Currency { get; set; } = "PKR";

    public double WidthCm { get; set; }

    public double HeightCm { get; set; }

    public Guid? MediumId { get; set; }

    public string? MediumName { get; set; }

    public string? ImageUrl { get; set; }

    public int StockQuantity { get; set; }

    public bool IsPublished { get; set; }

    public Guid? CategoryId { get; set; }

    public string? CategoryName { get; set; }

    /// <summary>Cached average of approved review ratings (0 when none).</summary>
    public decimal AverageRating { get; set; }

    /// <summary>Cached count of approved reviews.</summary>
    public int RatingCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
