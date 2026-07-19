using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Finance.Commands;

public record CreateCashBoxCommand(Guid TenantId, UpsertCashBoxDto Dto) : IRequest<Result<CashBoxDto>>;
public record UpdateCashBoxCommand(Guid Id, UpsertCashBoxDto Dto) : IRequest<Result<CashBoxDto>>;
public record ActivateCashBoxCommand(Guid Id) : IRequest<Result>;
public record DeactivateCashBoxCommand(Guid Id) : IRequest<Result>;
public record DeleteCashBoxCommand(Guid Id) : IRequest<Result>;
