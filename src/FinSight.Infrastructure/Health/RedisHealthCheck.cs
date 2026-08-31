using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace FinSight.Infrastructure.Health;

/// <summary>
/// Provides a health check implementation for verifying connectivity and latency to Redis.
/// </summary>
/// <param name="connection">The Redis connection multiplexer instance.</param>
public sealed class RedisHealthCheck(
    IConnectionMultiplexer connection)
    : IHealthCheck
{
    /// <summary>
    /// Asynchronously runs the health check by issuing a ping command to the configured Redis instance.
    /// </summary>
    /// <param name="context">A context object associated with the current health check execution.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the health check execution.</param>
    /// <returns>A <see cref="HealthCheckResult"/> indicating whether Redis is reachable along with latency metrics.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var database =
                connection.GetDatabase();

            var latency =
                await database.PingAsync();

            return HealthCheckResult.Healthy(
                $"Redis is reachable. Ping: {latency.TotalMilliseconds:F1} ms.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "Redis health check failed.",
                exception);
        }
    }
}
