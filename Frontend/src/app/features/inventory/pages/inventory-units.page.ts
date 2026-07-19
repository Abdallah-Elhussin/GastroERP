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
  CreateInventoryUnitPayload,
  INVENTORY_UNIT_CLASSIFICATIONS,
  INVENTORY_UNIT_TYPES,
  InventoryUnit
} from '../../../core/models/inventory.models';
import { InventoryPageShellComponent } from '../shared/inventory-page-shell.component';
import { InventorySkeletonComponent } from '../shared/inventory-skeleton.component';
import { InventoryEmptyStateComponent } from '../shared/inventory-empty-state.component';
import { InventoryErrorStateComponent } from '../shared/inventory-error-state.component';

@Component({
  selector: 'app-inventory-units-page',
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
  templateUrl: './inventory-units.page.html',
  styleUrl: './inventory-units.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryUnitsPage implements OnInit {
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
  typeFilter = signal<number | null>(null);
  sortActive = signal('nameAr');
  sortDirection = signal<'asc' | 'desc' | ''>('asc');

  unitTypes = INVENTORY_UNIT_TYPES;
  classifications = INVENTORY_UNIT_CLASSIFICATIONS;
  form: CreateInventoryUnitPayload = this.emptyForm();
  displayedColumns = ['number', 'code', 'nameAr', 'unitType', 'factor', 'baseUnit', 'isActive'];

  breadcrumbs = [
    { labelKey: 'nav.inventory', path: '/inventory/dashboard' },
    { labelKey: 'inv.nav.units' }
  ];

  canManage = computed(() => this.auth.hasPermission('Inventory.Manage'));

  selected = computed(() => {
    const id = this.selectedId();
    return this.inventory.units().find(u => u.id === id) ?? null;
  });

  filtered = computed(() => {
    const q = this.search().trim().toLowerCase();
    const type = this.typeFilter();
    let rows = [...this.inventory.units()];

    if (type != null) {
      rows = rows.filter(u => Number(u.unitType) === type);
    }
    if (q) {
      rows = rows.filter(u =>
        u.code?.toLowerCase().includes(q) ||
        u.symbol?.toLowerCase().includes(q) ||
        u.nameAr.toLowerCase().includes(q) ||
        (u.nameEn?.toLowerCase().includes(q) ?? false) ||
        this.baseUnitName(u.baseUnitId).toLowerCase().includes(q) ||
        String(u.sortOrder).includes(q)
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

  baseUnitOptions = computed(() => {
    const editing = this.editingId();
    return this.inventory.units().filter(u => u.id !== editing && u.isActive);
  });

  ngOnInit(): void {
    this.inventory.loadUnits();
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  refresh(): void {
    this.inventory.loadUnits();
  }

  onSort(sort: Sort): void {
    this.sortActive.set(sort.active || 'nameAr');
    this.sortDirection.set((sort.direction as 'asc' | 'desc' | '') || '');
  }

  selectRow(row: InventoryUnit): void {
    this.selectedId.set(row.id === this.selectedId() ? null : row.id);
  }

  openCreate(): void {
    if (!this.canManage()) return;
    this.editingId.set(null);
    this.form = this.emptyForm();
    this.formError.set(null);
    this.showForm.set(true);
  }

  openEdit(): void {
    const row = this.selected();
    if (!row || !this.canManage()) return;
    this.editingId.set(row.id);
    this.form = {
      nameAr: row.nameAr,
      nameEn: row.nameEn,
      symbol: row.symbol,
      symbolAr: row.symbolAr,
      code: row.code,
      decimalPlaces: row.decimalPlaces,
      baseUnitId: row.baseUnitId ?? null,
      conversionFactor: row.conversionFactor,
      unitType: Number(row.unitType),
      classification: Number(row.classification),
      sortOrder: row.sortOrder,
      isActive: row.isActive
    };
    this.formError.set(null);
    this.showForm.set(true);
  }

  closeForm(): void {
    this.showForm.set(false);
  }

  save(): void {
    if (!this.form.nameAr?.trim() || !this.form.symbol?.trim()) {
      this.formError.set(this.t('inv.units.validation.required'));
      return;
    }
    this.saving.set(true);
    this.formError.set(null);
    const id = this.editingId();
    const payload: CreateInventoryUnitPayload = {
      ...this.form,
      nameAr: this.form.nameAr.trim(),
      symbol: this.form.symbol.trim(),
      code: (this.form.code || this.form.symbol).trim(),
      nameEn: this.form.nameEn?.trim() || undefined,
      baseUnitId: this.form.baseUnitId || null,
      conversionFactor: Number(this.form.conversionFactor ?? 1),
      unitType: Number(this.form.unitType ?? 1),
      classification: Number(this.form.classification ?? 6),
      isActive: this.form.isActive ?? true
    };

    const onDone = () => {
      this.saving.set(false);
      this.showForm.set(false);
      this.inventory.loadUnits();
    };
    const onError = (err: { error?: { error?: string } }) => {
      this.saving.set(false);
      this.formError.set(err?.error?.error ?? this.t('inv.saveFailed'));
    };

    if (id) {
      this.inventory.updateUnit(id, payload).subscribe({ next: onDone, error: onError });
    } else {
      this.inventory.createUnit(payload).subscribe({ next: onDone, error: onError });
    }
  }

  deleteSelected(): void {
    const row = this.selected();
    if (!row || !this.canManage()) return;
    if (!confirm(this.t('inv.units.confirmDelete'))) return;
    this.inventory.deleteUnit(row.id).subscribe({
      next: () => {
        this.selectedId.set(null);
        this.inventory.loadUnits();
      },
      error: (err: { error?: { error?: string } }) => {
        this.inventory.error.set(err?.error?.error ?? this.t('inv.units.deleteFailed'));
      }
    });
  }

  baseUnitName(id?: string | null): string {
    if (!id) return '—';
    return this.inventory.units().find(u => u.id === id)?.nameAr ?? '—';
  }

  typeLabel(type: number): string {
    const found = this.unitTypes.find(t => t.value === Number(type));
    return found ? this.t(found.labelKey) : String(type);
  }

  private sortValue(row: InventoryUnit, active: string): string | number {
    switch (active) {
      case 'number': return row.sortOrder ?? 0;
      case 'code': return (row.code ?? '').toLowerCase();
      case 'nameAr': return row.nameAr.toLowerCase();
      case 'unitType': return Number(row.unitType);
      case 'factor': return Number(row.conversionFactor);
      case 'baseUnit': return this.baseUnitName(row.baseUnitId).toLowerCase();
      case 'isActive': return row.isActive ? 1 : 0;
      default: return row.nameAr.toLowerCase();
    }
  }

  private emptyForm(): CreateInventoryUnitPayload {
    return {
      nameAr: '',
      nameEn: '',
      symbol: '',
      code: '',
      decimalPlaces: 2,
      baseUnitId: null,
      conversionFactor: 1,
      unitType: 1,
      classification: 2,
      sortOrder: 0,
      isActive: true
    };
  }
}
