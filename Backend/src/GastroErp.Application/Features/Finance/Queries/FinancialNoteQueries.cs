using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Finance.Queries;

public record GetFinancialNotesQuery(Guid TenantId, FinancialNoteFilterDto Filter)
    : IRequest<PagedResult<FinancialNoteDto>>;

public record GetFinancialNoteByIdQuery(Guid Id) : IRequest<Result<FinancialNoteDto>>;
