using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.Finance.Services;
using GastroErp.Application.Features.Reporting.DTOs;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Reporting.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _context;
    private readonly IAccountBalanceService _accountBalance;

    public DashboardService(IApplicationDbContext context, IAccountBalanceService accountBalance)
        => (_context, _accountBalance) = (context, accountBalance);

    public async Task<ExecutiveDashboardDto> GetExecutiveDashboardAsync(
        Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var monthStart = new DateOnly(today.Year, today.Month, 1);

        var todayFilter = filter with { FromDate = today, ToDate = today };
        var monthFilter = filter with { FromDate = monthStart, ToDate = today };

        var todayOrders = ReportQueryHelper.FilterOrders(_context.SalesOrders, tenantId, todayFilter);
        var monthOrders = ReportQueryHelper.FilterOrders(_context.SalesOrders, tenantId, monthFilter);
        var periodOrders = ReportQueryHelper.FilterOrders(_context.SalesOrders, tenantId, filter);

        var revenueToday = await todayOrders.SumAsync(o => o.GrandTotal, ct);
        var revenueMonth = await monthOrders.SumAsync(o => o.GrandTotal, ct);
        var ordersToday = await todayOrders.CountAsync(ct);
        var ordersMonth = await monthOrders.CountAsync(ct);
        var avgTicket = ordersToday > 0 ? revenueToday / ordersToday : 0;

        var activeCustomers = await _context.Customers.AsNoTracking()
            .CountAsync(c => c.TenantId == tenantId && c.Status == CustomerStatus.Active, ct);

        var (from, to) = ReportQueryHelper.ResolveDateRange(filter);
        var newCustomers = await _context.Customers.AsNoTracking()
            .CountAsync(c => c.TenantId == tenantId && c.CreatedAt >= from && c.CreatedAt <= to, ct);

        var activeBranches = await _context.Branches.AsNoTracking()
            .CountAsync(b => b.TenantId == tenantId && b.Status == BranchStatus.Active, ct);

        var grossProfit = await periodOrders.SumAsync(o => o.SubTotal - o.DiscountTotal, ct);
        var netProfit = await periodOrders.SumAsync(o => o.GrandTotal - o.TaxTotal, ct);

        var cashAccount = await _context.ChartOfAccounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.AccountNumber == StandardAccountCodes.Cash, ct);
        var cashBalance = cashAccount is not null
            ? await _accountBalance.GetAccountBalanceAsync(cashAccount.Id, filter.ToDate, ct) : 0;

        var kpis = new List<KpiCardDto>
        {
            new("revenue_today", "Revenue Today", revenueToday, "SAR"),
            new("revenue_month", "Revenue This Month", revenueMonth, "SAR"),
            new("orders_today", "Orders Today", ordersToday),
            new("avg_ticket", "Average Ticket", avgTicket, "SAR"),
            new("active_customers", "Active Customers", activeCustomers),
            new("new_customers", "New Customers", newCustomers),
            new("active_branches", "Active Branches", activeBranches),
            new("gross_profit", "Gross Profit", grossProfit, "SAR"),
            new("net_profit", "Net Profit", netProfit, "SAR"),
            new("cash_balance", "Cash Balance", cashBalance, "SAR")
        };

        var periodRows = await periodOrders
            .Select(o => new { At = o.CompletedAt ?? o.CreatedAt, o.GrandTotal })
            .ToListAsync(ct);

        var dailyTrend = periodRows
            .GroupBy(o => DateOnly.FromDateTime(o.At.UtcDateTime))
            .Select(g => new { Date = g.Key, Revenue = g.Sum(x => x.GrandTotal) })
            .OrderBy(x => x.Date)
            .ToList();

        var revenueTrend = new LineChartDto(
            dailyTrend.Select(x => x.Date.ToString("yyyy-MM-dd")).ToList(),
            [new ChartSeriesDto("Revenue", dailyTrend.Select(x => x.Revenue).ToList())]);

        var (payFrom, payTo) = ReportQueryHelper.ResolveDateRange(filter);
        var paymentMix = await _context.Payments.AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.Status == PaymentStatus.Completed
                && p.ProcessedAt >= payFrom && p.ProcessedAt <= payTo)
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new { Method = g.Key, Amount = g.Sum(p => p.Amount) })
            .ToListAsync(ct);

        var orderTypeMix = periodRows.Count == 0
            ? []
            : (await periodOrders
                .GroupBy(o => o.OrderType)
                .Select(g => new { Type = g.Key, Revenue = g.Sum(o => o.GrandTotal) })
                .ToListAsync(ct));

        var orderIds = await periodOrders.Select(o => o.Id).ToListAsync(ct);
        var topProducts = orderIds.Count == 0
            ? []
            : (await _context.OrderItems.AsNoTracking()
                .Where(i => orderIds.Contains(i.SalesOrderId) && !i.IsVoided)
                .GroupBy(i => new { i.ProductId, i.ProductNameAr })
                .Select(g => new { g.Key.ProductId, g.Key.ProductNameAr, Revenue = g.Sum(i => i.LineTotal) })
                .OrderByDescending(x => x.Revenue).Take(10).ToListAsync(ct));

        var topCategories = orderIds.Count == 0
            ? []
            : (await (
                from item in _context.OrderItems.AsNoTracking()
                join product in _context.Products.AsNoTracking() on item.ProductId equals product.Id
                join category in _context.Categories.AsNoTracking() on product.CategoryId equals category.Id
                where orderIds.Contains(item.SalesOrderId) && !item.IsVoided
                group item by new { category.Id, category.NameAr } into g
                select new { g.Key.Id, g.Key.NameAr, Revenue = g.Sum(i => i.LineTotal) })
                .OrderByDescending(x => x.Revenue).Take(10).ToListAsync(ct));

        return new ExecutiveDashboardDto(
            kpis, revenueTrend,
            new PieChartDto(paymentMix.Select(p => p.Method.ToString()).ToList(), paymentMix.Select(p => p.Amount).ToList()),
            new PieChartDto(orderTypeMix.Select(o => o.Type.ToString()).ToList(), orderTypeMix.Select(o => o.Revenue).ToList()),
            new BarChartDto(topProducts.Select(p => p.ProductNameAr).ToList(), [new ChartSeriesDto("Revenue", topProducts.Select(p => p.Revenue).ToList())]),
            new BarChartDto(topCategories.Select(c => c.NameAr).ToList(), [new ChartSeriesDto("Revenue", topCategories.Select(c => c.Revenue).ToList())]));
    }
}
