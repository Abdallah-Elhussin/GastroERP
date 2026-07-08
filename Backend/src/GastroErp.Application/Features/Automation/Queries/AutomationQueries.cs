using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Automation.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Automation.Queries;

public record GetJobMonitoringQuery(Guid TenantId) : IRequest<Result<JobMonitoringDto>>;
public record GetJobHistoryQuery(Guid TenantId, int Take = 50) : IRequest<Result<IReadOnlyList<JobHistoryDto>>>;
public record GetJobsQuery(Guid TenantId, int Take = 50) : IRequest<Result<IReadOnlyList<JobDto>>>;

public record GetUserNotificationsQuery(Guid TenantId, Guid UserId, NotificationFilterDto Filter)
    : IRequest<Result<IReadOnlyList<NotificationDto>>>;

public record GetIntegrationsQuery(Guid TenantId) : IRequest<Result<IReadOnlyList<IntegrationDto>>>;
