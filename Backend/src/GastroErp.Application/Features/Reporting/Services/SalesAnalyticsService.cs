using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.Reporting.DTOs;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Reporting.Services;

public sealed class SalesAnalyticsService : ISalesAnalyticsService
{
    private readonly IApplicationDbContext _context;
    public SalesAnalyticsService(IApplicationDbContext context) => _context = context;

    private IQueryable<Domain.Entities.Sales.SalesOrder> Orders(Guid tenantId, ReportFilterDto filter) =>
        ReportQueryHelper.FilterOrders(_context.SalesOrders, tenantId, filter);

    public async Task<IReadOnlyList<PeriodSalesDto>> GetDailySalesAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
        => await GroupPeriodAsync(Orders(tenantId, filter), o => DateOnly.FromDateTime((o.CompletedAt ?? o.CreatedAt).UtcDateTime).ToString("yyyy-MM-dd"), ct);

    public async Task<IReadOnlyList<PeriodSalesDto>> GetMonthlySalesAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
        => await GroupPeriodAsync(Orders(tenantId, filter), o => $"{(o.CompletedAt ?? o.CreatedAt).UtcDateTime:yyyy-MM}", ct);

    public async Task<IReadOnlyList<PeriodSalesDto>> GetYearlySalesAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
        => await GroupPeriodAsync(Orders(tenantId, filter), o => $"{(o.CompletedAt ?? o.CreatedAt).UtcDateTime:yyyy}", ct);

