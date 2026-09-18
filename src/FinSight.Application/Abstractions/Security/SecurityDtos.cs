namespace FinSight.Application.Abstractions.Security;

/// <summary>
/// Represents a safe projection of a persisted security audit event.
/// </summary>
public sealed record SecurityActivityResponse(
    string EventType,
    DateTimeOffset OccurredAt);
