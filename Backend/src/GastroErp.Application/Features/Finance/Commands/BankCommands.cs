using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Finance.Commands;

public record CreateBankCommand(Guid TenantId, UpsertBankDto Dto) : IRequest<Result<BankDto>>;
public record UpdateBankCommand(Guid Id, UpsertBankDto Dto) : IRequest<Result<BankDto>>;
public record ActivateBankCommand(Guid Id) : IRequest<Result>;
public record DeactivateBankCommand(Guid Id, DateOnly? DeactivatedAt, string? Reason) : IRequest<Result>;
public record DeleteBankCommand(Guid Id) : IRequest<Result>;
