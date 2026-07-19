using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Finance.Commands;

public record CreateTaxCodeCommand(Guid TenantId, UpsertTaxCodeDto Dto) : IRequest<Result<TaxCodeDto>>;
public record UpdateTaxCodeCommand(Guid Id, UpsertTaxCodeDto Dto) : IRequest<Result<TaxCodeDto>>;
public record DeleteTaxCodeCommand(Guid Id) : IRequest<Result>;
