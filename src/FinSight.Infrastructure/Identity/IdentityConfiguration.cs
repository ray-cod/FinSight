using Microsoft.AspNetCore.Identity;

namespace FinSight.Infrastructure.Identity;

/// <summary>
/// Provides strongly defined password and account-security settings.
/// </summary>
public static class IdentityConfiguration
{
    /// <summary>
    /// Configures ASP.NET Core Identity security options.
    /// </summary>
    /// <param name="options">Identity options to configure.</param>
    public static void Configure(
        IdentityOptions options)
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);

        options.SignIn.RequireConfirmedEmail = false;
    }
}
