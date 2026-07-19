import { Observable } from 'rxjs';
import {
  CreateGoodsReceiptPayload,
  CreateInventoryCategoryPayload,
  CreateInventoryItemPayload,
  CreateInventoryUnitPayload,
  CreatePurchaseReturnPayload,
  CreateStockAdjustmentPayload,
  CreateStockCountPayload,
  CreateStockTransferPayload,
  CreateWarehousePayload,
  CreateWastePayload,
  GoodsReceiptRecord,
  InventoryCategory,
  InventoryDashboardSummary,
  InventoryItemDefinition,
  InventoryLedgerEntry,
  InventoryUnit,
  ItemPurchaseHistory,
  ItemSalesHistory,
  ItemStockMovement,
  PurchaseOrderSummary,
  PurchaseReturnRecord,
  StockAdjustmentRecord,
  StockCountRecord,
  StockTransferRecord,
  SupplierSummary,
  UpdateInventoryCategoryPayload,
  UpdateInventoryItemPayload,
  UpdateInventoryUnitPayload,
  UpdateWarehousePayload,
  Warehouse,
  WarehouseStockBalance,
  WasteRecord,
  InventoryReservationRecord,
  CreateReservationPayload,
  InventorySetting,
  UpsertInventorySettingPayload,
  WarehouseDetail,
  AddWarehouseLocationPayload,
  InventoryBrand,
  UpsertInventoryBrandPayload,
  InventoryManufacturer,
  UpsertInventoryManufacturerPayload,
  InventoryAttribute,
  UpsertInventoryAttributePayload,
  InventoryAttributeValue,
  InventoryPriceList,
  UpsertInventoryPriceListPayload,
  UpsertInventoryPriceListLinePayload,
  InventoryTaxGroup
} from '../models/inventory.models';
import {
  CreateInventoryItemTypePayload,
  InventoryItemType,
  InventoryItemTypePage,
  InventoryItemTypeQuery,
  UpdateInventoryItemTypePayload
} from '../models/inventory-item-type.models';

export abstract class InventoryRepository {
  abstract getItems(search?: string, page?: number, pageSize?: number): Observable<InventoryItemDefinition[]>;
  abstract getItemById(id: string): Observable<InventoryItemDefinition>;
  abstract createItem(payload: CreateInventoryItemPayload): Observable<InventoryItemDefinition>;
  abstract updateItem(id: string, payload: UpdateInventoryItemPayload): Observable<void>;

  abstract getCategories(): Observable<InventoryCategory[]>;
  abstract createCategory(payload: CreateInventoryCategoryPayload): Observable<InventoryCategory>;
  abstract updateCategory(id: string, payload: UpdateInventoryCategoryPayload): Observable<void>;
  abstract activateCategory(id: string): Observable<void>;
  abstract deactivateCategory(id: string): Observable<void>;
  abstract deleteCategory(id: string): Observable<void>;

  abstract getUnits(): Observable<InventoryUnit[]>;
  abstract createUnit(payload: CreateInventoryUnitPayload): Observable<InventoryUnit>;
  abstract updateUnit(id: string, payload: UpdateInventoryUnitPayload): Observable<void>;
  abstract activateUnit(id: string): Observable<void>;
  abstract deactivateUnit(id: string): Observable<void>;
  abstract deleteUnit(id: string): Observable<void>;

  abstract getWarehouses(): Observable<Warehouse[]>;
  abstract getWarehouseTypes(): Observable<import('../models/inventory.models').WarehouseTypeDefinition[]>;
  abstract getBranchesLookup(): Observable<import('../models/inventory.models').BranchLookup[]>;
  abstract createWarehouse(payload: CreateWarehousePayload): Observable<Warehouse>;
  abstract updateWarehouse(id: string, payload: UpdateWarehousePayload): Observable<void>;
  abstract deleteWarehouse(id: string): Observable<void>;
  abstract activateWarehouse(id: string): Observable<void>;
  abstract deactivateWarehouse(id: string): Observable<void>;

  abstract getStockByWarehouse(itemId: string): Observable<WarehouseStockBalance[]>;
  abstract getItemMovements(itemId: string, page?: number, pageSize?: number): Observable<ItemStockMovement[]>;
  abstract getItemPurchaseHistory(itemId: string, page?: number, pageSize?: number): Observable<ItemPurchaseHistory[]>;
  abstract getItemSalesHistory(itemId: string, page?: number, pageSize?: number): Observable<ItemSalesHistory[]>;

  abstract getLedger(page?: number, pageSize?: number): Observable<InventoryLedgerEntry[]>;
  abstract getTransfers(page?: number, pageSize?: number): Observable<StockTransferRecord[]>;
  abstract createTransfer(payload: CreateStockTransferPayload): Observable<StockTransferRecord>;
  abstract completeTransfer(id: string): Observable<void>;

  abstract getAdjustments(page?: number, pageSize?: number): Observable<StockAdjustmentRecord[]>;
  abstract createAdjustment(payload: CreateStockAdjustmentPayload): Observable<StockAdjustmentRecord>;
  abstract confirmAdjustment(id: string): Observable<void>;

  abstract getWaste(page?: number, pageSize?: number): Observable<WasteRecord[]>;
  abstract createWaste(payload: CreateWastePayload): Observable<WasteRecord>;
  abstract confirmWaste(id: string): Observable<void>;

