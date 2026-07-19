import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  inject,
  signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { catchError, of } from 'rxjs';
import { LanguageService } from '../../../core/services/language.service';
import { InventoryService } from '../../../core/services/inventory.service';
import {
  CreateGoodsReceiptPayload,
  CreatePurchaseReturnPayload,
  CreateStockAdjustmentPayload,
  CreateStockCountPayload,
  CreateStockTransferPayload,
  CreateWastePayload,
  CreateReservationPayload,
  GoodsReceiptRecord,
  InventoryLedgerEntry,
  InventoryReservationRecord,
  PurchaseOrderSummary,
  PurchaseReturnRecord,
  StockAdjustmentRecord,
  StockCountRecord,
  StockTransferRecord,
  SupplierSummary,
  WasteRecord
} from '../../../core/models/inventory.models';
import { InventoryPageShellComponent } from '../shared/inventory-page-shell.component';
import { InventorySkeletonComponent } from '../shared/inventory-skeleton.component';
import { InventoryEmptyStateComponent } from '../shared/inventory-empty-state.component';
import { InventoryErrorStateComponent } from '../shared/inventory-error-state.component';

type OpsTab = 'ledger' | 'transfer' | 'adjust' | 'waste' | 'grn' | 'count' | 'return' | 'reserve';

