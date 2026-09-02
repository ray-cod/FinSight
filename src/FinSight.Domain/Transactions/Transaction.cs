using FinSight.Domain.Common;

namespace FinSight.Domain.Transactions;

/// <summary>
/// Represents a normalized financial transaction imported from a provider.
/// </summary>
public sealed class Transaction
    : AggregateRoot<TransactionId>
{
    private Transaction()
    {
    }

    private Transaction(
        TransactionId id,
        Guid userId,
        Guid accountId,
        Guid institutionId,
        string providerTransactionId,
        string rawDescription,
        decimal amount,
        string currency,
        DateTimeOffset transactionDate,
        TransactionType type,
        TransactionStatus status,
        string fingerprint)
        : base(id)
    {
        UserId = userId;
        AccountId = accountId;
        InstitutionId = institutionId;
        ProviderTransactionId =
            providerTransactionId.Trim();
        RawDescription =
            rawDescription.Trim();
        Amount = amount;
        Currency =
            currency.Trim().ToUpperInvariant();
        TransactionDate = transactionDate;
        Type = type;
        Status = status;
        Fingerprint = fingerprint;
        ImportedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the owning user identifier.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the financial account identifier.
    /// </summary>
    public Guid AccountId { get; private set; }

    /// <summary>
    /// Gets the institution identifier.
    /// </summary>
    public Guid InstitutionId { get; private set; }

    /// <summary>
    /// Gets the provider transaction identifier.
    /// </summary>
    public string ProviderTransactionId { get; private set; } = null!;

    /// <summary>
    /// Gets the original transaction description received from the bank.
    /// </summary>
    public string RawDescription { get; private set; } = null!;

    /// <summary>
    /// Gets the signed transaction amount.
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Gets the ISO currency code.
    /// </summary>
    public string Currency { get; private set; } = null!;

    /// <summary>
    /// Gets the date on which the transaction occurred.
    /// </summary>
    public DateTimeOffset TransactionDate { get; private set; }

    /// <summary>
    /// Gets the transaction type.
    /// </summary>
    public TransactionType Type { get; private set; }

    /// <summary>
    /// Gets the transaction processing status.
    /// </summary>
    public TransactionStatus Status { get; private set; }

    /// <summary>
    /// Gets the fingerprint used to detect duplicate imports.
    /// </summary>
    public string Fingerprint { get; private set; } = null!;

    /// <summary>
    /// Gets the timestamp at which FinSight imported the transaction.
    /// </summary>
    public DateTimeOffset ImportedAt { get; private set; }

    /// <summary>
    /// Creates an imported transaction.
    /// </summary>
    /// <param name="userId">The owning user.</param>
    /// <param name="accountId">The financial account.</param>
    /// <param name="institutionId">The financial institution.</param>
    /// <param name="providerTransactionId">Provider transaction identifier.</param>
    /// <param name="rawDescription">Raw provider description.</param>
    /// <param name="amount">Signed transaction amount.</param>
    /// <param name="currency">ISO currency code.</param>
    /// <param name="transactionDate">Transaction timestamp.</param>
    /// <param name="type">Transaction type.</param>
    /// <param name="status">Transaction status.</param>
    /// <param name="fingerprint">Duplicate-detection fingerprint.</param>
    /// <returns>A new imported transaction.</returns>
    public static Transaction CreateImported(
        Guid userId,
        Guid accountId,
        Guid institutionId,
        string providerTransactionId,
        string rawDescription,
        decimal amount,
        string currency,
        DateTimeOffset transactionDate,
        TransactionType type,
        TransactionStatus status,
        string fingerprint)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User identifier cannot be empty.",
                nameof(userId));
        }

        if (accountId == Guid.Empty)
        {
            throw new ArgumentException(
                "Account identifier cannot be empty.",
                nameof(accountId));
        }

        if (institutionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Institution identifier cannot be empty.",
                nameof(institutionId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            providerTransactionId);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            rawDescription);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            currency);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            fingerprint);

        if (currency.Trim().Length != 3)
        {
            throw new ArgumentException(
                "Currency must be a three-letter ISO code.",
                nameof(currency));
        }

        return new Transaction(
            TransactionId.New(),
            userId,
            accountId,
            institutionId,
            providerTransactionId,
            rawDescription,
            amount,
            currency,
            transactionDate,
            type,
            status,
            fingerprint);
    }
}
