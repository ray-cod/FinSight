namespace FinSight.Domain.Transactions;

/// <summary>
/// Represents the processing status of a financial transaction.
/// </summary>
public enum TransactionStatus
{
    /// <summary>
    /// The transaction was imported from the provider.
    /// </summary>
    Imported = 1,

    /// <summary>
    /// The transaction is pending provider settlement.
    /// </summary>
    Pending = 2,

    /// <summary>
    /// The transaction has been cancelled or reversed.
    /// </summary>
    Cancelled = 3
}
