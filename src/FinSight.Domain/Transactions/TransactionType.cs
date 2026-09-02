namespace FinSight.Domain.Transactions;

/// <summary>
/// Represents the type of a financial transaction.
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// A purchase or outgoing expense.
    /// </summary>
    Purchase = 1,

    /// <summary>
    /// Money received by the account.
    /// </summary>
    Deposit = 2,

    /// <summary>
    /// A transfer between accounts.
    /// </summary>
    Transfer = 3,

    /// <summary>
    /// A bank fee or service charge.
    /// </summary>
    Fee = 4,

    /// <summary>
    /// An adjustment or correction.
    /// </summary>
    Adjustment = 5,

    /// <summary>
    /// Another transaction type.
    /// </summary>
    Other = 6
}
