namespace FinSight.Domain.Users;

/// <summary>
/// Represents the lifecycle status of a FinSight user account.
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// The account is active and may use the application.
    /// </summary>
    Active = 1,

    /// <summary>
    /// The account is temporarily suspended.
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// The account has been deactivated.
    /// </summary>
    Deactivated = 3
}
