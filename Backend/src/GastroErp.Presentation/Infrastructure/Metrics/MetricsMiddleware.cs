using System.Diagnostics;

namespace GastroErp.Presentation.Infrastructure.Metrics;

public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MetricsMiddleware> _logger;

    public MetricsMiddleware(RequestDelegate next, ILogger<MetricsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            // Passive structure: Push to Prometheus / OpenTelemetry
            _logger.LogDebug("Metrics: Request to {Path} took {ElapsedMilliseconds}ms", context.Request.Path, sw.ElapsedMilliseconds);
        }
    }
}
