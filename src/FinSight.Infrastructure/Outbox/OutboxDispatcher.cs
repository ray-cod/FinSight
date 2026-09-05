using FinSight.Application.Abstractions.Outbox;
using FinSight.Application.Abstractions.Persistence;
using FinSight.Domain.Outbox;
using FinSight.Infrastructure.Messaging.RabbitMq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinSight.Infrastructure.Outbox;

/// <summary>
/// Publishes transactional outbox messages to RabbitMQ.
/// </summary>
public sealed partial class OutboxDispatcher(
    IServiceScopeFactory scopeFactory,
    ReliableRabbitMqPublisher publisher,
    ILogger<OutboxDispatcher> logger)
    : BackgroundService
{
    private const int BatchSize = 50;

    private const int MaximumAttempts = 10;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        LogOutboxDispatcherStarted(
            logger);

        while (
            !stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processed =
                    await DispatchBatchAsync(
                        stoppingToken);

                if (processed == 0)
                {
                    await Task.Delay(
                        TimeSpan.FromSeconds(2),
                        stoppingToken);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                LogOutboxDispatchCycleFailed(
                    logger,
                    exception);

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }
        }

        LogOutboxDispatcherStopped(
            logger);
    }

    private async Task<int> DispatchBatchAsync(
        CancellationToken cancellationToken)
    {
        using var scope =
            scopeFactory.CreateScope();

        var repository =
            scope.ServiceProvider
                .GetRequiredService<
                    IOutboxRepository>();

        var unitOfWork =
            scope.ServiceProvider
                .GetRequiredService<
                    IUnitOfWork>();

        var messages =
            await repository.GetPendingAsync(
                DateTimeOffset.UtcNow,
                BatchSize,
                cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await publisher.PublishAsync(
                    message.RoutingKey,
                    message.Payload,
                    message.Id,
                    cancellationToken);

                message.MarkPublished();
            }
            catch (Exception exception)
            {
                if (
                    message.AttemptCount + 1 >=
                    MaximumAttempts)
                {
                    message.MarkDeadLettered(
                        exception.Message);
                }
                else
                {
                    var retryAt =
                        DateTimeOffset.UtcNow.Add(
                            CalculateBackoff(
                                message.AttemptCount));

                    message.RecordFailure(
                        exception.Message,
                        retryAt);
                }
            }
        }

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        return messages.Count;
    }

    private static TimeSpan CalculateBackoff(
        int attempt)
    {
        var seconds =
            Math.Min(
                300,
                Math.Pow(
                    2,
                    Math.Min(attempt, 8)));

        return TimeSpan.FromSeconds(
            seconds);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Outbox dispatcher started.")]
    private static partial void LogOutboxDispatcherStarted(
        ILogger logger);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Outbox dispatch cycle failed.")]
    private static partial void LogOutboxDispatchCycleFailed(
        ILogger logger,
        Exception exception);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Outbox dispatcher stopped.")]
    private static partial void LogOutboxDispatcherStopped(
        ILogger logger);
}
