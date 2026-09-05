using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace FinSight.Infrastructure.Resilience;

/// <summary>
/// Provides production HTTP resilience policies.
/// </summary>
public static class ResilienceExtensions
{
    /// <summary>
    /// Adds a standard resilient HTTP client.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The named client.</param>
    /// <param name="baseAddress">
    /// The remote service base address.
    /// </param>
    public static void AddResilientHttpClient(
        this IServiceCollection services,
        string name,
        Uri baseAddress)
    {
        services
            .AddHttpClient(
                name,
                client =>
                {
                    client.BaseAddress =
                        baseAddress;

                    client.Timeout =
                        TimeSpan.FromSeconds(30);
                })
            .AddStandardResilienceHandler(
                options =>
                {
                    options.Retry.MaxRetryAttempts = 3;

                    options.Retry.Delay =
                        TimeSpan.FromSeconds(1);

                    options.Retry.UseJitter = true;

                    options.TotalRequestTimeout.Timeout =
                        TimeSpan.FromSeconds(30);

                    options.AttemptTimeout.Timeout =
                        TimeSpan.FromSeconds(10);

                    options.CircuitBreaker.SamplingDuration =
                        TimeSpan.FromSeconds(30);
                });
    }
}
