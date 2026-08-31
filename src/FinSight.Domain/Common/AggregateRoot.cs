namespace FinSight.Domain.Common;

/// <summary>
/// Abstract base class for domain aggregate roots that encapsulate domain logic and manage recorded domain events.
/// </summary>
/// <typeparam name="TId">The type of the unique identifier for the aggregate root.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
{
    private readonly List<DomainEvent> _domainEvents = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class for ORM deserialization.
    /// </summary>
    protected AggregateRoot()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the aggregate root.</param>
    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    /// <summary>
    /// Gets a read-only collection of domain events raised by this aggregate root.
    /// </summary>
    public IReadOnlyCollection<DomainEvent> DomainEvents =>
        _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the aggregate root's internal pending events collection.
    /// </summary>
    /// <param name="domainEvent">The domain event instance to record.</param>
    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears and returns all pending domain events recorded by this aggregate root.
    /// </summary>
    /// <returns>A read-only list containing all dequeued <see cref="DomainEvent"/> instances.</returns>
    public IReadOnlyList<DomainEvent> DequeueDomainEvents()
    {
        if (_domainEvents.Count == 0)
        {
            return [];
        }

        var events = _domainEvents.ToArray();
        _domainEvents.Clear();

        return events;
    }
}
