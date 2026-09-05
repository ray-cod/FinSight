using System.Text;
using System.Text.Json;
using FinSight.Application.Abstractions.Outbox;
using FinSight.Application.Abstractions.Persistence;
using FinSight.Application.Features.Notifications;
using FinSight.Contracts.Events;
using FinSight.Infrastructure.Messaging.RabbitMq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FinSight.Workers.Consumers;

/// <summary>
/// Consumes notification delivery events.
/// </summary>
public sealed partial class NotificationCreatedConsumer
    : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly IRabbitMqConnectionProvider
        _connectionProvider;

    private readonly IServiceScopeFactory
        _scopeFactory;

    private readonly ILogger<
        NotificationCreatedConsumer>
        _logger;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="NotificationCreatedConsumer"/> class.
    /// </summary>
    public NotificationCreatedConsumer(
        IRabbitMqConnectionProvider connectionProvider,
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationCreatedConsumer> logger)
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
            0,
            10,
            false,
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
            RabbitMqTopology.NotificationQueue,
            false,
            consumer,
            stoppingToken);

        LogConsumerStarted(
            _logger);

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
                    NotificationCreatedEvent>(
                    json,
                    JsonOptions)
                ?? throw new InvalidOperationException(
                    "Invalid notification event.");

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
                    nameof(NotificationCreatedConsumer),
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
                        NotificationDeliveryService>();

            await service.DeliverAsync(
                message.NotificationId,
                message.UserId,
                message.Recipient,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(messageId))
            {
                processedStore.Add(
                    messageId,
                    nameof(NotificationCreatedConsumer));
            }

            var unitOfWork =
                scope.ServiceProvider
                    .GetRequiredService<
                        IUnitOfWork>();

            await unitOfWork.SaveChangesAsync(
                cancellationToken);

            await channel.BasicAckAsync(
                args.DeliveryTag,
                false,
                cancellationToken);
        }
        catch (Exception exception)
        {
            LogDeliveryFailed(
                _logger,
                exception);

            await channel.BasicNackAsync(
                args.DeliveryTag,
                false,
                false,
                cancellationToken);
        }
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Notification delivery consumer started.")]
    private static partial void LogConsumerStarted(
        ILogger logger);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Notification delivery failed.")]
    private static partial void LogDeliveryFailed(
        ILogger logger,
        Exception exception);
}
