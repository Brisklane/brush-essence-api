namespace BrushEssence.Api.Options;

/// <summary>
/// Strongly-typed rate-limiting configuration (bound from the "RateLimiting"
/// section). Lets the limits be tuned per environment without a rebuild.
/// </summary>
public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimiting";

    /// <summary>Applies to every request, partitioned by client IP.</summary>
    public RateLimitRule Global { get; set; } = new() { PermitLimit = 100, WindowSeconds = 60, SegmentsPerWindow = 1 };

    /// <summary>Stricter limit for auth endpoints (login/register/etc.).</summary>
    public RateLimitRule Auth { get; set; } = new() { PermitLimit = 10, WindowSeconds = 60, SegmentsPerWindow = 6 };
}

/// <summary>A single rate-limit rule.</summary>
public sealed class RateLimitRule
{
    /// <summary>Max requests allowed per window, per client.</summary>
    public int PermitLimit { get; set; } = 100;

    /// <summary>Length of the window in seconds.</summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>
    /// Sliding-window segments. 1 behaves like a fixed window; higher values
    /// give a smoother sliding window (less burst at window boundaries).
    /// </summary>
    public int SegmentsPerWindow { get; set; } = 1;
}
