using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Inventory.DTOs;

// ─── InventoryCategory DTOs ───────────────────────────────────────────────────

public record InventoryCategoryDto(
    Guid Id,
    Guid TenantId,
    Guid? ParentCategoryId,
    string Code,
    string NameAr,
    string? NameEn,
    string? DescriptionAr,
    string? DescriptionEn,
    string? Icon,
    string? ImageUrl,
    string? Color,
    int SortOrder,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateInventoryCategoryDto(
    Guid TenantId,
    string NameAr,
    string? NameEn = null,
    Guid? ParentCategoryId = null,
    string? Code = null,
    string? DescriptionAr = null,
    string? DescriptionEn = null,
    string? Icon = null,
    string? ImageUrl = null,
    string? Color = null,
    int SortOrder = 0
);

public record UpdateInventoryCategoryDto(
    string NameAr,
    string? NameEn,
    Guid? ParentCategoryId,
    string? Code,
    string? DescriptionAr,
    string? DescriptionEn,
    string? Icon,
    string? ImageUrl,
    string? Color,
    int SortOrder
);

// ─── InventoryUnit DTOs ───────────────────────────────────────────────────────

public record InventoryUnitDto(
    Guid Id,
    Guid TenantId,
    string Code,
    string NameAr,
    string? NameEn,
    string? Symbol,
    string? SymbolAr,
    byte DecimalPlaces,
    Guid? BaseUnitId,
    decimal ConversionFactor,
    InventoryUnitType UnitType,
    InventoryUnitClassification Classification,
    int SortOrder,
    bool IsActive
);

public record CreateInventoryUnitDto(
    Guid TenantId,
    string NameAr,
    string Symbol,
    string? NameEn = null,
    string? SymbolAr = null,
    string? Code = null,
    byte DecimalPlaces = 2,
    Guid? BaseUnitId = null,
    decimal ConversionFactor = 1m,
    InventoryUnitType UnitType = InventoryUnitType.Measured,
    InventoryUnitClassification Classification = InventoryUnitClassification.Other,
    int SortOrder = 0,
    bool IsActive = true
);

public record UpdateInventoryUnitDto(
    string NameAr,
    string Symbol,
    string? NameEn,
    string? SymbolAr,
    string? Code,
    byte DecimalPlaces,
    Guid? BaseUnitId,
    decimal ConversionFactor = 1m,
    InventoryUnitType UnitType = InventoryUnitType.Measured,
    InventoryUnitClassification Classification = InventoryUnitClassification.Other,
    int SortOrder = 0,
    bool IsActive = true
);

// ─── UnitConversion DTOs ──────────────────────────────────────────────────────

public record UnitConversionDto(
    Guid Id,
    Guid TenantId,
    Guid FromUnitId,
    string FromUnitNameAr,
    Guid ToUnitId,
    string ToUnitNameAr,
    decimal Factor
);

public record CreateUnitConversionDto(
    Guid TenantId,
    Guid FromUnitId,
    Guid ToUnitId,
    decimal Factor
);

// ─── InventoryItem DTOs ───────────────────────────────────────────────────────

public record InventoryItemDto(
    Guid Id,
    Guid TenantId,
    Guid CategoryId,
    string CategoryNameAr,
    string NameAr,
    string? NameEn,
    string? DescriptionAr,
    string? DescriptionEn,
    string? Sku,
    string? Barcode,
    string? ImageUrl,
    InventoryItemKind ItemKind,
    Guid BaseUnitId,
    string BaseUnitNameAr,
    Guid? DefaultPurchaseUnitId,
    Guid? DefaultRecipeUnitId,
    decimal ReorderLevel,
    decimal ReorderQuantity,
    decimal? AverageUnitCost,
    decimal? LastPurchaseUnitCost,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateInventoryItemDto(
    Guid TenantId,
    Guid CategoryId,
    string NameAr,
    Guid BaseUnitId,
    string? NameEn = null,
    string? DescriptionAr = null,
    string? DescriptionEn = null,
    string? Sku = null,
    string? Barcode = null,
    string? ImageUrl = null,
    InventoryItemKind ItemKind = InventoryItemKind.Raw,
    Guid? DefaultPurchaseUnitId = null,
    Guid? DefaultRecipeUnitId = null,
    decimal ReorderLevel = 0,
    decimal ReorderQuantity = 0
);

public record UpdateInventoryItemDto(
    string NameAr,
    string? NameEn,
    string? DescriptionAr,
    string? DescriptionEn,
    string? Sku,
    string? Barcode,
    string? ImageUrl,
    InventoryItemKind ItemKind,
    Guid? CategoryId,
    Guid? BaseUnitId,
    Guid? DefaultPurchaseUnitId,
    Guid? DefaultRecipeUnitId,
    decimal? ReorderLevel,
    decimal? ReorderQuantity
);

public record SetReorderInfoDto(
    decimal ReorderLevel,
    decimal ReorderQuantity
);

// ─── Warehouse DTOs ───────────────────────────────────────────────────────────

public record WarehouseDto(
    Guid Id,
    Guid TenantId,
    Guid? BranchId,
    Guid? CompanyId,
    string NameAr,
    string? NameEn,
    string? Code,
    string? Address,
    string? Phone,
    string? Email,
    string? Notes,
    WarehouseType WarehouseType,
    Guid? WarehouseTypeId,
    string? WarehouseTypeNameAr,
    Guid? ParentWarehouseId,
    string? ParentWarehouseNameAr,
    string? BranchNameAr,
    Guid? ManagerUserId,
    Guid? ResponsibleEmployeeId,
    bool AllowPurchase,
    bool AllowSales,
    bool AllowTransfer,
    bool AllowInventoryCount,
    bool AllowManufacturing,
    bool AllowNegativeStock,
    bool AllowReservation,
    bool AllowReceiving,
    bool AllowIssue,
    bool AllowAdjustment,
    bool IsPosWarehouse,
    bool IsDefault,
    bool IsSystem,
    bool UseBins,
    bool IsActive,
    int ZoneCount,
    DateTime CreatedAt
);

public record CreateWarehouseDto(
    Guid TenantId,
    string NameAr,
    string? NameEn = null,
    string? Code = null,
    Guid? BranchId = null,
    Guid? CompanyId = null,
    WarehouseType WarehouseType = WarehouseType.Main,
    Guid? WarehouseTypeId = null,
    Guid? ParentWarehouseId = null,
    string? Address = null,
    string? Phone = null,
    string? Email = null,
    string? Notes = null,
    Guid? ManagerUserId = null,
    Guid? ResponsibleEmployeeId = null,
    bool AllowPurchase = true,
    bool AllowSales = true,
    bool AllowTransfer = true,
    bool AllowInventoryCount = true,
    bool AllowManufacturing = true,
    bool AllowNegativeStock = false,
    bool AllowReservation = true,
    bool AllowReceiving = true,
    bool AllowIssue = true,
    bool AllowAdjustment = true,
    bool IsPosWarehouse = false,
    bool IsDefault = false,
    bool UseBins = false
);

public record UpdateWarehouseDto(
    string NameAr,
    string? NameEn,
    string? Code,
    string? Address,
    string? Phone,
    string? Email,
    string? Notes,
    Guid? BranchId,
    Guid? CompanyId,
    WarehouseType WarehouseType,
    Guid? WarehouseTypeId,
    Guid? ParentWarehouseId,
    Guid? ManagerUserId,
    Guid? ResponsibleEmployeeId,
    bool AllowPurchase,
    bool AllowSales,
    bool AllowTransfer,
    bool AllowInventoryCount,
    bool AllowManufacturing,
    bool AllowNegativeStock = false,
    bool AllowReservation = true,
    bool AllowReceiving = true,
    bool AllowIssue = true,
    bool AllowAdjustment = true,
    bool IsPosWarehouse = false,
    bool IsDefault = false,
    bool UseBins = false,
    bool IsActive = true
);

public record WarehouseTypeDefinitionDto(
    Guid Id,
    Guid TenantId,
    string Code,
    string NameAr,
    string? NameEn,
    string? Description,
    int SortOrder,
    bool IsSystem,
    bool IsActive
);

public record UpsertWarehouseTypeDefinitionDto(
    Guid TenantId,
    string Code,
    string NameAr,
    string? NameEn = null,
    string? Description = null,
    int SortOrder = 0
);

public record WarehouseZoneDto(
    Guid Id,
    Guid WarehouseId,
    string NameAr,
    string? NameEn,
    string? Code,
    bool IsActive,
    int ShelfCount
);

public record AddWarehouseZoneDto(
    string NameAr,
    string? NameEn = null,
    string? Code = null
);

public record WarehouseShelfDto(
    Guid Id,
    Guid ZoneId,
    string NameAr,
    string? NameEn,
    string? Code,
    bool IsActive,
    int BinCount
);

public record AddWarehouseShelfDto(
    string NameAr,
    string? NameEn = null,
    string? Code = null
);

public record WarehouseBinDto(
    Guid Id,
    Guid ShelfId,
    string NameAr,
    string? NameEn,
    string? Code,
    bool IsActive
);

public record AddWarehouseBinDto(
    string NameAr,
    string? NameEn = null,
    string? Code = null
);

public record WarehouseDetailDto(
    Guid Id,
    Guid TenantId,
    Guid? BranchId,
    Guid? CompanyId,
    string NameAr,
    string? NameEn,
    string? Code,
    string? Address,
    string? Phone,
    string? Email,
    string? Notes,
    WarehouseType WarehouseType,
    Guid? WarehouseTypeId,
    Guid? ParentWarehouseId,
    Guid? ManagerUserId,
    Guid? ResponsibleEmployeeId,
    bool AllowPurchase,
    bool AllowSales,
    bool AllowTransfer,
    bool AllowInventoryCount,
    bool AllowManufacturing,
    bool AllowNegativeStock,
    bool AllowReservation,
    bool AllowReceiving,
    bool AllowIssue,
    bool AllowAdjustment,
    bool IsPosWarehouse,
    bool IsDefault,
    bool IsSystem,
    bool UseBins,
    bool IsActive,
    int ZoneCount,
    DateTime CreatedAt,
    List<WarehouseZoneDetailDto> Zones
);

public record WarehouseZoneDetailDto(
    Guid Id,
    Guid WarehouseId,
    string NameAr,
    string? NameEn,
    string? Code,
    bool IsActive,
    List<WarehouseShelfDetailDto> Shelves
);

public record WarehouseShelfDetailDto(
    Guid Id,
    Guid ZoneId,
    string NameAr,
    string? NameEn,
    string? Code,
    bool IsActive,
    List<WarehouseBinDto> Bins
);

// ─── Phase J — Brands / Manufacturers / Attributes / Price Lists ──────────────

public record InventoryBrandDto(
    Guid Id,
    Guid TenantId,
    string Code,
    string NameAr,
    string? NameEn,
    bool IsActive
);

public record UpsertInventoryBrandDto(
    Guid TenantId,
    string NameAr,
    string? NameEn = null,
    string? Code = null
);

public record InventoryManufacturerDto(
    Guid Id,
    Guid TenantId,
    string Code,
    string NameAr,
    string? NameEn,
    string? Country,
    bool IsActive
);

public record UpsertInventoryManufacturerDto(
    Guid TenantId,
    string NameAr,
    string? NameEn = null,
    string? Code = null,
    string? Country = null
);

public record InventoryAttributeValueDto(
    Guid Id,
    Guid AttributeId,
    string ValueAr,
    string? ValueEn,
    int SortOrder
);

public record InventoryAttributeDto(
    Guid Id,
    Guid TenantId,
    string Code,
    string NameAr,
    string? NameEn,
    InventoryAttributeDataType DataType,
    bool IsActive,
    List<InventoryAttributeValueDto> Values
);

public record UpsertInventoryAttributeDto(
    Guid TenantId,
    string NameAr,
    InventoryAttributeDataType DataType = InventoryAttributeDataType.Text,
    string? NameEn = null,
    string? Code = null
);

public record AddInventoryAttributeValueDto(
    string ValueAr,
    string? ValueEn = null,
    int SortOrder = 0
);

public record InventoryPriceListLineDto(
    Guid Id,
    Guid PriceListId,
    Guid InventoryItemId,
    string? InventoryItemNameAr,
    Guid? UnitId,
    decimal UnitPrice
);

public record InventoryPriceListDto(
    Guid Id,
    Guid TenantId,
    string Code,
    string NameAr,
    string? NameEn,
    string Currency,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo,
    bool IsActive,
    int LineCount,
    List<InventoryPriceListLineDto> Lines
);

public record UpsertInventoryPriceListDto(
    Guid TenantId,
    string NameAr,
    string? NameEn = null,
    string? Code = null,
    string Currency = "SAR",
    DateTimeOffset? ValidFrom = null,
    DateTimeOffset? ValidTo = null
);

public record UpsertInventoryPriceListLineDto(
    Guid InventoryItemId,
    decimal UnitPrice,
    Guid? UnitId = null
);

// ─── Supplier DTOs ────────────────────────────────────────────────────────────

// Supplier DTOs moved to SupplierDtos.cs

// ─── Inventory Setting DTOs ───────────────────────────────────────────────────

public record InventoryDocumentNumberSeriesDto(
    Guid Id,
    InventoryDocumentSeriesType DocumentType,
    string Prefix,
    byte NumberLength,
    long NextNumber,
    bool AutoIncrement
);

public record UpsertInventoryDocumentNumberSeriesDto(
    InventoryDocumentSeriesType DocumentType,
    string Prefix,
    byte NumberLength = 6,
    long NextNumber = 1,
    bool AutoIncrement = true
);

public record InventorySettingDto(
    Guid Id,
    Guid TenantId,
    Guid? CompanyId,
    Guid? BranchId,
    Guid? DefaultWarehouseId,
    Guid? DefaultUnitId,
    string? DefaultCurrencyCode,
    bool AutoGenerateItemCode,
    bool EnableMultiWarehouse,
    bool EnableWarehouseHierarchy,
    bool EnableBatchTracking,
    bool EnableSerialTracking,
    bool EnableExpiryTracking,
    bool EnableBarcode,
    bool EnableQrCode,
    InventoryCostingMethod CostingMethod,
    byte CostPrecision,
    bool RoundCost,
    bool AutoRecalculateCost,
    bool AllowNegativeStock,
    bool CheckAvailableQuantity,
    bool EnableReservation,
    bool AutoReleaseReservation,
    bool FreezeDuringCount,
    bool AllowZeroCost,
    bool AllowNegativeCost,
    bool ValidateWarehouseBeforePosting,
    bool AutoIssueRecipe,
    bool RequireApprovalBeforePosting,
    bool AutoPostAfterApproval,
    bool AllowUnpost,
    bool CreateReverseEntry,
    bool LockPostedDocuments,
    bool AllowEditDraft,
    bool AllowDeleteDraft,
    bool EnablePurchasingIntegration,
    bool EnablePosIntegration,
    bool EnableProductionIntegration,
    bool EnableAccountingIntegration,
    bool EnableKitchenIntegration,
    bool EnableDeliveryIntegration,
    bool LowStockAlert,
    bool OutOfStockAlert,
    bool NearExpiryAlert,
    bool ExpiredItemsAlert,
    bool CycleCountReminder,
    bool EmailNotifications,
    bool PushNotifications,
    bool EnableMultiCompany,
    bool EnableMultiBranch,
    bool EnableWarehouseZones,
    bool EnableShelves,
    bool EnableBins,
    bool EnableRfid,
    bool EnableMobileScanner,
    bool IsActive,
    DateTime UpdatedAt,
    List<InventoryDocumentNumberSeriesDto> DocumentSeries
);

public record UpsertInventorySettingDto(
    Guid TenantId,
    Guid? CompanyId = null,
    Guid? BranchId = null,
    Guid? DefaultWarehouseId = null,
    Guid? DefaultUnitId = null,
    string? DefaultCurrencyCode = "SAR",
    bool AutoGenerateItemCode = true,
    bool EnableMultiWarehouse = true,
    bool EnableWarehouseHierarchy = true,
    bool EnableBatchTracking = false,
    bool EnableSerialTracking = false,
    bool EnableExpiryTracking = false,
    bool EnableBarcode = true,
    bool EnableQrCode = false,
    InventoryCostingMethod CostingMethod = InventoryCostingMethod.WeightedAverage,
    byte CostPrecision = 4,
    bool RoundCost = true,
    bool AutoRecalculateCost = true,
    bool AllowNegativeStock = false,
    bool CheckAvailableQuantity = true,
    bool EnableReservation = true,
    bool AutoReleaseReservation = true,
    bool FreezeDuringCount = true,
    bool AllowZeroCost = false,
    bool AllowNegativeCost = false,
    bool ValidateWarehouseBeforePosting = true,
    bool AutoIssueRecipe = true,
    bool RequireApprovalBeforePosting = false,
    bool AutoPostAfterApproval = true,
    bool AllowUnpost = false,
    bool CreateReverseEntry = true,
    bool LockPostedDocuments = true,
    bool AllowEditDraft = true,
    bool AllowDeleteDraft = true,
    bool EnablePurchasingIntegration = true,
    bool EnablePosIntegration = true,
    bool EnableProductionIntegration = true,
    bool EnableAccountingIntegration = true,
    bool EnableKitchenIntegration = true,
    bool EnableDeliveryIntegration = false,
    bool LowStockAlert = true,
    bool OutOfStockAlert = true,
    bool NearExpiryAlert = true,
    bool ExpiredItemsAlert = true,
    bool CycleCountReminder = false,
    bool EmailNotifications = false,
    bool PushNotifications = true,
    bool EnableMultiCompany = false,
    bool EnableMultiBranch = true,
    bool EnableWarehouseZones = true,
    bool EnableShelves = true,
    bool EnableBins = true,
    bool EnableRfid = false,
    bool EnableMobileScanner = true,
    List<UpsertInventoryDocumentNumberSeriesDto>? DocumentSeries = null
);

public record CreateItemDto(Guid Id);
public record UpdateItemDto(Guid Id);
public record UpdateRecipeDto(Guid Id);
public record StockCountDto(
    Guid Id,
    Guid TenantId,
    Guid WarehouseId,
    string CountNumber,
    DateTimeOffset CountDate,
    StockCountStatus Status,
    string? Notes,
    List<StockCountLineDto> Lines
);

public record StockCountLineDto(
    Guid Id,
    Guid InventoryItemId,
    string InventoryItemNameAr,
    Guid UnitId,
    string UnitNameAr,
    decimal ExpectedQuantity,
    decimal ActualQuantity,
    decimal Difference,
    string? BatchNumber
);

public record CreateStockCountDto(
    Guid WarehouseId,
    string CountNumber,
    string? Notes = null
);

public record AddStockCountLineDto(
    Guid InventoryItemId,
    Guid UnitId,
    decimal ExpectedQuantity,
    decimal ActualQuantity,
    string? BatchNumber = null
);

// ─── PurchaseReturn DTOs ──────────────────────────────────────────────────────
public record PurchaseReturnLineDto(
    Guid Id,
    Guid InventoryItemId,
    string? ItemNameAr,
    string? ItemSku,
    Guid UnitId,
    string? UnitNameAr,
    Guid? GoodsReceiptLineId,
    Guid? PurchaseInvoiceLineId,
    decimal OriginalQuantity,
    decimal PreviouslyReturnedQuantity,
    decimal AvailableToReturn,
    decimal ReturnQuantity,
    decimal UnitCost,
    decimal DiscountAmount,
    decimal TaxPercent,
    decimal TaxAmount,
    decimal LineSubTotal,
    decimal LineTotal,
    string? BatchNumber,
    DateTimeOffset? ExpiryDate,
    string? LineReason,
    string? Notes,
    decimal? ProductTemperature,
    bool DestroyItem
);

public record PurchaseReturnDto(
    Guid Id,
    Guid TenantId,
    Guid? BranchId,
    Guid SupplierId,
    string SupplierNameAr,
    Guid WarehouseId,
    string WarehouseNameAr,
    string ReturnNumber,
    DateTimeOffset ReturnDate,
    PurchaseReturnType ReturnType,
    PurchasingDocumentStatus Status,
    byte UnifiedStatusCode,
    Guid? GoodsReceiptId,
    string? GoodsReceiptNumber,
    Guid? PurchaseInvoiceId,
    string? PurchaseInvoiceNumber,
    Guid? ReturnReasonId,
    string? ReturnReasonNameAr,
    string? ReasonNotes,
    string? ReferenceNumber,
    string? Notes,
    string Currency,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    Guid? JournalEntryId,
    Guid? CreditNoteJournalEntryId,
    bool IsCompleted,
    int LineCount,
    IReadOnlyList<PurchaseReturnLineDto> Lines,
    DateTime CreatedAt
);

public record PurchaseReturnReasonDto(
    Guid Id,
    string Code,
    string NameAr,
    string? NameEn,
    int SortOrder,
    bool IsActive
);

// ─── Purchase invoice → return form (single round-trip) ───────────────────────

public record PurchaseInvoiceForReturnHeaderDto(
    Guid Id,
    string InvoiceNumber,
    PurchaseInvoiceKind Kind,
    PurchasingDocumentStatus Status,
    PurchaseInvoicePaymentMode PaymentMode,
    DirectPurchaseNature Nature,
    Guid SupplierId,
    string SupplierNameAr,
    Guid? WarehouseId,
    string? WarehouseNameAr,
    Guid? CostCenterId,
    string? CostCenterNameAr,
    DateOnly InvoiceDate,
    DateOnly? DueDate,
    string Currency,
    decimal ExchangeRate,
    string? SupplierInvoiceNumber,
    string? ExternalReference,
    string? Notes,
    Guid? ApAccountId,
    string? ApAccountNameAr,
    decimal DiscountAmount,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    bool CanCreateReturn,
    string? BlockReason,
    string? BlockReasonCode = null
);

public record PurchaseInvoiceForReturnLineDto(
    Guid PurchaseInvoiceLineId,
    Guid InventoryItemId,
    string? ItemNameAr,
    string? ItemSku,
    string? Description,
    Guid UnitId,
    string? UnitNameAr,
    Guid? WarehouseId,
    string? WarehouseNameAr,
    decimal OriginalQuantity,
    decimal PreviouslyReturnedQuantity,
    decimal RemainingQuantity,
    decimal ReturnQuantity,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal DiscountAmount,
    decimal TaxPercent,
    decimal TaxAmount,
    decimal LineSubTotal,
    decimal LineTotal,
    bool IsDisabled
);

public record PurchaseInvoiceForReturnTaxDto(
    decimal TaxPercent,
    decimal TaxableAmount,
    decimal TaxAmount
);

public record PurchaseInvoiceForReturnDto(
    PurchaseInvoiceForReturnHeaderDto Header,
    IReadOnlyList<PurchaseInvoiceForReturnLineDto> Items,
    IReadOnlyList<PurchaseInvoiceForReturnTaxDto> Taxes,
    decimal TotalRemainingQuantity,
    decimal InvoiceTotalAmount
);

public record CreatePurchaseReturnLineInputDto(
    Guid InventoryItemId,
    Guid UnitId,
    decimal OriginalQuantity,
    decimal PreviouslyReturnedQuantity,
    decimal ReturnQuantity,
    decimal UnitCost,
    decimal DiscountAmount = 0,
    decimal TaxPercent = 0,
    decimal TaxAmount = 0,
    Guid? GoodsReceiptLineId = null,
    Guid? PurchaseInvoiceLineId = null,
    string? BatchNumber = null,
    DateTimeOffset? ExpiryDate = null,
    string? LineReason = null,
    string? Notes = null,
    decimal? ProductTemperature = null,
    bool DestroyItem = false
);

public record CreatePurchaseReturnDto(
    PurchaseReturnType ReturnType,
    Guid WarehouseId,
    string? ReturnNumber = null,
    Guid? GoodsReceiptId = null,
    Guid? PurchaseInvoiceId = null,
    Guid? SupplierId = null,
    Guid? BranchId = null,
    DateTimeOffset? ReturnDate = null,
    Guid? ReturnReasonId = null,
    string? ReasonNotes = null,
    string? ReferenceNumber = null,
    string? Notes = null,
    string Currency = "SAR",
    IReadOnlyList<CreatePurchaseReturnLineInputDto>? Lines = null
);

public record UpdatePurchaseReturnDto(
    DateTimeOffset ReturnDate,
    Guid? ReturnReasonId = null,
    string? ReasonNotes = null,
    string? ReferenceNumber = null,
    string? Notes = null,
    IReadOnlyList<CreatePurchaseReturnLineInputDto>? Lines = null
);

/// <summary>Legacy add-line payload.</summary>
public record AddPurchaseReturnLineDto(
    Guid InventoryItemId,
    Guid UnitId,
    decimal ReturnQuantity,
    decimal UnitCost,
    string? Notes = null
);

// ─── InventoryReservation DTOs ────────────────────────────────────────────────
public record InventoryReservationDto(
    Guid Id,
    Guid TenantId,
    Guid WarehouseId,
    Guid InventoryItemId,
    string InventoryItemNameAr,
    decimal ReservedQuantity,
    string SourceDocument,
    ReservationStatus Status,
    DateTimeOffset? ExpirationDate
);

// ─── Product Details (Phase D) ────────────────────────────────────────────────

public record WarehouseStockBalanceDto(
    Guid WarehouseId,
    string WarehouseNameAr,
    string? WarehouseCode,
    decimal OnHand,
    decimal Reserved,
    decimal Available,
    decimal Ordered,
    decimal Incoming
);

public record ItemStockMovementDto(
    Guid MovementId,
    Guid TransactionId,
    DateTime OccurredAt,
    string TransactionType,
    string? ReferenceDocumentNumber,
    Guid WarehouseId,
    string WarehouseNameAr,
    decimal QuantityChange,
    decimal UnitCost,
    decimal TotalCost
);

public record ItemPurchaseHistoryDto(
    Guid PurchaseOrderId,
    string PoNumber,
    Guid SupplierId,
    string SupplierNameAr,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    DateTime OrderDate,
    string Status
);

public record ItemSalesHistoryDto(
    Guid SalesOrderId,
    string OrderNumber,
    Guid? CustomerId,
    string? CustomerName,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    DateTime OrderDate,
    string Status
);

public record ReserveStockDto(
    Guid WarehouseId,
    Guid InventoryItemId,
    decimal ReservedQuantity,
    string SourceDocument,
    DateTimeOffset? ExpirationDate = null
);
