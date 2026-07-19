using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Finance.Queries;

public record GetGeneralLedgerSettingsQuery(Guid TenantId, GeneralLedgerSettingFilterDto Filter)
    : IRequest<PagedResult<GeneralLedgerSettingDto>>;
public record GetGeneralLedgerSettingByIdQuery(Guid Id) : IRequest<Result<GeneralLedgerSettingDto>>;
