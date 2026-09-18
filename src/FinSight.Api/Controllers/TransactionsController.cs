using FinSight.Api.Extensions;
using FinSight.Application.Abstractions.Persistence;
using FinSight.Application.Features.Transactions;
using FinSight.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinSight.Api.Controllers;

/// <summary>
/// Provides user-owned financial transaction endpoints.
/// </summary>
[ApiController]
[Route("api/v1/transactions")]
[Authorize(Policy = AuthorizationPolicies.Authenticated)]
public sealed class TransactionsController(
    TransactionQueryService transactionQueryService,
    UpdateTransactionClassificationService classificationService)
    : ControllerBase
{
    /// <summary>
    /// Gets a filtered and paginated transaction list for the authenticated user.
    /// </summary>
    /// <param name="request">The transaction filters and pagination settings.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The requested transaction page.</returns>
    [HttpGet]
    public async Task<ActionResult<TransactionQueryResult>> GetTransactions(
        [FromQuery] TransactionListRequest request,
        CancellationToken cancellationToken)
    {
        if (request.MinAmount.HasValue &&
            request.MinAmount.Value < 0)
        {
            return BadRequest("Minimum amount cannot be negative.");
        }

        if (request.MaxAmount.HasValue &&
            request.MaxAmount.Value < 0)
        {
            return BadRequest("Maximum amount cannot be negative.");
        }

        if (request.MinAmount.HasValue &&
            request.MaxAmount.HasValue &&
            request.MinAmount.Value > request.MaxAmount.Value)
        {
            return BadRequest("Minimum amount cannot exceed maximum amount.");
        }

        if (request.From.HasValue &&
            request.To.HasValue &&
            request.From.Value >= request.To.Value)
        {
            return BadRequest("The start date must be earlier than the end date.");
        }

        var result = await transactionQueryService.GetPageAsync(
            User.GetRequiredUserId(),
            request,
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Gets detailed transaction information for an owned transaction.
    /// </summary>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The transaction details.</returns>
    [HttpGet("{transactionId:guid}")]
    public async Task<ActionResult<TransactionDetailResponse>> GetTransaction(
        Guid transactionId,
        CancellationToken cancellationToken)
    {
        var result = await transactionQueryService.GetDetailAsync(
            User.GetRequiredUserId(),
            transactionId,
            cancellationToken);

        return result is null
            ? NotFound()
            : Ok(result);
    }

    /// <summary>
    /// Corrects the classification of a user-owned transaction.
    /// </summary>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="request">The corrected category selection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPut("{transactionId:guid}/classification")]
    public async Task<IActionResult> CorrectClassification(
        Guid transactionId,
        UpdateTransactionClassificationRequest request,
        CancellationToken cancellationToken)
    {
        await classificationService.CorrectAsync(
            User.GetRequiredUserId(),
            transactionId,
            request.CategoryId,
            request.SubcategoryId,
            cancellationToken);

        return NoContent();
    }
}
