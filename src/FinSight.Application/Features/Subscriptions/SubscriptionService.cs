using FinSight.Application.Abstractions.Messaging;
using FinSight.Application.Abstractions.Persistence;
using FinSight.Contracts.Events;
using FinSight.Domain.Subscriptions;
using FinSight.Domain.Transactions;
using Microsoft.Extensions.Logging;

namespace FinSight.Application.Features.Subscriptions;

/// <summary>
/// Coordinates subscription detection, updates, price history,
/// and lifecycle operations.
/// </summary>
public sealed partial class SubscriptionService
{
    private const decimal PriceChangeThreshold = 0.05m;

    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMerchantRepository _merchantRepository;
    private readonly SubscriptionDetectionService _detectionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<SubscriptionService> _logger;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="SubscriptionService"/> class.
    /// </summary>
    public SubscriptionService(
        ISubscriptionRepository subscriptionRepository,
        ITransactionRepository transactionRepository,
        IMerchantRepository merchantRepository,
        SubscriptionDetectionService detectionService,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        ILogger<SubscriptionService> logger)
    {
        _subscriptionRepository =
            subscriptionRepository;

        _transactionRepository =
            transactionRepository;

        _merchantRepository =
            merchantRepository;

        _detectionService =
            detectionService;

        _unitOfWork =
            unitOfWork;

        _eventPublisher =
            eventPublisher;

        _logger =
            logger;
    }

    /// <summary>
    /// Re-evaluates a merchant's transaction history for a user.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="transactionId">
    /// The newly categorized transaction.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task ProcessTransactionAsync(
        Guid userId,
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        var transaction =
            await _transactionRepository
                .GetByIdAsync(
                    userId,
                    transactionId,
                    cancellationToken);

        if (transaction is null)
        {
            throw new KeyNotFoundException(
                "Transaction was not found.");
        }

        if (transaction.MerchantId is null ||
            transaction.Amount >= 0 ||
            transaction.Type !=
            TransactionType.Purchase ||
            transaction.ClassificationStatus ==
            ClassificationStatus.Failed)
        {
            return;
        }

        var detection =
            await _detectionService
                .AnalyzeAsync(
                    userId,
                    transaction.MerchantId.Value,
                    transaction.Currency,
                    cancellationToken);

        if (!detection.IsSubscription)
        {
            return;
        }

        var merchant =
            await _merchantRepository
                .GetByIdAsync(
                    transaction.MerchantId.Value,
                    cancellationToken);

        if (merchant is null)
        {
            return;
        }

        var merchantName =
            merchant.CanonicalName;

        var subscription =
            await _subscriptionRepository
                .GetByMerchantAsync(
                    userId,
                    transaction.MerchantId.Value,
                    transaction.Currency,
                    cancellationToken);

        if (subscription is null)
        {
            subscription =
                Subscription.Create(
                    userId,
                    transaction.MerchantId.Value,
                    merchantName,
                    transaction.Currency,
                    detection.Frequency,
                    detection.CurrentAmount,
                    detection.Confidence,
                    detection.FirstObservedAt,
                    detection.LastObservedAt,
                    detection.NextExpectedChargeAt);

            _subscriptionRepository.Add(
                subscription);

            await _unitOfWork
                .SaveChangesAsync(
                    cancellationToken);

            await AddPriceObservationIfNeededAsync(
                subscription,
                transaction,
                cancellationToken);

            await _unitOfWork
                .SaveChangesAsync(
                    cancellationToken);

            await _eventPublisher.PublishAsync(
                new SubscriptionDetectedEvent
                {
                    EventId = Guid.NewGuid(),
                    UserId = userId,
                    SubscriptionId =
                        subscription.Id,
                    MerchantId =
                        subscription.MerchantId,
                    MerchantName =
                        subscription.MerchantName,
                    Amount =
                        subscription.CurrentAmount,
                    Currency =
                        subscription.Currency,
                    Frequency =
                        subscription.Frequency.ToString(),
                    Confidence =
                        subscription.DetectionConfidence,
                    OccurredAt =
                        DateTimeOffset.UtcNow
                },
                "subscription.detected",
                cancellationToken);

            return;
        }

        if (subscription.Status ==
            SubscriptionStatus.Dismissed)
        {
            return;
        }

        var previousAmount =
            subscription.CurrentAmount;

        var priceChanged =
            previousAmount > 0 &&
            Math.Abs(
                detection.CurrentAmount -
                previousAmount) /
            previousAmount >=
            PriceChangeThreshold;

        subscription.ObserveCharge(
            detection.Frequency,
            detection.CurrentAmount,
            detection.AverageAmount,
            detection.Confidence,
            detection.LastObservedAt,
            detection.NextExpectedChargeAt,
            priceChanged);

        await _unitOfWork
            .SaveChangesAsync(
                cancellationToken);

        var observationAdded =
            await AddPriceObservationIfNeededAsync(
                subscription,
                transaction,
                cancellationToken);

        await _unitOfWork
            .SaveChangesAsync(
                cancellationToken);

