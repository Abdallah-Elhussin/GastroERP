using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Sales.Dashboard;

public record SalesDashboardFilterDto(
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    Guid? CompanyId = null,
    Guid? BranchId = null,
    Guid? CashierId = null,
    Guid? DeviceId = null,
    Guid? CustomerId = null,
    PaymentMethodType? PaymentMethod = null,
    OrderStatus? OrderStatus = null);

public record SalesDashboardDto(
    DateTimeOffset GeneratedAtUtc,
    SalesDashboardKpisDto Kpis,
    SalesDashboardChartsDto Charts,
    IReadOnlyList<SalesDashboardRecentOrderDto> RecentOrders,
    IReadOnlyList<SalesDashboardRecentReturnDto> RecentReturns,
    IReadOnlyList<SalesDashboardTopCustomerDto> TopCustomers,
    IReadOnlyList<SalesDashboardTopItemDto> TopItems,
    IReadOnlyList<SalesDashboardAlertDto> Alerts);

public record SalesDashboardKpisDto(
    // Sales periods
    decimal SalesToday,
    decimal SalesWeek,
    decimal SalesMonth,
    decimal SalesYear,
    decimal SalesTodayChangePercent,
    decimal SalesWeekChangePercent,
    decimal SalesMonthChangePercent,
    // Invoices / orders
    int InvoiceCount,
    decimal AverageInvoiceValue,
    decimal HighestInvoice,
    decimal LowestInvoice,
    // Customers
    int NewCustomers,
    int ActiveCustomers,
    int InactiveCustomers,
    int FirstTimeBuyers,
    // Returns / discounts / cancellations
    decimal ReturnsTotal,
    decimal ReturnsRatioPercent,
    decimal DiscountsTotal,
    decimal CancellationsTotal,
    // POS
    int PosInvoiceCount,
    double AverageInvoiceMinutes,
    decimal AverageItemsPerInvoice,
    string? MostActivePosDevice,
    // Profitability (gross approx. — COGS not stored on order lines)
    decimal GrossProfit,
    decimal ProfitMarginPercent,
    decimal Cogs,
    decimal NetProfit,
    // Professional KPIs
    decimal SalesGrowthPercent,
    decimal NewCustomerRatioPercent,
    decimal ReturningCustomerRatioPercent,
    decimal DiscountRatioPercent,
    string? BestBranchName,
    string? WorstBranchName,
    string? BestCashierName,
    string? TopSellingItemName,
    string? TopSellingCategoryName,
    string? TopPaymentMethodName);

public record SalesDashboardChartsDto(
    IReadOnlyList<SalesDashboardNamedValueDto> SalesByDay,
    IReadOnlyList<SalesDashboardNamedValueDto> SalesByBranch,
    IReadOnlyList<SalesDashboardNamedValueDto> SalesByPosDevice,
    IReadOnlyList<SalesDashboardNamedValueDto> SalesByCashier,
    IReadOnlyList<SalesDashboardNamedValueDto> TopCustomers,
    IReadOnlyList<SalesDashboardNamedValueDto> TopItems,
    IReadOnlyList<SalesDashboardNamedValueDto> SalesByCategory,
    IReadOnlyList<SalesDashboardNamedValueDto> PaymentMethods,
    IReadOnlyList<SalesDashboardHourlyCellDto> SalesByHour);

public record SalesDashboardNamedValueDto(string Label, decimal Value, int Count = 0);

public record SalesDashboardHourlyCellDto(int DayOfWeek, int Hour, decimal Value, int Count);

public record SalesDashboardRecentOrderDto(
    Guid Id,
    string OrderNumber,
    string? CustomerName,
    decimal GrandTotal,
    string? PaymentMethod,
    string? BranchName,
    string? CashierName,
    string Status,
    DateTimeOffset OccurredAt);

public record SalesDashboardRecentReturnDto(
    Guid Id,
    string CreditNoteNumber,
    string? OriginalInvoiceNumber,
    string? CustomerName,
    decimal TotalAmount,
    string Reason,
    DateTimeOffset? IssuedAt);

public record SalesDashboardTopCustomerDto(
    Guid? CustomerId,
    string CustomerName,
    decimal TotalSales,
    int InvoiceCount,
    DateTimeOffset? LastPurchaseAt);

public record SalesDashboardTopItemDto(
    Guid ProductId,
    string ProductName,
    decimal Quantity,
    decimal Revenue,
    decimal Profit);

public record SalesDashboardAlertDto(
    string Code,
    string Severity,
    string MessageEn,
    string MessageAr,
    string? Path);
