using System.Reflection;
using BrushEssence.Application.Admin;
using BrushEssence.Application.Auth;
using BrushEssence.Application.Carts;
using BrushEssence.Application.Categories;
using BrushEssence.Application.CustomRequests;
using BrushEssence.Application.Orders;
using BrushEssence.Application.Paintings;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BrushEssence.Application;

/// <summary>
/// Registration entry point for the Application layer. Keeps layer wiring next
/// to the layer it configures so <c>Program.cs</c> stays thin.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register every FluentValidation validator in this assembly.
        // (Object mapping is handled by explicit extension methods, no library.)
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        // Application use-case services.
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPaintingService, PaintingService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICustomRequestService, CustomRequestService>();

        // Admin dashboard services.
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
