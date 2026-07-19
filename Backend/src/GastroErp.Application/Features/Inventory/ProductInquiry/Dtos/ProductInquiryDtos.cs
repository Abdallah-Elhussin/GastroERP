namespace GastroErp.Application.Features.Inventory.ProductInquiry.Dtos;

public sealed record ProductInquiryListItemDto(
    Guid Id,
    string? Sku,
    string? Barcode,
    string NameAr,
    string? NameEn,
    string? CategoryNameAr,
    string? ItemTypeNameAr,
    string? UnitNameAr,
    decimal? SellingPrice,
    decimal? LastPurchasePrice,
    decimal TotalOnHand,
    bool IsActive,
    bool IsInventory,
    InventoryItemKindDto ItemKind);

public enum InventoryItemKindDto : byte
{
    Raw = 1,
    Manufactured = 2
}

public sealed record ProductInquiryDetailDto(
    Guid Id,
    ProductInquiryGeneralDto General,
    IReadOnlyList<ProductInquiryWarehouseStockDto> Warehouses,
    ProductInquiryCostDto Cost,
    ProductInquirySalesDto Sales,
    ProductInquiryPurchaseDto Purchase,
    IReadOnlyList<ProductInquiryPriceRowDto> Prices,
    IReadOnlyList<ProductInquiryReservationDto> Reservations,
    IReadOnlyList<ProductInquiryBatchDto> Batches,
    IReadOnlyList<ProductInquiryMovementDto> RecentMovements,
    ProductInquiryRecipeDto Recipe,
    ProductInquirySupplierDto Supplier,
    IReadOnlyList<ProductInquiryBranchStockDto> Branches,
    ProductInquiryAnalyticsDto Analytics);

public sealed record ProductInquiryGeneralDto(
    string? Sku,
    string NameAr,
    string? NameEn,
    string? Barcode,
    string? CategoryNameAr,
    string? ItemTypeNameAr,
    string? BaseUnitNameAr,
    bool IsActive,
    bool IsInventory,
    string ItemKind);

public sealed record ProductInquiryWarehouseStockDto(
    Guid WarehouseId,
    string WarehouseNameAr,
    string? WarehouseCode,
    Guid? BranchId,
    decimal OnHand,
    decimal Reserved,
    decimal Available);

public sealed record ProductInquiryCostDto(
    decimal AverageCost,
    decimal LastPurchaseCost,
    decimal? StandardCost,
    string CostingMethod,
    bool CanView);

public sealed record ProductInquirySalesDto(
    decimal? LastSalePrice,
    decimal? DefaultPrice,
    DateTimeOffset? LastSaleAt,
    string? LastCustomerName,
    string? LastOrderNumber);

public sealed record ProductInquiryPurchaseDto(
    decimal? LastPurchasePrice,
    string? LastSupplierName,
    string? LastDocumentNumber,
    DateTimeOffset? LastPurchaseAt);

public sealed record ProductInquiryPriceRowDto(
    string PriceListNameAr,
    string? UnitNameAr,
    decimal SellingPrice,
    DateTimeOffset StartDate,
    DateTimeOffset? EndDate,
    bool IsActive);

public sealed record ProductInquiryReservationDto(
    Guid Id,
    string WarehouseNameAr,
    decimal ReservedQuantity,
    string SourceDocument,
    string Status,
    DateTimeOffset? ExpirationDate);

public sealed record ProductInquiryBatchDto(
    Guid Id,
    string BatchNumber,
    string? LotNumber,
    DateTimeOffset? ExpirationDate,
    string Status,
    decimal Quantity);

public sealed record ProductInquiryMovementDto(
    Guid MovementId,
    DateTimeOffset OccurredAt,
    string TransactionType,
    decimal QuantityChange,
    string? ReferenceDocumentNumber,
    string WarehouseNameAr);

public sealed record ProductInquiryRecipeDto(
    bool HasRecipe,
    int IngredientCount,
    decimal? RecipeCost,
    string? RecipeNameAr);

public sealed record ProductInquirySupplierDto(
    string? PrimarySupplierName,
    string? LastSupplierName,
    int? LeadTimeDays,
    decimal? LastPrice,
    bool CanView);

public sealed record ProductInquiryBranchStockDto(
    Guid BranchId,
    string BranchNameAr,
    decimal Quantity);

public sealed record ProductInquiryAnalyticsDto(
    decimal TotalOnHand,
    decimal InventoryValue,
    decimal AverageMonthlySales,
    decimal AverageMonthlyConsumption,
    decimal? DaysOfCover,
    decimal ReorderLevel,
    string StockStatus);
