namespace FinSight.Infrastructure.AI.OpenAI;

/// <summary>
/// Configuration options for the OpenAI transaction-classification provider.
/// </summary>
public sealed class OpenAiOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName =
        "OpenAI";

    /// <summary>
    /// Gets the API key.
    /// </summary>
    public required string ApiKey { get; init; }

    /// <summary>
    /// Gets the model identifier used for classification.
    /// </summary>
    public required string Model { get; init; }

    /// <summary>
    /// Gets the maximum output token budget.
    /// </summary>
    public int MaxOutputTokens { get; init; } = 500;
}
