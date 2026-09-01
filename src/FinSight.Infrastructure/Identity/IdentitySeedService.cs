using Microsoft.AspNetCore.Identity;

namespace FinSight.Infrastructure.Identity;

/// <summary>
/// Seeds required FinSight identity roles.
/// </summary>
public sealed class IdentitySeedService(
    RoleManager<ApplicationRole> roleManager)
{
    private static readonly string[] Roles =
    [
        "User",
        "Admin"
    ];

    /// <summary>
    /// Ensures all required application roles exist.
    /// </summary>
    public async Task SeedAsync()
    {
        foreach (var roleName in Roles)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result =
                await roleManager.CreateAsync(
                    new ApplicationRole
                    {
                        Id = Guid.NewGuid(),
                        Name = roleName
                    });

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not create role '{roleName}'.");
            }
        }
    }
}
