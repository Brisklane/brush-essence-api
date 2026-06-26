using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Application.Common.Models;
using BrushEssence.Application.Reviews;
using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrushEssence.Infrastructure.Persistence.Repositories;

public sealed class ReviewRepository(ApplicationDbContext context) : IReviewRepository
{
    public async Task AddAsync(Review review, CancellationToken cancellationToken = default)
        => await context.Reviews.AddAsync(review, cancellationToken);

    public Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Reviews
            .Include(r => r.Painting)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<bool> ExistsForUserAsync(Guid paintingId, Guid userId, CancellationToken cancellationToken = default)
        => context.Reviews.AnyAsync(r => r.PaintingId == paintingId && r.UserId == userId, cancellationToken);

    public Task<ReviewDto?> GetUserReviewAsync(Guid paintingId, Guid userId, CancellationToken cancellationToken = default)
        => context.Reviews
            .AsNoTracking()
            .Where(r => r.PaintingId == paintingId && r.UserId == userId)
            .Select(ReviewMappings.ToDtoProjection)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<(IReadOnlyList<ReviewDto> Items, int TotalCount)> GetApprovedForPaintingAsync(
        Guid paintingId,
        PagedQuery query,
        CancellationToken cancellationToken = default)
    {
        var queryable = context.Reviews
            .AsNoTracking()
            .Where(r => r.PaintingId == paintingId && r.Status == ReviewStatus.Approved);

        var totalCount = await queryable.CountAsync(cancellationToken);

        var items = await queryable
            .OrderByDescending(r => r.CreatedAt)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .Select(ReviewMappings.ToDtoProjection)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(int Count, decimal Average)> GetApprovedAggregateAsync(
        Guid paintingId,
        CancellationToken cancellationToken = default)
    {
        var approved = context.Reviews
            .Where(r => r.PaintingId == paintingId && r.Status == ReviewStatus.Approved);

        var count = await approved.CountAsync(cancellationToken);
        if (count == 0)
        {
            return (0, 0m);
        }

        var sum = await approved.SumAsync(r => r.Rating, cancellationToken);
        return (count, (decimal)sum / count);
    }

    public async Task<IReadOnlyDictionary<int, int>> GetApprovedDistributionAsync(
        Guid paintingId,
        CancellationToken cancellationToken = default)
        => await context.Reviews
            .Where(r => r.PaintingId == paintingId && r.Status == ReviewStatus.Approved)
            .GroupBy(r => r.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Rating, x => x.Count, cancellationToken);

    public async Task<(IReadOnlyList<AdminReviewListItemDto> Items, int TotalCount)> GetPagedForAdminAsync(
        AdminReviewQuery query,
        CancellationToken cancellationToken = default)
    {
        var queryable = context.Reviews.AsNoTracking();

        if (query.StatusFilter is { } status)
        {
            queryable = queryable.Where(r => r.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            queryable = queryable.Where(r =>
                EF.Functions.ILike(r.Painting!.Title, term) ||
                EF.Functions.ILike(r.CustomerEmail, term) ||
                (r.Comment != null && EF.Functions.ILike(r.Comment, term)));
        }

        var totalCount = await queryable.CountAsync(cancellationToken);

        var items = await queryable
            .OrderByDescending(r => r.CreatedAt)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .Select(r => new AdminReviewListItemDto
            {
                Id = r.Id,
                PaintingId = r.PaintingId,
                PaintingTitle = r.Painting!.Title,
                CustomerEmail = r.CustomerEmail,
                Rating = r.Rating,
                Title = r.Title,
                Comment = r.Comment,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public void Remove(Review review) => context.Reviews.Remove(review);
}
