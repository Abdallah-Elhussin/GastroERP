using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Inventory.Commands;

// ─── InventoryCategory Commands ───────────────────────────────────────────────
public record CreateInventoryCategoryCommand(CreateInventoryCategoryDto Dto) : IRequest<Result<InventoryCategoryDto>>;
public record UpdateInventoryCategoryCommand(Guid Id, UpdateInventoryCategoryDto Dto) : IRequest<Result>;
public record DeactivateInventoryCategoryCommand(Guid Id) : IRequest<Result>;
public record ActivateInventoryCategoryCommand(Guid Id) : IRequest<Result>;
public record DeleteInventoryCategoryCommand(Guid Id) : IRequest<Result>;

// ─── InventoryUnit Commands ───────────────────────────────────────────────────
public record CreateInventoryUnitCommand(CreateInventoryUnitDto Dto) : IRequest<Result<InventoryUnitDto>>;
public record UpdateInventoryUnitCommand(Guid Id, UpdateInventoryUnitDto Dto) : IRequest<Result>;
public record ActivateInventoryUnitCommand(Guid Id) : IRequest<Result>;
public record DeactivateInventoryUnitCommand(Guid Id) : IRequest<Result>;
public record DeleteInventoryUnitCommand(Guid Id) : IRequest<Result>;
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
public record AddWarehouseShelfCommand(Guid WarehouseId, Guid ZoneId, AddWarehouseShelfDto Dto) : IRequest<Result<WarehouseShelfDto>>;
public record RemoveWarehouseShelfCommand(Guid WarehouseId, Guid ZoneId, Guid ShelfId) : IRequest<Result>;
public record AddWarehouseBinCommand(Guid WarehouseId, Guid ZoneId, Guid ShelfId, AddWarehouseBinDto Dto) : IRequest<Result<WarehouseBinDto>>;
public record RemoveWarehouseBinCommand(Guid WarehouseId, Guid ZoneId, Guid ShelfId, Guid BinId) : IRequest<Result>;
public record DeactivateWarehouseCommand(Guid Id) : IRequest<Result>;
public record ActivateWarehouseCommand(Guid Id) : IRequest<Result>;
public record DeleteWarehouseCommand(Guid Id) : IRequest<Result>;
public record CreateWarehouseTypeDefinitionCommand(UpsertWarehouseTypeDefinitionDto Dto) : IRequest<Result<WarehouseTypeDefinitionDto>>;
public record UpdateWarehouseTypeDefinitionCommand(Guid Id, UpsertWarehouseTypeDefinitionDto Dto) : IRequest<Result>;
public record ActivateWarehouseTypeDefinitionCommand(Guid Id, bool IsActive) : IRequest<Result>;

// ─── Phase J Master Data Commands ─────────────────────────────────────────────
public record CreateInventoryBrandCommand(UpsertInventoryBrandDto Dto) : IRequest<Result<InventoryBrandDto>>;
public record UpdateInventoryBrandCommand(Guid Id, UpsertInventoryBrandDto Dto) : IRequest<Result>;
public record ActivateInventoryBrandCommand(Guid Id) : IRequest<Result>;
public record DeactivateInventoryBrandCommand(Guid Id) : IRequest<Result>;

public record CreateInventoryManufacturerCommand(UpsertInventoryManufacturerDto Dto) : IRequest<Result<InventoryManufacturerDto>>;
public record UpdateInventoryManufacturerCommand(Guid Id, UpsertInventoryManufacturerDto Dto) : IRequest<Result>;
public record ActivateInventoryManufacturerCommand(Guid Id) : IRequest<Result>;
public record DeactivateInventoryManufacturerCommand(Guid Id) : IRequest<Result>;

public record CreateInventoryAttributeCommand(UpsertInventoryAttributeDto Dto) : IRequest<Result<InventoryAttributeDto>>;
public record UpdateInventoryAttributeCommand(Guid Id, UpsertInventoryAttributeDto Dto) : IRequest<Result>;
public record ActivateInventoryAttributeCommand(Guid Id) : IRequest<Result>;
public record DeactivateInventoryAttributeCommand(Guid Id) : IRequest<Result>;
public record AddInventoryAttributeValueCommand(Guid AttributeId, AddInventoryAttributeValueDto Dto) : IRequest<Result<InventoryAttributeValueDto>>;
public record RemoveInventoryAttributeValueCommand(Guid AttributeId, Guid ValueId) : IRequest<Result>;

