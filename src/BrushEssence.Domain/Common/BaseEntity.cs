namespace BrushEssence.Domain.Common;

/// <summary>
/// Base type for all domain entities, providing a strongly-typed primary key.
/// </summary>
/// <typeparam name="TId">The CLR type of the primary key.</typeparam>
public abstract class BaseEntity<TId>
    where TId : struct
{
    public TId Id { get; set; }
}

/// <summary>
/// Convenience base entity that uses a <see cref="Guid"/> primary key.
/// Most aggregate roots in the catalogue inherit from this.
/// </summary>
public abstract class BaseEntity : BaseEntity<Guid>
{
}
