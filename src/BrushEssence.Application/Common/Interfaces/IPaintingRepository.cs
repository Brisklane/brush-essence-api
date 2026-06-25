using BrushEssence.Application.Paintings;
using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Common.Interfaces;

public interface IPaintingRepository
{
    /// <summary>
    /// Returns a filtered, sorted, paged page of paintings projected straight to
    /// DTOs (no entity tracking) for fast catalogue reads.
    /// </summary>
    Task<(IReadOnlyList<PaintingDto> Items, int TotalCount)> GetPagedAsync(
        PaintingQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>Reads a single painting projected to a DTO (no tracking).</summary>
    Task<PaintingDto?> GetDtoByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Loads a tracked painting (with its category) for update or delete.</summary>
    Task<Painting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Painting painting, CancellationToken cancellationToken = default);

    void Remove(Painting painting);
}
