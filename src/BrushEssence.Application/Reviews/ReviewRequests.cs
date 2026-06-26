using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Reviews;

/// <summary>Submits a review for a painting (one per customer per painting).</summary>
public class CreateReviewRequest
{
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
}

/// <summary>Admin edit of a review's content.</summary>
public class AdminUpdateReviewRequest
{
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
}

/// <summary>Admin moderation action: approve / reject / re-queue a review.</summary>
public class UpdateReviewStatusRequest
{
    public ReviewStatus Status { get; set; }
}
