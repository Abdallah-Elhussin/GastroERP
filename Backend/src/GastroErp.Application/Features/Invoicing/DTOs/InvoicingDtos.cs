using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Invoicing.DTOs;

// ─── Invoice DTOs ─────────────────────────────────────────────────────────────

public record CreateInvoiceDto(
    Guid BranchId,
    InvoiceType InvoiceType,
    Guid? SalesOrderId = null,
    Guid? CustomerId = null,
    string? CustomerName = null,
    string? Notes = null
);

public record CancelInvoiceDto(string Reason, Guid? AuthorizedBy = null);

public record PrintInvoiceDto(string? PrinterName = null);

public record InvoiceDto(
    Guid Id, Guid BranchId, string InvoiceNumber, InvoiceType InvoiceType,
    InvoiceStatus Status, Guid? SalesOrderId, Guid? CustomerId, string? CustomerName,
    decimal SubTotal, decimal DiscountTotal, decimal TaxTotal, decimal GrandTotal,
    decimal PaidAmount, decimal CreditedAmount, decimal RemainingBalance,
    InvoicePaymentStatus PaymentStatus, string Currency,
    DateTimeOffset IssuedAt, DateTimeOffset? FinalizedAt, int PrintCount
);

public record InvoiceDetailDto(
    Guid Id, Guid BranchId, string InvoiceNumber, InvoiceType InvoiceType,
    InvoiceStatus Status, Guid? SalesOrderId, Guid? CustomerId, string? CustomerName,
    decimal SubTotal, decimal DiscountTotal, decimal TaxTotal, decimal GrandTotal,
    decimal PaidAmount, decimal CreditedAmount, decimal RemainingBalance,
    InvoicePaymentStatus PaymentStatus, string Currency,
    DateTimeOffset IssuedAt, DateTimeOffset? FinalizedAt, DateTimeOffset? CancelledAt,
    string? CancellationReason, int PrintCount, DateTimeOffset? LastPrintedAt,
    IReadOnlyList<InvoiceLineDto> Lines, IReadOnlyList<InvoiceTaxLineDto> TaxLines
);

public record InvoiceLineDto(
    Guid Id, int LineNumber, Guid ProductId, string ProductNameAr, string? ProductNameEn,
    string? Sku, decimal Quantity, decimal UnitPrice, decimal DiscountAmount,
    decimal TaxAmount, decimal NetAmount, decimal TotalAmount
);

public record InvoiceTaxLineDto(
    string TaxNameAr, string? TaxNameEn, decimal TaxRate,
    decimal TaxableAmount, decimal TaxAmount, bool IsInclusive
);

public record InvoiceFilterDto(
    Guid? BranchId = null, InvoiceStatus? Status = null, InvoiceType? InvoiceType = null,
    Guid? SalesOrderId = null, DateTimeOffset? FromDate = null, DateTimeOffset? ToDate = null,
    int Page = 1, int PageSize = 20
);

// ─── Tax DTOs ─────────────────────────────────────────────────────────────────

public record CreateTaxRateDto(
    string Code, string NameAr, TaxType TaxType, TaxCalculationMethod CalculationMethod,
    decimal Rate, bool IsInclusive = false, string? NameEn = null,
    decimal? FixedAmount = null, string? Description = null
);

public record UpdateTaxRateDto(
    string NameAr, TaxType TaxType, TaxCalculationMethod CalculationMethod,
    decimal Rate, bool IsInclusive, string? NameEn = null,
    decimal? FixedAmount = null, string? Description = null
);

public record TaxRateDto(
    Guid Id, string Code, string NameAr, string? NameEn, TaxType TaxType,
    TaxCalculationMethod CalculationMethod, decimal Rate, decimal? FixedAmount,
    bool IsInclusive, bool IsActive, string? Description
);

public record CreateTaxGroupDto(string NameAr, string? NameEn = null, string? Description = null);

