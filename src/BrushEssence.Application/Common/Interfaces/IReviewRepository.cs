using BrushEssence.Application.Common.Models;
using BrushEssence.Application.Reviews;
using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Common.Interfaces;

public interface IReviewRepository
{
    Task AddAsync(Review review, CancellationToken cancellationToken = default);

    /// <summary>Loads a tracked review for moderation/editing.</summary>
    Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>True when the user already has a review for the painting.</summary>
    Task<bool> ExistsForUserAsync(Guid paintingId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>The caller's own review for a painting (any status), or null.</summary>
    Task<ReviewDto?> GetUserReviewAsync(Guid paintingId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Paged approved reviews for a painting (newest first), projected.</summary>
    Task<(IReadOnlyList<ReviewDto> Items, int TotalCount)> GetApprovedForPaintingAsync(
        Guid paintingId,
        PagedQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>Approved (count, average) for a painting; average is 0 when none.</summary>
    Task<(int Count, decimal Average)> GetApprovedAggregateAsync(Guid paintingId, CancellationToken cancellationToken = default);

    /// <summary>Approved rating distribution for a painting (stars 1–5 → count).</summary>
    Task<IReadOnlyDictionary<int, int>> GetApprovedDistributionAsync(Guid paintingId, CancellationToken cancellationToken = default);

    /// <summary>Paged, filtered admin moderation listing.</summary>
    Task<(IReadOnlyList<AdminReviewListItemDto> Items, int TotalCount)> GetPagedForAdminAsync(
        AdminReviewQuery query,
        CancellationToken cancellationToken = default);

    void Remove(Review review);
}
