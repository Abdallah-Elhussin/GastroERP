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
import { InventoryValuationGroupRepository } from '../../../core/repositories/inventory-valuation-group.repository';
import {
  CostCenterLookup,
  CreateInventoryValuationGroupPayload,
  InventoryValuationGroup,
  UpdateInventoryValuationGroupPayload
} from '../../../core/models/inventory-valuation-group.models';
import { InventoryPageShellComponent } from '../shared/inventory-page-shell.component';
import { InventorySkeletonComponent } from '../shared/inventory-skeleton.component';
import { InventoryEmptyStateComponent } from '../shared/inventory-empty-state.component';
import { InventoryErrorStateComponent } from '../shared/inventory-error-state.component';

@Component({
  selector: 'app-inventory-valuation-groups-page',
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
  templateUrl: './inventory-valuation-groups.page.html',
  styleUrl: './inventory-valuation-groups.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryValuationGroupsPage implements OnInit {
  private repo = inject(InventoryValuationGroupRepository);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  error = signal<string | null>(null);
  rows = signal<InventoryValuationGroup[]>([]);
  costCenters = signal<CostCenterLookup[]>([]);
  selectedId = signal<string | null>(null);
  showForm = signal(false);
  showHelp = signal(false);
  editingId = signal<string | null>(null);
  saving = signal(false);
  formError = signal<string | null>(null);
  search = signal('');
  sortActive = signal('nameAr');
  sortDirection = signal<'asc' | 'desc' | ''>('asc');

  form: CreateInventoryValuationGroupPayload & { isActive?: boolean } = this.emptyForm();
  displayedColumns = ['code', 'nameAr', 'description', 'costCenter', 'isActive'];

  breadcrumbs = [
    { labelKey: 'nav.inventory', path: '/inventory/dashboard' },
    { labelKey: 'inv.nav.valuation' }
  ];

  canCreate = computed(() => this.auth.hasPermission('Inventory.ValuationGroups.Create'));
  canEdit = computed(() => this.auth.hasPermission('Inventory.ValuationGroups.Edit'));
  canDelete = computed(() => this.auth.hasPermission('Inventory.ValuationGroups.Delete'));

  selected = computed(() => {
    const id = this.selectedId();
    return this.rows().find(r => r.id === id) ?? null;
  });

  filtered = computed(() => {
    const q = this.search().trim().toLowerCase();
    let list = [...this.rows()];
    if (q) {
      list = list.filter(r =>
        r.code.toLowerCase().includes(q) ||
        r.nameAr.toLowerCase().includes(q) ||
        (r.nameEn?.toLowerCase().includes(q) ?? false) ||
        (r.description?.toLowerCase().includes(q) ?? false) ||
        (r.costCenterNameAr?.toLowerCase().includes(q) ?? false)
      );
    }
    const active = this.sortActive();
    const dir = this.sortDirection();
    if (active && dir) {
      const mul = dir === 'asc' ? 1 : -1;
      list.sort((a, b) => {
        const av = this.sortValue(a, active);
        const bv = this.sortValue(b, active);
        if (av < bv) return -1 * mul;
        if (av > bv) return 1 * mul;
        return 0;
      });
    }
    return list;
  });

  ngOnInit(): void {
    this.load();
    this.repo.getCostCenters().subscribe({
      next: rows => this.costCenters.set(rows ?? []),
      error: () => this.costCenters.set([])
    });
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo.getPaged({
      sortBy: this.sortActive() || 'nameAr',
      sortDesc: this.sortDirection() === 'desc',
      pageSize: 200
    }).subscribe({
      next: rows => {
        this.rows.set(rows ?? []);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('inv.valuation.loadFailed'));
        this.loading.set(false);
      }
    });
  }

  onSearch(): void {
    this.load();
  }

  onSort(sort: Sort): void {
    this.sortActive.set(sort.active || 'nameAr');
    this.sortDirection.set((sort.direction as 'asc' | 'desc' | '') || '');
  }

  selectRow(row: InventoryValuationGroup): void {
    this.selectedId.set(row.id === this.selectedId() ? null : row.id);
  }

  costCenterName(id?: string | null): string {
    if (!id) return '';
    return this.costCenters().find(c => c.id === id)?.nameAr
      ?? this.rows().find(r => r.costCenterId === id)?.costCenterNameAr
      ?? '';
  }

  openCreate(): void {
    if (!this.canCreate()) return;
    this.editingId.set(null);
    this.form = this.emptyForm();
    this.formError.set(null);
    this.showForm.set(true);
  }

  openEdit(): void {
    const row = this.selected();
    if (!row || !this.canEdit()) return;
    this.editingId.set(row.id);
    this.form = {
      code: row.code,
      nameAr: row.nameAr,
      nameEn: row.nameEn,
      description: row.description,
      costCenterId: row.costCenterId,
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
    if (!this.form.code?.trim() || !this.form.nameAr?.trim()) {
      this.formError.set(this.t('inv.valuation.validation.required'));
      return;
    }
    this.saving.set(true);
    this.formError.set(null);
    const id = this.editingId();

    const onDone = () => {
      this.saving.set(false);
      this.showForm.set(false);
      this.load();
    };
    const onError = (err: { error?: { error?: string } }) => {
      this.saving.set(false);
      this.formError.set(err?.error?.error ?? this.t('inv.saveFailed'));
    };

    if (id) {
      const payload: UpdateInventoryValuationGroupPayload = {
        nameAr: this.form.nameAr.trim(),
        nameEn: this.form.nameEn?.trim() || null,
        description: this.form.description?.trim() || null,
        costCenterId: this.form.costCenterId || null,
        sortOrder: this.form.sortOrder ?? 0,
        isActive: this.form.isActive ?? true
      };
      this.repo.update(id, payload).subscribe({ next: onDone, error: onError });
    } else {
      const payload: CreateInventoryValuationGroupPayload = {
        code: this.form.code.trim(),
        nameAr: this.form.nameAr.trim(),
        nameEn: this.form.nameEn?.trim() || null,
        description: this.form.description?.trim() || null,
        costCenterId: this.form.costCenterId || null,
        sortOrder: this.form.sortOrder ?? 0
      };
      this.repo.create(payload).subscribe({ next: onDone, error: onError });
    }
  }

  deleteSelected(): void {
    const row = this.selected();
    if (!row || !this.canDelete() || row.isSystem) return;
    if (!confirm(this.t('inv.valuation.confirmDelete'))) return;
    this.repo.delete(row.id).subscribe({
      next: () => {
        this.selectedId.set(null);
        this.load();
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('inv.valuation.deleteFailed'));
      }
    });
  }

  private sortValue(row: InventoryValuationGroup, active: string): string | number | boolean {
    switch (active) {
      case 'code': return row.code.toLowerCase();
      case 'description': return (row.description ?? '').toLowerCase();
      case 'costCenter': return (row.costCenterNameAr ?? '').toLowerCase();
      case 'isActive': return row.isActive;
      default: return row.nameAr.toLowerCase();
    }
  }

  private emptyForm(): CreateInventoryValuationGroupPayload & { isActive?: boolean } {
    return {
      code: '',
      nameAr: '',
      nameEn: '',
      description: '',
      costCenterId: null,
      sortOrder: 0,
      isActive: true
    };
  }
}
