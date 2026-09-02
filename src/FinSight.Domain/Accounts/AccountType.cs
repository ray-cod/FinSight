namespace FinSight.Domain.Accounts;

/// <summary>
/// Represents the type of a financial account.
/// </summary>
public enum AccountType
{
    /// <summary>
    /// A checking account.
    /// </summary>
    Checking = 1,

    /// <summary>
    /// A savings account.
    /// </summary>
    Savings = 2,

    /// <summary>
    /// A credit card account.
    /// </summary>
    CreditCard = 3,

    /// <summary>
    /// An investment account.
    /// </summary>
    Investment = 4,

    /// <summary>
    /// Another supported account type.
    /// </summary>
    Other = 5
}
