using BrushEssence.Application.Common.Models;

namespace BrushEssence.Application.Reviews;

/// <summary>
/// Review use cases. Customers submit and read reviews of paintings; admins
/// moderate them. Approved reviews drive a painting's cached average rating.
/// </summary>
public interface IReviewService
{
    /// <summary>
    /// Submits the current user's review for a painting. Starts Pending; one
    /// review per customer per painting.
    /// </summary>
    Task<ReviewDto> CreateAsync(Guid paintingId, CreateReviewRequest request, CancellationToken cancellationToken = default);

    /// <summary>Public, paged list of a painting's approved reviews (newest first).</summary>
    Task<PagedResult<ReviewDto>> GetForPaintingAsync(Guid paintingId, PagedQuery query, CancellationToken cancellationToken = default);

    /// <summary>Aggregate rating summary (average, count, distribution) for a painting.</summary>
    Task<ReviewSummaryDto> GetSummaryAsync(Guid paintingId, CancellationToken cancellationToken = default);

    /// <summary>The current user's own review for a painting (any status), or null.</summary>
    Task<ReviewDto?> GetMyReviewAsync(Guid paintingId, CancellationToken cancellationToken = default);

    // ----- Admin moderation -----

    Task<PagedResult<AdminReviewListItemDto>> GetAllAsync(AdminReviewQuery query, CancellationToken cancellationToken = default);

    /// <summary>Approve/reject a review, refreshing the painting's cached rating.</summary>
    Task<AdminReviewListItemDto> UpdateStatusAsync(Guid id, UpdateReviewStatusRequest request, CancellationToken cancellationToken = default);

    /// <summary>Edit a review's content (admin), refreshing the cached rating if approved.</summary>
    Task<AdminReviewListItemDto> UpdateAsync(Guid id, AdminUpdateReviewRequest request, CancellationToken cancellationToken = default);

    /// <summary>Remove a review, refreshing the painting's cached rating.</summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
