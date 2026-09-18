using System.Net;

namespace WowClassicEraItems.Api.Middleware;

/// <summary>
/// Catches unhandled exceptions and converts them into a consistent
/// ProblemDetails JSON response instead of leaking stack traces.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception while processing {Method} {Path}", context.Request.Method, context.Request.Path);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(new
            {
                title = "An unexpected error occurred.",
                status = context.Response.StatusCode
            });
        }
    }
}
