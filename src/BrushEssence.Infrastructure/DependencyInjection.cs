using BrushEssence.Application.Common.Interfaces;
using BrushEssence.Infrastructure.Identity;
using BrushEssence.Infrastructure.Logging;
using BrushEssence.Infrastructure.Persistence;
using BrushEssence.Infrastructure.Persistence.Interceptors;
using BrushEssence.Infrastructure.Persistence.Repositories;
using BrushEssence.Infrastructure.Storage;
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
        services.AddScoped<IAuditLogger, AuditLogger>();

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

        // Auth-specific repositories.
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

        // Catalogue repositories.
        services.AddScoped<IPaintingRepository, PaintingRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IMediumRepository, MediumRepository>();
        services.AddScoped<IPromotionRepository, PromotionRepository>();

        // Cart + order + custom-request repositories.
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICustomRequestRepository, CustomRequestRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();

        // Cross-aggregate reporting for the admin dashboard.
        services.AddScoped<IReportingRepository, ReportingRepository>();

        // Image storage in PostgreSQL (bytea). Scoped because it uses the
        // request-scoped DbContext. Swap for LocalFileStorageService or a cloud
        // implementation without touching callers. FileStorageOptions still
        // drives upload validation (size/type limits).
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
        services.AddScoped<IFileStorageService, DatabaseFileStorageService>();

        // Identity services (JWT issuing + BCrypt hashing) and their options.
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
