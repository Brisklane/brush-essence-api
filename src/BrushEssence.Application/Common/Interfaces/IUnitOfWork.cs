using BrushEssence.Domain.Common;

namespace BrushEssence.Application.Common.Interfaces;

/// <summary>
/// Coordinates work across repositories within a single transaction/scope and
/// commits all changes together.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Resolves a repository for the requested aggregate type.</summary>
    IGenericRepository<T> Repository<T>()
        where T : BaseEntity;

    /// <summary>Persists all tracked changes to the underlying store.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
