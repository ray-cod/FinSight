using FinSight.Application.Abstractions.AI;
using FinSight.Application.Abstractions.Caching;
using FinSight.Application.Abstractions.Intelligence;
using FinSight.Application.Abstractions.Messaging;
using FinSight.Application.Abstractions.Observability;
using FinSight.Application.Abstractions.Persistence;
using FinSight.Contracts.Events;
using FinSight.Domain.Transactions;
using Microsoft.Extensions.Logging;

namespace FinSight.Application.Features.Transactions;

/// <summary>
/// Coordinates transaction normalization, rule classification,
/// AI categorization, persistence, and event publication.
/// </summary>
public sealed partial class TransactionProcessingService(
    ITransactionRepository transactionRepository,
    ICategoryRepository categoryRepository,
    // IMerchantRepository merchantRepository,
    ICacheService cacheService,
    IMerchantNormalizer merchantNormalizer,
    ICategoryRuleEngine ruleEngine,
    ITransactionCategorizer aiCategorizer,
    MerchantResolutionService merchantResolutionService,
    IEventPublisher eventPublisher,
    IUnitOfWork unitOfWork,
    ILogger<TransactionProcessingService> logger,
    IFinSightTelemetry telemetry)
{
    private static readonly TimeSpan CacheExpiration =
        TimeSpan.FromDays(30);

    /// <summary>
    /// Processes a single imported transaction.
    /// </summary>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="userId">The owning user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task ProcessAsync(
        Guid transactionId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var transaction =
            await transactionRepository
                .GetByIdAsync(
                    userId,
                    transactionId,
                    cancellationToken);

        if (transaction is null)
        {
            throw new KeyNotFoundException(
                "Transaction was not found.");
        }

        if (transaction.ClassificationStatus ==
            ClassificationStatus.UserCorrected)
        {
            LogSkippingUserCorrected(transactionId);

            return;
        }

        var normalized =
            merchantNormalizer.Normalize(
                transaction.RawDescription);

        transaction.NormalizeDescription(
            normalized);

        var ruleResult =
            ruleEngine.TryClassify(
                normalized);

        if (ruleResult is not null)
        {
            await ApplyClassificationAsync(
                transaction,
                ruleResult.Merchant,
                ruleResult.CategoryCode,
                ruleResult.SubcategoryCode,
                ClassificationSource.Rule,
                ruleResult.Confidence,
                cancellationToken);

            return;
        }

        var cacheKey =
            BuildCacheKey(normalized);

        var cached =
            await cacheService
                .GetAsync<TransactionClassificationResult>(
                    cacheKey,
                    cancellationToken);

        if (cached is not null)
        {
            await ApplyClassificationAsync(
                transaction,
                cached.Merchant,
                cached.CategoryCode,
                cached.SubcategoryCode,
                ClassificationSource.Cache,
                cached.Confidence,
                cancellationToken);

            return;
        }

        var categories =
            await BuildCategoryOptionsAsync(
                cancellationToken);

        try
        {
            var aiResult =
                await aiCategorizer.CategorizeAsync(
                    new TransactionClassificationRequest(
                        transaction.RawDescription,
                        normalized,
                        transaction.Amount,
                        transaction.Currency,
                        transaction.Type.ToString(),
                        categories),
                    cancellationToken);

            ValidateAiResult(
                aiResult,
                categories);

            await cacheService.SetAsync(
                cacheKey,
                aiResult,
                CacheExpiration,
                cancellationToken);

            await ApplyClassificationAsync(
                transaction,
                aiResult.Merchant,
                aiResult.CategoryCode,
                aiResult.SubcategoryCode,
                ClassificationSource.Ai,
                aiResult.Confidence,
                cancellationToken);
        }
        catch (Exception exception)
        {
            transaction.MarkClassificationFailed();

            await unitOfWork.SaveChangesAsync(
                cancellationToken);

            LogClassificationFailed(
                exception,
                transactionId);

            throw;
        }
    }

    private async Task ApplyClassificationAsync(
        Transaction transaction,
        string merchantName,
        string categoryCode,
        string? subcategoryCode,
        ClassificationSource source,
        decimal confidence,
        CancellationToken cancellationToken)
    {
        var category =
            await categoryRepository.GetByCodeAsync(
                categoryCode,
                cancellationToken);

        if (category is null)
        {
            throw new InvalidOperationException(
                $"Unknown category code '{categoryCode}'.");
        }

        Guid? subcategoryId = null;

        if (!string.IsNullOrWhiteSpace(
            subcategoryCode))
        {
            var subcategory =
                await categoryRepository
                    .GetSubcategoryByCodeAsync(
                        subcategoryCode,
                        cancellationToken);

            if (subcategory is null ||
                subcategory.CategoryId != category.Id)
            {
                throw new InvalidOperationException(
                    $"Invalid subcategory '{subcategoryCode}'.");
            }

            subcategoryId =
                subcategory.Id;
        }

        var merchantId =
            await merchantResolutionService.ResolveAsync(
                merchantName,
                transaction.NormalizedDescription,
                cancellationToken);

        transaction.ApplyClassification(
            merchantId,
            category.Id,
            subcategoryId,
            source,
            confidence);

        await eventPublisher.PublishAsync(
            new TransactionCategorizedEvent
            {
                EventId = Guid.NewGuid(),
                UserId = transaction.UserId,
                TransactionId =
                    transaction.Id.Value,
                MerchantId =
                    merchantId,
                CategoryId =
                    category.Id,
                SubcategoryId =
                    subcategoryId,
                Source = source.ToString(),
                Confidence = confidence,
                OccurredAt =
                    DateTimeOffset.UtcNow
            },
            "transaction.categorized",
            cancellationToken);

        await unitOfWork.SaveChangesAsync(
                cancellationToken);

        telemetry.IncrementTransactionsCategorized(1);

        LogTransactionCategorized(
            transaction.Id.Value,
            source);
    }

    private async Task<
        IReadOnlyCollection<CategoryClassificationOption>>
        BuildCategoryOptionsAsync(
            CancellationToken cancellationToken)
    {
        var categories =
            await categoryRepository
                .GetActiveCategoriesAsync(
                    cancellationToken);

        var subcategories =
            await categoryRepository
                .GetActiveSubcategoriesAsync(
                    cancellationToken);

        return (
            from category in categories
            join subcategory in subcategories
                on category.Id
                equals subcategory.CategoryId
                into categorySubcategories
            from subcategory in
                categorySubcategories.DefaultIfEmpty()
            select new CategoryClassificationOption(
                category.Id,
                category.Code,
                category.Name,
                subcategory?.Code,
                subcategory?.Name))
            .ToArray();
    }

    private static void ValidateAiResult(
        TransactionClassificationResult result,
        IReadOnlyCollection<
            CategoryClassificationOption> categories)
    {
        if (string.IsNullOrWhiteSpace(
            result.Merchant))
        {
            throw new InvalidOperationException(
                "AI returned an empty merchant.");
        }

        if (result.Confidence < 0m ||
            result.Confidence > 1m)
        {
            throw new InvalidOperationException(
                "AI returned an invalid confidence score.");
        }

        var category =
            categories.FirstOrDefault(
                x =>
                    x.CategoryCode ==
                    result.CategoryCode);

        if (category is null)
        {
            throw new InvalidOperationException(
                "AI returned an unknown category.");
        }

        if (!string.IsNullOrWhiteSpace(
            result.SubcategoryCode) &&
            !categories.Any(
                x =>
                    x.CategoryCode ==
                    result.CategoryCode &&
                    x.SubcategoryCode ==
                    result.SubcategoryCode))
        {
            throw new InvalidOperationException(
                "AI returned an invalid subcategory.");
        }
    }

    private static string BuildCacheKey(
        string normalizedDescription)
    {
        return
            $"transaction-classification:{normalizedDescription}";
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Skipping user-corrected transaction {TransactionId}.")]
    private partial void LogSkippingUserCorrected(Guid transactionId);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "Transaction classification failed for {TransactionId}.")]
    private partial void LogClassificationFailed(Exception exception, Guid transactionId);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Transaction {TransactionId} categorized using {Source}.")]
    private partial void LogTransactionCategorized(Guid transactionId, ClassificationSource source);
}
