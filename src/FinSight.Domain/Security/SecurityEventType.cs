namespace FinSight.Domain.Security;

/// <summary>
/// Represents a security-related event that can be audited.
/// </summary>
public enum SecurityEventType
{
    /// <summary>
    /// A user registered successfully.
    /// </summary>
    UserRegistered = 1,

    /// <summary>
    /// A user successfully authenticated.
    /// </summary>
    LoginSucceeded = 2,

    /// <summary>
    /// A login attempt failed.
    /// </summary>
    LoginFailed = 3,

    /// <summary>
    /// A refresh token was issued or rotated.
    /// </summary>
    RefreshTokenIssued = 4,

    /// <summary>
    /// A refresh token was revoked.
    /// </summary>
    RefreshTokenRevoked = 5,

    /// <summary>
    /// A password was changed.
    /// </summary>
    PasswordChanged = 6,

    /// <summary>
    /// A password reset was requested.
    /// </summary>
    PasswordResetRequested = 7,

    /// <summary>
    /// A password was successfully reset.
    /// </summary>
    PasswordResetCompleted = 8,

    /// <summary>
    /// An email address was confirmed.
    /// </summary>
    EmailConfirmed = 9,

    /// <summary>
    /// A user's profile was updated.
    /// </summary>
    ProfileUpdated = 10
}
