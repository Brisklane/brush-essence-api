namespace BrushEssence.Application.Paintings;

/// <summary>Write model for updating an existing painting.</summary>
public sealed class UpdatePaintingRequest
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public double WidthCm { get; init; }
    public double HeightCm { get; init; }
    public Guid? MediumId { get; init; }
    public string? ImageUrl { get; init; }
    public int StockQuantity { get; init; } = 1;
    public bool IsPublished { get; init; }
    public Guid? CategoryId { get; init; }
}
