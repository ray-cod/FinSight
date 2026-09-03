namespace FinSight.Domain.Categories;

/// <summary>
/// Represents the semantic type of a financial category.
/// </summary>
public enum CategoryType
{
    /// <summary>
    /// A normal expense category.
    /// </summary>
    Expense = 1,

    /// <summary>
    /// Income received by the user.
    /// </summary>
    Income = 2,

    /// <summary>
    /// A transfer between accounts.
    /// </summary>
    Transfer = 3,

    /// <summary>
    /// A bank or service fee.
    /// </summary>
    Fee = 4,

    /// <summary>
    /// An uncategorized or unknown transaction.
    /// </summary>
    Other = 5
}
