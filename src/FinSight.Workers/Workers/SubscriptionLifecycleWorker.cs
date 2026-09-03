using FinSight.Application.Features.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinSight.Workers.Workers;

/// <summary>
/// Periodically evaluates detected subscriptions for inactivity.
/// </summary>
public sealed partial class SubscriptionLifecycleWorker
    : BackgroundService
{
    private static readonly TimeSpan Interval =
        TimeSpan.FromHours(24);

    private readonly IServiceScopeFactory _scopeFactory;

    private readonly ILogger<SubscriptionLifecycleWorker>
        _logger;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="SubscriptionLifecycleWorker"/> class.
    /// </summary>
    public SubscriptionLifecycleWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<SubscriptionLifecycleWorker> logger)
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

        await EvaluateAsync(
            stoppingToken);

        using var timer =
            new PeriodicTimer(
                Interval);

        while (
            await timer.WaitForNextTickAsync(
                stoppingToken))
        {
            await EvaluateAsync(
                stoppingToken);
        }
    }

    private async Task EvaluateAsync(
        CancellationToken cancellationToken)
    {
        using var scope =
            _scopeFactory.CreateScope();

        var service =
            scope.ServiceProvider
                .GetRequiredService<
                    SubscriptionService>();

        await service.MarkOverdueInactiveAsync(
            DateTimeOffset.UtcNow,
            cancellationToken);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Subscription lifecycle worker started.")]
    private partial void LogWorkerStarted();
}
