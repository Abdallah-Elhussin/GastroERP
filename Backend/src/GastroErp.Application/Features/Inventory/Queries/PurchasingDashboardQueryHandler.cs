using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Inventory.Queries;

public class GetPurchasingDashboardQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPurchasingDashboardQuery, Result<PurchasingDashboardDto>>
{
    private static readonly PurchaseOrderStatus[] OpenPoStatuses =
    [
        PurchaseOrderStatus.Draft,
        PurchaseOrderStatus.PendingApproval,
        PurchaseOrderStatus.Approved,
        PurchaseOrderStatus.SentToSupplier,
        PurchaseOrderStatus.PartiallyReceived
    ];

    public async Task<Result<PurchasingDashboardDto>> Handle(
        GetPurchasingDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = request.TenantId;
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.UtcDateTime);

        var orders = await context.PurchaseOrders.AsNoTracking()
            .Where(p => p.TenantId == tenantId)
            .Select(p => new OrderSnap(
                p.Id,
                p.PoNumber,
                p.OrderDate,
                p.ExpectedDeliveryDate,
                p.Status,
                p.Notes,
                p.CreatedAt))
            .ToListAsync(cancellationToken);

        var openPos = orders.Count(p => OpenPoStatuses.Contains(p.Status));
        var latePos = orders.Count(p =>
            OpenPoStatuses.Contains(p.Status)
            && p.ExpectedDeliveryDate.HasValue
            && p.ExpectedDeliveryDate.Value < now);
        var totalPos = orders.Count;

        var receipts = await context.GoodsReceipts.AsNoTracking()
            .Where(g => g.TenantId == tenantId)
            .Select(g => new ReceiptSnap(
                g.Id,
                g.ReceiptNumber,
                g.ReceiptDate,
                g.Status,
                g.Notes,
                g.CreatedAt,
                g.Lines.Any(l => l.AcceptedQuantity > l.InvoicedQuantity)))
            .ToListAsync(cancellationToken);

        var pendingReceipts = receipts.Count(g =>
            g.Status is GoodsReceiptStatus.Draft or GoodsReceiptStatus.Approved);
        var uninvoicedReceipts = receipts.Count(g =>
            g.Status == GoodsReceiptStatus.Posted && g.HasRemainingToInvoice);
        var totalReceipts = receipts.Count(g =>
            g.Status is not (GoodsReceiptStatus.Cancelled or GoodsReceiptStatus.Reversed));

        var invoices = await context.PurchaseInvoices.AsNoTracking()
            .Where(i => i.TenantId == tenantId)
            .Select(i => new InvoiceSnap(
                i.Id,
                i.InvoiceNumber,
                i.InvoiceDate,
                i.DueDate,
                i.Status,
                i.Kind,
                i.PaymentStatus,
                i.Notes,
                i.CreatedAt,
                i.TotalAmount - i.PaidAmount))
            .ToListAsync(cancellationToken);

        var unpaidInvoices = invoices.Count(i =>
            i.Status == PurchasingDocumentStatus.Posted
            && i.PaymentStatus is PurchaseInvoicePaymentStatus.Unpaid or PurchaseInvoicePaymentStatus.PartiallyPaid
            && i.Remaining > 0);
        var draftInvoices = invoices.Count(i => i.Status == PurchasingDocumentStatus.Draft);
        var totalInvoices = invoices.Count(i =>
            i.Status is not (PurchasingDocumentStatus.Cancelled or PurchasingDocumentStatus.Reversed));
        var overdueInvoices = invoices.Count(i =>
            i.Status == PurchasingDocumentStatus.Posted
            && i.DueDate.HasValue
            && i.DueDate.Value < today
            && i.Remaining > 0);

        var suppliers = await context.Suppliers.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .Select(s => new { s.Id, s.IsActive, s.CreditLimit, s.OpeningBalance })
            .ToListAsync(cancellationToken);

        var supplierIds = suppliers.Select(s => s.Id).ToList();
        var outstandingBySupplier = await context.PurchaseInvoices.AsNoTracking()
            .Where(i => supplierIds.Contains(i.SupplierId) && i.Status == PurchasingDocumentStatus.Posted)
            .GroupBy(i => i.SupplierId)
            .Select(g => new { SupplierId = g.Key, Outstanding = g.Sum(i => i.TotalAmount - i.PaidAmount) })
            .ToDictionaryAsync(x => x.SupplierId, x => x.Outstanding, cancellationToken);

        var activeSuppliers = suppliers.Count(s => s.IsActive);
        var overCreditSuppliers = suppliers.Count(s =>
        {
            outstandingBySupplier.TryGetValue(s.Id, out var outstanding);
            var balance = s.OpeningBalance + outstanding;
            return s.CreditLimit > 0 && balance > s.CreditLimit;
        });

        var alerts = BuildAlerts(latePos, uninvoicedReceipts, unpaidInvoices, overCreditSuppliers, overdueInvoices);
        var recent = BuildRecentActivities(orders, receipts, invoices);

