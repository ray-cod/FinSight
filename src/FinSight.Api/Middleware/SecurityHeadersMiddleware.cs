namespace FinSight.Api.Middleware;

/// <summary>
/// Adds baseline HTTP security headers to responses.
/// </summary>
public sealed class SecurityHeadersMiddleware(
    RequestDelegate next)
{
    /// <summary>
    /// Executes the middleware.
    /// </summary>
    /// <param name="context">
    /// The current HTTP context.
    /// </param>
    public async Task InvokeAsync(
        HttpContext context)
    {
        context.Response.Headers[
            "X-Content-Type-Options"] =
            "nosniff";

        context.Response.Headers[
            "X-Frame-Options"] =
            "DENY";

        context.Response.Headers[
            "Referrer-Policy"] =
            "strict-origin-when-cross-origin";

        context.Response.Headers[
            "Permissions-Policy"] =
            "camera=(), microphone=(), geolocation=()";

        await next(context);
    }
}
