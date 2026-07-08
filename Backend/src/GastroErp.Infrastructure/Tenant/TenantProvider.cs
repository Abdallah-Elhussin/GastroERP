using GastroErp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace GastroErp.Infrastructure.Tenant;

/// <summary>
/// مزود بيانات المستأجر (Tenant Provider)
/// يعتمد على الـ JWT Token أولاً، ثم الهيدر "X-Tenant-Id"
/// </summary>
public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return null;

            // 1. Try get from Claims (JWT)
            var claimValue = httpContext.User?.FindFirst("TenantId")?.Value;
            if (Guid.TryParse(claimValue, out var tenantIdFromClaim))
            {
                return tenantIdFromClaim;
            }

            // 2. Try get from Header
            foreach (var headerName in new[] { "X-Tenant-Id", "X-Tenant" })
            {
                if (httpContext.Request.Headers.TryGetValue(headerName, out var tenantIdHeader) &&
                    Guid.TryParse(tenantIdHeader.ToString(), out var tenantIdFromHeader))
                {
                    return tenantIdFromHeader;
                }
            }

            return null;
        }
    }
}
