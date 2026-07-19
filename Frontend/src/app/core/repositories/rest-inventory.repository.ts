import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { Observable, map, switchMap, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { InventoryRepository } from './inventory.repository';
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
  InventoryItemKind,
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
  WarehouseType,
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

const EMPTY_TENANT = '00000000-0000-0000-0000-000000000000';

interface ApiInventoryItem {
  id: string;
  tenantId: string;
  categoryId: string;
  categoryNameAr: string;
  nameAr: string;
  nameEn?: string;
  descriptionAr?: string;
  descriptionEn?: string;
  sku?: string;
  barcode?: string;
  imageUrl?: string;
  itemKind: number;
  baseUnitId: string;
  baseUnitNameAr: string;
  defaultPurchaseUnitId?: string;
  defaultRecipeUnitId?: string;
  reorderLevel: number;
  reorderQuantity: number;
  averageUnitCost?: number;
  lastPurchaseUnitCost?: number;
  isActive: boolean;
}

interface ApiCategory {
  id: string;
  tenantId: string;
  parentCategoryId?: string | null;
  code: string;
  nameAr: string;
  nameEn?: string;
  descriptionAr?: string;
  descriptionEn?: string;
  icon?: string;
  imageUrl?: string;
  color?: string;
  sortOrder: number;
  isActive: boolean;
  createdAt?: string;
}

interface ApiUnit {
  id: string;
  tenantId: string;
  code: string;
  nameAr: string;
  nameEn?: string;
  symbol: string;
  symbolAr?: string;
  decimalPlaces: number;
  baseUnitId?: string | null;
  conversionFactor?: number;
  unitType?: number | string;
  classification?: number | string;
  sortOrder?: number;
  isActive: boolean;
}

interface ApiWarehouse {
  id: string;
  tenantId: string;
  branchId?: string | null;
  companyId?: string | null;
  nameAr: string;
  nameEn?: string;
  code?: string;
  address?: string;
  phone?: string;
  email?: string;
  notes?: string;
  warehouseType: number | string;
  warehouseTypeId?: string | null;
  warehouseTypeNameAr?: string | null;
  parentWarehouseId?: string | null;
  parentWarehouseNameAr?: string | null;
  branchNameAr?: string | null;
  managerUserId?: string | null;
  responsibleEmployeeId?: string | null;
  allowPurchase: boolean;
  allowSales: boolean;
  allowTransfer: boolean;
  allowInventoryCount: boolean;
  allowManufacturing: boolean;
  allowNegativeStock?: boolean;
  allowReservation?: boolean;
  allowReceiving?: boolean;
  allowIssue?: boolean;
  allowAdjustment?: boolean;
  isPosWarehouse?: boolean;
  isDefault?: boolean;
  isSystem?: boolean;
  useBins?: boolean;
  isActive: boolean;
  zoneCount: number;
  createdAt?: string;
}

const WAREHOUSE_TYPE_BY_NUM: Record<number, WarehouseType> = {
  1: 'Main',
  2: 'POS',
  3: 'Production',
  4: 'RawMaterial',
  5: 'FinishedGoods',
  6: 'Returns',
  7: 'Damaged',
  8: 'Transit',
  9: 'Kitchen',
  10: 'Beverage',
  11: 'DryStore',
  12: 'Chiller',
  13: 'Freezer',
  14: 'Packaging',
  15: 'Cleaning',
  16: 'Waste'
};

const WAREHOUSE_TYPE_TO_NUM: Record<WarehouseType, number> = {
  Main: 1,
  POS: 2,
  Production: 3,
  RawMaterial: 4,
  FinishedGoods: 5,
  Returns: 6,
  Damaged: 7,
  Transit: 8,
  Kitchen: 9,
  Beverage: 10,
  DryStore: 11,
  Chiller: 12,
  Freezer: 13,
  Packaging: 14,
  Cleaning: 15,
  Waste: 16
};

@Injectable({
  providedIn: 'root'
})
export class RestInventoryRepository extends InventoryRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/inventory`;

  getItems(search?: string, page = 1, pageSize = 100): Observable<InventoryItemDefinition[]> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    if (search?.trim()) {
      params = params.set('search', search.trim());
    }
    return this.http.get<ApiInventoryItem[]>(`${this.base}/items`, { params }).pipe(
      map(rows => rows.map(mapApiItem))
    );
  }

  getItemById(id: string): Observable<InventoryItemDefinition> {
    return this.http.get<ApiInventoryItem>(`${this.base}/items/${id}`).pipe(
      map(mapApiItem)
    );
  }

  createItem(payload: CreateInventoryItemPayload): Observable<InventoryItemDefinition> {
    return this.http.post<ApiInventoryItem>(`${this.base}/items`, mapToApiPayload(payload)).pipe(
      map(mapApiItem)
    );
  }

  updateItem(id: string, payload: UpdateInventoryItemPayload): Observable<void> {
    return this.http.put<void>(`${this.base}/items/${id}`, mapToUpdatePayload(payload));
  }

  deleteItem(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/items/${id}/deactivate`, {});
  }

  activateItem(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/items/${id}/activate`, {});
  }

  getCategories(): Observable<InventoryCategory[]> {
    const params = new HttpParams().set('page', 1).set('pageSize', 200);
    return this.http.get<ApiCategory[]>(`${this.base}/categories`, { params }).pipe(
      map(rows => rows.map(mapCategory))
    );
  }

  createCategory(payload: CreateInventoryCategoryPayload): Observable<InventoryCategory> {
    return this.http.post<ApiCategory>(`${this.base}/categories`, {
      tenantId: '00000000-0000-0000-0000-000000000000',
      ...payload,
      parentCategoryId: payload.parentCategoryId || null,
      sortOrder: payload.sortOrder ?? 0
    }).pipe(map(mapCategory));
  }

  updateCategory(id: string, payload: UpdateInventoryCategoryPayload): Observable<void> {
    return this.http.put<void>(`${this.base}/categories/${id}`, {
      ...payload,
      parentCategoryId: payload.parentCategoryId || null,
      sortOrder: payload.sortOrder ?? 0
    });
  }

  activateCategory(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/categories/${id}/activate`, {});
  }

  deactivateCategory(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/categories/${id}/deactivate`, {});
  }

  deleteCategory(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/categories/${id}`);
  }

  getUnits(): Observable<InventoryUnit[]> {
    return this.http.get<ApiUnit[]>(`${this.base}/units`).pipe(
      map(rows => rows.map(mapUnit))
    );
  }

  createUnit(payload: CreateInventoryUnitPayload): Observable<InventoryUnit> {
    return this.http.post<ApiUnit>(`${this.base}/units`, {
      tenantId: '00000000-0000-0000-0000-000000000000',
      ...payload,
      decimalPlaces: payload.decimalPlaces ?? 2,
      conversionFactor: payload.conversionFactor ?? 1,
      unitType: payload.unitType ?? 1,
      classification: payload.classification ?? 6,
      sortOrder: payload.sortOrder ?? 0,
      isActive: payload.isActive ?? true,
      baseUnitId: payload.baseUnitId || null
    }).pipe(map(mapUnit));
  }

  updateUnit(id: string, payload: UpdateInventoryUnitPayload): Observable<void> {
    return this.http.put<void>(`${this.base}/units/${id}`, {
      ...payload,
      decimalPlaces: payload.decimalPlaces ?? 2,
      conversionFactor: payload.conversionFactor ?? 1,
      unitType: payload.unitType ?? 1,
      classification: payload.classification ?? 6,
      sortOrder: payload.sortOrder ?? 0,
      isActive: payload.isActive ?? true,
      baseUnitId: payload.baseUnitId || null
    });
  }

  activateUnit(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/units/${id}/activate`, {});
  }

  deactivateUnit(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/units/${id}/deactivate`, {});
  }

  deleteUnit(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/units/${id}`);
  }

  getWarehouses(): Observable<Warehouse[]> {
    const params = new HttpParams().set('page', 1).set('pageSize', 200);
    return this.http.get<ApiWarehouse[]>(`${this.base}/warehouses`, { params }).pipe(
      map(rows => rows.map(mapWarehouse))
    );
  }

  getWarehouseTypes(): Observable<import('../models/inventory.models').WarehouseTypeDefinition[]> {
    return this.http.get<import('../models/inventory.models').WarehouseTypeDefinition[]>(
      `${this.base}/warehouses/types`,
      { params: new HttpParams().set('isActive', 'true') }
    );
  }

  getBranchesLookup(): Observable<import('../models/inventory.models').BranchLookup[]> {
    const params = new HttpParams().set('page', 1).set('pageSize', 200);
    return this.http
      .get<import('../models/inventory.models').BranchLookup[]>(
        `${environment.apiBaseUrl}/organization/branches`,
        { params }
      )
      .pipe(map(rows => rows ?? []));
  }

  createWarehouse(payload: CreateWarehousePayload): Observable<Warehouse> {
    return this.http.post<ApiWarehouse>(`${this.base}/warehouses`, mapWarehousePayload(payload)).pipe(
      map(mapWarehouse)
    );
  }

  updateWarehouse(id: string, payload: UpdateWarehousePayload): Observable<void> {
    return this.http.put<void>(`${this.base}/warehouses/${id}`, mapWarehouseUpdatePayload(payload));
  }

  deleteWarehouse(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/warehouses/${id}`);
  }

  activateWarehouse(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/warehouses/${id}/activate`, {});
  }

  deactivateWarehouse(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/warehouses/${id}/deactivate`, {});
  }

  getStockByWarehouse(itemId: string): Observable<WarehouseStockBalance[]> {
    return this.http.get<WarehouseStockBalance[]>(`${this.base}/items/${itemId}/stock-by-warehouse`);
  }

  getItemMovements(itemId: string, page = 1, pageSize = 50): Observable<ItemStockMovement[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<ItemStockMovement[]>(`${this.base}/items/${itemId}/movements`, { params });
  }

  getItemPurchaseHistory(itemId: string, page = 1, pageSize = 50): Observable<ItemPurchaseHistory[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<ItemPurchaseHistory[]>(`${this.base}/items/${itemId}/purchase-history`, { params });
  }

  getItemSalesHistory(itemId: string, page = 1, pageSize = 50): Observable<ItemSalesHistory[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<ItemSalesHistory[]>(`${this.base}/items/${itemId}/sales-history`, { params });
  }

  getLedger(page = 1, pageSize = 50): Observable<InventoryLedgerEntry[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<InventoryLedgerEntry[]>(`${this.base}/stock`, { params });
  }

  getTransfers(page = 1, pageSize = 50): Observable<StockTransferRecord[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<StockTransferRecord[]>(`${this.base}/stock/transfers`, { params });
  }

  createTransfer(payload: CreateStockTransferPayload): Observable<StockTransferRecord> {
    return this.http.post<StockTransferRecord>(`${this.base}/stock/transfers`, {
      tenantId: EMPTY_TENANT,
      sourceWarehouseId: payload.sourceWarehouseId,
      destinationWarehouseId: payload.destinationWarehouseId,
      transferNumber: payload.transferNumber,
      notes: payload.notes || null
    }).pipe(
      switchMap(created =>
        this.http.post<void>(`${this.base}/stock/transfers/${created.id}/lines`, {
          inventoryItemId: payload.inventoryItemId,
          unitId: payload.unitId,
          quantity: payload.quantity
        }).pipe(
          switchMap(() =>
            payload.complete === false
              ? of(created)
              : this.http.post<void>(`${this.base}/stock/transfers/${created.id}/complete`, {}).pipe(map(() => ({
                  ...created,
                  status: 'Completed'
                })))
          )
        )
      )
    );
  }

  completeTransfer(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/stock/transfers/${id}/complete`, {});
  }

  getAdjustments(page = 1, pageSize = 50): Observable<StockAdjustmentRecord[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<StockAdjustmentRecord[]>(`${this.base}/stock/adjustments`, { params });
  }

  createAdjustment(payload: CreateStockAdjustmentPayload): Observable<StockAdjustmentRecord> {
    return this.http.post<StockAdjustmentRecord>(`${this.base}/stock/adjustments`, {
      tenantId: EMPTY_TENANT,
      warehouseId: payload.warehouseId,
      inventoryItemId: payload.inventoryItemId,
      adjustmentNumber: payload.adjustmentNumber,
      quantityAdjusted: payload.quantityAdjusted,
      unitId: payload.unitId,
      unitCost: payload.unitCost,
      reasonId: payload.reasonId || null,
      notes: payload.notes || null
    }).pipe(
      switchMap(created =>
        payload.confirm === false
          ? of(created)
          : this.http.post<void>(`${this.base}/stock/adjustments/${created.id}/confirm`, {}).pipe(map(() => created))
      )
    );
  }

  confirmAdjustment(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/stock/adjustments/${id}/confirm`, {});
  }

  getWaste(page = 1, pageSize = 50): Observable<WasteRecord[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<WasteRecord[]>(`${this.base}/stock/waste`, { params });
  }

  createWaste(payload: CreateWastePayload): Observable<WasteRecord> {
    return this.http.post<WasteRecord>(`${this.base}/stock/waste`, {
      tenantId: EMPTY_TENANT,
      warehouseId: payload.warehouseId,
      inventoryItemId: payload.inventoryItemId,
      unitId: payload.unitId,
      wasteNumber: payload.wasteNumber,
      quantity: payload.quantity,
      unitCost: payload.unitCost,
      reasonId: payload.reasonId || null,
      notes: payload.notes || null
    }).pipe(
      switchMap(created =>
        payload.confirm === false
          ? of(created)
          : this.http.post<void>(`${this.base}/stock/waste/${created.id}/confirm`, {}).pipe(map(() => created))
      )
    );
  }

  confirmWaste(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/stock/waste/${id}/confirm`, {});
  }

  getGoodsReceipts(page = 1, pageSize = 50): Observable<GoodsReceiptRecord[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<GoodsReceiptRecord[]>(`${this.base}/goods-receipts`, { params });
  }

  createGoodsReceipt(payload: CreateGoodsReceiptPayload): Observable<GoodsReceiptRecord> {
    return this.http
      .post<GoodsReceiptRecord>(`${this.base}/goods-receipts`, {
        tenantId: EMPTY_TENANT,
        purchaseOrderId: payload.purchaseOrderId,
        warehouseId: payload.warehouseId,
        grnNumber: payload.grnNumber,
        notes: payload.notes || null,
        lines: [
          {
            purchaseOrderLineId: payload.purchaseOrderLineId || null,
            inventoryItemId: payload.inventoryItemId,
            unitId: payload.unitId,
            receivedQuantity: payload.receivedQuantity,
            unitCost: payload.unitCost,
            acceptedQuantity: payload.receivedQuantity
          }
        ]
      })
      .pipe(
        switchMap(created =>
          payload.confirm === false
            ? of(created)
            : this.http
                .post<void>(`${this.base}/goods-receipts/${created.id}/confirm`, {})
                .pipe(map(() => ({ ...created, lineCount: 1 })))
        )
      );
  }

  confirmGoodsReceipt(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/goods-receipts/${id}/confirm`, {});
  }

  getStockCounts(page = 1, pageSize = 50): Observable<StockCountRecord[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<StockCountRecord[]>(`${this.base}/stock-counts`, { params });
  }

  createStockCount(payload: CreateStockCountPayload): Observable<StockCountRecord> {
    return this.http.post<StockCountRecord>(`${this.base}/stock-counts`, {
      warehouseId: payload.warehouseId,
      countNumber: payload.countNumber,
      notes: payload.notes || null
    }).pipe(
      switchMap(created =>
        this.http.post<void>(`${this.base}/stock-counts/${created.id}/lines`, {
          inventoryItemId: payload.inventoryItemId,
          unitId: payload.unitId,
          expectedQuantity: payload.expectedQuantity,
          actualQuantity: payload.actualQuantity
        }).pipe(map(() => created))
      )
    );
  }

  approveStockCount(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/stock-counts/${id}/approve`, {});
  }

  getPurchaseReturns(page = 1, pageSize = 50): Observable<PurchaseReturnRecord[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PurchaseReturnRecord[]>(`${this.base}/purchase-returns`, { params });
  }

  createPurchaseReturn(payload: CreatePurchaseReturnPayload): Observable<PurchaseReturnRecord> {
    const returnType = payload.goodsReceiptId ? 1 : 3; // BeforeInvoice when GRN present, else Direct
    return this.http
      .post<PurchaseReturnRecord>(`${this.base}/purchase-returns`, {
        returnType,
        warehouseId: payload.warehouseId,
        returnNumber: payload.returnNumber || null,
        goodsReceiptId: payload.goodsReceiptId || null,
        supplierId: payload.supplierId || null,
        reasonNotes: payload.reason || null,
        lines: [
          {
            inventoryItemId: payload.inventoryItemId,
            unitId: payload.unitId,
            originalQuantity: payload.returnQuantity,
            previouslyReturnedQuantity: 0,
            returnQuantity: payload.returnQuantity,
            unitCost: payload.unitCost
          }
        ]
      })
      .pipe(
        switchMap(created =>
          payload.approve === false
            ? of(created)
            : this.http.post<void>(`${this.base}/purchase-returns/${created.id}/approve`, {}).pipe(
                map(() => ({ ...created, isCompleted: true }))
              )
        )
      );
  }

  approvePurchaseReturn(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/purchase-returns/${id}/approve`, {});
  }

  getPurchaseOrders(page = 1, pageSize = 50): Observable<PurchaseOrderSummary[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PurchaseOrderSummary[]>(`${this.base}/purchases`, { params });
  }

  getSuppliers(page = 1, pageSize = 100): Observable<SupplierSummary[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<SupplierSummary[]>(`${this.base}/suppliers`, { params });
  }

  getDashboard(): Observable<InventoryDashboardSummary> {
    return this.http.get<InventoryDashboardSummary>(`${this.base}/dashboard`);
  }

  getReservations(page = 1, pageSize = 50): Observable<InventoryReservationRecord[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<InventoryReservationRecord[]>(`${this.base}/reservations`, { params });
  }

  createReservation(payload: CreateReservationPayload): Observable<InventoryReservationRecord> {
    return this.http.post<InventoryReservationRecord>(`${this.base}/reservations`, {
      warehouseId: payload.warehouseId,
      inventoryItemId: payload.inventoryItemId,
      reservedQuantity: payload.reservedQuantity,
      sourceDocument: payload.sourceDocument,
      expirationDate: payload.expirationDate || null
    });
  }

  releaseReservation(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/reservations/${id}/release`, {});
  }

  expireReservation(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/reservations/${id}/expire`, {});
  }

  getSettings(branchId?: string | null): Observable<InventorySetting> {
    let params = new HttpParams();
    if (branchId) params = params.set('branchId', branchId);
    return this.http.get<InventorySetting>(`${this.base}/settings`, { params });
  }

  upsertSettings(payload: UpsertInventorySettingPayload): Observable<InventorySetting> {
    return this.http.put<InventorySetting>(`${this.base}/settings`, {
      ...payload,
      tenantId: '00000000-0000-0000-0000-000000000000',
      defaultWarehouseId: payload.defaultWarehouseId || null,
      defaultUnitId: payload.defaultUnitId || null,
      documentSeries: payload.documentSeries ?? []
    });
  }

  resetSettings(branchId?: string | null): Observable<InventorySetting> {
    let params = new HttpParams();
    if (branchId) params = params.set('branchId', branchId);
    return this.http.post<InventorySetting>(`${this.base}/settings/reset`, {}, { params });
  }

  getWarehouseById(id: string): Observable<WarehouseDetail> {
    return this.http.get<WarehouseDetail>(`${this.base}/warehouses/${id}`);
  }

  addWarehouseZone(warehouseId: string, payload: AddWarehouseLocationPayload): Observable<unknown> {
    return this.http.post(`${this.base}/warehouses/${warehouseId}/zones`, payload);
  }

  removeWarehouseZone(warehouseId: string, zoneId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/warehouses/${warehouseId}/zones/${zoneId}`);
  }

  addWarehouseShelf(warehouseId: string, zoneId: string, payload: AddWarehouseLocationPayload): Observable<unknown> {
    return this.http.post(`${this.base}/warehouses/${warehouseId}/zones/${zoneId}/shelves`, payload);
  }

  removeWarehouseShelf(warehouseId: string, zoneId: string, shelfId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/warehouses/${warehouseId}/zones/${zoneId}/shelves/${shelfId}`);
  }

  addWarehouseBin(warehouseId: string, zoneId: string, shelfId: string, payload: AddWarehouseLocationPayload): Observable<unknown> {
    return this.http.post(`${this.base}/warehouses/${warehouseId}/zones/${zoneId}/shelves/${shelfId}/bins`, payload);
  }

  removeWarehouseBin(warehouseId: string, zoneId: string, shelfId: string, binId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/warehouses/${warehouseId}/zones/${zoneId}/shelves/${shelfId}/bins/${binId}`);
  }

  getBrands(): Observable<InventoryBrand[]> {
    return this.http.get<InventoryBrand[]>(`${this.base}/brands`);
  }

  createBrand(payload: UpsertInventoryBrandPayload): Observable<InventoryBrand> {
    return this.http.post<InventoryBrand>(`${this.base}/brands`, payload);
  }

  updateBrand(id: string, payload: UpsertInventoryBrandPayload): Observable<void> {
    return this.http.put<void>(`${this.base}/brands/${id}`, payload);
  }

  activateBrand(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/brands/${id}/activate`, {});
  }

  deactivateBrand(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/brands/${id}/deactivate`, {});
  }

  getManufacturers(): Observable<InventoryManufacturer[]> {
    return this.http.get<InventoryManufacturer[]>(`${this.base}/manufacturers`);
  }

  createManufacturer(payload: UpsertInventoryManufacturerPayload): Observable<InventoryManufacturer> {
    return this.http.post<InventoryManufacturer>(`${this.base}/manufacturers`, payload);
  }

  updateManufacturer(id: string, payload: UpsertInventoryManufacturerPayload): Observable<void> {
    return this.http.put<void>(`${this.base}/manufacturers/${id}`, payload);
  }

  activateManufacturer(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/manufacturers/${id}/activate`, {});
  }

  deactivateManufacturer(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/manufacturers/${id}/deactivate`, {});
  }

  getAttributes(): Observable<InventoryAttribute[]> {
    return this.http.get<InventoryAttribute[]>(`${this.base}/attributes`);
  }

  createAttribute(payload: UpsertInventoryAttributePayload): Observable<InventoryAttribute> {
    return this.http.post<InventoryAttribute>(`${this.base}/attributes`, payload);
  }

  updateAttribute(id: string, payload: UpsertInventoryAttributePayload): Observable<void> {
    return this.http.put<void>(`${this.base}/attributes/${id}`, payload);
  }

  activateAttribute(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/attributes/${id}/activate`, {});
  }

  deactivateAttribute(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/attributes/${id}/deactivate`, {});
  }

  addAttributeValue(attributeId: string, valueAr: string, valueEn?: string): Observable<InventoryAttributeValue> {
    return this.http.post<InventoryAttributeValue>(`${this.base}/attributes/${attributeId}/values`, {
      valueAr,
      valueEn: valueEn || null,
      sortOrder: 0
    });
  }

  removeAttributeValue(attributeId: string, valueId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/attributes/${attributeId}/values/${valueId}`);
  }

  getPriceLists(): Observable<InventoryPriceList[]> {
    return this.http.get<InventoryPriceList[]>(`${this.base}/price-lists`);
  }

  getPriceListById(id: string): Observable<InventoryPriceList> {
    return this.http.get<InventoryPriceList>(`${this.base}/price-lists/${id}`);
  }

  createPriceList(payload: UpsertInventoryPriceListPayload): Observable<InventoryPriceList> {
    return this.http.post<InventoryPriceList>(`${this.base}/price-lists`, {
      ...payload,
      currency: payload.currency || 'SAR',
      validFrom: payload.validFrom || null,
      validTo: payload.validTo || null
    });
  }

  updatePriceList(id: string, payload: UpsertInventoryPriceListPayload): Observable<void> {
    return this.http.put<void>(`${this.base}/price-lists/${id}`, {
      ...payload,
      currency: payload.currency || 'SAR',
      validFrom: payload.validFrom || null,
      validTo: payload.validTo || null
    });
  }

  activatePriceList(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/price-lists/${id}/activate`, {});
  }

  deactivatePriceList(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/price-lists/${id}/deactivate`, {});
  }

  upsertPriceListLine(priceListId: string, payload: UpsertInventoryPriceListLinePayload): Observable<unknown> {
    return this.http.post(`${this.base}/price-lists/${priceListId}/lines`, {
      inventoryItemId: payload.inventoryItemId,
      unitPrice: payload.unitPrice,
      unitId: payload.unitId || null
    });
  }

  removePriceListLine(priceListId: string, lineId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/price-lists/${priceListId}/lines/${lineId}`);
  }

  getTaxGroups(): Observable<InventoryTaxGroup[]> {
    return this.http.get<InventoryTaxGroup[]>(`${environment.apiBaseUrl}/sales/tax-groups`);
  }

  createTaxGroup(payload: { nameAr: string; nameEn?: string; description?: string }): Observable<InventoryTaxGroup> {
    return this.http.post<InventoryTaxGroup>(`${environment.apiBaseUrl}/sales/tax-groups`, payload);
  }

  updateTaxGroup(id: string, payload: { nameAr: string; nameEn?: string; description?: string }): Observable<void> {
    return this.http.put<void>(`${environment.apiBaseUrl}/sales/tax-groups/${id}`, payload);
  }

  getItemTypes(query: InventoryItemTypeQuery = {}): Observable<InventoryItemTypePage> {
    let params = new HttpParams()
      .set('page', String(query.page ?? 1))
      .set('pageSize', String(query.pageSize ?? 50));

    if (query.search) params = params.set('search', query.search);
    if (query.category != null) params = params.set('category', String(query.category));
    if (query.isActive != null) params = params.set('isActive', String(query.isActive));
    if (query.isInventory != null) params = params.set('isInventory', String(query.isInventory));
    if (query.canSell != null) params = params.set('canSell', String(query.canSell));
    if (query.canPurchase != null) params = params.set('canPurchase', String(query.canPurchase));
    if (query.isRecipe != null) params = params.set('isRecipe', String(query.isRecipe));
    if (query.isProduction != null) params = params.set('isProduction', String(query.isProduction));
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDesc) params = params.set('sortDesc', 'true');

    return this.http
      .get<InventoryItemType[]>(`${this.base}/item-types`, { params, observe: 'response' })
      .pipe(map(res => mapItemTypePage(res)));
  }

  getItemTypeById(id: string): Observable<InventoryItemType> {
    return this.http.get<InventoryItemType>(`${this.base}/item-types/${id}`);
  }

  createItemType(payload: CreateInventoryItemTypePayload): Observable<InventoryItemType> {
    return this.http.post<InventoryItemType>(`${this.base}/item-types`, {
      ...payload,
      tenantId: EMPTY_TENANT
    });
  }

  updateItemType(id: string, payload: UpdateInventoryItemTypePayload): Observable<void> {
    return this.http.put<void>(`${this.base}/item-types/${id}`, {
      ...payload,
      tenantId: EMPTY_TENANT
    });
  }

  deleteItemType(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/item-types/${id}`);
  }

  activateItemType(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/item-types/${id}/activate`, {});
  }

  deactivateItemType(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/item-types/${id}/deactivate`, {});
  }
}

