using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Sales.BackOffice.Reports;

public record GetBackOfficeSalesReportQuery(
    Guid TenantId,
    DateOnly? From = null,
    DateOnly? To = null,
    Guid? CustomerId = null,
    Guid? BranchId = null,
    int TopCustomers = 20,
    int TopItems = 20) : IRequest<Result<BackOfficeSalesReportDto>>;

public sealed class GetBackOfficeSalesReportQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetBackOfficeSalesReportQuery, Result<BackOfficeSalesReportDto>>
{
    public async Task<Result<BackOfficeSalesReportDto>> Handle(
        GetBackOfficeSalesReportQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var from = request.From ?? today.AddDays(-30);
        var to = request.To ?? today;
        if (to < from)
            return Result<BackOfficeSalesReportDto>.Failure("InvalidDateRange", "To date cannot precede From date.");
        var topCustomers = request.TopCustomers is <= 0 or > 200 ? 20 : request.TopCustomers;
        var topItems = request.TopItems is <= 0 or > 200 ? 20 : request.TopItems;

        var invoiceQuery = context.BackOfficeSalesInvoices.AsNoTracking()
            .Where(i => i.TenantId == request.TenantId);
        if (request.CustomerId.HasValue)
            invoiceQuery = invoiceQuery.Where(i => i.CustomerId == request.CustomerId.Value);
        if (request.BranchId.HasValue)
            invoiceQuery = invoiceQuery.Where(i => i.BranchId == request.BranchId.Value);

        var returnQuery = context.BackOfficeSalesReturns.AsNoTracking()
            .Where(r => r.TenantId == request.TenantId);
        if (request.CustomerId.HasValue)
            returnQuery = returnQuery.Where(r => r.CustomerId == request.CustomerId.Value);
        if (request.BranchId.HasValue)
            returnQuery = returnQuery.Where(r => r.BranchId == request.BranchId.Value);

        var debitQuery = context.BackOfficeSalesDebitNotes.AsNoTracking()
            .Where(d => d.TenantId == request.TenantId);
        if (request.CustomerId.HasValue)
            debitQuery = debitQuery.Where(d => d.CustomerId == request.CustomerId.Value);
        if (request.BranchId.HasValue)
            debitQuery = debitQuery.Where(d => d.BranchId == request.BranchId.Value);

        var postedInvoices = invoiceQuery
            .Where(i => i.Status == BackOfficeSalesDocumentStatus.Posted
                        && i.InvoiceDate >= from && i.InvoiceDate <= to);
        var postedReturns = returnQuery
            .Where(r => r.Status == BackOfficeSalesDocumentStatus.Posted
                        && r.ReturnDate >= from && r.ReturnDate <= to);
        var postedDebits = debitQuery
            .Where(d => d.Status == BackOfficeSalesDocumentStatus.Posted
                        && d.DebitDate >= from && d.DebitDate <= to);

        var grossSales = await postedInvoices.SumAsync(i => (decimal?)i.TotalAmount, cancellationToken) ?? 0;
        var taxAmount = await postedInvoices.SumAsync(i => (decimal?)i.TaxAmount, cancellationToken) ?? 0;
        var totalReturns = await postedReturns.SumAsync(r => (decimal?)r.TotalAmount, cancellationToken) ?? 0;
        var totalDebitNotes = await postedDebits.SumAsync(d => (decimal?)d.TotalAmount, cancellationToken) ?? 0;

        var invoiceCount = await postedInvoices.CountAsync(cancellationToken);
        var returnCount = await postedReturns.CountAsync(cancellationToken);
        var debitCount = await postedDebits.CountAsync(cancellationToken);

        var summary = new BackOfficeSalesReportSummaryDto(
            grossSales, taxAmount, totalReturns, totalDebitNotes,
            NetSales: Math.Round(grossSales - totalReturns + totalDebitNotes, 4),
            invoiceCount, returnCount, debitCount);

        var byCustomerRaw = await postedInvoices
            .GroupBy(i => i.CustomerId)
            .Select(g => new { CustomerId = g.Key, Value = g.Sum(x => x.TotalAmount), Count = g.Count() })
            .OrderByDescending(x => x.Value)
            .Take(topCustomers)
            .ToListAsync(cancellationToken);

        var customerIds = byCustomerRaw.Select(x => x.CustomerId).ToList();
        var customerNames = await context.Customers.AsNoTracking()
            .Where(c => customerIds.Contains(c.Id))
            .Select(c => new { c.Id, c.FullName })
            .ToDictionaryAsync(c => c.Id, c => c.FullName, cancellationToken);

        var salesByCustomer = byCustomerRaw
            .Select(x => new BackOfficeSalesReportNamedValueDto(
                customerNames.GetValueOrDefault(x.CustomerId, x.CustomerId.ToString("N")[..8]),
                x.Value, x.Count))
            .ToList();

        var byItemRaw = await context.BackOfficeSalesInvoices.AsNoTracking()
            .Where(i => i.TenantId == request.TenantId
                && i.Status == BackOfficeSalesDocumentStatus.Posted
                && i.InvoiceDate >= from && i.InvoiceDate <= to
                && (!request.CustomerId.HasValue || i.CustomerId == request.CustomerId.Value)
                && (!request.BranchId.HasValue || i.BranchId == request.BranchId.Value))
            .SelectMany(i => i.Lines)
            .GroupBy(l => new { l.InventoryItemId, l.Description })
            .Select(g => new
            {
                g.Key.InventoryItemId,
                g.Key.Description,
                Value = g.Sum(x => x.LineNet + x.TaxAmount),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Value)
            .Take(topItems)
            .ToListAsync(cancellationToken);

        var salesByItem = byItemRaw
            .Select(x => new BackOfficeSalesReportNamedValueDto(
                string.IsNullOrWhiteSpace(x.Description) ? "—" : x.Description,
                x.Value, x.Count))
            .ToList();

        var byDayRaw = await postedInvoices
            .GroupBy(i => i.InvoiceDate)
            .Select(g => new { Date = g.Key, Value = g.Sum(x => x.TotalAmount), Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        var salesByDay = byDayRaw
            .Select(x => new BackOfficeSalesReportNamedValueDto(
                x.Date.ToString("yyyy-MM-dd"), x.Value, x.Count))
            .ToList();

        var invoiceCounts = await CountByStatusAsync(
            invoiceQuery.Where(i => i.InvoiceDate >= from && i.InvoiceDate <= to),
            i => i.Status, cancellationToken);
        var returnCounts = await CountByStatusAsync(
            returnQuery.Where(r => r.ReturnDate >= from && r.ReturnDate <= to),
            r => r.Status, cancellationToken);
        var debitCounts = await CountByStatusAsync(
            debitQuery.Where(d => d.DebitDate >= from && d.DebitDate <= to),
            d => d.Status, cancellationToken);

        var orderQuery = context.BackOfficeSalesOrders.AsNoTracking()
            .Where(o => o.TenantId == request.TenantId
                && o.OrderDate >= from && o.OrderDate <= to
                && (!request.CustomerId.HasValue || o.CustomerId == request.CustomerId.Value)
                && (!request.BranchId.HasValue || o.BranchId == request.BranchId.Value));
        var orderCounts = await CountByStatusAsync(orderQuery, o => o.Status, cancellationToken);

        var quotationQuery = context.BackOfficeSalesQuotations.AsNoTracking()
            .Where(q => q.TenantId == request.TenantId
                && q.QuotationDate >= from && q.QuotationDate <= to
                && (!request.CustomerId.HasValue || q.CustomerId == request.CustomerId.Value)
                && (!request.BranchId.HasValue || q.BranchId == request.BranchId.Value));
        var quotationCounts = await CountByStatusAsync(quotationQuery, q => q.Status, cancellationToken);

        var deliveryQuery = context.BackOfficeSalesDeliveryNotes.AsNoTracking()
            .Where(d => d.TenantId == request.TenantId
                && d.DeliveryDate >= from && d.DeliveryDate <= to
                && (!request.CustomerId.HasValue || d.CustomerId == request.CustomerId.Value)
                && (!request.BranchId.HasValue || d.BranchId == request.BranchId.Value));
        var deliveryCounts = await CountByStatusAsync(deliveryQuery, d => d.Status, cancellationToken);

        var counts = new BackOfficeSalesReportDocumentCountsDto(
            invoiceCounts, returnCounts, debitCounts, orderCounts, quotationCounts, deliveryCounts);

        return Result<BackOfficeSalesReportDto>.Success(new BackOfficeSalesReportDto(
            DateTimeOffset.UtcNow, from, to, summary, salesByCustomer, salesByItem, salesByDay, counts));
    }

    private static async Task<IReadOnlyDictionary<BackOfficeSalesDocumentStatus, int>> CountByStatusAsync<T>(
        IQueryable<T> query,
        System.Linq.Expressions.Expression<Func<T, BackOfficeSalesDocumentStatus>> selector,
        CancellationToken ct)
    {
        var raw = await query
            .GroupBy(selector)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync(ct);
        return raw.ToDictionary(x => x.Key, x => x.Count);
    }
}
