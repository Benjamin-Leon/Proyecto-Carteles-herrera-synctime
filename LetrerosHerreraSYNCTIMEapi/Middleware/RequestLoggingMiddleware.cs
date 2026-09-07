namespace LetrerosHerreraSYNCTIMEapi.Middleware;

public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Este middleware envuelve cada endpoint y deja evidencia de metodo, ruta,
        // estado HTTP y tiempo de respuesta sin duplicar codigo en cada endpoint.
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            logger.LogInformation(
                "HTTP {Method} {Path} respondio {StatusCode} en {ElapsedMilliseconds} ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
