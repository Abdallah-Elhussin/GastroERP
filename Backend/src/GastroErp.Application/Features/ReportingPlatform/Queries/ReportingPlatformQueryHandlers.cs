using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.ReportingPlatform.DTOs;
using GastroErp.Application.Features.ReportingPlatform.Queries;
using GastroErp.Application.Features.ReportingPlatform.Services;
using MediatR;

namespace GastroErp.Application.Features.ReportingPlatform.Queries;

public sealed class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, Result<DashboardDto>>
{
    private readonly IDashboardManagementService _service;
    public GetDashboardQueryHandler(IDashboardManagementService service) => _service = service;
    public async Task<Result<DashboardDto>> Handle(GetDashboardQuery request, CancellationToken ct)
        => Result<DashboardDto>.Success(await _service.GetByIdAsync(request.TenantId, request.Id, ct));
}

public sealed class GetDashboardsQueryHandler : IRequestHandler<GetDashboardsQuery, Result<IReadOnlyList<DashboardDto>>>
{
    private readonly IDashboardManagementService _service;
    public GetDashboardsQueryHandler(IDashboardManagementService service) => _service = service;
    public async Task<Result<IReadOnlyList<DashboardDto>>> Handle(GetDashboardsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<DashboardDto>>.Success(await _service.GetListAsync(request.TenantId, request.UserId, request.Filter, ct));
}

public sealed class GetReportDefinitionQueryHandler : IRequestHandler<GetReportDefinitionQuery, Result<ReportDefinitionDto>>
{
    private readonly IReportDefinitionService _service;
    public GetReportDefinitionQueryHandler(IReportDefinitionService service) => _service = service;
    public async Task<Result<ReportDefinitionDto>> Handle(GetReportDefinitionQuery request, CancellationToken ct)
        => Result<ReportDefinitionDto>.Success(await _service.GetByIdAsync(request.TenantId, request.Id, ct));
}

public sealed class GetReportDefinitionsQueryHandler : IRequestHandler<GetReportDefinitionsQuery, Result<IReadOnlyList<ReportDefinitionDto>>>
{
    private readonly IReportDefinitionService _service;
    public GetReportDefinitionsQueryHandler(IReportDefinitionService service) => _service = service;
    public async Task<Result<IReadOnlyList<ReportDefinitionDto>>> Handle(GetReportDefinitionsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<ReportDefinitionDto>>.Success(await _service.GetListAsync(request.TenantId, request.Filter, ct));
}

public sealed class GetReportHistoryQueryHandler : IRequestHandler<GetReportHistoryQuery, Result<IReadOnlyList<ReportExecutionHistoryDto>>>
{
    private readonly IReportExecutionService _service;
    public GetReportHistoryQueryHandler(IReportExecutionService service) => _service = service;
    public async Task<Result<IReadOnlyList<ReportExecutionHistoryDto>>> Handle(GetReportHistoryQuery request, CancellationToken ct)
        => Result<IReadOnlyList<ReportExecutionHistoryDto>>.Success(await _service.GetHistoryAsync(request.TenantId, request.ReportDefinitionId, request.Take, ct));
}

public sealed class GetKpiDefinitionsQueryHandler : IRequestHandler<GetKpiDefinitionsQuery, Result<IReadOnlyList<KpiDefinitionDto>>>
{
    private readonly IKpiAnalyticsEngine _engine;
    public GetKpiDefinitionsQueryHandler(IKpiAnalyticsEngine engine) => _engine = engine;
    public async Task<Result<IReadOnlyList<KpiDefinitionDto>>> Handle(GetKpiDefinitionsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<KpiDefinitionDto>>.Success(await _engine.GetListAsync(request.TenantId, request.Filter, ct));
}

public sealed class GetKpiHistoryQueryHandler : IRequestHandler<GetKpiHistoryQuery, Result<IReadOnlyList<KpiHistoryDto>>>
{
    private readonly IKpiAnalyticsEngine _engine;
    public GetKpiHistoryQueryHandler(IKpiAnalyticsEngine engine) => _engine = engine;
    public async Task<Result<IReadOnlyList<KpiHistoryDto>>> Handle(GetKpiHistoryQuery request, CancellationToken ct)
        => Result<IReadOnlyList<KpiHistoryDto>>.Success(await _engine.GetHistoryAsync(request.TenantId, request.KpiDefinitionId, request.Take, ct));
}

public sealed class GetScheduledReportQueryHandler : IRequestHandler<GetScheduledReportQuery, Result<ScheduledReportDto>>
{
    private readonly IScheduledReportService _service;
    public GetScheduledReportQueryHandler(IScheduledReportService service) => _service = service;
    public async Task<Result<ScheduledReportDto>> Handle(GetScheduledReportQuery request, CancellationToken ct)
        => Result<ScheduledReportDto>.Success(await _service.GetByIdAsync(request.TenantId, request.Id, ct));
}

public sealed class GetScheduledReportsQueryHandler : IRequestHandler<GetScheduledReportsQuery, Result<IReadOnlyList<ScheduledReportDto>>>
{
    private readonly IScheduledReportService _service;
    public GetScheduledReportsQueryHandler(IScheduledReportService service) => _service = service;
    public async Task<Result<IReadOnlyList<ScheduledReportDto>>> Handle(GetScheduledReportsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<ScheduledReportDto>>.Success(await _service.GetListAsync(request.TenantId, ct));
}

public sealed class BuildChartQueryHandler : IRequestHandler<BuildChartQuery, Result<ChartResultDto>>
{
    private readonly IChartService _charts;
    public BuildChartQueryHandler(IChartService charts) => _charts = charts;
    public async Task<Result<ChartResultDto>> Handle(BuildChartQuery request, CancellationToken ct)
        => Result<ChartResultDto>.Success(await _charts.BuildChartAsync(request.TenantId, request.Request, ct));
}

public sealed class GetPowerBiConfigQueryHandler : IRequestHandler<GetPowerBiConfigQuery, Result<PowerBiWorkspaceConfigDto>>
{
    private readonly IPowerBiIntegrationService _powerBi;
    public GetPowerBiConfigQueryHandler(IPowerBiIntegrationService powerBi) => _powerBi = powerBi;
    public async Task<Result<PowerBiWorkspaceConfigDto>> Handle(GetPowerBiConfigQuery request, CancellationToken ct)
        => Result<PowerBiWorkspaceConfigDto>.Success(await _powerBi.GetWorkspaceConfigAsync(request.TenantId, ct));
}

public sealed class GetPowerBiEmbedTokenQueryHandler : IRequestHandler<GetPowerBiEmbedTokenQuery, Result<PowerBiEmbedTokenDto>>
{
    private readonly IPowerBiIntegrationService _powerBi;
    public GetPowerBiEmbedTokenQueryHandler(IPowerBiIntegrationService powerBi) => _powerBi = powerBi;
    public async Task<Result<PowerBiEmbedTokenDto>> Handle(GetPowerBiEmbedTokenQuery request, CancellationToken ct)
        => Result<PowerBiEmbedTokenDto>.Success(await _powerBi.GetEmbedTokenAsync(request.TenantId, request.ReportId, ct));
}
