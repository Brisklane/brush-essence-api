namespace BrushEssence.Application.Paintings;

/// <summary>Write model used when adding a new painting to the catalogue.</summary>
public class CreatePaintingRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public double WidthCm { get; set; }

    public double HeightCm { get; set; }

    public Guid? MediumId { get; set; }

    public string? ImageUrl { get; set; }

    public int StockQuantity { get; set; } = 1;

    public bool IsPublished { get; set; }

    public Guid? CategoryId { get; set; }
}
