using System.Text;
using System.Text.Json;
using FinSight.Application.Abstractions.Outbox;
using FinSight.Application.Abstractions.Persistence;
using FinSight.Application.Features.Subscriptions;
using FinSight.Contracts.Events;
using FinSight.Infrastructure.Messaging.RabbitMq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FinSight.Workers.Consumers;

/// <summary>
/// Consumes transaction-categorization events and detects subscriptions.
/// </summary>
public sealed partial class TransactionCategorizedConsumer
    : BackgroundService
{
    private readonly IRabbitMqConnectionProvider
        _connectionProvider;

    private readonly IServiceScopeFactory
        _scopeFactory;

    private readonly ILogger<TransactionCategorizedConsumer>
        _logger;

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="TransactionCategorizedConsumer"/> class.
    /// </summary>
    public TransactionCategorizedConsumer(
        IRabbitMqConnectionProvider connectionProvider,
        IServiceScopeFactory scopeFactory,
        ILogger<TransactionCategorizedConsumer> logger)
    {
        _connectionProvider =
            connectionProvider;

        _scopeFactory =
            scopeFactory;

        _logger =
            logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var connection =
            await _connectionProvider
                .GetConnectionAsync(
                    stoppingToken);

        var channel =
            await connection
                .CreateChannelAsync(
                    cancellationToken:
                        stoppingToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 10,
            global: false,
            cancellationToken:
                stoppingToken);

        var consumer =
            new AsyncEventingBasicConsumer(
                channel);

        consumer.ReceivedAsync +=
            async (_, eventArgs) =>
            {
                await HandleAsync(
                    channel,
                    eventArgs,
                    stoppingToken);
            };

        await channel.BasicConsumeAsync(
            RabbitMqTopology
                .SubscriptionDetectionQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken:
                stoppingToken);

        LogConsumerStarted();

        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
    }

    private async Task HandleAsync(
        IChannel channel,
        BasicDeliverEventArgs eventArgs,
        CancellationToken cancellationToken)
    {
        try
        {
            var json =
                Encoding.UTF8.GetString(
                    eventArgs.Body.ToArray());

            var message =
                JsonSerializer.Deserialize<
                    TransactionCategorizedEvent>(
                    json,
                    JsonOptions)
                ?? throw new InvalidOperationException(
                    "Unable to deserialize transaction-categorized event.");

            using var scope =
                _scopeFactory.CreateScope();

            var processedStore =
                scope.ServiceProvider
                    .GetRequiredService<
                        IProcessedMessageStore>();

            var messageId =
                eventArgs.BasicProperties.MessageId;

            if (!string.IsNullOrWhiteSpace(messageId) &&
                await processedStore.ExistsAsync(
                    messageId,
                    nameof(TransactionCategorizedConsumer),
                    cancellationToken))
            {
                await channel.BasicAckAsync(
                    eventArgs.DeliveryTag,
                    false,
                    cancellationToken);

                return;
            }

            var service =
                scope.ServiceProvider
                    .GetRequiredService<
                        SubscriptionService>();

            await service.ProcessTransactionAsync(
                message.UserId,
                message.TransactionId,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(messageId))
            {
                processedStore.Add(
                    messageId,
                    nameof(TransactionCategorizedConsumer));
            }

            var unitOfWork =
                scope.ServiceProvider
                    .GetRequiredService<
                        IUnitOfWork>();

            await unitOfWork.SaveChangesAsync(
                cancellationToken);

            await channel.BasicAckAsync(
                eventArgs.DeliveryTag,
                multiple: false,
                cancellationToken);
        }
        catch (Exception exception)
        {
            LogProcessingFailed(exception);

            await channel.BasicNackAsync(
                eventArgs.DeliveryTag,
                multiple: false,
                requeue: false,
                cancellationToken);
        }
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Subscription detection consumer started.")]
    private partial void LogConsumerStarted();

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Failed to process TransactionCategorized event.")]
    private partial void LogProcessingFailed(
        Exception exception);
}
