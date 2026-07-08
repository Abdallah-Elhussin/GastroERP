using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Invoicing.DTOs;
using GastroErp.Domain.Entities.Invoicing;
using GastroErp.Domain.Entities.Sales;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Invoicing.Services;

public sealed class InvoiceNumberGenerator : IInvoiceNumberGenerator
{
    private readonly IApplicationDbContext _context;

    public InvoiceNumberGenerator(IApplicationDbContext context) => _context = context;

    public async Task<string> GenerateAsync(Guid tenantId, Guid branchId, InvoiceType type, CancellationToken ct = default)
    {
        var prefix = type switch
        {
            InvoiceType.Tax => "TAX",
            InvoiceType.Simplified => "SIM",
            InvoiceType.Credit => "CRN",
            InvoiceType.Debit => "DBN",
            _ => "INV"
        };
        var year = DateTime.UtcNow.Year;
        var count = await _context.Invoices
            .CountAsync(i => i.TenantId == tenantId && i.BranchId == branchId && i.CreatedAt.Year == year, ct);
        return $"{prefix}-{year}-{(count + 1):D6}";
    }
}

public sealed class TaxCalculationService : ITaxCalculationService
{
    private readonly IApplicationDbContext _context;

    public TaxCalculationService(IApplicationDbContext context) => _context = context;

    public async Task<LineTaxResult> CalculateLineTaxAsync(
        Guid? taxGroupId, decimal quantity, decimal unitPrice, decimal discount, CancellationToken ct = default)
    {
        var netBeforeTax = Math.Round((quantity * unitPrice) - discount, 4);
        if (!taxGroupId.HasValue || netBeforeTax <= 0)
            return new LineTaxResult(0, netBeforeTax, netBeforeTax);

        var taxes = await CalculateTaxesForAmountAsync(taxGroupId.Value, netBeforeTax, ct);
        var taxAmount = taxes.Sum(t => t.TaxAmount);
        return new LineTaxResult(taxAmount, netBeforeTax, netBeforeTax + taxAmount);
    }

    public async Task<IReadOnlyList<(TaxRate Rate, decimal TaxAmount)>> CalculateTaxesForAmountAsync(
        Guid taxGroupId, decimal taxableAmount, CancellationToken ct = default)
    {
        var group = await _context.TaxGroups.AsNoTracking()
            .Include(g => g.Rates)
            .FirstOrDefaultAsync(g => g.Id == taxGroupId && g.IsActive, ct);

        if (group is null) return [];

        var rateIds = group.Rates.Select(r => r.TaxRateId).ToList();
        var rates = await _context.TaxRates.AsNoTracking()
            .Where(r => rateIds.Contains(r.Id) && r.IsActive)
            .ToListAsync(ct);

        return rates.Select(r => (r, r.CalculateTax(taxableAmount))).ToList();
    }
}

public sealed class FiscalValidationService : IFiscalValidationService
{
    public Result ValidateForFinalization(Invoice invoice, string? companyTaxNumber)
    {
        if (invoice.Lines.Count == 0)
            return Result.Failure("InvoiceHasNoLines", "Invoice must have at least one line.");
        if (invoice.GrandTotal < 0)
            return Result.Failure("FiscalValidationFailed", "Invoice total cannot be negative.");
        if (invoice.TaxTotal < 0)
            return Result.Failure("FiscalValidationFailed", "Tax amount cannot be negative.");
        if (invoice.InvoiceType == InvoiceType.Tax && string.IsNullOrWhiteSpace(companyTaxNumber))
            return Result.Failure("FiscalValidationFailed", "Tax invoice requires company tax number.");

        var lineTotal = invoice.Lines.Sum(l => l.TotalAmount);
        if (Math.Abs(lineTotal - invoice.GrandTotal) > 0.01m)
            return Result.Failure("FiscalValidationFailed", "Invoice totals do not match line totals.");

        return Result.Success();
    }

    public Result ValidateCreditNote(CreditNote creditNote, Invoice invoice)
    {
        if (invoice.Status != InvoiceStatus.Finalized)
            return Result.Failure("InvoiceNotFinalized", "Credit note requires a finalized invoice.");
        if (creditNote.TotalAmount + invoice.CreditedAmount > invoice.GrandTotal)
            return Result.Failure("CreditNoteExceedsInvoice", "Credit note exceeds invoice amount.");
        return Result.Success();
    }

    public Result ValidateDebitNote(DebitNote debitNote, Invoice invoice)
    {
        if (invoice.Status != InvoiceStatus.Finalized)
            return Result.Failure("InvoiceNotFinalized", "Debit note requires a finalized invoice.");
        if (debitNote.OriginalInvoiceId != invoice.Id)
            return Result.Failure("InvoiceReferenceRequired", "Debit note must reference the original invoice.");
        return Result.Success();
    }
}

public sealed class InvoiceGenerationService : IInvoiceGenerationService
{
    private readonly IApplicationDbContext _context;
    private readonly IInvoiceNumberGenerator _numberGenerator;

