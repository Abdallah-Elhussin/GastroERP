import { Injectable, inject, signal } from '@angular/core';
import { catchError, of, tap } from 'rxjs';
import { InventoryRepository } from '../repositories/inventory.repository';
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
  CreateReservationPayload,
  InventoryCategory,
  InventoryItemDefinition,
  InventoryUnit,
  UpdateInventoryCategoryPayload,
  UpdateInventoryItemPayload,
  UpdateInventoryUnitPayload,
  UpdateWarehousePayload,
  UpsertInventorySettingPayload,
  Warehouse
} from '../models/inventory.models';

@Injectable({
  providedIn: 'root'
})
export class InventoryService {
  private repo = inject(InventoryRepository);

  items = signal<InventoryItemDefinition[]>([]);
  categories = signal<InventoryCategory[]>([]);
  units = signal<InventoryUnit[]>([]);
  warehouses = signal<Warehouse[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  loadMasterData(): void {
    this.error.set(null);
    this.repo.getCategories().pipe(
      catchError(err => {
        this.error.set(err?.error?.message ?? err?.error?.error ?? 'Failed to load categories.');
        return of([] as InventoryCategory[]);
      }),
      tap(c => this.categories.set(c))
    ).subscribe();

    this.repo.getUnits().pipe(
      catchError(err => {
        if (!this.error()) {
          this.error.set(err?.error?.message ?? err?.error?.error ?? 'Failed to load units.');
        }
        return of([] as InventoryUnit[]);
      }),
      tap(u => this.units.set(u))
    ).subscribe();
  }

  loadCategories(): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo.getCategories().pipe(
      catchError(err => {
        this.error.set(err?.error?.error ?? 'Failed to load categories.');
        return of([] as InventoryCategory[]);
      }),
      tap(c => {
        this.categories.set(c);
        this.loading.set(false);
      })
    ).subscribe();
  }

