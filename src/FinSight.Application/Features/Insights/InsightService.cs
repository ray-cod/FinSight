using FinSight.Application.Abstractions.Intelligence;
using FinSight.Application.Abstractions.Messaging;
using FinSight.Application.Abstractions.Persistence;
using FinSight.Contracts.Events;
using FinSight.Domain.Anomalies;
using FinSight.Domain.Insights;
using Microsoft.Extensions.Logging;

namespace FinSight.Application.Features.Insights;

/// <summary>
/// Coordinates financial insight creation and lifecycle operations.
/// </summary>
public sealed partial class InsightService
{
    private readonly IInsightRepository _repository;
    private readonly IAnomalyRepository _anomalyRepository;
    private readonly IInsightGenerator _generator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<InsightService> _logger;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="InsightService"/> class.
    /// </summary>
    public InsightService(
        IInsightRepository repository,
        IAnomalyRepository anomalyRepository,
        IInsightGenerator generator,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        ILogger<InsightService> logger)
    {
        _repository = repository;
        _anomalyRepository =
            anomalyRepository;
        _generator =
            generator;
        _unitOfWork =
            unitOfWork;
        _eventPublisher =
            eventPublisher;
        _logger =
            logger;
    }

    /// <summary>
    /// Generates and persists an insight for an anomaly.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="anomalyId">The anomaly identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generated insight, if created.</returns>
    public async Task<FinancialInsight?>
        GenerateFromAnomalyAsync(
            Guid userId,
            Guid anomalyId,
            CancellationToken cancellationToken = default)
    {
        var anomaly =
            await _anomalyRepository
                .GetByIdAsync(
                    userId,
                    anomalyId,
                    cancellationToken);

        if (anomaly is null)
        {
            return null;
        }

        if (await _repository
            .ExistsForAnomalyAsync(
                anomalyId,
                cancellationToken))
        {
            return null;
        }

        var insight =
            _generator.Generate(
                anomaly);

        _repository.Add(
            insight);

        await _unitOfWork
            .SaveChangesAsync(
                cancellationToken);

        await _eventPublisher.PublishAsync(
            new InsightGeneratedEvent
            {
                EventId = Guid.NewGuid(),
                UserId = userId,
                InsightId = insight.Id,
                AnomalyId =
                    anomalyId,
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

        LogInsightGenerated(
            insight.Id,
            anomalyId);

        return insight;
    }

    /// <summary>
    /// Gets insights for a user.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="includeDismissed">
    /// Whether dismissed insights should be included.
    /// </param>
    /// <param name="limit">Maximum number of results.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's insights.</returns>
    public Task<IReadOnlyList<FinancialInsight>>
        GetForUserAsync(
            Guid userId,
            bool includeDismissed = false,
            int limit = 100,
            CancellationToken cancellationToken = default)
    {
        return _repository.GetByUserIdAsync(
            userId,
            includeDismissed,
            limit,
            cancellationToken);
    }

    /// <summary>
    /// Gets an insight owned by a user.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="insightId">The insight identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The requested insight.</returns>
    public async Task<FinancialInsight> GetAsync(
        Guid userId,
        Guid insightId,
        CancellationToken cancellationToken = default)
    {
        var insight =
            await _repository.GetByIdAsync(
                userId,
                insightId,
                cancellationToken);

        if (insight is null)
        {
            throw new KeyNotFoundException(
                "Financial insight was not found.");
        }

        return insight;
    }

    /// <summary>
    /// Marks an insight as seen.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="insightId">The insight identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task MarkSeenAsync(
        Guid userId,
        Guid insightId,
        CancellationToken cancellationToken = default)
    {
        var insight =
            await GetAsync(
                userId,
                insightId,
                cancellationToken);

        insight.MarkSeen();

        await _unitOfWork
            .SaveChangesAsync(
                cancellationToken);
    }

    /// <summary>
    /// Dismisses an insight.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="insightId">The insight identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task DismissAsync(
        Guid userId,
        Guid insightId,
        CancellationToken cancellationToken = default)
    {
        var insight =
            await GetAsync(
                userId,
                insightId,
                cancellationToken);

        insight.Dismiss();

        await _unitOfWork
            .SaveChangesAsync(
                cancellationToken);
    }

    /// <summary>
    /// Expires insights whose expiration dates have passed.
    /// </summary>
    /// <param name="asOf">Evaluation timestamp.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task ExpireAsync(
        DateTimeOffset asOf,
        CancellationToken cancellationToken = default)
    {
        var expired =
            await _repository
                .GetExpiredAsync(
                    asOf,
                    cancellationToken);

        foreach (var insight in expired)
        {
            insight.Expire();
        }

        if (expired.Count > 0)
        {
            await _unitOfWork
                .SaveChangesAsync(
                    cancellationToken);
        }
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Financial insight {InsightId} generated from anomaly {AnomalyId}.")]
    private partial void LogInsightGenerated(
        Guid insightId,
        Guid anomalyId);
}
