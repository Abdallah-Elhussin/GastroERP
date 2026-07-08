using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Application.Features.Finance.Services;
using GastroErp.Application.Features.Reporting.DTOs;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Reporting.Services;

public sealed class InventoryAnalyticsService : IInventoryAnalyticsService
{
    private readonly IApplicationDbContext _context;
    public InventoryAnalyticsService(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<StockBalanceDto>> GetStockBalanceAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var movements = _context.StockMovements.AsNoTracking().Where(m => m.TenantId == tenantId);
        var grouped = await movements
            .GroupBy(m => new { m.InventoryItemId, m.WarehouseId })
            .Select(g => new
            {
                g.Key.InventoryItemId, g.Key.WarehouseId,
                Qty = g.Sum(m => m.QuantityChange),
                AvgCost = g.Average(m => m.UnitCost)
            }).Where(x => x.Qty != 0).ToListAsync(ct);

        var itemIds = grouped.Select(g => g.InventoryItemId).Distinct().ToList();
        var whIds = grouped.Select(g => g.WarehouseId).Distinct().ToList();
        var items = await _context.InventoryItems.AsNoTracking()
            .Where(i => itemIds.Contains(i.Id)).ToDictionaryAsync(i => i.Id, ct);
        var warehouses = await _context.Warehouses.AsNoTracking()
            .Where(w => whIds.Contains(w.Id)).ToDictionaryAsync(w => w.Id, ct);

        return grouped.Select(g =>
        {
            items.TryGetValue(g.InventoryItemId, out var item);
            warehouses.TryGetValue(g.WarehouseId, out var wh);
            return new StockBalanceDto(g.InventoryItemId, item?.NameAr ?? "Unknown", item?.Sku,
                g.WarehouseId, wh?.NameAr ?? "Unknown", g.Qty, g.AvgCost, g.Qty * g.AvgCost);
        }).OrderByDescending(x => x.TotalValue).Take(500).ToList();
    }

