namespace FinSight.Application.Abstractions.Identity;

/// <summary>
/// Represents credentials required to register a new user.
/// </summary>
public sealed record RegisterRequest(
    string Email,
    string Password,
    string DisplayName);

/// <summary>
/// Represents credentials required to authenticate a user.
/// </summary>
public sealed record LoginRequest(
    string Email,
    string Password);

/// <summary>
/// Represents the access and refresh tokens issued to a user.
/// </summary>
public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    Guid UserId);

/// <summary>
/// Represents a refresh-token request.
/// </summary>
/// <param name="RefreshToken">The refresh token to rotate.</param>
public sealed record RefreshTokenRequest(
    string RefreshToken);

/// <summary>
/// Represents a logout request.
/// </summary>
/// <param name="RefreshToken">The refresh token to revoke.</param>
public sealed record LogoutRequest(
    string RefreshToken);

/// <summary>
/// Represents a forgot-password request.
/// </summary>
/// <param name="Email">The email address associated with the account.</param>
public sealed record ForgotPasswordRequest(
    string Email);

/// <summary>
/// Represents a password reset request.
/// </summary>
/// <param name="Email">The email address associated with the account.</param>
/// <param name="Token">The password reset token.</param>
/// <param name="NewPassword">The new password.</param>
public sealed record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword);

/// <summary>
/// Represents a change-password request.
/// </summary>
/// <param name="CurrentPassword">The current password.</param>
/// <param name="NewPassword">The new password.</param>
public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);

/// <summary>
/// Represents the authenticated user's profile.
/// </summary>
public sealed record CurrentUserResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    bool EmailConfirmed);
