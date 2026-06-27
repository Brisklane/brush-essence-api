namespace BrushEssence.Application.Mediums;

/// <summary>Admin CRUD for the reusable list of art mediums.</summary>
public interface IMediumService
{
    Task<IReadOnlyList<MediumDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<MediumDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<MediumDto> CreateAsync(CreateMediumRequest request, CancellationToken cancellationToken = default);

    Task<MediumDto> UpdateAsync(Guid id, UpdateMediumRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
