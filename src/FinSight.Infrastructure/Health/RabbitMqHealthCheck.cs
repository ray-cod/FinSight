using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FinSight.Infrastructure.Health;

/// <summary>
/// Provides a health check implementation for verifying connectivity to RabbitMQ.
/// </summary>
/// <param name="connectionProvider">The provider used to obtain an active RabbitMQ connection.</param>
public sealed class RabbitMqHealthCheck(
    Messaging.RabbitMq.IRabbitMqConnectionProvider connectionProvider)
    : IHealthCheck
{
    /// <summary>
    /// Asynchronously runs the health check to verify whether the RabbitMQ connection is open and operational.
    /// </summary>
    /// <param name="context">A context object associated with the current health check execution.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the health check execution.</param>
    /// <returns>A <see cref="HealthCheckResult"/> indicating whether RabbitMQ is healthy or unhealthy.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connection =
                await connectionProvider.GetConnectionAsync(
                    cancellationToken);

            return connection.IsOpen
                ? HealthCheckResult.Healthy(
                    "RabbitMQ is reachable.")
                : HealthCheckResult.Unhealthy(
                    "RabbitMQ connection is closed.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "RabbitMQ health check failed.",
                exception);
        }
    }
}
