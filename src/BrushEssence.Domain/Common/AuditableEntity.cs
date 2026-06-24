namespace BrushEssence.Domain.Common;

/// <summary>
/// Base entity that tracks creation and last-modification timestamps.
/// Timestamps are populated automatically when changes are saved, so domain
/// and application code never set them manually.
/// </summary>
public abstract class AuditableEntity : BaseEntity, IAuditableEntity
{
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