    public async Task<StockValuationDto> GetStockValuationAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var balances = await GetStockBalanceAsync(tenantId, filter, ct);
        return new StockValuationDto(balances.Sum(b => b.TotalValue), balances.Count, balances.Take(20).ToList());
    }

    public async Task<IReadOnlyList<InventoryMovementDto>> GetInventoryMovementAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var (dateFrom, dateTo) = ReportQueryHelper.ResolveDateRange(filter);
        var movements = await (
            from m in _context.StockMovements.AsNoTracking()
            join t in _context.InventoryTransactions.AsNoTracking() on m.InventoryTransactionId equals t.Id
            join item in _context.InventoryItems.AsNoTracking() on m.InventoryItemId equals item.Id
            join wh in _context.Warehouses.AsNoTracking() on m.WarehouseId equals wh.Id
            where m.TenantId == tenantId
            where t.TransactionDate >= dateFrom
            where t.TransactionDate <= dateTo
            orderby t.TransactionDate descending
            select new InventoryMovementDto(t.TransactionDate, t.TransactionType.ToString(),
                item.NameAr, m.QuantityChange, m.UnitCost, wh.NameAr))
            .Take(500).ToListAsync(ct);
        return movements;
    }

    public async Task<IReadOnlyList<WasteAnalysisDto>> GetWasteAnalysisAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var (dateFrom, dateTo) = ReportQueryHelper.ResolveDateRange(filter);
        var wasteItems = await (
            from wi in _context.WasteItems.AsNoTracking()
            join wr in _context.WasteRecords.AsNoTracking() on wi.WasteRecordId equals wr.Id
            join reason in _context.WasteReasons.AsNoTracking() on wi.WasteReasonId equals reason.Id
            where wi.TenantId == tenantId
            where wr.WasteDate >= dateFrom
            where wr.WasteDate <= dateTo
            group wi by reason.NameAr into g
            select new WasteAnalysisDto(g.Key, g.Count(), g.Sum(x => x.Quantity), g.Sum(x => x.Quantity * x.UnitCost)))
            .ToListAsync(ct);
        return wasteItems;
    }

    public async Task<IReadOnlyList<AdjustmentAnalysisDto>> GetAdjustmentAnalysisAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var (from, to) = ReportQueryHelper.ResolveDateRange(filter);
        var adjustments = await _context.StockAdjustments.AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.CreatedAt >= from && a.CreatedAt <= to)
            .CountAsync(ct);
        return [new AdjustmentAnalysisDto("Stock Adjustments", adjustments, 0)];
    }

    public async Task<IReadOnlyList<ConsumptionReportDto>> GetConsumptionReportAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var (dateFrom, dateTo) = ReportQueryHelper.ResolveDateRange(filter);
        return await (
            from m in _context.StockMovements.AsNoTracking()
            join t in _context.InventoryTransactions.AsNoTracking() on m.InventoryTransactionId equals t.Id
            join item in _context.InventoryItems.AsNoTracking() on m.InventoryItemId equals item.Id
            where m.TenantId == tenantId
            where m.QuantityChange < 0
            where t.TransactionDate >= dateFrom
            where t.TransactionDate <= dateTo
            group m by new { m.InventoryItemId, item.NameAr } into g
            select new ConsumptionReportDto(g.Key.InventoryItemId, g.Key.NameAr,
                Math.Abs(g.Sum(x => x.QuantityChange)), g.Sum(x => Math.Abs(x.QuantityChange) * x.UnitCost)))
            .OrderByDescending(x => x.TotalCost).Take(100).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<RecipeCostDto>> GetRecipeCostReportAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var recipes = await _context.Recipes.AsNoTracking()
            .Include(r => r.Items)
            .Where(r => r.TenantId == tenantId && r.Status == RecipeStatus.Active)
            .Take(100).ToListAsync(ct);

        var productIds = recipes.Select(r => r.ProductId).ToList();
        var products = await _context.Products.AsNoTracking()
            .Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, ct);

        return recipes.Select(r =>
        {
            products.TryGetValue(r.ProductId, out var product);
            var cost = 0m;
            var price = product?.BasePrice ?? 0;
            var margin = price > 0 ? (price - cost) / price * 100 : 0;
            return new RecipeCostDto(r.ProductId, r.NameAr, cost, price, margin);
        }).ToList();
    }

    public async Task<IReadOnlyList<PurchaseAnalysisDto>> GetPurchaseAnalysisAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var (from, to) = ReportQueryHelper.ResolveDateRange(filter);
        var data = await _context.PurchaseOrders.AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.CreatedAt >= from && p.CreatedAt <= to)
            .GroupBy(p => p.SupplierId)
            .Select(g => new { SupplierId = g.Key, Count = g.Count(), Total = g.Sum(p => p.TotalAmount) })
            .ToListAsync(ct);
        var supplierIds = data.Select(d => d.SupplierId).ToList();
        var suppliers = await _context.Suppliers.AsNoTracking()
            .Where(s => supplierIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, s => s.NameAr, ct);
        return data.Select(d => new PurchaseAnalysisDto(
            d.SupplierId, suppliers.GetValueOrDefault(d.SupplierId, "Unknown"), d.Count, d.Total))
            .OrderByDescending(x => x.TotalAmount).Take(100).ToList();
    }

    public async Task<IReadOnlyList<SupplierPerformanceDto>> GetSupplierPerformanceAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var purchases = await GetPurchaseAnalysisAsync(tenantId, filter, ct);
        return purchases.Select(p => new SupplierPerformanceDto(
            p.SupplierId, p.SupplierName, p.OrderCount, p.TotalAmount, 100)).ToList();
    }
}

