namespace FinSight.Application.Abstractions.Identity;

/// <summary>
/// Provides contact information for authenticated application users.
/// </summary>
public interface IUserContactService
{
    /// <summary>
    /// Gets the primary email address of a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's email address.</returns>
    Task<string?> GetEmailAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
