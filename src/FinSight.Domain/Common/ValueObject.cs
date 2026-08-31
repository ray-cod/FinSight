namespace FinSight.Domain.Common;

/// <summary>
/// Abstract base class for value objects defined by structural equality of their underlying components rather than identity.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Gets the atomic components that define equality for this value object.
    /// </summary>
    /// <returns>An enumerable collection of components to include in equality checks.</returns>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <summary>
    /// Determines whether the specified object is structurally equal to the current value object.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified object is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not ValueObject other)
        {
            return false;
        }

        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// Returns a hash code for this value object computed from all equality components.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(
                0,
                (current, component) =>
                    HashCode.Combine(current, component));
    }

    /// <summary>
    /// Compares two value objects for structural equality.
    /// </summary>
    /// <param name="left">The left value object operand.</param>
    /// <param name="right">The right value object operand.</param>
    /// <returns><see langword="true"/> if both value objects are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(
        ValueObject? left,
        ValueObject? right)
    {
        if (left is null && right is null)
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
    /// Compares two value objects for structural inequality.
    /// </summary>
    /// <param name="left">The left value object operand.</param>
    /// <param name="right">The right value object operand.</param>
    /// <returns><see langword="true"/> if the value objects are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(
        ValueObject? left,
        ValueObject? right)
    {
        return !(left == right);
    }
}
