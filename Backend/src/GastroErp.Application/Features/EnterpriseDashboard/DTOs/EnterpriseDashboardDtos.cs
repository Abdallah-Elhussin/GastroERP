namespace GastroErp.Application.Features.EnterpriseDashboard.DTOs;

public enum DashboardPeriod
{
    Today = 0,
    Yesterday = 1,
    ThisWeek = 2,
    ThisMonth = 3,
    Custom = 4
}

public record EnterpriseDashboardFilterDto(
    DashboardPeriod Period = DashboardPeriod.Today,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    Guid? BranchId = null,
    Guid? WarehouseId = null,
    Guid? CashierId = null,
    Guid? CompanyId = null,
    string? Currency = null);

public record DashboardKpiDto(
    string Key,
    string Label,
    decimal Value,
    string? Unit,
    decimal? ChangePercent,
    bool IsHigherBetter,
    IReadOnlyList<decimal> Sparkline,
    DateTimeOffset UpdatedAt);

public record DashboardNamedValueDto(string Name, decimal Value, decimal? Percent = null);

public record DashboardSeriesPointDto(string Label, decimal Sales, decimal Profit, decimal Discounts, decimal Tax);

public record DashboardTableRowDto(
    string Id,
    string Name,
    decimal Quantity,
    decimal Revenue,
    decimal? Profit,
    decimal? Percent,
    DateTimeOffset? LastActivity = null,
    decimal? CurrentQty = null,
    decimal? MinQty = null,
    decimal? SuggestedQty = null);

public record DashboardKitchenStatusDto(
    int Pending,
    int Preparing,
    int Ready,
    int Served,
    int Delayed,
    double AvgPrepMinutes);

public record DashboardDeliveryStatusDto(
    int InProgress,
    int Delivered,
    int Delayed,
    double AvgDeliveryMinutes);

public record DashboardHrSnapshotDto(
    int Present,
    int Absent,
    int Late,
    decimal WorkedHours);

public record DashboardFinanceSnapshotDto(
    decimal BankBalance,
    decimal CashBalance,
    decimal Receivables,
    decimal Payables,
    decimal Profit);

public record DashboardActivityDto(
    string Type,
    string Title,
    string? Reference,
    DateTimeOffset At,
    string? UserName);

public record DashboardNotificationDto(
    string Severity,
    string Code,
    string Message,
    DateTimeOffset At,
    string? Link);

public record DashboardInsightDto(
    string Category,
    string Title,
    string Detail,
    string? ActionHint);

public record DashboardQuickActionDto(string Key, string Label, string Route, string Icon);

public record DashboardHeaderDto(
    string CompanyName,
    string? BranchName,
    string UserName,
    DateTimeOffset ServerTime,
    DateTimeOffset LastSyncedAt,
    string Currency);

public record EnterpriseDashboardOverviewDto(
    DashboardHeaderDto Header,
    IReadOnlyList<DashboardKpiDto> Kpis,
    IReadOnlyList<DashboardNotificationDto> Notifications,
    IReadOnlyList<DashboardInsightDto> Insights,
    IReadOnlyList<DashboardQuickActionDto> QuickActions);

public record EnterpriseDashboardSalesDto(
    IReadOnlyList<DashboardSeriesPointDto> Trend,
    IReadOnlyList<DashboardNamedValueDto> RevenueSources,
    IReadOnlyList<DashboardNamedValueDto> PaymentMethods);

public record EnterpriseDashboardProductsDto(
    IReadOnlyList<DashboardTableRowDto> TopSelling,
    IReadOnlyList<DashboardTableRowDto> WorstSelling);

public record EnterpriseDashboardCustomersDto(
    IReadOnlyList<DashboardTableRowDto> TopCustomers);

public record EnterpriseDashboardInventoryDto(
    decimal InventoryValue,
    int LowStockCount,
    IReadOnlyList<DashboardTableRowDto> LowStockItems);

public record EnterpriseDashboardFinanceDto(DashboardFinanceSnapshotDto Snapshot);

public record EnterpriseDashboardKitchenDto(DashboardKitchenStatusDto Status);

public record EnterpriseDashboardDeliveryDto(DashboardDeliveryStatusDto Status);

public record EnterpriseDashboardHrDto(DashboardHrSnapshotDto Snapshot);

public record EnterpriseDashboardActivitiesDto(IReadOnlyList<DashboardActivityDto> Items);

public record EnterpriseDashboardLayoutDto(string LayoutJson);