  abstract getGoodsReceipts(page?: number, pageSize?: number): Observable<GoodsReceiptRecord[]>;
  abstract createGoodsReceipt(payload: CreateGoodsReceiptPayload): Observable<GoodsReceiptRecord>;
  abstract confirmGoodsReceipt(id: string): Observable<void>;

  abstract getStockCounts(page?: number, pageSize?: number): Observable<StockCountRecord[]>;
  abstract createStockCount(payload: CreateStockCountPayload): Observable<StockCountRecord>;
  abstract approveStockCount(id: string): Observable<void>;

  abstract getPurchaseReturns(page?: number, pageSize?: number): Observable<PurchaseReturnRecord[]>;
  abstract createPurchaseReturn(payload: CreatePurchaseReturnPayload): Observable<PurchaseReturnRecord>;
  abstract approvePurchaseReturn(id: string): Observable<void>;

  abstract getPurchaseOrders(page?: number, pageSize?: number): Observable<PurchaseOrderSummary[]>;
  abstract getSuppliers(page?: number, pageSize?: number): Observable<SupplierSummary[]>;
  abstract getDashboard(): Observable<InventoryDashboardSummary>;

  abstract getReservations(page?: number, pageSize?: number): Observable<InventoryReservationRecord[]>;
  abstract createReservation(payload: CreateReservationPayload): Observable<InventoryReservationRecord>;
  abstract releaseReservation(id: string): Observable<void>;
  abstract expireReservation(id: string): Observable<void>;

  abstract getSettings(branchId?: string | null): Observable<InventorySetting>;
  abstract upsertSettings(payload: UpsertInventorySettingPayload): Observable<InventorySetting>;
  abstract resetSettings(branchId?: string | null): Observable<InventorySetting>;

  abstract getWarehouseById(id: string): Observable<WarehouseDetail>;
  abstract addWarehouseZone(warehouseId: string, payload: AddWarehouseLocationPayload): Observable<unknown>;
  abstract removeWarehouseZone(warehouseId: string, zoneId: string): Observable<void>;
  abstract addWarehouseShelf(warehouseId: string, zoneId: string, payload: AddWarehouseLocationPayload): Observable<unknown>;
  abstract removeWarehouseShelf(warehouseId: string, zoneId: string, shelfId: string): Observable<void>;
  abstract addWarehouseBin(warehouseId: string, zoneId: string, shelfId: string, payload: AddWarehouseLocationPayload): Observable<unknown>;
  abstract removeWarehouseBin(warehouseId: string, zoneId: string, shelfId: string, binId: string): Observable<void>;

  abstract getBrands(): Observable<InventoryBrand[]>;
  abstract createBrand(payload: UpsertInventoryBrandPayload): Observable<InventoryBrand>;
  abstract updateBrand(id: string, payload: UpsertInventoryBrandPayload): Observable<void>;
  abstract activateBrand(id: string): Observable<void>;
  abstract deactivateBrand(id: string): Observable<void>;

  abstract getManufacturers(): Observable<InventoryManufacturer[]>;
  abstract createManufacturer(payload: UpsertInventoryManufacturerPayload): Observable<InventoryManufacturer>;
  abstract updateManufacturer(id: string, payload: UpsertInventoryManufacturerPayload): Observable<void>;
  abstract activateManufacturer(id: string): Observable<void>;
  abstract deactivateManufacturer(id: string): Observable<void>;

  abstract getAttributes(): Observable<InventoryAttribute[]>;
  abstract createAttribute(payload: UpsertInventoryAttributePayload): Observable<InventoryAttribute>;
  abstract updateAttribute(id: string, payload: UpsertInventoryAttributePayload): Observable<void>;
  abstract activateAttribute(id: string): Observable<void>;
  abstract deactivateAttribute(id: string): Observable<void>;
  abstract addAttributeValue(attributeId: string, valueAr: string, valueEn?: string): Observable<InventoryAttributeValue>;
  abstract removeAttributeValue(attributeId: string, valueId: string): Observable<void>;

  abstract getPriceLists(): Observable<InventoryPriceList[]>;
  abstract getPriceListById(id: string): Observable<InventoryPriceList>;
  abstract createPriceList(payload: UpsertInventoryPriceListPayload): Observable<InventoryPriceList>;
  abstract updatePriceList(id: string, payload: UpsertInventoryPriceListPayload): Observable<void>;
  abstract activatePriceList(id: string): Observable<void>;
  abstract deactivatePriceList(id: string): Observable<void>;
  abstract upsertPriceListLine(priceListId: string, payload: UpsertInventoryPriceListLinePayload): Observable<unknown>;
  abstract removePriceListLine(priceListId: string, lineId: string): Observable<void>;

  abstract getTaxGroups(): Observable<InventoryTaxGroup[]>;
  abstract createTaxGroup(payload: { nameAr: string; nameEn?: string; description?: string }): Observable<InventoryTaxGroup>;
  abstract updateTaxGroup(id: string, payload: { nameAr: string; nameEn?: string; description?: string }): Observable<void>;

  abstract getItemTypes(query?: InventoryItemTypeQuery): Observable<InventoryItemTypePage>;
  abstract getItemTypeById(id: string): Observable<InventoryItemType>;
  abstract createItemType(payload: CreateInventoryItemTypePayload): Observable<InventoryItemType>;
  abstract updateItemType(id: string, payload: UpdateInventoryItemTypePayload): Observable<void>;
  abstract deleteItemType(id: string): Observable<void>;
  abstract activateItemType(id: string): Observable<void>;
  abstract deactivateItemType(id: string): Observable<void>;
}
