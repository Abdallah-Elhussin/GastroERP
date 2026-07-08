using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Reporting.DTOs;
using GastroErp.Application.Features.ReportingPlatform.Commands;
using GastroErp.Application.Features.ReportingPlatform.DTOs;
using GastroErp.Application.Features.ReportingPlatform.Queries;
using GastroErp.Application.Features.ReportingPlatform.Services;
using MediatR;

namespace GastroErp.Application.Features.ReportingPlatform.Commands;

public sealed class CreateDashboardCommandHandler : IRequestHandler<CreateDashboardCommand, Result<DashboardDto>>
{
    private readonly IDashboardManagementService _service;
    public CreateDashboardCommandHandler(IDashboardManagementService service) => _service = service;
    public async Task<Result<DashboardDto>> Handle(CreateDashboardCommand request, CancellationToken ct)
        => Result<DashboardDto>.Success(await _service.CreateAsync(request.TenantId, request.OwnerUserId, request.Dto, ct));
}

public sealed class UpdateDashboardCommandHandler : IRequestHandler<UpdateDashboardCommand, Result<DashboardDto>>
{
    private readonly IDashboardManagementService _service;
    public UpdateDashboardCommandHandler(IDashboardManagementService service) => _service = service;
    public async Task<Result<DashboardDto>> Handle(UpdateDashboardCommand request, CancellationToken ct)
        => Result<DashboardDto>.Success(await _service.UpdateAsync(request.TenantId, request.Id, request.Dto, ct));
}

public sealed class DeleteDashboardCommandHandler : IRequestHandler<DeleteDashboardCommand, Result>
{
    private readonly IDashboardManagementService _service;
    public DeleteDashboardCommandHandler(IDashboardManagementService service) => _service = service;
    public async Task<Result> Handle(DeleteDashboardCommand request, CancellationToken ct)
    {
        await _service.DeleteAsync(request.TenantId, request.Id, request.DeletedBy, ct);
        return Result.Success();
    }
}

public sealed class ShareDashboardCommandHandler : IRequestHandler<ShareDashboardCommand, Result<DashboardDto>>
{
    private readonly IDashboardManagementService _service;
    public ShareDashboardCommandHandler(IDashboardManagementService service) => _service = service;
    public async Task<Result<DashboardDto>> Handle(ShareDashboardCommand request, CancellationToken ct)
        => Result<DashboardDto>.Success(await _service.ShareAsync(request.TenantId, request.Id, request.Dto, ct));
}

public sealed class SetDashboardFavoriteCommandHandler : IRequestHandler<SetDashboardFavoriteCommand, Result>
{
    private readonly IDashboardManagementService _service;
    public SetDashboardFavoriteCommandHandler(IDashboardManagementService service) => _service = service;
    public async Task<Result> Handle(SetDashboardFavoriteCommand request, CancellationToken ct)
    {
        await _service.SetFavoriteAsync(request.TenantId, request.Id, request.Favorite, ct);
        return Result.Success();
    }
}

public sealed class CreateReportDefinitionCommandHandler : IRequestHandler<CreateReportDefinitionCommand, Result<ReportDefinitionDto>>
{
    private readonly IReportDefinitionService _service;
    public CreateReportDefinitionCommandHandler(IReportDefinitionService service) => _service = service;
    public async Task<Result<ReportDefinitionDto>> Handle(CreateReportDefinitionCommand request, CancellationToken ct)
        => Result<ReportDefinitionDto>.Success(await _service.CreateAsync(request.TenantId, request.Dto, ct));
}

public sealed class UpdateReportDefinitionCommandHandler : IRequestHandler<UpdateReportDefinitionCommand, Result<ReportDefinitionDto>>
{
    private readonly IReportDefinitionService _service;
    public UpdateReportDefinitionCommandHandler(IReportDefinitionService service) => _service = service;
    public async Task<Result<ReportDefinitionDto>> Handle(UpdateReportDefinitionCommand request, CancellationToken ct)
        => Result<ReportDefinitionDto>.Success(await _service.UpdateAsync(request.TenantId, request.Id, request.Dto, ct));
}

public sealed class PublishReportCommandHandler : IRequestHandler<PublishReportCommand, Result<ReportDefinitionDto>>
{
    private readonly IReportDefinitionService _service;
    public PublishReportCommandHandler(IReportDefinitionService service) => _service = service;
    public async Task<Result<ReportDefinitionDto>> Handle(PublishReportCommand request, CancellationToken ct)
        => Result<ReportDefinitionDto>.Success(await _service.PublishAsync(request.TenantId, request.Id, ct));
}

public sealed class ExecuteReportCommandHandler : IRequestHandler<ExecuteReportCommand, Result<ReportExecutionDto>>
{
    private readonly IReportExecutionService _service;
    public ExecuteReportCommandHandler(IReportExecutionService service) => _service = service;
    public async Task<Result<ReportExecutionDto>> Handle(ExecuteReportCommand request, CancellationToken ct)
        => Result<ReportExecutionDto>.Success(await _service.ExecuteAsync(request.TenantId, request.UserId, request.Dto, ct));
}

public sealed class PreviewReportCommandHandler : IRequestHandler<PreviewReportCommand, Result<ReportExecutionDto>>
{
    private readonly IReportExecutionService _service;
    public PreviewReportCommandHandler(IReportExecutionService service) => _service = service;
    public async Task<Result<ReportExecutionDto>> Handle(PreviewReportCommand request, CancellationToken ct)
        => Result<ReportExecutionDto>.Success(await _service.PreviewAsync(request.TenantId, request.UserId, request.Dto, ct));
}

