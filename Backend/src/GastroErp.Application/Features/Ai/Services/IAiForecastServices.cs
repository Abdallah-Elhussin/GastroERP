using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Ai.Services;

public interface IDemandForecastService
{
    Task<DemandForecastResultDto> ForecastAsync(Guid tenantId, ForecastFilterDto filter, CancellationToken ct = default);
}

public interface ISalesForecastService
{
    Task<SalesForecastResultDto> ForecastAsync(Guid tenantId, ForecastFilterDto filter, CancellationToken ct = default);
}

public interface IInventoryForecastService
{
    Task<InventoryForecastResultDto> ForecastAsync(Guid tenantId, ForecastFilterDto filter, CancellationToken ct = default);
}

public interface IAiForecastOrchestrator
{
    Task<RefreshForecastsResultDto> RefreshAllAsync(Guid tenantId, RefreshForecastsDto options, CancellationToken ct = default);
}

public interface IPredictionRunService
{
    Task<IReadOnlyList<PredictionRunDto>> GetRecentAsync(Guid tenantId, ForecastType? type = null, int take = 50, CancellationToken ct = default);
}
