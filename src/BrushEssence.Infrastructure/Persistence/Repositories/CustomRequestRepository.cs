using BrushEssence.Application.Admin;
using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Application.CustomRequests;
using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrushEssence.Infrastructure.Persistence.Repositories;

public sealed class CustomRequestRepository(ApplicationDbContext context) : ICustomRequestRepository
{
    public async Task AddAsync(CustomRequest request, CancellationToken cancellationToken = default)
        => await context.CustomRequests.AddAsync(request, cancellationToken);

    public async Task<(IReadOnlyList<AdminCustomRequestListItemDto> Items, int TotalCount)> GetPagedForAdminAsync(
        AdminCustomRequestQuery query,
        CancellationToken cancellationToken = default)
    {
        var queryable = context.CustomRequests.AsNoTracking();

        if (query.StatusFilter is { } status)
        {
            queryable = queryable.Where(r => r.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            queryable = queryable.Where(r =>
                EF.Functions.ILike(r.Title, term) ||
                EF.Functions.ILike(r.CustomerEmail, term));
        }

        var totalCount = await queryable.CountAsync(cancellationToken);

        var items = await queryable
            .OrderByDescending(r => r.CreatedAt)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .Select(r => new AdminCustomRequestListItemDto
            {
                Id = r.Id,
                Title = r.Title,
                CustomerEmail = r.CustomerEmail,
                Status = r.Status,
                ImageCount = r.Images.Count,
                CreatedAt = r.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<CustomRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.CustomRequests
            .Include(r => r.Images)
            .Include(r => r.StatusHistory)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IReadOnlyList<CustomRequestSummaryDto>> GetSummariesByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await context.CustomRequests
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new CustomRequestSummaryDto
            {
                Id = r.Id,
                Title = r.Title,
                Status = r.Status,
                ImageCount = r.Images.Count,
                CreatedAt = r.CreatedAt,
            })
            .ToListAsync(cancellationToken);
}
