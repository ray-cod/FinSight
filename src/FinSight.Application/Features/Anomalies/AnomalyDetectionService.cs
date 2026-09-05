using FinSight.Application.Abstractions.Intelligence;
using FinSight.Application.Abstractions.Messaging;
using FinSight.Application.Abstractions.Observability;
using FinSight.Application.Abstractions.Persistence;
using FinSight.Contracts.Events;
using FinSight.Domain.Anomalies;
using Microsoft.Extensions.Logging;

namespace FinSight.Application.Features.Anomalies;

/// <summary>
/// Coordinates anomaly detection, persistence, and event publication.
/// </summary>
public sealed partial class AnomalyDetectionService
{
    private readonly IAnomalyDetector _detector;
    private readonly IAnomalyRepository _repository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly IFinSightTelemetry _telemetry;
    private readonly ILogger<AnomalyDetectionService> _logger;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="AnomalyDetectionService"/> class.
    /// </summary>
    public AnomalyDetectionService(
        IAnomalyDetector detector,
        IAnomalyRepository repository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        IFinSightTelemetry telemetry,
        ILogger<AnomalyDetectionService> logger)
    {
        _detector = detector;
        _repository = repository;
        _transactionRepository =
            transactionRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _telemetry = telemetry;
        _logger = logger;
    }

    /// <summary>
    /// Evaluates and persists anomalies for a transaction.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="transactionId">
    /// The transaction being evaluated.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created anomalies.</returns>
    public async Task<IReadOnlyList<Anomaly>>
        DetectAndPersistAsync(
            Guid userId,
            Guid transactionId,
            CancellationToken cancellationToken = default)
    {
        var results =
            await _detector.DetectAsync(
                userId,
                transactionId,
                cancellationToken);

        var transaction =
            await _transactionRepository
                .GetByIdAsync(
                    userId,
                    transactionId,
                    cancellationToken);

        if (transaction is null)
        {
            return [];
        }

        var created =
            new List<Anomaly>();

        foreach (var result in results)
        {
            if (await _repository
                .ExistsForTransactionAsync(
                    transactionId,
                    result.Type,
                    cancellationToken))
            {
                continue;
            }

            var anomaly =
                Anomaly.Create(
                    userId,
                    transactionId,
                    transaction.AccountId,
                    result.Type,
                    result.Severity,
                    result.Score,
                    result.Confidence,
                    result.Title,
                    result.Description,
                    result.Evidence);

            _repository.Add(
                anomaly);

            await _eventPublisher.PublishAsync(
                new AnomalyDetectedEvent
                {
                    EventId = Guid.NewGuid(),
                    UserId = userId,
                    AnomalyId = anomaly.Id,
                    TransactionId = transactionId,
                    Type =
                        anomaly.Type.ToString(),
                    Severity =
                        anomaly.Severity.ToString(),
                    Score =
                        anomaly.Score,
                    Confidence =
                        anomaly.Confidence,
                    Title =
                        anomaly.Title,
                    Description =
                        anomaly.Description,
                    OccurredAt =
                        anomaly.DetectedAt
                },
                "anomaly.detected",
                cancellationToken);

            await _unitOfWork
                .SaveChangesAsync(
                    cancellationToken);

            created.Add(
                anomaly);
        }

        if (created.Count > 0)
        {
            LogAnomaliesDetected(
                created.Count,
                transactionId);

            _telemetry.IncrementAnomaliesDetected(created.Count);
        }

        return created;
    }

    private static Task<
        Domain.Transactions.Transaction?>
        GetTransactionAsync(
            Guid userId,
            Guid transactionId,
            CancellationToken cancellationToken)
    {
        throw new NotSupportedException(
            "Register ITransactionRepository in this service " +
            "and use GetByIdAsync to retrieve the transaction.");
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Detected {Count} anomalies for transaction {TransactionId}.")]
    private partial void LogAnomaliesDetected(
        int count,
        Guid transactionId);
}