public sealed class CustomerAnalyticsService : ICustomerAnalyticsService
{
    private readonly IApplicationDbContext _context;
    public CustomerAnalyticsService(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<CustomerActivityDto>> GetCustomerActivityAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
        => await _context.Customers.AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.TotalOrders > 0)
            .OrderByDescending(c => c.LastVisit)
            .Take(200)
            .Select(c => new CustomerActivityDto(c.Id, c.FullName, c.TotalOrders, c.TotalSpending, c.LastVisit))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CustomerLtvDto>> GetCustomerLtvAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
        => await _context.Customers.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .OrderByDescending(c => c.TotalSpending)
            .Take(100)
            .Select(c => new CustomerLtvDto(c.Id, c.FullName, c.TotalSpending, c.TotalOrders, c.AverageTicket))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CustomerFrequencyDto>> GetCustomerFrequencyAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var customers = await _context.Customers.AsNoTracking()
            .Where(c => c.TenantId == tenantId).Select(c => c.TotalOrders).ToListAsync(ct);
        return
        [
            new("1 order", customers.Count(o => o == 1)),
            new("2-5 orders", customers.Count(o => o >= 2 && o <= 5)),
            new("6-10 orders", customers.Count(o => o >= 6 && o <= 10)),
            new("10+ orders", customers.Count(o => o > 10))
        ];
    }

    public async Task<LoyaltyPointsReportDto> GetLoyaltyPointsReportAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var (from, to) = ReportQueryHelper.ResolveDateRange(filter);
        var txns = _context.LoyaltyTransactions.AsNoTracking()
            .Where(t => t.TenantId == tenantId && t.CreatedAt >= from && t.CreatedAt <= to);
        var issued = await txns.Where(t => t.Type == LoyaltyTransactionType.Earn).SumAsync(t => t.Points, ct);
        var redeemed = await txns.Where(t => t.Type == LoyaltyTransactionType.Redeem).SumAsync(t => Math.Abs(t.Points), ct);
        var active = await _context.LoyaltyAccounts.AsNoTracking().CountAsync(a => a.TenantId == tenantId, ct);
        return new LoyaltyPointsReportDto(issued, redeemed, active);
    }

    public async Task<IReadOnlyList<CouponUsageDto>> GetCouponUsageAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
        => await _context.Coupons.AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .Select(c => new CouponUsageDto(c.Code, c.UsageLimit - c.RemainingUses, c.Value * (c.UsageLimit - c.RemainingUses)))
            .OrderByDescending(x => x.UsageCount).Take(100).ToListAsync(ct);

    public async Task<GiftCardUsageDto> GetGiftCardUsageAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var cards = _context.GiftCards.AsNoTracking().Where(g => g.TenantId == tenantId);
        var issued = await cards.CountAsync(ct);
        var redeemed = await cards.CountAsync(g => g.Status == GiftCardStatus.Depleted, ct);
        var balance = await cards.Where(g => g.Status == GiftCardStatus.Active).SumAsync(g => g.CurrentBalance, ct);
        return new GiftCardUsageDto(issued, redeemed, balance);
    }

    public async Task<IReadOnlyList<MembershipDistributionDto>> GetMembershipDistributionAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
        => await (
            from la in _context.LoyaltyAccounts.AsNoTracking()
            join tier in _context.MembershipTiers.AsNoTracking() on la.MembershipTierId equals tier.Id into tiers
            from tier in tiers.DefaultIfEmpty()
            where la.TenantId == tenantId
            group la by (tier != null ? tier.Name : "Standard") into g
            select new MembershipDistributionDto(g.Key, g.Count()))
            .ToListAsync(ct);
}

public sealed class FinancialAnalyticsService : IFinancialAnalyticsService
{
    private readonly IApplicationDbContext _context;
    private readonly ITrialBalanceService _trialBalance;
    private readonly IAccountBalanceService _accountBalance;

    public FinancialAnalyticsService(
        IApplicationDbContext context, ITrialBalanceService trialBalance, IAccountBalanceService accountBalance)
        => (_context, _trialBalance, _accountBalance) = (context, trialBalance, accountBalance);

    public async Task<BalanceSheetDto> GetBalanceSheetAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var tb = await _trialBalance.GetTrialBalanceAsync(tenantId, new TrialBalanceFilterDto(filter.ToDate), ct);
        var accounts = await _context.ChartOfAccounts.AsNoTracking()
            .Where(a => a.TenantId == tenantId).ToDictionaryAsync(a => a.Id, ct);

        var lines = tb.Select(l =>
        {
            accounts.TryGetValue(l.AccountId, out var acct);
            var amount = l.DebitBalance - l.CreditBalance;
            return new FinancialLineDto(l.AccountNumber, l.AccountName, acct?.AccountType.ToString() ?? "", amount);
        }).ToList();

