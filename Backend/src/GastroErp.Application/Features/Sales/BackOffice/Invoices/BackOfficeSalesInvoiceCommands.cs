using GastroErp.Application.Common.Responses;
using GastroErp.Domain.Enums;
using MediatR;

namespace GastroErp.Application.Features.Sales.BackOffice.Invoices;

public record BackOfficeSalesInvoiceLineDto(
    Guid Id,
    Guid? InventoryItemId,
    Guid? ProductId,
    Guid? UnitId,
    Guid? LineWarehouseId,
    Guid? CostCenterId,
    Guid? SalesOrderLineId,
    BackOfficeSalesLineNature LineNature,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal UnitCost,
    decimal DiscountPercent,
    decimal DiscountAmount,
    decimal TaxPercent,
    decimal TaxAmount,
    decimal LineNet,
    decimal LineTotal,
    decimal ReturnedQuantity,
    decimal RemainingToReturn);

public record BackOfficeSalesInvoiceDto(
    Guid Id,
    string InvoiceNumber,
    BackOfficeSalesInvoiceNature Nature,
    BackOfficeSalesPaymentMode PaymentMode,
    BackOfficeSalesDocumentStatus Status,
    Guid CustomerId,
    Guid? BranchId,
    Guid? WarehouseId,
    Guid? CostCenterId,
    Guid? SalesPersonId,
    Guid? BackOfficeSalesOrderId,
    DateOnly InvoiceDate,
    DateOnly? DueDate,
    string Currency,
    decimal ExchangeRate,
    string? ExternalReference,
    string? Notes,
    decimal DiscountAmount,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal RemainingAmount,
    BackOfficeSalesPaymentStatus PaymentStatus,
    Guid? JournalEntryId,
    Guid? CogsJournalEntryId,
    Guid? ReversalJournalEntryId,
    DateTimeOffset? ApprovedAt,
    DateTimeOffset? PostedAt,
    IReadOnlyList<BackOfficeSalesInvoiceLineDto> Lines);

public record CreateBackOfficeSalesInvoiceLineDto(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    BackOfficeSalesLineNature LineNature = BackOfficeSalesLineNature.Inventory,
    Guid? InventoryItemId = null,
    Guid? ProductId = null,
    Guid? UnitId = null,
    Guid? LineWarehouseId = null,
    Guid? CostCenterId = null,
    decimal DiscountPercent = 0,
    decimal DiscountAmount = 0,
    decimal TaxPercent = 0,
    decimal TaxAmount = 0,
    decimal? UnitCost = null,
    Guid? SalesOrderLineId = null);

public record CreateBackOfficeSalesInvoiceDto(
    Guid CustomerId,
    DateOnly InvoiceDate,
    BackOfficeSalesPaymentMode PaymentMode,
    BackOfficeSalesInvoiceNature Nature = BackOfficeSalesInvoiceNature.Inventory,
    string? InvoiceNumber = null,
    string Currency = "SAR",
    Guid? BranchId = null,
    Guid? WarehouseId = null,
    Guid? CostCenterId = null,
    Guid? SalesPersonId = null,
    DateOnly? DueDate = null,
    decimal ExchangeRate = 1m,
    string? ExternalReference = null,
    string? Notes = null,
    decimal DiscountAmount = 0,
    IReadOnlyList<CreateBackOfficeSalesInvoiceLineDto>? Lines = null);

public record UpdateBackOfficeSalesInvoiceDto(
    DateOnly InvoiceDate,
    BackOfficeSalesPaymentMode PaymentMode,
    BackOfficeSalesInvoiceNature? Nature = null,
    DateOnly? DueDate = null,
    Guid? WarehouseId = null,
    Guid? CostCenterId = null,
    Guid? SalesPersonId = null,
    Guid? BranchId = null,
    decimal? ExchangeRate = null,
    string? ExternalReference = null,
    string? Notes = null,
    decimal? DiscountAmount = null,
    IReadOnlyList<CreateBackOfficeSalesInvoiceLineDto>? Lines = null);

public record CreateBackOfficeSalesInvoiceCommand(Guid TenantId, CreateBackOfficeSalesInvoiceDto Dto)
    : IRequest<Result<BackOfficeSalesInvoiceDto>>;

public record UpdateBackOfficeSalesInvoiceCommand(Guid Id, UpdateBackOfficeSalesInvoiceDto Dto)
    : IRequest<Result<BackOfficeSalesInvoiceDto>>;

public record ApproveBackOfficeSalesInvoiceCommand(Guid Id, Guid UserId) : IRequest<Result>;

public record UnapproveBackOfficeSalesInvoiceCommand(Guid Id) : IRequest<Result>;

public record PostBackOfficeSalesInvoiceCommand(Guid Id, Guid UserId) : IRequest<Result>;

public record UnpostBackOfficeSalesInvoiceCommand(Guid Id, Guid UserId) : IRequest<Result>;

public record CancelBackOfficeSalesInvoiceCommand(Guid Id) : IRequest<Result>;

public record GetBackOfficeSalesInvoicesQuery(
    Guid TenantId,
    BackOfficeSalesDocumentStatus? Status = null,
    Guid? CustomerId = null,
    Guid? BranchId = null,
    BackOfficeSalesInvoiceNature? Nature = null,
    BackOfficeSalesPaymentMode? PaymentMode = null,
    string? Search = null,
    DateOnly? From = null,
    DateOnly? To = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<BackOfficeSalesInvoiceDto>>;

public record GetBackOfficeSalesInvoiceByIdQuery(Guid Id) : IRequest<Result<BackOfficeSalesInvoiceDto>>;
