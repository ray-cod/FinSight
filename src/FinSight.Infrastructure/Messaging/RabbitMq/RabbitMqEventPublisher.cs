using System.Text.Json;
using FinSight.Application.Abstractions.Messaging;
using RabbitMQ.Client;

namespace FinSight.Infrastructure.Messaging.RabbitMq;

/// <summary>
/// Publishes application events to RabbitMQ.
/// </summary>
public sealed class RabbitMqEventPublisher(
    IRabbitMqConnectionProvider connectionProvider)
    : IEventPublisher
{
    private const string ExchangeName =
        "finsight.events";

    /// <inheritdoc />
    public async Task PublishAsync<T>(
        T message,
        string routingKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            routingKey);

        cancellationToken.ThrowIfCancellationRequested();

        var connection =
            await connectionProvider.GetConnectionAsync(
                cancellationToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        var body =
            JsonSerializer.SerializeToUtf8Bytes(
                message);

        var properties =
            new BasicProperties
            {
                ContentType =
                    "application/json",

                ContentEncoding =
                    "utf-8",

                DeliveryMode =
                    DeliveryModes.Persistent,

                MessageId =
                    Guid.NewGuid().ToString("N"),

                Type =
                    typeof(T).FullName
            };

        await channel.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }
}
