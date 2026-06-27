using System.Text.Json;
using System.Threading.RateLimiting;
using BrushEssence.Api.Options;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BrushEssence.Api.Extensions;

/// <summary>Names of output-cache policies/tags used across the API.</summary>
public static class CachePolicies
{
    /// <summary>Policy for public, low-volatility catalogue reads.</summary>
    public const string Catalog = "catalog";

    /// <summary>Tag used to evict all cached catalogue responses on a write.</summary>
    public const string CatalogTag = "catalog";
}

/// <summary>Names of rate-limiting policies.</summary>
public static class RateLimitPolicies
{
    /// <summary>Stricter limiter for sensitive auth endpoints.</summary>
    public const string Auth = "auth";
}

/// <summary>Wiring for production-readiness concerns: caching, rate limiting, health.</summary>
public static class ProductionExtensions
{
    public static IServiceCollection AddCatalogOutputCache(this IServiceCollection services)
    {
        services.AddOutputCache(options =>
        {
            options.AddPolicy(CachePolicies.Catalog, builder => builder
                .Expire(TimeSpan.FromSeconds(60))
                .Tag(CachePolicies.CatalogTag)
                .SetVaryByQuery("*")
                // Only cache anonymous storefront reads. Authenticated callers
                // (admins) see draft/unpublished data and must bypass the cache.
                .With(context => !context.HttpContext.Request.Headers.ContainsKey("Authorization")));
        });

        return services;
    }

    public static IServiceCollection AddApiRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rules = configuration.GetSection(RateLimitOptions.SectionName).Get<RateLimitOptions>()
            ?? new RateLimitOptions();

        services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Global limiter: a sliding window per client IP (1 segment ≈ fixed).
            limiter.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    ClientKey(httpContext),
                    _ => SlidingWindow(rules.Global)));

            // Tighter, smoother limiter for auth endpoints (brute-force / abuse protection).
            limiter.AddPolicy(RateLimitPolicies.Auth, httpContext =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    ClientKey(httpContext),
                    _ => SlidingWindow(rules.Auth)));

            limiter.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString();
                }

                context.HttpContext.Response.ContentType = "application/problem+json";
                await context.HttpContext.Response.WriteAsync(
                    "{\"title\":\"Too many requests.\",\"status\":429," +
                    "\"detail\":\"Rate limit exceeded. Please retry later.\"}",
                    cancellationToken);
            };
        });

        return services;
    }

    /// <summary>Writes a health report as structured JSON instead of the default plaintext.</summary>
    public static Task WriteHealthResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                durationMs = entry.Value.Duration.TotalMilliseconds,
            }),
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }

    private static SlidingWindowRateLimiterOptions SlidingWindow(RateLimitRule rule) => new()
    {
        PermitLimit = rule.PermitLimit,
        Window = TimeSpan.FromSeconds(rule.WindowSeconds),
        SegmentsPerWindow = Math.Max(1, rule.SegmentsPerWindow),
    };

    private static string ClientKey(HttpContext httpContext)
        => httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
