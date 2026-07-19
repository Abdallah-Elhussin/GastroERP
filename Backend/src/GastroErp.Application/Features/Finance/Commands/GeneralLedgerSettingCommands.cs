using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Finance.Commands;

public record CreateGeneralLedgerSettingCommand(Guid TenantId, UpsertGeneralLedgerSettingDto Dto)
    : IRequest<Result<GeneralLedgerSettingDto>>;
public record UpdateGeneralLedgerSettingCommand(Guid Id, UpsertGeneralLedgerSettingDto Dto)
    : IRequest<Result<GeneralLedgerSettingDto>>;
public record DeleteGeneralLedgerSettingCommand(Guid Id) : IRequest<Result>;
