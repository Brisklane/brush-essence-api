namespace BrushEssence.Infrastructure.Email;

/// <summary>App-level settings bound from the "App" configuration section.</summary>
public sealed class AppOptions
{
    public const string SectionName = "App";

    /// <summary>Public base URL of the Next.js web app, used to build email links.</summary>
    public string WebBaseUrl { get; set; } = "http://localhost:3000";
}
