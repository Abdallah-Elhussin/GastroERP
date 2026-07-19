using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Entities.Finance;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Persistence.Seeders;

/// <summary>صناديق نقدية افتراضية مرتبطة بحسابات نقدية في الدليل.</summary>
public sealed class CashBoxSeeder : IDataSeeder
{
    private readonly ILogger<CashBoxSeeder> _logger;
    public CashBoxSeeder(ILogger<CashBoxSeeder> logger) => _logger = logger;
    public int Order => 25;

    public async Task SeedAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct = default)
    {
        if (await context.CashBoxes.AnyAsync(b => b.TenantId == tenantId, ct))
            return;

        var company = await context.Companies.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .Select(c => new { c.Id })
            .FirstOrDefaultAsync(ct);
        var branch = await context.Branches.AsNoTracking()
            .Where(b => b.TenantId == tenantId)
            .Select(b => new { b.Id, b.CompanyId })
            .FirstOrDefaultAsync(ct);
        var sar = await context.Currencies.AsNoTracking()
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Code == "SAR", ct);

        if (company is null || branch is null || sar is null)
        {
            _logger.LogWarning("Skipping cash box seed for tenant {TenantId}: company/branch/SAR missing.", tenantId);
            return;
        }

        var cashParent = await context.ChartOfAccounts.AsNoTracking()
            .Where(a => a.TenantId == tenantId && (a.AccountNumber == "1100" || a.NameEn == "Cash & Bank"))
            .Select(a => new { a.Id })
            .FirstOrDefaultAsync(ct)
            ?? await context.ChartOfAccounts.AsNoTracking()
                .Where(a => a.TenantId == tenantId && a.AccountType == AccountType.Asset && a.IsSummaryAccount)
                .OrderBy(a => a.AccountNumber)
                .Select(a => new { a.Id })
                .FirstOrDefaultAsync(ct);

        var cashClassId = await context.AccountClassifications.AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.Code == "cash")
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(ct);

        var boxes = new (string Ar, string En, string AccNo, string Location)[]
        {
            ("صندوق المطعم الرئيسي", "Main Restaurant Cash", "1110", "صالة المطعم"),
            ("صندوق الكاشير", "Cashier Cash Box", "1111", "نقطة البيع")
        };

        var n = 1;
        var companyId = branch.CompanyId != Guid.Empty ? branch.CompanyId : company.Id;
        foreach (var item in boxes)
        {
            var gl = await context.ChartOfAccounts
                .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.AccountNumber == item.AccNo, ct);
            if (gl is null)
            {
                gl = ChartOfAccount.Create(
                    tenantId, item.AccNo, item.Ar, AccountType.Asset, AccountCategory.CurrentAsset,
                    isPostingAllowed: true, isSummaryAccount: false,
                    parentAccountId: cashParent?.Id, nameEn: item.En,
                    currency: "SAR", sortOrder: n, isSystemAccount: true, accountClassificationId: cashClassId);
                context.ChartOfAccounts.Add(gl);
                await context.SaveChangesAsync(ct);
            }

            var box = CashBox.Create(
                tenantId, n, $"CASH-{n:D4}", item.Ar, companyId, branch.Id, gl.Id, sar.Id,
                item.En, item.Location, openingBalance: 0, sortOrder: n, isSystem: true);
            box.SetOperatingFlags(true, true, true, true, true, requireShiftBeforeUse: true, allowNegativeBalance: false);
            box.SetLimits(0, 50_000);
            context.CashBoxes.Add(box);
            n++;
        }

        await context.SaveChangesAsync(ct);
        _logger.LogInformation("Cash boxes seeded for tenant {TenantId}", tenantId);
    }
}
