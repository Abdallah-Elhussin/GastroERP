using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Entities.Finance;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Persistence.Seeders;

/// <summary>بنوك سعودية افتراضية مربوطة بحسابات دليل بنكية فرعية.</summary>
public sealed class BankSeeder : IDataSeeder
{
    private readonly ILogger<BankSeeder> _logger;
    public BankSeeder(ILogger<BankSeeder> logger) => _logger = logger;
    public int Order => 24;

    public async Task SeedAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct = default)
    {
        if (await context.Banks.AnyAsync(b => b.TenantId == tenantId, ct))
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
            _logger.LogWarning("Skipping bank seed for tenant {TenantId}: company/branch/SAR missing.", tenantId);
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

        var bankClassId = await context.AccountClassifications.AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.Code == "bank")
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(ct);

        var banks = new (string Code, string Ar, string En, string AccNo)[]
        {
            ("SNB", "البنك الأهلي السعودي", "Saudi National Bank", "1101"),
            ("RJHI", "مصرف الراجحي", "Al Rajhi Bank", "1102"),
            ("RIBL", "بنك الرياض", "Riyad Bank", "1103"),
            ("ALBI", "بنك البلاد", "Bank AlBilad", "1104"),
            ("INMA", "مصرف الإنماء", "Alinma Bank", "1105")
        };

        var n = 1;
        foreach (var item in banks)
        {
            var gl = await context.ChartOfAccounts
                .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.AccountNumber == item.AccNo, ct);
            if (gl is null)
            {
                gl = ChartOfAccount.Create(
                    tenantId, item.AccNo, $"بنك — {item.Ar}", AccountType.Asset, AccountCategory.CurrentAsset,
                    isPostingAllowed: true, isSummaryAccount: false,
                    parentAccountId: cashParent?.Id, nameEn: $"Bank — {item.En}",
                    currency: "SAR", sortOrder: n, isSystemAccount: true, accountClassificationId: bankClassId);
                context.ChartOfAccounts.Add(gl);
                await context.SaveChangesAsync(ct);
            }

            var bank = Bank.Create(
                tenantId, n, item.Ar, branch.CompanyId != Guid.Empty ? branch.CompanyId : company.Id,
                branch.Id, gl.Id, sar.Id, item.En, item.Code, sortOrder: n, isSystem: true);

            bank.ReplaceAccounts(
            [
                BankAccountDetail.Create(
                    bank.Id, sar.Id, $"SA{item.Code}0001", null,
                    minBalance: 0, maxBalance: null, isDefault: true, sortOrder: 0)
            ]);

            context.Banks.Add(bank);
            n++;
        }

        await context.SaveChangesAsync(ct);
        _logger.LogInformation("Banks seeded for tenant {TenantId}", tenantId);
    }
}
