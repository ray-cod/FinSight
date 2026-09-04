using FinSight.Api.Extensions;
using FinSight.Application.Features.Insights;
using FinSight.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinSight.Api.Controllers;

/// <summary>
/// Provides financial insight endpoints.
/// </summary>
[ApiController]
[Route("api/v1/insights")]
[Authorize(
    Policy =
        AuthorizationPolicies.Authenticated)]
public sealed class InsightsController(
    InsightService insightService)
    : ControllerBase
{
    /// <summary>
    /// Gets current insights for the authenticated user.
    /// </summary>
    /// <param name="includeDismissed">
    /// Whether dismissed insights should be returned.
    /// </param>
    /// <param name="limit">
    /// Maximum number of insights.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>The user's insights.</returns>
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] bool includeDismissed = false,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var insights =
            await insightService
                .GetForUserAsync(
                    User.GetRequiredUserId(),
                    includeDismissed,
                    limit,
                    cancellationToken);

        return Ok(
            insights.Select(
                Map));
    }

    /// <summary>
    /// Gets a specific financial insight.
    /// </summary>
    /// <param name="insightId">
    /// The insight identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>The requested insight.</returns>
    [HttpGet("{insightId:guid}")]
    public async Task<IActionResult> GetById(
        Guid insightId,
        CancellationToken cancellationToken)
    {
        var insight =
            await insightService.GetAsync(
                User.GetRequiredUserId(),
                insightId,
                cancellationToken);

        return Ok(Map(insight));
    }

    /// <summary>
    /// Marks an insight as seen.
    /// </summary>
    /// <param name="insightId">
    /// The insight identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    [HttpPost("{insightId:guid}/seen")]
    public async Task<IActionResult> MarkSeen(
        Guid insightId,
        CancellationToken cancellationToken)
    {
        await insightService.MarkSeenAsync(
            User.GetRequiredUserId(),
            insightId,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Dismisses an insight.
    /// </summary>
    /// <param name="insightId">
    /// The insight identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    [HttpPost("{insightId:guid}/dismiss")]
    public async Task<IActionResult> Dismiss(
        Guid insightId,
        CancellationToken cancellationToken)
    {
        await insightService.DismissAsync(
            User.GetRequiredUserId(),
            insightId,
            cancellationToken);

        return NoContent();
    }

    private static object Map(
        Domain.Insights.FinancialInsight insight)
    {
        return new
        {
            id = insight.Id,
            anomalyId =
                insight.AnomalyId,
            transactionId =
                insight.TransactionId,
            type =
                insight.Type,
            severity =
                insight.Severity,
            title =
                insight.Title,
            message =
                insight.Message,
            occurredAt =
                insight.OccurredAt,
            expiresAt =
                insight.ExpiresAt,
            status =
                insight.Status,
            createdAt =
                insight.CreatedAt
        };
    }
}
