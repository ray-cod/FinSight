using Microsoft.AspNetCore.Mvc;

namespace FinSight.Api.Controllers;

/// <summary>
/// Provides system health and diagnostic endpoints for the API service.
/// </summary>
[ApiController]
[Route("api/v1/system")]
public sealed class SystemController : ControllerBase
{
    /// <summary>
    /// Retrieves basic information about the API service and its current execution environment.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> containing service metadata.</returns>
    [HttpGet("info")]
    public IActionResult GetInfo()
    {
        return Ok(new
        {
            service = "FinSight.Api",
            version = "1.0.0",
            environment =
                Environment.GetEnvironmentVariable(
                    "ASPNETCORE_ENVIRONMENT")
                ?? "Production"
        });
    }
}
