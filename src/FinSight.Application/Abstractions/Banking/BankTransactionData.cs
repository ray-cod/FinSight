using FinSight.Domain.Transactions;

namespace FinSight.Application.Abstractions.Banking;

/// <summary>
/// Represents a raw transaction returned by a banking provider.
/// </summary>
public sealed record BankTransactionData(
    string ExternalAccountId,
    string ExternalTransactionId,
    string Description,
    decimal Amount,
    string Currency,
    DateTimeOffset TransactionDate,
    TransactionType Type,
    TransactionStatus Status);
