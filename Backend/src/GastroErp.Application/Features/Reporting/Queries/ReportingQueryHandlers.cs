using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Application.Features.Finance.Queries;
using GastroErp.Application.Features.Finance.Services;
using GastroErp.Application.Features.Reporting.DTOs;
using GastroErp.Application.Features.Reporting.Queries;
using GastroErp.Application.Features.Reporting.Services;
using MediatR;

namespace GastroErp.Application.Features.Reporting.Queries;

public class GetExecutiveDashboardQueryHandler : IRequestHandler<GetExecutiveDashboardQuery, Result<ExecutiveDashboardDto>>
{
    private readonly IDashboardService _service;
    public GetExecutiveDashboardQueryHandler(IDashboardService service) => _service = service;
    public async Task<Result<ExecutiveDashboardDto>> Handle(GetExecutiveDashboardQuery request, CancellationToken ct)
        => Result<ExecutiveDashboardDto>.Success(await _service.GetExecutiveDashboardAsync(request.TenantId, request.Filter, ct));
}

public class GetDailySalesReportQueryHandler : IRequestHandler<GetDailySalesReportQuery, Result<IReadOnlyList<PeriodSalesDto>>>
{
    private readonly ISalesAnalyticsService _service;
    public GetDailySalesReportQueryHandler(ISalesAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<PeriodSalesDto>>> Handle(GetDailySalesReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<PeriodSalesDto>>.Success(await _service.GetDailySalesAsync(request.TenantId, request.Filter, ct));
}

public class GetMonthlySalesReportQueryHandler : IRequestHandler<GetMonthlySalesReportQuery, Result<IReadOnlyList<PeriodSalesDto>>>
{
    private readonly ISalesAnalyticsService _service;
    public GetMonthlySalesReportQueryHandler(ISalesAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<PeriodSalesDto>>> Handle(GetMonthlySalesReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<PeriodSalesDto>>.Success(await _service.GetMonthlySalesAsync(request.TenantId, request.Filter, ct));
}

public class GetYearlySalesReportQueryHandler : IRequestHandler<GetYearlySalesReportQuery, Result<IReadOnlyList<PeriodSalesDto>>>
{
    private readonly ISalesAnalyticsService _service;
    public GetYearlySalesReportQueryHandler(ISalesAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<PeriodSalesDto>>> Handle(GetYearlySalesReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<PeriodSalesDto>>.Success(await _service.GetYearlySalesAsync(request.TenantId, request.Filter, ct));
}

public class GetSalesByBranchReportQueryHandler : IRequestHandler<GetSalesByBranchReportQuery, Result<IReadOnlyList<BranchSalesDto>>>
{
    private readonly ISalesAnalyticsService _service;
    public GetSalesByBranchReportQueryHandler(ISalesAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<BranchSalesDto>>> Handle(GetSalesByBranchReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<BranchSalesDto>>.Success(await _service.GetSalesByBranchAsync(request.TenantId, request.Filter, ct));
}

public class GetSalesByCashierReportQueryHandler : IRequestHandler<GetSalesByCashierReportQuery, Result<IReadOnlyList<CashierSalesDto>>>
{
    private readonly ISalesAnalyticsService _service;
    public GetSalesByCashierReportQueryHandler(ISalesAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<CashierSalesDto>>> Handle(GetSalesByCashierReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<CashierSalesDto>>.Success(await _service.GetSalesByCashierAsync(request.TenantId, request.Filter, ct));
}

public class GetSalesByProductReportQueryHandler : IRequestHandler<GetSalesByProductReportQuery, Result<IReadOnlyList<ProductSalesDto>>>
{
    private readonly ISalesAnalyticsService _service;
    public GetSalesByProductReportQueryHandler(ISalesAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<ProductSalesDto>>> Handle(GetSalesByProductReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<ProductSalesDto>>.Success(await _service.GetSalesByProductAsync(request.TenantId, request.Filter, ct));
}

public class GetSalesByCategoryReportQueryHandler : IRequestHandler<GetSalesByCategoryReportQuery, Result<IReadOnlyList<CategorySalesDto>>>
{
    private readonly ISalesAnalyticsService _service;
    public GetSalesByCategoryReportQueryHandler(ISalesAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<CategorySalesDto>>> Handle(GetSalesByCategoryReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<CategorySalesDto>>.Success(await _service.GetSalesByCategoryAsync(request.TenantId, request.Filter, ct));
}

public class GetSalesByHourReportQueryHandler : IRequestHandler<GetSalesByHourReportQuery, Result<IReadOnlyList<HourlySalesDto>>>
{
    private readonly ISalesAnalyticsService _service;
    public GetSalesByHourReportQueryHandler(ISalesAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<HourlySalesDto>>> Handle(GetSalesByHourReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<HourlySalesDto>>.Success(await _service.GetSalesByHourAsync(request.TenantId, request.Filter, ct));
}

public class GetSalesByOrderTypeReportQueryHandler : IRequestHandler<GetSalesByOrderTypeReportQuery, Result<IReadOnlyList<OrderTypeSalesDto>>>
{
    private readonly ISalesAnalyticsService _service;
    public GetSalesByOrderTypeReportQueryHandler(ISalesAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<OrderTypeSalesDto>>> Handle(GetSalesByOrderTypeReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<OrderTypeSalesDto>>.Success(await _service.GetSalesByOrderTypeAsync(request.TenantId, request.Filter, ct));
}

public class GetSalesByPaymentMethodReportQueryHandler : IRequestHandler<GetSalesByPaymentMethodReportQuery, Result<IReadOnlyList<PaymentMethodSalesDto>>>
{
    private readonly ISalesAnalyticsService _service;
    public GetSalesByPaymentMethodReportQueryHandler(ISalesAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<PaymentMethodSalesDto>>> Handle(GetSalesByPaymentMethodReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<PaymentMethodSalesDto>>.Success(await _service.GetSalesByPaymentMethodAsync(request.TenantId, request.Filter, ct));
}

public class GetCancelledOrdersReportQueryHandler : IRequestHandler<GetCancelledOrdersReportQuery, Result<IReadOnlyList<CancelledOrderDto>>>
{
    private readonly ISalesAnalyticsService _service;
    public GetCancelledOrdersReportQueryHandler(ISalesAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<CancelledOrderDto>>> Handle(GetCancelledOrdersReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<CancelledOrderDto>>.Success(await _service.GetCancelledOrdersAsync(request.TenantId, request.Filter, ct));
}

public class GetDiscountReportQueryHandler : IRequestHandler<GetDiscountReportQuery, Result<IReadOnlyList<DiscountReportDto>>>
{
    private readonly ISalesAnalyticsService _service;
    public GetDiscountReportQueryHandler(ISalesAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<DiscountReportDto>>> Handle(GetDiscountReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<DiscountReportDto>>.Success(await _service.GetDiscountReportAsync(request.TenantId, request.Filter, ct));
}

public class GetVatSalesReportQueryHandler : IRequestHandler<GetVatSalesReportQuery, Result<VatSalesReportDto>>
{
    private readonly ISalesAnalyticsService _service;
    public GetVatSalesReportQueryHandler(ISalesAnalyticsService service) => _service = service;
    public async Task<Result<VatSalesReportDto>> Handle(GetVatSalesReportQuery request, CancellationToken ct)
        => Result<VatSalesReportDto>.Success(await _service.GetVatReportAsync(request.TenantId, request.Filter, ct));
}

public class GetKitchenPerformanceReportQueryHandler : IRequestHandler<GetKitchenPerformanceReportQuery, Result<KitchenPerformanceDto>>
{
    private readonly IKitchenAnalyticsService _service;
    public GetKitchenPerformanceReportQueryHandler(IKitchenAnalyticsService service) => _service = service;
    public async Task<Result<KitchenPerformanceDto>> Handle(GetKitchenPerformanceReportQuery request, CancellationToken ct)
        => Result<KitchenPerformanceDto>.Success(await _service.GetKitchenPerformanceAsync(request.TenantId, request.Filter, ct));
}

public class GetDelayedOrdersReportQueryHandler : IRequestHandler<GetDelayedOrdersReportQuery, Result<IReadOnlyList<DelayedOrderDto>>>
{
    private readonly IKitchenAnalyticsService _service;
    public GetDelayedOrdersReportQueryHandler(IKitchenAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<DelayedOrderDto>>> Handle(GetDelayedOrdersReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<DelayedOrderDto>>.Success(await _service.GetDelayedOrdersAsync(request.TenantId, request.Filter, ct));
}

public class GetKitchenStationLoadReportQueryHandler : IRequestHandler<GetKitchenStationLoadReportQuery, Result<IReadOnlyList<KitchenStationLoadDto>>>
{
    private readonly IKitchenAnalyticsService _service;
    public GetKitchenStationLoadReportQueryHandler(IKitchenAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<KitchenStationLoadDto>>> Handle(GetKitchenStationLoadReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<KitchenStationLoadDto>>.Success(await _service.GetStationLoadAsync(request.TenantId, request.Filter, ct));
}

public class GetTopDelayedProductsReportQueryHandler : IRequestHandler<GetTopDelayedProductsReportQuery, Result<IReadOnlyList<TopDelayedProductDto>>>
{
    private readonly IKitchenAnalyticsService _service;
    public GetTopDelayedProductsReportQueryHandler(IKitchenAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<TopDelayedProductDto>>> Handle(GetTopDelayedProductsReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<TopDelayedProductDto>>.Success(await _service.GetTopDelayedProductsAsync(request.TenantId, request.Filter, ct));
}

public class GetDeliverySummaryReportQueryHandler : IRequestHandler<GetDeliverySummaryReportQuery, Result<DeliverySummaryDto>>
{
    private readonly IDeliveryAnalyticsService _service;
    public GetDeliverySummaryReportQueryHandler(IDeliveryAnalyticsService service) => _service = service;
    public async Task<Result<DeliverySummaryDto>> Handle(GetDeliverySummaryReportQuery request, CancellationToken ct)
        => Result<DeliverySummaryDto>.Success(await _service.GetDeliverySummaryAsync(request.TenantId, request.Filter, ct));
}

public class GetDriverPerformanceReportQueryHandler : IRequestHandler<GetDriverPerformanceReportQuery, Result<IReadOnlyList<DriverPerformanceDto>>>
{
    private readonly IDeliveryAnalyticsService _service;
    public GetDriverPerformanceReportQueryHandler(IDeliveryAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<DriverPerformanceDto>>> Handle(GetDriverPerformanceReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<DriverPerformanceDto>>.Success(await _service.GetDriverPerformanceAsync(request.TenantId, request.Filter, ct));
}

public class GetDeliveryByZoneReportQueryHandler : IRequestHandler<GetDeliveryByZoneReportQuery, Result<IReadOnlyList<DeliveryZoneReportDto>>>
{
    private readonly IDeliveryAnalyticsService _service;
    public GetDeliveryByZoneReportQueryHandler(IDeliveryAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<DeliveryZoneReportDto>>> Handle(GetDeliveryByZoneReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<DeliveryZoneReportDto>>.Success(await _service.GetDeliveryByZoneAsync(request.TenantId, request.Filter, ct));
}

public class GetFailedDeliveriesReportQueryHandler : IRequestHandler<GetFailedDeliveriesReportQuery, Result<IReadOnlyList<FailedDeliveryDto>>>
{
    private readonly IDeliveryAnalyticsService _service;
    public GetFailedDeliveriesReportQueryHandler(IDeliveryAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<FailedDeliveryDto>>> Handle(GetFailedDeliveriesReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<FailedDeliveryDto>>.Success(await _service.GetFailedDeliveriesAsync(request.TenantId, request.Filter, ct));
}

public class GetStockBalanceReportQueryHandler : IRequestHandler<GetStockBalanceReportQuery, Result<IReadOnlyList<StockBalanceDto>>>
{
    private readonly IInventoryAnalyticsService _service;
    public GetStockBalanceReportQueryHandler(IInventoryAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<StockBalanceDto>>> Handle(GetStockBalanceReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<StockBalanceDto>>.Success(await _service.GetStockBalanceAsync(request.TenantId, request.Filter, ct));
}

public class GetStockValuationReportQueryHandler : IRequestHandler<GetStockValuationReportQuery, Result<StockValuationDto>>
{
    private readonly IInventoryAnalyticsService _service;
    public GetStockValuationReportQueryHandler(IInventoryAnalyticsService service) => _service = service;
    public async Task<Result<StockValuationDto>> Handle(GetStockValuationReportQuery request, CancellationToken ct)
        => Result<StockValuationDto>.Success(await _service.GetStockValuationAsync(request.TenantId, request.Filter, ct));
}

public class GetInventoryMovementReportQueryHandler : IRequestHandler<GetInventoryMovementReportQuery, Result<IReadOnlyList<InventoryMovementDto>>>
{
    private readonly IInventoryAnalyticsService _service;
    public GetInventoryMovementReportQueryHandler(IInventoryAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<InventoryMovementDto>>> Handle(GetInventoryMovementReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<InventoryMovementDto>>.Success(await _service.GetInventoryMovementAsync(request.TenantId, request.Filter, ct));
}

public class GetWasteAnalysisReportQueryHandler : IRequestHandler<GetWasteAnalysisReportQuery, Result<IReadOnlyList<WasteAnalysisDto>>>
{
    private readonly IInventoryAnalyticsService _service;
    public GetWasteAnalysisReportQueryHandler(IInventoryAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<WasteAnalysisDto>>> Handle(GetWasteAnalysisReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<WasteAnalysisDto>>.Success(await _service.GetWasteAnalysisAsync(request.TenantId, request.Filter, ct));
}

public class GetAdjustmentAnalysisReportQueryHandler : IRequestHandler<GetAdjustmentAnalysisReportQuery, Result<IReadOnlyList<AdjustmentAnalysisDto>>>
{
    private readonly IInventoryAnalyticsService _service;
    public GetAdjustmentAnalysisReportQueryHandler(IInventoryAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<AdjustmentAnalysisDto>>> Handle(GetAdjustmentAnalysisReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<AdjustmentAnalysisDto>>.Success(await _service.GetAdjustmentAnalysisAsync(request.TenantId, request.Filter, ct));
}

public class GetConsumptionReportQueryHandler : IRequestHandler<GetConsumptionReportQuery, Result<IReadOnlyList<ConsumptionReportDto>>>
{
    private readonly IInventoryAnalyticsService _service;
    public GetConsumptionReportQueryHandler(IInventoryAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<ConsumptionReportDto>>> Handle(GetConsumptionReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<ConsumptionReportDto>>.Success(await _service.GetConsumptionReportAsync(request.TenantId, request.Filter, ct));
}

public class GetRecipeCostReportQueryHandler : IRequestHandler<GetRecipeCostReportQuery, Result<IReadOnlyList<RecipeCostDto>>>
{
    private readonly IInventoryAnalyticsService _service;
    public GetRecipeCostReportQueryHandler(IInventoryAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<RecipeCostDto>>> Handle(GetRecipeCostReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<RecipeCostDto>>.Success(await _service.GetRecipeCostReportAsync(request.TenantId, request.Filter, ct));
}

public class GetPurchaseAnalysisReportQueryHandler : IRequestHandler<GetPurchaseAnalysisReportQuery, Result<IReadOnlyList<PurchaseAnalysisDto>>>
{
    private readonly IInventoryAnalyticsService _service;
    public GetPurchaseAnalysisReportQueryHandler(IInventoryAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<PurchaseAnalysisDto>>> Handle(GetPurchaseAnalysisReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<PurchaseAnalysisDto>>.Success(await _service.GetPurchaseAnalysisAsync(request.TenantId, request.Filter, ct));
}

public class GetSupplierPerformanceReportQueryHandler : IRequestHandler<GetSupplierPerformanceReportQuery, Result<IReadOnlyList<SupplierPerformanceDto>>>
{
    private readonly IInventoryAnalyticsService _service;
    public GetSupplierPerformanceReportQueryHandler(IInventoryAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<SupplierPerformanceDto>>> Handle(GetSupplierPerformanceReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<SupplierPerformanceDto>>.Success(await _service.GetSupplierPerformanceAsync(request.TenantId, request.Filter, ct));
}

public class GetCustomerActivityReportQueryHandler : IRequestHandler<GetCustomerActivityReportQuery, Result<IReadOnlyList<CustomerActivityDto>>>
{
    private readonly ICustomerAnalyticsService _service;
    public GetCustomerActivityReportQueryHandler(ICustomerAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<CustomerActivityDto>>> Handle(GetCustomerActivityReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<CustomerActivityDto>>.Success(await _service.GetCustomerActivityAsync(request.TenantId, request.Filter, ct));
}

public class GetCustomerLtvReportQueryHandler : IRequestHandler<GetCustomerLtvReportQuery, Result<IReadOnlyList<CustomerLtvDto>>>
{
    private readonly ICustomerAnalyticsService _service;
    public GetCustomerLtvReportQueryHandler(ICustomerAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<CustomerLtvDto>>> Handle(GetCustomerLtvReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<CustomerLtvDto>>.Success(await _service.GetCustomerLtvAsync(request.TenantId, request.Filter, ct));
}

public class GetCustomerFrequencyReportQueryHandler : IRequestHandler<GetCustomerFrequencyReportQuery, Result<IReadOnlyList<CustomerFrequencyDto>>>
{
    private readonly ICustomerAnalyticsService _service;
    public GetCustomerFrequencyReportQueryHandler(ICustomerAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<CustomerFrequencyDto>>> Handle(GetCustomerFrequencyReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<CustomerFrequencyDto>>.Success(await _service.GetCustomerFrequencyAsync(request.TenantId, request.Filter, ct));
}

public class GetLoyaltyPointsReportQueryHandler : IRequestHandler<GetLoyaltyPointsReportQuery, Result<LoyaltyPointsReportDto>>
{
    private readonly ICustomerAnalyticsService _service;
    public GetLoyaltyPointsReportQueryHandler(ICustomerAnalyticsService service) => _service = service;
    public async Task<Result<LoyaltyPointsReportDto>> Handle(GetLoyaltyPointsReportQuery request, CancellationToken ct)
        => Result<LoyaltyPointsReportDto>.Success(await _service.GetLoyaltyPointsReportAsync(request.TenantId, request.Filter, ct));
}

public class GetCouponUsageReportQueryHandler : IRequestHandler<GetCouponUsageReportQuery, Result<IReadOnlyList<CouponUsageDto>>>
{
    private readonly ICustomerAnalyticsService _service;
    public GetCouponUsageReportQueryHandler(ICustomerAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<CouponUsageDto>>> Handle(GetCouponUsageReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<CouponUsageDto>>.Success(await _service.GetCouponUsageAsync(request.TenantId, request.Filter, ct));
}

public class GetGiftCardUsageReportQueryHandler : IRequestHandler<GetGiftCardUsageReportQuery, Result<GiftCardUsageDto>>
{
    private readonly ICustomerAnalyticsService _service;
    public GetGiftCardUsageReportQueryHandler(ICustomerAnalyticsService service) => _service = service;
    public async Task<Result<GiftCardUsageDto>> Handle(GetGiftCardUsageReportQuery request, CancellationToken ct)
        => Result<GiftCardUsageDto>.Success(await _service.GetGiftCardUsageAsync(request.TenantId, request.Filter, ct));
}

public class GetMembershipDistributionReportQueryHandler : IRequestHandler<GetMembershipDistributionReportQuery, Result<IReadOnlyList<MembershipDistributionDto>>>
{
    private readonly ICustomerAnalyticsService _service;
    public GetMembershipDistributionReportQueryHandler(ICustomerAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<MembershipDistributionDto>>> Handle(GetMembershipDistributionReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<MembershipDistributionDto>>.Success(await _service.GetMembershipDistributionAsync(request.TenantId, request.Filter, ct));
}

public class GetBalanceSheetReportQueryHandler : IRequestHandler<GetBalanceSheetReportQuery, Result<BalanceSheetDto>>
{
    private readonly IFinancialAnalyticsService _service;
    public GetBalanceSheetReportQueryHandler(IFinancialAnalyticsService service) => _service = service;
    public async Task<Result<BalanceSheetDto>> Handle(GetBalanceSheetReportQuery request, CancellationToken ct)
        => Result<BalanceSheetDto>.Success(await _service.GetBalanceSheetAsync(request.TenantId, request.Filter, ct));
}

public class GetIncomeStatementReportQueryHandler : IRequestHandler<GetIncomeStatementReportQuery, Result<IncomeStatementDto>>
{
    private readonly IFinancialAnalyticsService _service;
    public GetIncomeStatementReportQueryHandler(IFinancialAnalyticsService service) => _service = service;
    public async Task<Result<IncomeStatementDto>> Handle(GetIncomeStatementReportQuery request, CancellationToken ct)
        => Result<IncomeStatementDto>.Success(await _service.GetIncomeStatementAsync(request.TenantId, request.Filter, ct));
}

public class GetCashFlowReportQueryHandler : IRequestHandler<GetCashFlowReportQuery, Result<CashFlowDto>>
{
    private readonly IFinancialAnalyticsService _service;
    public GetCashFlowReportQueryHandler(IFinancialAnalyticsService service) => _service = service;
    public async Task<Result<CashFlowDto>> Handle(GetCashFlowReportQuery request, CancellationToken ct)
        => Result<CashFlowDto>.Success(await _service.GetCashFlowAsync(request.TenantId, request.Filter, ct));
}

public class GetVatSummaryReportQueryHandler : IRequestHandler<GetVatSummaryReportQuery, Result<VatSummaryDto>>
{
    private readonly IFinancialAnalyticsService _service;
    public GetVatSummaryReportQueryHandler(IFinancialAnalyticsService service) => _service = service;
    public async Task<Result<VatSummaryDto>> Handle(GetVatSummaryReportQuery request, CancellationToken ct)
        => Result<VatSummaryDto>.Success(await _service.GetVatSummaryAsync(request.TenantId, request.Filter, ct));
}

public class GetRevenueAnalysisReportQueryHandler : IRequestHandler<GetRevenueAnalysisReportQuery, Result<IReadOnlyList<RevenueAnalysisDto>>>
{
    private readonly IFinancialAnalyticsService _service;
    public GetRevenueAnalysisReportQueryHandler(IFinancialAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<RevenueAnalysisDto>>> Handle(GetRevenueAnalysisReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<RevenueAnalysisDto>>.Success(await _service.GetRevenueAnalysisAsync(request.TenantId, request.Filter, ct));
}

public class GetExpenseAnalysisReportQueryHandler : IRequestHandler<GetExpenseAnalysisReportQuery, Result<IReadOnlyList<ExpenseAnalysisDto>>>
{
    private readonly IFinancialAnalyticsService _service;
    public GetExpenseAnalysisReportQueryHandler(IFinancialAnalyticsService service) => _service = service;
    public async Task<Result<IReadOnlyList<ExpenseAnalysisDto>>> Handle(GetExpenseAnalysisReportQuery request, CancellationToken ct)
        => Result<IReadOnlyList<ExpenseAnalysisDto>>.Success(await _service.GetExpenseAnalysisAsync(request.TenantId, request.Filter, ct));
}

public class GetTrialBalanceReportQueryHandler : IRequestHandler<GetTrialBalanceReportQuery, Result<IReadOnlyList<TrialBalanceLineDto>>>
{
    private readonly IMediator _mediator;
    public GetTrialBalanceReportQueryHandler(IMediator mediator) => _mediator = mediator;
    public async Task<Result<IReadOnlyList<TrialBalanceLineDto>>> Handle(GetTrialBalanceReportQuery request, CancellationToken ct)
        => await _mediator.Send(new GetTrialBalanceQuery(request.TenantId, request.Filter), ct);
}

public class GetGeneralLedgerReportQueryHandler : IRequestHandler<GetGeneralLedgerReportQuery, Result<IReadOnlyList<GeneralLedgerLineDto>>>
{
    private readonly IMediator _mediator;
    public GetGeneralLedgerReportQueryHandler(IMediator mediator) => _mediator = mediator;
    public async Task<Result<IReadOnlyList<GeneralLedgerLineDto>>> Handle(GetGeneralLedgerReportQuery request, CancellationToken ct)
        => await _mediator.Send(new GetGeneralLedgerQuery(request.Filter), ct);
}

public class GetJournalRegisterReportQueryHandler : IRequestHandler<GetJournalRegisterReportQuery, PagedResult<JournalDto>>
{
    private readonly IMediator _mediator;
    public GetJournalRegisterReportQueryHandler(IMediator mediator) => _mediator = mediator;
    public Task<PagedResult<JournalDto>> Handle(GetJournalRegisterReportQuery request, CancellationToken ct)
        => _mediator.Send(new GetJournalRegisterQuery(request.TenantId, request.Filter), ct);
}

public class GetKpiDashboardQueryHandler : IRequestHandler<GetKpiDashboardQuery, Result<KpiDashboardDto>>
{
    private readonly IKpiEngineService _service;
    public GetKpiDashboardQueryHandler(IKpiEngineService service) => _service = service;
    public async Task<Result<KpiDashboardDto>> Handle(GetKpiDashboardQuery request, CancellationToken ct)
        => Result<KpiDashboardDto>.Success(await _service.GetKpiDashboardAsync(request.TenantId, request.Filter, ct));
}

public class ExportReportQueryHandler : IRequestHandler<ExportReportQuery, Result<ExportResultDto>>
{
    private readonly IReportExportService _service;
    public ExportReportQueryHandler(IReportExportService service) => _service = service;
    public async Task<Result<ExportResultDto>> Handle(ExportReportQuery request, CancellationToken ct)
        => Result<ExportResultDto>.Success(await _service.ExportAsync(request.TenantId, request.Request, ct));
}
