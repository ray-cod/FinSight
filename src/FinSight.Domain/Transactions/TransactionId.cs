using FinSight.Domain.Common;

namespace FinSight.Domain.Transactions;

/// <summary>
/// Represents the strongly typed identifier of a financial transaction.
/// </summary>
public sealed class TransactionId : ValueObject
{
    /// <summary>
    /// Initializes a new transaction identifier.
    /// </summary>
    /// <param name="value">The identifier value.</param>
    public TransactionId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Transaction identifier cannot be empty.",
                nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// Gets the underlying identifier.
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Creates a new transaction identifier.
    /// </summary>
    /// <returns>A new transaction identifier.</returns>
    public static TransactionId New() =>
        new(Guid.NewGuid());

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Converts a transaction identifier to a GUID.
    /// </summary>
    /// <param name="transactionId">The identifier.</param>
    public static implicit operator Guid(
        TransactionId transactionId) =>
        transactionId.Value;
}
