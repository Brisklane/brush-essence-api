using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrushEssence.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository(ApplicationDbContext context) : ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Categories.AnyAsync(c => c.Id == id, cancellationToken);

    public Task<bool> NameExistsAsync(
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
        => context.Categories.AnyAsync(
            c => c.Name.ToLower() == name.ToLower() && (excludeId == null || c.Id != excludeId),
            cancellationToken);

    public Task<bool> SlugExistsAsync(
        string slug,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
        => context.Categories.AnyAsync(
            c => c.Slug == slug && (excludeId == null || c.Id != excludeId),
            cancellationToken);

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
        => await context.Categories.AddAsync(category, cancellationToken);

    public void Remove(Category category) => context.Categories.Remove(category);
}
