using Microsoft.AspNetCore.Identity;

namespace FinSight.Infrastructure.Identity;

/// <summary>
/// Represents the ASP.NET Core Identity persistence model for a FinSight user.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// Gets or sets the user's display name.
    /// </summary>
    public string DisplayName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the time at which the user was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the time at which the user was last modified.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}
