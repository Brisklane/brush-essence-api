using System.Collections.Concurrent;
using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Domain.Common;
using BrushEssence.Infrastructure.Persistence.Repositories;

namespace BrushEssence.Infrastructure.Persistence;

/// <summary>
/// EF Core unit of work. Caches one repository instance per aggregate type for
/// the lifetime of the (scoped) DbContext and commits via SaveChanges.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<T> Repository<T>()
        where T : BaseEntity
        => (IGenericRepository<T>)_repositories.GetOrAdd(
            typeof(T),
            _ => new GenericRepository<T>(_context));

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