public record UpdateTaxGroupDto(string NameAr, string? NameEn, string? Description);

public record AddTaxGroupRateDto(Guid TaxRateId, int SortOrder = 0);

public record TaxGroupDto(
    Guid Id, string NameAr, string? NameEn, bool IsActive, string? Description,
    IReadOnlyList<TaxGroupRateDto> Rates
);

public record TaxGroupRateDto(Guid Id, Guid TaxRateId, int SortOrder, TaxRateDto? TaxRate);

// ─── Credit Note DTOs ─────────────────────────────────────────────────────────

public record CreateCreditNoteDto(
    Guid BranchId, Guid OriginalInvoiceId, CreditNoteType CreditType,
    string Reason, IReadOnlyList<CreateCreditNoteLineDto> Lines
);

public record CreateCreditNoteLineDto(
    Guid? InvoiceLineId, Guid ProductId, string ProductNameAr, string? ProductNameEn,
    decimal Quantity, decimal UnitPrice, decimal TaxAmount
);

public record CancelCreditNoteDto(string Reason);

public record CreditNoteDto(
    Guid Id, string CreditNoteNumber, Guid OriginalInvoiceId, CreditNoteType CreditType,
    CreditNoteStatus Status, decimal SubTotal, decimal TaxTotal, decimal TotalAmount,
    string Currency, string Reason, DateTimeOffset? IssuedAt,
    IReadOnlyList<CreditNoteLineDto> Lines
);

public record CreditNoteLineDto(
    Guid Id, int LineNumber, Guid ProductId, string ProductNameAr, string? ProductNameEn,
    decimal Quantity, decimal UnitPrice, decimal TaxAmount, decimal TotalAmount
);

// ─── Debit Note DTOs ──────────────────────────────────────────────────────────

public record CreateDebitNoteDto(
    Guid BranchId, Guid OriginalInvoiceId, string Reason,
    IReadOnlyList<CreateDebitNoteLineDto> Lines
);

public record CreateDebitNoteLineDto(
    string DescriptionAr, string? DescriptionEn,
    decimal Quantity, decimal UnitPrice, decimal TaxAmount
);

public record CancelDebitNoteDto(string Reason);

public record DebitNoteDto(
    Guid Id, string DebitNoteNumber, Guid OriginalInvoiceId, DebitNoteStatus Status,
    decimal SubTotal, decimal TaxTotal, decimal TotalAmount,
    string Currency, string Reason, DateTimeOffset? IssuedAt,
    IReadOnlyList<DebitNoteLineDto> Lines
);

public record DebitNoteLineDto(
    Guid Id, int LineNumber, string DescriptionAr, string? DescriptionEn,
    decimal Quantity, decimal UnitPrice, decimal TaxAmount, decimal TotalAmount
);

// ─── Reporting Read Models ────────────────────────────────────────────────────

public record DailySalesReportDto(DateOnly Date, int InvoiceCount, decimal SubTotal, decimal TaxTotal, decimal GrandTotal, string Currency);

public record VatSummaryDto(string TaxName, decimal TaxableAmount, decimal TaxAmount, string Currency);

public record InvoiceRegisterDto(
    Guid Id, string InvoiceNumber, InvoiceType InvoiceType, InvoiceStatus Status,
    DateTimeOffset IssuedAt, decimal GrandTotal, decimal TaxTotal, string Currency
);

public record OutstandingInvoiceDto(
    Guid Id, string InvoiceNumber, Guid? SalesOrderId, decimal GrandTotal,
    decimal PaidAmount, decimal RemainingBalance, DateTimeOffset IssuedAt
);

public record TaxReportDto(
    DateTimeOffset FromDate, DateTimeOffset ToDate,
    IReadOnlyList<VatSummaryDto> VatBreakdown, decimal TotalTax, string Currency
);

public record PrintInvoiceResultDto(byte[] QrCodeImage, int PrintCount, string InvoiceNumber);
