using FinSight.Application.Features.Accounts;
using FinSight.Application.Features.Institutions;
using FinSight.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinSight.Api.Controllers;

/// <summary>
/// Provides supported financial institution endpoints.
/// </summary>
[ApiController]
[Route("api/v1/institutions")]
[Authorize(
    Policy =
        AuthorizationPolicies.Authenticated)]
public sealed class InstitutionsController(
    InstitutionService institutionService)
    : ControllerBase
{
    /// <summary>
    /// Gets all supported financial institutions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Supported financial institutions.</returns>
    [HttpGet]
    public async Task<ActionResult<
        IReadOnlyList<InstitutionResponse>>> GetInstitutions(
        CancellationToken cancellationToken)
    {
        var response =
            await institutionService.GetActiveAsync(
                cancellationToken);

        return Ok(response);
    }
}
