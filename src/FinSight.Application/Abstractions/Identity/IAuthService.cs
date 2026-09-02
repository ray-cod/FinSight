namespace FinSight.Application.Abstractions.Identity;

/// <summary>
/// Provides application-level authentication operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">Registration details.</param>
    /// <param name="ipAddress">The originating IP address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly issued authentication tokens.</returns>
    Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates an existing user.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <param name="ipAddress">The originating IP address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The issued authentication tokens.</returns>
    Task<AuthResponse> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rotates a refresh token and issues a new access token.
    /// </summary>
    /// <param name="request">Refresh token request.</param>
    /// <param name="ipAddress">The originating IP address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly issued authentication tokens.</returns>
    Task<AuthResponse> RefreshTokenAsync(
        RefreshTokenRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a refresh token.
    /// </summary>
    /// <param name="request">Logout request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task LogoutAsync(
        LogoutRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the authenticated user's password.
    /// </summary>
    /// <param name="userId">The authenticated user identifier.</param>
    /// <param name="request">Password change request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default);
}
