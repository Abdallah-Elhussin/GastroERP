namespace GastroErp.Application.Features.Reporting.DTOs;

// ─── Common Filters ───────────────────────────────────────────────────────────

public record ReportFilterDto(
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    Guid? BranchId = null,
    Guid? CompanyId = null,
    int Page = 1,
    int PageSize = 50,
    string? SortBy = null,
    bool SortDescending = false);

public record PagedReportFilterDto(
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    Guid? BranchId = null,
    int Page = 1,
    int PageSize = 50);

// ─── Chart DTOs ───────────────────────────────────────────────────────────────

public record KpiCardDto(string Key, string Label, decimal Value, string? Unit = null, decimal? ChangePercent = null);

public record ChartSeriesDto(string Name, IReadOnlyList<decimal> Data);

public record LineChartDto(IReadOnlyList<string> Labels, IReadOnlyList<ChartSeriesDto> Series);

public record BarChartDto(IReadOnlyList<string> Labels, IReadOnlyList<ChartSeriesDto> Series);

public record PieChartDto(IReadOnlyList<string> Labels, IReadOnlyList<decimal> Values);

public record AreaChartDto(IReadOnlyList<string> Labels, IReadOnlyList<ChartSeriesDto> Series);

public record HeatMapCellDto(int X, int Y, decimal Value, string? Label = null);

public record HeatMapDto(IReadOnlyList<string> XLabels, IReadOnlyList<string> YLabels, IReadOnlyList<HeatMapCellDto> Cells);

// ─── Dashboard ────────────────────────────────────────────────────────────────

public record ExecutiveDashboardDto(
    IReadOnlyList<KpiCardDto> Kpis,
    LineChartDto RevenueTrend,
    PieChartDto PaymentMix,
    PieChartDto OrderTypeMix,
    BarChartDto TopProducts,
    BarChartDto TopCategories);

// ─── Sales Reports ────────────────────────────────────────────────────────────

public record PeriodSalesDto(string Period, int OrderCount, decimal Revenue, decimal AverageTicket, decimal DiscountTotal, decimal TaxTotal);

public record BranchSalesDto(Guid BranchId, string BranchName, int OrderCount, decimal Revenue, decimal AverageTicket);

public record CashierSalesDto(Guid CashierId, string CashierName, int OrderCount, decimal Revenue);

public record ProductSalesDto(Guid ProductId, string ProductName, decimal Quantity, decimal Revenue);

public record CategorySalesDto(Guid CategoryId, string CategoryName, decimal Quantity, decimal Revenue);

public record HourlySalesDto(int Hour, int OrderCount, decimal Revenue);

public record OrderTypeSalesDto(string OrderType, int OrderCount, decimal Revenue);

public record PaymentMethodSalesDto(string PaymentMethod, int PaymentCount, decimal Amount);

public record CancelledOrderDto(Guid OrderId, string OrderNumber, DateTimeOffset CancelledAt, string? Reason, decimal GrandTotal);

public record DiscountReportDto(Guid OrderId, string OrderNumber, decimal DiscountTotal, decimal GrandTotal, DateTimeOffset OrderDate);

public record VatSalesReportDto(decimal TaxableAmount, decimal VatCollected, int InvoiceCount);

// ─── Kitchen Reports ──────────────────────────────────────────────────────────

public record KitchenPerformanceDto(
    int TotalTickets, int CompletedTickets, double AveragePrepMinutes,
    int DelayedTickets, double OnTimePercent);

public record DelayedOrderDto(Guid TicketId, string TicketNumber, string? ProductName, double PrepMinutes, int EstimatedMinutes);

public record KitchenStationLoadDto(Guid StationId, string StationName, int ActiveTickets, int CompletedTickets, double AvgPrepMinutes);

public record TopDelayedProductDto(string ProductName, int DelayCount, double AvgDelayMinutes);

// ─── Delivery Reports ─────────────────────────────────────────────────────────

public record DeliverySummaryDto(int TotalOrders, int Delivered, int Failed, decimal Revenue, decimal TotalFees, double AvgDeliveryMinutes);

