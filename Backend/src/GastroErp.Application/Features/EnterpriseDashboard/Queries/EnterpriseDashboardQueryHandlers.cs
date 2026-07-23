using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.EnterpriseDashboard.DTOs;
using GastroErp.Application.Features.EnterpriseDashboard.Services;
using MediatR;

namespace GastroErp.Application.Features.EnterpriseDashboard.Queries;

public sealed class GetEnterpriseDashboardOverviewQueryHandler(IEnterpriseDashboardAggregator aggregator)
    : IRequestHandler<GetEnterpriseDashboardOverviewQuery, Result<EnterpriseDashboardOverviewDto>>
{
    public async Task<Result<EnterpriseDashboardOverviewDto>> Handle(
        GetEnterpriseDashboardOverviewQuery request, CancellationToken cancellationToken)
        => Result<EnterpriseDashboardOverviewDto>.Success(
            await aggregator.GetOverviewAsync(request.TenantId, request.UserName, request.Filter, cancellationToken));
}

public sealed class GetEnterpriseDashboardSalesQueryHandler(IEnterpriseDashboardAggregator aggregator)
    : IRequestHandler<GetEnterpriseDashboardSalesQuery, Result<EnterpriseDashboardSalesDto>>
{
    public async Task<Result<EnterpriseDashboardSalesDto>> Handle(
        GetEnterpriseDashboardSalesQuery request, CancellationToken cancellationToken)
        => Result<EnterpriseDashboardSalesDto>.Success(
            await aggregator.GetSalesAsync(request.TenantId, request.Filter, cancellationToken));
}

public sealed class GetEnterpriseDashboardProductsQueryHandler(IEnterpriseDashboardAggregator aggregator)
    : IRequestHandler<GetEnterpriseDashboardProductsQuery, Result<EnterpriseDashboardProductsDto>>
{
    public async Task<Result<EnterpriseDashboardProductsDto>> Handle(
        GetEnterpriseDashboardProductsQuery request, CancellationToken cancellationToken)
        => Result<EnterpriseDashboardProductsDto>.Success(
            await aggregator.GetProductsAsync(request.TenantId, request.Filter, cancellationToken));
}

public sealed class GetEnterpriseDashboardCustomersQueryHandler(IEnterpriseDashboardAggregator aggregator)
    : IRequestHandler<GetEnterpriseDashboardCustomersQuery, Result<EnterpriseDashboardCustomersDto>>
{
    public async Task<Result<EnterpriseDashboardCustomersDto>> Handle(
        GetEnterpriseDashboardCustomersQuery request, CancellationToken cancellationToken)
        => Result<EnterpriseDashboardCustomersDto>.Success(
            await aggregator.GetCustomersAsync(request.TenantId, request.Filter, cancellationToken));
}

public sealed class GetEnterpriseDashboardInventoryQueryHandler(IEnterpriseDashboardAggregator aggregator)
    : IRequestHandler<GetEnterpriseDashboardInventoryQuery, Result<EnterpriseDashboardInventoryDto>>
{
    public async Task<Result<EnterpriseDashboardInventoryDto>> Handle(
        GetEnterpriseDashboardInventoryQuery request, CancellationToken cancellationToken)
        => Result<EnterpriseDashboardInventoryDto>.Success(
            await aggregator.GetInventoryAsync(request.TenantId, request.Filter, cancellationToken));
}

public sealed class GetEnterpriseDashboardFinanceQueryHandler(IEnterpriseDashboardAggregator aggregator)
    : IRequestHandler<GetEnterpriseDashboardFinanceQuery, Result<EnterpriseDashboardFinanceDto>>
{
    public async Task<Result<EnterpriseDashboardFinanceDto>> Handle(
        GetEnterpriseDashboardFinanceQuery request, CancellationToken cancellationToken)
        => Result<EnterpriseDashboardFinanceDto>.Success(
            await aggregator.GetFinanceAsync(request.TenantId, request.Filter, cancellationToken));
}

public sealed class GetEnterpriseDashboardKitchenQueryHandler(IEnterpriseDashboardAggregator aggregator)
    : IRequestHandler<GetEnterpriseDashboardKitchenQuery, Result<EnterpriseDashboardKitchenDto>>
{
    public async Task<Result<EnterpriseDashboardKitchenDto>> Handle(
        GetEnterpriseDashboardKitchenQuery request, CancellationToken cancellationToken)
        => Result<EnterpriseDashboardKitchenDto>.Success(
            await aggregator.GetKitchenAsync(request.TenantId, request.Filter, cancellationToken));
}

public sealed class GetEnterpriseDashboardDeliveryQueryHandler(IEnterpriseDashboardAggregator aggregator)
    : IRequestHandler<GetEnterpriseDashboardDeliveryQuery, Result<EnterpriseDashboardDeliveryDto>>
{
    public async Task<Result<EnterpriseDashboardDeliveryDto>> Handle(
        GetEnterpriseDashboardDeliveryQuery request, CancellationToken cancellationToken)
        => Result<EnterpriseDashboardDeliveryDto>.Success(
            await aggregator.GetDeliveryAsync(request.TenantId, request.Filter, cancellationToken));
}

public sealed class GetEnterpriseDashboardHrQueryHandler(IEnterpriseDashboardAggregator aggregator)
    : IRequestHandler<GetEnterpriseDashboardHrQuery, Result<EnterpriseDashboardHrDto>>
{
    public async Task<Result<EnterpriseDashboardHrDto>> Handle(
        GetEnterpriseDashboardHrQuery request, CancellationToken cancellationToken)
        => Result<EnterpriseDashboardHrDto>.Success(
            await aggregator.GetHrAsync(request.TenantId, request.Filter, cancellationToken));
}

public sealed class GetEnterpriseDashboardActivitiesQueryHandler(IEnterpriseDashboardAggregator aggregator)
    : IRequestHandler<GetEnterpriseDashboardActivitiesQuery, Result<EnterpriseDashboardActivitiesDto>>
{
    public async Task<Result<EnterpriseDashboardActivitiesDto>> Handle(
        GetEnterpriseDashboardActivitiesQuery request, CancellationToken cancellationToken)
        => Result<EnterpriseDashboardActivitiesDto>.Success(
            await aggregator.GetActivitiesAsync(request.TenantId, request.Filter, cancellationToken));
}
