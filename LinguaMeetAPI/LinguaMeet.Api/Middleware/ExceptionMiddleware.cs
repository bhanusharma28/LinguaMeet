using System.Text.Json;

namespace LinguaMeet.Api.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> log)
{
    public async Task InvokeAsync(HttpContext c)
    {
        try
        {
            await next(c);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Request failed");
            c.Response.StatusCode = ex is UnauthorizedAccessException ? 403 : 400;
            c.Response.ContentType = "application/json";
            await c.Response.WriteAsync(
                JsonSerializer.Serialize(
                    new
                    {
                        success = false,
                        message = ex is InvalidOperationException
                            ? ex.Message
                            : "The request could not be completed.",
                    }
                )
            );
        }
    }
}
