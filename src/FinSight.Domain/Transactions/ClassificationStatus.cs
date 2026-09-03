namespace FinSight.Domain.Transactions;

/// <summary>
/// Represents the status of transaction classification.
/// </summary>
public enum ClassificationStatus
{
    /// <summary>
    /// The transaction has not been classified yet.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// A classification was applied successfully.
    /// </summary>
    Classified = 2,

    /// <summary>
    /// The classification is uncertain and requires additional review.
    /// </summary>
    Uncertain = 3,

    /// <summary>
    /// Classification processing failed.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// The user explicitly corrected the classification.
    /// </summary>
    UserCorrected = 5
}
