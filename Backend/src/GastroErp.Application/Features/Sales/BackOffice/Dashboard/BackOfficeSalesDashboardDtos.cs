using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Sales.BackOffice.Dashboard;

public record BackOfficeSalesDashboardFilterDto(
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    Guid? BranchId = null,
    Guid? CustomerId = null,
    BackOfficeSalesDocumentStatus? Status = null);

public record BackOfficeSalesDashboardDto(
    DateTimeOffset GeneratedAtUtc,
    BackOfficeSalesDashboardKpisDto Kpis,
    IReadOnlyList<BackOfficeSalesDashboardNamedValueDto> SalesByDay,
    IReadOnlyList<BackOfficeSalesDashboardNamedValueDto> SalesByCustomer,
    IReadOnlyList<BackOfficeSalesDashboardNamedValueDto> SalesByNature,
    IReadOnlyList<BackOfficeSalesDashboardNamedValueDto> SalesByPaymentMode,
    IReadOnlyList<BackOfficeSalesDashboardRecentInvoiceDto> RecentInvoices,
    IReadOnlyList<BackOfficeSalesDashboardAlertDto> Alerts);

public record BackOfficeSalesDashboardKpisDto(
    decimal SalesToday,
    decimal SalesWeek,
    decimal SalesMonth,
    decimal SalesPeriod,
    decimal SalesTodayChangePercent,
    int InvoiceCount,
    int DraftCount,
    int ApprovedCount,
    int PostedCount,
    decimal AverageInvoiceValue,
    decimal CreditOutstanding,
    decimal CashSalesPeriod,
    decimal CreditSalesPeriod,
    int ActiveCustomers);

public record BackOfficeSalesDashboardNamedValueDto(string Label, decimal Value, int Count = 0);

public record BackOfficeSalesDashboardRecentInvoiceDto(
    Guid Id,
    string InvoiceNumber,
    string? CustomerName,
    decimal TotalAmount,
    BackOfficeSalesDocumentStatus Status,
    BackOfficeSalesPaymentMode PaymentMode,
    DateOnly InvoiceDate,
    DateTimeOffset? PostedAt);

public record BackOfficeSalesDashboardAlertDto(
    string Code,
    string Severity,
    string MessageEn,
    string MessageAr,
    string? Path = null);
