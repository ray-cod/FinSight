using FinSight.Api.Extensions;
using FinSight.Application.Features.Anomalies;
using FinSight.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinSight.Api.Controllers;

/// <summary>
/// Provides financial anomaly endpoints.
/// </summary>
[ApiController]
[Route("api/v1/anomalies")]
[Authorize(
    Policy =
        AuthorizationPolicies.Authenticated)]
public sealed class AnomaliesController(
    AnomalyService anomalyService)
    : ControllerBase
{
    /// <summary>
    /// Gets anomalies belonging to the authenticated user.
    /// </summary>
    /// <param name="includeResolved">
    /// Whether resolved anomalies should be returned.
    /// </param>
    /// <param name="limit">
    /// Maximum number of anomalies.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>The user's anomalies.</returns>
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] bool includeResolved = false,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var anomalies =
            await anomalyService
                .GetForUserAsync(
                    User.GetRequiredUserId(),
                    includeResolved,
                    limit,
                    cancellationToken);

        return Ok(
            anomalies.Select(
                Map));
    }

    /// <summary>
    /// Gets a specific anomaly owned by the authenticated user.
    /// </summary>
    /// <param name="anomalyId">The anomaly identifier.</param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>The requested anomaly.</returns>
    [HttpGet("{anomalyId:guid}")]
    public async Task<IActionResult> GetById(
        Guid anomalyId,
        CancellationToken cancellationToken)
    {
        var anomaly =
            await anomalyService.GetAsync(
                User.GetRequiredUserId(),
                anomalyId,
                cancellationToken);

        return Ok(Map(anomaly));
    }

    /// <summary>
    /// Resolves an anomaly.
    /// </summary>
    /// <param name="anomalyId">The anomaly identifier.</param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    [HttpPost("{anomalyId:guid}/resolve")]
    public async Task<IActionResult> Resolve(
        Guid anomalyId,
        CancellationToken cancellationToken)
    {
        await anomalyService.ResolveAsync(
            User.GetRequiredUserId(),
            anomalyId,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Dismisses an anomaly.
    /// </summary>
    /// <param name="anomalyId">The anomaly identifier.</param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    [HttpPost("{anomalyId:guid}/dismiss")]
    public async Task<IActionResult> Dismiss(
        Guid anomalyId,
        CancellationToken cancellationToken)
    {
        await anomalyService.DismissAsync(
            User.GetRequiredUserId(),
            anomalyId,
            cancellationToken);

        return NoContent();
    }

    private static object Map(
        Domain.Anomalies.Anomaly anomaly)
    {
        return new
        {
            id = anomaly.Id,
            transactionId =
                anomaly.TransactionId,
            accountId =
                anomaly.AccountId,
            type =
                anomaly.Type,
            severity =
                anomaly.Severity,
            score =
                anomaly.Score,
            confidence =
                anomaly.Confidence,
            title =
                anomaly.Title,
            description =
                anomaly.Description,
            evidence =
                anomaly.Evidence,
            detectedAt =
                anomaly.DetectedAt,
            status =
                anomaly.Status,
            resolvedAt =
                anomaly.ResolvedAt
        };
    }
}
