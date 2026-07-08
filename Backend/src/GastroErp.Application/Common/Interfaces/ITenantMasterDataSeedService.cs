namespace GastroErp.Application.Common.Interfaces;

public interface ITenantMasterDataSeedService
{
    Task SeedAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