function mapItemTypePage(res: HttpResponse<InventoryItemType[]>): InventoryItemTypePage {
  const items = res.body ?? [];
  const totalCount = Number(res.headers.get('X-Pagination-TotalCount') ?? items.length);
  const pageNumber = Number(res.headers.get('X-Pagination-PageNumber') ?? 1);
  const pageSize = Number(res.headers.get('X-Pagination-PageSize') ?? (items.length || 50));
  const totalPages = Number(res.headers.get('X-Pagination-TotalPages') ?? 1);
  return { items, totalCount, pageNumber, pageSize, totalPages };
}

function mapCategory(c: ApiCategory): InventoryCategory {
  return {
    id: c.id,
    tenantId: c.tenantId,
    parentCategoryId: c.parentCategoryId,
    code: c.code ?? '',
    nameAr: c.nameAr,
    nameEn: c.nameEn,
    descriptionAr: c.descriptionAr,
    descriptionEn: c.descriptionEn,
    icon: c.icon,
    imageUrl: c.imageUrl,
    color: c.color,
    sortOrder: c.sortOrder ?? 0,
    isActive: c.isActive !== false,
    createdAt: c.createdAt
  };
}

function mapUnit(u: ApiUnit): InventoryUnit {
  return {
    id: u.id,
    tenantId: u.tenantId,
    code: u.code ?? u.symbol ?? '',
    nameAr: u.nameAr,
    nameEn: u.nameEn,
    symbol: u.symbol,
    symbolAr: u.symbolAr,
    decimalPlaces: u.decimalPlaces ?? 2,
    baseUnitId: u.baseUnitId,
    conversionFactor: Number(u.conversionFactor ?? 1),
    unitType: typeof u.unitType === 'string'
      ? (u.unitType === 'Count' ? 2 : 1)
      : Number(u.unitType ?? 1),
    classification: typeof u.classification === 'string'
      ? mapUnitClassification(u.classification)
      : Number(u.classification ?? 6),
    sortOrder: u.sortOrder ?? 0,
    isActive: u.isActive
  };
}

