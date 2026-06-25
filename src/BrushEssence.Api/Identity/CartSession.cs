using BrushEssence.Application.Common.Interfaces;

namespace BrushEssence.Api.Identity;

/// <summary>
/// Reads the guest cart token from the <c>X-Cart-Token</c> request header.
/// Scoped, backed by <see cref="IHttpContextAccessor"/>. A missing or malformed
/// header simply yields <c>null</c> (the caller is treated as having no cart).
/// </summary>
public sealed class CartSession(IHttpContextAccessor httpContextAccessor) : ICartSession
{
    /// <summary>Header the browser sends its opaque cart token on.</summary>
    public const string HeaderName = "X-Cart-Token";

    public Guid? AnonymousCartId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.Request.Headers[HeaderName].ToString();
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}
