using GastroErp.Application.Features.EnterpriseDashboard.DTOs;
using GastroErp.Application.Features.Reporting.DTOs;

namespace GastroErp.Application.Features.EnterpriseDashboard.Services;

public interface IEnterpriseDashboardAggregator
{
    Task<EnterpriseDashboardOverviewDto> GetOverviewAsync(
        Guid tenantId, string? userName, EnterpriseDashboardFilterDto filter, CancellationToken ct = default);

    Task<EnterpriseDashboardSalesDto> GetSalesAsync(
        Guid tenantId, EnterpriseDashboardFilterDto filter, CancellationToken ct = default);

    Task<EnterpriseDashboardProductsDto> GetProductsAsync(
        Guid tenantId, EnterpriseDashboardFilterDto filter, CancellationToken ct = default);

    Task<EnterpriseDashboardCustomersDto> GetCustomersAsync(
        Guid tenantId, EnterpriseDashboardFilterDto filter, CancellationToken ct = default);

    Task<EnterpriseDashboardInventoryDto> GetInventoryAsync(
        Guid tenantId, EnterpriseDashboardFilterDto filter, CancellationToken ct = default);

    Task<EnterpriseDashboardFinanceDto> GetFinanceAsync(
        Guid tenantId, EnterpriseDashboardFilterDto filter, CancellationToken ct = default);

    Task<EnterpriseDashboardKitchenDto> GetKitchenAsync(
        Guid tenantId, EnterpriseDashboardFilterDto filter, CancellationToken ct = default);

    Task<EnterpriseDashboardDeliveryDto> GetDeliveryAsync(
        Guid tenantId, EnterpriseDashboardFilterDto filter, CancellationToken ct = default);

    Task<EnterpriseDashboardHrDto> GetHrAsync(
        Guid tenantId, EnterpriseDashboardFilterDto filter, CancellationToken ct = default);

    Task<EnterpriseDashboardActivitiesDto> GetActivitiesAsync(
        Guid tenantId, EnterpriseDashboardFilterDto filter, CancellationToken ct = default);

    ReportFilterDto ToReportFilter(EnterpriseDashboardFilterDto filter);
}
