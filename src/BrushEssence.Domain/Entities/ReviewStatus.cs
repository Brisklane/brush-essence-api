namespace BrushEssence.Domain.Entities;

/// <summary>
/// Moderation state of a review. Stored as a string (see configuration). Only
/// <see cref="Approved"/> reviews are shown publicly and counted toward a
/// painting's average rating.
/// </summary>
public enum ReviewStatus
{
    /// <summary>Awaiting moderation. The default for new submissions.</summary>
    Pending = 0,

    /// <summary>Approved and visible on the storefront.</summary>
    Approved = 1,

    /// <summary>Rejected by a moderator; hidden from the storefront.</summary>
    Rejected = 2,
}