        var assets = lines.Where(l => l.Category == AccountType.Asset.ToString()).Sum(l => l.Amount);
        var liabilities = lines.Where(l => l.Category == AccountType.Liability.ToString()).Sum(l => -l.Amount);
        var equity = lines.Where(l => l.Category == AccountType.Equity.ToString()).Sum(l => -l.Amount);
        return new BalanceSheetDto(assets, liabilities, equity, lines);
    }

    public async Task<IncomeStatementDto> GetIncomeStatementAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var tb = await _trialBalance.GetTrialBalanceAsync(tenantId, new TrialBalanceFilterDto(filter.ToDate), ct);
        var accountIds = tb.Select(t => t.AccountId).ToList();
        var accounts = await _context.ChartOfAccounts.AsNoTracking()
            .Where(a => accountIds.Contains(a.Id)).ToDictionaryAsync(a => a.Id, ct);

        decimal revenue = 0, cogs = 0, expenses = 0;
        foreach (var line in tb)
        {
            if (!accounts.TryGetValue(line.AccountId, out var acct)) continue;
            var net = line.CreditBalance - line.DebitBalance;
            if (acct.AccountType == AccountType.Revenue) revenue += net;
            else if (acct.AccountCategory == AccountCategory.CostOfGoodsSold) cogs += line.DebitBalance - line.CreditBalance;
            else if (acct.AccountType == AccountType.Expense) expenses += line.DebitBalance - line.CreditBalance;
        }
        var gross = revenue - cogs;
        return new IncomeStatementDto(revenue, cogs, gross, expenses, gross - expenses);
    }

    public async Task<CashFlowDto> GetCashFlowAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var cash = await _context.ChartOfAccounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.AccountNumber == StandardAccountCodes.Cash, ct);
        if (cash is null) return new CashFlowDto(0, 0, 0, 0);

        var closing = await _accountBalance.GetAccountBalanceAsync(cash.Id, filter.ToDate, ct);
        var (from, to) = ReportQueryHelper.ResolveDateRange(filter);
        var cashIn = await _context.JournalEntryLines.AsNoTracking()
            .Where(l => l.ChartOfAccountId == cash.Id && l.Debit > 0)
            .Join(_context.JournalEntries.AsNoTracking().Where(j => j.Status == JournalStatus.Posted && j.PostingDate >= DateOnly.FromDateTime(from.UtcDateTime)),
                l => l.JournalEntryId, j => j.Id, (l, j) => l.Debit)
            .SumAsync(ct);
        var cashOut = await _context.JournalEntryLines.AsNoTracking()
            .Where(l => l.ChartOfAccountId == cash.Id && l.Credit > 0)
            .Join(_context.JournalEntries.AsNoTracking().Where(j => j.Status == JournalStatus.Posted),
                l => l.JournalEntryId, j => j.Id, (l, j) => l.Credit)
            .SumAsync(ct);
        return new CashFlowDto(closing - cashIn + cashOut, cashIn, cashOut, closing);
    }

    public async Task<VatSummaryDto> GetVatSummaryAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var vatReport = await new SalesAnalyticsService(_context).GetVatReportAsync(tenantId, filter, ct);
        return new VatSummaryDto(vatReport.VatCollected, 0, vatReport.VatCollected);
    }

    public async Task<IReadOnlyList<RevenueAnalysisDto>> GetRevenueAnalysisAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
        => await ReportQueryHelper.FilterOrders(_context.SalesOrders, tenantId, filter)
            .GroupBy(o => o.OrderType)
            .Select(g => new RevenueAnalysisDto(g.Key.ToString(), g.Sum(o => o.GrandTotal), g.Count()))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ExpenseAnalysisDto>> GetExpenseAnalysisAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var tb = await _trialBalance.GetTrialBalanceAsync(tenantId, new TrialBalanceFilterDto(filter.ToDate), ct);
        var expenseAccounts = await _context.ChartOfAccounts.AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.AccountType == AccountType.Expense).ToListAsync(ct);
        return expenseAccounts.Select(a =>
        {
            var line = tb.FirstOrDefault(t => t.AccountId == a.Id);
            return new ExpenseAnalysisDto(a.NameAr, line?.DebitBalance ?? 0, line != null ? 1 : 0);
        }).Where(x => x.Amount > 0).OrderByDescending(x => x.Amount).ToList();
    }
}

public sealed class KpiEngineService : IKpiEngineService
{
    private readonly ISalesAnalyticsService _sales;
    private readonly IKitchenAnalyticsService _kitchen;
    private readonly IDeliveryAnalyticsService _delivery;
    private readonly ICustomerAnalyticsService _customer;
    private readonly IApplicationDbContext _context;

    public KpiEngineService(
        ISalesAnalyticsService sales, IKitchenAnalyticsService kitchen,
        IDeliveryAnalyticsService delivery, ICustomerAnalyticsService customer,
        IApplicationDbContext context)
        => (_sales, _kitchen, _delivery, _customer, _context) = (sales, kitchen, delivery, customer, context);

    public async Task<KpiDashboardDto> GetKpiDashboardAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var orders = ReportQueryHelper.FilterOrders(_context.SalesOrders, tenantId, filter);
        var orderCount = await orders.CountAsync(ct);
        var revenue = await orders.SumAsync(o => o.GrandTotal, ct);
        var avgTicket = orderCount > 0 ? revenue / orderCount : 0;

        var kitchen = await _kitchen.GetKitchenPerformanceAsync(tenantId, filter, ct);
        var delivery = await _delivery.GetDeliverySummaryAsync(tenantId, filter, ct);
        var loyalty = await _customer.GetLoyaltyPointsReportAsync(tenantId, filter, ct);

