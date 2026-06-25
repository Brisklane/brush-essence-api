namespace BrushEssence.Application.Common.Interfaces;

/// <summary>
/// Ambient information about the authenticated caller, sourced from the current
/// request's claims. Implemented in the API layer over IHttpContextAccessor.
/// </summary>
public interface ICurrentUser
{
    Guid? UserId { get; }

    string? Email { get; }

    bool IsAuthenticated { get; }

    IReadOnlyList<string> Roles { get; }
}
