using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Finance.Commands;

public record CreateFinancialNoteCommand(Guid TenantId, UpsertFinancialNoteDto Dto)
    : IRequest<Result<FinancialNoteDto>>;

public record UpdateFinancialNoteCommand(Guid Id, UpsertFinancialNoteDto Dto)
    : IRequest<Result<FinancialNoteDto>>;

public record SubmitFinancialNoteCommand(Guid Id) : IRequest<Result<FinancialNoteDto>>;

public record ApproveFinancialNoteCommand(Guid Id, Guid UserId) : IRequest<Result<FinancialNoteDto>>;

public record PostFinancialNoteCommand(Guid Id, Guid UserId) : IRequest<Result<FinancialNoteDto>>;

public record ReverseFinancialNoteCommand(Guid Id, Guid UserId) : IRequest<Result<FinancialNoteDto>>;

public record CancelFinancialNoteCommand(Guid Id, Guid UserId) : IRequest<Result<FinancialNoteDto>>;

public record DeleteFinancialNoteCommand(Guid Id) : IRequest<Result>;
