using Microsoft.Extensions.DependencyInjection;

namespace FinSight.Application;

/// <summary>
/// Provides extension methods for registering Application layer services in the dependency injection container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Application layer components into the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance for method chaining.</returns>
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        return services;
    }
}
