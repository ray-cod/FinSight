namespace FinSight.Infrastructure.Banking.MockBank.Models;

/// <summary>
/// Represents a transaction exposed by the mock bank.
/// </summary>
public sealed record MockBankTransaction(
    string ExternalTransactionId,
    string AccountId,
    string Description,
    decimal Amount,
    string Currency,
    DateTimeOffset TransactionDate,
    string Type,
    string Status);
