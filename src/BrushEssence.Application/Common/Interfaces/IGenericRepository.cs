using System.Linq.Expressions;
using BrushEssence.Domain.Common;

namespace BrushEssence.Application.Common.Interfaces;

/// <summary>
/// Generic read/write abstraction over a single aggregate type. Implemented in
/// the Infrastructure layer on top of EF Core, keeping persistence concerns out
/// of the Application layer.
/// </summary>
public interface IGenericRepository<T>
    where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    void Update(T entity);

    void Remove(T entity);

    /// <summary>
    /// Escape hatch for composing complex, query-specific projections.
    /// Prefer the explicit methods above for common cases.
    /// </summary>
    IQueryable<T> Query();
}
