using GastroErp.Application.Common.Responses;
using GastroErp.Domain.Enums;
using MediatR;

namespace GastroErp.Application.Features.Sales.BackOffice.Quotations;

public record BackOfficeSalesQuotationLineDto(
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
    decimal LineTotal);

public record BackOfficeSalesQuotationDto(
    Guid Id,
    string QuotationNumber,
    BackOfficeSalesDocumentStatus Status,
    Guid CustomerId,
    Guid? BranchId,
    Guid? WarehouseId,
    Guid? SalesPersonId,
    DateOnly QuotationDate,
    DateOnly? ValidUntil,
    string Currency,
    decimal ExchangeRate,
    string? Notes,
    decimal DiscountAmount,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    Guid? ConvertedOrderId,
    DateTimeOffset? ApprovedAt,
    bool IsExpired,
    IReadOnlyList<BackOfficeSalesQuotationLineDto> Lines);

public record CreateBackOfficeSalesQuotationLineDto(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    Guid? InventoryItemId = null,
    Guid? UnitId = null,
    BackOfficeSalesLineNature LineNature = BackOfficeSalesLineNature.Inventory,
    decimal DiscountAmount = 0,
    decimal TaxPercent = 0,
    decimal TaxAmount = 0,
    decimal UnitCost = 0);

public record CreateBackOfficeSalesQuotationDto(
    Guid CustomerId,
    DateOnly QuotationDate,
    string? QuotationNumber = null,
    string Currency = "SAR",
    Guid? BranchId = null,
    Guid? WarehouseId = null,
    Guid? SalesPersonId = null,
    DateOnly? ValidUntil = null,
    decimal ExchangeRate = 1m,
    string? Notes = null,
    decimal DiscountAmount = 0,
    IReadOnlyList<CreateBackOfficeSalesQuotationLineDto>? Lines = null);

public record UpdateBackOfficeSalesQuotationDto(
    DateOnly QuotationDate,
    Guid? WarehouseId = null,
    Guid? SalesPersonId = null,
    Guid? BranchId = null,
    DateOnly? ValidUntil = null,
    string? Notes = null,
    decimal? DiscountAmount = null,
    IReadOnlyList<CreateBackOfficeSalesQuotationLineDto>? Lines = null);

public record CreateBackOfficeSalesQuotationCommand(Guid TenantId, CreateBackOfficeSalesQuotationDto Dto)
    : IRequest<Result<BackOfficeSalesQuotationDto>>;

public record UpdateBackOfficeSalesQuotationCommand(Guid Id, UpdateBackOfficeSalesQuotationDto Dto)
    : IRequest<Result<BackOfficeSalesQuotationDto>>;

public record ApproveBackOfficeSalesQuotationCommand(Guid Id, Guid UserId) : IRequest<Result>;

public record UnapproveBackOfficeSalesQuotationCommand(Guid Id) : IRequest<Result>;

public record CancelBackOfficeSalesQuotationCommand(Guid Id) : IRequest<Result>;

/// <summary>يحوّل عرض السعر إلى أمر بيع إداري (يعتبر ترحيلًا للعرض).</summary>
public record ConvertBackOfficeSalesQuotationToOrderCommand(
    Guid QuotationId,
    Guid UserId,
    DateOnly? OrderDate = null,
    DateOnly? ExpectedDeliveryDate = null,
    string? OrderNumber = null) : IRequest<Result<Guid>>;

public record GetBackOfficeSalesQuotationsQuery(
    Guid TenantId,
    BackOfficeSalesDocumentStatus? Status = null,
    Guid? CustomerId = null,
    Guid? BranchId = null,
    string? Search = null,
    DateOnly? From = null,
    DateOnly? To = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<BackOfficeSalesQuotationDto>>;

public record GetBackOfficeSalesQuotationByIdQuery(Guid Id) : IRequest<Result<BackOfficeSalesQuotationDto>>;
