using BrushEssence.Domain.Entities;

namespace BrushEssence.Application.Mediums;

/// <summary>Explicit, dependency-free mapping between <see cref="Medium"/> and its DTO.</summary>
public static class MediumMappings
{
    public static MediumDto ToDto(this Medium medium) => new()
    {
        Id = medium.Id,
        Name = medium.Name,
        CreatedAt = medium.CreatedAt,
    };

    public static IReadOnlyList<MediumDto> ToDtoList(this IEnumerable<Medium> mediums)
        => mediums.Select(ToDto).ToList();
}
