using BrushEssence.Domain.Common;

namespace BrushEssence.Domain.Entities;

/// <summary>
/// A hand-painted oil painting on canvas offered for sale in the store.
/// </summary>
public class Painting : AuditableEntity
{
    /// <summary>Display name of the artwork.</summary>
    public required string Title { get; set; }

    /// <summary>Long-form description / story behind the piece.</summary>
    public string? Description { get; set; }

    /// <summary>Selling price in the smallest sensible decimal precision.</summary>
    public decimal Price { get; set; }

    /// <summary>ISO 4217 currency code (e.g. "USD", "PKR").</summary>
    public string Currency { get; set; } = "USD";

    /// <summary>Canvas width in centimetres.</summary>
    public double WidthCm { get; set; }

    /// <summary>Canvas height in centimetres.</summary>
    public double HeightCm { get; set; }

    /// <summary>Medium / technique, e.g. "Oil on canvas".</summary>
    public string? Medium { get; set; }

    /// <summary>Primary image URL for the listing.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Available stock. Original paintings are one-of-a-kind, so this is
    /// typically 1, but prints/commissions may carry more.
    /// </summary>
    public int StockQuantity { get; set; } = 1;

    /// <summary>Whether the painting is visible in the public catalogue.</summary>
    public bool IsPublished { get; set; }

    /// <summary>Optional category this painting belongs to.</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Cached average of approved review ratings (0 when there are none).
    /// Recomputed by the review service whenever approved reviews change.
    /// </summary>
    public decimal AverageRating { get; set; }

    /// <summary>Cached count of approved reviews.</summary>
    public int RatingCount { get; set; }

    // Navigation property
    public Category? Category { get; set; }
}
