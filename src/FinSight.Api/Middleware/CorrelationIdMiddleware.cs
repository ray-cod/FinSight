namespace FinSight.Api.Middleware;

/// <summary>
/// Middleware that inspects incoming HTTP requests for an <c>X-Correlation-ID</c> header, 
/// generating a new unique identifier if one is not present.
/// </summary>
/// <param name="next">The delegate representing the next middleware in the HTTP request pipeline.</param>
public sealed class CorrelationIdMiddleware(
    RequestDelegate next)
{
    private const string HeaderName =
        "X-Correlation-ID";

    /// <summary>
    /// Invokes the middleware to process the HTTP request context.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task InvokeAsync(
        HttpContext context)
    {
        var correlationId =
            context.Request.Headers[HeaderName]
                .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId =
                Guid.NewGuid().ToString("N");
        }

        context.Items[HeaderName] =
            correlationId;

        context.Response.Headers[HeaderName] =
            correlationId;

        await next(context);
    }
}
