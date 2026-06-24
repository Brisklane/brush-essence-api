using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace BrushEssence.IntegrationTests;

/// <summary>
/// Smoke test that boots the full API host in-memory. Uses the liveness probe
/// (which runs no checks) so it passes without a live PostgreSQL instance.
/// </summary>
public class HealthCheckTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthCheckTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(Environments.Development);

            // Provide a dummy connection string so the host composes without a DB.
            builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] =
                        "Host=localhost;Database=test;Username=test;Password=test",
                }));
        });
    }

    [Fact]
    public async Task Liveness_endpoint_returns_healthy()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
