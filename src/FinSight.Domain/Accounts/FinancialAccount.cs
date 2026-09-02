using FinSight.Domain.Common;

namespace FinSight.Domain.Accounts;

/// <summary>
/// Represents a financial account owned by a FinSight user.
/// </summary>
public sealed class FinancialAccount
    : AggregateRoot<AccountId>
{
    private FinancialAccount()
    {
    }

    private FinancialAccount(
        AccountId id,
        Guid userId,
        Guid connectionId,
        Guid institutionId,
        string externalAccountId,
        string name,
        AccountType type,
        string currency,
        decimal currentBalance,
        decimal availableBalance)
        : base(id)
    {
        UserId = userId;
        ConnectionId = connectionId;
        InstitutionId = institutionId;
        ExternalAccountId = externalAccountId.Trim();
        Name = name.Trim();
        Type = type;
        Currency = currency.Trim().ToUpperInvariant();
        CurrentBalance = currentBalance;
        AvailableBalance = availableBalance;
        Status = AccountStatus.Active;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the owning user identifier.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the account connection identifier.
    /// </summary>
    public Guid ConnectionId { get; private set; }

    /// <summary>
    /// Gets the institution identifier.
    /// </summary>
    public Guid InstitutionId { get; private set; }

    /// <summary>
    /// Gets the provider-specific account identifier.
    /// </summary>
    public string ExternalAccountId { get; private set; } = null!;

    /// <summary>
    /// Gets the display name of the account.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the account type.
    /// </summary>
    public AccountType Type { get; private set; }

    /// <summary>
    /// Gets the ISO currency code.
    /// </summary>
    public string Currency { get; private set; } = null!;

    /// <summary>
    /// Gets the current account balance.
    /// </summary>
    public decimal CurrentBalance { get; private set; }

    /// <summary>
    /// Gets the currently available balance.
    /// </summary>
    public decimal AvailableBalance { get; private set; }

    /// <summary>
    /// Gets the account status.
    /// </summary>
    public AccountStatus Status { get; private set; }

    /// <summary>
    /// Gets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Gets the last modification timestamp.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; private set; }

    /// <summary>
    /// Creates a financial account from provider data.
    /// </summary>
    /// <param name="userId">The owning user identifier.</param>
    /// <param name="connectionId">The account connection.</param>
    /// <param name="institutionId">The financial institution.</param>
    /// <param name="externalAccountId">The provider account identifier.</param>
    /// <param name="name">The account display name.</param>
    /// <param name="type">The account type.</param>
    /// <param name="currency">The account currency.</param>
    /// <param name="currentBalance">The current balance.</param>
    /// <param name="availableBalance">The available balance.</param>
    /// <returns>A new financial account.</returns>
    public static FinancialAccount Create(
        Guid userId,
        Guid connectionId,
        Guid institutionId,
        string externalAccountId,
        string name,
        AccountType type,
        string currency,
        decimal currentBalance,
        decimal availableBalance)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User identifier cannot be empty.",
                nameof(userId));
        }

        if (connectionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Connection identifier cannot be empty.",
                nameof(connectionId));
        }

        if (institutionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Institution identifier cannot be empty.",
                nameof(institutionId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            externalAccountId);

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        if (currency.Trim().Length != 3)
        {
            throw new ArgumentException(
                "Currency must be a three-letter ISO code.",
                nameof(currency));
        }

        return new FinancialAccount(
            AccountId.New(),
            userId,
            connectionId,
            institutionId,
            externalAccountId,
            name,
            type,
            currency,
            currentBalance,
            availableBalance);
    }

    /// <summary>
    /// Updates the balances received from the provider.
    /// </summary>
    /// <param name="currentBalance">The current balance.</param>
    /// <param name="availableBalance">The available balance.</param>
    public void UpdateBalances(
        decimal currentBalance,
        decimal availableBalance)
    {
        CurrentBalance = currentBalance;
        AvailableBalance = availableBalance;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marks the account as closed.
    /// </summary>
    public void Close()
    {
        Status = AccountStatus.Closed;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marks the account as unavailable.
    /// </summary>
    public void MarkUnavailable()
    {
        Status = AccountStatus.Unavailable;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
