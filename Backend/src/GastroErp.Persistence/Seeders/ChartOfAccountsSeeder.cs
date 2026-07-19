using GastroErp.Application.Features.Finance.Services;
using GastroErp.Domain.Entities.Finance;
using GastroErp.Domain.Enums;
using GastroErp.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Persistence.Seeders;

/// <summary>دليل حسابات مطعم سعودي — يعلّم الحسابات الأساسية كـ System ويُهيئ AccountingSettings.</summary>
public sealed class ChartOfAccountsSeeder : IDataSeeder
{
    private readonly ILogger<ChartOfAccountsSeeder> _logger;
    private readonly AccountClassificationSeeder _classificationSeeder;

    public ChartOfAccountsSeeder(
        ILogger<ChartOfAccountsSeeder> logger,
        AccountClassificationSeeder classificationSeeder)
    {
        _logger = logger;
        _classificationSeeder = classificationSeeder;
    }

    public int Order => 20;

    public async Task SeedAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct = default)
    {
        await _classificationSeeder.SeedAsync(tenantId, context, ct);

        if (await context.ChartOfAccounts.AnyAsync(a => a.TenantId == tenantId, ct))
        {
            await EnsureSettingsAsync(tenantId, context, ct);
            return;
        }

        var assets = await Add(context, tenantId, "1000", "الأصول", "Assets", AccountType.Asset, AccountCategory.CurrentAsset, true, null, 1, system: true);
        var cash = await Add(context, tenantId, StandardAccountCodes.Cash, "النقدية والبنوك", "Cash & Bank", AccountType.Asset, AccountCategory.CurrentAsset, false, assets.Id, 2, system: true);
        var ar = await Add(context, tenantId, StandardAccountCodes.AccountsReceivable, "ذمم مدينة", "Accounts Receivable", AccountType.Asset, AccountCategory.CurrentAsset, false, assets.Id, 3, system: true);
        var inventory = await Add(context, tenantId, "1300", "المخزون", "Inventory", AccountType.Asset, AccountCategory.CurrentAsset, false, assets.Id, 4, system: true);
        await Add(context, tenantId, "1400", "مصروفات مدفوعة مقدماً", "Prepaid Expenses", AccountType.Asset, AccountCategory.CurrentAsset, false, assets.Id, 5);

        var liabilities = await Add(context, tenantId, "2000", "الخصوم", "Liabilities", AccountType.Liability, AccountCategory.CurrentLiability, true, null, 10, system: true);
        var vat = await Add(context, tenantId, StandardAccountCodes.VatPayable, "ضريبة القيمة المضافة", "VAT Payable", AccountType.Liability, AccountCategory.CurrentLiability, false, liabilities.Id, 11, system: true);
        var salariesPayable = await Add(context, tenantId, StandardAccountCodes.SalariesPayable, "رواتب مستحقة", "Salaries Payable", AccountType.Liability, AccountCategory.CurrentLiability, false, liabilities.Id, 12, system: true);
        var ap = await Add(context, tenantId, "2300", "ذمم دائنة", "Accounts Payable", AccountType.Liability, AccountCategory.CurrentLiability, false, liabilities.Id, 13, system: true);

        var equity = await Add(context, tenantId, "3000", "حقوق الملكية", "Equity", AccountType.Equity, AccountCategory.Equity, true, null, 20, system: true);
        await Add(context, tenantId, "3100", "رأس المال", "Capital", AccountType.Equity, AccountCategory.Equity, false, equity.Id, 21);
        var re = await Add(context, tenantId, "3200", "الأرباح المحتجزة", "Retained Earnings", AccountType.Equity, AccountCategory.Equity, false, equity.Id, 22, system: true);

        var sales = await Add(context, tenantId, StandardAccountCodes.SalesRevenue, "إيرادات المبيعات", "Sales Revenue", AccountType.Revenue, AccountCategory.OperatingRevenue, false, null, 30, system: true);
        var deliveryRev = await Add(context, tenantId, "4100", "إيرادات التوصيل", "Delivery Revenue", AccountType.Revenue, AccountCategory.OperatingRevenue, false, null, 31, system: true);
        await Add(context, tenantId, "4200", "إيرادات أخرى", "Other Revenue", AccountType.Revenue, AccountCategory.OtherRevenue, false, null, 32);

        var cogs = await Add(context, tenantId, "5000", "تكلفة المبيعات", "Cost of Goods Sold", AccountType.Expense, AccountCategory.CostOfGoodsSold, true, null, 40, system: true);
        var foodCogs = await Add(context, tenantId, "5010", "تكلفة المواد الغذائية", "Food COGS", AccountType.Expense, AccountCategory.CostOfGoodsSold, false, cogs.Id, 41, system: true);
        await Add(context, tenantId, "5020", "تكلفة المشروبات", "Beverage COGS", AccountType.Expense, AccountCategory.CostOfGoodsSold, false, cogs.Id, 42);

        var expenses = await Add(context, tenantId, "6000", "المصروفات التشغيلية", "Operating Expenses", AccountType.Expense, AccountCategory.OperatingExpense, true, null, 50, system: true);
        var salary = await Add(context, tenantId, StandardAccountCodes.SalaryExpense, "مصروف الرواتب", "Salary Expense", AccountType.Expense, AccountCategory.OperatingExpense, false, expenses.Id, 51, system: true);
        await Add(context, tenantId, "5200", "الإيجار", "Rent Expense", AccountType.Expense, AccountCategory.OperatingExpense, false, expenses.Id, 52);
        await Add(context, tenantId, "5300", "المرافق", "Utilities", AccountType.Expense, AccountCategory.OperatingExpense, false, expenses.Id, 53);
        await Add(context, tenantId, "5400", "التسويق", "Marketing", AccountType.Expense, AccountCategory.OperatingExpense, false, expenses.Id, 54);
        await Add(context, tenantId, "5500", "الصيانة", "Maintenance", AccountType.Expense, AccountCategory.OperatingExpense, false, expenses.Id, 55);

        await context.SaveChangesAsync(ct);

        var settings = AccountingSettings.CreateDefault(tenantId);
        settings.UpdateAccountMappings(
            cash: cash.Id, bank: cash.Id, inventory: inventory.Id, cogs: foodCogs.Id, salesRevenue: sales.Id, purchase: ap.Id,
            ar: ar.Id, ap: ap.Id, vatInput: vat.Id, vatOutput: vat.Id, discount: null, roundOff: null,
            openingBalance: re.Id, retainedEarnings: re.Id, payrollExpense: salary.Id, payrollLiability: salariesPayable.Id,
            productionVariance: null, inventoryAdjustment: null, waste: null, deliveryRevenue: deliveryRev.Id,
            deliveryExpense: null, kitchenConsumption: null, customerAdvances: null, supplierAdvances: null,
            exchangeDifference: null);
        context.AccountingSettings.Add(settings);
        await context.SaveChangesAsync(ct);

        _logger.LogInformation("Chart of accounts + accounting settings seeded for tenant {TenantId}", tenantId);
    }

    private static async Task EnsureSettingsAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct)
    {
        if (await context.AccountingSettings.AnyAsync(s => s.TenantId == tenantId, ct))
            return;

        var accounts = await context.ChartOfAccounts.AsNoTracking()
            .Where(a => a.TenantId == tenantId)
            .ToDictionaryAsync(a => a.AccountNumber, a => a.Id, ct);

        Guid? IdOf(string code) => accounts.TryGetValue(code, out var id) ? id : null;

        var settings = AccountingSettings.CreateDefault(tenantId);
        settings.UpdateAccountMappings(
            cash: IdOf(StandardAccountCodes.Cash), bank: IdOf(StandardAccountCodes.Cash), inventory: IdOf("1300"),
            cogs: IdOf("5010"), salesRevenue: IdOf(StandardAccountCodes.SalesRevenue), purchase: IdOf("2300"),
            ar: IdOf(StandardAccountCodes.AccountsReceivable), ap: IdOf("2300"),
            vatInput: IdOf(StandardAccountCodes.VatPayable), vatOutput: IdOf(StandardAccountCodes.VatPayable),
            discount: null, roundOff: null, openingBalance: IdOf("3200"), retainedEarnings: IdOf("3200"),
            payrollExpense: IdOf(StandardAccountCodes.SalaryExpense), payrollLiability: IdOf(StandardAccountCodes.SalariesPayable),
            productionVariance: null, inventoryAdjustment: null, waste: null, deliveryRevenue: IdOf("4100"),
            deliveryExpense: null, kitchenConsumption: null, customerAdvances: null, supplierAdvances: null,
            exchangeDifference: null);

        // Mark known system codes
        var systemCodes = new[]
        {
            "1000", StandardAccountCodes.Cash, StandardAccountCodes.AccountsReceivable, "1300",
            "2000", StandardAccountCodes.VatPayable, StandardAccountCodes.SalariesPayable, "2300",
            "3000", "3200", StandardAccountCodes.SalesRevenue, "4100", "5000", "5010", "6000",
            StandardAccountCodes.SalaryExpense
        };
        var toMark = await context.ChartOfAccounts
            .Where(a => a.TenantId == tenantId && systemCodes.Contains(a.AccountNumber) && !a.IsSystemAccount)
            .ToListAsync(ct);
        foreach (var a in toMark)
            a.MarkAsSystemAccount(true);

        context.AccountingSettings.Add(settings);
        await context.SaveChangesAsync(ct);
    }

    private static async Task<ChartOfAccount> Add(
        IApplicationDbContext ctx, Guid tenantId, string number, string nameAr, string? nameEn,
        AccountType type, AccountCategory category, bool isSummary, Guid? parentId, int sort, bool system = false)
    {
        var exists = await ctx.ChartOfAccounts.AnyAsync(a => a.TenantId == tenantId && a.AccountNumber == number);
        if (exists)
            return (await ctx.ChartOfAccounts.FirstAsync(a => a.TenantId == tenantId && a.AccountNumber == number))!;

        var account = ChartOfAccount.Create(tenantId, number, nameAr, type, category,
            isPostingAllowed: !isSummary, isSummaryAccount: isSummary, parentAccountId: parentId,
            nameEn: nameEn, sortOrder: sort, isSystemAccount: system);
        ctx.ChartOfAccounts.Add(account);
        return account;
    }
}