public record CreateInventoryPriceListCommand(UpsertInventoryPriceListDto Dto) : IRequest<Result<InventoryPriceListDto>>;
public record UpdateInventoryPriceListCommand(Guid Id, UpsertInventoryPriceListDto Dto) : IRequest<Result>;
public record ActivateInventoryPriceListCommand(Guid Id) : IRequest<Result>;
public record DeactivateInventoryPriceListCommand(Guid Id) : IRequest<Result>;
public record UpsertInventoryPriceListLineCommand(Guid PriceListId, UpsertInventoryPriceListLineDto Dto) : IRequest<Result<InventoryPriceListLineDto>>;
public record RemoveInventoryPriceListLineCommand(Guid PriceListId, Guid LineId) : IRequest<Result>;

// ─── Supplier Commands ────────────────────────────────────────────────────────
public record CreateSupplierCommand(CreateSupplierDto Dto) : IRequest<Result<SupplierDto>>;
public record UpdateSupplierCommand(Guid Id, UpdateSupplierDto Dto) : IRequest<Result>;
public record UpsertSupplierMasterCommand(Guid Id, UpsertSupplierMasterDto Dto) : IRequest<Result<SupplierDto>>;
public record UpdateSupplierFinancialCommand(Guid Id, UpdateSupplierFinancialDto Dto) : IRequest<Result>;
public record SetSupplierRatingCommand(Guid Id, int Rating) : IRequest<Result>;
public record SetSupplierPreferredCommand(Guid Id, bool IsPreferred) : IRequest<Result>;
public record AddSupplierContactCommand(Guid SupplierId, AddSupplierContactDto Dto) : IRequest<Result>;
public record RemoveSupplierContactCommand(Guid SupplierId, Guid ContactId) : IRequest<Result>;
public record DeactivateSupplierCommand(Guid Id) : IRequest<Result>;
public record ActivateSupplierCommand(Guid Id) : IRequest<Result>;
public record BlacklistSupplierCommand(Guid Id, string? Reason) : IRequest<Result>;
public record ClearSupplierBlacklistCommand(Guid Id) : IRequest<Result>;
public record DeleteSupplierCommand(Guid Id) : IRequest<Result>;
public record SetSupplierDefaultPaymentMethodCommand(Guid SupplierId, Guid PaymentMethodId) : IRequest<Result>;
public record RemoveSupplierPaymentMethodCommand(Guid SupplierId, Guid PaymentMethodId) : IRequest<Result>;
public record RemoveSupplierAttachmentCommand(Guid SupplierId, Guid AttachmentId) : IRequest<Result>;

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
public record UpdateGoodsReceiptCommand(Guid Id, UpdateGoodsReceiptDto Dto) : IRequest<Result<GoodsReceiptDto>>;
public record AddGoodsReceiptLineCommand(Guid GoodsReceiptId, AddGoodsReceiptLineDto Dto) : IRequest<Result>;
public record ApproveGoodsReceiptCommand(Guid Id) : IRequest<Result>;
public record CancelGoodsReceiptCommand(Guid Id) : IRequest<Result>;
public record ConfirmGoodsReceiptCommand(Guid Id) : IRequest<Result>;
public record UnpostGoodsReceiptCommand(Guid Id) : IRequest<Result>;

// ─── StockTransfer Commands ───────────────────────────────────────────────────
// ─── StockTransfer Commands ───────────────────────────────────────────────────
public record CreateStockTransferCommand(CreateStockTransferDto Dto) : IRequest<Result<StockTransferDto>>;
public record UpdateStockTransferCommand(Guid Id, UpdateStockTransferDto Dto) : IRequest<Result<StockTransferDto>>;
public record AddTransferLineCommand(Guid TransferId, AddTransferLineDto Dto) : IRequest<Result>;
public record ApproveStockTransferCommand(Guid Id) : IRequest<Result>;
public record UnapproveStockTransferCommand(Guid Id) : IRequest<Result>;
public record ShipStockTransferCommand(Guid Id) : IRequest<Result>;
public record CompleteStockTransferCommand(Guid Id) : IRequest<Result>;
public record CancelStockTransferCommand(Guid Id) : IRequest<Result>;
public record DeleteStockTransferCommand(Guid Id, Guid TenantId) : IRequest<Result>;
public record GenerateStockTransferNumberCommand(Guid TenantId) : IRequest<Result<string>>;

