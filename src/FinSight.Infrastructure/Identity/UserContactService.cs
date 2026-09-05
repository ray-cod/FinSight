using FinSight.Application.Abstractions.Identity;
using Microsoft.AspNetCore.Identity;

namespace FinSight.Infrastructure.Identity;

/// <summary>
/// Retrieves user contact information from ASP.NET Core Identity.
/// </summary>
public sealed class UserContactService(
    UserManager<ApplicationUser> userManager)
    : IUserContactService
{
    /// <inheritdoc />
    public async Task<string?> GetEmailAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user =
            await userManager.FindByIdAsync(
                userId.ToString());

        return user?.Email;
    }
}