    public async Task<IReadOnlyList<BranchSalesDto>> GetSalesByBranchAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var data = await Orders(tenantId, filter)
            .GroupBy(o => o.BranchId)
            .Select(g => new { BranchId = g.Key, Count = g.Count(), Revenue = g.Sum(o => o.GrandTotal) })
            .ToListAsync(ct);
        var branchIds = data.Select(d => d.BranchId).ToList();
        var branches = await _context.Branches.AsNoTracking()
            .Where(b => branchIds.Contains(b.Id)).ToDictionaryAsync(b => b.Id, b => b.NameAr, ct);
        return data.Select(d => new BranchSalesDto(
            d.BranchId, branches.GetValueOrDefault(d.BranchId, "Unknown"),
            d.Count, d.Revenue, d.Count > 0 ? d.Revenue / d.Count : 0)).OrderByDescending(x => x.Revenue).ToList();
    }

    public async Task<IReadOnlyList<CashierSalesDto>> GetSalesByCashierAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var data = await Orders(tenantId, filter)
            .GroupBy(o => o.CashierId)
            .Select(g => new { CashierId = g.Key, Count = g.Count(), Revenue = g.Sum(o => o.GrandTotal) })
            .ToListAsync(ct);
        var ids = data.Select(d => d.CashierId).ToList();
        var users = await _context.AppUsers.AsNoTracking()
            .Where(u => ids.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.FirstName + " " + u.LastName, ct);
        return data.Select(d => new CashierSalesDto(
            d.CashierId, users.GetValueOrDefault(d.CashierId, "Unknown"), d.Count, d.Revenue))
            .OrderByDescending(x => x.Revenue).ToList();
    }

    public async Task<IReadOnlyList<ProductSalesDto>> GetSalesByProductAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
        => await (
            from item in _context.OrderItems.AsNoTracking()
            join order in Orders(tenantId, filter) on item.SalesOrderId equals order.Id
            where !item.IsVoided
            group item by new { item.ProductId, item.ProductNameAr } into g
            select new ProductSalesDto(g.Key.ProductId, g.Key.ProductNameAr, g.Sum(i => i.Quantity), g.Sum(i => i.LineTotal)))
            .OrderByDescending(x => x.Revenue).Take(100).ToListAsync(ct);

    public async Task<IReadOnlyList<CategorySalesDto>> GetSalesByCategoryAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
        => await (
            from item in _context.OrderItems.AsNoTracking()
            join order in Orders(tenantId, filter) on item.SalesOrderId equals order.Id
            join product in _context.Products.AsNoTracking() on item.ProductId equals product.Id
            join category in _context.Categories.AsNoTracking() on product.CategoryId equals category.Id
            where !item.IsVoided
            group item by new { category.Id, category.NameAr } into g
            select new CategorySalesDto(g.Key.Id, g.Key.NameAr, g.Sum(i => i.Quantity), g.Sum(i => i.LineTotal)))
            .OrderByDescending(x => x.Revenue).Take(100).ToListAsync(ct);

    public async Task<IReadOnlyList<HourlySalesDto>> GetSalesByHourAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
        => await Orders(tenantId, filter)
            .GroupBy(o => (o.CompletedAt ?? o.CreatedAt).UtcDateTime.Hour)
            .Select(g => new HourlySalesDto(g.Key, g.Count(), g.Sum(o => o.GrandTotal)))
            .OrderBy(x => x.Hour).ToListAsync(ct);

    public async Task<IReadOnlyList<OrderTypeSalesDto>> GetSalesByOrderTypeAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
        => await Orders(tenantId, filter)
            .GroupBy(o => o.OrderType)
            .Select(g => new OrderTypeSalesDto(g.Key.ToString(), g.Count(), g.Sum(o => o.GrandTotal)))
            .OrderByDescending(x => x.Revenue).ToListAsync(ct);

    public async Task<IReadOnlyList<PaymentMethodSalesDto>> GetSalesByPaymentMethodAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var (from, to) = ReportQueryHelper.ResolveDateRange(filter);
        var query = _context.Payments.AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.Status == PaymentStatus.Completed
                && p.ProcessedAt >= from && p.ProcessedAt <= to);
        if (filter.BranchId.HasValue) query = query.Where(p => p.BranchId == filter.BranchId);
        return await query.GroupBy(p => p.PaymentMethod)
            .Select(g => new PaymentMethodSalesDto(g.Key.ToString(), g.Count(), g.Sum(p => p.Amount)))
            .OrderByDescending(x => x.Amount).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<CancelledOrderDto>> GetCancelledOrdersAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var (from, to) = ReportQueryHelper.ResolveDateRange(filter);
        var query = _context.SalesOrders.AsNoTracking()
            .Where(o => o.TenantId == tenantId && o.Status == OrderStatus.Cancelled
                && o.CancelledAt >= from && o.CancelledAt <= to);
        if (filter.BranchId.HasValue) query = query.Where(o => o.BranchId == filter.BranchId);
        return await query.Select(o => new CancelledOrderDto(
            o.Id, o.OrderNumber, o.CancelledAt!.Value, o.CancellationReason, o.GrandTotal))
            .OrderByDescending(x => x.CancelledAt).Take(500).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<DiscountReportDto>> GetDiscountReportAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
        => await Orders(tenantId, filter).Where(o => o.DiscountTotal > 0)
            .Select(o => new DiscountReportDto(o.Id, o.OrderNumber, o.DiscountTotal, o.GrandTotal, o.CompletedAt ?? o.CreatedAt))
            .OrderByDescending(x => x.DiscountTotal).Take(500).ToListAsync(ct);

    public async Task<VatSalesReportDto> GetVatReportAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var (from, to) = ReportQueryHelper.ResolveDateRange(filter);
        var query = _context.Invoices.AsNoTracking()
            .Where(i => i.TenantId == tenantId && i.Status == InvoiceStatus.Finalized
                && i.FinalizedAt >= from && i.FinalizedAt <= to);
        if (filter.BranchId.HasValue) query = query.Where(i => i.BranchId == filter.BranchId);
        var taxable = await query.SumAsync(i => i.SubTotal - i.DiscountTotal, ct);
        var vat = await query.SumAsync(i => i.TaxTotal, ct);
        var count = await query.CountAsync(ct);
        return new VatSalesReportDto(taxable, vat, count);
    }

    private static async Task<IReadOnlyList<PeriodSalesDto>> GroupPeriodAsync(
        IQueryable<Domain.Entities.Sales.SalesOrder> query,
        System.Linq.Expressions.Expression<Func<Domain.Entities.Sales.SalesOrder, string>> periodSelector,
        CancellationToken ct)
    {
        var grouped = await query.GroupBy(periodSelector)
            .Select(g => new { Period = g.Key, Count = g.Count(), Revenue = g.Sum(o => o.GrandTotal),
                Discount = g.Sum(o => o.DiscountTotal), Tax = g.Sum(o => o.TaxTotal) })
            .OrderBy(x => x.Period).ToListAsync(ct);
        return grouped.Select(g => new PeriodSalesDto(
            g.Period, g.Count, g.Revenue, g.Count > 0 ? g.Revenue / g.Count : 0, g.Discount, g.Tax)).ToList();
    }
}
