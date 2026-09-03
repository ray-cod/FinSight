using FinSight.Domain.Subscriptions;

namespace FinSight.Application.Features.Subscriptions;

/// <summary>
/// Represents the result of recurring-payment analysis.
/// </summary>
public sealed record SubscriptionDetectionResult(
    bool IsSubscription,
    BillingFrequency Frequency,
    decimal Confidence,
    decimal AverageAmount,
    decimal CurrentAmount,
    DateTimeOffset FirstObservedAt,
    DateTimeOffset LastObservedAt,
    DateTimeOffset? NextExpectedChargeAt,
    decimal? PreviousAmount,
    decimal? PriceChangePercentage);