  loadUnits(): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo.getUnits().pipe(
      catchError(err => {
        this.error.set(err?.error?.error ?? 'Failed to load units.');
        return of([] as InventoryUnit[]);
      }),
      tap(u => {
        this.units.set(u);
        this.loading.set(false);
      })
    ).subscribe();
  }

  loadWarehouses(): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo.getWarehouses({ isActive: true, pageSize: 200 }).pipe(
      catchError(err => {
        this.error.set(err?.error?.error ?? 'Failed to load warehouses.');
        return of([] as Warehouse[]);
      }),
      tap(w => {
        this.warehouses.set(w);
        this.loading.set(false);
      })
    ).subscribe();
  }

  loadItems(search?: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo.getItems(search, 1, 200).pipe(
      catchError(err => {
        this.error.set(err?.error?.error ?? 'Failed to load inventory items.');
        return of([] as InventoryItemDefinition[]);
      }),
      tap(items => {
        this.items.set(items);
        this.loading.set(false);
      })
    ).subscribe();
  }

  getItem(id: string) {
    return this.repo.getItemById(id);
  }

  createItem(payload: CreateInventoryItemPayload) {
    return this.repo.createItem(payload);
  }

  updateItem(id: string, payload: UpdateInventoryItemPayload) {
    return this.repo.updateItem(id, payload);
  }

  deleteItem(id: string) {
    return this.repo.deleteItem(id);
  }

  activateItem(id: string) {
    return this.repo.activateItem(id);
  }

  createCategory(payload: CreateInventoryCategoryPayload) {
    return this.repo.createCategory(payload);
  }

  updateCategory(id: string, payload: UpdateInventoryCategoryPayload) {
    return this.repo.updateCategory(id, payload);
  }

  activateCategory(id: string) {
    return this.repo.activateCategory(id);
  }

  deactivateCategory(id: string) {
    return this.repo.deactivateCategory(id);
  }

  deleteCategory(id: string) {
    return this.repo.deleteCategory(id);
  }

  createUnit(payload: CreateInventoryUnitPayload) {
    return this.repo.createUnit(payload);
  }

  updateUnit(id: string, payload: UpdateInventoryUnitPayload) {
    return this.repo.updateUnit(id, payload);
  }

  activateUnit(id: string) {
    return this.repo.activateUnit(id);
  }

  deactivateUnit(id: string) {
    return this.repo.deactivateUnit(id);
  }

  deleteUnit(id: string) {
    return this.repo.deleteUnit(id);
  }

  createWarehouse(payload: CreateWarehousePayload) {
    return this.repo.createWarehouse(payload);
  }

  updateWarehouse(id: string, payload: UpdateWarehousePayload) {
    return this.repo.updateWarehouse(id, payload);
  }

  deleteWarehouse(id: string) {
    return this.repo.deleteWarehouse(id);
  }

  getWarehouseTypes() {
    return this.repo.getWarehouseTypes();
  }

  getBranchesLookup() {
    return this.repo.getBranchesLookup();
  }

  activateWarehouse(id: string) {
    return this.repo.activateWarehouse(id);
  }

  deactivateWarehouse(id: string) {
    return this.repo.deactivateWarehouse(id);
  }

  getStockByWarehouse(itemId: string) {
    return this.repo.getStockByWarehouse(itemId);
  }

  getItemMovements(itemId: string) {
    return this.repo.getItemMovements(itemId);
  }

  getItemPurchaseHistory(itemId: string) {
    return this.repo.getItemPurchaseHistory(itemId);
  }

  getItemSalesHistory(itemId: string) {
    return this.repo.getItemSalesHistory(itemId);
  }

  getLedger(page?: number, pageSize?: number) {
    return this.repo.getLedger(page, pageSize);
  }

  getTransfers(page?: number, pageSize?: number) {
    return this.repo.getTransfers(page, pageSize);
  }

  createTransfer(payload: CreateStockTransferPayload) {
    return this.repo.createTransfer(payload);
  }

  completeTransfer(id: string) {
    return this.repo.completeTransfer(id);
  }

  getAdjustments(page?: number, pageSize?: number) {
    return this.repo.getAdjustments(page, pageSize);
  }

  createAdjustment(payload: CreateStockAdjustmentPayload) {
    return this.repo.createAdjustment(payload);
  }

  confirmAdjustment(id: string) {
    return this.repo.confirmAdjustment(id);
  }

  getWaste(page?: number, pageSize?: number) {
    return this.repo.getWaste(page, pageSize);
  }

  createWaste(payload: CreateWastePayload) {
    return this.repo.createWaste(payload);
  }

  confirmWaste(id: string) {
    return this.repo.confirmWaste(id);
  }

  getGoodsReceipts(page?: number, pageSize?: number) {
    return this.repo.getGoodsReceipts(page, pageSize);
  }

  createGoodsReceipt(payload: CreateGoodsReceiptPayload) {
    return this.repo.createGoodsReceipt(payload);
  }

  confirmGoodsReceipt(id: string) {
    return this.repo.confirmGoodsReceipt(id);
  }

  getStockCounts(page?: number, pageSize?: number) {
    return this.repo.getStockCounts(page, pageSize);
  }

  createStockCount(payload: CreateStockCountPayload) {
    return this.repo.createStockCount(payload);
  }

  approveStockCount(id: string) {
    return this.repo.approveStockCount(id);
  }

  getPurchaseReturns(page?: number, pageSize?: number) {
    return this.repo.getPurchaseReturns(page, pageSize);
  }

  createPurchaseReturn(payload: CreatePurchaseReturnPayload) {
    return this.repo.createPurchaseReturn(payload);
  }

  approvePurchaseReturn(id: string) {
    return this.repo.approvePurchaseReturn(id);
  }

  getPurchaseOrders(page?: number, pageSize?: number) {
    return this.repo.getPurchaseOrders(page, pageSize);
  }

  getSuppliers(page?: number, pageSize?: number) {
    return this.repo.getSuppliers(page, pageSize);
  }

  getDashboard() {
    return this.repo.getDashboard();
  }

  getReservations(page?: number, pageSize?: number) {
    return this.repo.getReservations(page, pageSize);
  }

  createReservation(payload: CreateReservationPayload) {
    return this.repo.createReservation(payload);
  }

  releaseReservation(id: string) {
    return this.repo.releaseReservation(id);
  }

  expireReservation(id: string) {
    return this.repo.expireReservation(id);
  }

  getSettings(branchId?: string | null) {
    return this.repo.getSettings(branchId);
  }

  upsertSettings(payload: UpsertInventorySettingPayload) {
    return this.repo.upsertSettings(payload);
  }

  resetSettings(branchId?: string | null) {
    return this.repo.resetSettings(branchId);
  }

  getWarehouseById(id: string) {
    return this.repo.getWarehouseById(id);
  }

  addWarehouseZone(warehouseId: string, payload: import('../models/inventory.models').AddWarehouseLocationPayload) {
    return this.repo.addWarehouseZone(warehouseId, payload);
  }

  removeWarehouseZone(warehouseId: string, zoneId: string) {
    return this.repo.removeWarehouseZone(warehouseId, zoneId);
  }

  addWarehouseShelf(warehouseId: string, zoneId: string, payload: import('../models/inventory.models').AddWarehouseLocationPayload) {
    return this.repo.addWarehouseShelf(warehouseId, zoneId, payload);
  }

  removeWarehouseShelf(warehouseId: string, zoneId: string, shelfId: string) {
    return this.repo.removeWarehouseShelf(warehouseId, zoneId, shelfId);
  }

  addWarehouseBin(warehouseId: string, zoneId: string, shelfId: string, payload: import('../models/inventory.models').AddWarehouseLocationPayload) {
    return this.repo.addWarehouseBin(warehouseId, zoneId, shelfId, payload);
  }

  removeWarehouseBin(warehouseId: string, zoneId: string, shelfId: string, binId: string) {
    return this.repo.removeWarehouseBin(warehouseId, zoneId, shelfId, binId);
  }

  getBrands() { return this.repo.getBrands(); }
  createBrand(payload: import('../models/inventory.models').UpsertInventoryBrandPayload) { return this.repo.createBrand(payload); }
  updateBrand(id: string, payload: import('../models/inventory.models').UpsertInventoryBrandPayload) { return this.repo.updateBrand(id, payload); }
  activateBrand(id: string) { return this.repo.activateBrand(id); }
  deactivateBrand(id: string) { return this.repo.deactivateBrand(id); }

  getManufacturers() { return this.repo.getManufacturers(); }
  createManufacturer(payload: import('../models/inventory.models').UpsertInventoryManufacturerPayload) { return this.repo.createManufacturer(payload); }
  updateManufacturer(id: string, payload: import('../models/inventory.models').UpsertInventoryManufacturerPayload) { return this.repo.updateManufacturer(id, payload); }
  activateManufacturer(id: string) { return this.repo.activateManufacturer(id); }
  deactivateManufacturer(id: string) { return this.repo.deactivateManufacturer(id); }

  getAttributes() { return this.repo.getAttributes(); }
  createAttribute(payload: import('../models/inventory.models').UpsertInventoryAttributePayload) { return this.repo.createAttribute(payload); }
  updateAttribute(id: string, payload: import('../models/inventory.models').UpsertInventoryAttributePayload) { return this.repo.updateAttribute(id, payload); }
  activateAttribute(id: string) { return this.repo.activateAttribute(id); }
  deactivateAttribute(id: string) { return this.repo.deactivateAttribute(id); }
  addAttributeValue(attributeId: string, valueAr: string, valueEn?: string) { return this.repo.addAttributeValue(attributeId, valueAr, valueEn); }
  removeAttributeValue(attributeId: string, valueId: string) { return this.repo.removeAttributeValue(attributeId, valueId); }

  getPriceLists() { return this.repo.getPriceLists(); }
  getPriceListById(id: string) { return this.repo.getPriceListById(id); }
  createPriceList(payload: import('../models/inventory.models').UpsertInventoryPriceListPayload) { return this.repo.createPriceList(payload); }
  updatePriceList(id: string, payload: import('../models/inventory.models').UpsertInventoryPriceListPayload) { return this.repo.updatePriceList(id, payload); }
  activatePriceList(id: string) { return this.repo.activatePriceList(id); }
  deactivatePriceList(id: string) { return this.repo.deactivatePriceList(id); }
  upsertPriceListLine(priceListId: string, payload: import('../models/inventory.models').UpsertInventoryPriceListLinePayload) { return this.repo.upsertPriceListLine(priceListId, payload); }
  removePriceListLine(priceListId: string, lineId: string) { return this.repo.removePriceListLine(priceListId, lineId); }

  getTaxGroups() { return this.repo.getTaxGroups(); }
  createTaxGroup(payload: { nameAr: string; nameEn?: string; description?: string }) { return this.repo.createTaxGroup(payload); }
  updateTaxGroup(id: string, payload: { nameAr: string; nameEn?: string; description?: string }) { return this.repo.updateTaxGroup(id, payload); }
}
