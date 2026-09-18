namespace FinSight.Application.Abstractions.Security;

/// <summary>
/// Provides safe, user-scoped security activity information.
/// </summary>
public interface ISecurityService
{
    /// <summary>
    /// Gets recent security activity for the authenticated user.
    /// </summary>
    /// <param name="userId">The authenticated user identifier.</param>
    /// <param name="limit">The maximum number of events to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The recent security activity.</returns>
    Task<IReadOnlyList<SecurityActivityResponse>> GetRecentActivityAsync(
        Guid userId,
        int limit = 25,
        CancellationToken cancellationToken = default);
}
