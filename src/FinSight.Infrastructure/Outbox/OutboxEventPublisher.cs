using System.Text.Json;
using FinSight.Application.Abstractions.Messaging;
using FinSight.Application.Abstractions.Outbox;
using FinSight.Domain.Outbox;

namespace FinSight.Infrastructure.Outbox;

/// <summary>
/// Converts integration events into transactional outbox messages.
/// </summary>
public sealed class OutboxEventPublisher(
    IOutboxRepository repository)
    : IEventPublisher,
      IOutboxWriter
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public Task PublishAsync<T>(
        T message,
        string routingKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Enqueue(
            message,
            routingKey);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Enqueue<T>(
        T @event,
        string routingKey)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var payload =
            JsonSerializer.Serialize(
                @event,
                JsonOptions);

        var message =
            OutboxMessage.Create(
                typeof(T).AssemblyQualifiedName!,
                payload,
                routingKey,
                DateTimeOffset.UtcNow);

        repository.Add(message);
    }
}
