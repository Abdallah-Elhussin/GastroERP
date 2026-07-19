using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Inventory.DTOs;

public record PurchaseOrderLineDto(
    Guid Id,
    Guid InventoryItemId,
    string? ItemNameAr,
    string? ItemSku,
    Guid UnitId,
    string? UnitNameAr,
    Guid? WarehouseId,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal LineSubTotal,
    decimal LineTotal,
    decimal ReceivedQuantity,
    decimal InvoicedQuantity,
    decimal RemainingQuantity,
    string? Description,
    string? LineNotes);

public record PurchaseOrderDto(
    Guid Id,
    Guid TenantId,
    Guid SupplierId,
    string? SupplierNameAr,
    Guid DestinationWarehouseId,
    string? WarehouseNameAr,
    Guid? BranchId,
    Guid? CostCenterId,
    Guid? ResponsibleEmployeeId,
    string PoNumber,
    byte OrderType,
    DateTimeOffset OrderDate,
    DateTimeOffset? ExpectedDeliveryDate,
    PurchaseOrderStatus Status,
    byte StatusCode,
    string Currency,
    decimal ExchangeRate,
    string? PaymentMethod,
    string? PaymentTerms,
    string? ExternalReference,
    string? Notes,
    decimal TotalAmount,
    decimal CompletionPercent,
    decimal RemainingQuantity,
    int LineCount,
    DateTimeOffset? LastReceiptDate,
    DateTimeOffset CreatedAt,
    IReadOnlyList<PurchaseOrderLineDto> Lines);

public record PurchaseOrderLineInputDto(
    Guid InventoryItemId,
    Guid UnitId,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountAmount = 0,
    decimal TaxAmount = 0,
    string? Description = null,
    Guid? WarehouseId = null,
    string? LineNotes = null);

public record CreatePurchaseOrderDto(
    Guid SupplierId,
    Guid DestinationWarehouseId,
    string? PoNumber = null,
    DateTimeOffset? OrderDate = null,
    DateTimeOffset? ExpectedDeliveryDate = null,
    string Currency = "SAR",
    decimal ExchangeRate = 1,
    byte OrderType = 1,
    Guid? BranchId = null,
    Guid? CostCenterId = null,
    Guid? ResponsibleEmployeeId = null,
    string? PaymentMethod = null,
    string? PaymentTerms = null,
    string? ExternalReference = null,
    string? Notes = null,
    IReadOnlyList<PurchaseOrderLineInputDto>? Lines = null);

public record UpdatePurchaseOrderDto(
    Guid SupplierId,
    Guid DestinationWarehouseId,
    DateTimeOffset OrderDate,
    DateTimeOffset? ExpectedDeliveryDate,
    string Currency,
    decimal ExchangeRate,
    byte OrderType,
    Guid? BranchId,
    Guid? CostCenterId,
    Guid? ResponsibleEmployeeId,
    string? PaymentMethod,
    string? PaymentTerms,
    string? ExternalReference,
    string? Notes,
    IReadOnlyList<PurchaseOrderLineInputDto> Lines);

public record PurchaseOrderDashboardDto(
    int OrdersToday,
    int ApprovedCount,
    int AwaitingReceiptCount,
    int ClosedCount,
    int OverdueCount,
    decimal TotalValue);

/// <summary>Kept for older AddLine command callers.</summary>
public record AddPurchaseOrderLineDto(
    Guid InventoryItemId,
    Guid UnitId,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxAmount = 0);
