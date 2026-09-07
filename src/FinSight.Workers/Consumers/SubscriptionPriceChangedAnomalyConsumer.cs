using System.Text;
using System.Text.Json;
using FinSight.Application.Abstractions.Outbox;
using FinSight.Application.Abstractions.Persistence;
using FinSight.Application.Features.Insights;
using FinSight.Contracts.Events;
using FinSight.Infrastructure.Messaging.RabbitMq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FinSight.Workers.Consumers;

/// <summary>
/// Consumes subscription price changes and creates financial insights.
/// </summary>
public sealed partial class SubscriptionPriceChangedAnomalyConsumer
    : BackgroundService
{
    private readonly IRabbitMqConnectionProvider
        _connectionProvider;

    private readonly IServiceScopeFactory
        _scopeFactory;

    private readonly ILogger<
        SubscriptionPriceChangedAnomalyConsumer>
        _logger;

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="SubscriptionPriceChangedAnomalyConsumer"/> class.
    /// </summary>
    public SubscriptionPriceChangedAnomalyConsumer(
        IRabbitMqConnectionProvider connectionProvider,
        IServiceScopeFactory scopeFactory,
        ILogger<SubscriptionPriceChangedAnomalyConsumer> logger)
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

        var consumer =
            new AsyncEventingBasicConsumer(
                channel);

        consumer.ReceivedAsync +=
            async (_, args) =>
            {
                await HandleAsync(
                    channel,
                    args,
                    stoppingToken);
            };

        await channel.BasicConsumeAsync(
            RabbitMqTopology
                .SubscriptionPriceAnomalyQueue,
            autoAck: false,
            consumer,
            stoppingToken);

        LogConsumerStarted();

        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
    }

    private async Task HandleAsync(
        IChannel channel,
        BasicDeliverEventArgs args,
        CancellationToken cancellationToken)
    {
        try
        {
            var json =
                Encoding.UTF8.GetString(
                    args.Body.ToArray());

            var message =
                JsonSerializer.Deserialize<
                    SubscriptionPriceChangedEvent>(
                    json,
                    JsonOptions)
                ?? throw new InvalidOperationException(
                    "Invalid SubscriptionPriceChanged event.");

            using var scope =
                _scopeFactory.CreateScope();

            var processedStore =
                scope.ServiceProvider
                    .GetRequiredService<
                        IProcessedMessageStore>();

            var messageId =
                args.BasicProperties.MessageId;

            if (!string.IsNullOrWhiteSpace(messageId) &&
                await processedStore.ExistsAsync(
                    messageId,
                    nameof(SubscriptionPriceChangedAnomalyConsumer),
                    cancellationToken))
            {
                await channel.BasicAckAsync(
                    args.DeliveryTag,
                    false,
                    cancellationToken);

                return;
            }

            var service =
                scope.ServiceProvider
                    .GetRequiredService<
                        SubscriptionPriceInsightService>();

            await service.ProcessAsync(
                message,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(messageId))
            {
                processedStore.Add(
                    messageId,
                    nameof(SubscriptionPriceChangedAnomalyConsumer));
            }

            var unitOfWork =
                scope.ServiceProvider
                    .GetRequiredService<
                        IUnitOfWork>();

            await unitOfWork.SaveChangesAsync(
                cancellationToken);

            await channel.BasicAckAsync(
                args.DeliveryTag,
                multiple: false,
                cancellationToken);
        }
        catch (Exception exception)
        {
            LogInsightCreationFailed(
                exception);

            await channel.BasicNackAsync(
                args.DeliveryTag,
                multiple: false,
                requeue: false,
                cancellationToken);
        }
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Subscription price insight consumer started.")]
    private partial void LogConsumerStarted();

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Failed to create subscription price insight.")]
    private partial void LogInsightCreationFailed(
        Exception exception);
}
