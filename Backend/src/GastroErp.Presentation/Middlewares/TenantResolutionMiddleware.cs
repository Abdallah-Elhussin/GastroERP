using GastroErp.Application.Common.Interfaces.Platform;

namespace GastroErp.Presentation.Middlewares;

public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantResolver tenantResolver, ITenantContext tenantContext)
    {
        var tenantId = tenantResolver.ResolveTenantId();
        if (tenantId.HasValue)
        {
            tenantContext.SetTenant(
                tenantId.Value,
                tenantResolver.ResolveTenantSlug(),
                ResolveSource(context));
        }

        await _next(context);
    }

    private static string ResolveSource(HttpContext context)
    {
        if (context.User?.FindFirst("TenantId") is not null)
        {
            return "jwt";
        }

        if (context.Request.Headers.ContainsKey("X-Tenant-Id") || context.Request.Headers.ContainsKey("X-Tenant"))
        {
            return "header";
        }

        if (context.Request.Headers.ContainsKey("X-Api-Key"))
        {
            return "api-key";
        }

        if (context.Request.Headers.ContainsKey("X-Tenant-Slug"))
        {
            return "slug-header";
        }

        return "subdomain";
    }
}