        if (priceChanged &&
            observationAdded)
        {
            var percentage =
                previousAmount == 0
                    ? 0m
                    : (detection.CurrentAmount -
                       previousAmount) /
                      previousAmount;

            await _eventPublisher.PublishAsync(
                new SubscriptionPriceChangedEvent
                {
                    EventId = Guid.NewGuid(),
                    UserId = userId,
                    SubscriptionId =
                        subscription.Id,
                    MerchantId =
                        subscription.MerchantId,
                    MerchantName =
                        subscription.MerchantName,
                    PreviousAmount =
                        previousAmount,
                    CurrentAmount =
                        detection.CurrentAmount,
                    ChangePercentage =
                        percentage,
                    Currency =
                        subscription.Currency,
                    TransactionId =
                        transaction.Id.Value,
                    OccurredAt =
                        DateTimeOffset.UtcNow
                },
                "subscription.price.changed",
                cancellationToken);
        }

        LogSubscriptionProcessed(
            subscription.Id,
            userId);
    }

    /// <summary>
    /// Gets subscriptions belonging to a user.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="includeDismissed">
    /// Whether dismissed subscriptions should be included.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's subscriptions.</returns>
    public async Task<IReadOnlyList<Subscription>>
        GetForUserAsync(
            Guid userId,
            bool includeDismissed = false,
            CancellationToken cancellationToken = default)
    {
        return await _subscriptionRepository
            .GetByUserIdAsync(
                userId,
                includeDismissed,
                cancellationToken);
    }

    /// <summary>
    /// Gets a subscription owned by the authenticated user.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="subscriptionId">
    /// The subscription identifier.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The subscription.</returns>
    public async Task<Subscription>
        GetAsync(
            Guid userId,
            Guid subscriptionId,
            CancellationToken cancellationToken = default)
    {
        var subscription =
            await _subscriptionRepository
                .GetByIdAsync(
                    userId,
                    subscriptionId,
                    cancellationToken);

        if (subscription is null)
        {
            throw new KeyNotFoundException(
                "Subscription was not found.");
        }

        return subscription;
    }

    /// <summary>
    /// Gets subscription price history for the authenticated user.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="subscriptionId">The subscription identifier.</param>
    /// <param name="limit">Maximum observations.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Price history.</returns>
    public async Task<
        IReadOnlyList<SubscriptionPriceHistory>>
        GetPriceHistoryAsync(
            Guid userId,
            Guid subscriptionId,
            int limit = 24,
            CancellationToken cancellationToken = default)
    {
        await GetAsync(
            userId,
            subscriptionId,
            cancellationToken);

        return await _subscriptionRepository
            .GetPriceHistoryAsync(
                subscriptionId,
                limit,
                cancellationToken);
    }

    /// <summary>
    /// Dismisses a subscription for the authenticated user.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="subscriptionId">
    /// The subscription identifier.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task DismissAsync(
        Guid userId,
        Guid subscriptionId,
        CancellationToken cancellationToken = default)
    {
        var subscription =
            await GetAsync(
                userId,
                subscriptionId,
                cancellationToken);

        subscription.Dismiss();

        await _unitOfWork
            .SaveChangesAsync(
                cancellationToken);
    }

    /// <summary>
    /// Marks overdue subscriptions as inactive.
    /// </summary>
    /// <param name="asOf">The evaluation timestamp.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task MarkOverdueInactiveAsync(
        DateTimeOffset asOf,
        CancellationToken cancellationToken = default)
    {
        var overdue =
            await _subscriptionRepository
                .GetOverdueAsync(
                    asOf,
                    cancellationToken);

        foreach (var subscription in overdue)
        {
            var nextExpected =
                subscription.NextExpectedChargeAt;

            if (!nextExpected.HasValue)
            {
                continue;
            }

            var gracePeriod =
                GetGracePeriod(
                    subscription.Frequency);

            if (asOf <
                nextExpected.Value.Add(
                    gracePeriod))
            {
                continue;
            }

            subscription.MarkInactive();
        }

        if (overdue.Count > 0)
        {
            await _unitOfWork
                .SaveChangesAsync(
                    cancellationToken);
        }
    }

    private async Task<bool>
        AddPriceObservationIfNeededAsync(
            Subscription subscription,
            Domain.Transactions.Transaction transaction,
            CancellationToken cancellationToken)
    {
        var exists =
            await _subscriptionRepository
                .HasPriceObservationAsync(
                    subscription.Id,
                    transaction.Id.Value,
                    cancellationToken);

        if (exists)
        {
            return false;
        }

        var history =
            SubscriptionPriceHistory.Create(
                subscription.Id,
                transaction.Id.Value,
                Math.Abs(transaction.Amount),
                transaction.TransactionDate);

        _subscriptionRepository
            .AddPriceHistory(history);

        return true;
    }

    private static TimeSpan GetGracePeriod(
        BillingFrequency frequency)
    {
        return frequency switch
        {
            BillingFrequency.Weekly =>
                TimeSpan.FromDays(7),

            BillingFrequency.BiWeekly =>
                TimeSpan.FromDays(14),

            BillingFrequency.Monthly =>
                TimeSpan.FromDays(30),

            BillingFrequency.Quarterly =>
                TimeSpan.FromDays(45),

            BillingFrequency.SemiAnnual =>
                TimeSpan.FromDays(60),

            BillingFrequency.Annual =>
                TimeSpan.FromDays(90),

            _ =>
                TimeSpan.FromDays(30)
        };
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Subscription {SubscriptionId} processed for user {UserId}.")]
    private partial void LogSubscriptionProcessed(
        Guid subscriptionId,
        Guid userId);
}
