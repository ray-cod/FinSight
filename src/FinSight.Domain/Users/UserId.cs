using FinSight.Domain.Common;

namespace FinSight.Domain.Users;

/// <summary>
/// Represents the strongly typed identifier of a FinSight user.
/// </summary>
public sealed class UserId : ValueObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserId"/> class.
    /// </summary>
    /// <param name="value">The unique identifier value.</param>
    public UserId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "User identifier cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// Gets the underlying identifier value.
    /// </summary>
    public Guid Value { get; }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Creates a new user identifier.
    /// </summary>
    /// <returns>A new unique user identifier.</returns>
    public static UserId New() => new(Guid.NewGuid());
}
