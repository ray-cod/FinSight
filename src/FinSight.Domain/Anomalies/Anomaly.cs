using FinSight.Domain.Common;

namespace FinSight.Domain.Anomalies;

/// <summary>
/// Represents an unusual financial event detected by FinSight.
/// </summary>
public sealed class Anomaly
    : AggregateRoot<Guid>
{
    private Anomaly()
    {
    }

    private Anomaly(
        Guid id,
        Guid userId,
        Guid transactionId,
        Guid accountId,
        AnomalyType type,
        AnomalySeverity severity,
        decimal score,
        decimal confidence,
        string title,
        string description,
        string evidence,
        DateTimeOffset detectedAt)
        : base(id)
    {
        UserId = userId;
        TransactionId = transactionId;
        AccountId = accountId;
        Type = type;
        Severity = severity;
        Score = score;
        Confidence = confidence;
        Title = title;
        Description = description;
        Evidence = evidence;
        DetectedAt = detectedAt;
        Status = AnomalyStatus.Open;
    }

    /// <summary>
    /// Gets the user who owns this anomaly.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the transaction that triggered the anomaly.
    /// </summary>
    public Guid TransactionId { get; private set; }

    /// <summary>
    /// Gets the financial account associated with the transaction.
    /// </summary>
    public Guid AccountId { get; private set; }

    /// <summary>
    /// Gets the anomaly type.
    /// </summary>
    public AnomalyType Type { get; private set; }

    /// <summary>
    /// Gets the anomaly severity.
    /// </summary>
    public AnomalySeverity Severity { get; private set; }

    /// <summary>
    /// Gets the anomaly score.
    /// </summary>
    public decimal Score { get; private set; }

    /// <summary>
    /// Gets the detection confidence.
    /// </summary>
    public decimal Confidence { get; private set; }

    /// <summary>
    /// Gets the anomaly title.
    /// </summary>
    public string Title { get; private set; } = null!;

    /// <summary>
    /// Gets the user-readable anomaly description.
    /// </summary>
    public string Description { get; private set; } = null!;

    /// <summary>
    /// Gets evidence supporting the anomaly classification.
    /// </summary>
    public string Evidence { get; private set; } = null!;

    /// <summary>
    /// Gets when the anomaly was detected.
    /// </summary>
    public DateTimeOffset DetectedAt { get; private set; }

    /// <summary>
    /// Gets the current anomaly status.
    /// </summary>
    public AnomalyStatus Status { get; private set; }

    /// <summary>
    /// Gets when the anomaly was resolved.
    /// </summary>
    public DateTimeOffset? ResolvedAt { get; private set; }

    /// <summary>
    /// Creates an anomaly.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="transactionId">The triggering transaction.</param>
    /// <param name="accountId">The financial account.</param>
    /// <param name="type">The anomaly type.</param>
    /// <param name="severity">The severity.</param>
    /// <param name="score">The anomaly score.</param>
    /// <param name="confidence">Detection confidence.</param>
    /// <param name="title">The anomaly title.</param>
    /// <param name="description">The anomaly description.</param>
    /// <param name="evidence">Supporting evidence.</param>
    /// <returns>A new anomaly.</returns>
    public static Anomaly Create(
        Guid userId,
        Guid transactionId,
        Guid accountId,
        AnomalyType type,
        AnomalySeverity severity,
        decimal score,
        decimal confidence,
        string title,
        string description,
        string evidence)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User identifier cannot be empty.",
                nameof(userId));
        }

        if (transactionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Transaction identifier cannot be empty.",
                nameof(transactionId));
        }

        if (accountId == Guid.Empty)
        {
            throw new ArgumentException(
                "Account identifier cannot be empty.",
                nameof(accountId));
        }

        ValidateProbability(
            score,
            nameof(score));

        ValidateProbability(
            confidence,
            nameof(confidence));

        ArgumentException.ThrowIfNullOrWhiteSpace(
            title);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            description);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            evidence);

        return new Anomaly(
            Guid.NewGuid(),
            userId,
            transactionId,
            accountId,
            type,
            severity,
            score,
            confidence,
            title.Trim(),
            description.Trim(),
            evidence.Trim(),
            DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Resolves the anomaly.
    /// </summary>
    public void Resolve()
    {
        Status = AnomalyStatus.Resolved;
        ResolvedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Dismisses the anomaly.
    /// </summary>
    public void Dismiss()
    {
        Status = AnomalyStatus.Dismissed;
        ResolvedAt = DateTimeOffset.UtcNow;
    }

    private static void ValidateProbability(
        decimal value,
        string parameterName)
    {
        if (value < 0m || value > 1m)
        {
            throw new ArgumentOutOfRangeException(
                parameterName);
        }
    }
}
