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
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatTooltipModule } from '@angular/material/tooltip';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { InventoryService } from '../../../core/services/inventory.service';
import {
  BranchLookup,
  CreateWarehousePayload,
  Warehouse,
  WarehouseDetail,
  WarehouseType,
  WarehouseTypeDefinition
} from '../../../core/models/inventory.models';
import { InventoryPageShellComponent } from '../shared/inventory-page-shell.component';
import { InventorySkeletonComponent } from '../shared/inventory-skeleton.component';
import { InventoryEmptyStateComponent } from '../shared/inventory-empty-state.component';
import { InventoryErrorStateComponent } from '../shared/inventory-error-state.component';

@Component({
  selector: 'app-inventory-warehouses-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatTableModule,
    MatSortModule,
    MatTooltipModule,
    InventoryPageShellComponent,
    InventorySkeletonComponent,
    InventoryEmptyStateComponent,
    InventoryErrorStateComponent
  ],
  templateUrl: './inventory-warehouses.page.html',
  styleUrl: './inventory-warehouses.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryWarehousesPage implements OnInit {
  lang = inject(LanguageService);
  auth = inject(AuthService);
  inventory = inject(InventoryService);

  showForm = signal(false);
  showHelp = signal(false);
  editingId = signal<string | null>(null);
  selectedId = signal<string | null>(null);
  saving = signal(false);
  formError = signal<string | null>(null);
  search = signal('');
  branchFilter = signal<string | null>(null);
  typeFilter = signal<string | null>(null);
  activeOnly = signal(false);
  posOnly = signal(false);
  defaultOnly = signal(false);
  sortActive = signal('nameAr');
  sortDirection = signal<'asc' | 'desc' | ''>('asc');

  branches = signal<BranchLookup[]>([]);
  warehouseTypes = signal<WarehouseTypeDefinition[]>([]);
  structureWarehouse = signal<WarehouseDetail | null>(null);
  structureLoading = signal(false);
  locationDraft = signal({ nameAr: '', code: '' });
  locationParent = signal<{ zoneId?: string; shelfId?: string } | null>(null);

  form: CreateWarehousePayload = this.emptyForm();
  displayedColumns = [
    'code',
    'nameAr',
    'branch',
    'type',
    'manager',
    'isActive',
    'isDefault',
    'allowNegativeStock',
    'useBins'
  ];

  breadcrumbs = [
    { labelKey: 'nav.inventory', path: '/inventory/dashboard' },
    { labelKey: 'inv.nav.warehouses' }
  ];

  canView = computed(() => this.auth.hasPermission('Inventory.Warehouses.View'));
  canCreate = computed(() => this.auth.hasPermission('Inventory.Warehouses.Create'));
  canEdit = computed(() => this.auth.hasPermission('Inventory.Warehouses.Edit'));
  canDelete = computed(() => this.auth.hasPermission('Inventory.Warehouses.Delete'));
  canExport = computed(() => this.auth.hasPermission('Inventory.Warehouses.Export'));

  selected = computed(() => {
    const id = this.selectedId();
    return this.inventory.warehouses().find(w => w.id === id) ?? null;
  });

  parentOptions = computed(() => {
    const editing = this.editingId();
    const branchId = this.form.branchId;
    return this.inventory.warehouses().filter(w =>
      w.id !== editing &&
      (!branchId || w.branchId === branchId) &&
      w.isActive
    );
  });

  filtered = computed(() => {
    const q = this.search().trim().toLowerCase();
    const branchId = this.branchFilter();
    const typeId = this.typeFilter();
    let rows = [...this.inventory.warehouses()];

    if (branchId) rows = rows.filter(w => w.branchId === branchId);
    if (typeId) rows = rows.filter(w => w.warehouseTypeId === typeId);
    if (this.activeOnly()) rows = rows.filter(w => w.isActive);
    if (this.posOnly()) rows = rows.filter(w => w.isPosWarehouse);
    if (this.defaultOnly()) rows = rows.filter(w => w.isDefault);

    if (q) {
      rows = rows.filter(w =>
        (w.code?.toLowerCase().includes(q) ?? false) ||
        w.nameAr.toLowerCase().includes(q) ||
        (w.nameEn?.toLowerCase().includes(q) ?? false) ||
        (w.branchNameAr?.toLowerCase().includes(q) ?? false) ||
        (w.warehouseTypeNameAr?.toLowerCase().includes(q) ?? false) ||
        this.typeLabel(w).toLowerCase().includes(q)
      );
    }

    const active = this.sortActive();
    const dir = this.sortDirection();
    if (active && dir) {
      const mul = dir === 'asc' ? 1 : -1;
      rows.sort((a, b) => {
        const av = this.sortValue(a, active);
        const bv = this.sortValue(b, active);
        if (av < bv) return -1 * mul;
        if (av > bv) return 1 * mul;
        return 0;
      });
    }
    return rows;
  });

  ngOnInit(): void {
    this.refresh();
    this.inventory.getBranchesLookup().subscribe({
      next: rows => this.branches.set(rows ?? []),
      error: () => this.branches.set([])
    });
    this.inventory.getWarehouseTypes().subscribe({
      next: rows => this.warehouseTypes.set(rows ?? []),
      error: () => this.warehouseTypes.set([])
    });
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  refresh(): void {
    this.inventory.loadWarehouses();
  }

  onSort(sort: Sort): void {
    this.sortActive.set(sort.active || 'nameAr');
    this.sortDirection.set((sort.direction as 'asc' | 'desc' | '') || '');
  }

  selectRow(row: Warehouse): void {
    this.selectedId.set(row.id === this.selectedId() ? null : row.id);
  }

  typeLabel(row: Warehouse): string {
    if (row.warehouseTypeNameAr) return row.warehouseTypeNameAr;
    const key = typeof row.warehouseType === 'number'
      ? String(row.warehouseType)
      : row.warehouseType;
    const typed = this.t(`inv.wh.type.${key}`);
    return typed.startsWith('inv.wh.type.') ? String(row.warehouseType) : typed;
  }

  branchName(branchId?: string | null): string {
    if (!branchId) return '—';
    return this.branches().find(b => b.id === branchId)?.nameAr
      ?? this.inventory.warehouses().find(w => w.branchId === branchId)?.branchNameAr
      ?? '—';
  }

  openCreate(): void {
    if (!this.canCreate()) return;
    this.editingId.set(null);
    this.form = this.emptyForm();
    if (this.branches().length === 1) {
      this.form.branchId = this.branches()[0].id;
    }
    if (this.warehouseTypes().length) {
      const main = this.warehouseTypes().find(t => t.code === 'MAIN') ?? this.warehouseTypes()[0];
      this.form.warehouseTypeId = main.id;
      this.form.warehouseType = this.mapTypeCode(main.code);
    }
    this.formError.set(null);
    this.showForm.set(true);
  }

  openEdit(): void {
    const row = this.selected();
    if (!row || !this.canEdit()) return;
    this.editingId.set(row.id);
    this.form = {
      nameAr: row.nameAr,
      nameEn: row.nameEn,
      code: row.code,
      branchId: row.branchId,
      companyId: row.companyId,
      warehouseType: row.warehouseType as WarehouseType,
      warehouseTypeId: row.warehouseTypeId,
      parentWarehouseId: row.parentWarehouseId,
      address: row.address,
      phone: row.phone,
      email: row.email,
      notes: row.notes,
      managerUserId: row.managerUserId,
      responsibleEmployeeId: row.responsibleEmployeeId,
      allowPurchase: row.allowPurchase,
      allowSales: row.allowSales,
      allowTransfer: row.allowTransfer,
      allowInventoryCount: row.allowInventoryCount,
      allowManufacturing: row.allowManufacturing,
      allowNegativeStock: row.allowNegativeStock,
      allowReservation: row.allowReservation,
      allowReceiving: row.allowReceiving,
      allowIssue: row.allowIssue,
      allowAdjustment: row.allowAdjustment,
      isPosWarehouse: row.isPosWarehouse,
      isDefault: row.isDefault,
      useBins: row.useBins,
      isActive: row.isActive
    };
    this.formError.set(null);
    this.showForm.set(true);
  }

  closeForm(): void {
    this.showForm.set(false);
  }

  onTypeSelected(typeId: string | null): void {
    this.form.warehouseTypeId = typeId;
    const def = this.warehouseTypes().find(t => t.id === typeId);
    if (def) {
      this.form.warehouseType = this.mapTypeCode(def.code);
      if (def.code === 'POS') this.form.isPosWarehouse = true;
    }
  }

  save(): void {
    if (!this.form.nameAr?.trim() || !this.form.branchId) {
      this.formError.set(this.t('inv.warehouses.validation.required'));
      return;
    }
    this.saving.set(true);
    this.formError.set(null);
    const id = this.editingId();
    const payload: CreateWarehousePayload = {
      ...this.form,
      nameAr: this.form.nameAr.trim(),
      nameEn: this.form.nameEn?.trim() || undefined,
      code: this.form.code?.trim() || undefined,
      parentWarehouseId: this.form.parentWarehouseId || null,
      warehouseTypeId: this.form.warehouseTypeId || null
    };

    const onDone = () => {
      this.saving.set(false);
      this.showForm.set(false);
      this.inventory.loadWarehouses();
    };
    const onError = (err: { error?: { error?: string } }) => {
      this.saving.set(false);
      this.formError.set(err?.error?.error ?? this.t('inv.saveFailed'));
    };

    if (id) {
      this.inventory.updateWarehouse(id, payload).subscribe({ next: onDone, error: onError });
    } else {
      this.inventory.createWarehouse(payload).subscribe({ next: onDone, error: onError });
    }
  }

  deleteSelected(): void {
    const row = this.selected();
    if (!row || !this.canDelete()) return;
    if (!confirm(this.t('inv.warehouses.confirmDelete'))) return;
    this.inventory.deleteWarehouse(row.id).subscribe({
      next: () => {
        this.selectedId.set(null);
        this.inventory.loadWarehouses();
      },
      error: (err: { error?: { error?: string } }) => {
        this.inventory.error.set(err?.error?.error ?? this.t('inv.warehouses.deleteFailed'));
      }
    });
  }

  exportExcel(): void {
    if (!this.canExport()) return;
    const header = [
      this.t('inv.warehouses.col.code'),
      this.t('inv.warehouses.col.name'),
      this.t('inv.field.nameEn'),
      this.t('inv.warehouses.col.branch'),
      this.t('inv.warehouses.col.type'),
      this.t('inv.warehouses.col.active'),
      this.t('inv.warehouses.col.default')
    ];
    const lines = this.filtered().map(r =>
      [r.code ?? '', r.nameAr, r.nameEn ?? '', r.branchNameAr ?? this.branchName(r.branchId), this.typeLabel(r), r.isActive, r.isDefault]
        .map(v => `"${String(v).replace(/"/g, '""')}"`)
        .join(',')
    );
    const blob = new Blob([[header.join(','), ...lines].join('\n')], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'warehouses.csv';
    a.click();
    URL.revokeObjectURL(url);
  }

  exportPdf(): void {
    if (!this.canExport()) return;
    window.print();
  }

  openStructure(row: Warehouse): void {
    this.structureLoading.set(true);
    this.structureWarehouse.set(null);
    this.inventory.getWarehouseById(row.id).subscribe({
      next: detail => {
        this.structureWarehouse.set(detail);
        this.structureLoading.set(false);
      },
      error: () => this.structureLoading.set(false)
    });
  }

  closeStructure(): void {
    this.structureWarehouse.set(null);
    this.locationParent.set(null);
  }

  refreshStructure(): void {
    const wh = this.structureWarehouse();
    if (!wh) return;
    this.openStructure(wh);
  }

  beginAddZone(): void {
    this.locationParent.set({});
    this.locationDraft.set({ nameAr: '', code: '' });
  }

  beginAddShelf(zoneId: string): void {
    this.locationParent.set({ zoneId });
    this.locationDraft.set({ nameAr: '', code: '' });
  }

  beginAddBin(zoneId: string, shelfId: string): void {
    this.locationParent.set({ zoneId, shelfId });
    this.locationDraft.set({ nameAr: '', code: '' });
  }

  patchLocationNameAr(nameAr: string): void {
    this.locationDraft.set({ ...this.locationDraft(), nameAr });
  }

  patchLocationCode(code: string): void {
    this.locationDraft.set({ ...this.locationDraft(), code });
  }

  cancelLocationForm(): void {
    this.locationParent.set(null);
  }

  locationFormTitle(parent: { zoneId?: string; shelfId?: string }): string {
    if (!parent.zoneId) return this.t('inv.warehouses.addZone');
    if (!parent.shelfId) return this.t('inv.warehouses.addShelf');
    return this.t('inv.warehouses.addBin');
  }

  saveLocation(): void {
    const wh = this.structureWarehouse();
    const parent = this.locationParent();
    const draft = this.locationDraft();
    if (!wh || !parent || !draft.nameAr.trim()) return;

    const payload = { nameAr: draft.nameAr.trim(), code: draft.code || undefined };
    const done = () => {
      this.locationParent.set(null);
      this.refreshStructure();
      this.inventory.loadWarehouses();
    };

    if (!parent.zoneId) {
      this.inventory.addWarehouseZone(wh.id, payload).subscribe({ next: done });
    } else if (!parent.shelfId) {
      this.inventory.addWarehouseShelf(wh.id, parent.zoneId, payload).subscribe({ next: done });
    } else {
      this.inventory.addWarehouseBin(wh.id, parent.zoneId, parent.shelfId, payload).subscribe({ next: done });
    }
  }

  removeZone(zoneId: string): void {
    const wh = this.structureWarehouse();
    if (!wh) return;
    this.inventory.removeWarehouseZone(wh.id, zoneId).subscribe({ next: () => this.refreshStructure() });
  }

  removeShelf(zoneId: string, shelfId: string): void {
    const wh = this.structureWarehouse();
    if (!wh) return;
    this.inventory.removeWarehouseShelf(wh.id, zoneId, shelfId).subscribe({ next: () => this.refreshStructure() });
  }

  removeBin(zoneId: string, shelfId: string, binId: string): void {
    const wh = this.structureWarehouse();
    if (!wh) return;
    this.inventory.removeWarehouseBin(wh.id, zoneId, shelfId, binId).subscribe({ next: () => this.refreshStructure() });
  }

  private sortValue(row: Warehouse, active: string): string | number | boolean {
    switch (active) {
      case 'code': return (row.code ?? '').toLowerCase();
      case 'branch': return (row.branchNameAr ?? '').toLowerCase();
      case 'type': return this.typeLabel(row).toLowerCase();
      case 'manager': return (row.managerUserId ?? '').toLowerCase();
      case 'isActive': return row.isActive;
      case 'isDefault': return row.isDefault;
      case 'allowNegativeStock': return row.allowNegativeStock;
      case 'useBins': return row.useBins;
      default: return row.nameAr.toLowerCase();
    }
  }

  private mapTypeCode(code: string): WarehouseType {
    const map: Record<string, WarehouseType> = {
      MAIN: 'Main',
      POS: 'POS',
      PRODUCTION: 'Production',
      RAW: 'RawMaterial',
      FINISHED: 'FinishedGoods',
      RETURNS: 'Returns',
      DAMAGED: 'Damaged',
      TRANSIT: 'Transit',
      KITCHEN: 'Kitchen',
      BEVERAGE: 'Beverage',
      DRYSTORE: 'DryStore',
      CHILLER: 'Chiller',
      FREEZER: 'Freezer',
      PACKAGING: 'Packaging',
      CLEANING: 'Cleaning',
      WASTE: 'Waste'
    };
    return map[code.toUpperCase()] ?? 'Main';
  }

  private emptyForm(): CreateWarehousePayload {
    return {
      nameAr: '',
      nameEn: '',
      code: '',
      branchId: null,
      warehouseType: 'Main',
      warehouseTypeId: null,
      parentWarehouseId: null,
      address: '',
      phone: '',
      email: '',
      notes: '',
      allowPurchase: true,
      allowSales: true,
      allowTransfer: true,
      allowInventoryCount: true,
      allowManufacturing: true,
      allowNegativeStock: false,
      allowReservation: true,
      allowReceiving: true,
      allowIssue: true,
      allowAdjustment: true,
      isPosWarehouse: false,
      isDefault: false,
      useBins: false,
      isActive: true
    };
  }
}
