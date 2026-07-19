using GastroErp.Application.Common.Responses;
using GastroErp.Domain.Enums;
using MediatR;

namespace GastroErp.Application.Features.Sales.BackOffice.Orders;

public record BackOfficeSalesOrderLineDto(
    Guid Id,
    Guid? InventoryItemId,
    Guid? UnitId,
    BackOfficeSalesLineNature LineNature,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal UnitCost,
    decimal DiscountAmount,
    decimal TaxPercent,
    decimal TaxAmount,
    decimal LineNet,
    decimal DeliveredQuantity,
    decimal InvoicedQuantity,
    decimal RemainingToDeliver,
    decimal RemainingToInvoice);

public record BackOfficeSalesOrderDto(
    Guid Id,
    string OrderNumber,
    BackOfficeSalesDocumentStatus Status,
    BackOfficeSalesFulfillmentStatus FulfillmentStatus,
    Guid CustomerId,
    Guid? BranchId,
    Guid? WarehouseId,
    Guid? SalesPersonId,
    Guid? QuotationId,
    DateOnly OrderDate,
    DateOnly? ExpectedDeliveryDate,
    string Currency,
    decimal ExchangeRate,
    string? Notes,
    decimal DiscountAmount,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    DateTimeOffset? ApprovedAt,
    IReadOnlyList<BackOfficeSalesOrderLineDto> Lines);

public record CreateBackOfficeSalesOrderLineDto(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    Guid? InventoryItemId = null,
    Guid? UnitId = null,
    BackOfficeSalesLineNature LineNature = BackOfficeSalesLineNature.Inventory,
    decimal TaxPercent = 0,
    decimal DiscountAmount = 0,
    decimal UnitCost = 0);

public record CreateBackOfficeSalesOrderDto(
    Guid CustomerId,
    DateOnly OrderDate,
    string? OrderNumber = null,
    string Currency = "SAR",
    Guid? BranchId = null,
    Guid? WarehouseId = null,
    Guid? SalesPersonId = null,
    Guid? QuotationId = null,
    DateOnly? ExpectedDeliveryDate = null,
    decimal ExchangeRate = 1m,
    string? Notes = null,
    decimal DiscountAmount = 0,
    IReadOnlyList<CreateBackOfficeSalesOrderLineDto>? Lines = null);

public record UpdateBackOfficeSalesOrderDto(
    DateOnly OrderDate,
    Guid? WarehouseId = null,
    Guid? SalesPersonId = null,
    Guid? BranchId = null,
    DateOnly? ExpectedDeliveryDate = null,
    string? Notes = null,
    decimal? DiscountAmount = null,
    IReadOnlyList<CreateBackOfficeSalesOrderLineDto>? Lines = null);

public record CreateBackOfficeSalesOrderCommand(Guid TenantId, CreateBackOfficeSalesOrderDto Dto)
    : IRequest<Result<BackOfficeSalesOrderDto>>;

public record UpdateBackOfficeSalesOrderCommand(Guid Id, UpdateBackOfficeSalesOrderDto Dto)
    : IRequest<Result<BackOfficeSalesOrderDto>>;

public record ApproveBackOfficeSalesOrderCommand(Guid Id, Guid UserId) : IRequest<Result>;

public record UnapproveBackOfficeSalesOrderCommand(Guid Id) : IRequest<Result>;

public record CancelBackOfficeSalesOrderCommand(Guid Id) : IRequest<Result>;

public record CloseBackOfficeSalesOrderCommand(Guid Id) : IRequest<Result>;

public record GetBackOfficeSalesOrdersQuery(
    Guid TenantId,
    BackOfficeSalesDocumentStatus? Status = null,
    BackOfficeSalesFulfillmentStatus? FulfillmentStatus = null,
    Guid? CustomerId = null,
    Guid? BranchId = null,
    string? Search = null,
    DateOnly? From = null,
    DateOnly? To = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<BackOfficeSalesOrderDto>>;

public record GetBackOfficeSalesOrderByIdQuery(Guid Id) : IRequest<Result<BackOfficeSalesOrderDto>>;
