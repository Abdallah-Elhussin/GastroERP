using GastroErp.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Persistence.Seeders;

public sealed class TenantMasterDataSeeder
{
    private readonly IEnumerable<IDataSeeder> _seeders;
    private readonly ILogger<TenantMasterDataSeeder> _logger;

    public TenantMasterDataSeeder(IEnumerable<IDataSeeder> seeders, ILogger<TenantMasterDataSeeder> logger)
        => (_seeders, _logger) = (seeders, logger);

    public async Task SeedTenantAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct = default)
    {
        foreach (var seeder in _seeders.OrderBy(s => s.Order))
        {
            await seeder.SeedAsync(tenantId, context, ct);
        }
        _logger.LogInformation("All master data seeders completed for tenant {TenantId}", tenantId);
    }

    public async Task SeedAllTenantsAsync(IApplicationDbContext context, CancellationToken ct = default)
    {
        var tenantIds = await context.Tenants.Select(t => t.Id).ToListAsync(ct);
        foreach (var tenantId in tenantIds)
            await SeedTenantAsync(tenantId, context, ct);
    }
}
