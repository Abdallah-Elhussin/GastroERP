using GastroErp.Application.Common.Interfaces;

namespace GastroErp.Persistence.Seeders;

public sealed class TenantMasterDataSeedService : ITenantMasterDataSeedService
{
    private readonly TenantMasterDataSeeder _seeder;
    private readonly IApplicationDbContext _context;

    public TenantMasterDataSeedService(TenantMasterDataSeeder seeder, IApplicationDbContext context)
        => (_seeder, _context) = (seeder, context);

    public Task SeedAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => _seeder.SeedTenantAsync(tenantId, _context, cancellationToken);
}
