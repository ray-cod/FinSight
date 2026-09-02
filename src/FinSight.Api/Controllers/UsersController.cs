using FinSight.Api.Extensions;
using FinSight.Application.Abstractions.Identity;
using FinSight.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinSight.Api.Controllers;

/// <summary>
/// Provides authenticated user profile endpoints.
/// </summary>
[ApiController]
[Route("api/v1/users")]
[Authorize(
    Policy =
        AuthorizationPolicies.Authenticated)]
public sealed class UsersController(
    IUserService userService)
    : ControllerBase
{
    /// <summary>
    /// Returns the authenticated user's profile.
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserResponse>> GetCurrentUser(
        CancellationToken cancellationToken)
    {
        var response =
            await userService.GetCurrentUserAsync(
                User.GetRequiredUserId(),
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Updates the authenticated user's display name.
    /// </summary>
    [HttpPatch("me")]
    public async Task<ActionResult<CurrentUserResponse>> UpdateCurrentUser(
        UpdateCurrentUserRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await userService.UpdateDisplayNameAsync(
                User.GetRequiredUserId(),
                request.DisplayName,
                cancellationToken);

        return Ok(response);
    }
}

/// <summary>
/// Represents an update to the current user's profile.
/// </summary>
/// <param name="DisplayName">The new display name.</param>
public sealed record UpdateCurrentUserRequest(
    string DisplayName);
