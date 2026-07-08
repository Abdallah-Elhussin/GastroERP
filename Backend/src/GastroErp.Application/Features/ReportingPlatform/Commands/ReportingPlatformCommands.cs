using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.ReportingPlatform.DTOs;
using MediatR;

namespace GastroErp.Application.Features.ReportingPlatform.Commands;

public record CreateDashboardCommand(Guid TenantId, Guid? OwnerUserId, CreateDashboardDto Dto) : IRequest<Result<DashboardDto>>;
public record UpdateDashboardCommand(Guid TenantId, Guid Id, UpdateDashboardDto Dto) : IRequest<Result<DashboardDto>>;
public record DeleteDashboardCommand(Guid TenantId, Guid Id, string? DeletedBy) : IRequest<Result>;
public record ShareDashboardCommand(Guid TenantId, Guid Id, ShareDashboardDto Dto) : IRequest<Result<DashboardDto>>;
public record SetDashboardFavoriteCommand(Guid TenantId, Guid Id, bool Favorite) : IRequest<Result>;

public record CreateReportDefinitionCommand(Guid TenantId, CreateReportDefinitionDto Dto) : IRequest<Result<ReportDefinitionDto>>;
public record UpdateReportDefinitionCommand(Guid TenantId, Guid Id, UpdateReportDefinitionDto Dto) : IRequest<Result<ReportDefinitionDto>>;
public record PublishReportCommand(Guid TenantId, Guid Id) : IRequest<Result<ReportDefinitionDto>>;
public record ExecuteReportCommand(Guid TenantId, Guid UserId, ExecuteReportDto Dto) : IRequest<Result<ReportExecutionDto>>;
public record PreviewReportCommand(Guid TenantId, Guid UserId, ExecuteReportDto Dto) : IRequest<Result<ReportExecutionDto>>;

public record CreateScheduledReportCommand(Guid TenantId, CreateScheduledReportDto Dto) : IRequest<Result<ScheduledReportDto>>;
public record UpdateScheduledReportCommand(Guid TenantId, Guid Id, UpdateScheduledReportDto Dto) : IRequest<Result<ScheduledReportDto>>;
public record DeleteScheduledReportCommand(Guid TenantId, Guid Id, string? DeletedBy) : IRequest<Result>;
public record EnableScheduledReportCommand(Guid TenantId, Guid Id) : IRequest<Result>;
public record DisableScheduledReportCommand(Guid TenantId, Guid Id) : IRequest<Result>;
public record ExecuteScheduledReportNowCommand(Guid TenantId, Guid Id, Guid UserId) : IRequest<Result>;

public record CreateKpiDefinitionCommand(Guid TenantId, CreateKpiDefinitionDto Dto) : IRequest<Result<KpiDefinitionDto>>;
public record UpdateKpiDefinitionCommand(Guid TenantId, Guid Id, UpdateKpiDefinitionDto Dto) : IRequest<Result<KpiDefinitionDto>>;
public record CalculateKpiCommand(Guid TenantId, Guid KpiDefinitionId, DateOnly? FromDate, DateOnly? ToDate, Guid? BranchId) : IRequest<Result<KpiValueDto>>;

public record ExportReportPlatformCommand(Guid TenantId, PlatformExportRequestDto Dto) : IRequest<Result<PlatformExportResultDto>>;
