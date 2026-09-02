using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FinSight.Application;

/// <summary>
/// Provides dependency registration for the application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers FinSight application services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly);

        return services;
    }
}
