namespace BrushEssence.Application.Common.Interfaces;

/// <summary>
/// The guest's opaque cart token for the current request, sourced from the
/// <c>X-Cart-Token</c> header. Implemented in the API layer over
/// <see cref="Microsoft.AspNetCore.Http.IHttpContextAccessor"/>. Pairs with
/// <see cref="ICurrentUser"/>: a signed-in user is identified by their id, a
/// guest by this token.
/// </summary>
public interface ICartSession
{
    /// <summary>The guest cart token, or null when the header is absent/invalid.</summary>
    Guid? AnonymousCartId { get; }
}
