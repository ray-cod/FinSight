namespace FinSight.Application.Abstractions.Outbox;

/// <summary>
/// Writes integration events to the transactional outbox.
/// </summary>
public interface IOutboxWriter
{
    /// <summary>
    /// Enqueues an integration event for publication.
    /// </summary>
    /// <typeparam name="T">The event contract type.</typeparam>
    /// <param name="integrationEvent">The event instance.</param>
    /// <param name="routingKey">The RabbitMQ routing key.</param>
    void Enqueue<T>(
        T integrationEvent,
        string routingKey);
}
