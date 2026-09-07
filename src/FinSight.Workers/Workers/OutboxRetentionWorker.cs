using FinSight.Domain.Outbox;
using FinSight.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinSight.Workers.Workers;

/// <summary>
/// Removes successfully published outbox messages after retention.
/// </summary>
public sealed partial class OutboxRetentionWorker
    : BackgroundService
{
    private static readonly TimeSpan Interval =
        TimeSpan.FromHours(6);

    private readonly IServiceScopeFactory
        _scopeFactory;

    private readonly ILogger<
        OutboxRetentionWorker>
        _logger;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="OutboxRetentionWorker"/> class.
    /// </summary>
    public OutboxRetentionWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxRetentionWorker> logger)
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
                    .AddDays(-7);

            var deleted =
                await dbContext
                    .Set<OutboxMessage>()
                    .Where(
                        x =>
                            x.Status ==
                            OutboxMessageStatus.Published &&
                            x.PublishedAt <
                            cutoff)
                    .ExecuteDeleteAsync(
                        stoppingToken);

            if (deleted > 0)
            {
                LogOutboxMessagesRemoved(
                    _logger,
                    deleted);
            }
        }
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Removed {Count} published outbox messages.")]
    private static partial void LogOutboxMessagesRemoved(
        ILogger logger,
        int count);
}
