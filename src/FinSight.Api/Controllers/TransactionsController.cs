using FinSight.Api.Extensions;
using FinSight.Application.Abstractions.Persistence;
using FinSight.Application.Features.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinSight.Api.Controllers;

/// <summary>
/// Provides financial transaction endpoints.
/// </summary>
[ApiController]
[Route("api/v1/transactions")]
[Authorize]
public sealed class TransactionsController(
    ITransactionRepository transactionRepository,
    UpdateTransactionClassificationService
        classificationService)
    : ControllerBase
{
    /// <summary>
    /// Gets a transaction belonging to the authenticated user.
    /// </summary>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The transaction.</returns>
    [HttpGet("{transactionId:guid}")]
    public async Task<IActionResult> Get(
        Guid transactionId,
        CancellationToken cancellationToken)
    {
        var transaction =
            await transactionRepository.GetByIdAsync(
                User.GetRequiredUserId(),
                transactionId,
                cancellationToken);

        if (transaction is null)
        {
            return NotFound();
        }

        return Ok(
            new
            {
                id = transaction.Id.Value,
                accountId = transaction.AccountId,
                rawDescription =
                    transaction.RawDescription,
                normalizedDescription =
                    transaction.NormalizedDescription,
                merchantId =
                    transaction.MerchantId,
                categoryId =
                    transaction.CategoryId,
                subcategoryId =
                    transaction.SubcategoryId,
                amount =
                    transaction.Amount,
                currency =
                    transaction.Currency,
                transactionDate =
                    transaction.TransactionDate,
                type =
                    transaction.Type,
                status =
                    transaction.Status,
                classificationStatus =
                    transaction.ClassificationStatus,
                classificationSource =
                    transaction.ClassificationSource,
                classificationConfidence =
                    transaction.ClassificationConfidence,
                classifiedAt =
                    transaction.ClassifiedAt
            });
    }

    /// <summary>
    /// Corrects the classification of a transaction.
    /// </summary>
    /// <param name="transactionId">
    /// The transaction identifier.
    /// </param>
    /// <param name="request">
    /// The corrected classification.
    /// </param>
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