        var grossMargin = revenue > 0 ? (revenue - await orders.SumAsync(o => o.TaxTotal, ct)) / revenue * 100 : 0;
        var retention = loyalty.ActiveAccounts > 0 ? (decimal)loyalty.ActiveAccounts : 0;

        var (from, to) = ReportQueryHelper.ResolveDateRange(filter);
        var kpis = new List<KpiSnapshotDto>
        {
            new("Average Ticket", avgTicket, "SAR", null, "green", DateOnly.FromDateTime(from.UtcDateTime), DateOnly.FromDateTime(to.UtcDateTime)),
            new("Gross Margin", grossMargin, "%", 30, grossMargin >= 30 ? "green" : "amber", DateOnly.FromDateTime(from.UtcDateTime), DateOnly.FromDateTime(to.UtcDateTime)),
            new("Kitchen SLA", (decimal)kitchen.OnTimePercent, "%", 90, kitchen.OnTimePercent >= 90 ? "green" : "red", DateOnly.FromDateTime(from.UtcDateTime), DateOnly.FromDateTime(to.UtcDateTime)),
            new("Delivery SLA", (decimal)delivery.AvgDeliveryMinutes, "min", 45, delivery.AvgDeliveryMinutes <= 45 ? "green" : "amber", DateOnly.FromDateTime(from.UtcDateTime), DateOnly.FromDateTime(to.UtcDateTime)),
            new("Customer Retention", retention, "accounts", null, "green", DateOnly.FromDateTime(from.UtcDateTime), DateOnly.FromDateTime(to.UtcDateTime)),
            new("Inventory Turnover", 0, "x", null, "green", DateOnly.FromDateTime(from.UtcDateTime), DateOnly.FromDateTime(to.UtcDateTime))
        };
        return new KpiDashboardDto(kpis);
    }
}

public sealed class ReportExportService : IReportExportService
{
    private readonly ISalesAnalyticsService _sales;
    private readonly IDashboardService _dashboard;

    public ReportExportService(ISalesAnalyticsService sales, IDashboardService dashboard)
        => (_sales, _dashboard) = (sales, dashboard);

    public async Task<ExportResultDto> ExportAsync(Guid tenantId, ExportReportRequestDto request, CancellationToken ct = default)
    {
        var rows = request.ReportKey.ToLowerInvariant() switch
        {
            "daily-sales" => (await _sales.GetDailySalesAsync(tenantId, request.Filter, ct))
                .Select(r => new[] { r.Period, r.OrderCount.ToString(), r.Revenue.ToString("F2") }),
            "sales-by-branch" => (await _sales.GetSalesByBranchAsync(tenantId, request.Filter, ct))
                .Select(r => new[] { r.BranchName, r.OrderCount.ToString(), r.Revenue.ToString("F2") }),
            _ => Enumerable.Empty<string[]>()
        };

        var header = request.ReportKey.ToLowerInvariant() switch
        {
            "daily-sales" => new[] { "Period", "Orders", "Revenue" },
            "sales-by-branch" => new[] { "Branch", "Orders", "Revenue" },
            _ => Array.Empty<string>()
        };

        return request.Format switch
        {
            ExportFormat.Csv or ExportFormat.Excel => ExportCsv(header, rows, request.ReportKey),
            ExportFormat.Pdf => ExportPdf(header, rows, request.ReportKey),
            _ => ExportCsv(header, rows, request.ReportKey)
        };
    }

    private static ExportResultDto ExportCsv(IReadOnlyList<string> header, IEnumerable<string[]> rows, string reportKey)
    {
        var lines = new List<string> { string.Join(",", header) };
        lines.AddRange(rows.Select(r => string.Join(",", r.Select(c => $"\"{c}\""))));
        var bytes = System.Text.Encoding.UTF8.GetPreamble()
            .Concat(System.Text.Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines))).ToArray();
        var ext = reportKey.Replace(' ', '-').ToLowerInvariant();
        return new ExportResultDto(bytes, "text/csv", $"{ext}.csv");
    }

    private static ExportResultDto ExportPdf(IReadOnlyList<string> header, IEnumerable<string[]> rows, string reportKey)
    {
        var html = $"<html><body><h1>{reportKey}</h1><table border='1'><tr>{string.Join("", header.Select(h => $"<th>{h}</th>"))}</tr>";
        foreach (var row in rows)
            html += $"<tr>{string.Join("", row.Select(c => $"<td>{c}</td>"))}</tr>";
        html += "</table></body></html>";
        var bytes = System.Text.Encoding.UTF8.GetBytes(html);
        return new ExportResultDto(bytes, "text/html", $"{reportKey}.html");
    }
}
