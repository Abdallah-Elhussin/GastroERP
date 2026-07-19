using Asp.Versioning;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Application.Features.Reporting.DTOs;
using GastroErp.Application.Features.Reporting.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Reporting;

[ApiVersion("1.0")]
public class DashboardController : BaseApiController
{
    [HttpGet(ApiRoutes.Reports.Dashboard)]
    [HasPermission(Permissions.Dashboard.View)]
    public async Task<IActionResult> GetExecutiveDashboard([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetExecutiveDashboardQuery(TenantId, filter)));
}

[ApiVersion("1.0")]
public class SalesReportsController : BaseApiController
{
    [HttpGet($"{ApiRoutes.Reports.Sales}/daily")]
    [HasPermission(Permissions.SalesReports.View)]
    public async Task<IActionResult> GetDailySales([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetDailySalesReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Sales}/monthly")]
    [HasPermission(Permissions.SalesReports.View)]
    public async Task<IActionResult> GetMonthlySales([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetMonthlySalesReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Sales}/yearly")]
    [HasPermission(Permissions.SalesReports.View)]
    public async Task<IActionResult> GetYearlySales([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetYearlySalesReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Sales}/by-branch")]
    [HasPermission(Permissions.SalesReports.View)]
    public async Task<IActionResult> GetSalesByBranch([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetSalesByBranchReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Sales}/by-cashier")]
    [HasPermission(Permissions.SalesReports.View)]
    public async Task<IActionResult> GetSalesByCashier([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetSalesByCashierReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Sales}/by-product")]
    [HasPermission(Permissions.SalesReports.View)]
    public async Task<IActionResult> GetSalesByProduct([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetSalesByProductReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Sales}/by-category")]
    [HasPermission(Permissions.SalesReports.View)]
    public async Task<IActionResult> GetSalesByCategory([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetSalesByCategoryReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Sales}/by-hour")]
    [HasPermission(Permissions.SalesReports.View)]
    public async Task<IActionResult> GetSalesByHour([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetSalesByHourReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Sales}/by-order-type")]
    [HasPermission(Permissions.SalesReports.View)]
    public async Task<IActionResult> GetSalesByOrderType([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetSalesByOrderTypeReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Sales}/by-payment-method")]
    [HasPermission(Permissions.SalesReports.View)]
    public async Task<IActionResult> GetSalesByPaymentMethod([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetSalesByPaymentMethodReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Sales}/cancelled")]
    [HasPermission(Permissions.SalesReports.View)]
    public async Task<IActionResult> GetCancelledOrders([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetCancelledOrdersReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Sales}/discounts")]
    [HasPermission(Permissions.SalesReports.View)]
    public async Task<IActionResult> GetDiscountReport([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetDiscountReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Sales}/vat")]
    [HasPermission(Permissions.SalesReports.View)]
    public async Task<IActionResult> GetVatReport([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetVatSalesReportQuery(TenantId, filter)));
}

[ApiVersion("1.0")]
public class KitchenReportsController : BaseApiController
{
    [HttpGet($"{ApiRoutes.Reports.Kitchen}/performance")]
    [HasPermission(Permissions.KitchenReports.View)]
    public async Task<IActionResult> GetKitchenPerformance([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetKitchenPerformanceReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Kitchen}/delayed")]
    [HasPermission(Permissions.KitchenReports.View)]
    public async Task<IActionResult> GetDelayedOrders([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetDelayedOrdersReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Kitchen}/station-load")]
    [HasPermission(Permissions.KitchenReports.View)]
    public async Task<IActionResult> GetStationLoad([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetKitchenStationLoadReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Kitchen}/top-delayed-products")]
    [HasPermission(Permissions.KitchenReports.View)]
    public async Task<IActionResult> GetTopDelayedProducts([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetTopDelayedProductsReportQuery(TenantId, filter)));
}

[ApiVersion("1.0")]
public class DeliveryReportsController : BaseApiController
{
    [HttpGet($"{ApiRoutes.Reports.Delivery}/summary")]
    [HasPermission(Permissions.DeliveryReports.View)]
    public async Task<IActionResult> GetDeliverySummary([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetDeliverySummaryReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Delivery}/drivers")]
    [HasPermission(Permissions.DeliveryReports.View)]
    public async Task<IActionResult> GetDriverPerformance([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetDriverPerformanceReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Delivery}/zones")]
    [HasPermission(Permissions.DeliveryReports.View)]
    public async Task<IActionResult> GetDeliveryByZone([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetDeliveryByZoneReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Delivery}/failed")]
    [HasPermission(Permissions.DeliveryReports.View)]
    public async Task<IActionResult> GetFailedDeliveries([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetFailedDeliveriesReportQuery(TenantId, filter)));
}

[ApiVersion("1.0")]
public class InventoryReportsController : BaseApiController
{
    [HttpGet($"{ApiRoutes.Reports.Inventory}/stock-balance")]
    [HasPermission(Permissions.InventoryReports.View)]
    public async Task<IActionResult> GetStockBalance([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetStockBalanceReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Inventory}/valuation")]
    [HasPermission(Permissions.InventoryReports.View)]
    public async Task<IActionResult> GetStockValuation([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetStockValuationReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Inventory}/movements")]
    [HasPermission(Permissions.InventoryReports.View)]
    public async Task<IActionResult> GetInventoryMovement([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetInventoryMovementReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Inventory}/waste")]
    [HasPermission(Permissions.InventoryReports.View)]
    public async Task<IActionResult> GetWasteAnalysis([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetWasteAnalysisReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Inventory}/adjustments")]
    [HasPermission(Permissions.InventoryReports.View)]
    public async Task<IActionResult> GetAdjustmentAnalysis([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetAdjustmentAnalysisReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Inventory}/consumption")]
    [HasPermission(Permissions.InventoryReports.View)]
    public async Task<IActionResult> GetConsumption([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetConsumptionReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Inventory}/recipe-cost")]
    [HasPermission(Permissions.InventoryReports.View)]
    public async Task<IActionResult> GetRecipeCost([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetRecipeCostReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Inventory}/purchases")]
    [HasPermission(Permissions.InventoryReports.View)]
    public async Task<IActionResult> GetPurchaseAnalysis([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetPurchaseAnalysisReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Inventory}/suppliers")]
    [HasPermission(Permissions.InventoryReports.View)]
    public async Task<IActionResult> GetSupplierPerformance([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetSupplierPerformanceReportQuery(TenantId, filter)));
}

[ApiVersion("1.0")]
public class CustomerReportsController : BaseApiController
{
    [HttpGet($"{ApiRoutes.Reports.Customers}/activity")]
    [HasPermission(Permissions.CustomerReports.View)]
    public async Task<IActionResult> GetCustomerActivity([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetCustomerActivityReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Customers}/ltv")]
    [HasPermission(Permissions.CustomerReports.View)]
    public async Task<IActionResult> GetCustomerLtv([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetCustomerLtvReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Customers}/frequency")]
    [HasPermission(Permissions.CustomerReports.View)]
    public async Task<IActionResult> GetCustomerFrequency([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetCustomerFrequencyReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Customers}/loyalty")]
    [HasPermission(Permissions.CustomerReports.View)]
    public async Task<IActionResult> GetLoyaltyPoints([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetLoyaltyPointsReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Customers}/coupons")]
    [HasPermission(Permissions.CustomerReports.View)]
    public async Task<IActionResult> GetCouponUsage([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetCouponUsageReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Customers}/gift-cards")]
    [HasPermission(Permissions.CustomerReports.View)]
    public async Task<IActionResult> GetGiftCardUsage([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetGiftCardUsageReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Customers}/membership")]
    [HasPermission(Permissions.CustomerReports.View)]
    public async Task<IActionResult> GetMembershipDistribution([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetMembershipDistributionReportQuery(TenantId, filter)));
}

[ApiVersion("1.0")]
public class FinanceReportsController : BaseApiController
{
    [HttpGet($"{ApiRoutes.Reports.Finance}/balance-sheet")]
    [HasPermission(Permissions.FinanceReports.View)]
    public async Task<IActionResult> GetBalanceSheet([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetBalanceSheetReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Finance}/income-statement")]
    [HasPermission(Permissions.FinanceReports.View)]
    public async Task<IActionResult> GetIncomeStatement([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetIncomeStatementReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Finance}/cash-flow")]
    [HasPermission(Permissions.FinanceReports.View)]
    public async Task<IActionResult> GetCashFlow([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetCashFlowReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Finance}/vat-summary")]
    [HasPermission(Permissions.FinanceReports.View)]
    public async Task<IActionResult> GetVatSummary([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetVatSummaryReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Finance}/revenue-analysis")]
    [HasPermission(Permissions.FinanceReports.View)]
    public async Task<IActionResult> GetRevenueAnalysis([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetRevenueAnalysisReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Finance}/expense-analysis")]
    [HasPermission(Permissions.FinanceReports.View)]
    public async Task<IActionResult> GetExpenseAnalysis([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetExpenseAnalysisReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Finance}/trial-balance")]
    [HasPermission(Permissions.FinanceReports.View)]
    public async Task<IActionResult> GetTrialBalance([FromQuery] TrialBalanceFilterDto filter)
        => HandleResult(await Mediator.Send(new GetTrialBalanceReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Finance}/general-ledger")]
    [HasPermission(Permissions.FinanceReports.View)]
    public async Task<IActionResult> GetGeneralLedger([FromQuery] GeneralLedgerFilterDto filter)
        => HandleResult(await Mediator.Send(new GetGeneralLedgerReportQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Reports.Finance}/journal-register")]
    [HasPermission(Permissions.FinanceReports.View)]
    public async Task<IActionResult> GetJournalRegister([FromQuery] JournalRegisterFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetJournalRegisterReportQuery(TenantId, filter)));
}

[ApiVersion("1.0")]
public class AnalyticsController : BaseApiController
{
    [HttpGet($"{ApiRoutes.Reports.Analytics}/kpi")]
    [HasPermission(Permissions.Reports.View)]
    public async Task<IActionResult> GetKpiDashboard([FromQuery] ReportFilterDto filter)
        => HandleResult(await Mediator.Send(new GetKpiDashboardQuery(TenantId, filter)));

    [HttpPost($"{ApiRoutes.Reports.Analytics}/export")]
    [HasPermission(Permissions.Reports.Export)]
    public async Task<IActionResult> ExportReport([FromBody] ExportReportRequestDto request)
    {
        var result = await Mediator.Send(new ExportReportQuery(TenantId, request));
        if (!result.IsSuccess) return HandleResult(result);
        return File(result.Data!.Content, result.Data.ContentType, result.Data.FileName);
    }
}
