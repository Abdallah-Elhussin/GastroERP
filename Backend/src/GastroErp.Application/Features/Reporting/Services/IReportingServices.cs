using GastroErp.Application.Features.Reporting.DTOs;

namespace GastroErp.Application.Features.Reporting.Services;

public interface IDashboardService
{
    Task<ExecutiveDashboardDto> GetExecutiveDashboardAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
}

public interface ISalesAnalyticsService
{
    Task<IReadOnlyList<PeriodSalesDto>> GetDailySalesAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<PeriodSalesDto>> GetMonthlySalesAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<PeriodSalesDto>> GetYearlySalesAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<BranchSalesDto>> GetSalesByBranchAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<CashierSalesDto>> GetSalesByCashierAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<ProductSalesDto>> GetSalesByProductAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<CategorySalesDto>> GetSalesByCategoryAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<HourlySalesDto>> GetSalesByHourAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<OrderTypeSalesDto>> GetSalesByOrderTypeAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<PaymentMethodSalesDto>> GetSalesByPaymentMethodAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<CancelledOrderDto>> GetCancelledOrdersAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<DiscountReportDto>> GetDiscountReportAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<VatSalesReportDto> GetVatReportAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
}

public interface IKitchenAnalyticsService
{
    Task<KitchenPerformanceDto> GetKitchenPerformanceAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<DelayedOrderDto>> GetDelayedOrdersAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<KitchenStationLoadDto>> GetStationLoadAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<TopDelayedProductDto>> GetTopDelayedProductsAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
}

public interface IDeliveryAnalyticsService
{
    Task<DeliverySummaryDto> GetDeliverySummaryAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<DriverPerformanceDto>> GetDriverPerformanceAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<DeliveryZoneReportDto>> GetDeliveryByZoneAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<FailedDeliveryDto>> GetFailedDeliveriesAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
}

public interface IInventoryAnalyticsService
{
    Task<IReadOnlyList<StockBalanceDto>> GetStockBalanceAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<StockValuationDto> GetStockValuationAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryMovementDto>> GetInventoryMovementAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<WasteAnalysisDto>> GetWasteAnalysisAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<AdjustmentAnalysisDto>> GetAdjustmentAnalysisAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<ConsumptionReportDto>> GetConsumptionReportAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<RecipeCostDto>> GetRecipeCostReportAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<PurchaseAnalysisDto>> GetPurchaseAnalysisAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<SupplierPerformanceDto>> GetSupplierPerformanceAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
}

public interface ICustomerAnalyticsService
{
    Task<IReadOnlyList<CustomerActivityDto>> GetCustomerActivityAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerLtvDto>> GetCustomerLtvAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerFrequencyDto>> GetCustomerFrequencyAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<LoyaltyPointsReportDto> GetLoyaltyPointsReportAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<CouponUsageDto>> GetCouponUsageAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<GiftCardUsageDto> GetGiftCardUsageAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<MembershipDistributionDto>> GetMembershipDistributionAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
}

public interface IFinancialAnalyticsService
{
    Task<BalanceSheetDto> GetBalanceSheetAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IncomeStatementDto> GetIncomeStatementAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<CashFlowDto> GetCashFlowAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<VatSummaryDto> GetVatSummaryAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<RevenueAnalysisDto>> GetRevenueAnalysisAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<ExpenseAnalysisDto>> GetExpenseAnalysisAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
}

public interface IKpiEngineService
{
    Task<KpiDashboardDto> GetKpiDashboardAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default);
}

public interface IReportExportService
{
    Task<ExportResultDto> ExportAsync(Guid tenantId, ExportReportRequestDto request, CancellationToken ct = default);
}
