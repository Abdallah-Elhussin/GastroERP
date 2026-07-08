using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.ReportingPlatform.DTOs;
using MediatR;

namespace GastroErp.Application.Features.ReportingPlatform.Queries;

public record GetDashboardQuery(Guid TenantId, Guid Id) : IRequest<Result<DashboardDto>>;
public record GetDashboardsQuery(Guid TenantId, Guid? UserId, ReportingPlatformFilterDto Filter) : IRequest<Result<IReadOnlyList<DashboardDto>>>;

public record GetReportDefinitionQuery(Guid TenantId, Guid Id) : IRequest<Result<ReportDefinitionDto>>;
public record GetReportDefinitionsQuery(Guid TenantId, ReportingPlatformFilterDto Filter) : IRequest<Result<IReadOnlyList<ReportDefinitionDto>>>;
public record GetReportHistoryQuery(Guid TenantId, Guid? ReportDefinitionId, int Take = 50) : IRequest<Result<IReadOnlyList<ReportExecutionHistoryDto>>>;

public record GetKpiDefinitionsQuery(Guid TenantId, ReportingPlatformFilterDto Filter) : IRequest<Result<IReadOnlyList<KpiDefinitionDto>>>;
public record GetKpiHistoryQuery(Guid TenantId, Guid KpiDefinitionId, int Take = 90) : IRequest<Result<IReadOnlyList<KpiHistoryDto>>>;

public record GetScheduledReportQuery(Guid TenantId, Guid Id) : IRequest<Result<ScheduledReportDto>>;
public record GetScheduledReportsQuery(Guid TenantId) : IRequest<Result<IReadOnlyList<ScheduledReportDto>>>;

public record BuildChartQuery(Guid TenantId, ChartRequestDto Request) : IRequest<Result<ChartResultDto>>;
public record GetPowerBiConfigQuery(Guid TenantId) : IRequest<Result<PowerBiWorkspaceConfigDto>>;
public record GetPowerBiEmbedTokenQuery(Guid TenantId, string ReportId) : IRequest<Result<PowerBiEmbedTokenDto>>;
