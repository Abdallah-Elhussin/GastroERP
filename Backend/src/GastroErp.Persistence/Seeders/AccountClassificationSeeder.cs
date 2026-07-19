using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Entities.Finance;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Persistence.Seeders;

/// <summary>Seed main + detail account classifications before Chart of Accounts.</summary>
public sealed class AccountClassificationSeeder : IDataSeeder
{
    private readonly ILogger<AccountClassificationSeeder> _logger;
    public AccountClassificationSeeder(ILogger<AccountClassificationSeeder> logger) => _logger = logger;
    public int Order => 18;

    public async Task SeedAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct = default)
    {
        if (await context.AccountMainClassifications.AnyAsync(m => m.TenantId == tenantId, ct))
            return;

        var assets = AddMain(context, tenantId, "assets", "الأصول", "Assets", AccountType.Asset, 1);
        var liabilities = AddMain(context, tenantId, "liabilities", "الخصوم", "Liabilities", AccountType.Liability, 2);
        var equity = AddMain(context, tenantId, "equity", "حقوق الملكية", "Equity", AccountType.Equity, 3);
        var revenue = AddMain(context, tenantId, "revenue", "الإيرادات", "Revenue", AccountType.Revenue, 4);
        var expenses = AddMain(context, tenantId, "expenses", "المصروفات", "Expenses", AccountType.Expense, 5);
        await context.SaveChangesAsync(ct);

        var n = 1;
        void Add(AccountMainClassification main, string code, string ar, string en)
        {
            context.AccountClassifications.Add(AccountClassification.Create(
                tenantId, n, code, ar, en, main.Id, n, isDefault: true, isSystem: true));
            n++;
        }

        Add(assets, "cash", "النقدية", "Cash");
        Add(assets, "bank", "البنوك", "Banks");
        Add(assets, "receivable", "العملاء", "Accounts Receivable");
        Add(assets, "inventory", "المخزون", "Inventory");
        Add(assets, "fixed_asset", "الأصول الثابتة", "Fixed Assets");
        Add(assets, "prepaid", "المصروفات المقدمة", "Prepaid Expenses");

        Add(liabilities, "payable", "الموردون", "Accounts Payable");
        Add(liabilities, "salaries_payable", "الرواتب المستحقة", "Salaries Payable");
        Add(liabilities, "vat", "ضريبة القيمة المضافة", "VAT");
        Add(liabilities, "loans", "القروض", "Loans");

        Add(equity, "capital", "رأس المال", "Capital");
        Add(equity, "retained_earnings", "الأرباح المحتجزة", "Retained Earnings");

        Add(revenue, "food_sales", "مبيعات الطعام", "Food Sales");
        Add(revenue, "beverage_sales", "مبيعات المشروبات", "Beverage Sales");
        Add(revenue, "delivery_revenue", "إيرادات التوصيل", "Delivery Revenue");
        Add(revenue, "other_revenue", "إيرادات أخرى", "Other Revenue");

        Add(expenses, "food_cogs", "تكلفة الأغذية", "Food COGS");
        Add(expenses, "beverage_cogs", "تكلفة المشروبات", "Beverage COGS");
        Add(expenses, "salaries", "الرواتب", "Salaries");
        Add(expenses, "rent", "الإيجارات", "Rent");
        Add(expenses, "electricity", "الكهرباء", "Electricity");
        Add(expenses, "water", "المياه", "Water");
        Add(expenses, "gas", "الغاز", "Gas");
        Add(expenses, "maintenance", "الصيانة", "Maintenance");
        Add(expenses, "marketing", "التسويق", "Marketing");
        Add(expenses, "admin_expense", "المصروفات الإدارية", "Administrative Expenses");
        Add(expenses, "delivery_expense", "مصروفات التوصيل", "Delivery Expenses");

        await context.SaveChangesAsync(ct);
        _logger.LogInformation("Account classifications seeded for tenant {TenantId}", tenantId);
    }

    private static AccountMainClassification AddMain(
        IApplicationDbContext ctx, Guid tenantId, string code, string ar, string en, AccountType type, int sort)
    {
        var main = AccountMainClassification.Create(tenantId, code, ar, en, type, sort);
        ctx.AccountMainClassifications.Add(main);
        return main;
    }
}
