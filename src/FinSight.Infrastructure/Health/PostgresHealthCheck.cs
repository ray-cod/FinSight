using FinSight.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FinSight.Infrastructure.Health;

/// <summary>
/// Performs a health check to verify connectivity to the PostgreSQL database.
/// </summary>
/// <param name="dbContext">The database context used to verify connectivity.</param>
public sealed class PostgresHealthCheck(
    FinSightDbContext dbContext)
    : IHealthCheck
{
    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect =
                await dbContext.Database.CanConnectAsync(
                    cancellationToken);

            return canConnect
                ? HealthCheckResult.Healthy(
                    "PostgreSQL is reachable.")
                : HealthCheckResult.Unhealthy(
                    "PostgreSQL is not reachable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "PostgreSQL health check failed.",
                exception);
        }
    }
}
