using GastroErp.Application.Common.Responses;
using GastroErp.Domain.Enums;
using MediatR;

namespace GastroErp.Application.Features.Sales.BackOffice.Returns;

public record BackOfficeSalesReturnLineDto(
    Guid Id,
    Guid InvoiceLineId,
    Guid? InventoryItemId,
    Guid? UnitId,
    BackOfficeSalesLineNature LineNature,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal UnitCost,
    decimal TaxPercent,
    decimal TaxAmount,
    decimal LineNet,
    decimal LineTotal);

public record BackOfficeSalesReturnDto(
    Guid Id,
    string ReturnNumber,
    BackOfficeSalesDocumentStatus Status,
    Guid CustomerId,
    Guid? WarehouseId,
    Guid InvoiceId,
    Guid? BranchId,
    DateOnly ReturnDate,
    string? Notes,
    decimal DiscountAmount,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    Guid? JournalEntryId,
    Guid? ReversalJournalEntryId,
    DateTimeOffset? ApprovedAt,
    DateTimeOffset? PostedAt,
    IReadOnlyList<BackOfficeSalesReturnLineDto> Lines);

public record CreateBackOfficeSalesReturnLineDto(
    Guid InvoiceLineId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    Guid? InventoryItemId = null,
    Guid? UnitId = null,
    BackOfficeSalesLineNature LineNature = BackOfficeSalesLineNature.Inventory,
    decimal TaxPercent = 0,
    decimal TaxAmount = 0,
    decimal UnitCost = 0);

public record CreateBackOfficeSalesReturnDto(
    Guid InvoiceId,
    Guid CustomerId,
    DateOnly ReturnDate,
    string? ReturnNumber = null,
    Guid? WarehouseId = null,
    Guid? BranchId = null,
    string? Notes = null,
    decimal DiscountAmount = 0,
    IReadOnlyList<CreateBackOfficeSalesReturnLineDto>? Lines = null);

public record UpdateBackOfficeSalesReturnDto(
    DateOnly ReturnDate,
    Guid? WarehouseId = null,
    Guid? BranchId = null,
    string? Notes = null,
    decimal? DiscountAmount = null,
    IReadOnlyList<CreateBackOfficeSalesReturnLineDto>? Lines = null);

public record CreateBackOfficeSalesReturnCommand(Guid TenantId, CreateBackOfficeSalesReturnDto Dto)
    : IRequest<Result<BackOfficeSalesReturnDto>>;

public record UpdateBackOfficeSalesReturnCommand(Guid Id, UpdateBackOfficeSalesReturnDto Dto)
    : IRequest<Result<BackOfficeSalesReturnDto>>;

public record ApproveBackOfficeSalesReturnCommand(Guid Id, Guid UserId) : IRequest<Result>;

public record UnapproveBackOfficeSalesReturnCommand(Guid Id) : IRequest<Result>;

public record PostBackOfficeSalesReturnCommand(Guid Id, Guid UserId) : IRequest<Result>;

public record UnpostBackOfficeSalesReturnCommand(Guid Id, Guid UserId) : IRequest<Result>;

public record CancelBackOfficeSalesReturnCommand(Guid Id) : IRequest<Result>;

public record GetBackOfficeSalesReturnsQuery(
    Guid TenantId,
    BackOfficeSalesDocumentStatus? Status = null,
    Guid? CustomerId = null,
    Guid? InvoiceId = null,
    Guid? BranchId = null,
    string? Search = null,
    DateOnly? From = null,
    DateOnly? To = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<BackOfficeSalesReturnDto>>;

public record GetBackOfficeSalesReturnByIdQuery(Guid Id) : IRequest<Result<BackOfficeSalesReturnDto>>;
