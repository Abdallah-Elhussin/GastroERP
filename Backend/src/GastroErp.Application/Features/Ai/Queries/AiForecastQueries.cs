using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Domain.Enums;
using MediatR;

namespace GastroErp.Application.Features.Ai.Queries;

public record GetDemandForecastQuery(Guid TenantId, ForecastFilterDto Filter) : IRequest<Result<DemandForecastResultDto>>;
public record GetSalesForecastQuery(Guid TenantId, ForecastFilterDto Filter) : IRequest<Result<SalesForecastResultDto>>;
public record GetInventoryForecastQuery(Guid TenantId, ForecastFilterDto Filter) : IRequest<Result<InventoryForecastResultDto>>;
public record GetPredictionRunsQuery(Guid TenantId, ForecastType? Type = null, int Take = 50) : IRequest<Result<IReadOnlyList<PredictionRunDto>>>;
