using FinSight.Application.Abstractions.Identity;
using FinSight.Application.Abstractions.Security;
using FinSight.Domain.Security;
using Microsoft.AspNetCore.Identity;

namespace FinSight.Infrastructure.Identity;

/// <summary>
/// Provides password-reset operations backed by ASP.NET Core Identity.
/// </summary>
public sealed class PasswordResetService(
    UserManager<ApplicationUser> userManager,
    IAuditService auditService)
    : IPasswordResetService
{
    /// <inheritdoc />
    public async Task RequestResetAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentException.ThrowIfNullOrWhiteSpace(
            email);

        var normalizedEmail =
            email.Trim().ToLowerInvariant();

        var user =
            await userManager.FindByEmailAsync(
                normalizedEmail);

        /*
         * Security requirement:
         * Do not disclose whether the supplied email exists.
         */
        if (user is null)
        {
            return;
        }

        var token =
            await userManager.GeneratePasswordResetTokenAsync(
                user);

        await auditService.RecordAsync(
            SecurityEventType.PasswordResetRequested,
            user.Id,
            null,
            cancellationToken: cancellationToken);

        /*
         * The token must be delivered through the notification
         * infrastructure in Phase 8.
         *
         * It is intentionally not returned from this method.
         *
         * For local development, a future development-only
         * notification implementation may log the token at
         * Debug level.
         */
    }

    /// <inheritdoc />
    public async Task ResetAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.Email);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.Token);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.NewPassword);

        var normalizedEmail =
            request.Email.Trim().ToLowerInvariant();

        var user =
            await userManager.FindByEmailAsync(
                normalizedEmail);

        /*
         * Return the same generic failure for a missing account
         * or invalid token so account existence is not disclosed.
         */
        if (user is null)
        {
            throw new InvalidOperationException(
                "The password reset request is invalid.");
        }

        var result =
            await userManager.ResetPasswordAsync(
                user,
                request.Token,
                request.NewPassword);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                "The password reset request is invalid.");
        }

        await userManager.UpdateSecurityStampAsync(
            user);

        await auditService.RecordAsync(
            SecurityEventType.PasswordResetCompleted,
            user.Id,
            null,
            cancellationToken: cancellationToken);
    }
}
