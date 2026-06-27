using BrushEssence.Application.Common.Exceptions;
using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Mediums;

public sealed class MediumService(
    IMediumRepository mediums,
    IUnitOfWork unitOfWork) : IMediumService
{
    public async Task<IReadOnlyList<MediumDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => (await mediums.GetAllAsync(cancellationToken)).ToDtoList();

    public async Task<MediumDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var medium = await mediums.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Medium not found.");

        return medium.ToDto();
    }

    public async Task<MediumDto> CreateAsync(
        CreateMediumRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();

        if (await mediums.NameExistsAsync(name, null, cancellationToken))
        {
            throw new ConflictException("A medium with this name already exists.");
        }

        var medium = new Medium { Name = name };
        await mediums.AddAsync(medium, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return medium.ToDto();
    }

    public async Task<MediumDto> UpdateAsync(
        Guid id,
        UpdateMediumRequest request,
        CancellationToken cancellationToken = default)
    {
        var medium = await mediums.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Medium not found.");

        var name = request.Name.Trim();

        if (await mediums.NameExistsAsync(name, id, cancellationToken))
        {
            throw new ConflictException("A medium with this name already exists.");
        }

        medium.Name = name;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return medium.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var medium = await mediums.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Medium not found.");

        // Paintings keep existing but lose their medium (FK is ON DELETE SET NULL).
        mediums.Remove(medium);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
