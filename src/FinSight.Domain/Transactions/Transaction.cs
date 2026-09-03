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

        NormalizedDescription =
            RawDescription;

        Amount = amount;
        Currency =
            currency.Trim().ToUpperInvariant();

        TransactionDate = transactionDate;
        Type = type;
        Status = status;
        Fingerprint = fingerprint;
        ClassificationStatus =
            ClassificationStatus.Pending;
        ClassificationSource =
            ClassificationSource.None;
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
    /// Gets the original transaction description received from the provider.
    /// </summary>
    public string RawDescription { get; private set; } = null!;

    /// <summary>
    /// Gets the normalized transaction description.
    /// </summary>
    public string NormalizedDescription { get; private set; } = null!;

    /// <summary>
    /// Gets the normalized merchant identifier.
    /// </summary>
    public Guid? MerchantId { get; private set; }

    /// <summary>
    /// Gets the assigned category identifier.
    /// </summary>
    public Guid? CategoryId { get; private set; }

    /// <summary>
    /// Gets the assigned subcategory identifier.
    /// </summary>
    public Guid? SubcategoryId { get; private set; }

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
    /// Gets the duplicate-detection fingerprint.
    /// </summary>
    public string Fingerprint { get; private set; } = null!;

    /// <summary>
    /// Gets the transaction import timestamp.
    /// </summary>
    public DateTimeOffset ImportedAt { get; private set; }

    /// <summary>
    /// Gets the classification status.
    /// </summary>
    public ClassificationStatus ClassificationStatus { get; private set; }

    /// <summary>
    /// Gets the classification source.
    /// </summary>
    public ClassificationSource ClassificationSource { get; private set; }

    /// <summary>
    /// Gets the classifier's confidence score.
    /// </summary>
    public decimal? ClassificationConfidence { get; private set; }

    /// <summary>
    /// Gets the classification timestamp.
    /// </summary>
    public DateTimeOffset? ClassifiedAt { get; private set; }

    /// <summary>
    /// Gets the timestamp at which the user corrected the classification.
    /// </summary>
    public DateTimeOffset? UserCorrectedAt { get; private set; }

    /// <summary>
    /// Creates an imported transaction.
    /// </summary>
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

    /// <summary>
    /// Updates the normalized description.
    /// </summary>
    /// <param name="normalizedDescription">
    /// The normalized description.
    /// </param>
    public void NormalizeDescription(
        string normalizedDescription)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            normalizedDescription);

        NormalizedDescription =
            normalizedDescription.Trim();
    }

    /// <summary>
    /// Applies a machine-generated transaction classification.
    /// </summary>
    /// <param name="merchantId">
    /// The normalized merchant identifier.
    /// </param>
    /// <param name="categoryId">
    /// The category identifier.
    /// </param>
    /// <param name="subcategoryId">
    /// The optional subcategory identifier.
    /// </param>
    /// <param name="source">
    /// The classification source.
    /// </param>
    /// <param name="confidence">
    /// The classifier confidence score.
    /// </param>
    public void ApplyClassification(
        Guid? merchantId,
        Guid? categoryId,
        Guid? subcategoryId,
        ClassificationSource source,
        decimal confidence)
    {
        if (confidence < 0m ||
            confidence > 1m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(confidence));
        }

        MerchantId = merchantId;
        CategoryId = categoryId;
        SubcategoryId = subcategoryId;
        ClassificationSource = source;
        ClassificationConfidence = confidence;

        ClassificationStatus =
            confidence >= 0.85m
                ? ClassificationStatus.Classified
                : ClassificationStatus.Uncertain;

        ClassifiedAt =
            DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Applies an explicit user classification.
    /// </summary>
    /// <param name="merchantId">The merchant identifier.</param>
    /// <param name="categoryId">The category identifier.</param>
    /// <param name="subcategoryId">
    /// The optional subcategory identifier.
    /// </param>
    public void ApplyUserCorrection(
        Guid? merchantId,
        Guid categoryId,
        Guid? subcategoryId)
    {
        if (categoryId == Guid.Empty)
        {
            throw new ArgumentException(
                "Category identifier cannot be empty.",
                nameof(categoryId));
        }

        MerchantId = merchantId;
        CategoryId = categoryId;
        SubcategoryId = subcategoryId;
        ClassificationSource =
            ClassificationSource.User;
        ClassificationStatus =
            ClassificationStatus.UserCorrected;
        ClassificationConfidence = 1m;
        ClassifiedAt =
            DateTimeOffset.UtcNow;
        UserCorrectedAt =
            DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marks classification as failed.
    /// </summary>
    public void MarkClassificationFailed()
    {
        ClassificationStatus =
            ClassificationStatus.Failed;
        ClassificationSource =
            ClassificationSource.None;
    }
}
