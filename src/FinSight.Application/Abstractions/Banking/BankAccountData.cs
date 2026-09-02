using FinSight.Domain.Accounts;

namespace FinSight.Application.Abstractions.Banking;

/// <summary>
/// Represents account information returned by a banking provider.
/// </summary>
public sealed record BankAccountData(
    string ExternalAccountId,
    string Name,
    AccountType Type,
    string Currency,
    decimal CurrentBalance,
    decimal AvailableBalance);
