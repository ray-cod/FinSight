using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace FinSight.Infrastructure.Messaging.RabbitMq;

/// <summary>
/// Publishes outbox messages to RabbitMQ using publisher confirms.
/// </summary>
public sealed partial class ReliableRabbitMqPublisher(
    IRabbitMqConnectionProvider connectionProvider,
    ILogger<ReliableRabbitMqPublisher> logger)
{
    /// <summary>
    /// Publishes a message and waits for RabbitMQ confirmation.
    /// </summary>
    /// <param name="routingKey">
    /// The event routing key.
    /// </param>
    /// <param name="payload">
    /// The serialized event payload.
    /// </param>
    /// <param name="messageId">
    /// The outbox message identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    public async Task PublishAsync(
        string routingKey,
        string payload,
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var connection =
            await connectionProvider
                .GetConnectionAsync(
                    cancellationToken);

        var channelOptions =
            new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true);

        await using var channel =
            await connection
                .CreateChannelAsync(
                    channelOptions,
                    cancellationToken:
                        cancellationToken);

        var properties =
            new BasicProperties
            {
                ContentType =
                    "application/json",

                DeliveryMode =
                    DeliveryModes.Persistent,

                MessageId =
                    messageId.ToString(),

                Type =
                    "finsight.integration-event"
            };

        var body =
            Encoding.UTF8.GetBytes(payload);

        await channel.BasicPublishAsync(
            RabbitMqTopology.ExchangeName,
            routingKey,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken:
                cancellationToken);

        LogOutboxMessagePublished(
            logger,
            messageId);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Outbox message {MessageId} published with RabbitMQ confirmation.")]
    private static partial void LogOutboxMessagePublished(
        ILogger logger,
        Guid messageId);
}
