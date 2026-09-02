using FinSight.Api.Extensions;
using FinSight.Application.Abstractions.Identity;
using FinSight.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinSight.Api.Controllers;

/// <summary>
/// Provides authentication and account-security endpoints.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(
    IAuthService authService,
    IPasswordResetService passwordResetService)
    : ControllerBase
{
    /// <summary>
    /// Registers a new FinSight account.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await authService.RegisterAsync(
                request,
                HttpContext.Connection.RemoteIpAddress
                    ?.ToString(),
                cancellationToken);

        return Created(
            string.Empty,
            response);
    }

    /// <summary>
    /// Authenticates a FinSight user.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await authService.LoginAsync(
                request,
                HttpContext.Connection.RemoteIpAddress
                    ?.ToString(),
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Rotates a refresh token and returns a new token pair.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<AuthResponse>> Refresh(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await authService.RefreshTokenAsync(
                request,
                HttpContext.Connection.RemoteIpAddress
                    ?.ToString(),
                cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Revokes the supplied refresh token.
    /// </summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(
        LogoutRequest request,
        CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(
            request,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Changes the password of the authenticated user.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize(
        Policy =
            AuthorizationPolicies.Authenticated)]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        await authService.ChangePasswordAsync(
            User.GetRequiredUserId(),
            request,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Requests a password reset.
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await passwordResetService.RequestResetAsync(
            request.Email,
            cancellationToken);

        // Always return the same response so account existence
        // is not disclosed.
        return Accepted();
    }

    /// <summary>
    /// Resets a user's password using a valid reset token.
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await passwordResetService.ResetAsync(
            request,
            cancellationToken);

        return NoContent();
    }
}
