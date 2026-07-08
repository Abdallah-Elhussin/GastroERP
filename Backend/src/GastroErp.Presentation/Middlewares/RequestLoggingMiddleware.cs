using System.Diagnostics;
using Serilog.Context;

namespace GastroErp.Presentation.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        var userId = context.User.Identity?.IsAuthenticated == true 
            ? context.User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value 
            : "Anonymous";
            
        var tenantId = context.User.Claims.FirstOrDefault(c => c.Type == "TenantId")?.Value ?? "N/A";
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "N/A";
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("TenantId", tenantId))
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("ClientIP", ipAddress))
        {
            try
            {
                await _next(context);
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation("HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {ElapsedMilliseconds} ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    sw.ElapsedMilliseconds);
            }
        }
    }
}
