using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.EnterpriseDashboard.DTOs;
using MediatR;

namespace GastroErp.Application.Features.EnterpriseDashboard.Queries;

public record GetEnterpriseDashboardOverviewQuery(
    Guid TenantId,
    string? UserName,
    EnterpriseDashboardFilterDto Filter) : IRequest<Result<EnterpriseDashboardOverviewDto>>;

public record GetEnterpriseDashboardSalesQuery(
    Guid TenantId, EnterpriseDashboardFilterDto Filter) : IRequest<Result<EnterpriseDashboardSalesDto>>;

public record GetEnterpriseDashboardProductsQuery(
    Guid TenantId, EnterpriseDashboardFilterDto Filter) : IRequest<Result<EnterpriseDashboardProductsDto>>;

public record GetEnterpriseDashboardCustomersQuery(
    Guid TenantId, EnterpriseDashboardFilterDto Filter) : IRequest<Result<EnterpriseDashboardCustomersDto>>;

public record GetEnterpriseDashboardInventoryQuery(
    Guid TenantId, EnterpriseDashboardFilterDto Filter) : IRequest<Result<EnterpriseDashboardInventoryDto>>;

public record GetEnterpriseDashboardFinanceQuery(
    Guid TenantId, EnterpriseDashboardFilterDto Filter) : IRequest<Result<EnterpriseDashboardFinanceDto>>;

public record GetEnterpriseDashboardKitchenQuery(
    Guid TenantId, EnterpriseDashboardFilterDto Filter) : IRequest<Result<EnterpriseDashboardKitchenDto>>;

public record GetEnterpriseDashboardDeliveryQuery(
    Guid TenantId, EnterpriseDashboardFilterDto Filter) : IRequest<Result<EnterpriseDashboardDeliveryDto>>;

public record GetEnterpriseDashboardHrQuery(
    Guid TenantId, EnterpriseDashboardFilterDto Filter) : IRequest<Result<EnterpriseDashboardHrDto>>;

public record GetEnterpriseDashboardActivitiesQuery(
    Guid TenantId, EnterpriseDashboardFilterDto Filter) : IRequest<Result<EnterpriseDashboardActivitiesDto>>;
