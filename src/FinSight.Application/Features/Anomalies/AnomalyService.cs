using FinSight.Application.Abstractions.Messaging;
using FinSight.Application.Abstractions.Persistence;
using FinSight.Contracts.Events;
using FinSight.Domain.Anomalies;
using Microsoft.Extensions.Logging;

namespace FinSight.Application.Features.Anomalies;

/// <summary>
/// Provides user-facing anomaly lifecycle operations.
/// </summary>
public sealed partial class AnomalyService
{
    private readonly IAnomalyRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AnomalyService> _logger;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="AnomalyService"/> class.
    /// </summary>
    public AnomalyService(
        IAnomalyRepository repository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        ILogger<AnomalyService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    /// <summary>
    /// Gets anomalies belonging to a user.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="includeResolved">
    /// Whether resolved anomalies should be included.
    /// </param>
    /// <param name="limit">Maximum number of results.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's anomalies.</returns>
    public Task<IReadOnlyList<Anomaly>>
        GetForUserAsync(
            Guid userId,
            bool includeResolved = false,
            int limit = 100,
            CancellationToken cancellationToken = default)
    {
        return _repository.GetByUserIdAsync(
            userId,
            includeResolved,
            limit,
            cancellationToken);
    }

    /// <summary>
    /// Gets an anomaly within the user's ownership scope.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="anomalyId">The anomaly identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The requested anomaly.</returns>
    public async Task<Anomaly> GetAsync(
        Guid userId,
        Guid anomalyId,
        CancellationToken cancellationToken = default)
    {
        var anomaly =
            await _repository.GetByIdAsync(
                userId,
                anomalyId,
                cancellationToken);

        if (anomaly is null)
        {
            throw new KeyNotFoundException(
                "Anomaly was not found.");
        }

        return anomaly;
    }

    /// <summary>
    /// Resolves an anomaly.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="anomalyId">The anomaly identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task ResolveAsync(
        Guid userId,
        Guid anomalyId,
        CancellationToken cancellationToken = default)
    {
        return UpdateStatusAsync(
            userId,
            anomalyId,
            false,
            cancellationToken);
    }

    /// <summary>
    /// Dismisses an anomaly.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="anomalyId">The anomaly identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task DismissAsync(
        Guid userId,
        Guid anomalyId,
        CancellationToken cancellationToken = default)
    {
        return UpdateStatusAsync(
            userId,
            anomalyId,
            true,
            cancellationToken);
    }

    private async Task UpdateStatusAsync(
        Guid userId,
        Guid anomalyId,
        bool dismiss,
        CancellationToken cancellationToken)
    {
        var anomaly =
            await GetAsync(
                userId,
                anomalyId,
                cancellationToken);

        if (dismiss)
        {
            anomaly.Dismiss();
        }
        else
        {
            anomaly.Resolve();
        }

        await _unitOfWork
            .SaveChangesAsync(
                cancellationToken);

        await _eventPublisher.PublishAsync(
            new AnomalyResolvedEvent
            {
                EventId = Guid.NewGuid(),
                UserId = userId,
                AnomalyId = anomalyId,
                Status =
                    anomaly.Status.ToString(),
                OccurredAt =
                    DateTimeOffset.UtcNow
            },
            "anomaly.resolved",
            cancellationToken);

        LogAnomalyStatusChanged(
            anomalyId,
            anomaly.Status);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Anomaly {AnomalyId} changed to {Status}.")]
    private partial void LogAnomalyStatusChanged(
        Guid anomalyId,
        AnomalyStatus status);
}
