using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Inventory.DTOs;

// Purchase Order DTOs live in PurchaseOrderDtos.cs

// ─── GoodsReceipt DTOs ────────────────────────────────────────────────────────

public record GoodsReceiptLineDto(
    Guid Id,
    Guid InventoryItemId,
    string? ItemNameAr,
    string? ItemSku,
    Guid UnitId,
    string? UnitNameAr,
    Guid? PurchaseOrderLineId,
    decimal OrderedQuantity,
    decimal PreviouslyReceivedQuantity,
    decimal RemainingQuantity,
    decimal ReceivedQuantity,
    decimal AcceptedQuantity,
    decimal RejectedQuantity,
    decimal UnitCost,
    decimal DiscountAmount,
    decimal TaxPercent,
    decimal TaxAmount,
    decimal LineSubTotal,
    decimal InvoicedQuantity,
    string? BatchNumber,
    DateTimeOffset? ProductionDate,
    DateTimeOffset? ExpiryDate,
    string? StorageLocation,
    string? Description
);

public record GoodsReceiptDto(
    Guid Id,
    Guid TenantId,
    Guid? BranchId,
    Guid? PurchaseOrderId,
    string PoNumber,
    decimal? PoCompletionPercent,
    Guid SupplierId,
    string SupplierNameAr,
    Guid WarehouseId,
    string WarehouseNameAr,
    string GrnNumber,
    string? ReferenceNumber,
    DateTimeOffset ReceiptDate,
    GoodsReceiptStatus Status,
    byte UnifiedStatusCode,
    GoodsReceiptSource Source,
    string Currency,
    decimal ExchangeRate,
    string? ReceiptMethod,
    string? ReceivedByName,
    string? SupplierRepName,
    string? VehicleNumber,
    string? WaybillNumber,
    string? Notes,
    InspectionResult InspectionResult,
    string? InspectedBy,
    DateTimeOffset? InspectionDate,
    string? QualityNotes,
    string? RejectionReason,
    string? QualityCertificateRef,
    string? ExpiryCertificateRef,
    Guid? JournalEntryId,
    int LineCount,
    decimal TotalQuantity,
    decimal TotalValue,
    decimal TotalTax,
    decimal GrandTotal,
    bool IsInvoiced,
    bool IsPartiallyInvoiced,
    IReadOnlyList<GoodsReceiptLineDto> Lines,
    DateTime CreatedAt
);

public record CreateGoodsReceiptLineInputDto(
    Guid InventoryItemId,
    Guid UnitId,
    decimal ReceivedQuantity,
    decimal UnitCost,
    Guid? PurchaseOrderLineId = null,
    decimal OrderedQuantity = 0,
    decimal PreviouslyReceivedQuantity = 0,
    decimal? AcceptedQuantity = null,
    decimal RejectedQuantity = 0,
    decimal DiscountAmount = 0,
    decimal TaxPercent = 0,
    decimal TaxAmount = 0,
    string? BatchNumber = null,
    DateTimeOffset? ProductionDate = null,
    DateTimeOffset? ExpiryDate = null,
    string? StorageLocation = null,
    string? Description = null
);

public record CreateGoodsReceiptDto(
    Guid TenantId,
    Guid WarehouseId,
    string? GrnNumber = null,
    Guid? PurchaseOrderId = null,
    Guid? SupplierId = null,
    bool DirectReceipt = false,
    Guid? BranchId = null,
    DateTimeOffset? ReceiptDate = null,
    string Currency = "SAR",
    decimal ExchangeRate = 1,
    string? ReferenceNumber = null,
    string? Notes = null,
    string? ReceiptMethod = null,
    string? ReceivedByName = null,
    string? SupplierRepName = null,
    string? VehicleNumber = null,
    string? WaybillNumber = null,
    InspectionResult InspectionResult = InspectionResult.Accepted,
    string? InspectedBy = null,
    DateTimeOffset? InspectionDate = null,
    string? QualityNotes = null,
    string? RejectionReason = null,
    string? QualityCertificateRef = null,
    string? ExpiryCertificateRef = null,
    IReadOnlyList<CreateGoodsReceiptLineInputDto>? Lines = null
);

