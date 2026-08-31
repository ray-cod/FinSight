using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FinSight.Api.Middleware;

/// <summary>
/// Global exception handler that catches unhandled exceptions and returns standardized RFC 7807 problem details.
/// </summary>
/// <param name="logger">The logger used to record unhandled exceptions.</param>
public sealed partial class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    /// <summary>
    /// Attempts to handle an unhandled exception that occurred during HTTP request execution.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
    /// <param name="exception">The unhandled exception that was thrown.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if the exception was successfully handled; otherwise, <see langword="false"/>.</returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        LogUnhandledException(
            logger,
            exception,
            httpContext.TraceIdentifier);

        var problem = new ProblemDetails
        {
            Status =
                StatusCodes.Status500InternalServerError,

            Title = "An unexpected error occurred.",

            Detail =
                "The server could not complete the request.",

            Instance =
                httpContext.Request.Path
        };

        problem.Extensions["traceId"] =
            httpContext.TraceIdentifier;

        httpContext.Response.StatusCode =
            problem.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(
            problem,
            cancellationToken);

        return true;
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Unhandled exception. TraceId: {TraceId}")]
    private static partial void LogUnhandledException(
        ILogger logger,
        Exception exception,
        string traceId);
}