function mapUnitClassification(value: string): number {
  const map: Record<string, number> = {
    Weight: 1, Volume: 2, Length: 3, Count: 4, Packaging: 5, Other: 6
  };
  return map[value] ?? 6;
}

function mapWarehouseType(value: number | string): WarehouseType {
  if (typeof value === 'string' && value in WAREHOUSE_TYPE_TO_NUM) {
    return value as WarehouseType;
  }
  return WAREHOUSE_TYPE_BY_NUM[Number(value)] ?? 'Main';
}

function mapWarehouse(w: ApiWarehouse): Warehouse {
  return {
    id: w.id,
    tenantId: w.tenantId,
    branchId: w.branchId,
    companyId: w.companyId,
    nameAr: w.nameAr,
    nameEn: w.nameEn,
    code: w.code,
    address: w.address,
    phone: w.phone,
    email: w.email,
    notes: w.notes,
    warehouseType: mapWarehouseType(w.warehouseType),
    warehouseTypeId: w.warehouseTypeId,
    warehouseTypeNameAr: w.warehouseTypeNameAr,
    parentWarehouseId: w.parentWarehouseId,
    parentWarehouseNameAr: w.parentWarehouseNameAr,
    branchNameAr: w.branchNameAr,
    managerUserId: w.managerUserId,
    responsibleEmployeeId: w.responsibleEmployeeId,
    allowPurchase: w.allowPurchase ?? true,
    allowSales: w.allowSales ?? true,
    allowTransfer: w.allowTransfer ?? true,
    allowInventoryCount: w.allowInventoryCount ?? true,
    allowManufacturing: w.allowManufacturing ?? true,
    allowNegativeStock: w.allowNegativeStock ?? false,
    allowReservation: w.allowReservation ?? true,
    allowReceiving: w.allowReceiving ?? true,
    allowIssue: w.allowIssue ?? true,
    allowAdjustment: w.allowAdjustment ?? true,
    isPosWarehouse: w.isPosWarehouse ?? false,
    isDefault: w.isDefault ?? false,
    isSystem: w.isSystem ?? false,
    useBins: w.useBins ?? false,
    isActive: w.isActive,
    zoneCount: w.zoneCount ?? 0,
    createdAt: w.createdAt
  };
}

