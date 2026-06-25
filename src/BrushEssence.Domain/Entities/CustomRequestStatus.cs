namespace BrushEssence.Domain.Entities;

/// <summary>
/// Lifecycle states a custom painting request moves through. Stored as a string
/// (see configuration) so values stay readable and stable across releases.
/// </summary>
public enum CustomRequestStatus
{
    /// <summary>Submitted by the customer, awaiting the artist's review.</summary>
    Submitted = 0,

    /// <summary>Reviewed by the artist; feasibility/quote discussed.</summary>
    Reviewed = 1,

    /// <summary>Accepted and actively being painted.</summary>
    InProgress = 2,

    /// <summary>Finished. Terminal (success).</summary>
    Completed = 3,

    /// <summary>Declined by the artist. Terminal.</summary>
    Declined = 4,
}
