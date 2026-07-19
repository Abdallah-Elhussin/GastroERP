using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Finance.Queries;

public record GetNotificationReasonsQuery(Guid TenantId, NotificationReasonFilterDto Filter)
    : IRequest<PagedResult<NotificationReasonDto>>;

public record GetNotificationReasonByIdQuery(Guid Id) : IRequest<Result<NotificationReasonDto>>;