function resolveWarehouseTypeNum(value?: WarehouseType | number): number {
  if (typeof value === 'number') return value;
  if (value && value in WAREHOUSE_TYPE_TO_NUM) return WAREHOUSE_TYPE_TO_NUM[value];
  return 1;
}

function mapWarehousePayload(payload: CreateWarehousePayload): Record<string, unknown> {
  return {
    tenantId: '00000000-0000-0000-0000-000000000000',
    nameAr: payload.nameAr,
    nameEn: payload.nameEn || null,
    code: payload.code || null,
    branchId: payload.branchId || null,
    companyId: payload.companyId || null,
    warehouseType: resolveWarehouseTypeNum(payload.warehouseType),
    warehouseTypeId: payload.warehouseTypeId || null,
    parentWarehouseId: payload.parentWarehouseId || null,
    address: payload.address || null,
    phone: payload.phone || null,
    email: payload.email || null,
    notes: payload.notes || null,
    managerUserId: payload.managerUserId || null,
    responsibleEmployeeId: payload.responsibleEmployeeId || null,
    allowPurchase: payload.allowPurchase ?? true,
    allowSales: payload.allowSales ?? true,
    allowTransfer: payload.allowTransfer ?? true,
    allowInventoryCount: payload.allowInventoryCount ?? true,
    allowManufacturing: payload.allowManufacturing ?? true,
    allowNegativeStock: payload.allowNegativeStock ?? false,
    allowReservation: payload.allowReservation ?? true,
    allowReceiving: payload.allowReceiving ?? true,
    allowIssue: payload.allowIssue ?? true,
    allowAdjustment: payload.allowAdjustment ?? true,
    isPosWarehouse: payload.isPosWarehouse ?? false,
    isDefault: payload.isDefault ?? false,
    useBins: payload.useBins ?? false,
    isActive: payload.isActive ?? true
  };
}

