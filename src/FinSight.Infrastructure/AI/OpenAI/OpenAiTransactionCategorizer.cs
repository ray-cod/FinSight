using System.ClientModel;
using System.Text.Json;
using FinSight.Application.Abstractions.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;

namespace FinSight.Infrastructure.AI.OpenAI;

/// <summary>
/// Uses an OpenAI-backed chat client to classify financial transactions.
/// </summary>
public sealed class OpenAiTransactionCategorizer
    : ITransactionCategorizer
{
    private readonly IChatClient _chatClient;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="OpenAiTransactionCategorizer"/> class.
    /// </summary>
    /// <param name="options">OpenAI configuration.</param>
    public OpenAiTransactionCategorizer(
        IOptions<OpenAiOptions> options)
    {
        var configuration =
            options.Value;

        var client =
            new OpenAIClient(
                new ApiKeyCredential(
                    configuration.ApiKey));

        _chatClient =
            client
                .GetChatClient(
                    configuration.Model)
                .AsIChatClient();
    }

    /// <inheritdoc />
    public async Task<TransactionClassificationResult> CategorizeAsync(
        TransactionClassificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var categoryJson =
            JsonSerializer.Serialize(
                request.Categories);

        var prompt = $"""
            Classify the following financial transaction.

            Raw description:
            {request.RawDescription}

            Normalized description:
            {request.NormalizedDescription}

            Amount:
            {request.Amount}

            Currency:
            {request.Currency}

            Transaction type:
            {request.TransactionType}

            Available categories:
            {categoryJson}
            """;

        var messages =
            new List<ChatMessage>
            {
                new(
                    ChatRole.System,
                    """
                    You classify financial transactions for a
                    production financial data pipeline.

                    Never invent category codes.
                    Return structured data only.
                    Lower confidence when evidence is ambiguous.
                    """),

                new(
                    ChatRole.User,
                    prompt)
            };

        var options =
            new ChatOptions
            {
                Temperature = 0f,
                MaxOutputTokens = 500,
                ResponseFormat =
                    ChatResponseFormat.ForJsonSchema<
                        StructuredClassification>()
            };

        var response =
            await _chatClient.GetResponseAsync<
                StructuredClassification>(
                messages,
                options,
                cancellationToken: cancellationToken);

        var result =
            response.Result;

        if (result is null)
        {
            throw new InvalidOperationException(
                "AI classification returned no result.");
        }

        return new TransactionClassificationResult(
            result.Merchant.Trim(),
            result.CategoryCode.Trim().ToUpperInvariant(),
            string.IsNullOrWhiteSpace(
                result.SubcategoryCode)
                ? null
                : result.SubcategoryCode
                    .Trim()
                    .ToUpperInvariant(),
            result.Confidence,
            result.ClassificationRationale.Trim());
    }

    /// <summary>
    /// Represents the structured response expected from the AI model.
    /// </summary>
    private sealed record StructuredClassification
    {
        /// <summary>
        /// Gets or initializes the merchant name.
        /// </summary>
        public required string Merchant { get; init; }

        /// <summary>
        /// Gets or initializes the category code.
        /// </summary>
        public required string CategoryCode { get; init; }

        /// <summary>
        /// Gets or initializes the optional subcategory code.
        /// </summary>
        public string? SubcategoryCode { get; init; }

        /// <summary>
        /// Gets or initializes the confidence score.
        /// </summary>
        public decimal Confidence { get; init; }

        /// <summary>
        /// Gets or initializes the classification rationale.
        /// </summary>
        public required string ClassificationRationale { get; init; }
    }
}
