using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Automation.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Automation.Commands;

public record ExecuteJobCommand(Guid TenantId, ExecuteJobDto Dto) : IRequest<Result<JobDto>>;
public record RetryJobCommand(Guid TenantId, Guid JobLogId) : IRequest<Result<JobDto>>;
public record CancelJobCommand(Guid TenantId, Guid JobLogId) : IRequest<Result>;

public record SendNotificationCommand(Guid TenantId, SendNotificationDto Dto) : IRequest<Result<NotificationDto>>;
public record ResendNotificationCommand(Guid TenantId, Guid NotificationId) : IRequest<Result<NotificationDto>>;
public record MarkNotificationReadCommand(Guid NotificationId) : IRequest<Result>;
public record ArchiveNotificationCommand(Guid NotificationId) : IRequest<Result>;

public record UpsertIntegrationCommand(Guid TenantId, UpsertIntegrationDto Dto) : IRequest<Result<IntegrationDto>>;
public record TestIntegrationCommand(Guid TenantId, TestIntegrationDto Dto) : IRequest<Result<IntegrationStatusDto>>;
public record ProcessInboundWebhookCommand(Guid TenantId, InboundWebhookDto Dto) : IRequest<Result>;
