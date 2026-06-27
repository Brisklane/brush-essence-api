using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Common.Interfaces;

public interface IMediumRepository
{
    Task<IReadOnlyList<Medium>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Medium?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task AddAsync(Medium medium, CancellationToken cancellationToken = default);

    void Remove(Medium medium);
}
