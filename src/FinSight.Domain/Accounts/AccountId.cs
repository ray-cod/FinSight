using FinSight.Domain.Common;

namespace FinSight.Domain.Accounts;

/// <summary>
/// Represents the strongly typed identifier of a financial account.
/// </summary>
public sealed class AccountId : ValueObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountId"/> class.
    /// </summary>
    /// <param name="value">The underlying account identifier.</param>
    public AccountId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Account identifier cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// Gets the underlying identifier.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Creates a new account identifier.
    /// </summary>
    /// <returns>A new account identifier.</returns>
    public static AccountId New() =>
        new(Guid.NewGuid());

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Implicitly converts an account identifier to a GUID.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    public static implicit operator Guid(
        AccountId accountId) =>
        accountId.Value;
}
