namespace FinSight.Infrastructure.Banking.MockBank.Models;

/// <summary>
/// Represents an account exposed by the mock bank.
/// </summary>
public sealed record MockBankAccount(
    string ExternalAccountId,
    string Name,
    string Type,
    string Currency,
    decimal CurrentBalance,
    decimal AvailableBalance);
