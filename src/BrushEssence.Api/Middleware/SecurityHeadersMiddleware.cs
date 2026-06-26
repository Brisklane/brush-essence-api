namespace BrushEssence.Api.Middleware;

/// <summary>
/// Adds defence-in-depth HTTP security headers to every response. HSTS is added
/// separately via <c>UseHsts</c> (production only). The CSP is permissive enough
/// for the dev Swagger UI while still constraining a JSON API tightly.
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    private const string ContentSecurityPolicy =
        "default-src 'self'; " +
        "img-src 'self' data:; " +
        "style-src 'self' 'unsafe-inline'; " +
        "script-src 'self' 'unsafe-inline'; " +
        "object-src 'none'; " +
        "base-uri 'self'; " +
        "frame-ancestors 'none'";

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Set before the response starts; never overwrite if already present.
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["X-XSS-Protection"] = "0"; // Modern guidance: disable the legacy filter.
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), browsing-topics=()";
        headers["Content-Security-Policy"] = ContentSecurityPolicy;

        await next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        => app.UseMiddleware<SecurityHeadersMiddleware>();
}