public record DriverPerformanceDto(Guid DriverId, string DriverName, int Deliveries, int Failures, double AvgDeliveryMinutes, decimal Revenue);

public record DeliveryZoneReportDto(Guid ZoneId, string ZoneName, int OrderCount, decimal Revenue, decimal TotalFees);

public record FailedDeliveryDto(Guid DeliveryId, string DeliveryNumber, string? Reason, DateTimeOffset FailedAt);

// ─── Inventory Reports ────────────────────────────────────────────────────────

public record StockBalanceDto(Guid ItemId, string ItemName, string? Sku, Guid WarehouseId, string WarehouseName, decimal Quantity, decimal UnitCost, decimal TotalValue);

public record StockValuationDto(decimal TotalValue, int ItemCount, IReadOnlyList<StockBalanceDto> TopItems);

public record InventoryMovementDto(DateTimeOffset Date, string TransactionType, string ItemName, decimal QuantityChange, decimal UnitCost, string WarehouseName);

public record WasteAnalysisDto(string Reason, int RecordCount, decimal TotalQuantity, decimal TotalCost);

public record AdjustmentAnalysisDto(string Reason, int Count, decimal TotalAdjustment);

public record ConsumptionReportDto(Guid ItemId, string ItemName, decimal ConsumedQuantity, decimal TotalCost);

public record RecipeCostDto(Guid ProductId, string ProductName, decimal RecipeCost, decimal MenuPrice, decimal MarginPercent);

public record PurchaseAnalysisDto(Guid SupplierId, string SupplierName, int OrderCount, decimal TotalAmount);

public record SupplierPerformanceDto(Guid SupplierId, string SupplierName, int PurchaseCount, decimal TotalSpend, decimal OnTimePercent);

// ─── Customer Reports ─────────────────────────────────────────────────────────

public record CustomerActivityDto(Guid CustomerId, string CustomerName, int OrderCount, decimal TotalSpending, DateTimeOffset? LastVisit);

public record CustomerLtvDto(Guid CustomerId, string CustomerName, decimal LifetimeValue, int OrderCount, decimal AverageTicket);

public record CustomerFrequencyDto(string FrequencyBand, int CustomerCount);

public record LoyaltyPointsReportDto(decimal PointsIssued, decimal PointsRedeemed, int ActiveAccounts);

public record CouponUsageDto(string CouponCode, int UsageCount, decimal DiscountTotal);

public record GiftCardUsageDto(int CardsIssued, int CardsRedeemed, decimal OutstandingBalance);

public record MembershipDistributionDto(string TierName, int MemberCount);

// ─── Financial Reports ──────────────────────────────────────────────────────────

public record BalanceSheetDto(decimal TotalAssets, decimal TotalLiabilities, decimal TotalEquity, IReadOnlyList<FinancialLineDto> Lines);

public record IncomeStatementDto(decimal Revenue, decimal CostOfGoodsSold, decimal GrossProfit, decimal Expenses, decimal NetProfit);

public record CashFlowDto(decimal OpeningBalance, decimal CashIn, decimal CashOut, decimal ClosingBalance);

public record VatSummaryDto(decimal OutputVat, decimal InputVat, decimal NetVat);

public record FinancialLineDto(string AccountNumber, string AccountName, string Category, decimal Amount);

public record RevenueAnalysisDto(string Source, decimal Amount, int TransactionCount);

public record ExpenseAnalysisDto(string Category, decimal Amount, int TransactionCount);

// ─── KPI Engine ───────────────────────────────────────────────────────────────

public record KpiSnapshotDto(
    string KpiName, decimal Value, string Unit, decimal? Target, string Status,
    DateOnly PeriodStart, DateOnly PeriodEnd);

public record KpiDashboardDto(IReadOnlyList<KpiSnapshotDto> Kpis);

// ─── Export ───────────────────────────────────────────────────────────────────

public enum ExportFormat { Csv = 1, Pdf = 2, Excel = 3 }

public record ExportReportRequestDto(string ReportKey, ExportFormat Format, ReportFilterDto Filter);

public record ExportResultDto(byte[] Content, string ContentType, string FileName);
