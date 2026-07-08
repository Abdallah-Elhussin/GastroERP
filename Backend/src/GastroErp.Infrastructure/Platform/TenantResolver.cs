using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Platform;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Infrastructure.Platform;

public sealed class TenantResolver : ITenantResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IApplicationDbContext _context;

    public TenantResolver(IHttpContextAccessor httpContextAccessor, IApplicationDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    public Guid? ResolveTenantId()
    {
        return ResolveTenantId(_httpContextAccessor.HttpContext);
    }

    public string? ResolveTenantSlug()
    {
        return ResolveTenantSlug(_httpContextAccessor.HttpContext);
    }

    public Guid? ResolveTenantId(HttpContext? httpContext)
    {
        httpContext ??= _httpContextAccessor.HttpContext;
        if (httpContext is null) return null;

        var claim = httpContext.User?.FindFirst("TenantId")?.Value;
        if (Guid.TryParse(claim, out var tenantId))
        {
            return tenantId;
        }

        foreach (var headerName in new[] { "X-Tenant-Id", "X-Tenant" })
        {
            if (httpContext.Request.Headers.TryGetValue(headerName, out var headerValue) &&
                Guid.TryParse(headerValue.ToString(), out tenantId))
            {
                return tenantId;
            }
        }

        if (httpContext.Request.Headers.TryGetValue("X-Api-Key", out var apiKeyHeader))
        {
            var apiKey = apiKeyHeader.ToString();
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                var tenantFromApiKey = _context.Tenants
                    .AsNoTracking()
                    .Where(t => t.Slug == apiKey.ToLowerInvariant())
                    .Select(t => (Guid?)t.Id)
                    .FirstOrDefault();
                if (tenantFromApiKey.HasValue)
                {
                    return tenantFromApiKey;
                }
            }
        }

        var slug = ResolveTenantSlug(httpContext);
        if (!string.IsNullOrWhiteSpace(slug))
        {
            return _context.Tenants
                .AsNoTracking()
                .Where(t => t.Slug == slug)
                .Select(t => (Guid?)t.Id)
                .FirstOrDefault();
        }

        return null;
    }

    public string? ResolveTenantSlug(HttpContext? httpContext)
    {
        httpContext ??= _httpContextAccessor.HttpContext;
        if (httpContext is null) return null;

        var host = httpContext.Request.Host.Host;
        var parts = host.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 3 && !string.Equals(parts[0], "www", StringComparison.OrdinalIgnoreCase))
        {
            return parts[0].ToLowerInvariant();
        }

        if (httpContext.Request.Headers.TryGetValue("X-Tenant-Slug", out var slugHeader))
        {
            var slug = slugHeader.ToString();
            return string.IsNullOrWhiteSpace(slug) ? null : slug.ToLowerInvariant();
        }

        return null;
    }
}