function mapWarehouseUpdatePayload(payload: UpdateWarehousePayload): Record<string, unknown> {
  const { tenantId: _, ...rest } = mapWarehousePayload(payload) as Record<string, unknown> & { tenantId?: string };
  return rest;
}

function mapApiItemKind(value: number): InventoryItemKind {
  return value === 2 ? 'manufactured' : 'raw';
}

function mapToApiItemKind(value: InventoryItemKind): number {
  return value === 'manufactured' ? 2 : 1;
}

function mapApiItem(raw: ApiInventoryItem): InventoryItemDefinition {
  return {
    id: raw.id,
    tenantId: raw.tenantId,
    categoryId: raw.categoryId,
    categoryNameAr: raw.categoryNameAr,
    nameAr: raw.nameAr,
    nameEn: raw.nameEn,
    descriptionAr: raw.descriptionAr,
    descriptionEn: raw.descriptionEn,
    sku: raw.sku,
    barcode: raw.barcode,
    imageUrl: raw.imageUrl,
    itemKind: mapApiItemKind(raw.itemKind),
    baseUnitId: raw.baseUnitId,
    baseUnitNameAr: raw.baseUnitNameAr,
    defaultPurchaseUnitId: raw.defaultPurchaseUnitId,
    defaultRecipeUnitId: raw.defaultRecipeUnitId,
    reorderLevel: Number(raw.reorderLevel),
    reorderQuantity: Number(raw.reorderQuantity),
    averageUnitCost: raw.averageUnitCost != null ? Number(raw.averageUnitCost) : undefined,
    lastPurchaseUnitCost: raw.lastPurchaseUnitCost != null ? Number(raw.lastPurchaseUnitCost) : undefined,
    isActive: raw.isActive
  };
}

