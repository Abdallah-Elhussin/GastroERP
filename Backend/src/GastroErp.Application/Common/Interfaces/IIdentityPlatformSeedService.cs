namespace GastroErp.Application.Common.Interfaces;

public interface IIdentityPlatformSeedService
{
    Task EnsureGlobalPermissionsAsync(CancellationToken cancellationToken = default);
    Task<Domain.Entities.Identity.Role> EnsureTenantRolesAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
