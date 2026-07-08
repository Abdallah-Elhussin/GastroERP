using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Application.Features.Ai.Queries;
using GastroErp.Application.Features.Ai.Services;
using MediatR;

namespace GastroErp.Application.Features.Ai.Queries;

public sealed class GetDemandForecastQueryHandler : IRequestHandler<GetDemandForecastQuery, Result<DemandForecastResultDto>>
{
    private readonly IDemandForecastService _service;
    public GetDemandForecastQueryHandler(IDemandForecastService service) => _service = service;
    public async Task<Result<DemandForecastResultDto>> Handle(GetDemandForecastQuery request, CancellationToken ct)
        => Result<DemandForecastResultDto>.Success(await _service.ForecastAsync(request.TenantId, request.Filter, ct));
}

public sealed class GetSalesForecastQueryHandler : IRequestHandler<GetSalesForecastQuery, Result<SalesForecastResultDto>>
{
    private readonly ISalesForecastService _service;
    public GetSalesForecastQueryHandler(ISalesForecastService service) => _service = service;
    public async Task<Result<SalesForecastResultDto>> Handle(GetSalesForecastQuery request, CancellationToken ct)
        => Result<SalesForecastResultDto>.Success(await _service.ForecastAsync(request.TenantId, request.Filter, ct));
}

public sealed class GetInventoryForecastQueryHandler : IRequestHandler<GetInventoryForecastQuery, Result<InventoryForecastResultDto>>
{
    private readonly IInventoryForecastService _service;
    public GetInventoryForecastQueryHandler(IInventoryForecastService service) => _service = service;
    public async Task<Result<InventoryForecastResultDto>> Handle(GetInventoryForecastQuery request, CancellationToken ct)
        => Result<InventoryForecastResultDto>.Success(await _service.ForecastAsync(request.TenantId, request.Filter, ct));
}

public sealed class GetPredictionRunsQueryHandler : IRequestHandler<GetPredictionRunsQuery, Result<IReadOnlyList<PredictionRunDto>>>
{
    private readonly IPredictionRunService _service;
    public GetPredictionRunsQueryHandler(IPredictionRunService service) => _service = service;
    public async Task<Result<IReadOnlyList<PredictionRunDto>>> Handle(GetPredictionRunsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<PredictionRunDto>>.Success(
            await _service.GetRecentAsync(request.TenantId, request.Type, request.Take, ct));
}
