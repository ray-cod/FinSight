namespace FinSight.Application.Security;

/// <summary>
/// Contains application authorization policy names.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Requires an authenticated user.
    /// </summary>
    public const string Authenticated =
        "authenticated";

    /// <summary>
    /// Requires an administrator.
    /// </summary>
    public const string Administrator =
        "administrator";
}
