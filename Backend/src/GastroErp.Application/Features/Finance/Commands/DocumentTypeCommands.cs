using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Finance.Commands;

public record CreateDocumentTypeCommand(Guid TenantId, UpsertDocumentTypeDto Dto) : IRequest<Result<DocumentTypeDto>>;
public record UpdateDocumentTypeCommand(Guid Id, UpsertDocumentTypeDto Dto) : IRequest<Result<DocumentTypeDto>>;
public record ActivateDocumentTypeCommand(Guid Id) : IRequest<Result>;
public record DeactivateDocumentTypeCommand(Guid Id) : IRequest<Result>;
public record DeleteDocumentTypeCommand(Guid Id) : IRequest<Result>;
public record CopyDocumentTypeCommand(Guid Id, CopyDocumentTypeDto Dto) : IRequest<Result<DocumentTypeDto>>;
