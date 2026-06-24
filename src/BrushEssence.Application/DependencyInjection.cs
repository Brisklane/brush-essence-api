using System.Reflection;
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

        return services;
    }
}
