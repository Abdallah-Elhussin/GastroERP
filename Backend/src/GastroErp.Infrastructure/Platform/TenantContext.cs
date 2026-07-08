namespace GastroErp.Infrastructure.Platform;

using GastroErp.Application.Common.Interfaces.Platform;

public sealed class TenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }
    public string? TenantSlug { get; private set; }
    public string? ResolutionSource { get; private set; }

    public void SetTenant(Guid tenantId, string? slug = null, string? source = null)
    {
        TenantId = tenantId;
        TenantSlug = slug;
        ResolutionSource = source;
    }

    public void Clear()
    {
        TenantId = null;
        TenantSlug = null;
        ResolutionSource = null;
    }
}
