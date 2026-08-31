namespace FinSight.Domain.Common;

/// <summary>
/// Abstract base class representing a domain entity defined by its identity.
/// </summary>
/// <typeparam name="TId">The type of the entity's unique identifier.</typeparam>
public abstract class Entity<TId>
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    public TId Id { get; protected set; } = default!;

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TId}"/> class for ORM deserialization.
    /// </summary>
    protected Entity()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TId}"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current entity based on type and unique identifier.
    /// </summary>
    /// <param name="obj">The object to compare with the current entity.</param>
    /// <returns><see langword="true"/> if the specified object is an entity of the same type with an identical identifier; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <summary>
    /// Returns a hash code for this entity based on its runtime type and unique identifier.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }

    /// <summary>
    /// Compares two entity instances for equality.
    /// </summary>
    /// <param name="left">The left entity operand.</param>
    /// <param name="right">The right entity operand.</param>
    /// <returns><see langword="true"/> if both entities are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    /// <summary>
    /// Compares two entity instances for inequality.
    /// </summary>
    /// <param name="left">The left entity operand.</param>
    /// <param name="right">The right entity operand.</param>
    /// <returns><see langword="true"/> if the entities are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }
}
