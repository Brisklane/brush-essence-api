using BrushEssence.Domain.Common;

namespace BrushEssence.Domain.Entities;

/// <summary>
/// A customer's star rating and written review of a painting. New reviews start
/// <see cref="ReviewStatus.Pending"/> and become visible once a moderator
/// approves them. A customer may review a given painting at most once.
/// </summary>
public class Review : AuditableEntity
{
    public Guid PaintingId { get; set; }

    public Guid UserId { get; set; }

    /// <summary>Email captured at submission (snapshot of the user's email).</summary>
    public required string CustomerEmail { get; set; }

    /// <summary>Star rating, 1–5 inclusive.</summary>
    public int Rating { get; set; }

    /// <summary>Optional short headline.</summary>
    public string? Title { get; set; }

    /// <summary>Optional free-text review body.</summary>
    public string? Comment { get; set; }

    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;

    // Navigation properties
    public Painting? Painting { get; set; }
    public User? User { get; set; }
}