public sealed class CreateScheduledReportCommandHandler : IRequestHandler<CreateScheduledReportCommand, Result<ScheduledReportDto>>
{
    private readonly IScheduledReportService _service;
    public CreateScheduledReportCommandHandler(IScheduledReportService service) => _service = service;
    public async Task<Result<ScheduledReportDto>> Handle(CreateScheduledReportCommand request, CancellationToken ct)
        => Result<ScheduledReportDto>.Success(await _service.CreateAsync(request.TenantId, request.Dto, ct));
}

public sealed class UpdateScheduledReportCommandHandler : IRequestHandler<UpdateScheduledReportCommand, Result<ScheduledReportDto>>
{
    private readonly IScheduledReportService _service;
    public UpdateScheduledReportCommandHandler(IScheduledReportService service) => _service = service;
    public async Task<Result<ScheduledReportDto>> Handle(UpdateScheduledReportCommand request, CancellationToken ct)
        => Result<ScheduledReportDto>.Success(await _service.UpdateAsync(request.TenantId, request.Id, request.Dto, ct));
}

public sealed class DeleteScheduledReportCommandHandler : IRequestHandler<DeleteScheduledReportCommand, Result>
{
    private readonly IScheduledReportService _service;
    public DeleteScheduledReportCommandHandler(IScheduledReportService service) => _service = service;
    public async Task<Result> Handle(DeleteScheduledReportCommand request, CancellationToken ct)
    {
        await _service.DeleteAsync(request.TenantId, request.Id, request.DeletedBy, ct);
        return Result.Success();
    }
}

public sealed class EnableScheduledReportCommandHandler : IRequestHandler<EnableScheduledReportCommand, Result>
{
    private readonly IScheduledReportService _service;
    public EnableScheduledReportCommandHandler(IScheduledReportService service) => _service = service;
    public async Task<Result> Handle(EnableScheduledReportCommand request, CancellationToken ct)
    {
        await _service.EnableAsync(request.TenantId, request.Id, ct);
        return Result.Success();
    }
}

public sealed class DisableScheduledReportCommandHandler : IRequestHandler<DisableScheduledReportCommand, Result>
{
    private readonly IScheduledReportService _service;
    public DisableScheduledReportCommandHandler(IScheduledReportService service) => _service = service;
    public async Task<Result> Handle(DisableScheduledReportCommand request, CancellationToken ct)
    {
        await _service.DisableAsync(request.TenantId, request.Id, ct);
        return Result.Success();
    }
}

public sealed class ExecuteScheduledReportNowCommandHandler : IRequestHandler<ExecuteScheduledReportNowCommand, Result>
{
    private readonly IScheduledReportService _service;
    public ExecuteScheduledReportNowCommandHandler(IScheduledReportService service) => _service = service;
    public async Task<Result> Handle(ExecuteScheduledReportNowCommand request, CancellationToken ct)
    {
        await _service.ExecuteNowAsync(request.TenantId, request.Id, request.UserId, ct);
        return Result.Success();
    }
}

public sealed class CreateKpiDefinitionCommandHandler : IRequestHandler<CreateKpiDefinitionCommand, Result<KpiDefinitionDto>>
{
    private readonly IKpiAnalyticsEngine _engine;
    public CreateKpiDefinitionCommandHandler(IKpiAnalyticsEngine engine) => _engine = engine;
    public async Task<Result<KpiDefinitionDto>> Handle(CreateKpiDefinitionCommand request, CancellationToken ct)
        => Result<KpiDefinitionDto>.Success(await _engine.CreateAsync(request.TenantId, request.Dto, ct));
}

public sealed class UpdateKpiDefinitionCommandHandler : IRequestHandler<UpdateKpiDefinitionCommand, Result<KpiDefinitionDto>>
{
    private readonly IKpiAnalyticsEngine _engine;
    public UpdateKpiDefinitionCommandHandler(IKpiAnalyticsEngine engine) => _engine = engine;
    public async Task<Result<KpiDefinitionDto>> Handle(UpdateKpiDefinitionCommand request, CancellationToken ct)
        => Result<KpiDefinitionDto>.Success(await _engine.UpdateAsync(request.TenantId, request.Id, request.Dto, ct));
}

public sealed class CalculateKpiCommandHandler : IRequestHandler<CalculateKpiCommand, Result<KpiValueDto>>
{
    private readonly IKpiAnalyticsEngine _engine;
    public CalculateKpiCommandHandler(IKpiAnalyticsEngine engine) => _engine = engine;
    public async Task<Result<KpiValueDto>> Handle(CalculateKpiCommand request, CancellationToken ct)
    {
        var filter = new ReportFilterDto(request.FromDate, request.ToDate, request.BranchId);
        return Result<KpiValueDto>.Success(await _engine.CalculateAsync(request.TenantId, request.KpiDefinitionId, filter, ct));
    }
}

public sealed class ExportReportPlatformCommandHandler : IRequestHandler<ExportReportPlatformCommand, Result<PlatformExportResultDto>>
{
    private readonly IPlatformExportService _export;
    public ExportReportPlatformCommandHandler(IPlatformExportService export) => _export = export;
    public async Task<Result<PlatformExportResultDto>> Handle(ExportReportPlatformCommand request, CancellationToken ct)
        => Result<PlatformExportResultDto>.Success(await _export.ExportAsync(request.TenantId, request.Dto, ct));
}
