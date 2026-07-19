using GastroErp.Application.Common.Responses;
using GastroErp.Domain.Enums;
using MediatR;

namespace GastroErp.Application.Features.Inventory.Commands;

public record PurchaseInvoiceLineDto(
    Guid Id,
    Guid InventoryItemId,
    Guid UnitId,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal DiscountAmount,
    decimal TaxPercent,
    decimal TaxAmount,
    decimal LineNet,
    decimal LineTotal,
    Guid? GoodsReceiptLineId,
    Guid? PurchaseOrderLineId,
    Guid? LineWarehouseId,
    Guid? CostCenterId,
    string? BatchNumber,
    string? SerialNumber,
    DateTimeOffset? ProductionDate,
    DateTimeOffset? ExpiryDate,
    string? Description,
    decimal ReturnedQuantity,
    decimal RemainingToReturn);

public record PurchaseInvoiceDto(
    Guid Id,
    string InvoiceNumber,
    PurchaseInvoiceKind Kind,
    PurchaseInvoicePaymentMode PaymentMode,
    DirectPurchaseNature Nature,
    PurchasingDocumentStatus Status,
    Guid SupplierId,
    Guid? BranchId,
    Guid? PurchaseOrderId,
    Guid? GoodsReceiptId,
    Guid? WarehouseId,
    Guid? CostCenterId,
    DateOnly InvoiceDate,
    DateOnly? DueDate,
    string Currency,
    decimal ExchangeRate,
    string? SupplierInvoiceNumber,
    string? ExternalReference,
    string? Notes,
    decimal DiscountAmount,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal RemainingAmount,
    PurchaseInvoicePaymentStatus PaymentStatus,
    Guid? JournalEntryId,
    Guid? ReversalJournalEntryId,
    DateTimeOffset? PostedAt,
    IReadOnlyList<PurchaseInvoiceLineDto> Lines);

public record CreatePurchaseInvoiceLineDto(
    Guid InventoryItemId,
    Guid UnitId,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxAmount = 0,
    Guid? GoodsReceiptLineId = null,
    Guid? PurchaseOrderLineId = null,
    string? Description = null,
    decimal DiscountPercent = 0,
    decimal DiscountAmount = 0,
    decimal TaxPercent = 0,
    string? BatchNumber = null,
    string? SerialNumber = null,
    DateTimeOffset? ProductionDate = null,
    DateTimeOffset? ExpiryDate = null,
    Guid? LineWarehouseId = null,
    Guid? CostCenterId = null);

public record CreatePurchaseInvoiceDto(
    PurchaseInvoiceKind Kind,
    PurchaseInvoicePaymentMode PaymentMode,
    Guid SupplierId,
    DateOnly InvoiceDate,
    string? InvoiceNumber = null,
    string Currency = "SAR",
    Guid? WarehouseId = null,
    Guid? PurchaseOrderId = null,
    Guid? GoodsReceiptId = null,
    DateOnly? DueDate = null,
    string? SupplierInvoiceNumber = null,
    string? Notes = null,
    DirectPurchaseNature Nature = DirectPurchaseNature.Inventory,
    decimal ExchangeRate = 1m,
    string? ExternalReference = null,
    Guid? CostCenterId = null,
    Guid? BranchId = null,
    decimal DiscountAmount = 0,
    IReadOnlyList<CreatePurchaseInvoiceLineDto>? Lines = null);

public record UpdatePurchaseInvoiceDto(
    DateOnly InvoiceDate,
    PurchaseInvoicePaymentMode PaymentMode,
    DateOnly? DueDate = null,
    string? SupplierInvoiceNumber = null,
    string? Notes = null,
    Guid? WarehouseId = null,
    DirectPurchaseNature? Nature = null,
    decimal? ExchangeRate = null,
    string? ExternalReference = null,
    Guid? CostCenterId = null,
    Guid? BranchId = null,
    decimal? DiscountAmount = null,
    IReadOnlyList<CreatePurchaseInvoiceLineDto>? Lines = null);

public record CreatePurchaseInvoiceCommand(Guid TenantId, CreatePurchaseInvoiceDto Dto)
    : IRequest<Result<PurchaseInvoiceDto>>;

public record UpdatePurchaseInvoiceCommand(Guid Id, UpdatePurchaseInvoiceDto Dto)
    : IRequest<Result<PurchaseInvoiceDto>>;

public record ApprovePurchaseInvoiceCommand(Guid Id) : IRequest<Result>;

public record PostPurchaseInvoiceCommand(Guid Id, Guid UserId) : IRequest<Result>;

public record UnpostPurchaseInvoiceCommand(Guid Id, Guid UserId) : IRequest<Result>;

public record CancelPurchaseInvoiceCommand(Guid Id) : IRequest<Result>;
