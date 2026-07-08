using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Domain.Enums;
using MediatR;

namespace GastroErp.Application.Features.Inventory.Queries;

// ─── InventoryCategory ────────────────────────────────────────────────────────
public record GetInventoryCategoriesQuery(Guid TenantId, bool? IsActive = null, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<InventoryCategoryDto>>;
public record GetInventoryCategoryByIdQuery(Guid Id) : IRequest<Result<InventoryCategoryDto>>;

// ─── InventoryUnit ────────────────────────────────────────────────────────────
public record GetInventoryUnitsQuery(Guid TenantId, bool? IsActive = null) : IRequest<Result<List<InventoryUnitDto>>>;

// ─── InventoryItem ────────────────────────────────────────────────────────────
public record GetInventoryItemsQuery(Guid TenantId, Guid? CategoryId = null, bool? IsActive = null, string? SearchTerm = null, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<InventoryItemDto>>;
public record GetInventoryItemByIdQuery(Guid Id) : IRequest<Result<InventoryItemDto>>;
public record GetLowStockItemsQuery(Guid TenantId, Guid? WarehouseId = null) : IRequest<Result<List<InventoryItemDto>>>;

// ─── Warehouse ────────────────────────────────────────────────────────────────
public record GetWarehousesQuery(Guid TenantId, Guid? BranchId = null, bool? IsActive = null, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<WarehouseDto>>;
public record GetWarehouseByIdQuery(Guid Id) : IRequest<Result<WarehouseDto>>;

// ─── Supplier ─────────────────────────────────────────────────────────────────
public record GetSuppliersQuery(Guid TenantId, bool? IsActive = null, bool? IsPreferred = null, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<SupplierDto>>;
public record GetSupplierByIdQuery(Guid Id) : IRequest<Result<SupplierDto>>;

// ─── PurchaseOrder ────────────────────────────────────────────────────────────
public record GetPurchaseOrdersQuery(Guid TenantId, Guid? SupplierId = null, PurchaseOrderStatus? Status = null, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<PurchaseOrderDto>>;
public record GetPurchaseOrderByIdQuery(Guid Id) : IRequest<Result<PurchaseOrderDto>>;

// ─── GoodsReceipt ─────────────────────────────────────────────────────────────
public record GetGoodsReceiptsQuery(Guid TenantId, Guid? SupplierId = null, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<GoodsReceiptDto>>;
public record GetGoodsReceiptByIdQuery(Guid Id) : IRequest<Result<GoodsReceiptDto>>;

// ─── StockTransfer ────────────────────────────────────────────────────────────
public record GetStockTransfersQuery(Guid TenantId, Guid? SourceWarehouseId = null, Guid? DestinationWarehouseId = null, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<StockTransferDto>>;

// ─── StockAdjustment ─────────────────────────────────────────────────────────
public record GetStockAdjustmentsQuery(Guid TenantId, Guid? WarehouseId = null, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<StockAdjustmentDto>>;

// ─── WasteRecord ──────────────────────────────────────────────────────────────
public record GetWasteRecordsQuery(Guid TenantId, Guid? WarehouseId = null, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<WasteRecordDto>>;

// ─── Recipe ───────────────────────────────────────────────────────────────────
public record GetRecipesQuery(Guid TenantId, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<RecipeDto>>;
public record GetRecipesByProductIdQuery(Guid ProductId) : IRequest<Result<List<RecipeDto>>>;
public record GetRecipeByIdQuery(Guid Id) : IRequest<Result<RecipeDto>>;

// ─── InventoryTransaction ─────────────────────────────────────────────────────
public record GetInventoryTransactionsQuery(Guid TenantId, Guid? WarehouseId = null, int PageNumber = 1, int PageSize = 50) : IRequest<PagedResult<InventoryTransactionDto>>;

// ─── StockCount ───────────────────────────────────────────────────────────────
public record GetStockCountsQuery(Guid TenantId, Guid? WarehouseId = null, StockCountStatus? Status = null, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<StockCountDto>>;
public record GetStockCountByIdQuery(Guid Id) : IRequest<Result<StockCountDto>>;

// ─── PurchaseReturn ───────────────────────────────────────────────────────────
public record GetPurchaseReturnsQuery(Guid TenantId, Guid? SupplierId = null, Guid? WarehouseId = null, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<PurchaseReturnDto>>;
public record GetPurchaseReturnByIdQuery(Guid Id) : IRequest<Result<PurchaseReturnDto>>;

// ─── InventoryReservation ─────────────────────────────────────────────────────
public record GetInventoryReservationsQuery(Guid TenantId, Guid? WarehouseId = null, Guid? InventoryItemId = null, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<InventoryReservationDto>>;
