using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Entities.Inventory.Purchasing;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Inventory.Services;

/// <summary>
/// Applies / reverses purchase-invoice settlement from vouchers and related finance documents.
/// </summary>
public static class PurchaseInvoiceSettlement
{
    public static async Task ApplyAsync(
        IApplicationDbContext context,
        Guid tenantId,
        Guid purchaseInvoiceId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0) return;

        var inv = await context.PurchaseInvoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == purchaseInvoiceId && i.TenantId == tenantId, cancellationToken)
            ?? throw new BusinessException("PurchaseInvoiceNotFound", "Purchase invoice not found.");

        if (inv.Status != PurchasingDocumentStatus.Posted)
            throw new BusinessException("InvalidStatusTransition", "Only posted invoices can receive payments.");

        // Cash invoices settle at posting time; ignore duplicate voucher applications.
        if (inv.PaymentMode == PurchaseInvoicePaymentMode.Cash)
            return;

        var apply = Math.Min(amount, inv.RemainingAmount);
        if (apply <= 0) return;

        inv.ApplyPayment(apply);
        context.PurchaseInvoices.Update(inv);
    }

    public static async Task ReverseAsync(
        IApplicationDbContext context,
        Guid tenantId,
        Guid purchaseInvoiceId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0) return;

        var inv = await context.PurchaseInvoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == purchaseInvoiceId && i.TenantId == tenantId, cancellationToken)
            ?? throw new BusinessException("PurchaseInvoiceNotFound", "Purchase invoice not found.");

        if (inv.PaymentMode == PurchaseInvoicePaymentMode.Cash)
            return;

        var reverse = Math.Min(amount, inv.PaidAmount);
        if (reverse <= 0) return;

        inv.ReversePayment(reverse);
        context.PurchaseInvoices.Update(inv);
    }

    public static async Task ApplyByInvoiceNumberAsync(
        IApplicationDbContext context,
        Guid tenantId,
        Guid? supplierId,
        string? invoiceNumber,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0 || string.IsNullOrWhiteSpace(invoiceNumber)) return;

        var number = invoiceNumber.Trim().ToUpperInvariant();
        var query = context.PurchaseInvoices
            .Include(i => i.Lines)
            .Where(i => i.TenantId == tenantId
                        && i.Status == PurchasingDocumentStatus.Posted
                        && i.PaymentMode == PurchaseInvoicePaymentMode.Credit
                        && i.InvoiceNumber == number);

        if (supplierId.HasValue)
            query = query.Where(i => i.SupplierId == supplierId.Value);

        var inv = await query.FirstOrDefaultAsync(cancellationToken);
        if (inv is null) return;

        var apply = Math.Min(amount, inv.RemainingAmount);
        if (apply <= 0) return;
        inv.ApplyPayment(apply);
        context.PurchaseInvoices.Update(inv);
    }

    public static async Task ReverseByInvoiceNumberAsync(
        IApplicationDbContext context,
        Guid tenantId,
        Guid? supplierId,
        string? invoiceNumber,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0 || string.IsNullOrWhiteSpace(invoiceNumber)) return;

        var number = invoiceNumber.Trim().ToUpperInvariant();
        var query = context.PurchaseInvoices
            .Include(i => i.Lines)
            .Where(i => i.TenantId == tenantId
                        && i.InvoiceNumber == number
                        && i.PaymentMode == PurchaseInvoicePaymentMode.Credit);

        if (supplierId.HasValue)
            query = query.Where(i => i.SupplierId == supplierId.Value);

        var inv = await query.FirstOrDefaultAsync(cancellationToken);
        if (inv is null) return;

        var reverse = Math.Min(amount, inv.PaidAmount);
        if (reverse <= 0) return;
        inv.ReversePayment(reverse);
        context.PurchaseInvoices.Update(inv);
    }
}
