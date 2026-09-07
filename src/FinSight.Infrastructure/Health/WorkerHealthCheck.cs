using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FinSight.Infrastructure.Health;

/// <summary>
/// Reports the health of the FinSight worker process.
/// </summary>
public sealed class WorkerHealthCheck
    : IHealthCheck
{
    /// <inheritdoc />
    public Task<HealthCheckResult>
        CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            HealthCheckResult.Healthy(
                "FinSight worker process is running."));
    }
}
