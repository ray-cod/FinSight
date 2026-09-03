using System.Text;
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
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public async Task PublishAsync<T>(
        T message,
        string routingKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentException.ThrowIfNullOrWhiteSpace(
            routingKey);

        var connection =
            await connectionProvider
                .GetConnectionAsync(
                    cancellationToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        var body =
            Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(
                    message,
                    JsonOptions));

        var properties =
            new BasicProperties
            {
                ContentType =
                    "application/json",

                DeliveryMode =
                    DeliveryModes.Persistent,

                MessageId =
                    ExtractEventId(message)
            };

        await channel.BasicPublishAsync(
            RabbitMqTopology.ExchangeName,
            routingKey,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }

    private static string? ExtractEventId<T>(
        T message)
    {
        var property =
            typeof(T).GetProperty(
                "EventId");

        return property?
            .GetValue(message)?
            .ToString();
    }
}
