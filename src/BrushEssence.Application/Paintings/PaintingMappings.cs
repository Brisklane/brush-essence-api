using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Paintings;

/// <summary>
/// Explicit, dependency-free mapping between <see cref="Painting"/> entities and
/// their DTOs. Manual mapping keeps conversions compile-time safe and obvious.
/// </summary>
public static class PaintingMappings
{
    public static PaintingDto ToDto(this Painting painting) => new()
    {
        Id = painting.Id,
        Title = painting.Title,
        Description = painting.Description,
        Price = painting.Price,
        Currency = painting.Currency,
        WidthCm = painting.WidthCm,
        HeightCm = painting.HeightCm,
        Medium = painting.Medium,
        ImageUrl = painting.ImageUrl,
        StockQuantity = painting.StockQuantity,
        IsPublished = painting.IsPublished,
        CategoryId = painting.CategoryId,
        CategoryName = painting.Category?.Name,
        CreatedAt = painting.CreatedAt,
    };

    public static IReadOnlyList<PaintingDto> ToDtoList(this IEnumerable<Painting> paintings)
        => paintings.Select(ToDto).ToList();

    public static Painting ToEntity(this CreatePaintingRequest request) => new()
    {
        Title = request.Title.Trim(),
        Description = request.Description,
        Price = request.Price,
        Currency = request.Currency,
        WidthCm = request.WidthCm,
        HeightCm = request.HeightCm,
        Medium = request.Medium,
        ImageUrl = request.ImageUrl,
        StockQuantity = request.StockQuantity,
        CategoryId = request.CategoryId,
    };

    /// <summary>Copies updatable fields from the request onto an existing entity.</summary>
    public static void ApplyUpdate(this Painting painting, UpdatePaintingRequest request)
    {
        painting.Title = request.Title.Trim();
        painting.Description = request.Description;
        painting.Price = request.Price;
        painting.Currency = request.Currency;
        painting.WidthCm = request.WidthCm;
        painting.HeightCm = request.HeightCm;
        painting.Medium = request.Medium;
        painting.ImageUrl = request.ImageUrl;
        painting.StockQuantity = request.StockQuantity;
        painting.IsPublished = request.IsPublished;
        painting.CategoryId = request.CategoryId;
    }
}
