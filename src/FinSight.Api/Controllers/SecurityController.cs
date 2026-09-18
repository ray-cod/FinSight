using FinSight.Api.Extensions;
using FinSight.Application.Abstractions.Security;
using FinSight.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinSight.Api.Controllers;

/// <summary>
/// Provides authenticated security activity endpoints.
/// </summary>
[ApiController]
[Route("api/v1/security")]
[Authorize(Policy = AuthorizationPolicies.Authenticated)]
public sealed class SecurityController(
    ISecurityService securityService)
    : ControllerBase
{
    /// <summary>
    /// Returns recent security activity for the authenticated user.
    /// </summary>
    [HttpGet("activity")]
    [EnableRateLimiting("api")]
    public async Task<ActionResult<IReadOnlyList<SecurityActivityResponse>>> GetActivity(
        [FromQuery] int limit = 25,
        CancellationToken cancellationToken = default)
    {
        var response = await securityService.GetRecentActivityAsync(
            User.GetRequiredUserId(),
            limit,
            cancellationToken);

        return Ok(response);
    }
}
