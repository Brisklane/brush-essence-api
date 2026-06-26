using BrushEssence.Application.Common.Exceptions;
using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Application.Common.Models;
using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Reviews;

public sealed class ReviewService(
    IReviewRepository reviews,
    IPaintingRepository paintings,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IReviewService
{
    public async Task<ReviewDto> CreateAsync(
        Guid paintingId,
        CreateReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireUserId();

        var painting = await paintings.GetByIdAsync(paintingId, cancellationToken);
        if (painting is null || !painting.IsPublished)
        {
            throw new NotFoundException("Painting not found.");
        }

        if (await reviews.ExistsForUserAsync(paintingId, userId, cancellationToken))
        {
            throw new ConflictException("You have already reviewed this painting.");
        }

        var review = new Review
        {
            PaintingId = paintingId,
            UserId = userId,
            CustomerEmail = currentUser.Email ?? string.Empty,
            Rating = request.Rating,
            Title = Clean(request.Title),
            Comment = Clean(request.Comment),
            Status = ReviewStatus.Pending,
        };

        await reviews.AddAsync(review, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return review.ToDto();
    }

    public async Task<PagedResult<ReviewDto>> GetForPaintingAsync(
        Guid paintingId,
        PagedQuery query,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await reviews.GetApprovedForPaintingAsync(paintingId, query, cancellationToken);

        return new PagedResult<ReviewDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<ReviewSummaryDto> GetSummaryAsync(Guid paintingId, CancellationToken cancellationToken = default)
    {
        var (count, average) = await reviews.GetApprovedAggregateAsync(paintingId, cancellationToken);
        var distribution = await reviews.GetApprovedDistributionAsync(paintingId, cancellationToken);

        return new ReviewSummaryDto
        {
            Average = decimal.Round(average, 2, MidpointRounding.AwayFromZero),
            Count = count,
            // Always present stars 1..5, defaulting missing ones to 0.
            Distribution = Enumerable.Range(1, 5)
                .ToDictionary(star => star, star => distribution.TryGetValue(star, out var c) ? c : 0),
        };
    }

    public async Task<ReviewDto?> GetMyReviewAsync(Guid paintingId, CancellationToken cancellationToken = default)
        => await reviews.GetUserReviewAsync(paintingId, RequireUserId(), cancellationToken);

    // ----- Admin moderation -----

    public async Task<PagedResult<AdminReviewListItemDto>> GetAllAsync(
        AdminReviewQuery query,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await reviews.GetPagedForAdminAsync(query, cancellationToken);

        return new PagedResult<AdminReviewListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<AdminReviewListItemDto> UpdateStatusAsync(
        Guid id,
        UpdateReviewStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var review = await LoadAsync(id, cancellationToken);

        review.Status = request.Status;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Approving/rejecting changes the approved set, so refresh the cache.
        await RecalculateRatingAsync(review.PaintingId, cancellationToken);

        return ToAdminDto(review);
    }

    public async Task<AdminReviewListItemDto> UpdateAsync(
        Guid id,
        AdminUpdateReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var review = await LoadAsync(id, cancellationToken);

        review.Rating = request.Rating;
        review.Title = Clean(request.Title);
        review.Comment = Clean(request.Comment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // A rating edit only affects the cache if this review counts (is approved).
        if (review.Status == ReviewStatus.Approved)
        {
            await RecalculateRatingAsync(review.PaintingId, cancellationToken);
        }

        return ToAdminDto(review);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var review = await LoadAsync(id, cancellationToken);
        var paintingId = review.PaintingId;
        var wasApproved = review.Status == ReviewStatus.Approved;

        reviews.Remove(review);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (wasApproved)
        {
            await RecalculateRatingAsync(paintingId, cancellationToken);
        }
    }

    // ----- Helpers -----

    private Guid RequireUserId()
        => currentUser.UserId
            ?? throw new AuthenticationException("You must be signed in to review a painting.");

    private async Task<Review> LoadAsync(Guid id, CancellationToken cancellationToken)
        => await reviews.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Review not found.");

    /// <summary>Recomputes and caches a painting's average rating from approved reviews.</summary>
    private async Task RecalculateRatingAsync(Guid paintingId, CancellationToken cancellationToken)
    {
        var painting = await paintings.GetByIdAsync(paintingId, cancellationToken);
        if (painting is null)
        {
            return;
        }

        var (count, average) = await reviews.GetApprovedAggregateAsync(paintingId, cancellationToken);
        painting.RatingCount = count;
        painting.AverageRating = decimal.Round(average, 2, MidpointRounding.AwayFromZero);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static AdminReviewListItemDto ToAdminDto(Review review) => new()
    {
        Id = review.Id,
        PaintingId = review.PaintingId,
        PaintingTitle = review.Painting?.Title ?? string.Empty,
        CustomerEmail = review.CustomerEmail,
        Rating = review.Rating,
        Title = review.Title,
        Comment = review.Comment,
        Status = review.Status,
        CreatedAt = review.CreatedAt,
    };

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
