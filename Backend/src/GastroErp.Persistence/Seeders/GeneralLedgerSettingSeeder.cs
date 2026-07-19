using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Entities.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Persistence.Seeders;

/// <summary>إعدادات أستاذ عام افتراضية لأول شركة/فرع.</summary>
public sealed class GeneralLedgerSettingSeeder : IDataSeeder
{
    private readonly ILogger<GeneralLedgerSettingSeeder> _logger;
    public GeneralLedgerSettingSeeder(ILogger<GeneralLedgerSettingSeeder> logger) => _logger = logger;
    public int Order => 27;

    public async Task SeedAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct = default)
    {
        if (await context.GeneralLedgerSettings.AnyAsync(s => s.TenantId == tenantId, ct))
            return;

        var company = await context.Companies.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .Select(c => new { c.Id })
            .FirstOrDefaultAsync(ct);
        var branch = await context.Branches.AsNoTracking()
            .Where(b => b.TenantId == tenantId)
            .Select(b => new { b.Id, b.CompanyId })
            .FirstOrDefaultAsync(ct);

        if (company is null || branch is null)
        {
            _logger.LogWarning("Skipping GL settings seed for tenant {TenantId}: company/branch missing.", tenantId);
            return;
        }

        var companyId = branch.CompanyId != Guid.Empty ? branch.CompanyId : company.Id;
        var setting = GeneralLedgerSetting.Create(
            tenantId, 1, companyId, branch.Id, isSystem: true);

        context.GeneralLedgerSettings.Add(setting);
        await context.SaveChangesAsync(ct);
        _logger.LogInformation("General ledger settings seeded for tenant {TenantId}", tenantId);
    }
}
