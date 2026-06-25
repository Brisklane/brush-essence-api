using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Application.Paintings;
using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrushEssence.Infrastructure.Persistence.Repositories;

public sealed class PaintingRepository(ApplicationDbContext context) : IPaintingRepository
{
    public async Task<(IReadOnlyList<PaintingDto> Items, int TotalCount)> GetPagedAsync(
        PaintingQuery query,
        CancellationToken cancellationToken = default)
    {
        var queryable = context.Paintings.AsNoTracking();

        queryable = ApplyFilters(queryable, query);

        var totalCount = await queryable.CountAsync(cancellationToken);

        var items = await ApplySort(queryable, query.SortOrder)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(PaintingMappings.ToDtoProjection)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<PaintingDto?> GetDtoByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Paintings
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(PaintingMappings.ToDtoProjection)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<Painting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Paintings
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task AddAsync(Painting painting, CancellationToken cancellationToken = default)
        => await context.Paintings.AddAsync(painting, cancellationToken);

    public void Remove(Painting painting) => context.Paintings.Remove(painting);

    private static IQueryable<Painting> ApplyFilters(IQueryable<Painting> queryable, PaintingQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            queryable = queryable.Where(p =>
                EF.Functions.ILike(p.Title, term) ||
                (p.Description != null && EF.Functions.ILike(p.Description, term)));
        }

        if (query.CategoryIds is { Count: > 0 } categoryIds)
        {
            queryable = queryable.Where(p => p.CategoryId != null && categoryIds.Contains(p.CategoryId.Value));
        }
        else if (query.CategoryId is { } categoryId)
        {
            queryable = queryable.Where(p => p.CategoryId == categoryId);
        }

        if (query.MinPrice is { } minPrice)
        {
            queryable = queryable.Where(p => p.Price >= minPrice);
        }

        if (query.MaxPrice is { } maxPrice)
        {
            queryable = queryable.Where(p => p.Price <= maxPrice);
        }

        if (!string.IsNullOrWhiteSpace(query.Medium))
        {
            var medium = $"%{query.Medium.Trim()}%";
            queryable = queryable.Where(p => p.Medium != null && EF.Functions.ILike(p.Medium, medium));
        }

        if (query.IsPublished is { } isPublished)
        {
            queryable = queryable.Where(p => p.IsPublished == isPublished);
        }

        return queryable;
    }

    // A stable secondary sort on Id keeps pagination deterministic when the
    // primary key (e.g. price or title) has ties.
    private static IQueryable<Painting> ApplySort(IQueryable<Painting> queryable, PaintingSort sort) => sort switch
    {
        PaintingSort.Oldest => queryable.OrderBy(p => p.CreatedAt).ThenBy(p => p.Id),
        PaintingSort.PriceAsc => queryable.OrderBy(p => p.Price).ThenBy(p => p.Id),
        PaintingSort.PriceDesc => queryable.OrderByDescending(p => p.Price).ThenBy(p => p.Id),
        PaintingSort.TitleAsc => queryable.OrderBy(p => p.Title).ThenBy(p => p.Id),
        PaintingSort.TitleDesc => queryable.OrderByDescending(p => p.Title).ThenBy(p => p.Id),
        _ => queryable.OrderByDescending(p => p.CreatedAt).ThenBy(p => p.Id),
    };
}
