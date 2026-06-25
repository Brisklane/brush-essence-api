using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Categories;

public static class CategoryMappings
{
    public static CategoryDto ToDto(this Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Slug = category.Slug,
        Description = category.Description,
        CreatedAt = category.CreatedAt,
    };

    public static IReadOnlyList<CategoryDto> ToDtoList(this IEnumerable<Category> categories)
        => categories.Select(ToDto).ToList();
}
