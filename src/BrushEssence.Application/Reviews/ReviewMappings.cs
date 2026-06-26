using System.Linq.Expressions;
using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Reviews;

/// <summary>Explicit, dependency-free mapping for reviews.</summary>
public static class ReviewMappings
{
    /// <summary>
    /// EF projection for public review lists. The author's display name falls
    /// back to "Anonymous" so we never leak the reviewer's email.
    /// </summary>
    public static readonly Expression<Func<Review, ReviewDto>> ToDtoProjection = review => new ReviewDto
    {
        Id = review.Id,
        PaintingId = review.PaintingId,
        AuthorName = review.User != null && review.User.FullName != null ? review.User.FullName : "Anonymous",
        Rating = review.Rating,
        Title = review.Title,
        Comment = review.Comment,
        Status = review.Status,
        CreatedAt = review.CreatedAt,
    };

    public static ReviewDto ToDto(this Review review) => new()
    {
        Id = review.Id,
        PaintingId = review.PaintingId,
        AuthorName = review.User?.FullName ?? "Anonymous",
        Rating = review.Rating,
        Title = review.Title,
        Comment = review.Comment,
        Status = review.Status,
        CreatedAt = review.CreatedAt,
    };
}
