namespace BrushEssence.Api.Options;

/// <summary>
/// Strongly-typed binding for the "Cors" configuration section. Example of the
/// options pattern used across the API for environment-specific settings.
/// </summary>
public class CorsOptions
{
    public const string SectionName = "Cors";

    /// <summary>Origins allowed to call the API (e.g. the Next.js frontend).</summary>
    public string[] AllowedOrigins { get; set; } = [];
}
