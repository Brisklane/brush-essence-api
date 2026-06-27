using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrushEssence.Infrastructure.Persistence.Repositories;

public sealed class MediumRepository(ApplicationDbContext context) : IMediumRepository
{
    public async Task<IReadOnlyList<Medium>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Mediums
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);

    public Task<Medium?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Mediums.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Mediums.AnyAsync(m => m.Id == id, cancellationToken);

    public Task<bool> NameExistsAsync(
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
        => context.Mediums.AnyAsync(
            m => m.Name.ToLower() == name.ToLower() && (excludeId == null || m.Id != excludeId),
            cancellationToken);

    public async Task AddAsync(Medium medium, CancellationToken cancellationToken = default)
        => await context.Mediums.AddAsync(medium, cancellationToken);

    public void Remove(Medium medium) => context.Mediums.Remove(medium);
}
