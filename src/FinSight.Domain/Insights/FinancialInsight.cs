using FinSight.Domain.Common;

namespace FinSight.Domain.Insights;

/// <summary>
/// Represents an explanation derived from a financial event or trend.
/// </summary>
public sealed class FinancialInsight
    : AggregateRoot<Guid>
{
    private FinancialInsight()
    {
    }

    private FinancialInsight(
        Guid id,
        Guid userId,
        Guid? anomalyId,
        Guid? transactionId,
        InsightType type,
        InsightSeverity severity,
        string title,
        string message,
        DateTimeOffset occurredAt,
        DateTimeOffset? expiresAt)
        : base(id)
    {
        UserId = userId;
        AnomalyId = anomalyId;
        TransactionId = transactionId;
        Type = type;
        Severity = severity;
        Title = title;
        Message = message;
        OccurredAt = occurredAt;
        ExpiresAt = expiresAt;
        Status = InsightStatus.Active;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the owning user identifier.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the source anomaly identifier, when applicable.
    /// </summary>
    public Guid? AnomalyId { get; private set; }

    /// <summary>
    /// Gets the source transaction identifier, when applicable.
    /// </summary>
    public Guid? TransactionId { get; private set; }

    /// <summary>
    /// Gets the insight type.
    /// </summary>
    public InsightType Type { get; private set; }

    /// <summary>
    /// Gets the insight severity.
    /// </summary>
    public InsightSeverity Severity { get; private set; }

    /// <summary>
    /// Gets the insight title.
    /// </summary>
    public string Title { get; private set; } = null!;

    /// <summary>
    /// Gets the user-readable insight message.
    /// </summary>
    public string Message { get; private set; } = null!;

    /// <summary>
    /// Gets when the event represented by the insight occurred.
    /// </summary>
    public DateTimeOffset OccurredAt { get; private set; }

    /// <summary>
    /// Gets when the insight expires.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; private set; }

    /// <summary>
    /// Gets the insight lifecycle status.
    /// </summary>
    public InsightStatus Status { get; private set; }

    /// <summary>
    /// Gets when the insight was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Marks the insight as seen.
    /// </summary>
    public void MarkSeen()
    {
        if (Status == InsightStatus.Active)
        {
            Status = InsightStatus.Seen;
        }
    }

    /// <summary>
    /// Dismisses the insight.
    /// </summary>
    public void Dismiss()
    {
        Status = InsightStatus.Dismissed;
    }

    /// <summary>
    /// Marks the insight as expired.
    /// </summary>
    public void Expire()
    {
        if (Status != InsightStatus.Dismissed)
        {
            Status = InsightStatus.Expired;
        }
    }

    /// <summary>
    /// Creates a financial insight.
    /// </summary>
    /// <param name="userId">The owning user identifier.</param>
    /// <param name="anomalyId">The source anomaly, if any.</param>
    /// <param name="transactionId">The source transaction, if any.</param>
    /// <param name="type">The insight type.</param>
    /// <param name="severity">The insight severity.</param>
    /// <param name="title">The insight title.</param>
    /// <param name="message">The insight message.</param>
    /// <param name="occurredAt">When the event occurred.</param>
    /// <param name="expiresAt">When the insight should expire.</param>
    /// <returns>A new financial insight.</returns>
    public static FinancialInsight Create(
        Guid userId,
        Guid? anomalyId,
        Guid? transactionId,
        InsightType type,
        InsightSeverity severity,
        string title,
        string message,
        DateTimeOffset occurredAt,
        DateTimeOffset? expiresAt)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User identifier cannot be empty.",
                nameof(userId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            title);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            message);

        return new FinancialInsight(
            Guid.NewGuid(),
            userId,
            anomalyId,
            transactionId,
            type,
            severity,
            title.Trim(),
            message.Trim(),
            occurredAt,
            expiresAt);
    }
}
