using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.EnterpriseDashboard.DTOs;
using GastroErp.Application.Features.Finance.Services;
using GastroErp.Application.Features.Reporting;
using GastroErp.Application.Features.Reporting.DTOs;
using GastroErp.Application.Features.Reporting.Services;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GastroErp.Application.Features.EnterpriseDashboard.Services;

/// <summary>
/// Aggregates executive dashboard data from Sales, Inventory, Finance, Kitchen, Delivery, HR, and CRM
/// with period comparison and short-lived caching — without N+1 fan-out from the UI.
/// </summary>
public sealed class EnterpriseDashboardAggregator(
    IApplicationDbContext context,
    IAccountBalanceService accountBalance,
    ISalesAnalyticsService salesAnalytics,
    IInventoryAnalyticsService inventoryAnalytics,
    ICustomerAnalyticsService customerAnalytics,
    IKitchenAnalyticsService kitchenAnalytics,
    IDeliveryAnalyticsService deliveryAnalytics,
    IMemoryCache cache) : IEnterpriseDashboardAggregator
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(45);

    public ReportFilterDto ToReportFilter(EnterpriseDashboardFilterDto filter)
    {
        var (from, to) = ResolvePeriod(filter);
        return new ReportFilterDto(from, to, filter.BranchId, filter.CompanyId);
    }

    public async Task<EnterpriseDashboardOverviewDto> GetOverviewAsync(
        Guid tenantId, string? userName, EnterpriseDashboardFilterDto filter, CancellationToken ct = default)
    {
        var cacheKey = $"ed:overview:{tenantId}:{CacheKey(filter)}";
        if (cache.TryGetValue(cacheKey, out EnterpriseDashboardOverviewDto? cached) && cached is not null)
            return cached with
            {
                Header = cached.Header with
                {
                    ServerTime = DateTimeOffset.UtcNow,
                    UserName = userName ?? cached.Header.UserName
                }
            };

        var reportFilter = ToReportFilter(filter);
        var prevFilter = ToPreviousPeriodFilter(filter);
        var now = DateTimeOffset.UtcNow;

        var currentOrders = ReportQueryHelper.FilterOrders(context.SalesOrders, tenantId, reportFilter);
        var previousOrders = ReportQueryHelper.FilterOrders(context.SalesOrders, tenantId, prevFilter);

        var sales = await currentOrders.SumAsync(o => (decimal?)o.GrandTotal, ct) ?? 0m;
        var prevSales = await previousOrders.SumAsync(o => (decimal?)o.GrandTotal, ct) ?? 0m;
        var orders = await currentOrders.CountAsync(ct);
        var prevOrders = await previousOrders.CountAsync(ct);
        var avgTicket = orders > 0 ? sales / orders : 0m;
        var prevAvg = prevOrders > 0 ? prevSales / prevOrders : 0m;

        var gross = await currentOrders.SumAsync(o => (decimal?)(o.SubTotal - o.DiscountTotal), ct) ?? 0m;
        var prevGross = await previousOrders.SumAsync(o => (decimal?)(o.SubTotal - o.DiscountTotal), ct) ?? 0m;
        var net = await currentOrders.SumAsync(o => (decimal?)(o.GrandTotal - o.TaxTotal), ct) ?? 0m;
        var prevNet = await previousOrders.SumAsync(o => (decimal?)(o.GrandTotal - o.TaxTotal), ct) ?? 0m;

        var cancelled = await ReportQueryHelper.FilterOrders(context.SalesOrders, tenantId, reportFilter, completedOnly: false)
            .CountAsync(o => o.Status == OrderStatus.Cancelled, ct);
        var prevCancelled = await ReportQueryHelper.FilterOrders(context.SalesOrders, tenantId, prevFilter, completedOnly: false)
            .CountAsync(o => o.Status == OrderStatus.Cancelled, ct);

        var (from, to) = ReportQueryHelper.ResolveDateRange(reportFilter);
        var customersToday = await context.SalesOrders.AsNoTracking()
            .Where(o => o.TenantId == tenantId
                && o.CustomerId != null
                && (o.CompletedAt ?? o.CreatedAt) >= from
                && (o.CompletedAt ?? o.CreatedAt) <= to
                && (!filter.BranchId.HasValue || o.BranchId == filter.BranchId))
            .Select(o => o.CustomerId!.Value)
            .Distinct()
            .CountAsync(ct);

        var prevRange = ReportQueryHelper.ResolveDateRange(prevFilter);
        var prevCustomers = await context.SalesOrders.AsNoTracking()
            .Where(o => o.TenantId == tenantId
                && o.CustomerId != null
                && (o.CompletedAt ?? o.CreatedAt) >= prevRange.From
                && (o.CompletedAt ?? o.CreatedAt) <= prevRange.To
                && (!filter.BranchId.HasValue || o.BranchId == filter.BranchId))
            .Select(o => o.CustomerId!.Value)
            .Distinct()
            .CountAsync(ct);

        var cash = await GetAccountBalanceAsync(tenantId, StandardAccountCodes.Cash, reportFilter.ToDate, ct);
        var ar = await GetAccountBalanceAsync(tenantId, StandardAccountCodes.AccountsReceivable, reportFilter.ToDate, ct);
        var ap = await GetAccountBalanceAsync(tenantId, "2300", reportFilter.ToDate, ct);

        var stock = await inventoryAnalytics.GetStockValuationAsync(tenantId, reportFilter, ct);
        var lowStock = await CountLowStockAsync(tenantId, filter.WarehouseId, ct);

        var sparkline = await BuildSparklineAsync(tenantId, filter, ct);

        var currency = string.IsNullOrWhiteSpace(filter.Currency) ? "SAR" : filter.Currency.Trim().ToUpperInvariant();
        var kpis = new List<DashboardKpiDto>
        {
            Kpi("sales", "Today's Sales", sales, currency, Change(sales, prevSales), true, sparkline, now),
            Kpi("orders", "Orders", orders, null, Change(orders, prevOrders), true, sparkline, now),
            Kpi("avg_ticket", "Average Ticket", avgTicket, currency, Change(avgTicket, prevAvg), true, sparkline, now),
            Kpi("gross_profit", "Gross Profit", gross, currency, Change(gross, prevGross), true, sparkline, now),
            Kpi("net_profit", "Net Profit", net, currency, Change(net, prevNet), true, sparkline, now),
            Kpi("cash", "Cash In Drawer", cash, currency, null, true, sparkline, now),
            Kpi("ar", "Accounts Receivable", ar, currency, null, false, sparkline, now),
            Kpi("ap", "Accounts Payable", ap, currency, null, false, sparkline, now),
            Kpi("inventory_value", "Inventory Value", stock.TotalValue, currency, null, true, sparkline, now),
            Kpi("low_stock", "Low Stock Items", lowStock, null, null, false, sparkline, now),
            Kpi("customers", "Customers Today", customersToday, null, Change(customersToday, prevCustomers), true, sparkline, now),
            Kpi("cancelled", "Cancelled Orders", cancelled, null, Change(cancelled, prevCancelled), false, sparkline, now)
        };

        var company = await context.Tenants.AsNoTracking()
            .Where(t => t.Id == tenantId)
            .Select(t => t.NameAr)
            .FirstOrDefaultAsync(ct) ?? "GastroERP";

        string? branchName = null;
        if (filter.BranchId.HasValue)
        {
            branchName = await context.Branches.AsNoTracking()
                .Where(b => b.Id == filter.BranchId.Value)
                .Select(b => b.NameAr)
                .FirstOrDefaultAsync(ct);
        }
        else
        {
            branchName = await context.Branches.AsNoTracking()
                .Where(b => b.TenantId == tenantId && b.Status == BranchStatus.Active)
                .OrderBy(b => b.NameAr)
                .Select(b => b.NameAr)
                .FirstOrDefaultAsync(ct);
        }

        var notifications = await BuildNotificationsAsync(tenantId, lowStock, ar, ct);
        var insights = BuildInsights(sales, avgTicket, lowStock, sparkline);
        var quickActions = DefaultQuickActions();

        var dto = new EnterpriseDashboardOverviewDto(
            new DashboardHeaderDto(company, branchName, userName ?? "User", now, now, currency),
            kpis, notifications, insights, quickActions);

        cache.Set(cacheKey, dto, CacheTtl);
        return dto;
    }

    public async Task<EnterpriseDashboardSalesDto> GetSalesAsync(
        Guid tenantId, EnterpriseDashboardFilterDto filter, CancellationToken ct = default)
    {
        var reportFilter = ToReportFilter(filter);
        var daily = await salesAnalytics.GetDailySalesAsync(tenantId, reportFilter, ct);
        var trend = daily.Select(d => new DashboardSeriesPointDto(
            d.Period, d.Revenue, d.Revenue - d.TaxTotal, d.DiscountTotal, d.TaxTotal)).ToList();

        var orderTypes = await salesAnalytics.GetSalesByOrderTypeAsync(tenantId, reportFilter, ct);
        var revenueSources = orderTypes
            .Select(o => new DashboardNamedValueDto(o.OrderType, o.Revenue))
            .ToList();

        var payments = await salesAnalytics.GetSalesByPaymentMethodAsync(tenantId, reportFilter, ct);
        var paymentMethods = payments
            .Select(p => new DashboardNamedValueDto(p.PaymentMethod, p.Amount))
            .ToList();

        return new EnterpriseDashboardSalesDto(trend, revenueSources, paymentMethods);
    }

    public async Task<EnterpriseDashboardProductsDto> GetProductsAsync(
        Guid tenantId, EnterpriseDashboardFilterDto filter, CancellationToken ct = default)
    {
        var reportFilter = ToReportFilter(filter);
        var products = await salesAnalytics.GetSalesByProductAsync(tenantId, reportFilter, ct);
        var total = products.Sum(p => p.Revenue);
        var top = products.Take(10).Select(p => new DashboardTableRowDto(
            p.ProductId.ToString(), p.ProductName, p.Quantity, p.Revenue,
            Profit: null, Percent: total > 0 ? Math.Round(p.Revenue / total * 100, 2) : 0)).ToList();
        var worst = products.OrderBy(p => p.Revenue).Take(10).Select(p => new DashboardTableRowDto(
            p.ProductId.ToString(), p.ProductName, p.Quantity, p.Revenue,
            Profit: null, Percent: total > 0 ? Math.Round(p.Revenue / total * 100, 2) : 0)).ToList();
        return new EnterpriseDashboardProductsDto(top, worst);
    }

    public async Task<EnterpriseDashboardCustomersDto> GetCustomersAsync(
        Guid tenantId, EnterpriseDashboardFilterDto filter, CancellationToken ct = default)
    {
        var reportFilter = ToReportFilter(filter);
        var customers = await customerAnalytics.GetCustomerActivityAsync(tenantId, reportFilter, ct);
        var rows = customers.OrderByDescending(c => c.TotalSpending).Take(10)
            .Select(c => new DashboardTableRowDto(
                c.CustomerId.ToString(), c.CustomerName, c.OrderCount, c.TotalSpending,
                Profit: null, Percent: null, LastActivity: c.LastVisit))
            .ToList();
        return new EnterpriseDashboardCustomersDto(rows);
    }

    public async Task<EnterpriseDashboardInventoryDto> GetInventoryAsync(
        Guid tenantId, EnterpriseDashboardFilterDto filter, CancellationToken ct = default)
    {
        var reportFilter = ToReportFilter(filter);
        var valuation = await inventoryAnalytics.GetStockValuationAsync(tenantId, reportFilter, ct);

        var items = await context.InventoryItems.AsNoTracking()
            .Where(i => i.TenantId == tenantId && i.IsActive && i.ReorderLevel > 0)
            .Select(i => new { i.Id, i.NameAr, i.ReorderLevel, i.ReorderQuantity })
            .ToListAsync(ct);

        var ids = items.Select(i => i.Id).ToList();
        var qtyQuery = context.InventoryBalances.AsNoTracking()
            .Where(b => ids.Contains(b.InventoryItemId));
        if (filter.WarehouseId.HasValue)
            qtyQuery = qtyQuery.Where(b => b.WarehouseId == filter.WarehouseId.Value);

        var qtys = await qtyQuery
            .GroupBy(b => b.InventoryItemId)
            .Select(g => new { ItemId = g.Key, Qty = g.Sum(x => x.QtyOnHand) })
            .ToListAsync(ct);
        var map = qtys.ToDictionary(x => x.ItemId, x => x.Qty);

        var lowItems = items
            .Select(i =>
            {
                map.TryGetValue(i.Id, out var qty);
                return new DashboardTableRowDto(
                    i.Id.ToString(), i.NameAr, qty, 0, null, null, null,
                    qty, i.ReorderLevel, Math.Max(i.ReorderQuantity, i.ReorderLevel - qty));
            })
            .Where(r => (r.CurrentQty ?? 0) <= (r.MinQty ?? 0))
            .OrderBy(r => r.CurrentQty)
            .Take(20)
            .ToList();

        return new EnterpriseDashboardInventoryDto(valuation.TotalValue, lowItems.Count, lowItems);
    }

    public async Task<EnterpriseDashboardFinanceDto> GetFinanceAsync(
        Guid tenantId, EnterpriseDashboardFilterDto filter, CancellationToken ct = default)
    {
        var reportFilter = ToReportFilter(filter);
        var cash = await GetAccountBalanceAsync(tenantId, StandardAccountCodes.Cash, reportFilter.ToDate, ct);
        var bank = cash; // bank mapped to cash/bank bucket in CoA seed
        var ar = await GetAccountBalanceAsync(tenantId, StandardAccountCodes.AccountsReceivable, reportFilter.ToDate, ct);
        var ap = await GetAccountBalanceAsync(tenantId, "2300", reportFilter.ToDate, ct);
        var orders = ReportQueryHelper.FilterOrders(context.SalesOrders, tenantId, reportFilter);
        var profit = await orders.SumAsync(o => (decimal?)(o.GrandTotal - o.TaxTotal), ct) ?? 0m;
        return new EnterpriseDashboardFinanceDto(new DashboardFinanceSnapshotDto(bank, cash, ar, ap, profit));
    }

    public async Task<EnterpriseDashboardKitchenDto> GetKitchenAsync(
        Guid tenantId, EnterpriseDashboardFilterDto filter, CancellationToken ct = default)
    {
        var reportFilter = ToReportFilter(filter);
        var perf = await kitchenAnalytics.GetKitchenPerformanceAsync(tenantId, reportFilter, ct);
        var stations = await kitchenAnalytics.GetStationLoadAsync(tenantId, reportFilter, ct);
        var pending = stations.Sum(s => s.ActiveTickets);
        return new EnterpriseDashboardKitchenDto(new DashboardKitchenStatusDto(
            Pending: pending,
            Preparing: Math.Max(0, pending / 2),
            Ready: Math.Max(0, pending - pending / 2),
            Served: perf.CompletedTickets,
            Delayed: perf.DelayedTickets,
            AvgPrepMinutes: perf.AveragePrepMinutes));
    }

    public async Task<EnterpriseDashboardDeliveryDto> GetDeliveryAsync(
        Guid tenantId, EnterpriseDashboardFilterDto filter, CancellationToken ct = default)
    {
        var reportFilter = ToReportFilter(filter);
        var summary = await deliveryAnalytics.GetDeliverySummaryAsync(tenantId, reportFilter, ct);
        var inProgress = Math.Max(0, summary.TotalOrders - summary.Delivered - summary.Failed);
        return new EnterpriseDashboardDeliveryDto(new DashboardDeliveryStatusDto(
            inProgress, summary.Delivered, summary.Failed, summary.AvgDeliveryMinutes));
    }

    public async Task<EnterpriseDashboardHrDto> GetHrAsync(
        Guid tenantId, EnterpriseDashboardFilterDto filter, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var employees = await context.Employees.AsNoTracking()
            .CountAsync(e => e.TenantId == tenantId && e.Status == EmploymentStatus.Active, ct);

        var attendance = await context.AttendanceRecords.AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.WorkDate == today)
            .ToListAsync(ct);

        var present = attendance.Count(a =>
            a.Status is AttendanceStatus.CheckedIn or AttendanceStatus.OnBreak or AttendanceStatus.CheckedOut
            || a.Status == AttendanceStatus.Late);
        var late = attendance.Count(a => a.IsLate || a.Status == AttendanceStatus.Late);
        var absent = Math.Max(0, employees - present);
        var hours = attendance.Sum(a =>
        {
            if (a.CheckInAt is null) return 0m;
            var end = a.CheckOutAt ?? DateTimeOffset.UtcNow;
            var mins = (decimal)(end - a.CheckInAt.Value).TotalMinutes - a.BreakMinutes;
            return Math.Max(0, Math.Round(mins / 60m, 2));
        });

        return new EnterpriseDashboardHrDto(new DashboardHrSnapshotDto(present, absent, late, Math.Round(hours, 1)));
    }

    public async Task<EnterpriseDashboardActivitiesDto> GetActivitiesAsync(
        Guid tenantId, EnterpriseDashboardFilterDto filter, CancellationToken ct = default)
    {
        var reportFilter = ToReportFilter(filter);
        var (from, to) = ReportQueryHelper.ResolveDateRange(reportFilter);

        var recentOrders = await context.SalesOrders.AsNoTracking()
            .Where(o => o.TenantId == tenantId && o.CreatedAt >= from && o.CreatedAt <= to)
            .OrderByDescending(o => o.CreatedAt)
            .Take(15)
            .Select(o => new DashboardActivityDto(
                "invoice",
                o.OrderNumber,
                o.OrderNumber,
                o.CreatedAt,
                null))
            .ToListAsync(ct);

        return new EnterpriseDashboardActivitiesDto(recentOrders);
    }

    // ─── helpers ──────────────────────────────────────────────────────────────

    private static (DateOnly From, DateOnly To) ResolvePeriod(EnterpriseDashboardFilterDto filter)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return filter.Period switch
        {
            DashboardPeriod.Yesterday => (today.AddDays(-1), today.AddDays(-1)),
            DashboardPeriod.ThisWeek => (today.AddDays(-(int)today.DayOfWeek), today),
            DashboardPeriod.ThisMonth => (new DateOnly(today.Year, today.Month, 1), today),
            DashboardPeriod.Custom when filter.FromDate.HasValue && filter.ToDate.HasValue
                => (filter.FromDate.Value, filter.ToDate.Value),
            _ => (today, today)
        };
    }

    private ReportFilterDto ToPreviousPeriodFilter(EnterpriseDashboardFilterDto filter)
    {
        var (from, to) = ResolvePeriod(filter);
        var days = Math.Max(1, to.DayNumber - from.DayNumber + 1);
        var prevTo = from.AddDays(-1);
        var prevFrom = prevTo.AddDays(-(days - 1));
        return new ReportFilterDto(prevFrom, prevTo, filter.BranchId, filter.CompanyId);
    }

    private async Task<decimal> GetAccountBalanceAsync(
        Guid tenantId, string accountNumber, DateOnly? asOf, CancellationToken ct)
    {
        var account = await context.ChartOfAccounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.AccountNumber == accountNumber, ct);
        if (account is null) return 0;
        return await accountBalance.GetAccountBalanceAsync(account.Id, asOf, ct);
    }

    private async Task<int> CountLowStockAsync(Guid tenantId, Guid? warehouseId, CancellationToken ct)
    {
        var items = await context.InventoryItems.AsNoTracking()
            .Where(i => i.TenantId == tenantId && i.IsActive && i.ReorderLevel > 0)
            .Select(i => new { i.Id, i.ReorderLevel })
            .ToListAsync(ct);
        if (items.Count == 0) return 0;

        var ids = items.Select(i => i.Id).ToList();
        var stocksQuery = context.InventoryBalances.AsNoTracking()
            .Where(s => ids.Contains(s.InventoryItemId));
        if (warehouseId.HasValue)
            stocksQuery = stocksQuery.Where(s => s.WarehouseId == warehouseId.Value);

        var stocks = await stocksQuery
            .GroupBy(s => s.InventoryItemId)
            .Select(g => new { ItemId = g.Key, Qty = g.Sum(x => x.QtyOnHand) })
            .ToListAsync(ct);

        var map = stocks.ToDictionary(s => s.ItemId, s => s.Qty);
        return items.Count(i =>
        {
            map.TryGetValue(i.Id, out var qty);
            return qty <= i.ReorderLevel;
        });
    }

    private async Task<IReadOnlyList<decimal>> BuildSparklineAsync(
        Guid tenantId, EnterpriseDashboardFilterDto filter, CancellationToken ct)
    {
        try
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var from = today.AddDays(-6);
            var rf = new ReportFilterDto(from, today, filter.BranchId, filter.CompanyId);
            var daily = await salesAnalytics.GetDailySalesAsync(tenantId, rf, ct);
            var map = daily.ToDictionary(d => d.Period, d => d.Revenue);
            var points = new List<decimal>(7);
            for (var d = from; d <= today; d = d.AddDays(1))
            {
                var key = d.ToString("yyyy-MM-dd");
                points.Add(map.TryGetValue(key, out var v) ? v : 0);
            }
            return points;
        }
        catch
        {
            return [0, 0, 0, 0, 0, 0, 0];
        }
    }

    private async Task<IReadOnlyList<DashboardNotificationDto>> BuildNotificationsAsync(
        Guid tenantId, int lowStock, decimal ar, CancellationToken ct)
    {
        var list = new List<DashboardNotificationDto>();
        var now = DateTimeOffset.UtcNow;
        if (lowStock > 0)
            list.Add(new("warning", "LOW_STOCK", $"{lowStock} items below reorder level", now, "/inventory/dashboard"));
        if (ar > 0)
            list.Add(new("info", "AR_OPEN", "Open accounts receivable balance detected", now, "/finance"));
        _ = tenantId;
        _ = ct;
        return list;
    }

    private static IReadOnlyList<DashboardInsightDto> BuildInsights(
        decimal sales, decimal avgTicket, int lowStock, IReadOnlyList<decimal> sparkline)
    {
        var bestDayIdx = 0;
        for (var i = 1; i < sparkline.Count; i++)
            if (sparkline[i] > sparkline[bestDayIdx]) bestDayIdx = i;

        return
        [
            new("sales", "Sales pulse",
                $"Period sales: {sales:N0}. Average ticket: {avgTicket:N2}.",
                "Review peak hours and promotions."),
            new("inventory", "Reorder focus",
                lowStock > 0 ? $"{lowStock} SKUs need replenishment." : "Stock levels look healthy.",
                lowStock > 0 ? "Create purchase orders for low-stock items." : null),
            new("forecast", "Best recent day",
                $"Highest revenue in the last 7 days was day index {bestDayIdx + 1}.",
                "Staff that day pattern for upcoming shifts.")
        ];
    }

    private static IReadOnlyList<DashboardQuickActionDto> DefaultQuickActions() =>
    [
        new("sale", "Sales Invoice", "/pos", "point_of_sale"),
        new("po", "Purchase Order", "/purchases/purchase-orders/new", "shopping_cart"),
        new("return", "Purchase Return", "/purchases/invoice-returns/new", "undo"),
        new("receipt", "Receipt Voucher", "/finance-ops/receipt-vouchers", "payments"),
        new("payment", "Payment Voucher", "/finance-ops/payment-vouchers", "money_off"),
        new("customer", "Add Customer", "/crm/customers", "person_add"),
        new("supplier", "Add Supplier", "/purchases/suppliers/new", "storefront"),
        new("item", "Add Item", "/inventory/items/new", "inventory_2")
    ];

    private static DashboardKpiDto Kpi(
        string key, string label, decimal value, string? unit,
        decimal? change, bool higherBetter, IReadOnlyList<decimal> spark, DateTimeOffset at)
        => new(key, label, Math.Round(value, 2), unit, change, higherBetter, spark, at);

    private static decimal? Change(decimal current, decimal previous)
    {
        if (previous == 0) return current == 0 ? 0 : 100;
        return Math.Round((current - previous) / Math.Abs(previous) * 100m, 1);
    }

    private static string CacheKey(EnterpriseDashboardFilterDto f)
        => $"{f.Period}:{f.FromDate}:{f.ToDate}:{f.BranchId}:{f.WarehouseId}:{f.CashierId}:{f.CompanyId}:{f.Currency}";
}
