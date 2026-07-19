using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Entities.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Persistence.Seeders;

/// <summary>ملف ضريبي افتراضي للشركة من بيانات الشركة القائمة.</summary>
public sealed class TaxRegistrationSeeder : IDataSeeder
{
    private readonly ILogger<TaxRegistrationSeeder> _logger;
    public TaxRegistrationSeeder(ILogger<TaxRegistrationSeeder> logger) => _logger = logger;
    public int Order => 26;

    public async Task SeedAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct = default)
    {
        if (await context.TaxRegistrationProfiles.AnyAsync(p => p.TenantId == tenantId, ct))
            return;

        var company = await context.Companies.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .Select(c => new { c.Id, c.TaxNumber, Vat = c.VatNumber != null ? c.VatNumber.Value : (string?)null })
            .FirstOrDefaultAsync(ct);
        var branch = await context.Branches.AsNoTracking()
            .Where(b => b.TenantId == tenantId)
            .Select(b => new { b.Id, b.CompanyId })
            .FirstOrDefaultAsync(ct);

        if (company is null)
        {
            _logger.LogWarning("Skipping tax registration seed for tenant {TenantId}: no company.", tenantId);
            return;
        }

        var vat = !string.IsNullOrWhiteSpace(company.Vat)
            ? company.Vat!
            : !string.IsNullOrWhiteSpace(company.TaxNumber)
                ? company.TaxNumber
                : "300000000000003";

        var companyId = branch?.CompanyId is Guid cid && cid != Guid.Empty ? cid : company.Id;

        var profile = TaxRegistrationProfile.Create(
            tenantId, 1, companyId, vat,
            branchId: null,
            taxOffice: "الرياض",
            taxpayerType: TaxpayerType.Company,
            activityCode: "561001",
            activityNameAr: "أنشطة المطاعم",
            activityNameEn: "Restaurants activities",
            defaultTaxRate: 15m,
            registrationDate: DateOnly.FromDateTime(DateTime.UtcNow),
            sortOrder: 1,
            isSystem: true);

        context.TaxRegistrationProfiles.Add(profile);
        await context.SaveChangesAsync(ct);
        _logger.LogInformation("Tax registration profile seeded for tenant {TenantId}", tenantId);
    }
}