    public InvoiceGenerationService(IApplicationDbContext context, IInvoiceNumberGenerator numberGenerator)
        => (_context, _numberGenerator) = (context, numberGenerator);

    public async Task<Result<Invoice>> GenerateFromOrderAsync(Guid orderId, InvoiceType invoiceType, CancellationToken ct = default)
    {
        var order = await _context.SalesOrders.AsNoTracking()
            .Include(o => o.Items).ThenInclude(i => i.Modifiers)
            .Include(o => o.Taxes)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);

        if (order is null) return Result<Invoice>.Failure("NotFound", "Order not found.");

        var existing = await _context.Invoices
            .AnyAsync(i => i.SalesOrderId == orderId && i.Status == InvoiceStatus.Finalized, ct);
        if (existing)
            return Result<Invoice>.Failure("InvoiceAlreadyExistsForOrder", "A finalized invoice already exists for this order.");

        var invoiceNumber = await _numberGenerator.GenerateAsync(order.TenantId, order.BranchId, invoiceType, ct);
        var invoice = Invoice.CreateDraft(
            order.TenantId, order.CompanyId, order.BranchId, invoiceNumber,
            invoiceType, order.Currency, order.Id, order.CustomerId);

        var lineNum = 1;
        foreach (var item in order.Items.Where(i => !i.IsVoided))
        {
            invoice.AddLine(
                item.Id, lineNum++, item.ProductId, item.ProductNameAr, item.ProductNameEn, item.Sku,
                item.Quantity, item.UnitPrice, item.LineDiscount, item.LineTax);
        }

        foreach (var tax in order.Taxes)
        {
            invoice.AddTaxLine(tax.TaxNameAr, tax.TaxNameEn, tax.TaxRate, tax.TaxableAmount, tax.TaxAmount, tax.IsInclusive);
        }

        return Result<Invoice>.Success(invoice);
    }

    public async Task<Result<Invoice>> EnsureInvoiceForCompletedOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        var existing = await _context.Invoices
            .Include(i => i.Lines).Include(i => i.TaxLines)
            .FirstOrDefaultAsync(i => i.SalesOrderId == orderId && i.Status != InvoiceStatus.Cancelled, ct);

        if (existing is not null) return Result<Invoice>.Success(existing);

        var result = await GenerateFromOrderAsync(orderId, InvoiceType.Sales, ct);
        if (!result.IsSuccess) return result;

        var order = await _context.SalesOrders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);

        var invoice = result.Data!;
        invoice.Finalize(Guid.Empty);

        if (order?.PaidAmount > 0)
            invoice.RecordPayment(order.PaidAmount);

        _context.Invoices.Add(invoice);
        return Result<Invoice>.Success(invoice);
    }
}

public sealed class ReceiptPrintingService : IReceiptPrintingService
{
    private readonly IApplicationDbContext _context;
    private readonly IInvoiceQrCodeGenerator _qrGenerator;
    private readonly IInvoicePrinter _printer;
    private readonly ILogger<ReceiptPrintingService> _logger;

    public ReceiptPrintingService(
        IApplicationDbContext context, IInvoiceQrCodeGenerator qrGenerator,
        IInvoicePrinter printer, ILogger<ReceiptPrintingService> logger)
        => (_context, _qrGenerator, _printer, _logger) = (context, qrGenerator, printer, logger);

    public async Task<Result<PrintInvoiceResultDto>> PrintInvoiceAsync(
        Guid invoiceId, string? printerName, CancellationToken ct = default)
    {
        var invoice = await _context.Invoices
            .Include(i => i.TaxLines)
            .FirstOrDefaultAsync(i => i.Id == invoiceId, ct);

        if (invoice is null) return Result<PrintInvoiceResultDto>.Failure("NotFound", "Invoice not found.");
        if (invoice.Status != InvoiceStatus.Finalized)
            return Result<PrintInvoiceResultDto>.Failure("InvoiceNotFinalized", "Only finalized invoices can be printed.");

        var company = await _context.Companies.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == invoice.CompanyId, ct);

        var sellerName = company?.NameAr ?? "GastroERP";
        var taxNumber = company?.TaxNumber ?? "";

        var qrImage = _qrGenerator.Generate(
            sellerName, taxNumber, invoice.FinalizedAt?.UtcDateTime ?? DateTime.UtcNow,
            invoice.GrandTotal, invoice.TaxTotal);

        invoice.RecordPrint();
        _context.Invoices.Update(invoice);

        if (!string.IsNullOrWhiteSpace(printerName))
            await _printer.PrintAsync(qrImage, printerName, ct);

        _logger.LogInformation("Invoice {InvoiceNumber} printed (count: {PrintCount})", invoice.InvoiceNumber, invoice.PrintCount);

        return Result<PrintInvoiceResultDto>.Success(
            new PrintInvoiceResultDto(qrImage, invoice.PrintCount, invoice.InvoiceNumber));
    }
}
