using BrushEssence.Api.Middleware;
using BrushEssence.Api.Options;
using BrushEssence.Application;
using BrushEssence.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
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

    // Web/API services.
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddOpenApi();

    // RFC 7807 ProblemDetails + centralized exception handling.
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    // Health checks: API liveness + PostgreSQL readiness.
    builder.Services.AddHealthChecks()
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

    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseCors();
    app.UseAuthorization();

    app.MapControllers();

    // Health endpoints.
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
    });
    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false,
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
