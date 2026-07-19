using GastroErp.Application.Common.Responses;
using GastroErp.Domain.Enums;
using MediatR;

namespace GastroErp.Application.Features.Sales.BackOffice.DeliveryNotes;

public record BackOfficeSalesDeliveryNoteLineDto(
    Guid Id,
    Guid OrderLineId,
    Guid? InventoryItemId,
    Guid? UnitId,
    string Description,
    decimal Quantity,
    decimal UnitCost,
    decimal LineCost);

public record BackOfficeSalesDeliveryNoteDto(
    Guid Id,
    string DeliveryNumber,
    BackOfficeSalesDocumentStatus Status,
    Guid CustomerId,
    Guid WarehouseId,
    Guid OrderId,
    Guid? BranchId,
    DateOnly DeliveryDate,
    string? Notes,
    decimal TotalCost,
    Guid? JournalEntryId,
    Guid? ReversalJournalEntryId,
    DateTimeOffset? ApprovedAt,
    DateTimeOffset? PostedAt,
    IReadOnlyList<BackOfficeSalesDeliveryNoteLineDto> Lines);

public record CreateBackOfficeSalesDeliveryNoteLineDto(
    Guid OrderLineId,
    string Description,
    decimal Quantity,
    Guid? InventoryItemId = null,
    Guid? UnitId = null,
    decimal UnitCost = 0);

public record CreateBackOfficeSalesDeliveryNoteDto(
    Guid OrderId,
    Guid CustomerId,
    Guid WarehouseId,
    DateOnly DeliveryDate,
    string? DeliveryNumber = null,
    Guid? BranchId = null,
    string? Notes = null,
    IReadOnlyList<CreateBackOfficeSalesDeliveryNoteLineDto>? Lines = null);

public record UpdateBackOfficeSalesDeliveryNoteDto(
    DateOnly DeliveryDate,
    Guid? WarehouseId = null,
    Guid? BranchId = null,
    string? Notes = null,
    IReadOnlyList<CreateBackOfficeSalesDeliveryNoteLineDto>? Lines = null);

public record CreateBackOfficeSalesDeliveryNoteCommand(Guid TenantId, CreateBackOfficeSalesDeliveryNoteDto Dto)
    : IRequest<Result<BackOfficeSalesDeliveryNoteDto>>;

public record UpdateBackOfficeSalesDeliveryNoteCommand(Guid Id, UpdateBackOfficeSalesDeliveryNoteDto Dto)
    : IRequest<Result<BackOfficeSalesDeliveryNoteDto>>;

public record ApproveBackOfficeSalesDeliveryNoteCommand(Guid Id, Guid UserId) : IRequest<Result>;

public record UnapproveBackOfficeSalesDeliveryNoteCommand(Guid Id) : IRequest<Result>;

public record PostBackOfficeSalesDeliveryNoteCommand(Guid Id, Guid UserId) : IRequest<Result>;

public record UnpostBackOfficeSalesDeliveryNoteCommand(Guid Id, Guid UserId) : IRequest<Result>;

public record CancelBackOfficeSalesDeliveryNoteCommand(Guid Id) : IRequest<Result>;

public record GetBackOfficeSalesDeliveryNotesQuery(
    Guid TenantId,
    BackOfficeSalesDocumentStatus? Status = null,
    Guid? CustomerId = null,
    Guid? OrderId = null,
    Guid? WarehouseId = null,
    Guid? BranchId = null,
    string? Search = null,
    DateOnly? From = null,
    DateOnly? To = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<BackOfficeSalesDeliveryNoteDto>>;

public record GetBackOfficeSalesDeliveryNoteByIdQuery(Guid Id) : IRequest<Result<BackOfficeSalesDeliveryNoteDto>>;
