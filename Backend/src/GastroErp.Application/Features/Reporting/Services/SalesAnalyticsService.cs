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
    {
        // Materialize first — DateOnly.ToString is not translatable by EF Core.
        var rows = await Orders(tenantId, filter)
            .Select(o => new
            {
                At = o.CompletedAt ?? o.CreatedAt,
                o.GrandTotal,
                o.DiscountTotal,
                o.TaxTotal
            })
            .ToListAsync(ct);

        return rows
            .GroupBy(o => DateOnly.FromDateTime(o.At.UtcDateTime).ToString("yyyy-MM-dd"))
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var count = g.Count();
                var revenue = g.Sum(x => x.GrandTotal);
                return new PeriodSalesDto(
                    g.Key,
                    count,
                    revenue,
                    count > 0 ? revenue / count : 0,
                    g.Sum(x => x.DiscountTotal),
                    g.Sum(x => x.TaxTotal));
            })
            .ToList();
    }

    public async Task<IReadOnlyList<PeriodSalesDto>> GetMonthlySalesAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var rows = await Orders(tenantId, filter)
            .Select(o => new { At = o.CompletedAt ?? o.CreatedAt, o.GrandTotal, o.DiscountTotal, o.TaxTotal })
            .ToListAsync(ct);

        return rows
            .GroupBy(o => $"{o.At.UtcDateTime:yyyy-MM}")
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var count = g.Count();
                var revenue = g.Sum(x => x.GrandTotal);
                return new PeriodSalesDto(g.Key, count, revenue, count > 0 ? revenue / count : 0,
                    g.Sum(x => x.DiscountTotal), g.Sum(x => x.TaxTotal));
            })
            .ToList();
    }

    public async Task<IReadOnlyList<PeriodSalesDto>> GetYearlySalesAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var rows = await Orders(tenantId, filter)
            .Select(o => new { At = o.CompletedAt ?? o.CreatedAt, o.GrandTotal, o.DiscountTotal, o.TaxTotal })
            .ToListAsync(ct);

        return rows
            .GroupBy(o => $"{o.At.UtcDateTime:yyyy}")
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var count = g.Count();
                var revenue = g.Sum(x => x.GrandTotal);
                return new PeriodSalesDto(g.Key, count, revenue, count > 0 ? revenue / count : 0,
                    g.Sum(x => x.DiscountTotal), g.Sum(x => x.TaxTotal));
            })
            .ToList();
    }

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
    {
        var orderIds = await Orders(tenantId, filter).Select(o => o.Id).ToListAsync(ct);
        if (orderIds.Count == 0) return [];

        var rows = await _context.OrderItems.AsNoTracking()
            .Where(i => orderIds.Contains(i.SalesOrderId) && !i.IsVoided)
            .Select(i => new { i.ProductId, i.ProductNameAr, i.Quantity, i.LineTotal })
            .ToListAsync(ct);

        return rows
            .GroupBy(i => new { i.ProductId, i.ProductNameAr })
            .Select(g => new ProductSalesDto(g.Key.ProductId, g.Key.ProductNameAr, g.Sum(x => x.Quantity), g.Sum(x => x.LineTotal)))
            .OrderByDescending(x => x.Revenue)
            .Take(100)
            .ToList();
    }

    public async Task<IReadOnlyList<CategorySalesDto>> GetSalesByCategoryAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var orderIds = await Orders(tenantId, filter).Select(o => o.Id).ToListAsync(ct);
        if (orderIds.Count == 0) return [];

        var rows = await (
            from item in _context.OrderItems.AsNoTracking()
            join product in _context.Products.AsNoTracking() on item.ProductId equals product.Id
            join category in _context.Categories.AsNoTracking() on product.CategoryId equals category.Id
            where orderIds.Contains(item.SalesOrderId) && !item.IsVoided
            select new { category.Id, category.NameAr, item.Quantity, item.LineTotal })
            .ToListAsync(ct);

        return rows
            .GroupBy(x => new { x.Id, x.NameAr })
            .Select(g => new CategorySalesDto(g.Key.Id, g.Key.NameAr, g.Sum(x => x.Quantity), g.Sum(x => x.LineTotal)))
            .OrderByDescending(x => x.Revenue)
            .Take(100)
            .ToList();
    }

    public async Task<IReadOnlyList<HourlySalesDto>> GetSalesByHourAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var rows = await Orders(tenantId, filter)
            .Select(o => new { At = o.CompletedAt ?? o.CreatedAt, o.GrandTotal })
            .ToListAsync(ct);

        return rows
            .GroupBy(o => o.At.UtcDateTime.Hour)
            .Select(g => new HourlySalesDto(g.Key, g.Count(), g.Sum(x => x.GrandTotal)))
            .OrderBy(x => x.Hour)
            .ToList();
    }

    public async Task<IReadOnlyList<OrderTypeSalesDto>> GetSalesByOrderTypeAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var data = await Orders(tenantId, filter)
            .GroupBy(o => o.OrderType)
            .Select(g => new { Type = g.Key, Count = g.Count(), Revenue = g.Sum(o => o.GrandTotal) })
            .ToListAsync(ct);

        return data
            .OrderByDescending(x => x.Revenue)
            .Select(x => new OrderTypeSalesDto(x.Type.ToString(), x.Count, x.Revenue))
            .ToList();
    }

    public async Task<IReadOnlyList<PaymentMethodSalesDto>> GetSalesByPaymentMethodAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var (from, to) = ReportQueryHelper.ResolveDateRange(filter);
        var query = _context.Payments.AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.Status == PaymentStatus.Completed
                && p.ProcessedAt >= from && p.ProcessedAt <= to);
        if (filter.BranchId.HasValue) query = query.Where(p => p.BranchId == filter.BranchId);

        var data = await query
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new { Method = g.Key, Count = g.Count(), Amount = g.Sum(p => p.Amount) })
            .ToListAsync(ct);

        return data
            .OrderByDescending(x => x.Amount)
            .Select(x => new PaymentMethodSalesDto(x.Method.ToString(), x.Count, x.Amount))
            .ToList();
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
