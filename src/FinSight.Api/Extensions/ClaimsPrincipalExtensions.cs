using System.Security.Claims;

namespace FinSight.Api.Extensions;

/// <summary>
/// Provides helpers for extracting FinSight identity information from claims.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the authenticated FinSight user identifier.
    /// </summary>
    /// <param name="principal">The current claims principal.</param>
    /// <returns>The authenticated user identifier.</returns>
    public static Guid GetRequiredUserId(
        this ClaimsPrincipal principal)
    {
        var value =
            principal.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException(
                "The authenticated user identifier is invalid.");
        }

        return userId;
    }

    /// <summary>
    /// Gets the authenticated FinSight session identifier.
    /// </summary>
    /// <param name="principal">The current claims principal.</param>
    /// <returns>The authenticated session identifier.</returns>
    public static Guid GetRequiredSessionId(
        this ClaimsPrincipal principal)
    {
        var value =
            principal.FindFirstValue(
                ClaimTypes.Sid)
            ?? principal.FindFirstValue("sid");

        if (!Guid.TryParse(value, out var sessionId))
        {
            throw new UnauthorizedAccessException(
                "The authenticated session identifier is invalid.");
        }

        return sessionId;
    }
}
