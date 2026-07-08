using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Inventory.Commands;

// ─── InventoryCategory Commands ───────────────────────────────────────────────
public record CreateInventoryCategoryCommand(CreateInventoryCategoryDto Dto) : IRequest<Result<InventoryCategoryDto>>;
public record UpdateInventoryCategoryCommand(Guid Id, UpdateInventoryCategoryDto Dto) : IRequest<Result>;
public record DeactivateInventoryCategoryCommand(Guid Id) : IRequest<Result>;
public record ActivateInventoryCategoryCommand(Guid Id) : IRequest<Result>;

// ─── InventoryUnit Commands ───────────────────────────────────────────────────
public record CreateInventoryUnitCommand(CreateInventoryUnitDto Dto) : IRequest<Result<InventoryUnitDto>>;
public record CreateUnitConversionCommand(CreateUnitConversionDto Dto) : IRequest<Result<UnitConversionDto>>;

// ─── InventoryItem Commands ───────────────────────────────────────────────────
public record CreateInventoryItemCommand(CreateInventoryItemDto Dto) : IRequest<Result<InventoryItemDto>>;
public record UpdateInventoryItemCommand(Guid Id, UpdateInventoryItemDto Dto) : IRequest<Result>;
public record SetInventoryItemReorderInfoCommand(Guid Id, SetReorderInfoDto Dto) : IRequest<Result>;
public record SetInventoryItemUnitsCommand(Guid Id, Guid? PurchaseUnitId, Guid? RecipeUnitId) : IRequest<Result>;
public record SetInventoryItemCategoryCommand(Guid Id, Guid CategoryId) : IRequest<Result>;
public record DeactivateInventoryItemCommand(Guid Id) : IRequest<Result>;
public record ActivateInventoryItemCommand(Guid Id) : IRequest<Result>;

// ─── Warehouse Commands ───────────────────────────────────────────────────────
public record CreateWarehouseCommand(CreateWarehouseDto Dto) : IRequest<Result<WarehouseDto>>;
public record UpdateWarehouseCommand(Guid Id, UpdateWarehouseDto Dto) : IRequest<Result>;
public record AddWarehouseZoneCommand(Guid WarehouseId, AddWarehouseZoneDto Dto) : IRequest<Result<WarehouseZoneDto>>;
public record RemoveWarehouseZoneCommand(Guid WarehouseId, Guid ZoneId) : IRequest<Result>;
public record DeactivateWarehouseCommand(Guid Id) : IRequest<Result>;
public record ActivateWarehouseCommand(Guid Id) : IRequest<Result>;

// ─── Supplier Commands ────────────────────────────────────────────────────────
public record CreateSupplierCommand(CreateSupplierDto Dto) : IRequest<Result<SupplierDto>>;
public record UpdateSupplierCommand(Guid Id, UpdateSupplierDto Dto) : IRequest<Result>;
public record UpdateSupplierFinancialCommand(Guid Id, UpdateSupplierFinancialDto Dto) : IRequest<Result>;
public record SetSupplierRatingCommand(Guid Id, int Rating) : IRequest<Result>;
public record SetSupplierPreferredCommand(Guid Id, bool IsPreferred) : IRequest<Result>;
public record AddSupplierContactCommand(Guid SupplierId, AddSupplierContactDto Dto) : IRequest<Result>;
public record RemoveSupplierContactCommand(Guid SupplierId, Guid ContactId) : IRequest<Result>;
public record DeactivateSupplierCommand(Guid Id) : IRequest<Result>;
public record ActivateSupplierCommand(Guid Id) : IRequest<Result>;

// ─── PurchaseOrder Commands ───────────────────────────────────────────────────
public record CreatePurchaseOrderCommand(CreatePurchaseOrderDto Dto) : IRequest<Result<PurchaseOrderDto>>;
public record AddPurchaseOrderLineCommand(Guid PurchaseOrderId, AddPurchaseOrderLineDto Dto) : IRequest<Result>;
public record RemovePurchaseOrderLineCommand(Guid PurchaseOrderId, Guid LineId) : IRequest<Result>;
public record ApprovePurchaseOrderCommand(Guid Id) : IRequest<Result>;
public record SubmitPurchaseOrderForApprovalCommand(Guid Id) : IRequest<Result>;
public record SendPurchaseOrderToSupplierCommand(Guid Id) : IRequest<Result>;
public record CancelPurchaseOrderCommand(Guid Id) : IRequest<Result>;

