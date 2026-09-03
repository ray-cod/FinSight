namespace FinSight.Domain.Transactions;

/// <summary>
/// Identifies the mechanism that produced a transaction classification.
/// </summary>
public enum ClassificationSource
{
    /// <summary>
    /// No classification source exists yet.
    /// </summary>
    None = 0,

    /// <summary>
    /// Classification was produced by deterministic business rules.
    /// </summary>
    Rule = 1,

    /// <summary>
    /// Classification was retrieved from the cache.
    /// </summary>
    Cache = 2,

    /// <summary>
    /// Classification was produced by artificial intelligence.
    /// </summary>
    Ai = 3,

    /// <summary>
    /// Classification was explicitly provided by the user.
    /// </summary>
    User = 4
}
