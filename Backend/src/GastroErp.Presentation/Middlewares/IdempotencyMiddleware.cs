namespace GastroErp.Presentation.Middlewares;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IdempotencyMiddleware> _logger;

    public IdempotencyMiddleware(RequestDelegate next, ILogger<IdempotencyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey))
        {
            _logger.LogInformation("Idempotency key received: {IdempotencyKey}", idempotencyKey.ToString());
            // Passive infrastructure: In the future, check cache/DB for this key to prevent duplicate execution.
            // For now, we just pass the request through.
        }

        await _next(context);
    }
}
