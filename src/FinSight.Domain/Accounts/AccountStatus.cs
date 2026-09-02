namespace FinSight.Domain.Accounts;

/// <summary>
/// Represents the current status of a financial account.
/// </summary>
public enum AccountStatus
{
    /// <summary>
    /// The account is active.
    /// </summary>
    Active = 1,

    /// <summary>
    /// The account has been closed.
    /// </summary>
    Closed = 2,

    /// <summary>
    /// The account is temporarily unavailable.
    /// </summary>
    Unavailable = 3
}
