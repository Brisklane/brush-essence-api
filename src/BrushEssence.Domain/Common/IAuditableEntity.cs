namespace BrushEssence.Domain.Common;

/// <summary>
/// Marks an entity whose create/update timestamps are maintained automatically
/// by the persistence layer (see the auditing interceptor in Infrastructure).
/// </summary>
public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; set; }

    DateTimeOffset? UpdatedAt { get; set; }
}
