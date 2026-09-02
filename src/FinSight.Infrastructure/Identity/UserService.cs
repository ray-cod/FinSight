using FinSight.Application.Abstractions.Identity;
using FinSight.Application.Abstractions.Security;
using FinSight.Domain.Security;
using Microsoft.AspNetCore.Identity;

namespace FinSight.Infrastructure.Identity;

/// <summary>
/// Provides application user profile operations.
/// </summary>
public sealed class UserService(
    UserManager<ApplicationUser> userManager,
    IAuditService auditService)
    : IUserService
{
    /// <inheritdoc />
    public async Task<CurrentUserResponse> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user =
            await userManager.FindByIdAsync(
                userId.ToString());

        if (user is null)
        {
            throw new KeyNotFoundException(
                "User was not found.");
        }

        return ToResponse(user);
    }

    /// <inheritdoc />
    public async Task<CurrentUserResponse> UpdateDisplayNameAsync(
        Guid userId,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentException.ThrowIfNullOrWhiteSpace(
            displayName);

        var user =
            await userManager.FindByIdAsync(
                userId.ToString());

        if (user is null)
        {
            throw new KeyNotFoundException(
                "User was not found.");
        }

        var normalized =
            displayName.Trim();

        if (normalized.Length > 100)
        {
            throw new ArgumentException(
                "Display name cannot exceed 100 characters.",
                nameof(displayName));
        }

        user.DisplayName = normalized;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        var result =
            await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    " ",
                    result.Errors.Select(
                        error => error.Description)));
        }

        await auditService.RecordAsync(
            SecurityEventType.ProfileUpdated,
            user.Id,
            null,
            cancellationToken: cancellationToken);

        return ToResponse(user);
    }

    private static CurrentUserResponse ToResponse(
        ApplicationUser user)
    {
        return new CurrentUserResponse(
            user.Id,
            user.Email!,
            user.DisplayName,
            user.EmailConfirmed);
    }
}
