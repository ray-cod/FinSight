using FinSight.Api.Extensions;
using FinSight.Application.Features.Dashboard;
using FinSight.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinSight.Api.Controllers;

/// <summary>
/// Provides the aggregated financial overview used by the authenticated dashboard.
/// </summary>
[ApiController]
[Route("api/v1/dashboard")]
[Authorize(Policy = AuthorizationPolicies.Authenticated)]
public sealed class DashboardController(
    DashboardService dashboardService)
    : ControllerBase
{
    /// <summary>
    /// Gets the authenticated user's dashboard overview.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The dashboard read model.</returns>
    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> Get(
        CancellationToken cancellationToken)
    {
        var response = await dashboardService.GetAsync(
            User.GetRequiredUserId(),
            cancellationToken);

        return Ok(response);
    }
}
