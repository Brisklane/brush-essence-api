using BrushEssence.Application.Common.Models;

namespace BrushEssence.Application.Paintings;

public interface IPaintingService
{
    Task<PagedResult<PaintingDto>> GetPagedAsync(PaintingQuery query, CancellationToken cancellationToken = default);

    Task<PaintingDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PaintingDto> CreateAsync(CreatePaintingRequest request, CancellationToken cancellationToken = default);

    Task<PaintingDto> UpdateAsync(Guid id, UpdatePaintingRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
