using GastroErp.Application.Features.Reporting.DTOs;
using GastroErp.Domain.Entities.Sales;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Reporting;

internal static class ReportQueryHelper
{
    public static (DateTimeOffset From, DateTimeOffset To) ResolveDateRange(ReportFilterDto filter)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var fromDate = filter.FromDate ?? today.AddDays(-30);
        var toDate = filter.ToDate ?? today;
        var from = new DateTimeOffset(fromDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var to = new DateTimeOffset(toDate.ToDateTime(new TimeOnly(23, 59, 59)), TimeSpan.Zero);
        return (from, to);
    }

    public static IQueryable<SalesOrder> FilterOrders(
        IQueryable<SalesOrder> query, Guid tenantId, ReportFilterDto filter, bool completedOnly = true)
    {
        var (from, to) = ResolveDateRange(filter);
        query = query.AsNoTracking().Where(o => o.TenantId == tenantId);
        if (completedOnly)
            query = query.Where(o => o.Status == OrderStatus.Completed);
        if (filter.BranchId.HasValue)
            query = query.Where(o => o.BranchId == filter.BranchId);
        if (filter.CompanyId.HasValue)
            query = query.Where(o => o.CompanyId == filter.CompanyId);
        return query.Where(o => (o.CompletedAt ?? o.CreatedAt) >= from && (o.CompletedAt ?? o.CreatedAt) <= to);
    }

    public static (int Page, int PageSize) NormalizePaging(int page, int pageSize) =>
        (Math.Max(page, 1), Math.Clamp(pageSize, 1, 500));
}
