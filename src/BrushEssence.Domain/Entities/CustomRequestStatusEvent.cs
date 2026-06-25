using BrushEssence.Domain.Common;

namespace BrushEssence.Domain.Entities;

/// <summary>
/// A single entry in a custom request's status timeline, recorded on every
/// transition. Powers the "status updates" shown on the request details page
/// (<see cref="AuditableEntity.CreatedAt"/> is when it happened).
/// </summary>
public class CustomRequestStatusEvent : AuditableEntity
{
    public Guid CustomRequestId { get; set; }

    public CustomRequestStatus Status { get; set; }

    /// <summary>Optional message from the artist (e.g. a quote or timeline).</summary>
    public string? Note { get; set; }

    // Navigation property
    public CustomRequest? CustomRequest { get; set; }
}
