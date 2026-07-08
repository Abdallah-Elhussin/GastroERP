namespace GastroErp.Presentation.Infrastructure.ApiKeys;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "X-Api-Key";

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Check if endpoint requires API Key (e.g., via attribute)
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<RequireApiKeyAttribute>() != null)
        {
            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key was not provided.");
                return;
            }

            // 2. Validate Key (passive structure, DB validation omitted)
            if (extractedApiKey != "valid-api-key-placeholder")
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized client.");
                return;
            }
        }

        await _next(context);
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireApiKeyAttribute : Attribute
{
}
