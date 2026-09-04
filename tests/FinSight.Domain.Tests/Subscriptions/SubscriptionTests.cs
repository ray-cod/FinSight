using FinSight.Domain.Subscriptions;
using FluentAssertions;

namespace FinSight.Domain.Tests.Subscriptions;

/// <summary>
/// Tests subscription domain behavior.
/// </summary>
public sealed class SubscriptionTests
{
    /// <summary>
    /// Verifies that a subscription is created active.
    /// </summary>
    [Fact]
    public void CreateShouldCreateActiveSubscription()
    {
        var subscription =
            Subscription.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Netflix",
                "USD",
                BillingFrequency.Monthly,
                15.99m,
                0.95m,
                DateTimeOffset.UtcNow.AddMonths(-3),
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddMonths(1));

        subscription.Status
            .Should()
            .Be(SubscriptionStatus.Active);

        subscription.Frequency
            .Should()
            .Be(BillingFrequency.Monthly);

        subscription.CurrentAmount
            .Should()
            .Be(15.99m);
    }

    /// <summary>
    /// Verifies that a subscription can be dismissed.
    /// </summary>
    [Fact]
    public void DismissShouldMarkSubscriptionDismissed()
    {
        var subscription =
            Subscription.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Netflix",
                "USD",
                BillingFrequency.Monthly,
                15.99m,
                0.95m,
                DateTimeOffset.UtcNow.AddMonths(-3),
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddMonths(1));

        subscription.Dismiss();

        subscription.Status
            .Should()
            .Be(SubscriptionStatus.Dismissed);
    }

    /// <summary>
    /// Verifies that an inactive subscription can reactivate.
    /// </summary>
    [Fact]
    public void ObserveChargeShouldReactivateSubscription()
    {
        var subscription =
            Subscription.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Netflix",
                "USD",
                BillingFrequency.Monthly,
                15.99m,
                0.95m,
                DateTimeOffset.UtcNow.AddMonths(-3),
                DateTimeOffset.UtcNow.AddMonths(-1),
                DateTimeOffset.UtcNow);

        subscription.MarkInactive();

        subscription.ObserveCharge(
            BillingFrequency.Monthly,
            17.99m,
            16.99m,
            0.96m,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMonths(1),
            true);

        subscription.Status
            .Should()
            .Be(SubscriptionStatus.Active);

        subscription.CurrentAmount
            .Should()
            .Be(17.99m);
    }
}