@Component({
  selector: 'app-inventory-operations-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    InventoryPageShellComponent,
    InventorySkeletonComponent,
    InventoryEmptyStateComponent,
    InventoryErrorStateComponent
  ],
  templateUrl: './inventory-operations.page.html',
  styleUrl: './inventory-operations.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryOperationsPage implements OnInit {
  lang = inject(LanguageService);
  inventory = inject(InventoryService);

  tab = signal<OpsTab>('ledger');
  loading = signal(false);
  error = signal<string | null>(null);
  saving = signal(false);
  formError = signal<string | null>(null);
  showForm = signal(false);

  ledger = signal<InventoryLedgerEntry[]>([]);
  transfers = signal<StockTransferRecord[]>([]);
  adjustments = signal<StockAdjustmentRecord[]>([]);
  waste = signal<WasteRecord[]>([]);
  receipts = signal<GoodsReceiptRecord[]>([]);
  counts = signal<StockCountRecord[]>([]);
  returns = signal<PurchaseReturnRecord[]>([]);
  reservations = signal<InventoryReservationRecord[]>([]);
  purchaseOrders = signal<PurchaseOrderSummary[]>([]);
  suppliers = signal<SupplierSummary[]>([]);

  transferForm: CreateStockTransferPayload = this.emptyTransfer();
  adjustForm: CreateStockAdjustmentPayload = this.emptyAdjust();
  wasteForm: CreateWastePayload = this.emptyWaste();
  grnForm: CreateGoodsReceiptPayload = this.emptyGrn();
  countForm: CreateStockCountPayload = this.emptyCount();
  returnForm: CreatePurchaseReturnPayload = this.emptyReturn();
  reserveForm: CreateReservationPayload = this.emptyReserve();

  breadcrumbs = [
    { labelKey: 'nav.inventory', path: '/inventory/dashboard' },
    { labelKey: 'inv.nav.transactions' }
  ];

  tabs: { id: OpsTab; labelKey: string; icon: string }[] = [
    { id: 'ledger', labelKey: 'inv.ops.tab.ledger', icon: 'receipt_long' },
    { id: 'transfer', labelKey: 'inv.ops.tab.transfer', icon: 'swap_horiz' },
    { id: 'adjust', labelKey: 'inv.ops.tab.adjust', icon: 'tune' },
    { id: 'waste', labelKey: 'inv.ops.tab.waste', icon: 'delete_forever' },
    { id: 'grn', labelKey: 'inv.ops.tab.grn', icon: 'local_shipping' },
    { id: 'count', labelKey: 'inv.ops.tab.count', icon: 'fact_check' },
    { id: 'return', labelKey: 'inv.ops.tab.return', icon: 'assignment_return' },
    { id: 'reserve', labelKey: 'inv.ops.tab.reserve', icon: 'lock_clock' }
  ];

  ngOnInit(): void {
    this.inventory.loadWarehouses();
    this.inventory.loadItems();
    this.inventory.loadUnits();
    this.loadTab('ledger');
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  selectTab(id: OpsTab): void {
    this.tab.set(id);
    this.showForm.set(false);
    this.formError.set(null);
    this.loadTab(id);
  }

  openCreate(): void {
    this.resetForm();
    this.formError.set(null);
    this.showForm.set(true);
  }

  closeForm(): void {
    this.showForm.set(false);
  }

  reload(): void {
    this.loadTab(this.tab());
  }

  save(): void {
    const id = this.tab();
    this.saving.set(true);
    this.formError.set(null);

    const done = () => {
      this.saving.set(false);
      this.showForm.set(false);
      this.loadTab(id);
    };
    const fail = (err: { error?: { error?: string } }) => {
      this.saving.set(false);
      this.formError.set(err?.error?.error ?? this.t('inv.saveFailed'));
    };

    if (id === 'transfer') {
      if (!this.transferForm.sourceWarehouseId || !this.transferForm.destinationWarehouseId || !this.transferForm.inventoryItemId) {
        this.saving.set(false);
        this.formError.set(this.t('inv.ops.validation.required'));
        return;
      }
      this.inventory.createTransfer({ ...this.transferForm, complete: true }).subscribe({ next: done, error: fail });
      return;
    }
    if (id === 'adjust') {
      if (!this.adjustForm.warehouseId || !this.adjustForm.inventoryItemId || !this.adjustForm.quantityAdjusted) {
        this.saving.set(false);
        this.formError.set(this.t('inv.ops.validation.required'));
        return;
      }
      this.inventory.createAdjustment({ ...this.adjustForm, confirm: true }).subscribe({ next: done, error: fail });
      return;
    }
    if (id === 'waste') {
      if (!this.wasteForm.warehouseId || !this.wasteForm.inventoryItemId || this.wasteForm.quantity <= 0) {
        this.saving.set(false);
        this.formError.set(this.t('inv.ops.validation.required'));
        return;
      }
      this.inventory.createWaste({ ...this.wasteForm, confirm: true }).subscribe({ next: done, error: fail });
      return;
    }
    if (id === 'grn') {
      if (!this.grnForm.purchaseOrderId || !this.grnForm.warehouseId || !this.grnForm.inventoryItemId) {
        this.saving.set(false);
        this.formError.set(this.t('inv.ops.validation.required'));
        return;
      }
      this.inventory.createGoodsReceipt({ ...this.grnForm, confirm: true }).subscribe({ next: done, error: fail });
      return;
    }
    if (id === 'count') {
      if (!this.countForm.warehouseId || !this.countForm.inventoryItemId) {
        this.saving.set(false);
        this.formError.set(this.t('inv.ops.validation.required'));
        return;
      }
      this.inventory.createStockCount(this.countForm).subscribe({ next: done, error: fail });
      return;
    }
    if (id === 'return') {
      if (!this.returnForm.supplierId || !this.returnForm.warehouseId || !this.returnForm.inventoryItemId) {
        this.saving.set(false);
        this.formError.set(this.t('inv.ops.validation.required'));
        return;
      }
      this.inventory.createPurchaseReturn({ ...this.returnForm, approve: true }).subscribe({ next: done, error: fail });
      return;
    }
    if (id === 'reserve') {
      if (!this.reserveForm.warehouseId || !this.reserveForm.inventoryItemId || this.reserveForm.reservedQuantity <= 0) {
        this.saving.set(false);
        this.formError.set(this.t('inv.ops.validation.required'));
        return;
      }
      this.inventory.createReservation(this.reserveForm).subscribe({ next: done, error: fail });
    }
  }

  confirmTransfer(id: string): void {
    this.inventory.completeTransfer(id).subscribe({ next: () => this.reload() });
  }

  confirmAdjustment(id: string): void {
    this.inventory.confirmAdjustment(id).subscribe({ next: () => this.reload() });
  }

  confirmWaste(id: string): void {
    this.inventory.confirmWaste(id).subscribe({ next: () => this.reload() });
  }

  confirmGrn(id: string): void {
    this.inventory.confirmGoodsReceipt(id).subscribe({ next: () => this.reload() });
  }

  approveCount(id: string): void {
    this.inventory.approveStockCount(id).subscribe({ next: () => this.reload() });
  }

  approveReturn(id: string): void {
    this.inventory.approvePurchaseReturn(id).subscribe({ next: () => this.reload() });
  }

  releaseReservation(id: string): void {
    this.inventory.releaseReservation(id).subscribe({ next: () => this.reload() });
  }

  expireReservation(id: string): void {
    this.inventory.expireReservation(id).subscribe({ next: () => this.reload() });
  }

  onItemPicked(itemId: string, target: 'transfer' | 'adjust' | 'waste' | 'grn' | 'count' | 'return' | 'reserve'): void {
    const item = this.inventory.items().find(i => i.id === itemId);
    if (!item) return;
    if (target === 'transfer') {
      this.transferForm.inventoryItemId = itemId;
      this.transferForm.unitId = item.baseUnitId;
    } else if (target === 'adjust') {
      this.adjustForm.inventoryItemId = itemId;
      this.adjustForm.unitId = item.baseUnitId;
      this.adjustForm.unitCost = item.averageUnitCost ?? item.lastPurchaseUnitCost ?? 0;
    } else if (target === 'waste') {
      this.wasteForm.inventoryItemId = itemId;
      this.wasteForm.unitId = item.baseUnitId;
      this.wasteForm.unitCost = item.averageUnitCost ?? item.lastPurchaseUnitCost ?? 0;
    } else if (target === 'grn') {
      this.grnForm.inventoryItemId = itemId;
      this.grnForm.unitId = item.baseUnitId;
      this.grnForm.unitCost = item.lastPurchaseUnitCost ?? item.averageUnitCost ?? 0;
    } else if (target === 'count') {
      this.countForm.inventoryItemId = itemId;
      this.countForm.unitId = item.baseUnitId;
    } else if (target === 'reserve') {
      this.reserveForm.inventoryItemId = itemId;
    } else {
      this.returnForm.inventoryItemId = itemId;
      this.returnForm.unitId = item.baseUnitId;
      this.returnForm.unitCost = item.lastPurchaseUnitCost ?? item.averageUnitCost ?? 0;
    }
  }

  onPoPicked(poId: string): void {
    const po = this.purchaseOrders().find(p => p.id === poId);
    if (!po) return;
    this.grnForm.purchaseOrderId = poId;
    this.grnForm.warehouseId = po.destinationWarehouseId;
  }

  private loadTab(id: OpsTab): void {
    this.loading.set(true);
    this.error.set(null);

    if (id === 'ledger') {
      this.inventory.getLedger().pipe(
        catchError(e => {
          this.setError(e);
          return of([] as InventoryLedgerEntry[]);
        })
      ).subscribe(rows => {
        this.ledger.set(rows);
        this.loading.set(false);
      });
      return;
    }
    if (id === 'transfer') {
      this.inventory.getTransfers().pipe(
        catchError(e => {
          this.setError(e);
          return of([] as StockTransferRecord[]);
        })
      ).subscribe(rows => {
        this.transfers.set(rows);
        this.loading.set(false);
      });
      return;
    }
    if (id === 'adjust') {
      this.inventory.getAdjustments().pipe(
        catchError(e => {
          this.setError(e);
          return of([] as StockAdjustmentRecord[]);
        })
      ).subscribe(rows => {
        this.adjustments.set(rows);
        this.loading.set(false);
      });
      return;
    }
    if (id === 'waste') {
      this.inventory.getWaste().pipe(
        catchError(e => {
          this.setError(e);
          return of([] as WasteRecord[]);
        })
      ).subscribe(rows => {
        this.waste.set(rows);
        this.loading.set(false);
      });
      return;
    }
    if (id === 'grn') {
      this.inventory.getPurchaseOrders().pipe(catchError(() => of([] as PurchaseOrderSummary[]))).subscribe(pos => this.purchaseOrders.set(pos));
      this.inventory.getGoodsReceipts().pipe(
        catchError(e => {
          this.setError(e);
          return of([] as GoodsReceiptRecord[]);
        })
      ).subscribe(rows => {
        this.receipts.set(rows);
        this.loading.set(false);
      });
      return;
    }
    if (id === 'count') {
      this.inventory.getStockCounts().pipe(
        catchError(e => {
          this.setError(e);
          return of([] as StockCountRecord[]);
        })
      ).subscribe(rows => {
        this.counts.set(rows);
        this.loading.set(false);
      });
      return;
    }
    if (id === 'reserve') {
      this.inventory.getReservations().pipe(
        catchError(e => {
          this.setError(e);
          return of([] as InventoryReservationRecord[]);
        })
      ).subscribe(rows => {
        this.reservations.set(rows);
        this.loading.set(false);
      });
      return;
    }
    this.inventory.getSuppliers().pipe(catchError(() => of([] as SupplierSummary[]))).subscribe(s => this.suppliers.set(s));
    this.inventory.getPurchaseReturns().pipe(
      catchError(e => {
        this.setError(e);
        return of([] as PurchaseReturnRecord[]);
      })
    ).subscribe(rows => {
      this.returns.set(rows);
      this.loading.set(false);
    });
  }

  private setError(err: { error?: { error?: string } }): void {
    this.error.set(err?.error?.error ?? this.t('inv.error.message'));
    this.loading.set(false);
  }

  private resetForm(): void {
    const n = Date.now().toString().slice(-6);
    const firstWh = this.inventory.warehouses()[0]?.id ?? '';
    const firstItem = this.inventory.items()[0];
    const unitId = firstItem?.baseUnitId ?? '';
    const itemId = firstItem?.id ?? '';

    this.transferForm = {
      ...this.emptyTransfer(),
      transferNumber: `TR-${n}`,
      sourceWarehouseId: firstWh,
      destinationWarehouseId: this.inventory.warehouses()[1]?.id ?? firstWh,
      inventoryItemId: itemId,
      unitId
    };
    this.adjustForm = {
      ...this.emptyAdjust(),
      adjustmentNumber: `ADJ-${n}`,
      warehouseId: firstWh,
      inventoryItemId: itemId,
      unitId,
      unitCost: firstItem?.averageUnitCost ?? 0
    };
    this.wasteForm = {
      ...this.emptyWaste(),
      wasteNumber: `WST-${n}`,
      warehouseId: firstWh,
      inventoryItemId: itemId,
      unitId,
      unitCost: firstItem?.averageUnitCost ?? 0
    };
    this.grnForm = {
      ...this.emptyGrn(),
      grnNumber: `GRN-${n}`,
      warehouseId: firstWh,
      inventoryItemId: itemId,
      unitId,
      purchaseOrderId: this.purchaseOrders()[0]?.id ?? '',
      unitCost: firstItem?.lastPurchaseUnitCost ?? 0
    };
    this.countForm = {
      ...this.emptyCount(),
      countNumber: `CNT-${n}`,
      warehouseId: firstWh,
      inventoryItemId: itemId,
      unitId
    };
    this.returnForm = {
      ...this.emptyReturn(),
      returnNumber: `PR-${n}`,
      warehouseId: firstWh,
      inventoryItemId: itemId,
      unitId,
      supplierId: this.suppliers()[0]?.id ?? '',
      unitCost: firstItem?.lastPurchaseUnitCost ?? 0
    };
    this.reserveForm = {
      ...this.emptyReserve(),
      warehouseId: firstWh,
      inventoryItemId: itemId,
      sourceDocument: `MANUAL-${n}`,
      reservedQuantity: 1
    };
  }

  private emptyTransfer(): CreateStockTransferPayload {
    return {
      sourceWarehouseId: '',
      destinationWarehouseId: '',
      transferNumber: '',
      inventoryItemId: '',
      unitId: '',
      quantity: 1,
      complete: true
    };
  }

  private emptyAdjust(): CreateStockAdjustmentPayload {
    return {
      warehouseId: '',
      inventoryItemId: '',
      adjustmentNumber: '',
      quantityAdjusted: 1,
      unitId: '',
      unitCost: 0,
      confirm: true
    };
  }

  private emptyWaste(): CreateWastePayload {
    return {
      warehouseId: '',
      inventoryItemId: '',
      unitId: '',
      wasteNumber: '',
      quantity: 1,
      unitCost: 0,
      confirm: true
    };
  }

  private emptyGrn(): CreateGoodsReceiptPayload {
    return {
      purchaseOrderId: '',
      warehouseId: '',
      grnNumber: '',
      inventoryItemId: '',
      unitId: '',
      receivedQuantity: 1,
      unitCost: 0,
      confirm: true
    };
  }

  private emptyCount(): CreateStockCountPayload {
    return {
      warehouseId: '',
      countNumber: '',
      inventoryItemId: '',
      unitId: '',
      expectedQuantity: 0,
      actualQuantity: 0
    };
  }

  private emptyReturn(): CreatePurchaseReturnPayload {
    return {
      supplierId: '',
      warehouseId: '',
      returnNumber: '',
      inventoryItemId: '',
      unitId: '',
      returnQuantity: 1,
      unitCost: 0,
      approve: true
    };
  }

  private emptyReserve(): CreateReservationPayload {
    return {
      warehouseId: '',
      inventoryItemId: '',
      reservedQuantity: 1,
      sourceDocument: ''
    };
  }
}
