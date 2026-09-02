namespace FinSight.Application.Abstractions.Identity;

/// <summary>
/// Creates and validates application authentication tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Creates an access token for the specified identity.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="email">The user's email.</param>
    /// <param name="roles">The user's roles.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A generated access token result.</returns>
    Task<AccessTokenResult> CreateAccessTokenAsync(
        Guid userId,
        string email,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a generated access token.
/// </summary>
/// <param name="Token">The encoded access token.</param>
/// <param name="ExpiresAt">The token expiration timestamp.</param>
public sealed record AccessTokenResult(
    string Token,
    DateTimeOffset ExpiresAt);
