using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Finance.Queries;

public record GetDocumentTypesQuery(Guid TenantId, DocumentTypeFilterDto Filter) : IRequest<PagedResult<DocumentTypeDto>>;
public record GetDocumentTypeByIdQuery(Guid Id) : IRequest<Result<DocumentTypeDto>>;
