namespace FinSight.Infrastructure.AI;

/// <summary>
/// Controls AI transaction-processing safeguards.
/// </summary>
public sealed class AiProcessingOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName =
        "AiProcessing";

    /// <summary>
    /// Gets the daily AI request limit per user.
    /// </summary>
    public int DailyRequestLimit { get; init; } = 500;

    /// <summary>
    /// Gets the maximum classification duration.
    /// </summary>
    public TimeSpan Timeout { get; init; } =
        TimeSpan.FromSeconds(20);
}
