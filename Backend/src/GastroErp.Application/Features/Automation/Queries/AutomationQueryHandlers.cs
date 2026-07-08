using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Automation.Commands;
using GastroErp.Application.Features.Automation.DTOs;
using GastroErp.Application.Features.Automation.Queries;
using GastroErp.Application.Features.Automation.Services;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Automation.Queries;

public sealed class GetJobMonitoringQueryHandler : IRequestHandler<GetJobMonitoringQuery, Result<JobMonitoringDto>>
{
    private readonly IJobMonitoringService _service;
    public GetJobMonitoringQueryHandler(IJobMonitoringService service) => _service = service;

    public async Task<Result<JobMonitoringDto>> Handle(GetJobMonitoringQuery request, CancellationToken cancellationToken)
        => Result<JobMonitoringDto>.Success(await _service.GetStatusAsync(request.TenantId, cancellationToken));
}

public sealed class GetJobHistoryQueryHandler : IRequestHandler<GetJobHistoryQuery, Result<IReadOnlyList<JobHistoryDto>>>
{
    private readonly IJobHistoryService _service;
    public GetJobHistoryQueryHandler(IJobHistoryService service) => _service = service;

    public async Task<Result<IReadOnlyList<JobHistoryDto>>> Handle(GetJobHistoryQuery request, CancellationToken cancellationToken)
        => Result<IReadOnlyList<JobHistoryDto>>.Success(await _service.GetHistoryAsync(request.TenantId, request.Take, cancellationToken));
}

public sealed class GetJobsQueryHandler : IRequestHandler<GetJobsQuery, Result<IReadOnlyList<JobDto>>>
{
    private readonly IApplicationDbContext _context;
    public GetJobsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<IReadOnlyList<JobDto>>> Handle(GetJobsQuery request, CancellationToken cancellationToken)
    {
        var jobs = await _context.JobExecutionLogs.AsNoTracking()
            .Where(j => j.TenantId == request.TenantId)
            .OrderByDescending(j => j.CreatedAt)
            .Take(request.Take)
            .Select(j => new JobDto(
                j.Id, j.JobName, j.Queue, j.Status, j.ExternalJobId,
                j.StartedAt, j.FinishedAt, j.RetryCount, j.ErrorMessage))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<JobDto>>.Success(jobs);
    }
}

public sealed class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, Result<IReadOnlyList<NotificationDto>>>
{
    private readonly INotificationInboxService _inbox;
    public GetUserNotificationsQueryHandler(INotificationInboxService inbox) => _inbox = inbox;

    public async Task<Result<IReadOnlyList<NotificationDto>>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
        => Result<IReadOnlyList<NotificationDto>>.Success(
            await _inbox.GetUserNotificationsAsync(request.TenantId, request.UserId, request.Filter, cancellationToken));
}

public sealed class GetIntegrationsQueryHandler : IRequestHandler<GetIntegrationsQuery, Result<IReadOnlyList<IntegrationDto>>>
{
    private readonly IIntegrationRegistryService _registry;
    public GetIntegrationsQueryHandler(IIntegrationRegistryService registry) => _registry = registry;

    public async Task<Result<IReadOnlyList<IntegrationDto>>> Handle(GetIntegrationsQuery request, CancellationToken cancellationToken)
        => Result<IReadOnlyList<IntegrationDto>>.Success(await _registry.GetAllAsync(request.TenantId, cancellationToken));
}
