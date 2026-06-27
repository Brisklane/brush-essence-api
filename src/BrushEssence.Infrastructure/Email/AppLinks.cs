using BrushEssence.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace BrushEssence.Infrastructure.Email;

/// <summary>Builds web-app links from the configured <see cref="AppOptions.WebBaseUrl"/>.</summary>
public sealed class AppLinks(IOptions<AppOptions> options) : IAppLinks
{
    private readonly string _baseUrl = options.Value.WebBaseUrl.TrimEnd('/');

    public string VerifyEmailUrl(string token)
        => $"{_baseUrl}/verify-email?token={Uri.EscapeDataString(token)}";

    public string ResetPasswordUrl(string token)
        => $"{_baseUrl}/reset-password?token={Uri.EscapeDataString(token)}";
}
