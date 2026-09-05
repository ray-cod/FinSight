using FinSight.Application.Abstractions.Persistence;
using FinSight.Application.Features.Anomalies;
using FinSight.Application.Features.Insights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinSight.Workers.Workers;

/// <summary>
/// Periodically expires stale anomalies and financial insights.
/// </summary>
public sealed partial class AnomalyLifecycleWorker
    : BackgroundService
{
    private static readonly TimeSpan Interval =
        TimeSpan.FromHours(24);

    private readonly IServiceScopeFactory
        _scopeFactory;

    private readonly ILogger<AnomalyLifecycleWorker>
        _logger;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="AnomalyLifecycleWorker"/> class.
    /// </summary>
    public AnomalyLifecycleWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<AnomalyLifecycleWorker> logger)
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
        LogWorkerStarted();

        await RunOnceAsync(
            stoppingToken);

        using var timer =
            new PeriodicTimer(
                Interval);

        while (
            await timer.WaitForNextTickAsync(
                stoppingToken))
        {
            await RunOnceAsync(
                stoppingToken);
        }
    }

    private async Task RunOnceAsync(
        CancellationToken cancellationToken)
    {
        using var scope =
            _scopeFactory.CreateScope();

        var anomalyRepository =
            scope.ServiceProvider
                .GetRequiredService<
                    IAnomalyRepository>();

        var unitOfWork =
            scope.ServiceProvider
                .GetRequiredService<
                    IUnitOfWork>();

        var staleAnomalies =
            await anomalyRepository
                .GetOpenBeforeAsync(
                    DateTimeOffset.UtcNow,
                    cancellationToken);

        foreach (var anomaly in staleAnomalies)
        {
            anomaly.Resolve();
        }

        if (staleAnomalies.Count > 0)
        {
            await unitOfWork
                .SaveChangesAsync(
                    cancellationToken);
        }

        var insightService =
            scope.ServiceProvider
                .GetRequiredService<
                    InsightService>();

        await insightService.ExpireAsync(
            DateTimeOffset.UtcNow,
            cancellationToken);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Anomaly lifecycle worker started.")]
    private partial void LogWorkerStarted();
}
