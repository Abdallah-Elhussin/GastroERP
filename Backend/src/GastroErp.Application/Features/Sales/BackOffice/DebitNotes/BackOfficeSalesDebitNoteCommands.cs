using GastroErp.Application.Common.Responses;
using GastroErp.Domain.Enums;
using MediatR;

namespace GastroErp.Application.Features.Sales.BackOffice.DebitNotes;

public record BackOfficeSalesDebitNoteLineDto(
    Guid Id,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxPercent,
    decimal TaxAmount,
    decimal LineNet,
    decimal LineTotal);

public record BackOfficeSalesDebitNoteDto(
    Guid Id,
    string DebitNoteNumber,
    BackOfficeSalesDocumentStatus Status,
    Guid CustomerId,
    Guid? InvoiceId,
    Guid? BranchId,
    DateOnly DebitDate,
    string Currency,
    string? Notes,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    Guid? JournalEntryId,
    Guid? ReversalJournalEntryId,
    DateTimeOffset? ApprovedAt,
    DateTimeOffset? PostedAt,
    IReadOnlyList<BackOfficeSalesDebitNoteLineDto> Lines);

public record CreateBackOfficeSalesDebitNoteLineDto(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxPercent = 0,
    decimal TaxAmount = 0);

public record CreateBackOfficeSalesDebitNoteDto(
    Guid CustomerId,
    DateOnly DebitDate,
    string? DebitNoteNumber = null,
    string Currency = "SAR",
    Guid? InvoiceId = null,
    Guid? BranchId = null,
    string? Notes = null,
    IReadOnlyList<CreateBackOfficeSalesDebitNoteLineDto>? Lines = null);

public record UpdateBackOfficeSalesDebitNoteDto(
    DateOnly DebitDate,
    Guid? InvoiceId = null,
    Guid? BranchId = null,
    string? Notes = null,
    IReadOnlyList<CreateBackOfficeSalesDebitNoteLineDto>? Lines = null);

public record CreateBackOfficeSalesDebitNoteCommand(Guid TenantId, CreateBackOfficeSalesDebitNoteDto Dto)
    : IRequest<Result<BackOfficeSalesDebitNoteDto>>;

public record UpdateBackOfficeSalesDebitNoteCommand(Guid Id, UpdateBackOfficeSalesDebitNoteDto Dto)
    : IRequest<Result<BackOfficeSalesDebitNoteDto>>;

public record ApproveBackOfficeSalesDebitNoteCommand(Guid Id, Guid UserId) : IRequest<Result>;

public record UnapproveBackOfficeSalesDebitNoteCommand(Guid Id) : IRequest<Result>;

public record PostBackOfficeSalesDebitNoteCommand(Guid Id, Guid UserId) : IRequest<Result>;

public record UnpostBackOfficeSalesDebitNoteCommand(Guid Id, Guid UserId) : IRequest<Result>;

public record CancelBackOfficeSalesDebitNoteCommand(Guid Id) : IRequest<Result>;

public record GetBackOfficeSalesDebitNotesQuery(
    Guid TenantId,
    BackOfficeSalesDocumentStatus? Status = null,
    Guid? CustomerId = null,
    Guid? InvoiceId = null,
    Guid? BranchId = null,
    string? Search = null,
    DateOnly? From = null,
    DateOnly? To = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<BackOfficeSalesDebitNoteDto>>;

public record GetBackOfficeSalesDebitNoteByIdQuery(Guid Id) : IRequest<Result<BackOfficeSalesDebitNoteDto>>;
