using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FinSight.Api.Middleware;

/// <summary>
/// Converts application exceptions into consistent HTTP ProblemDetails responses.
/// </summary>
public sealed partial class ApiExceptionHandler(
    ILogger<ApiExceptionHandler> logger)
    : IExceptionHandler
{
    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) =
            exception switch
            {
                UnauthorizedAccessException =>
                    (StatusCodes.Status401Unauthorized,
                        "Authentication required."),

                KeyNotFoundException =>
                    (StatusCodes.Status404NotFound,
                        "Resource not found."),

                ArgumentException =>
                    (StatusCodes.Status400BadRequest,
                        "Invalid request."),

                InvalidOperationException =>
                    (StatusCodes.Status409Conflict,
                        "Request could not be completed."),

                _ =>
                    (
                        StatusCodes
                            .Status500InternalServerError,
                        "An unexpected error occurred."
                    )
            };

        if (status >= 500)
        {
            LogUnhandledException(
                logger,
                exception);
        }
        else
        {
            LogHandledException(
                logger,
                status,
                exception.GetType().Name);
        }

        httpContext.Response.StatusCode =
            status;

        var problem =
            new ProblemDetails
            {
                Status = status,
                Title = title,
                Instance =
                    httpContext.Request.Path
            };

        problem.Extensions["traceId"] =
            System.Diagnostics.Activity
                .Current?
                .TraceId
                .ToString();

        await httpContext.Response
            .WriteAsJsonAsync(
                problem,
                cancellationToken);

        return true;
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Unhandled application exception.")]
    private static partial void LogUnhandledException(
        ILogger logger,
        Exception exception);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Warning,
        Message = "Handled application exception. Status={Status}, Type={Type}")]
    private static partial void LogHandledException(
        ILogger logger,
        int status,
        string type);
}
