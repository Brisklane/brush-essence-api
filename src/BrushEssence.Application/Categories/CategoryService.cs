using BrushEssence.Application.Common;
using BrushEssence.Application.Common.Exceptions;
using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Categories;

public sealed class CategoryService(
    ICategoryRepository categories,
    IUnitOfWork unitOfWork) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => (await categories.GetAllAsync(cancellationToken)).ToDtoList();

    public async Task<CategoryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await categories.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        return category.ToDto();
    }

    public async Task<CategoryDto> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();

        if (await categories.NameExistsAsync(name, null, cancellationToken))
        {
            throw new ConflictException("A category with this name already exists.");
        }

        var category = new Category
        {
            Name = name,
            Slug = await GenerateUniqueSlugAsync(name, null, cancellationToken),
            Description = request.Description,
        };

        await categories.AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return category.ToDto();
    }

    public async Task<CategoryDto> UpdateAsync(
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await categories.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        var name = request.Name.Trim();

        if (await categories.NameExistsAsync(name, id, cancellationToken))
        {
            throw new ConflictException("A category with this name already exists.");
        }

        // Re-slug only when the name actually changes.
        if (!string.Equals(category.Name, name, StringComparison.OrdinalIgnoreCase))
        {
            category.Slug = await GenerateUniqueSlugAsync(name, id, cancellationToken);
        }

        category.Name = name;
        category.Description = request.Description;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return category.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await categories.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        // Paintings keep existing but become uncategorized (FK is ON DELETE SET NULL).
        categories.Remove(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> GenerateUniqueSlugAsync(
        string name,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var baseSlug = SlugGenerator.Generate(name);
        if (string.IsNullOrEmpty(baseSlug))
        {
            baseSlug = "category";
        }

        var slug = baseSlug;
        var suffix = 2;
        while (await categories.SlugExistsAsync(slug, excludeId, cancellationToken))
        {
            slug = $"{baseSlug}-{suffix++}";
        }

        return slug;
    }
}
