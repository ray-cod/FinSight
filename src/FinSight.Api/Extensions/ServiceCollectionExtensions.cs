using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace FinSight.Api.Extensions;

/// <summary>
/// Extension methods for setting up FinSight API infrastructure services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers API controllers, problem details, global exception handling, and rate limiting rules.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to register services into.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance for method chaining.</returns>
    public static IServiceCollection AddFinSightApi(
        this IServiceCollection services)
    {
        services.AddControllers();

        services.AddProblemDetails();

        services.AddExceptionHandler<
            Middleware.GlobalExceptionHandler>();

        services.AddRateLimiter(
            options =>
            {
                options.RejectionStatusCode =
                    StatusCodes.Status429TooManyRequests;

                options.AddFixedWindowLimiter(
                    "api",
                    limiterOptions =>
                    {
                        limiterOptions.PermitLimit = 120;

                        limiterOptions.Window =
                            TimeSpan.FromMinutes(1);

                        limiterOptions.QueueProcessingOrder =
                            QueueProcessingOrder.OldestFirst;

                        limiterOptions.QueueLimit = 0;
                    });
            });

        return services;
    }
}
