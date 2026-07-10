using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Inventory.DTOs;

// ─── InventoryCategory DTOs ───────────────────────────────────────────────────

public record InventoryCategoryDto(
    Guid Id,
    Guid TenantId,
    string NameAr,
    string? NameEn,
    string? Color,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateInventoryCategoryDto(
    Guid TenantId,
    string NameAr,
    string? NameEn = null,
    string? Color = null
);

public record UpdateInventoryCategoryDto(
    string NameAr,
    string? NameEn,
    string? Color
);

// ─── InventoryUnit DTOs ───────────────────────────────────────────────────────

public record InventoryUnitDto(
    Guid Id,
    Guid TenantId,
    string NameAr,
    string? NameEn,
    string? Symbol,
    bool IsActive
);

public record CreateInventoryUnitDto(
    Guid TenantId,
    string NameAr,
    string? NameEn = null,
    string? Symbol = null
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
    string NameAr,
    string? NameEn,
    string? Code,
    string? Address,
    bool IsActive,
    int ZoneCount,
    DateTime CreatedAt
);

public record CreateWarehouseDto(
    Guid TenantId,
    string NameAr,
    string? NameEn = null,
    string? Code = null,
    Guid? BranchId = null
);

public record UpdateWarehouseDto(
    string NameAr,
    string? NameEn,
    string? Code,
    string? Address
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
    string Code,
    bool IsActive
);

// ─── Supplier DTOs ────────────────────────────────────────────────────────────

public record SupplierDto(
    Guid Id,
    Guid TenantId,
    string NameAr,
    string? NameEn,
    string? TaxNumber,
    string? PaymentTerms,
    decimal CreditLimit,
    string Currency,
    int LeadTimeDays,
    bool IsPreferred,
    int Rating,
    bool IsActive,
    int ContactCount,
    DateTime CreatedAt
);

public record CreateSupplierDto(
    Guid TenantId,
    string NameAr,
    string? NameEn = null,
    string Currency = "SAR"
);

public record UpdateSupplierFinancialDto(
    string? TaxNumber,
    string? PaymentTerms,
    decimal CreditLimit,
    int LeadTimeDays
);

public record SupplierContactDto(
    Guid Id,
    string NameAr,
    string? NameEn,
    string PhoneNumber,
    string? Email,
    string? Position
);

public record AddSupplierContactDto(
    string NameAr,
    string PhoneNumber,
    string? Email = null,
    string? Position = null,
    string? NameEn = null
);

// ─── Inventory Setting DTOs ───────────────────────────────────────────────────

public record InventorySettingDto(
    Guid Id,
    Guid TenantId,
    string SettingKey,
    string SettingValue,
    string? Description,
    DateTime UpdatedAt
);

public record UpsertInventorySettingDto(
    Guid TenantId,
    string SettingKey,
    string SettingValue,
    string? Description = null
);

public record CreateItemDto(Guid Id);
public record UpdateItemDto(Guid Id);
public record UpdateRecipeDto(Guid Id);
public record UpdateSupplierDto(string NameAr, string? NameEn = null, string Currency = "SAR");
public record CreatePurchaseDto(Guid Id);

// ─── StockCount DTOs ──────────────────────────────────────────────────────────
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
public record PurchaseReturnDto(
    Guid Id,
    Guid TenantId,
    Guid SupplierId,
    Guid WarehouseId,
    Guid? GoodsReceiptId,
    string ReturnNumber,
    DateTimeOffset ReturnDate,
    string? Reason,
    bool IsCompleted,
    List<PurchaseReturnLineDto> Lines
);

public record PurchaseReturnLineDto(
    Guid Id,
    Guid InventoryItemId,
    string InventoryItemNameAr,
    Guid UnitId,
    string UnitNameAr,
    decimal ReturnQuantity,
    decimal UnitCost,
    string? Notes
);

public record CreatePurchaseReturnDto(
    Guid SupplierId,
    Guid WarehouseId,
    string ReturnNumber,
    Guid? GoodsReceiptId = null,
    string? Reason = null
);

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

public record ReserveStockDto(
    Guid WarehouseId,
    Guid InventoryItemId,
    decimal ReservedQuantity,
    string SourceDocument,
    DateTimeOffset? ExpirationDate = null
);