// ─── GoodsIssue Commands ──────────────────────────────────────────────────────
public record CreateGoodsIssueCommand(CreateGoodsIssueDto Dto) : IRequest<Result<GoodsIssueDto>>;
public record UpdateGoodsIssueCommand(Guid Id, UpdateGoodsIssueDto Dto) : IRequest<Result<GoodsIssueDto>>;
public record AddGoodsIssueLineCommand(Guid GoodsIssueId, AddGoodsIssueLineDto Dto) : IRequest<Result>;
public record ApproveGoodsIssueCommand(Guid Id) : IRequest<Result>;
public record UnapproveGoodsIssueCommand(Guid Id) : IRequest<Result>;
public record PostGoodsIssueCommand(Guid Id) : IRequest<Result>;
public record CancelGoodsIssueCommand(Guid Id) : IRequest<Result>;
public record ConfirmGoodsIssueCommand(Guid Id) : IRequest<Result>;
public record GenerateGoodsIssueNumberCommand(Guid TenantId) : IRequest<Result<string>>;

// ─── IssueDestination Commands ────────────────────────────────────────────────
public record CreateIssueDestinationCommand(CreateIssueDestinationDto Dto) : IRequest<Result<IssueDestinationDto>>;
public record UpdateIssueDestinationCommand(Guid Id, UpdateIssueDestinationDto Dto) : IRequest<Result<IssueDestinationDto>>;
public record DeleteIssueDestinationCommand(Guid Id, Guid TenantId) : IRequest<Result>;
public record ActivateIssueDestinationCommand(Guid Id, Guid TenantId) : IRequest<Result>;
public record DeactivateIssueDestinationCommand(Guid Id, Guid TenantId) : IRequest<Result>;

// ─── OpeningBalance Commands ──────────────────────────────────────────────────
public record CreateOpeningBalanceCommand(CreateOpeningBalanceDto Dto) : IRequest<Result<OpeningBalanceDto>>;
public record UpdateOpeningBalanceCommand(Guid Id, UpdateOpeningBalanceDto Dto) : IRequest<Result<OpeningBalanceDto>>;
public record AddOpeningBalanceLineCommand(Guid OpeningBalanceId, AddOpeningBalanceLineDto Dto) : IRequest<Result>;
public record ApproveOpeningBalanceCommand(Guid Id) : IRequest<Result>;
public record UnapproveOpeningBalanceCommand(Guid Id) : IRequest<Result>;
public record PostOpeningBalanceCommand(Guid Id) : IRequest<Result>;
public record GenerateOpeningBalanceNumberCommand(Guid TenantId) : IRequest<Result<string>>;

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
public record UpdateInventorySettingsCommand(UpsertInventorySettingDto Dto) : IRequest<Result<InventorySettingDto>>;
public record ResetInventorySettingsCommand(Guid TenantId, Guid? BranchId = null, Guid? CompanyId = null)
    : IRequest<Result<InventorySettingDto>>;

// ─── StockCount Commands ──────────────────────────────────────────────────────
public record CreateStockCountCommand(Guid TenantId, CreateStockCountDto Dto) : IRequest<Result<StockCountDto>>;
public record AddStockCountLineCommand(Guid StockCountId, AddStockCountLineDto Dto) : IRequest<Result>;
public record FreezeInventoryCommand(Guid Id) : IRequest<Result>;
public record ApproveStockCountCommand(Guid Id) : IRequest<Result>;

// ─── PurchaseReturn Commands ──────────────────────────────────────────────────
public record CreatePurchaseReturnCommand(Guid TenantId, CreatePurchaseReturnDto Dto) : IRequest<Result<PurchaseReturnDto>>;
public record UpdatePurchaseReturnCommand(Guid Id, UpdatePurchaseReturnDto Dto) : IRequest<Result<PurchaseReturnDto>>;
public record AddPurchaseReturnLineCommand(Guid PurchaseReturnId, AddPurchaseReturnLineDto Dto) : IRequest<Result>;
public record ApprovePurchaseReturnCommand(Guid Id) : IRequest<Result>;
public record PostPurchaseReturnCommand(Guid Id, Guid UserId) : IRequest<Result>;
public record UnpostPurchaseReturnCommand(Guid Id, Guid UserId) : IRequest<Result>;
public record CancelPurchaseReturnCommand(Guid Id) : IRequest<Result>;
public record SeedPurchaseReturnReasonsCommand(Guid TenantId) : IRequest<Result<IReadOnlyList<PurchaseReturnReasonDto>>>;
// ─── InventoryReservation Commands ────────────────────────────────────────────
public record ReserveStockCommand(Guid TenantId, ReserveStockDto Dto) : IRequest<Result<InventoryReservationDto>>;
public record ReleaseStockCommand(Guid Id) : IRequest<Result>;
public record ExpireStockReservationCommand(Guid Id) : IRequest<Result>;

// ─── Additional PurchaseOrder Commands ─────────────────────────────────────────
public record RejectPurchaseOrderCommand(Guid Id) : IRequest<Result>;
public record ClosePurchaseOrderCommand(Guid Id) : IRequest<Result>;
