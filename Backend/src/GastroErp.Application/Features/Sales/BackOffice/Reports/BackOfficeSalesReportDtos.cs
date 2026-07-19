using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Sales.BackOffice.Reports;

public record BackOfficeSalesReportNamedValueDto(string Label, decimal Value, int Count);

public record BackOfficeSalesReportSummaryDto(
    decimal GrossSales,
    decimal TaxAmount,
    decimal TotalReturns,
    decimal TotalDebitNotes,
    decimal NetSales,
    int PostedInvoiceCount,
    int PostedReturnCount,
    int PostedDebitNoteCount);

public record BackOfficeSalesReportDocumentCountsDto(
    IReadOnlyDictionary<BackOfficeSalesDocumentStatus, int> Invoices,
    IReadOnlyDictionary<BackOfficeSalesDocumentStatus, int> Returns,
    IReadOnlyDictionary<BackOfficeSalesDocumentStatus, int> DebitNotes,
    IReadOnlyDictionary<BackOfficeSalesDocumentStatus, int> Orders,
    IReadOnlyDictionary<BackOfficeSalesDocumentStatus, int> Quotations,
    IReadOnlyDictionary<BackOfficeSalesDocumentStatus, int> DeliveryNotes);

public record BackOfficeSalesReportDto(
    DateTimeOffset GeneratedAtUtc,
    DateOnly From,
    DateOnly To,
    BackOfficeSalesReportSummaryDto Summary,
    IReadOnlyList<BackOfficeSalesReportNamedValueDto> SalesByCustomer,
    IReadOnlyList<BackOfficeSalesReportNamedValueDto> SalesByItem,
    IReadOnlyList<BackOfficeSalesReportNamedValueDto> SalesByDay,
    BackOfficeSalesReportDocumentCountsDto DocumentCounts);
