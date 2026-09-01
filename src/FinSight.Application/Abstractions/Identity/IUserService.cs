namespace FinSight.Application.Abstractions.Identity;

/// <summary>
/// Provides operations for retrieving and updating user profiles.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets the currently authenticated user's profile.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's profile.</returns>
    Task<CurrentUserResponse> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the currently authenticated user's display name.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="displayName">The new display name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<CurrentUserResponse> UpdateDisplayNameAsync(
        Guid userId,
        string displayName,
        CancellationToken cancellationToken = default);
}
