namespace GastroErp.Application.Common.Interfaces.Platform;

public interface ITenantContext
{
    Guid? TenantId { get; }
    string? TenantSlug { get; }
    string? ResolutionSource { get; }
    void SetTenant(Guid tenantId, string? slug = null, string? source = null);
    void Clear();
}
