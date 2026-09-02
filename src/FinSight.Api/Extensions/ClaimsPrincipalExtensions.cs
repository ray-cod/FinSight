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
}