// ─── GoodsReceipt Commands ────────────────────────────────────────────────────
public record CreateGoodsReceiptCommand(CreateGoodsReceiptDto Dto) : IRequest<Result<GoodsReceiptDto>>;
public record AddGoodsReceiptLineCommand(Guid GoodsReceiptId, AddGoodsReceiptLineDto Dto) : IRequest<Result>;
public record ConfirmGoodsReceiptCommand(Guid Id) : IRequest<Result>;

// ─── StockTransfer Commands ───────────────────────────────────────────────────
public record CreateStockTransferCommand(CreateStockTransferDto Dto) : IRequest<Result<StockTransferDto>>;
public record AddTransferLineCommand(Guid TransferId, AddTransferLineDto Dto) : IRequest<Result>;
public record CompleteStockTransferCommand(Guid Id) : IRequest<Result>;
public record CancelStockTransferCommand(Guid Id) : IRequest<Result>;

// ─── StockAdjustment Commands ─────────────────────────────────────────────────
public record CreateStockAdjustmentCommand(CreateStockAdjustmentDto Dto) : IRequest<Result<StockAdjustmentDto>>;
public record ConfirmStockAdjustmentCommand(Guid Id) : IRequest<Result>;

// ─── WasteRecord Commands ─────────────────────────────────────────────────────
public record CreateWasteRecordCommand(CreateWasteRecordDto Dto) : IRequest<Result<WasteRecordDto>>;
public record ConfirmWasteRecordCommand(Guid Id) : IRequest<Result>;

// ─── Recipe Commands ──────────────────────────────────────────────────────────
public record CreateRecipeCommand(CreateRecipeDto Dto) : IRequest<Result<RecipeDto>>;
public record UpdateRecipeCommand(Guid Id, UpdateRecipeDto Dto) : IRequest<Result>;
public record AddRecipeIngredientCommand(Guid RecipeId, AddRecipeIngredientDto Dto) : IRequest<Result>;
public record RemoveRecipeIngredientCommand(Guid RecipeId, Guid IngredientId) : IRequest<Result>;
public record DeactivateRecipeCommand(Guid Id) : IRequest<Result>;
public record ActivateRecipeCommand(Guid Id) : IRequest<Result>;

// ─── InventorySetting Commands ────────────────────────────────────────────────
public record UpsertInventorySettingCommand(UpsertInventorySettingDto Dto) : IRequest<Result<InventorySettingDto>>;

// ─── StockCount Commands ──────────────────────────────────────────────────────
public record CreateStockCountCommand(Guid TenantId, CreateStockCountDto Dto) : IRequest<Result<StockCountDto>>;
public record AddStockCountLineCommand(Guid StockCountId, AddStockCountLineDto Dto) : IRequest<Result>;
public record FreezeInventoryCommand(Guid Id) : IRequest<Result>;
public record ApproveStockCountCommand(Guid Id) : IRequest<Result>;

// ─── PurchaseReturn Commands ──────────────────────────────────────────────────
public record CreatePurchaseReturnCommand(Guid TenantId, CreatePurchaseReturnDto Dto) : IRequest<Result<PurchaseReturnDto>>;
public record AddPurchaseReturnLineCommand(Guid PurchaseReturnId, AddPurchaseReturnLineDto Dto) : IRequest<Result>;
public record ApprovePurchaseReturnCommand(Guid Id) : IRequest<Result>;

// ─── InventoryReservation Commands ────────────────────────────────────────────
public record ReserveStockCommand(Guid TenantId, ReserveStockDto Dto) : IRequest<Result<InventoryReservationDto>>;
public record ReleaseStockCommand(Guid Id) : IRequest<Result>;
public record ExpireStockReservationCommand(Guid Id) : IRequest<Result>;

// ─── Additional PurchaseOrder Commands ─────────────────────────────────────────
public record RejectPurchaseOrderCommand(Guid Id) : IRequest<Result>;
public record ClosePurchaseOrderCommand(Guid Id) : IRequest<Result>;
