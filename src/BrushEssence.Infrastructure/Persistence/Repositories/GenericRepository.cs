using System.Linq.Expressions;
using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace BrushEssence.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IGenericRepository{T}"/>.</summary>
public class GenericRepository<T> : IGenericRepository<T>
    where T : BaseEntity
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);

    public async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(predicate, cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await _dbSet.AddAsync(entity, cancellationToken);

    public void Update(T entity) => _dbSet.Update(entity);

    public void Remove(T entity) => _dbSet.Remove(entity);

    public IQueryable<T> Query() => _dbSet.AsQueryable();
}
