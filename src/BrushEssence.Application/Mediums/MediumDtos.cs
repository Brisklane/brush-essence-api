namespace BrushEssence.Application.Mediums;

public sealed class MediumDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed class CreateMediumRequest
{
    public string Name { get; init; } = string.Empty;
}

public sealed class UpdateMediumRequest
{
    public string Name { get; init; } = string.Empty;
}
