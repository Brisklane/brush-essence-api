using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Application.Paintings;
using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrushEssence.Infrastructure.Persistence.Repositories;

public sealed class PaintingRepository(ApplicationDbContext context) : IPaintingRepository
{
    public async Task<(IReadOnlyList<Painting> Items, int TotalCount)> GetPagedAsync(
        PaintingQuery query,
        CancellationToken cancellationToken = default)
    {
        var queryable = context.Paintings
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            queryable = queryable.Where(p =>
                EF.Functions.ILike(p.Title, term) ||
                (p.Description != null && EF.Functions.ILike(p.Description, term)));
        }

        if (query.CategoryId is { } categoryId)
        {
            queryable = queryable.Where(p => p.CategoryId == categoryId);
        }

        if (query.IsPublished is { } isPublished)
        {
            queryable = queryable.Where(p => p.IsPublished == isPublished);
        }

        var totalCount = await queryable.CountAsync(cancellationToken);

        var items = await queryable
            .OrderByDescending(p => p.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Painting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Paintings
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task AddAsync(Painting painting, CancellationToken cancellationToken = default)
        => await context.Paintings.AddAsync(painting, cancellationToken);

    public void Remove(Painting painting) => context.Paintings.Remove(painting);
}
