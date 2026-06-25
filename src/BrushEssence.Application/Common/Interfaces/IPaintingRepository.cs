using BrushEssence.Application.Paintings;
using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Common.Interfaces;

public interface IPaintingRepository
{
    /// <summary>Returns a filtered, paged set of paintings (with their category).</summary>
    Task<(IReadOnlyList<Painting> Items, int TotalCount)> GetPagedAsync(
        PaintingQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>Loads a tracked painting (with its category) for read or update.</summary>
    Task<Painting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Painting painting, CancellationToken cancellationToken = default);

    void Remove(Painting painting);
}
