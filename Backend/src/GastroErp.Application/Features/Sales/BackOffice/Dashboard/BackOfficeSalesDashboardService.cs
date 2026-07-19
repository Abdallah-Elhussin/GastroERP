using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Sales.BackOffice.Dashboard;

public record GetBackOfficeSalesDashboardQuery(Guid TenantId, BackOfficeSalesDashboardFilterDto Filter)
    : IRequest<Result<BackOfficeSalesDashboardDto>>;

public sealed class GetBackOfficeSalesDashboardQueryHandler(IBackOfficeSalesDashboardService service)
    : IRequestHandler<GetBackOfficeSalesDashboardQuery, Result<BackOfficeSalesDashboardDto>>
{
    public async Task<Result<BackOfficeSalesDashboardDto>> Handle(
        GetBackOfficeSalesDashboardQuery request, CancellationToken cancellationToken)
    {
        var data = await service.GetAsync(request.TenantId, request.Filter, cancellationToken);
        return Result<BackOfficeSalesDashboardDto>.Success(data);
    }
}

public interface IBackOfficeSalesDashboardService
{
    Task<BackOfficeSalesDashboardDto> GetAsync(
        Guid tenantId, BackOfficeSalesDashboardFilterDto filter, CancellationToken ct = default);
}

public sealed class BackOfficeSalesDashboardService(IApplicationDbContext context) : IBackOfficeSalesDashboardService
{
    public async Task<BackOfficeSalesDashboardDto> GetAsync(
        Guid tenantId, BackOfficeSalesDashboardFilterDto filter, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var periodFrom = filter.FromDate ?? today.AddDays(-30);
        var periodTo = filter.ToDate ?? today;

        var query = context.BackOfficeSalesInvoices.AsNoTracking()
            .Where(i => i.TenantId == tenantId && !i.IsDeleted);

        if (filter.BranchId.HasValue)
            query = query.Where(i => i.BranchId == filter.BranchId);
        if (filter.CustomerId.HasValue)
            query = query.Where(i => i.CustomerId == filter.CustomerId);
        if (filter.Status.HasValue)
            query = query.Where(i => i.Status == filter.Status);

        var posted = query.Where(i => i.Status == BackOfficeSalesDocumentStatus.Posted);

        async Task<decimal> SumPosted(DateOnly from, DateOnly to) =>
            await posted
                .Where(i => i.InvoiceDate >= from && i.InvoiceDate <= to)
                .SumAsync(i => (decimal?)i.TotalAmount, ct) ?? 0;

        var salesToday = await SumPosted(today, today);
        var salesYesterday = await SumPosted(today.AddDays(-1), today.AddDays(-1));
        var salesWeek = await SumPosted(today.AddDays(-6), today);
        var salesMonth = await SumPosted(new DateOnly(today.Year, today.Month, 1), today);
        var salesPeriod = await SumPosted(periodFrom, periodTo);

        var periodPosted = posted.Where(i => i.InvoiceDate >= periodFrom && i.InvoiceDate <= periodTo);

        var invoiceCount = await periodPosted.CountAsync(ct);
        var average = invoiceCount > 0
            ? await periodPosted.AverageAsync(i => i.TotalAmount, ct)
            : 0m;

        var draftCount = await query.CountAsync(i => i.Status == BackOfficeSalesDocumentStatus.Draft, ct);
        var approvedCount = await query.CountAsync(i => i.Status == BackOfficeSalesDocumentStatus.Approved, ct);
        var postedCount = await query.CountAsync(i => i.Status == BackOfficeSalesDocumentStatus.Posted, ct);

        var creditOutstanding = await posted
            .Where(i => i.PaymentMode == BackOfficeSalesPaymentMode.Credit && i.PaidAmount < i.TotalAmount)
            .SumAsync(i => (decimal?)(i.TotalAmount - i.PaidAmount), ct) ?? 0;

        var cashSalesPeriod = await periodPosted
            .Where(i => i.PaymentMode == BackOfficeSalesPaymentMode.Cash)
            .SumAsync(i => (decimal?)i.TotalAmount, ct) ?? 0;
        var creditSalesPeriod = await periodPosted
            .Where(i => i.PaymentMode == BackOfficeSalesPaymentMode.Credit)
            .SumAsync(i => (decimal?)i.TotalAmount, ct) ?? 0;

        var activeCustomers = await periodPosted.Select(i => i.CustomerId).Distinct().CountAsync(ct);

        var salesTodayChange = salesYesterday == 0
            ? (salesToday > 0 ? 100m : 0m)
            : Math.Round((salesToday - salesYesterday) / salesYesterday * 100m, 1);

        var byDayRaw = await periodPosted
            .GroupBy(i => i.InvoiceDate)
            .Select(g => new { Date = g.Key, Value = g.Sum(x => x.TotalAmount), Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync(ct);

        var salesByDay = byDayRaw
            .Select(x => new BackOfficeSalesDashboardNamedValueDto(x.Date.ToString("yyyy-MM-dd"), x.Value, x.Count))
            .ToList();

        var byCustomer = await periodPosted
            .GroupBy(i => i.CustomerId)
            .Select(g => new { CustomerId = g.Key, Value = g.Sum(x => x.TotalAmount), Count = g.Count() })
            .OrderByDescending(x => x.Value)
            .Take(10)
            .ToListAsync(ct);

        var customerIds = byCustomer.Select(x => x.CustomerId).ToList();
        var customerNames = await context.Customers.AsNoTracking()
            .Where(c => customerIds.Contains(c.Id))
            .Select(c => new { c.Id, c.FullName })
            .ToDictionaryAsync(c => c.Id, c => c.FullName, ct);

        var salesByCustomer = byCustomer
            .Select(x => new BackOfficeSalesDashboardNamedValueDto(
                customerNames.GetValueOrDefault(x.CustomerId, x.CustomerId.ToString("N")[..8]),
                x.Value,
                x.Count))
            .ToList();

        var byNature = await periodPosted
            .GroupBy(i => i.Nature)
            .Select(g => new { Nature = g.Key, Value = g.Sum(x => x.TotalAmount), Count = g.Count() })
            .ToListAsync(ct);

        var salesByNature = byNature
            .Select(x => new BackOfficeSalesDashboardNamedValueDto(x.Nature.ToString(), x.Value, x.Count))
            .ToList();

        var byPay = await periodPosted
            .GroupBy(i => i.PaymentMode)
            .Select(g => new { Mode = g.Key, Value = g.Sum(x => x.TotalAmount), Count = g.Count() })
            .ToListAsync(ct);

        var salesByPayment = byPay
            .Select(x => new BackOfficeSalesDashboardNamedValueDto(x.Mode.ToString(), x.Value, x.Count))
            .ToList();

        var recent = await posted
            .OrderByDescending(i => i.PostedAt ?? i.CreatedAt)
            .Take(10)
            .Select(i => new
            {
                i.Id,
                i.InvoiceNumber,
                i.CustomerId,
                i.TotalAmount,
                i.Status,
                i.PaymentMode,
                i.InvoiceDate,
                i.PostedAt
            })
            .ToListAsync(ct);

        var recentCustomerIds = recent.Select(r => r.CustomerId).Distinct().ToList();
        var recentNames = await context.Customers.AsNoTracking()
            .Where(c => recentCustomerIds.Contains(c.Id))
            .Select(c => new { c.Id, c.FullName })
            .ToDictionaryAsync(c => c.Id, c => c.FullName, ct);

        var recentInvoices = recent
            .Select(r => new BackOfficeSalesDashboardRecentInvoiceDto(
                r.Id,
                r.InvoiceNumber,
                recentNames.GetValueOrDefault(r.CustomerId),
                r.TotalAmount,
                r.Status,
                r.PaymentMode,
                r.InvoiceDate,
                r.PostedAt))
            .ToList();

        var alerts = new List<BackOfficeSalesDashboardAlertDto>();
        if (draftCount > 0)
        {
            alerts.Add(new BackOfficeSalesDashboardAlertDto(
                "DRAFTS_PENDING",
                "warning",
                $"{draftCount} draft invoice(s) awaiting approval.",
                $"يوجد {draftCount} فاتورة مسودة بانتظار الاعتماد.",
                "/sales/invoices"));
        }

        if (approvedCount > 0)
        {
            alerts.Add(new BackOfficeSalesDashboardAlertDto(
                "APPROVED_UNPOSTED",
                "info",
                $"{approvedCount} approved invoice(s) not yet posted.",
                $"يوجد {approvedCount} فاتورة معتمدة لم تُرحّل بعد.",
                "/sales/invoices"));
        }

        if (creditOutstanding > 0)
        {
            alerts.Add(new BackOfficeSalesDashboardAlertDto(
                "CREDIT_OUTSTANDING",
                "warning",
                $"Credit outstanding: {creditOutstanding:N2}.",
                $"ذمم مدينة قائمة: {creditOutstanding:N2}.",
                "/sales/invoices"));
        }

        var overdueCount = await posted
            .Where(i => i.PaymentMode == BackOfficeSalesPaymentMode.Credit
                && i.DueDate != null
                && i.DueDate < today
                && i.PaidAmount < i.TotalAmount)
            .CountAsync(ct);

        if (overdueCount > 0)
        {
            alerts.Add(new BackOfficeSalesDashboardAlertDto(
                "OVERDUE_INVOICES",
                "danger",
                $"{overdueCount} overdue credit invoice(s).",
                $"يوجد {overdueCount} فاتورة آجلة متأخرة السداد.",
                "/sales/invoices"));
        }

        return new BackOfficeSalesDashboardDto(
            DateTimeOffset.UtcNow,
            new BackOfficeSalesDashboardKpisDto(
                salesToday,
                salesWeek,
                salesMonth,
                salesPeriod,
                salesTodayChange,
                invoiceCount,
                draftCount,
                approvedCount,
                postedCount,
                average,
                creditOutstanding,
                cashSalesPeriod,
                creditSalesPeriod,
                activeCustomers),
            salesByDay,
            salesByCustomer,
            salesByNature,
            salesByPayment,
            recentInvoices,
            alerts);
    }
}