        var dto = new PurchasingDashboardDto(
            OpenPurchaseOrders: openPos,
            LatePurchaseOrders: latePos,
            TotalPurchaseOrders: totalPos,
            UninvoicedReceipts: uninvoicedReceipts,
            PendingReceipts: pendingReceipts,
            TotalReceipts: totalReceipts,
            UnpaidInvoices: unpaidInvoices,
            DraftInvoices: draftInvoices,
            TotalInvoices: totalInvoices,
            TotalSuppliers: suppliers.Count,
            ActiveSuppliers: activeSuppliers,
            OverCreditSuppliers: overCreditSuppliers,
            Alerts: alerts,
            RecentActivities: recent);

        return Result<PurchasingDashboardDto>.Success(dto);
    }

    private static List<PurchasingDashboardAlertDto> BuildAlerts(
        int latePos,
        int uninvoicedReceipts,
        int unpaidInvoices,
        int overCreditSuppliers,
        int overdueInvoices)
    {
        var alerts = new List<PurchasingDashboardAlertDto>();

        if (latePos > 0)
        {
            alerts.Add(new PurchasingDashboardAlertDto(
                "LatePurchaseOrders",
                "warning",
                $"{latePos} purchase order(s) are past expected delivery.",
                $"يوجد {latePos} أمر شراء متأخر عن موعد التسليم المتوقع.",
                "/purchases/purchase-orders"));
        }

        if (uninvoicedReceipts > 0)
        {
            alerts.Add(new PurchasingDashboardAlertDto(
                "UninvoicedReceipts",
                "warning",
                $"{uninvoicedReceipts} posted receipt(s) still need invoicing.",
                $"يوجد {uninvoicedReceipts} سند استلام مرحّل بانتظار الفوترة.",
                "/purchases/goods-receipts"));
        }

        if (overdueInvoices > 0)
        {
            alerts.Add(new PurchasingDashboardAlertDto(
                "OverdueInvoices",
                "danger",
                $"{overdueInvoices} posted invoice(s) are past due date.",
                $"يوجد {overdueInvoices} فاتورة مشتريات تجاوزت تاريخ الاستحقاق.",
                "/purchases/purchase-invoices"));
        }
        else if (unpaidInvoices > 0)
        {
            alerts.Add(new PurchasingDashboardAlertDto(
                "UnpaidInvoices",
                "info",
                $"{unpaidInvoices} posted invoice(s) are not fully paid.",
                $"يوجد {unpaidInvoices} فاتورة مشتريات غير مسددة بالكامل.",
                "/purchases/purchase-invoices"));
        }

        if (overCreditSuppliers > 0)
        {
            alerts.Add(new PurchasingDashboardAlertDto(
                "OverCreditSuppliers",
                "danger",
                $"{overCreditSuppliers} supplier(s) exceeded credit limit.",
                $"يوجد {overCreditSuppliers} مورد تجاوز حد الائتمان.",
                "/purchases/suppliers"));
        }

        return alerts;
    }

    private static List<PurchasingDashboardActivityDto> BuildRecentActivities(
        IReadOnlyList<OrderSnap> orders,
        IReadOnlyList<ReceiptSnap> receipts,
        IReadOnlyList<InvoiceSnap> invoices)
    {
        var fromOrders = orders.Select(p => new PurchasingDashboardActivityDto(
            p.Id,
            "PurchaseOrder",
            p.PoNumber,
            p.OrderDate,
            p.Notes,
            $"/purchases/purchase-orders/{p.Id}"));

        var fromReceipts = receipts.Select(g => new PurchasingDashboardActivityDto(
            g.Id,
            "GoodsReceipt",
            g.ReceiptNumber,
            g.ReceiptDate,
            g.Notes,
            $"/purchases/goods-receipts/{g.Id}"));

        var fromInvoices = invoices.Select(i =>
        {
            var path = i.Kind == PurchaseInvoiceKind.Direct
                ? $"/purchases/direct-invoices/{i.Id}"
                : $"/purchases/purchase-invoices/{i.Id}";
            var occurred = i.InvoiceDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            return new PurchasingDashboardActivityDto(
                i.Id,
                i.Kind == PurchaseInvoiceKind.Direct ? "DirectInvoice" : "PurchaseInvoice",
                i.InvoiceNumber,
                new DateTimeOffset(occurred),
                i.Notes,
                path);
        });

        return fromOrders
            .Concat(fromReceipts)
            .Concat(fromInvoices)
            .OrderByDescending(a => a.OccurredAt)
            .Take(12)
            .ToList();
    }

    private sealed record OrderSnap(
        Guid Id,
        string PoNumber,
        DateTimeOffset OrderDate,
        DateTimeOffset? ExpectedDeliveryDate,
        PurchaseOrderStatus Status,
        string? Notes,
        DateTimeOffset CreatedAt);

    private sealed record ReceiptSnap(
        Guid Id,
        string ReceiptNumber,
        DateTimeOffset ReceiptDate,
        GoodsReceiptStatus Status,
        string? Notes,
        DateTimeOffset CreatedAt,
        bool HasRemainingToInvoice);

    private sealed record InvoiceSnap(
        Guid Id,
        string InvoiceNumber,
        DateOnly InvoiceDate,
        DateOnly? DueDate,
        PurchasingDocumentStatus Status,
        PurchaseInvoiceKind Kind,
        PurchaseInvoicePaymentStatus PaymentStatus,
        string? Notes,
        DateTimeOffset CreatedAt,
        decimal Remaining);
}
