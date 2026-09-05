using System.Threading.RateLimiting;
using FinSight.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;

namespace FinSight.Api.Extensions;

/// <summary>
/// Provides extension methods for configuring FinSight API services
/// in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers API controllers, problem details, global exception handling,
    /// authorization policies, and rate limiting rules.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to register services into.
    /// </param>
    /// <returns>
    /// The updated <see cref="IServiceCollection"/> instance for method chaining.
    /// </returns>
    public static IServiceCollection AddFinSightApi(
        this IServiceCollection services)
    {
        AddControllers(
            services);

        AddExceptionHandling(
            services);

        AddAuthorization(
            services);

        AddRateLimiting(
            services);

        return services;
    }

    private static void AddControllers(
        IServiceCollection services)
    {
        services.AddControllers();

        services.Configure<FormOptions>(
            options =>
            {
                options.MultipartBodyLengthLimit =
                    10 * 1024 * 1024;
            });
    }

    private static void AddExceptionHandling(
        IServiceCollection services)
    {
        services.AddProblemDetails();

        services.AddExceptionHandler<
            Middleware.GlobalExceptionHandler>();
    }

    private static void AddAuthorization(
        IServiceCollection services)
    {
        services.AddAuthorization(
            options =>
            {
                options.FallbackPolicy =
                    new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();

                options.AddPolicy(
                    AuthorizationPolicies.Authenticated,
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();
                    });

                options.AddPolicy(
                    AuthorizationPolicies.Administrator,
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();

                        policy.RequireRole(
                            "Admin");
                    });
            });
    }

    private static void AddRateLimiting(
        IServiceCollection services)
    {
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

                options.AddFixedWindowLimiter(
                    "auth",
                    limiterOptions =>
                    {
                        limiterOptions.PermitLimit = 10;

                        limiterOptions.Window =
                            TimeSpan.FromMinutes(1);

                        limiterOptions.QueueProcessingOrder =
                            QueueProcessingOrder.OldestFirst;

                        limiterOptions.QueueLimit = 0;
                    });
            });
    }
}
