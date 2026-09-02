using Microsoft.AspNetCore.Identity;

namespace FinSight.Infrastructure.Identity;

/// <summary>
/// Represents an application authorization role.
/// </summary>
public sealed class ApplicationRole : IdentityRole<Guid>
{
}
