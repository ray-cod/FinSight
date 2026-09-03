namespace FinSight.Domain.Subscriptions;

/// <summary>
/// Represents the estimated billing cadence of a recurring payment.
/// </summary>
public enum BillingFrequency
{
    /// <summary>
    /// The billing cadence could not be determined.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The payment occurs approximately once per week.
    /// </summary>
    Weekly = 1,

    /// <summary>
    /// The payment occurs approximately every two weeks.
    /// </summary>
    BiWeekly = 2,

    /// <summary>
    /// The payment occurs approximately once per month.
    /// </summary>
    Monthly = 3,

    /// <summary>
    /// The payment occurs approximately once per quarter.
    /// </summary>
    Quarterly = 4,

    /// <summary>
    /// The payment occurs approximately twice per year.
    /// </summary>
    SemiAnnual = 5,

    /// <summary>
    /// The payment occurs approximately once per year.
    /// </summary>
    Annual = 6
}
