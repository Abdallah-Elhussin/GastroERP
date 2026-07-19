import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { InventoryService } from '../../../core/services/inventory.service';
import { StockTransferRepository } from '../../../core/repositories/stock-transfer.repository';
import {
  CreateStockTransferPayload,
  StockTransferLine,
  UpdateStockTransferPayload
} from '../../../core/models/stock-transfer.models';
import { InventoryItemDefinition, InventoryUnit, Warehouse } from '../../../core/models/inventory.models';
import { InventoryPageShellComponent } from '../shared/inventory-page-shell.component';

@Component({
  selector: 'app-inventory-stock-transfer-form-page',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, InventoryPageShellComponent],
  templateUrl: './inventory-stock-transfer-form.page.html',
  styleUrl: './inventory-stock-transfer-form.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryStockTransferFormPage implements OnInit {
  private repo = inject(StockTransferRepository);
  private inventory = inject(InventoryService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);

  docId = signal<string | null>(null);
  transferNumber = signal('');
  transferDate = signal(this.todayIso());
  status = signal('Draft');
  transferType = signal(1);
  notes = signal('');
  sourceWarehouseId = signal<string | null>(null);
  destinationWarehouseId = signal<string | null>(null);
  lines = signal<StockTransferLine[]>([]);
  selectedLineIndex = signal<number | null>(null);

  items = signal<InventoryItemDefinition[]>([]);
  units = signal<InventoryUnit[]>([]);
  warehouses = signal<Warehouse[]>([]);

  breadcrumbs = [
    { labelKey: 'nav.inventory', path: '/inventory/dashboard' },
    { labelKey: 'inv.nav.stockTransfer', path: '/inventory/stock-transfers' },
    { labelKey: 'inv.st.formBreadcrumb' }
  ];

  canManage = computed(() =>
    this.auth.hasPermission('Stock.Transfer') || this.auth.hasPermission('Inventory.Manage')
  );
  isDraft = computed(() => this.status() === 'Draft');
  isNew = computed(() => !this.docId());
  pageTitle = computed(() =>
    this.isNew() ? this.t('inv.st.createTitle') : `${this.t('inv.nav.stockTransfer')} — ${this.transferNumber()}`
  );
  statusLabel = computed(() => {
    switch (this.status()) {
      case 'Approved': return this.t('inv.st.status.approved');
      case 'InTransit': return this.t('inv.st.status.posted');
      case 'Completed': return this.t('inv.st.status.received');
      case 'Cancelled': return this.t('inv.st.status.cancelled');
      default: return this.t('inv.st.status.draft');
    }
  });

  ngOnInit(): void {
    this.inventory.loadWarehouses();
    this.inventory.loadItems();
    this.inventory.loadUnits();
    const pull = () => {
      this.warehouses.set([...this.inventory.warehouses()]);
      this.items.set([...this.inventory.items()]);
      this.units.set([...this.inventory.units()]);
      if (!this.sourceWarehouseId() && this.warehouses().length) {
        this.sourceWarehouseId.set(this.warehouses()[0].id);
      }
      if (!this.destinationWarehouseId() && this.warehouses().length > 1) {
        this.destinationWarehouseId.set(this.warehouses()[1].id);
      }
    };
    pull();
    const timer = setInterval(pull, 500);
    setTimeout(() => clearInterval(timer), 5000);

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.loadDoc(id);
    } else {
      this.repo.nextNumber().subscribe({
        next: n => this.transferNumber.set(typeof n === 'string' ? n : String(n)),
        error: () => {
          const stamp = new Date().toISOString().replace(/\D/g, '').slice(0, 14);
          this.transferNumber.set(`TR${stamp}`);
        }
      });
    }
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  addLine(): void {
    if (!this.isDraft()) return;
    const item = this.items()[0];
    const unit = this.units()[0];
    if (!item || !unit) {
      this.error.set(this.t('inv.st.validation.masters'));
      return;
    }
    this.lines.update(rows => [
      ...rows,
      {
        inventoryItemId: item.id,
        unitId: item.baseUnitId || unit.id,
        quantity: 1,
        unitCost: 0,
        receivedQuantity: 0
      }
    ]);
    this.selectedLineIndex.set(this.lines().length - 1);
  }

  removeLine(): void {
    const idx = this.selectedLineIndex();
    if (idx == null || !this.isDraft()) return;
    this.lines.update(rows => rows.filter((_, i) => i !== idx));
    this.selectedLineIndex.set(null);
  }

  selectLine(index: number): void {
    this.selectedLineIndex.set(index);
  }

  patchLine(index: number, patch: Partial<StockTransferLine>): void {
    this.lines.update(rows => rows.map((row, i) => (i === index ? { ...row, ...patch } : row)));
  }

  onItemChange(index: number, itemId: string): void {
    const item = this.items().find(i => i.id === itemId);
    this.patchLine(index, {
      inventoryItemId: itemId,
      unitId: item?.baseUnitId || this.lines()[index].unitId
    });
  }

  save(): void {
    if (!this.canManage() || !this.isDraft()) return;
    if (!this.sourceWarehouseId() || !this.destinationWarehouseId()) {
      this.error.set(this.t('inv.st.validation.warehouses'));
      return;
    }
    if (this.sourceWarehouseId() === this.destinationWarehouseId()) {
      this.error.set(this.t('inv.st.validation.sameWarehouse'));
      return;
    }
    if (this.lines().length === 0) {
      this.error.set(this.t('inv.st.validation.lines'));
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    const linePayload = this.lines().map(l => ({
      inventoryItemId: l.inventoryItemId,
      unitId: l.unitId,
      quantity: l.quantity,
      unitCost: l.unitCost,
      batchNumber: l.batchNumber
    }));

    if (this.docId()) {
      const payload: UpdateStockTransferPayload = {
        transferDate: new Date(this.transferDate()).toISOString(),
        sourceWarehouseId: this.sourceWarehouseId()!,
        destinationWarehouseId: this.destinationWarehouseId()!,
        transferType: this.transferType(),
        notes: this.notes() || null,
        lines: linePayload
      };
      this.repo.update(this.docId()!, payload).subscribe({
        next: doc => {
          this.applyDoc(doc);
          this.saving.set(false);
        },
        error: err => {
          this.error.set(err?.error?.error ?? this.t('inv.st.saveFailed'));
          this.saving.set(false);
        }
      });
      return;
    }

    const payload: CreateStockTransferPayload = {
      transferNumber: this.transferNumber(),
      autoGenerateNumber: false,
      transferDate: new Date(this.transferDate()).toISOString(),
      sourceWarehouseId: this.sourceWarehouseId()!,
      destinationWarehouseId: this.destinationWarehouseId()!,
      transferType: this.transferType(),
      notes: this.notes() || null,
      lines: linePayload
    };
    this.repo.create(payload).subscribe({
      next: doc => {
        this.applyDoc(doc);
        this.saving.set(false);
        void this.router.navigate(['/inventory/stock-transfers', doc.id], { replaceUrl: true });
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('inv.st.saveFailed'));
        this.saving.set(false);
      }
    });
  }

  cancel(): void {
    void this.router.navigate(['/inventory/stock-transfers']);
  }

  private loadDoc(id: string): void {
    this.loading.set(true);
    this.repo.getById(id).subscribe({
      next: doc => {
        this.applyDoc(doc);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('inv.st.loadFailed'));
        this.loading.set(false);
      }
    });
  }

  private applyDoc(doc: import('../../../core/models/stock-transfer.models').StockTransferDoc): void {
    this.docId.set(doc.id);
    this.transferNumber.set(doc.transferNumber);
    this.transferDate.set(doc.transferDate?.slice(0, 10) || this.todayIso());
    this.status.set(doc.status);
    this.transferType.set(doc.transferTypeCode || 1);
    this.notes.set(doc.notes || '');
    this.sourceWarehouseId.set(doc.sourceWarehouseId);
    this.destinationWarehouseId.set(doc.destinationWarehouseId);
    this.lines.set(
      (doc.lines ?? []).map(l => ({
        id: l.id,
        inventoryItemId: l.inventoryItemId,
        itemNameAr: l.itemNameAr,
        unitId: l.unitId,
        unitNameAr: l.unitNameAr,
        quantity: l.quantity,
        unitCost: l.unitCost,
        lineTotal: l.lineTotal,
        receivedQuantity: l.receivedQuantity ?? 0,
        batchNumber: l.batchNumber
      }))
    );
  }

  private todayIso(): string {
    return new Date().toISOString().slice(0, 10);
  }
}
