using FinSight.Application.Abstractions.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinSight.Api.Controllers;

/// <summary>
/// Provides transaction classification taxonomy endpoints.
/// </summary>
[ApiController]
[Route("api/v1/categories")]
[Authorize]
public sealed class CategoriesController(
    ICategoryRepository categoryRepository)
    : ControllerBase
{
    /// <summary>
    /// Gets active financial categories.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Available categories.</returns>
    [HttpGet]
    public async Task<IActionResult> GetCategories(
        CancellationToken cancellationToken)
    {
        var categories =
            await categoryRepository
                .GetActiveCategoriesAsync(
                    cancellationToken);

        var subcategories =
            await categoryRepository
                .GetActiveSubcategoriesAsync(
                    cancellationToken);

        return Ok(
            categories.Select(
                category =>
                    new
                    {
                        id = category.Id,
                        code = category.Code,
                        name = category.Name,
                        type = category.Type,
                        subcategories =
                            subcategories
                                .Where(
                                    x =>
                                        x.CategoryId ==
                                        category.Id)
                                .Select(
                                    x =>
                                        new
                                        {
                                            id = x.Id,
                                            code = x.Code,
                                            name = x.Name
                                        })
                                .ToArray()
                    }));
    }
}
