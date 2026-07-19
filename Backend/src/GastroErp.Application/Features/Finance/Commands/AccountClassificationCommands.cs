using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Finance.Commands;

public record CreateAccountClassificationCommand(Guid TenantId, CreateAccountClassificationDto Dto)
    : IRequest<Result<AccountClassificationDto>>;

public record UpdateAccountClassificationCommand(Guid Id, UpdateAccountClassificationDto Dto)
    : IRequest<Result<AccountClassificationDto>>;

public record DeleteAccountClassificationCommand(Guid Id) : IRequest<Result>;
