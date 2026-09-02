namespace FinSight.Application.Abstractions.Banking;

/// <summary>
/// Represents the result of a provider synchronization operation.
/// </summary>
public sealed record BankSyncResult(
    IReadOnlyCollection<BankTransactionData> Transactions,
    IReadOnlyCollection<BankAccountData> Accounts,
    string? NextCursor,
    bool HasMore);
