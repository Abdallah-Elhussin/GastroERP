using GastroErp.Application.Features.Finance.Services;
using GastroErp.Domain.Entities.Finance;
using GastroErp.Domain.Enums;
using GastroErp.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Persistence.Seeders;

/// <summary>دليل حسابات مطعم سعودي — متوافق مع StandardAccountCodes.</summary>
public sealed class ChartOfAccountsSeeder : IDataSeeder
{
    private readonly ILogger<ChartOfAccountsSeeder> _logger;

    public ChartOfAccountsSeeder(ILogger<ChartOfAccountsSeeder> logger) => _logger = logger;

    public int Order => 20;

    public async Task SeedAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct = default)
    {
        if (await context.ChartOfAccounts.AnyAsync(a => a.TenantId == tenantId, ct))
            return;

        var assets = await Add(context, tenantId, "1000", "الأصول", "Assets", AccountType.Asset, AccountCategory.CurrentAsset, true, null, 1);
        await Add(context, tenantId, StandardAccountCodes.Cash, "النقدية والبنوك", "Cash & Bank", AccountType.Asset, AccountCategory.CurrentAsset, false, assets.Id, 2);
        await Add(context, tenantId, StandardAccountCodes.AccountsReceivable, "ذمم مدينة", "Accounts Receivable", AccountType.Asset, AccountCategory.CurrentAsset, false, assets.Id, 3);
        await Add(context, tenantId, "1300", "المخزون", "Inventory", AccountType.Asset, AccountCategory.CurrentAsset, false, assets.Id, 4);
        await Add(context, tenantId, "1400", "مصروفات مدفوعة مقدماً", "Prepaid Expenses", AccountType.Asset, AccountCategory.CurrentAsset, false, assets.Id, 5);

        var liabilities = await Add(context, tenantId, "2000", "الخصوم", "Liabilities", AccountType.Liability, AccountCategory.CurrentLiability, true, null, 10);
        await Add(context, tenantId, StandardAccountCodes.VatPayable, "ضريبة القيمة المضافة", "VAT Payable", AccountType.Liability, AccountCategory.CurrentLiability, false, liabilities.Id, 11);
        await Add(context, tenantId, StandardAccountCodes.SalariesPayable, "رواتب مستحقة", "Salaries Payable", AccountType.Liability, AccountCategory.CurrentLiability, false, liabilities.Id, 12);
        await Add(context, tenantId, "2300", "ذمم دائنة", "Accounts Payable", AccountType.Liability, AccountCategory.CurrentLiability, false, liabilities.Id, 13);

        var equity = await Add(context, tenantId, "3000", "حقوق الملكية", "Equity", AccountType.Equity, AccountCategory.Equity, true, null, 20);
        await Add(context, tenantId, "3100", "رأس المال", "Capital", AccountType.Equity, AccountCategory.Equity, false, equity.Id, 21);
        await Add(context, tenantId, "3200", "الأرباح المحتجزة", "Retained Earnings", AccountType.Equity, AccountCategory.Equity, false, equity.Id, 22);

        await Add(context, tenantId, StandardAccountCodes.SalesRevenue, "إيرادات المبيعات", "Sales Revenue", AccountType.Revenue, AccountCategory.OperatingRevenue, false, null, 30);
        await Add(context, tenantId, "4100", "إيرادات التوصيل", "Delivery Revenue", AccountType.Revenue, AccountCategory.OperatingRevenue, false, null, 31);
        await Add(context, tenantId, "4200", "إيرادات أخرى", "Other Revenue", AccountType.Revenue, AccountCategory.OtherRevenue, false, null, 32);

        var cogs = await Add(context, tenantId, "5000", "تكلفة المبيعات", "Cost of Goods Sold", AccountType.Expense, AccountCategory.CostOfGoodsSold, true, null, 40);
        await Add(context, tenantId, "5010", "تكلفة المواد الغذائية", "Food COGS", AccountType.Expense, AccountCategory.CostOfGoodsSold, false, cogs.Id, 41);
        await Add(context, tenantId, "5020", "تكلفة المشروبات", "Beverage COGS", AccountType.Expense, AccountCategory.CostOfGoodsSold, false, cogs.Id, 42);

        var expenses = await Add(context, tenantId, "6000", "المصروفات التشغيلية", "Operating Expenses", AccountType.Expense, AccountCategory.OperatingExpense, true, null, 50);
        await Add(context, tenantId, StandardAccountCodes.SalaryExpense, "مصروف الرواتب", "Salary Expense", AccountType.Expense, AccountCategory.OperatingExpense, false, expenses.Id, 51);
        await Add(context, tenantId, "5200", "الإيجار", "Rent Expense", AccountType.Expense, AccountCategory.OperatingExpense, false, expenses.Id, 52);
        await Add(context, tenantId, "5300", "المرافق", "Utilities", AccountType.Expense, AccountCategory.OperatingExpense, false, expenses.Id, 53);
        await Add(context, tenantId, "5400", "التسويق", "Marketing", AccountType.Expense, AccountCategory.OperatingExpense, false, expenses.Id, 54);
        await Add(context, tenantId, "5500", "الصيانة", "Maintenance", AccountType.Expense, AccountCategory.OperatingExpense, false, expenses.Id, 55);

        await context.SaveChangesAsync(ct);
        _logger.LogInformation("Chart of accounts seeded for tenant {TenantId}", tenantId);
    }

    private static async Task<ChartOfAccount> Add(
        IApplicationDbContext ctx, Guid tenantId, string number, string nameAr, string? nameEn,
        AccountType type, AccountCategory category, bool isSummary, Guid? parentId, int sort)
    {
        var exists = await ctx.ChartOfAccounts.AnyAsync(a => a.TenantId == tenantId && a.AccountNumber == number);
        if (exists)
            return (await ctx.ChartOfAccounts.FirstAsync(a => a.TenantId == tenantId && a.AccountNumber == number))!;

        var account = ChartOfAccount.Create(tenantId, number, nameAr, type, category,
            isPostingAllowed: !isSummary, isSummaryAccount: isSummary, parentAccountId: parentId,
            nameEn: nameEn, sortOrder: sort);
        ctx.ChartOfAccounts.Add(account);
        return account;
    }
}
