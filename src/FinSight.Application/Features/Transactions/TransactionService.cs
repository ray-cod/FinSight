using FinSight.Application.Abstractions.Persistence;
using FinSight.Domain.Transactions;

namespace FinSight.Application.Features.Transactions;

/// <summary>
/// Provides transaction retrieval operations.
/// </summary>
public sealed class TransactionService(
    ITransactionRepository repository)
{
    /// <summary>
    /// Gets transactions belonging to a user's financial account.
    /// </summary>
    /// <param name="userId">The authenticated user.</param>
    /// <param name="accountId">The financial account.</param>
    /// <param name="limit">Maximum number of transactions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's account transactions.</returns>
    public async Task<IReadOnlyList<TransactionResponse>> GetForAccountAsync(
        Guid userId,
        Guid accountId,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 500);

        var transactions =
            await repository.GetForAccountAsync(
                userId,
                accountId,
                limit,
                cancellationToken);

        return transactions
            .Select(
                transaction =>
                    new TransactionResponse(
                        transaction.Id.Value,
                        transaction.ProviderTransactionId,
                        transaction.RawDescription,
                        transaction.Amount,
                        transaction.Currency,
                        transaction.TransactionDate,
                        transaction.Type,
                        transaction.Status))
            .ToArray();
    }
}

/// <summary>
/// Represents a transaction returned by the API.
/// </summary>
public sealed record TransactionResponse(
    Guid Id,
    string ProviderTransactionId,
    string RawDescription,
    decimal Amount,
    string Currency,
    DateTimeOffset TransactionDate,
    TransactionType Type,
    TransactionStatus Status);
