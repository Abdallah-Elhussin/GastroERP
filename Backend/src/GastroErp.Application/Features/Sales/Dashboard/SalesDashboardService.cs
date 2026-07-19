using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Sales.Dashboard;

public record GetSalesDashboardQuery(Guid TenantId, SalesDashboardFilterDto Filter)
    : IRequest<Result<SalesDashboardDto>>;

public sealed class GetSalesDashboardQueryHandler(ISalesDashboardService service)
    : IRequestHandler<GetSalesDashboardQuery, Result<SalesDashboardDto>>
{
    public async Task<Result<SalesDashboardDto>> Handle(
        GetSalesDashboardQuery request, CancellationToken cancellationToken)
    {
        var data = await service.GetAsync(request.TenantId, request.Filter, cancellationToken);
        return Result<SalesDashboardDto>.Success(data);
    }
}

public interface ISalesDashboardService
{
    Task<SalesDashboardDto> GetAsync(Guid tenantId, SalesDashboardFilterDto filter, CancellationToken ct = default);
}

public sealed class SalesDashboardService(IApplicationDbContext context) : ISalesDashboardService
{
    public async Task<SalesDashboardDto> GetAsync(
        Guid tenantId, SalesDashboardFilterDto filter, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var periodFrom = filter.FromDate ?? today.AddDays(-30);
        var periodTo = filter.ToDate ?? today;

        var todayStart = ToUtc(today);
        var todayEnd = ToUtcEnd(today);
        var yesterdayStart = ToUtc(today.AddDays(-1));
        var yesterdayEnd = ToUtcEnd(today.AddDays(-1));

        var weekStart = ToUtc(today.AddDays(-6));
        var prevWeekStart = ToUtc(today.AddDays(-13));
        var prevWeekEnd = ToUtcEnd(today.AddDays(-7));

        var monthStart = ToUtc(new DateOnly(today.Year, today.Month, 1));
        var prevMonthStartDate = new DateOnly(today.Year, today.Month, 1).AddMonths(-1);
        var prevMonthEndDate = new DateOnly(today.Year, today.Month, 1).AddDays(-1);
        var prevMonthStart = ToUtc(prevMonthStartDate);
        var prevMonthEnd = ToUtcEnd(prevMonthEndDate);

        var yearStart = ToUtc(new DateOnly(today.Year, 1, 1));
        var periodStart = ToUtc(periodFrom);
        var periodEnd = ToUtcEnd(periodTo);

        var baseQuery = context.SalesOrders.AsNoTracking()
            .Where(o => o.TenantId == tenantId);
        if (filter.CompanyId.HasValue)
            baseQuery = baseQuery.Where(o => o.CompanyId == filter.CompanyId);
        if (filter.BranchId.HasValue)
            baseQuery = baseQuery.Where(o => o.BranchId == filter.BranchId);
        if (filter.CashierId.HasValue)
            baseQuery = baseQuery.Where(o => o.CashierId == filter.CashierId);
        if (filter.DeviceId.HasValue)
            baseQuery = baseQuery.Where(o => o.DeviceId == filter.DeviceId);
        if (filter.CustomerId.HasValue)
            baseQuery = baseQuery.Where(o => o.CustomerId == filter.CustomerId);
        if (filter.OrderStatus.HasValue)
            baseQuery = baseQuery.Where(o => o.Status == filter.OrderStatus);

        var completed = baseQuery.Where(o => o.Status == OrderStatus.Completed);

        async Task<decimal> SumRevenue(DateTimeOffset from, DateTimeOffset to) =>
            await completed
                .Where(o => (o.CompletedAt ?? o.CreatedAt) >= from && (o.CompletedAt ?? o.CreatedAt) <= to)
                .SumAsync(o => (decimal?)o.GrandTotal, ct) ?? 0;

        var salesToday = await SumRevenue(todayStart, todayEnd);
        var salesYesterday = await SumRevenue(yesterdayStart, yesterdayEnd);
        var salesWeek = await SumRevenue(weekStart, todayEnd);
        var salesPrevWeek = await SumRevenue(prevWeekStart, prevWeekEnd);
        var salesMonth = await SumRevenue(monthStart, todayEnd);
        var salesPrevMonth = await SumRevenue(prevMonthStart, prevMonthEnd);
        var salesYear = await SumRevenue(yearStart, todayEnd);

        var periodOrders = completed
            .Where(o => (o.CompletedAt ?? o.CreatedAt) >= periodStart && (o.CompletedAt ?? o.CreatedAt) <= periodEnd);

        // Payment-method filter: payments in range whose allocations touch the order set.
        if (filter.PaymentMethod.HasValue)
        {
            var method = filter.PaymentMethod.Value;
            var orderIdsWithMethod = context.Payments.AsNoTracking()
                .Where(p => p.TenantId == tenantId
                    && p.Status == PaymentStatus.Completed
                    && p.PaymentMethod == method
                    && p.ProcessedAt >= periodStart
                    && p.ProcessedAt <= periodEnd)
                .SelectMany(p => p.Allocations.Select(a => a.SalesOrderId));
            periodOrders = periodOrders.Where(o => orderIdsWithMethod.Contains(o.Id));
        }

        var invoiceCount = await periodOrders.CountAsync(ct);
        var salesPeriod = await periodOrders.SumAsync(o => (decimal?)o.GrandTotal, ct) ?? 0;
        var discountsTotal = await periodOrders.SumAsync(o => (decimal?)o.DiscountTotal, ct) ?? 0;
        var avgInvoice = invoiceCount > 0 ? salesPeriod / invoiceCount : 0;
        var highest = invoiceCount > 0
            ? await periodOrders.MaxAsync(o => o.GrandTotal, ct)
            : 0;
        var lowest = invoiceCount > 0
            ? await periodOrders.MinAsync(o => o.GrandTotal, ct)
            : 0;

        var cancellationsTotal = await baseQuery
            .Where(o => o.Status == OrderStatus.Cancelled
                && (o.CancelledAt ?? o.CreatedAt) >= periodStart
                && (o.CancelledAt ?? o.CreatedAt) <= periodEnd)
            .SumAsync(o => (decimal?)o.GrandTotal, ct) ?? 0;

        var returnsQuery = context.CreditNotes.AsNoTracking()
            .Where(c => c.TenantId == tenantId
                && c.Status == CreditNoteStatus.Issued
                && c.IssuedAt != null
                && c.IssuedAt >= periodStart
                && c.IssuedAt <= periodEnd);
        if (filter.BranchId.HasValue)
            returnsQuery = returnsQuery.Where(c => c.BranchId == filter.BranchId);

        var returnsTotal = await returnsQuery.SumAsync(c => (decimal?)c.TotalAmount, ct) ?? 0;
        var returnsRatio = salesPeriod > 0 ? returnsTotal / salesPeriod * 100m : 0;

        var newCustomers = await context.Customers.AsNoTracking()
            .CountAsync(c => c.TenantId == tenantId
                && c.CreatedAt >= periodStart && c.CreatedAt <= periodEnd, ct);
        var activeCustomers = await context.Customers.AsNoTracking()
            .CountAsync(c => c.TenantId == tenantId && c.Status == CustomerStatus.Active, ct);
        var inactiveCustomers = await context.Customers.AsNoTracking()
            .CountAsync(c => c.TenantId == tenantId && c.Status != CustomerStatus.Active, ct);

        var firstTimeBuyers = await completed
            .Where(o => o.CustomerId != null)
            .GroupBy(o => o.CustomerId!.Value)
            .Select(g => new { CustomerId = g.Key, FirstAt = g.Min(x => x.CompletedAt ?? x.CreatedAt) })
            .Where(x => x.FirstAt >= periodStart && x.FirstAt <= periodEnd)
            .CountAsync(ct);

        var posOrders = periodOrders.Where(o => o.SalesChannel == SalesChannel.Pos);
        var posCount = await posOrders.CountAsync(ct);
        var avgMinutes = await periodOrders
            .Where(o => o.CompletedAt != null)
            .Select(o => (double?)(o.CompletedAt!.Value - o.CreatedAt).TotalMinutes)
            .AverageAsync(ct) ?? 0;

        var avgItems = await (
            from o in periodOrders
            join i in context.OrderItems.AsNoTracking() on o.Id equals i.SalesOrderId
            where !i.IsVoided
            group i by o.Id into g
            select (decimal?)g.Sum(x => x.Quantity))
            .AverageAsync(ct) ?? 0;

        var topDevice = await periodOrders
            .GroupBy(o => o.DeviceId)
            .Select(g => new { DeviceId = g.Key, Revenue = g.Sum(x => x.GrandTotal), Count = g.Count() })
            .OrderByDescending(x => x.Revenue)
            .FirstOrDefaultAsync(ct);

        string? mostActiveDevice = null;
        if (topDevice is not null)
        {
            mostActiveDevice = await context.Devices.AsNoTracking()
                .Where(d => d.Id == topDevice.DeviceId)
                .Select(d => d.NameAr)
                .FirstOrDefaultAsync(ct) ?? topDevice.DeviceId.ToString("N")[..8];
        }

        var grossProfit = await periodOrders.SumAsync(o => (decimal?)(o.SubTotal - o.DiscountTotal), ct) ?? 0;
        var netProfit = await periodOrders.SumAsync(o => (decimal?)(o.GrandTotal - o.TaxTotal), ct) ?? 0;
        const decimal cogs = 0m; // Order lines do not persist COGS yet
        var margin = salesPeriod > 0 ? grossProfit / salesPeriod * 100m : 0;
        var discountRatio = salesPeriod > 0 ? discountsTotal / salesPeriod * 100m : 0;
        var newCustomerRatio = activeCustomers > 0 ? (decimal)newCustomers / activeCustomers * 100m : 0;
        var returningRatio = invoiceCount > 0
            ? (decimal)await periodOrders.Where(o => o.CustomerId != null).Select(o => o.CustomerId).Distinct().CountAsync(ct)
              / Math.Max(1, invoiceCount) * 100m
            : 0;

        // Charts — period scoped
        var salesByDay = await periodOrders
            .GroupBy(o => DateOnly.FromDateTime((o.CompletedAt ?? o.CreatedAt).UtcDateTime))
            .Select(g => new { Day = g.Key, Value = g.Sum(x => x.GrandTotal), Count = g.Count() })
            .OrderBy(x => x.Day)
            .ToListAsync(ct);

        var salesByBranchRaw = await periodOrders
            .GroupBy(o => o.BranchId)
            .Select(g => new { BranchId = g.Key, Value = g.Sum(x => x.GrandTotal), Count = g.Count() })
            .OrderByDescending(x => x.Value)
            .Take(10)
            .ToListAsync(ct);
        var branchIds = salesByBranchRaw.Select(x => x.BranchId).ToList();
        var branchNames = await context.Branches.AsNoTracking()
            .Where(b => branchIds.Contains(b.Id))
            .Select(b => new { b.Id, b.NameAr })
            .ToDictionaryAsync(b => b.Id, b => b.NameAr, ct);

        var salesByDeviceRaw = await periodOrders
            .GroupBy(o => o.DeviceId)
            .Select(g => new { DeviceId = g.Key, Value = g.Sum(x => x.GrandTotal), Count = g.Count() })
            .OrderByDescending(x => x.Value)
            .Take(10)
            .ToListAsync(ct);
        var deviceIds = salesByDeviceRaw.Select(x => x.DeviceId).ToList();
        var deviceNames = await context.Devices.AsNoTracking()
            .Where(d => deviceIds.Contains(d.Id))
            .Select(d => new { d.Id, d.NameAr })
            .ToDictionaryAsync(d => d.Id, d => d.NameAr, ct);

        var salesByCashierRaw = await periodOrders
            .GroupBy(o => o.CashierId)
            .Select(g => new { CashierId = g.Key, Value = g.Sum(x => x.GrandTotal), Count = g.Count() })
            .OrderByDescending(x => x.Value)
            .Take(10)
            .ToListAsync(ct);
        var cashierIds = salesByCashierRaw.Select(x => x.CashierId).ToList();
        var cashierNames = await context.AppUsers.AsNoTracking()
            .Where(u => cashierIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.UserName })
            .ToListAsync(ct);
        var cashierNameMap = cashierNames.ToDictionary(
            u => u.Id,
            u => string.IsNullOrWhiteSpace(u.LastName) ? (u.FirstName ?? u.UserName) : $"{u.FirstName} {u.LastName}");

