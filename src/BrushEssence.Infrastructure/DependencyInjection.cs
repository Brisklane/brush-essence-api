using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Infrastructure.Persistence;
using BrushEssence.Infrastructure.Persistence.Interceptors;
using BrushEssence.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrushEssence.Infrastructure;

/// <summary>Registration entry point for the Infrastructure layer.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not configured.");

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsAssembly(
                    typeof(ApplicationDbContext).Assembly.FullName));

            options.AddInterceptors(
                serviceProvider.GetRequiredService<AuditableEntityInterceptor>());
        });

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
