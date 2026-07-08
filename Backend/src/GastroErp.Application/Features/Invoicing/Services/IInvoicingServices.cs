using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Invoicing.DTOs;
using GastroErp.Domain.Entities.Invoicing;
using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Invoicing.Services;

public record LineTaxResult(decimal TaxAmount, decimal NetAmount, decimal TotalAmount);

public interface IInvoiceNumberGenerator
{
    Task<string> GenerateAsync(Guid tenantId, Guid branchId, InvoiceType type, CancellationToken ct = default);
}

public interface ITaxCalculationService
{
    Task<LineTaxResult> CalculateLineTaxAsync(Guid? taxGroupId, decimal quantity, decimal unitPrice, decimal discount, CancellationToken ct = default);
    Task<IReadOnlyList<(TaxRate Rate, decimal TaxAmount)>> CalculateTaxesForAmountAsync(Guid taxGroupId, decimal taxableAmount, CancellationToken ct = default);
}

public interface IInvoiceGenerationService
{
    Task<Result<Invoice>> GenerateFromOrderAsync(Guid orderId, InvoiceType invoiceType, CancellationToken ct = default);
    Task<Result<Invoice>> EnsureInvoiceForCompletedOrderAsync(Guid orderId, CancellationToken ct = default);
}

public interface IFiscalValidationService
{
    Result ValidateForFinalization(Invoice invoice, string? companyTaxNumber);
    Result ValidateCreditNote(CreditNote creditNote, Invoice invoice);
    Result ValidateDebitNote(DebitNote debitNote, Invoice invoice);
}

public interface IReceiptPrintingService
{
    Task<Result<PrintInvoiceResultDto>> PrintInvoiceAsync(Guid invoiceId, string? printerName, CancellationToken ct = default);
}