/// <summary>Legacy add-line payload (kept for inventory-operations compatibility).</summary>
public record AddGoodsReceiptLineDto(
    Guid PurchaseOrderLineId,
    Guid InventoryItemId,
    Guid UnitId,
    decimal ReceivedQuantity,
    decimal UnitCost,
    Guid? BatchId = null
);

public record UpdateGoodsReceiptDto(
    DateTimeOffset ReceiptDate,
    Guid WarehouseId,
    string? ReferenceNumber = null,
    string? Notes = null,
    string? ReceiptMethod = null,
    string? ReceivedByName = null,
    string? SupplierRepName = null,
    string? VehicleNumber = null,
    string? WaybillNumber = null,
    string Currency = "SAR",
    decimal ExchangeRate = 1,
    Guid? BranchId = null,
    InspectionResult InspectionResult = InspectionResult.Accepted,
    string? InspectedBy = null,
    DateTimeOffset? InspectionDate = null,
    string? QualityNotes = null,
    string? RejectionReason = null,
    string? QualityCertificateRef = null,
    string? ExpiryCertificateRef = null,
    IReadOnlyList<CreateGoodsReceiptLineInputDto>? Lines = null
);

// ─── StockTransfer DTOs ───────────────────────────────────────────────────────

public record StockTransferLineDetailDto(
    Guid Id,
    Guid InventoryItemId,
    string? ItemNameAr,
    string? ItemSku,
    Guid UnitId,
    string? UnitNameAr,
    decimal Quantity,
    decimal UnitCost,
    decimal LineTotal,
    decimal ReceivedQuantity,
    string? BatchNumber
);

public record StockTransferDto(
    Guid Id,
    Guid TenantId,
    Guid SourceWarehouseId,
    string SourceWarehouseNameAr,
    Guid DestinationWarehouseId,
    string DestinationWarehouseNameAr,
    string TransferNumber,
    DateTimeOffset TransferDate,
    string TransferType,
    byte TransferTypeCode,
    string Status,
    byte StatusCode,
    string? Notes,
    int LineCount,
    decimal TotalAmount,
    IReadOnlyList<StockTransferLineDetailDto> Lines,
    DateTime CreatedAt
);

public record StockTransferLineInputDto(
    Guid InventoryItemId,
    Guid UnitId,
    decimal Quantity,
    decimal UnitCost = 0,
    string? BatchNumber = null
);

public record CreateStockTransferDto(
    Guid TenantId,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    string? TransferNumber = null,
    bool AutoGenerateNumber = true,
    DateTimeOffset? TransferDate = null,
    byte TransferType = 1,
    string? Notes = null,
    IReadOnlyList<StockTransferLineInputDto>? Lines = null
);

public record UpdateStockTransferDto(
    Guid TenantId,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    DateTimeOffset TransferDate,
    byte TransferType = 1,
    string? Notes = null,
    IReadOnlyList<StockTransferLineInputDto>? Lines = null
);

