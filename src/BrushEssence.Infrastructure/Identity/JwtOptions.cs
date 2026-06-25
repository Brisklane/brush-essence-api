namespace BrushEssence.Infrastructure.Identity;

/// <summary>
/// Strongly-typed JWT settings bound from the "Jwt" configuration section. Used
/// both for issuing tokens (Infrastructure) and validating them (API).
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;

    public int RefreshTokenDays { get; set; } = 7;
}
