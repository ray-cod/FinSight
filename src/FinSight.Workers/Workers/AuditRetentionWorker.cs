using FinSight.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinSight.Workers.Workers;

/// <summary>
/// Removes audit events older than the configured retention period.
/// </summary>
public sealed partial class AuditRetentionWorker
    : BackgroundService
{
    private static readonly TimeSpan Interval =
        TimeSpan.FromDays(1);

    private readonly IServiceScopeFactory
        _scopeFactory;

    private readonly ILogger<
        AuditRetentionWorker>
        _logger;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="AuditRetentionWorker"/> class.
    /// </summary>
    public AuditRetentionWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<AuditRetentionWorker> logger)
    {
        _scopeFactory =
            scopeFactory;

        _logger =
            logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var timer =
            new PeriodicTimer(
                Interval);

        while (
            await timer.WaitForNextTickAsync(
                stoppingToken))
        {
            using var scope =
                _scopeFactory.CreateScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<
                        FinSightDbContext>();

            var cutoff =
                DateTimeOffset.UtcNow
                    .AddDays(-365);

            var deleted =
                await dbContext
                    .Set<
                        FinSight.Domain.Auditing
                            .AuditEvent>()
                    .Where(
                        x =>
                            x.OccurredAt < cutoff)
                    .ExecuteDeleteAsync(
                        stoppingToken);

            if (deleted > 0)
            {
                LogAuditEventsRemoved(
                    _logger,
                    deleted);
            }
        }
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Removed {Count} expired audit events.")]
    private static partial void LogAuditEventsRemoved(
        ILogger logger,
        int count);
}
