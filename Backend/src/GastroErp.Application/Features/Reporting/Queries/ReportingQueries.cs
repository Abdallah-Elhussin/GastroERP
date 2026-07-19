using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Application.Features.Reporting.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Reporting.Queries;

// Dashboard
public record GetExecutiveDashboardQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<ExecutiveDashboardDto>>;

// Sales
public record GetDailySalesReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<PeriodSalesDto>>>;
public record GetMonthlySalesReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<PeriodSalesDto>>>;
public record GetYearlySalesReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<PeriodSalesDto>>>;
public record GetSalesByBranchReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<BranchSalesDto>>>;
public record GetSalesByCashierReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<CashierSalesDto>>>;
public record GetSalesByProductReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<ProductSalesDto>>>;
public record GetSalesByCategoryReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<CategorySalesDto>>>;
public record GetSalesByHourReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<HourlySalesDto>>>;
public record GetSalesByOrderTypeReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<OrderTypeSalesDto>>>;
public record GetSalesByPaymentMethodReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<PaymentMethodSalesDto>>>;
public record GetCancelledOrdersReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<CancelledOrderDto>>>;
public record GetDiscountReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<DiscountReportDto>>>;
public record GetVatSalesReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<VatSalesReportDto>>;

// Kitchen
public record GetKitchenPerformanceReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<KitchenPerformanceDto>>;
public record GetDelayedOrdersReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<DelayedOrderDto>>>;
public record GetKitchenStationLoadReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<KitchenStationLoadDto>>>;
public record GetTopDelayedProductsReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<TopDelayedProductDto>>>;

// Delivery
public record GetDeliverySummaryReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<DeliverySummaryDto>>;
public record GetDriverPerformanceReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<DriverPerformanceDto>>>;
public record GetDeliveryByZoneReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<DeliveryZoneReportDto>>>;
public record GetFailedDeliveriesReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<FailedDeliveryDto>>>;

// Inventory
public record GetStockBalanceReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<StockBalanceDto>>>;
public record GetStockValuationReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<StockValuationDto>>;
public record GetInventoryMovementReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<InventoryMovementDto>>>;
public record GetWasteAnalysisReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<WasteAnalysisDto>>>;
public record GetAdjustmentAnalysisReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<AdjustmentAnalysisDto>>>;
public record GetConsumptionReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<ConsumptionReportDto>>>;
public record GetRecipeCostReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<RecipeCostDto>>>;
public record GetPurchaseAnalysisReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<PurchaseAnalysisDto>>>;
public record GetSupplierPerformanceReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<SupplierPerformanceDto>>>;

// Customer
public record GetCustomerActivityReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<CustomerActivityDto>>>;
public record GetCustomerLtvReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<CustomerLtvDto>>>;
public record GetCustomerFrequencyReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<CustomerFrequencyDto>>>;
public record GetLoyaltyPointsReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<LoyaltyPointsReportDto>>;
public record GetCouponUsageReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<CouponUsageDto>>>;
public record GetGiftCardUsageReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<GiftCardUsageDto>>;
public record GetMembershipDistributionReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<MembershipDistributionDto>>>;

// Financial
public record GetBalanceSheetReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<BalanceSheetDto>>;
public record GetIncomeStatementReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IncomeStatementDto>>;
public record GetCashFlowReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<CashFlowDto>>;
public record GetVatSummaryReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<VatSummaryDto>>;
public record GetRevenueAnalysisReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<RevenueAnalysisDto>>>;
public record GetExpenseAnalysisReportQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<IReadOnlyList<ExpenseAnalysisDto>>>;
public record GetTrialBalanceReportQuery(Guid TenantId, TrialBalanceFilterDto Filter) : IRequest<Result<IReadOnlyList<TrialBalanceLineDto>>>;
public record GetGeneralLedgerReportQuery(Guid TenantId, GeneralLedgerFilterDto Filter) : IRequest<Result<GeneralLedgerResultDto>>;
public record GetJournalRegisterReportQuery(Guid TenantId, JournalRegisterFilterDto Filter) : IRequest<PagedResult<JournalDto>>;

// Analytics & KPI
public record GetKpiDashboardQuery(Guid TenantId, ReportFilterDto Filter) : IRequest<Result<KpiDashboardDto>>;

// Export
public record ExportReportQuery(Guid TenantId, ExportReportRequestDto Request) : IRequest<Result<ExportResultDto>>;
