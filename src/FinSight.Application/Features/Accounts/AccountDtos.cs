using FinSight.Domain.Accounts;

namespace FinSight.Application.Features.Accounts;

/// <summary>
/// Represents a connected financial account.
/// </summary>
public sealed record AccountResponse(
    Guid Id,
    Guid InstitutionId,
    string Name,
    AccountType Type,
    string Currency,
    decimal CurrentBalance,
    decimal AvailableBalance,
    AccountStatus Status);

/// <summary>
/// Represents a connected institution.
/// </summary>
public sealed record InstitutionResponse(
    Guid Id,
    string ProviderCode,
    string Name,
    bool IsActive);

/// <summary>
/// Represents a request to connect a financial institution.
/// </summary>
public sealed record ConnectAccountRequest(
    string InstitutionCode);
