using FinSight.Api.Extensions;
using FinSight.Application.Features.Subscriptions;
using FinSight.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinSight.Api.Controllers;

/// <summary>
/// Provides subscription intelligence endpoints.
/// </summary>
[ApiController]
[Route("api/v1/subscriptions")]
[Authorize]
public sealed class SubscriptionsController
    : ControllerBase
{
    private readonly SubscriptionService
        _subscriptionService;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="SubscriptionsController"/> class.
    /// </summary>
    /// <param name="subscriptionService">
    /// The subscription service.
    /// </param>
    public SubscriptionsController(
        SubscriptionService subscriptionService)
    {
        _subscriptionService =
            subscriptionService;
    }

    /// <summary>
    /// Gets subscriptions belonging to the authenticated user.
    /// </summary>
    /// <param name="includeDismissed">
    /// Whether dismissed subscriptions should be included.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's subscriptions.</returns>
    [HttpGet]
    public async Task<IActionResult> GetSubscriptions(
        [FromQuery] bool includeDismissed = false,
        CancellationToken cancellationToken = default)
    {
        var subscriptions =
            await _subscriptionService
                .GetForUserAsync(
                    User.GetRequiredUserId(),
                    includeDismissed,
                    cancellationToken);

        return Ok(
            subscriptions.Select(
                Map));
    }

    /// <summary>
    /// Gets a specific subscription.
    /// </summary>
    /// <param name="subscriptionId">
    /// The subscription identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>The requested subscription.</returns>
    [HttpGet("{subscriptionId:guid}")]
    public async Task<IActionResult> GetSubscription(
        Guid subscriptionId,
        CancellationToken cancellationToken)
    {
        var subscription =
            await _subscriptionService.GetAsync(
                User.GetRequiredUserId(),
                subscriptionId,
                cancellationToken);

        return Ok(Map(subscription));
    }

    /// <summary>
    /// Gets historical prices for a subscription.
    /// </summary>
    /// <param name="subscriptionId">
    /// The subscription identifier.
    /// </param>
    /// <param name="limit">
    /// Maximum number of observations.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>Subscription price history.</returns>
    [HttpGet("{subscriptionId:guid}/price-history")]
    public async Task<IActionResult> GetPriceHistory(
        Guid subscriptionId,
        [FromQuery] int limit = 24,
        CancellationToken cancellationToken = default)
    {
        var history =
            await _subscriptionService
                .GetPriceHistoryAsync(
                    User.GetRequiredUserId(),
                    subscriptionId,
                    limit,
                    cancellationToken);

        return Ok(
            history.Select(
                x =>
                    new
                    {
                        id = x.Id,
                        transactionId =
                            x.TransactionId,
                        amount =
                            x.Amount,
                        observedAt =
                            x.ObservedAt
                    }));
    }

    /// <summary>
    /// Dismisses a subscription.
    /// </summary>
    /// <param name="subscriptionId">
    /// The subscription identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    [HttpPost("{subscriptionId:guid}/dismiss")]
    public async Task<IActionResult> Dismiss(
        Guid subscriptionId,
        CancellationToken cancellationToken)
    {
        await _subscriptionService.DismissAsync(
            User.GetRequiredUserId(),
            subscriptionId,
            cancellationToken);

        return NoContent();
    }

    private static object Map(
        Domain.Subscriptions.Subscription subscription)
    {
        return new
        {
            id = subscription.Id,
            merchantId =
                subscription.MerchantId,
            merchantName =
                subscription.MerchantName,
            currency =
                subscription.Currency,
            frequency =
                subscription.Frequency,
            currentAmount =
                subscription.CurrentAmount,
            averageAmount =
                subscription.AverageAmount,
            detectionConfidence =
                subscription.DetectionConfidence,
            firstDetectedAt =
                subscription.FirstDetectedAt,
            lastChargeAt =
                subscription.LastChargeAt,
            nextExpectedChargeAt =
                subscription.NextExpectedChargeAt,
            lastPriceChangedAt =
                subscription.LastPriceChangedAt,
            status =
                subscription.Status
        };
    }
}
