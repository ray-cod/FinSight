namespace FinSight.Application.Abstractions.Messaging;

/// <summary>
/// Defines a contract for publishing event messages across the application messaging infrastructure.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Asynchronously publishes an event message to a specified routing target.
    /// </summary>
    /// <typeparam name="T">The type of event message to publish.</typeparam>
    /// <param name="message">The event payload instance.</param>
    /// <param name="routingKey">The destination routing key or topic identifier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task PublishAsync<T>(
        T message,
        string routingKey,
        CancellationToken cancellationToken = default);
}