        var topCustomersRaw = await periodOrders
            .Where(o => o.CustomerId != null)
            .GroupBy(o => o.CustomerId!.Value)
            .Select(g => new
            {
                CustomerId = g.Key,
                Value = g.Sum(x => x.GrandTotal),
                Count = g.Count(),
                LastAt = g.Max(x => x.CompletedAt ?? x.CreatedAt)
            })
            .OrderByDescending(x => x.Value)
            .Take(10)
            .ToListAsync(ct);
        var customerIds = topCustomersRaw.Select(x => x.CustomerId).ToList();
        var customerNames = await context.Customers.AsNoTracking()
            .Where(c => customerIds.Contains(c.Id))
            .Select(c => new { c.Id, c.FullName })
            .ToDictionaryAsync(c => c.Id, c => c.FullName, ct);

        var topItemsRaw = await (
            from item in context.OrderItems.AsNoTracking()
            join order in periodOrders on item.SalesOrderId equals order.Id
            where !item.IsVoided
            group item by new { item.ProductId, item.ProductNameAr } into g
            select new
            {
                g.Key.ProductId,
                Name = g.Key.ProductNameAr,
                Quantity = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.LineTotal)
            })
            .OrderByDescending(x => x.Revenue)
            .Take(10)
            .ToListAsync(ct);

        var byCategory = await (
            from item in context.OrderItems.AsNoTracking()
            join order in periodOrders on item.SalesOrderId equals order.Id
            join product in context.Products.AsNoTracking() on item.ProductId equals product.Id
            join category in context.Categories.AsNoTracking() on product.CategoryId equals category.Id
            where !item.IsVoided
            group item by new { category.Id, category.NameAr } into g
            select new { g.Key.NameAr, Value = g.Sum(i => i.LineTotal), Count = g.Count() })
            .OrderByDescending(x => x.Value)
            .Take(10)
            .ToListAsync(ct);

        var paymentMix = await context.Payments.AsNoTracking()
            .Where(p => p.TenantId == tenantId
                && p.Status == PaymentStatus.Completed
                && p.ProcessedAt >= periodStart
                && p.ProcessedAt <= periodEnd
                && (!filter.BranchId.HasValue || p.BranchId == filter.BranchId))
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new { Method = g.Key, Value = g.Sum(x => x.Amount), Count = g.Count() })
            .OrderByDescending(x => x.Value)
            .ToListAsync(ct);

        var hourly = await periodOrders
            .Select(o => new { At = o.CompletedAt ?? o.CreatedAt, o.GrandTotal })
            .ToListAsync(ct);
        var hourCells = hourly
            .GroupBy(x => new { Dow = (int)x.At.UtcDateTime.DayOfWeek, Hour = x.At.UtcDateTime.Hour })
            .Select(g => new SalesDashboardHourlyCellDto(
                g.Key.Dow, g.Key.Hour, g.Sum(x => x.GrandTotal), g.Count()))
            .ToList();

        var recentOrdersRaw = await periodOrders
            .OrderByDescending(o => o.CompletedAt ?? o.CreatedAt)
            .Take(15)
            .Select(o => new
            {
                o.Id,
                o.OrderNumber,
                o.CustomerId,
                o.GrandTotal,
                o.BranchId,
                o.CashierId,
                o.Status,
                OccurredAt = o.CompletedAt ?? o.CreatedAt
            })
            .ToListAsync(ct);

        var recentOrderIds = recentOrdersRaw.Select(o => o.Id).ToList();
        var recentPayments = await context.Payments.AsNoTracking()
            .Where(p => p.Status == PaymentStatus.Completed
                && p.Allocations.Any(a => recentOrderIds.Contains(a.SalesOrderId)))
            .SelectMany(p => p.Allocations
                .Where(a => recentOrderIds.Contains(a.SalesOrderId))
                .Select(a => new { a.SalesOrderId, p.PaymentMethod, p.ProcessedAt }))
            .ToListAsync(ct);
        var recentPaymentMap = recentPayments
            .GroupBy(x => x.SalesOrderId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.ProcessedAt).First().PaymentMethod.ToString());

        var allBranchIds = recentOrdersRaw.Select(o => o.BranchId).Concat(branchIds).Distinct().ToList();
        var allBranches = await context.Branches.AsNoTracking()
            .Where(b => allBranchIds.Contains(b.Id))
            .Select(b => new { b.Id, b.NameAr })
            .ToDictionaryAsync(b => b.Id, b => b.NameAr, ct);

        var allCashierIds = recentOrdersRaw.Select(o => o.CashierId).Concat(cashierIds).Distinct().ToList();
        var allCashierRows = await context.AppUsers.AsNoTracking()
            .Where(u => allCashierIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.UserName })
            .ToListAsync(ct);
        var allCashiers = allCashierRows.ToDictionary(
            u => u.Id,
            u => string.IsNullOrWhiteSpace(u.LastName) ? (u.FirstName ?? u.UserName) : $"{u.FirstName} {u.LastName}");

        var recentCustomerIds = recentOrdersRaw.Where(o => o.CustomerId.HasValue).Select(o => o.CustomerId!.Value).Distinct().ToList();
        var recentCustomers = await context.Customers.AsNoTracking()
            .Where(c => recentCustomerIds.Contains(c.Id))
            .Select(c => new { c.Id, c.FullName })
            .ToDictionaryAsync(c => c.Id, c => c.FullName, ct);

        var recentReturnsRaw = await returnsQuery
            .OrderByDescending(c => c.IssuedAt)
            .Take(10)
            .Select(c => new
            {
                c.Id,
                c.CreditNoteNumber,
                c.OriginalInvoiceId,
                c.TotalAmount,
                c.Reason,
                c.IssuedAt
            })
            .ToListAsync(ct);

        var invIds = recentReturnsRaw.Select(r => r.OriginalInvoiceId).Distinct().ToList();
        var invoiceNumbers = await context.Invoices.AsNoTracking()
            .Where(i => invIds.Contains(i.Id))
            .Select(i => new { i.Id, i.InvoiceNumber, i.CustomerId })
            .ToDictionaryAsync(i => i.Id, i => i, ct);

        var returnCustomerIds = invoiceNumbers.Values.Where(i => i.CustomerId.HasValue).Select(i => i.CustomerId!.Value).Distinct().ToList();
        var returnCustomers = await context.Customers.AsNoTracking()
            .Where(c => returnCustomerIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.FullName, ct);

        // Alerts
        var alerts = new List<SalesDashboardAlertDto>();
        var unpostedDrafts = await baseQuery.CountAsync(o => o.Status == OrderStatus.Draft, ct);
        if (unpostedDrafts > 0)
        {
            alerts.Add(new("draft_orders", "warning",
                $"{unpostedDrafts} draft order(s) awaiting completion.",
                $"يوجد {unpostedDrafts} طلب(ات) مسودة بانتظار الإكمال.",
                "/pos"));
        }

        var pendingOrders = await baseQuery.CountAsync(
            o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Confirmed, ct);
        if (pendingOrders > 0)
        {
            alerts.Add(new("pending_orders", "warning",
                $"{pendingOrders} order(s) awaiting approval/processing.",
                $"يوجد {pendingOrders} طلب(ات) بانتظار الاعتماد/المعالجة.",
                "/pos"));
        }

        var draftReturns = await context.CreditNotes.AsNoTracking()
            .CountAsync(c => c.TenantId == tenantId && c.Status == CreditNoteStatus.Draft, ct);
        if (draftReturns > 0)
        {
            alerts.Add(new("draft_returns", "warning",
                $"{draftReturns} credit note(s) need approval.",
                $"يوجد {draftReturns} إشعار(ات) دائن بحاجة لاعتماد.",
                "/sales/returns"));
        }

        if (salesYesterday > 0 && salesToday < salesYesterday * 0.85m)
        {
            alerts.Add(new("sales_drop", "danger",
                "Sales today are significantly lower than yesterday.",
                "انخفاض ملحوظ في مبيعات اليوم مقارنة بالأمس.",
                "/sales/dashboard"));
        }

        if (salesPeriod > 0 && returnsRatio >= 10m)
        {
            alerts.Add(new("high_returns", "danger",
                $"Return ratio is high ({returnsRatio:F1}%).",
                $"نسبة المرتجعات مرتفعة ({returnsRatio:F1}%).",
                "/sales/returns"));
        }

        var bestBranch = salesByBranchRaw.FirstOrDefault();
        var worstBranch = salesByBranchRaw.OrderBy(x => x.Value).FirstOrDefault();
        var bestCashier = salesByCashierRaw.FirstOrDefault();
        var topItem = topItemsRaw.FirstOrDefault();
        var topCat = byCategory.FirstOrDefault();
        var topPay = paymentMix.FirstOrDefault();

        var kpis = new SalesDashboardKpisDto(
            SalesToday: salesToday,
            SalesWeek: salesWeek,
            SalesMonth: salesMonth,
            SalesYear: salesYear,
            SalesTodayChangePercent: PctChange(salesToday, salesYesterday),
            SalesWeekChangePercent: PctChange(salesWeek, salesPrevWeek),
            SalesMonthChangePercent: PctChange(salesMonth, salesPrevMonth),
            InvoiceCount: invoiceCount,
            AverageInvoiceValue: avgInvoice,
            HighestInvoice: highest,
            LowestInvoice: lowest,
            NewCustomers: newCustomers,
            ActiveCustomers: activeCustomers,
            InactiveCustomers: inactiveCustomers,
            FirstTimeBuyers: firstTimeBuyers,
            ReturnsTotal: returnsTotal,
            ReturnsRatioPercent: returnsRatio,
            DiscountsTotal: discountsTotal,
            CancellationsTotal: cancellationsTotal,
            PosInvoiceCount: posCount,
            AverageInvoiceMinutes: Math.Round(avgMinutes, 1),
            AverageItemsPerInvoice: Math.Round(avgItems, 2),
            MostActivePosDevice: mostActiveDevice,
            GrossProfit: grossProfit,
            ProfitMarginPercent: margin,
            Cogs: cogs,
            NetProfit: netProfit,
            SalesGrowthPercent: PctChange(salesMonth, salesPrevMonth),
            NewCustomerRatioPercent: newCustomerRatio,
            ReturningCustomerRatioPercent: returningRatio,
            DiscountRatioPercent: discountRatio,
            BestBranchName: bestBranch is null ? null : branchNames.GetValueOrDefault(bestBranch.BranchId),
            WorstBranchName: worstBranch is null ? null : branchNames.GetValueOrDefault(worstBranch.BranchId),
            BestCashierName: bestCashier is null ? null : cashierNameMap.GetValueOrDefault(bestCashier.CashierId),
            TopSellingItemName: topItem?.Name,
            TopSellingCategoryName: topCat?.NameAr,
            TopPaymentMethodName: topPay?.Method.ToString());

        var charts = new SalesDashboardChartsDto(
            SalesByDay: salesByDay.Select(x => new SalesDashboardNamedValueDto(x.Day.ToString("yyyy-MM-dd"), x.Value, x.Count)).ToList(),
            SalesByBranch: salesByBranchRaw.Select(x => new SalesDashboardNamedValueDto(
                branchNames.GetValueOrDefault(x.BranchId) ?? x.BranchId.ToString("N")[..8], x.Value, x.Count)).ToList(),
            SalesByPosDevice: salesByDeviceRaw.Select(x => new SalesDashboardNamedValueDto(
                deviceNames.GetValueOrDefault(x.DeviceId) ?? x.DeviceId.ToString("N")[..8], x.Value, x.Count)).ToList(),
            SalesByCashier: salesByCashierRaw.Select(x => new SalesDashboardNamedValueDto(
                cashierNameMap.GetValueOrDefault(x.CashierId) ?? "—", x.Value, x.Count)).ToList(),
            TopCustomers: topCustomersRaw.Select(x => new SalesDashboardNamedValueDto(
                customerNames.GetValueOrDefault(x.CustomerId) ?? "—", x.Value, x.Count)).ToList(),
            TopItems: topItemsRaw.Select(x => new SalesDashboardNamedValueDto(x.Name, x.Revenue, (int)x.Quantity)).ToList(),
            SalesByCategory: byCategory.Select(x => new SalesDashboardNamedValueDto(x.NameAr, x.Value, x.Count)).ToList(),
            PaymentMethods: paymentMix.Select(x => new SalesDashboardNamedValueDto(x.Method.ToString(), x.Value, x.Count)).ToList(),
            SalesByHour: hourCells);

        var recentOrders = recentOrdersRaw.Select(o => new SalesDashboardRecentOrderDto(
            o.Id,
            o.OrderNumber,
            o.CustomerId.HasValue ? recentCustomers.GetValueOrDefault(o.CustomerId.Value) : null,
            o.GrandTotal,
            recentPaymentMap.GetValueOrDefault(o.Id),
            allBranches.GetValueOrDefault(o.BranchId),
            allCashiers.GetValueOrDefault(o.CashierId),
            o.Status.ToString(),
            o.OccurredAt)).ToList();

        var recentReturns = recentReturnsRaw.Select(r =>
        {
            invoiceNumbers.TryGetValue(r.OriginalInvoiceId, out var inv);
            string? cust = null;
            if (inv?.CustomerId is Guid cid)
                cust = returnCustomers.GetValueOrDefault(cid);
            return new SalesDashboardRecentReturnDto(
                r.Id, r.CreditNoteNumber, inv?.InvoiceNumber, cust, r.TotalAmount, r.Reason, r.IssuedAt);
        }).ToList();

        var topCustomers = topCustomersRaw.Select(x => new SalesDashboardTopCustomerDto(
            x.CustomerId,
            customerNames.GetValueOrDefault(x.CustomerId) ?? "—",
            x.Value,
            x.Count,
            x.LastAt)).ToList();

        var topItems = topItemsRaw.Select(x => new SalesDashboardTopItemDto(
            x.ProductId, x.Name, x.Quantity, x.Revenue, x.Revenue)).ToList();

        return new SalesDashboardDto(
            DateTimeOffset.UtcNow,
            kpis,
            charts,
            recentOrders,
            recentReturns,
            topCustomers,
            topItems,
            alerts);
    }

    private static decimal PctChange(decimal current, decimal previous)
    {
        if (previous == 0) return current > 0 ? 100m : 0m;
        return Math.Round((current - previous) / previous * 100m, 1);
    }

    private static DateTimeOffset ToUtc(DateOnly d) =>
        new(d.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

    private static DateTimeOffset ToUtcEnd(DateOnly d) =>
        new(d.ToDateTime(new TimeOnly(23, 59, 59)), TimeSpan.Zero);
}
