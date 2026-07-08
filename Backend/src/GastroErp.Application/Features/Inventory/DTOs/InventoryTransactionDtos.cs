using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Inventory.DTOs;

// ─── PurchaseOrder DTOs ───────────────────────────────────────────────────────

public record PurchaseOrderDto(
    Guid Id,
    Guid TenantId,
    Guid SupplierId,
    string SupplierNameAr,
    Guid DestinationWarehouseId,
    string WarehouseNameAr,
    string PoNumber,
    DateTimeOffset OrderDate,
    DateTimeOffset? ExpectedDeliveryDate,
    PurchaseOrderStatus Status,
    decimal TotalAmount,
    string Currency,
    string? Notes,
    int LineCount,
    DateTime CreatedAt
);

public record CreatePurchaseOrderDto(
    Guid TenantId,
    Guid SupplierId,
    Guid DestinationWarehouseId,
    string PoNumber,
    DateTimeOffset ExpectedDeliveryDate,
    string Currency = "SAR",
    string? Notes = null
);

public record AddPurchaseOrderLineDto(
    Guid InventoryItemId,
    Guid UnitId,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxAmount = 0
);

public record PurchaseOrderLineDto(
    Guid Id,
    Guid InventoryItemId,
    string ItemNameAr,
    Guid UnitId,
    string UnitNameAr,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxAmount,
    decimal ReceivedQuantity,
    decimal LineTotal
);

// ─── GoodsReceipt DTOs ────────────────────────────────────────────────────────

public record GoodsReceiptDto(
    Guid Id,
    Guid TenantId,
    Guid PurchaseOrderId,
    string PoNumber,
    Guid WarehouseId,
    string WarehouseNameAr,
    string GrnNumber,
    DateTimeOffset ReceiptDate,
    string? Notes,
    int LineCount,
    DateTime CreatedAt
);

public record CreateGoodsReceiptDto(
    Guid TenantId,
    Guid PurchaseOrderId,
    Guid WarehouseId,
    string GrnNumber,
    string? Notes = null
);

public record AddGoodsReceiptLineDto(
    Guid PurchaseOrderLineId,
    Guid InventoryItemId,
    Guid UnitId,
    decimal ReceivedQuantity,
    decimal UnitCost,
    Guid? BatchId = null
);

// ─── StockTransfer DTOs ───────────────────────────────────────────────────────

public record StockTransferDto(
    Guid Id,
    Guid TenantId,
    Guid SourceWarehouseId,
    string SourceWarehouseNameAr,
    Guid DestinationWarehouseId,
    string DestinationWarehouseNameAr,
    string TransferNumber,
    DateTimeOffset TransferDate,
    string Status,
    string? Notes,
    DateTime CreatedAt
);

public record CreateStockTransferDto(
    Guid TenantId,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    string TransferNumber,
    string? Notes = null
);

public record AddTransferLineDto(
    Guid InventoryItemId,
    Guid UnitId,
    decimal Quantity
);

// ─── InventoryTransaction DTOs ────────────────────────────────────────────────

public record InventoryTransactionDto(
    Guid Id,
    Guid TenantId,
    string TransactionType,
    string ReferenceDocumentNumber,
    Guid ReferenceDocumentId,
    DateTimeOffset TransactionDate,
    string? Notes,
    int MovementCount
);



// ─── StockAdjustment DTOs ─────────────────────────────────────────────────────

public record StockAdjustmentDto(
    Guid Id,
    Guid TenantId,
    Guid WarehouseId,
    string WarehouseNameAr,
    Guid InventoryItemId,
    string ItemNameAr,
    string AdjustmentNumber,
    decimal QuantityAdjusted,
    Guid? ReasonId,
    string? Notes,
    DateTimeOffset AdjustmentDate,
    DateTime CreatedAt
);

public record CreateStockAdjustmentDto(
    Guid TenantId,
    Guid WarehouseId,
    Guid InventoryItemId,
    string AdjustmentNumber,
    decimal QuantityAdjusted,
    Guid UnitId,
    decimal UnitCost,
    Guid? ReasonId = null,
    string? Notes = null
);

// ─── WasteRecord DTOs ─────────────────────────────────────────────────────────

public record WasteRecordDto(
    Guid Id,
    Guid TenantId,
    Guid WarehouseId,
    string WarehouseNameAr,
    Guid InventoryItemId,
    string ItemNameAr,
    string WasteNumber,
    decimal Quantity,
    decimal UnitCost,
    Guid? ReasonId,
    string? Notes,
    DateTimeOffset WasteDate,
    DateTime CreatedAt
);

public record CreateWasteRecordDto(
    Guid TenantId,
    Guid WarehouseId,
    Guid InventoryItemId,
    Guid UnitId,
    string WasteNumber,
    decimal Quantity,
    decimal UnitCost,
    Guid? ReasonId = null,
    string? Notes = null
);

// ─── Recipe DTOs ──────────────────────────────────────────────────────────────

public record RecipeDto(
    Guid Id,
    Guid TenantId,
    Guid ProductId,
    string ProductNameAr,
    int Yield,
    string? Notes,
    bool IsActive,
    int IngredientCount,
    DateTime CreatedAt
);

public record CreateRecipeDto(
    Guid TenantId,
    Guid ProductId,
    int Yield = 1,
    string? Notes = null
);

public record RecipeIngredientDto(
    Guid Id,
    Guid InventoryItemId,
    string ItemNameAr,
    Guid UnitId,
    string UnitNameAr,
    decimal Quantity,
    bool IsOptional
);

public record AddRecipeIngredientDto(
    Guid InventoryItemId,
    Guid UnitId,
    decimal Quantity,
    bool IsOptional = false
);

public record RecordWasteDto(Guid Id);
