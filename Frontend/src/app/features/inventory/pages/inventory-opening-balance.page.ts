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
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { InventoryService } from '../../../core/services/inventory.service';
import { OpeningBalanceRepository } from '../../../core/repositories/opening-balance.repository';
import {
  AccountLookup,
  CreateOpeningBalancePayload,
  OpeningBalanceDoc,
  OpeningBalanceLine,
  UpdateOpeningBalancePayload
} from '../../../core/models/opening-balance.models';
import { CostCenterLookup } from '../../../core/models/inventory-valuation-group.models';
import { InventoryItemDefinition, InventoryUnit, Warehouse } from '../../../core/models/inventory.models';
import { InventoryPageShellComponent } from '../shared/inventory-page-shell.component';

@Component({
  selector: 'app-inventory-opening-balance-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatSlideToggleModule,
    MatTooltipModule,
    InventoryPageShellComponent
  ],
  templateUrl: './inventory-opening-balance.page.html',
  styleUrl: './inventory-opening-balance.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryOpeningBalancePage implements OnInit {
  private repo = inject(OpeningBalanceRepository);
  private inventory = inject(InventoryService);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  statusMessage = signal('');
  showBrowse = signal(false);
  browseRows = signal<OpeningBalanceDoc[]>([]);

  docId = signal<string | null>(null);
  documentNumber = signal('');
  documentDate = signal(this.todayIso());
  approvalDate = signal<string | null>(null);
  status = signal('Draft');
  entryMethod = signal(1);
  displayMethod = signal(1);
  costingMethod = signal(2);
  weightedAverageScope = signal(1);
  useExpiryDate = signal(false);
  useBatchNumbers = signal(false);
  useSerialNumbers = signal(false);
  contraAccountId = signal<string | null>(null);
  costCenterId = signal<string | null>(null);
  notes = signal('');
  defaultWarehouseId = signal<string | null>(null);
  lines = signal<OpeningBalanceLine[]>([]);

  items = signal<InventoryItemDefinition[]>([]);
  units = signal<InventoryUnit[]>([]);
  warehouses = signal<Warehouse[]>([]);
  accounts = signal<AccountLookup[]>([]);
  costCenters = signal<CostCenterLookup[]>([]);

  // draft line editor
  draftItemId = signal<string | null>(null);
  draftWarehouseId = signal<string | null>(null);
  draftUnitId = signal<string | null>(null);
  draftQty = signal(1);
  draftCost = signal(0);

  breadcrumbs = [
    { labelKey: 'nav.inventory', path: '/inventory/dashboard' },
    { labelKey: 'inv.nav.openingBalance' }
  ];

  canManage = computed(() => this.auth.hasPermission('Inventory.Manage'));
  isDraft = computed(() => this.status() === 'Draft');
  isApproved = computed(() => this.status() === 'Approved');
  isPosted = computed(() => this.status() === 'Posted');
  lineCount = computed(() => this.lines().length);

  statusLabel = computed(() => {
    switch (this.status()) {
      case 'Approved': return this.t('inv.ob.status.approved');
      case 'Posted': return this.t('inv.ob.status.posted');
      default: return this.t('inv.ob.status.draft');
    }
  });

  ngOnInit(): void {
    this.statusMessage.set(this.t('inv.ob.ready'));
    this.inventory.loadWarehouses();
    this.inventory.loadItems();
    this.inventory.loadUnits();

    // Reflect InventoryService master data into local signals after loads settle.
    const pull = () => {
      this.warehouses.set([...this.inventory.warehouses()]);
      this.items.set([...this.inventory.items()]);
      this.units.set([...this.inventory.units()]);
      if (!this.defaultWarehouseId() && this.warehouses().length) {
        this.defaultWarehouseId.set(this.warehouses()[0].id);
      }
    };
    pull();
    const timer = setInterval(pull, 500);
    setTimeout(() => clearInterval(timer), 5000);

    this.repo.getAccounts().subscribe({ next: a => this.accounts.set(a ?? []), error: () => this.accounts.set([]) });
    this.repo.getCostCenters().subscribe({ next: c => this.costCenters.set(c ?? []), error: () => this.costCenters.set([]) });
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  newDocument(): void {
    this.docId.set(null);
    this.documentNumber.set('');
    this.documentDate.set(this.todayIso());
    this.approvalDate.set(null);
    this.status.set('Draft');
    this.entryMethod.set(1);
    this.displayMethod.set(1);
    this.costingMethod.set(2);
    this.weightedAverageScope.set(1);
    this.useExpiryDate.set(false);
    this.useBatchNumbers.set(false);
    this.useSerialNumbers.set(false);
    this.contraAccountId.set(null);
    this.costCenterId.set(null);
    this.notes.set('');
    this.defaultWarehouseId.set(this.warehouses()[0]?.id ?? null);
    this.lines.set([]);
    this.error.set(null);
    this.statusMessage.set(this.t('inv.ob.newReady'));
    this.autoGenerate();
  }

  autoGenerate(): void {
    this.repo.nextNumber().subscribe({
      next: n => this.documentNumber.set(typeof n === 'string' ? n : String(n)),
      error: () => {
        const stamp = new Date().toISOString().replace(/\D/g, '').slice(0, 14);
        this.documentNumber.set(`OB${stamp}`);
      }
    });
  }

  save(): void {
    if (!this.canManage() || !this.isDraft()) return;
    if (!this.documentNumber().trim() && !this.docId()) {
      this.error.set(this.t('inv.ob.validation.docNumber'));
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    const linePayload = this.lines().map(l => ({
      inventoryItemId: l.inventoryItemId,
      unitId: l.unitId,
      quantity: l.quantity,
      unitCost: l.unitCost,
      warehouseId: l.warehouseId,
      batchNumber: l.batchNumber,
      expiryDate: l.expiryDate,
      serialNumber: l.serialNumber
    }));

    if (this.docId()) {
      const payload: UpdateOpeningBalancePayload = {
        documentDate: new Date(this.documentDate()).toISOString(),
        warehouseId: this.defaultWarehouseId(),
        notes: this.notes() || null,
        entryMethod: this.entryMethod(),
        displayMethod: this.displayMethod(),
        costingMethod: this.costingMethod(),
        weightedAverageScope: this.weightedAverageScope(),
        useExpiryDate: this.useExpiryDate(),
        useBatchNumbers: this.useBatchNumbers(),
        useSerialNumbers: this.useSerialNumbers(),
        contraAccountId: this.contraAccountId(),
        costCenterId: this.costCenterId(),
        lines: linePayload
      };
      this.repo.update(this.docId()!, payload).subscribe({
        next: doc => {
          this.applyDoc(doc);
          this.saving.set(false);
          this.statusMessage.set(this.t('inv.ob.saved'));
        },
        error: err => {
          this.error.set(err?.error?.error ?? this.t('inv.ob.saveFailed'));
          this.saving.set(false);
        }
      });
      return;
    }

    const create: CreateOpeningBalancePayload = {
      documentNumber: this.documentNumber() || null,
      autoGenerateNumber: !this.documentNumber().trim(),
      documentDate: new Date(this.documentDate()).toISOString(),
      warehouseId: this.defaultWarehouseId(),
      notes: this.notes() || null,
      entryMethod: this.entryMethod(),
      displayMethod: this.displayMethod(),
      costingMethod: this.costingMethod(),
      weightedAverageScope: this.weightedAverageScope(),
      useExpiryDate: this.useExpiryDate(),
      useBatchNumbers: this.useBatchNumbers(),
      useSerialNumbers: this.useSerialNumbers(),
      contraAccountId: this.contraAccountId(),
      costCenterId: this.costCenterId(),
      lines: linePayload
    };

    this.repo.create(create).subscribe({
      next: doc => {
        this.applyDoc(doc);
        this.saving.set(false);
        this.statusMessage.set(this.t('inv.ob.saved'));
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('inv.ob.saveFailed'));
        this.saving.set(false);
      }
    });
  }

  approve(): void {
    const id = this.docId();
    if (!id || !this.isDraft()) return;
    this.saving.set(true);
    this.repo.approve(id).subscribe({
      next: () => this.reload(id, this.t('inv.ob.approved')),
      error: err => {
        this.error.set(err?.error?.error ?? this.t('inv.ob.actionFailed'));
        this.saving.set(false);
      }
    });
  }

  unapprove(): void {
    const id = this.docId();
    if (!id || !this.isApproved()) return;
    this.saving.set(true);
    this.repo.unapprove(id).subscribe({
      next: () => this.reload(id, this.t('inv.ob.unapproved')),
      error: err => {
        this.error.set(err?.error?.error ?? this.t('inv.ob.actionFailed'));
        this.saving.set(false);
      }
    });
  }

  post(): void {
    const id = this.docId();
    if (!id || !this.isApproved()) return;
    if (!this.contraAccountId()) {
      this.error.set(this.t('inv.ob.validation.contra'));
      return;
    }
    this.saving.set(true);
    this.repo.post(id).subscribe({
      next: () => this.reload(id, this.t('inv.ob.posted')),
      error: err => {
        this.error.set(err?.error?.error ?? this.t('inv.ob.actionFailed'));
        this.saving.set(false);
      }
    });
  }

  browse(): void {
    this.showBrowse.set(true);
    this.repo.getList(1, 100).subscribe({
      next: rows => this.browseRows.set(rows ?? []),
      error: () => this.browseRows.set([])
    });
  }

  openDoc(row: OpeningBalanceDoc): void {
    this.showBrowse.set(false);
    this.reload(row.id);
  }

  refresh(): void {
    const id = this.docId();
    if (id) this.reload(id);
    else {
      this.inventory.loadWarehouses();
      this.inventory.loadItems();
      this.inventory.loadUnits();
      this.statusMessage.set(this.t('inv.ob.refreshed'));
    }
  }

  addLine(): void {
    if (!this.isDraft()) return;
    const itemId = this.draftItemId();
    const whId = this.draftWarehouseId() || this.defaultWarehouseId();
    const unitId = this.draftUnitId();
    if (!itemId || !whId || !unitId) {
      this.error.set(this.t('inv.ob.validation.line'));
      return;
    }
    if (this.draftQty() <= 0) {
      this.error.set(this.t('inv.ob.validation.qty'));
      return;
    }

    const item = this.items().find(i => i.id === itemId);
    const wh = this.warehouses().find(w => w.id === whId);
    const unit = this.units().find(u => u.id === unitId);

    this.lines.update(list => [
      ...list,
      {
        inventoryItemId: itemId,
        itemNameAr: item?.nameAr,
        itemSku: item?.sku,
        warehouseId: whId,
        warehouseNameAr: wh?.nameAr,
        unitId,
        unitNameAr: unit?.nameAr ?? unit?.symbol,
        quantity: this.draftQty(),
        unitCost: this.draftCost()
      }
    ]);
    this.draftQty.set(1);
    this.draftCost.set(0);
    this.error.set(null);
  }

  removeLine(index: number): void {
    if (!this.isDraft()) return;
    this.lines.update(list => list.filter((_, i) => i !== index));
  }

  onItemPicked(itemId: string | null): void {
    this.draftItemId.set(itemId);
    const item = this.items().find(i => i.id === itemId);
    if (item?.baseUnitId) this.draftUnitId.set(item.baseUnitId);
  }

  downloadTemplate(): void {
    this.repo.downloadTemplate().subscribe({
      next: blob => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'opening-balance-template.csv';
        a.click();
        URL.revokeObjectURL(url);
      },
      error: () => this.error.set(this.t('inv.ob.templateFailed'))
    });
  }

  importExcel(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file || !this.isDraft()) return;
    const reader = new FileReader();
    reader.onload = () => {
      const text = String(reader.result ?? '');
      const rows = text.split(/\r?\n/).slice(1).filter(r => r.trim());
      const added: OpeningBalanceLine[] = [];
      for (const row of rows) {
        const [sku, whCode, unitSymbol, qtyStr, costStr] = row.split(',').map(c => c.trim());
        const item = this.items().find(i => (i.sku ?? '').toLowerCase() === sku.toLowerCase());
        const wh = this.warehouses().find(w => (w.code ?? '').toLowerCase() === whCode.toLowerCase())
          ?? this.warehouses().find(w => w.id === this.defaultWarehouseId());
        const unit = this.units().find(u =>
          (u.symbol ?? '').toLowerCase() === unitSymbol.toLowerCase()
          || (u.nameAr ?? '').toLowerCase() === unitSymbol.toLowerCase())
          ?? (item?.baseUnitId ? this.units().find(u => u.id === item.baseUnitId) : undefined);
        if (!item || !wh || !unit) continue;
        added.push({
          inventoryItemId: item.id,
          itemNameAr: item.nameAr,
          itemSku: item.sku,
          warehouseId: wh.id,
          warehouseNameAr: wh.nameAr,
          unitId: unit.id,
          unitNameAr: unit.nameAr ?? unit.symbol,
          quantity: Number(qtyStr) || 0,
          unitCost: Number(costStr) || 0
        });
      }
      if (added.length) {
        this.lines.update(l => [...l, ...added.filter(a => a.quantity > 0)]);
        this.statusMessage.set(this.t('inv.ob.imported').replace('{n}', String(added.length)));
      } else {
        this.error.set(this.t('inv.ob.importEmpty'));
      }
      input.value = '';
    };
    reader.readAsText(file);
  }

  private reload(id: string, message?: string): void {
    this.loading.set(true);
    this.repo.getById(id).subscribe({
      next: doc => {
        this.applyDoc(doc);
        this.loading.set(false);
        this.saving.set(false);
        if (message) this.statusMessage.set(message);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('inv.ob.loadFailed'));
        this.loading.set(false);
        this.saving.set(false);
      }
    });
  }

  private applyDoc(doc: OpeningBalanceDoc): void {
    this.docId.set(doc.id);
    this.documentNumber.set(doc.documentNumber);
    this.documentDate.set(doc.documentDate.slice(0, 10));
    this.approvalDate.set(doc.approvalDate ? doc.approvalDate.slice(0, 10) : null);
    this.status.set(doc.status);
    this.entryMethod.set(this.methodCode(doc.entryMethod, 1));
    this.displayMethod.set(this.methodCode(doc.displayMethod, 1));
    this.costingMethod.set(doc.costingMethod === 'FIFO' ? 1 : doc.costingMethod === 'StandardCost' ? 3 : 2);
    this.weightedAverageScope.set(this.scopeCode(doc.weightedAverageScope));
    this.useExpiryDate.set(doc.useExpiryDate);
    this.useBatchNumbers.set(doc.useBatchNumbers);
    this.useSerialNumbers.set(doc.useSerialNumbers);
    this.contraAccountId.set(doc.contraAccountId ?? null);
    this.costCenterId.set(doc.costCenterId ?? null);
    this.notes.set(doc.notes ?? '');
    this.defaultWarehouseId.set(doc.warehouseId ?? null);
    this.lines.set((doc.lines ?? []).map(l => ({
      id: l.id,
      inventoryItemId: l.inventoryItemId,
      itemNameAr: l.itemNameAr,
      itemSku: l.itemSku,
      warehouseId: l.warehouseId,
      warehouseNameAr: l.warehouseNameAr,
      unitId: l.unitId,
      unitNameAr: l.unitNameAr,
      quantity: l.quantity,
      unitCost: l.unitCost,
      batchNumber: l.batchNumber,
      expiryDate: l.expiryDate,
      serialNumber: l.serialNumber
    })));
  }

  private methodCode(name: string, fallback: number): number {
    if (name === 'ExcelImport' || name === 'ByWarehouse') return 2;
    if (name === 'AutoGenerate' || name === 'ByCategory') return 3;
    if (name === 'Manual' || name === 'ByItem') return 1;
    return fallback;
  }

  private scopeCode(name: string): number {
    if (name === 'Branch') return 2;
    if (name === 'Company') return 3;
    return 1;
  }

  private todayIso(): string {
    const d = new Date();
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }
}
