using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Paintings;

/// <summary>
/// Explicit, dependency-free mapping between <see cref="Painting"/> entities and
/// their DTOs. Manual mapping keeps conversions compile-time safe and obvious;
/// if the catalogue grows to many DTOs, this is the seam to swap in a
/// source-generated mapper (e.g. Mapperly) later.
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
        CreatedAt = painting.CreatedAt,
    };

    public static IReadOnlyList<PaintingDto> ToDtoList(this IEnumerable<Painting> paintings)
        => paintings.Select(ToDto).ToList();

    public static Painting ToEntity(this CreatePaintingRequest request) => new()
    {
        Title = request.Title,
        Description = request.Description,
        Price = request.Price,
        Currency = request.Currency,
        WidthCm = request.WidthCm,
        HeightCm = request.HeightCm,
        Medium = request.Medium,
        ImageUrl = request.ImageUrl,
        StockQuantity = request.StockQuantity,
    };
}
