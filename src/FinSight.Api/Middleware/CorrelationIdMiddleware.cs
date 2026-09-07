using System.Diagnostics;
using Serilog.Context;

namespace FinSight.Api.Middleware;

/// <summary>
/// Adds a stable correlation identifier to requests and logs.
/// </summary>
public sealed class CorrelationIdMiddleware(
    RequestDelegate next)
{
    private const string HeaderName =
        "X-Correlation-ID";

    /// <summary>
    /// Executes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(
        HttpContext context)
    {
        var correlationId =
            context.Request.Headers[
                HeaderName]
                .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(
                correlationId))
        {
            correlationId =
                Guid.NewGuid().ToString("N");
        }

        context.Items[HeaderName] =
            correlationId;

        context.Response.Headers[
            HeaderName] =
            correlationId;

        using (
            LogContext.PushProperty(
                "CorrelationId",
                correlationId))
        using (
            LogContext.PushProperty(
                "TraceId",
                Activity.Current?
                    .TraceId
                    .ToString()))
        {
            await next(context);
        }
    }
}
