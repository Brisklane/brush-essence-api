using BrushEssence.Application.Common.Models;
using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Reviews;

/// <summary>Public read model for a review shown on a painting's page.</summary>
public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid PaintingId { get; set; }
    public string AuthorName { get; set; } = "Anonymous";
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public ReviewStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>Aggregate rating summary for a painting (drives the star breakdown UI).</summary>
public class ReviewSummaryDto
{
    public decimal Average { get; set; }
    public int Count { get; set; }

    /// <summary>Count of approved reviews per star (keys "1".."5").</summary>
    public Dictionary<int, int> Distribution { get; set; } = [];
}

/// <summary>Admin moderation list row (includes painting and customer context).</summary>
public class AdminReviewListItemDto
{
    public Guid Id { get; set; }
    public Guid PaintingId { get; set; }
    public string PaintingTitle { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public ReviewStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>Search/filter/paging parameters for the admin review list.</summary>
public sealed class AdminReviewQuery : PagedQuery
{
    public string? Status { get; set; }

    public ReviewStatus? StatusFilter =>
        Enum.TryParse<ReviewStatus>(Status, ignoreCase: true, out var parsed) ? parsed : null;
}
