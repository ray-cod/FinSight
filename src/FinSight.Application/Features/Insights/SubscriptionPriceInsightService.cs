using FinSight.Application.Abstractions.Messaging;
using FinSight.Application.Abstractions.Persistence;
using FinSight.Contracts.Events;
using FinSight.Domain.Insights;

namespace FinSight.Application.Features.Insights;

/// <summary>
/// Creates financial insights from subscription price changes.
/// </summary>
public sealed class SubscriptionPriceInsightService
{
    private readonly IInsightRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="SubscriptionPriceInsightService"/> class.
    /// </summary>
    public SubscriptionPriceInsightService(
        IInsightRepository repository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Creates an insight describing a subscription price change.
    /// </summary>
    /// <param name="message">The price-change event.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task ProcessAsync(
        SubscriptionPriceChangedEvent message,
        CancellationToken cancellationToken = default)
    {
        var type =
            message.ChangePercentage > 0
                ? InsightType.SubscriptionPriceIncrease
                : InsightType.SubscriptionPriceDecrease;

        var direction =
            message.ChangePercentage > 0
                ? "increased"
                : "decreased";

        var percentage =
            Math.Abs(
                message.ChangePercentage *
                100m);

        var severity =
            percentage >= 20m
                ? InsightSeverity.High
                : percentage >= 10m
                    ? InsightSeverity.Medium
                    : InsightSeverity.Low;

        var insight =
            FinancialInsight.Create(
                message.UserId,
                null,
                message.TransactionId,
                type,
                severity,
                $"{message.MerchantName} price changed",
                $"{message.MerchantName} {direction} from {message.PreviousAmount:F2} to {message.CurrentAmount:F2} {message.Currency}, a {percentage:F1}% change.",
                message.OccurredAt,
                message.OccurredAt.AddDays(30));

        _repository.Add(
            insight);

        await _unitOfWork
            .SaveChangesAsync(
                cancellationToken);

        await _eventPublisher.PublishAsync(
            new InsightGeneratedEvent
            {
                EventId = Guid.NewGuid(),
                UserId = message.UserId,
                InsightId = insight.Id,
                AnomalyId = null,
                Type =
                    insight.Type.ToString(),
                Severity =
                    insight.Severity.ToString(),
                Title =
                    insight.Title,
                OccurredAt =
                    insight.CreatedAt
            },
            "insight.generated",
            cancellationToken);
    }
}
