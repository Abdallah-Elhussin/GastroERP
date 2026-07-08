namespace GastroErp.Presentation.Resolution;

public interface ITenantResolver
{
    Guid? ResolveTenantId();
}

public class TenantResolver : ITenantResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantResolver(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? ResolveTenantId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return null;

        // 1. JWT Claims
        var tenantClaim = context.User.Claims.FirstOrDefault(c => c.Type == "TenantId")?.Value;
        if (Guid.TryParse(tenantClaim, out var tenantId))
        {
            return tenantId;
        }

        // 2. X-Tenant Header
        if (context.Request.Headers.TryGetValue("X-Tenant", out var tenantHeader))
        {
            if (Guid.TryParse(tenantHeader.ToString(), out tenantId))
            {
                return tenantId;
            }
        }

        // 3. SubDomain (Future)

        return null;
    }
}