public record AddTransferLineDto(
    Guid InventoryItemId,
    Guid UnitId,
    decimal Quantity,
    decimal UnitCost = 0,
    string? BatchNumber = null
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

// ─── GoodsIssue DTOs ──────────────────────────────────────────────────────────

public record GoodsIssueLineDetailDto(
    Guid Id,
    Guid InventoryItemId,
    string? ItemNameAr,
    string? ItemSku,
    Guid UnitId,
    string? UnitNameAr,
    Guid WarehouseId,
    string? WarehouseNameAr,
    decimal Quantity,
    decimal UnitCost,
    decimal TotalCost,
    Guid? CostCenterId,
    string? CostCenterNameAr,
    string? Notes
);

public record GoodsIssueDto(
    Guid Id,
    Guid TenantId,
    Guid? WarehouseId,
    string? WarehouseNameAr,
    Guid? IssueDestinationId,
    string? IssueDestinationNameAr,
    string IssueNumber,
    DateTimeOffset IssueDate,
    DateTimeOffset? ApprovalDate,
    string Currency,
    string? Notes,
    string Status,
    byte StatusCode,
    bool IsConfirmed,
    bool IsCompleted,
    int LineCount,
    decimal TotalAmount,
    IReadOnlyList<GoodsIssueLineDetailDto> Lines,
    DateTime CreatedAt
);

public record GoodsIssueLineInputDto(
    Guid InventoryItemId,
    Guid UnitId,
    decimal Quantity,
    decimal UnitCost = 0,
    Guid? WarehouseId = null,
    Guid? CostCenterId = null,
    string? Notes = null
);

public record CreateGoodsIssueDto(
    Guid TenantId,
    string? IssueNumber = null,
    bool AutoGenerateNumber = true,
    DateTimeOffset? IssueDate = null,
    Guid? WarehouseId = null,
    Guid? IssueDestinationId = null,
    string Currency = "SAR",
    string? Notes = null,
    IReadOnlyList<GoodsIssueLineInputDto>? Lines = null
);

public record UpdateGoodsIssueDto(
    Guid TenantId,
    DateTimeOffset IssueDate,
    Guid? WarehouseId = null,
    Guid? IssueDestinationId = null,
    string Currency = "SAR",
    string? Notes = null,
    IReadOnlyList<GoodsIssueLineInputDto>? Lines = null
);

public record AddGoodsIssueLineDto(
    Guid InventoryItemId,
    Guid UnitId,
    decimal Quantity,
    decimal UnitCost = 0,
    Guid? WarehouseId = null,
    Guid? CostCenterId = null,
    string? Notes = null
);

public record GoodsIssueLineDto(
    Guid Id,
    Guid InventoryItemId,
    Guid UnitId,
    decimal Quantity,
    decimal UnitCost,
    Guid WarehouseId,
    Guid? CostCenterId,
    string? Notes
);

public record IssueDestinationDto(
    Guid Id,
    Guid TenantId,
    string Code,
    string NameAr,
    string? NameEn,
    string? Description,
    string DestinationType,
    byte DestinationTypeCode,
    Guid? DefaultGlAccountId,
    string? GlAccountNameAr,
    Guid? DefaultCostCenterId,
    string? CostCenterNameAr,
    bool AllowChangeAccountOnIssue,
    bool RequireEmployee,
    bool RequireProject,
    bool RequireCostCenter,
    bool RequireBranch,
    bool RequireReason,
    bool RequireApproval,
    bool AllowDirectIssue,
    bool AllowNegativeStock,
    Guid? WorkflowDefinitionId,
    string PolicySummary,
    int SortOrder,
    bool IsSystem,
    bool IsActive
);

public record CreateIssueDestinationDto(
    Guid TenantId,
    string Code,
    string NameAr,
    byte DestinationType = 13,
    string? NameEn = null,
    string? Description = null,
    Guid? DefaultGlAccountId = null,
    Guid? DefaultCostCenterId = null,
    bool AllowChangeAccountOnIssue = true,
    bool RequireEmployee = false,
    bool RequireProject = false,
    bool RequireCostCenter = false,
    bool RequireBranch = false,
    bool RequireReason = false,
    bool RequireApproval = false,
    bool AllowDirectIssue = true,
    bool AllowNegativeStock = false,
    Guid? WorkflowDefinitionId = null,
    int SortOrder = 0,
    bool IsActive = true
);

public record UpdateIssueDestinationDto(
    Guid TenantId,
    string NameAr,
    byte DestinationType = 13,
    string? NameEn = null,
    string? Description = null,
    Guid? DefaultGlAccountId = null,
    Guid? DefaultCostCenterId = null,
    bool AllowChangeAccountOnIssue = true,
    bool RequireEmployee = false,
    bool RequireProject = false,
    bool RequireCostCenter = false,
    bool RequireBranch = false,
    bool RequireReason = false,
    bool RequireApproval = false,
    bool AllowDirectIssue = true,
    bool AllowNegativeStock = false,
    Guid? WorkflowDefinitionId = null,
    int SortOrder = 0,
    bool IsActive = true
);

// ─── OpeningBalance DTOs ──────────────────────────────────────────────────────

public record OpeningBalanceLineDetailDto(
    Guid Id,
    Guid InventoryItemId,
    string? ItemNameAr,
    string? ItemSku,
    Guid WarehouseId,
    string? WarehouseNameAr,
    Guid UnitId,
    string? UnitNameAr,
    decimal Quantity,
    decimal UnitCost,
    string? BatchNumber,
    DateTimeOffset? ExpiryDate,
    string? SerialNumber
);

public record OpeningBalanceDto(
    Guid Id,
    Guid TenantId,
    Guid? WarehouseId,
    string? WarehouseNameAr,
    string DocumentNumber,
    DateTimeOffset DocumentDate,
    DateTimeOffset? ApprovalDate,
    string? Notes,
    string Status,
    byte StatusCode,
    string EntryMethod,
    string DisplayMethod,
    string CostingMethod,
    string WeightedAverageScope,
    bool UseExpiryDate,
    bool UseBatchNumbers,
    bool UseSerialNumbers,
    Guid? ContraAccountId,
    string? ContraAccountName,
    Guid? CostCenterId,
    string? CostCenterNameAr,
    bool IsApproved,
    bool IsPosted,
    int LineCount,
    IReadOnlyList<OpeningBalanceLineDetailDto> Lines,
    DateTime CreatedAt
);

public record OpeningBalanceLineInputDto(
    Guid InventoryItemId,
    Guid UnitId,
    decimal Quantity,
    decimal UnitCost,
    Guid? WarehouseId = null,
    string? BatchNumber = null,
    DateTimeOffset? ExpiryDate = null,
    string? SerialNumber = null,
    Guid? LineId = null
);

public record CreateOpeningBalanceDto(
    Guid TenantId,
    string? DocumentNumber = null,
    bool AutoGenerateNumber = true,
    DateTimeOffset? DocumentDate = null,
    Guid? WarehouseId = null,
    string? Notes = null,
    byte EntryMethod = 1,
    byte DisplayMethod = 1,
    byte CostingMethod = 2,
    byte WeightedAverageScope = 1,
    bool UseExpiryDate = false,
    bool UseBatchNumbers = false,
    bool UseSerialNumbers = false,
    Guid? ContraAccountId = null,
    Guid? CostCenterId = null,
    IReadOnlyList<OpeningBalanceLineInputDto>? Lines = null
);

public record UpdateOpeningBalanceDto(
    Guid TenantId,
    DateTimeOffset DocumentDate,
    Guid? WarehouseId = null,
    string? Notes = null,
    byte EntryMethod = 1,
    byte DisplayMethod = 1,
    byte CostingMethod = 2,
    byte WeightedAverageScope = 1,
    bool UseExpiryDate = false,
    bool UseBatchNumbers = false,
    bool UseSerialNumbers = false,
    Guid? ContraAccountId = null,
    Guid? CostCenterId = null,
    IReadOnlyList<OpeningBalanceLineInputDto>? Lines = null
);

public record AddOpeningBalanceLineDto(
    Guid InventoryItemId,
    Guid UnitId,
    decimal Quantity,
    decimal UnitCost,
    Guid? WarehouseId = null,
    string? BatchNumber = null,
    DateTimeOffset? ExpiryDate = null,
    string? SerialNumber = null
);

public record OpeningBalanceLineDto(
    Guid Id,
    Guid InventoryItemId,
    Guid UnitId,
    decimal Quantity,
    decimal UnitCost,
    Guid WarehouseId
);
