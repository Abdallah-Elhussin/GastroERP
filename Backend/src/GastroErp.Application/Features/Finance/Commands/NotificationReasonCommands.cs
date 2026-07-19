using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Finance.Commands;

public record CreateNotificationReasonCommand(Guid TenantId, UpsertNotificationReasonDto Dto)
    : IRequest<Result<NotificationReasonDto>>;

public record UpdateNotificationReasonCommand(Guid Id, UpsertNotificationReasonDto Dto)
    : IRequest<Result<NotificationReasonDto>>;

public record DeleteNotificationReasonCommand(Guid Id) : IRequest<Result>;
