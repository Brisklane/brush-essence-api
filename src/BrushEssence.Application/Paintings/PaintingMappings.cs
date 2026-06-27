using System.Linq.Expressions;

using BrushEssence.Domain.Common;
using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Paintings;

/// <summary>
/// Explicit, dependency-free mapping between <see cref="Painting"/> entities and
/// their DTOs. Manual mapping keeps conversions compile-time safe and obvious.
/// </summary>
public static class PaintingMappings
{
    /// <summary>
    /// EF Core projection used by read queries. Selecting straight into the DTO
    /// lets the database return only the columns we need (and a single LEFT JOIN
    /// for the category name) instead of materialising whole entity graphs.
    /// </summary>
    public static readonly Expression<Func<Painting, PaintingDto>> ToDtoProjection = painting => new PaintingDto
    {
        Id = painting.Id,
        Title = painting.Title,
        Description = painting.Description,
        Price = painting.Price,
        Currency = painting.Currency,
        WidthCm = painting.WidthCm,
        HeightCm = painting.HeightCm,
        MediumId = painting.MediumId,
        MediumName = painting.Medium != null ? painting.Medium.Name : null,
        ImageUrl = painting.ImageUrl,
        StockQuantity = painting.StockQuantity,
        IsPublished = painting.IsPublished,
        CategoryId = painting.CategoryId,
        CategoryName = painting.Category != null ? painting.Category.Name : null,
        AverageRating = painting.AverageRating,
        RatingCount = painting.RatingCount,
        CreatedAt = painting.CreatedAt,
    };

    public static PaintingDto ToDto(this Painting painting) => new()
    {
        Id = painting.Id,
        Title = painting.Title,
        Description = painting.Description,
        Price = painting.Price,
        Currency = painting.Currency,
        WidthCm = painting.WidthCm,
        HeightCm = painting.HeightCm,
        MediumId = painting.MediumId,
        MediumName = painting.Medium?.Name,
        ImageUrl = painting.ImageUrl,
        StockQuantity = painting.StockQuantity,
        IsPublished = painting.IsPublished,
        CategoryId = painting.CategoryId,
        CategoryName = painting.Category?.Name,
        AverageRating = painting.AverageRating,
        RatingCount = painting.RatingCount,
        CreatedAt = painting.CreatedAt,
    };

    public static IReadOnlyList<PaintingDto> ToDtoList(this IEnumerable<Painting> paintings)
        => paintings.Select(ToDto).ToList();

    public static Painting ToEntity(this CreatePaintingRequest request) => new()
    {
        Title = request.Title.Trim(),
        Description = request.Description,
        Price = request.Price,
        Currency = StoreDefaults.Currency,
        WidthCm = request.WidthCm,
        HeightCm = request.HeightCm,
        MediumId = request.MediumId,
        ImageUrl = request.ImageUrl,
        StockQuantity = request.StockQuantity,
        IsPublished = request.IsPublished,
        CategoryId = request.CategoryId,
    };

    /// <summary>Copies updatable fields from the request onto an existing entity.</summary>
    public static void ApplyUpdate(this Painting painting, UpdatePaintingRequest request)
    {
        painting.Title = request.Title.Trim();
        painting.Description = request.Description;
        painting.Price = request.Price;
        painting.Currency = StoreDefaults.Currency;
        painting.WidthCm = request.WidthCm;
        painting.HeightCm = request.HeightCm;
        painting.MediumId = request.MediumId;
        painting.ImageUrl = request.ImageUrl;
        painting.StockQuantity = request.StockQuantity;
        painting.IsPublished = request.IsPublished;
        painting.CategoryId = request.CategoryId;
    }
}