function mapToApiPayload(payload: CreateInventoryItemPayload): Record<string, unknown> {
  return {
    tenantId: '00000000-0000-0000-0000-000000000000',
    categoryId: payload.categoryId,
    nameAr: payload.nameAr,
    baseUnitId: payload.baseUnitId,
    nameEn: payload.nameEn || null,
    descriptionAr: payload.descriptionAr || null,
    descriptionEn: payload.descriptionEn || null,
    sku: payload.sku || null,
    barcode: payload.barcode || null,
    imageUrl: payload.imageUrl || null,
    itemKind: mapToApiItemKind(payload.itemKind),
    defaultPurchaseUnitId: payload.defaultPurchaseUnitId || null,
    defaultRecipeUnitId: payload.defaultRecipeUnitId || null,
    reorderLevel: payload.reorderLevel ?? 0,
    reorderQuantity: payload.reorderQuantity ?? 0
  };
}

function mapToUpdatePayload(payload: UpdateInventoryItemPayload): Record<string, unknown> {
  return {
    nameAr: payload.nameAr,
    nameEn: payload.nameEn || null,
    descriptionAr: payload.descriptionAr || null,
    descriptionEn: payload.descriptionEn || null,
    sku: payload.sku || null,
    barcode: payload.barcode || null,
    imageUrl: payload.imageUrl || null,
    itemKind: mapToApiItemKind(payload.itemKind),
    categoryId: payload.categoryId,
    baseUnitId: payload.baseUnitId,
    defaultPurchaseUnitId: payload.defaultPurchaseUnitId || null,
    defaultRecipeUnitId: payload.defaultRecipeUnitId || null,
    reorderLevel: payload.reorderLevel ?? 0,
    reorderQuantity: payload.reorderQuantity ?? 0
  };
}
