using BrushEssence.Api.Extensions;
using BrushEssence.Api.Identity;
using BrushEssence.Api.Middleware;
using BrushEssence.Api.Options;
using BrushEssence.Application;
using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Infrastructure;
using BrushEssence.Infrastructure.Persistence;
using BrushEssence.Infrastructure.Storage;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Serilog;

// Bootstrap logger captures failures that occur before the host is fully built.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting BrushEssence API host");

    var builder = WebApplication.CreateBuilder(args);

    // Structured logging via Serilog, read from the "Serilog" config section.
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // Strongly-typed options binding (options pattern).
    builder.Services.Configure<CorsOptions>(
        builder.Configuration.GetSection(CorsOptions.SectionName));

    // Application + Infrastructure layers (Clean Architecture composition root).
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Physical directory uploaded images are written to and served from.
    var uploadsRoot = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads");
    Directory.CreateDirectory(uploadsRoot);
    builder.Services.PostConfigure<FileStorageOptions>(options => options.PhysicalRootPath = uploadsRoot);

    // Authentication & authorization (JWT bearer + policies).
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUser, CurrentUser>();
    builder.Services.AddScoped<ICartSession, CartSession>();
    builder.Services.AddJwtAuthentication(builder.Configuration);

    // Web/API services. Serialize enums as their readable names (e.g. order
    // status "Placed") so the JSON contract is self-describing and stable.
    builder.Services.AddControllers()
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));

    // Output caching for low-volatility public catalogue reads, and rate
    // limiting to protect the API from abuse.
    builder.Services.AddCatalogOutputCache();
    builder.Services.AddApiRateLimiting(builder.Configuration);
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerWithJwt();
    builder.Services.AddOpenApi();

    // RFC 7807 ProblemDetails + centralized exception handling.
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    // Health checks: API liveness + PostgreSQL readiness.
    builder.Services.AddHealthChecks()
        // Liveness: the process is up and serving.
        .AddCheck("self", () => HealthCheckResult.Healthy("API is running."), tags: ["live"])
        // Readiness: critical dependencies (the database) are reachable.
        .AddNpgSql(
            builder.Configuration.GetConnectionString("DefaultConnection")!,
            name: "postgresql",
            tags: ["ready"]);

    // CORS for the Next.js frontend.
    var corsOptions = builder.Configuration
        .GetSection(CorsOptions.SectionName)
        .Get<CorsOptions>() ?? new CorsOptions();

    builder.Services.AddCors(options =>
        options.AddDefaultPolicy(policy => policy
            .WithOrigins(corsOptions.AllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()));

    var app = builder.Build();

    // Bring the database schema up to date on startup so a fresh/empty database
    // (e.g. after running scripts/clear-db.sql) works with no manual step —
    // migrations recreate the tables and re-seed the Admin/Customer roles.
    // Wrapped so a transient DB outage (or a test host with no real database)
    // doesn't crash startup; the readiness health check reports DB status.
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();
            Log.Information("Database migrations applied (or already up to date).");
        }
        catch (Exception migrationException)
        {
            Log.Warning(migrationException, "Could not apply database migrations on startup.");
        }
    }

    // When running behind a reverse proxy / load balancer, honour X-Forwarded-*
    // so the real client IP (used for rate limiting) and scheme are correct.
    // OFF by default: trusting these headers without a proxy would let clients
    // spoof their IP and bypass IP-based rate limiting. Enable only behind a
    // trusted proxy (and ideally pin KnownProxies/KnownNetworks there).
    if (app.Configuration.GetValue<bool>("ForwardedHeaders:Enabled"))
    {
        var forwardedHeaders = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        };
        // The proxy address is usually not known at build time in cloud setups;
        // trust the immediate upstream. Lock down with KnownProxies in production.
        forwardedHeaders.KnownNetworks.Clear();
        forwardedHeaders.KnownProxies.Clear();
        app.UseForwardedHeaders(forwardedHeaders);
    }

    app.UseSerilogRequestLogging();
    app.UseSecurityHeaders();
    app.UseExceptionHandler();

    // HSTS in production only (dev runs over http and would poison the browser).
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    // Serve uploaded images at /uploads/* from the physical uploads directory.
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsRoot),
        RequestPath = "/uploads",
    });

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // CORS must run BEFORE the rate limiter and output cache so that:
    //  - preflight OPTIONS requests are answered here and never count against the
    //    rate limit, and
    //  - short-circuited responses (a 429 from the limiter, or a cache hit) still
    //    carry the Access-Control-Allow-Origin header. Otherwise the browser
    //    reports them as opaque "CORS error"s instead of the real status.
    app.UseCors();
    app.UseRateLimiter();
    app.UseOutputCache();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Health endpoints (structured JSON responses).
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = ProductionExtensions.WriteHealthResponse,
    });
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = ProductionExtensions.WriteHealthResponse,
    });
    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("live"),
        ResponseWriter = ProductionExtensions.WriteHealthResponse,
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "BrushEssence API host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Exposed so the integration test project can use WebApplicationFactory<Program>.
public partial class Program;
