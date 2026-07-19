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
  CreateInventoryCategoryPayload,
  InventoryCategory
} from '../../../core/models/inventory.models';
import { InventoryPageShellComponent } from '../shared/inventory-page-shell.component';
import { InventorySkeletonComponent } from '../shared/inventory-skeleton.component';
import { InventoryEmptyStateComponent } from '../shared/inventory-empty-state.component';
import { InventoryErrorStateComponent } from '../shared/inventory-error-state.component';

@Component({
  selector: 'app-inventory-categories-page',
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
  templateUrl: './inventory-categories.page.html',
  styleUrl: './inventory-categories.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryCategoriesPage implements OnInit {
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
  sortActive = signal('nameAr');
  sortDirection = signal<'asc' | 'desc' | ''>('asc');

  form: CreateInventoryCategoryPayload = this.emptyForm();

  displayedColumns = ['number', 'code', 'nameAr', 'parent', 'description'];

  breadcrumbs = [
    { labelKey: 'nav.inventory', path: '/inventory/dashboard' },
    { labelKey: 'inv.nav.categories' }
  ];

  canManage = computed(() => this.auth.hasPermission('Inventory.Manage'));

  selected = computed(() => {
    const id = this.selectedId();
    return this.inventory.categories().find(c => c.id === id) ?? null;
  });

  filtered = computed(() => {
    const q = this.search().trim().toLowerCase();
    let rows = [...this.inventory.categories()];
    if (q) {
      rows = rows.filter(c =>
        c.code?.toLowerCase().includes(q) ||
        c.nameAr.toLowerCase().includes(q) ||
        (c.nameEn?.toLowerCase().includes(q) ?? false) ||
        (c.descriptionAr?.toLowerCase().includes(q) ?? false) ||
        this.parentName(c.parentCategoryId).toLowerCase().includes(q) ||
        String(c.sortOrder).includes(q)
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

  parentOptions = computed(() => {
    const editing = this.editingId();
    return this.inventory.categories().filter(c => c.id !== editing && c.isActive);
  });

  ngOnInit(): void {
    this.inventory.loadCategories();
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  refresh(): void {
    this.inventory.loadCategories();
  }

  onSort(sort: Sort): void {
    this.sortActive.set(sort.active || 'nameAr');
    this.sortDirection.set((sort.direction as 'asc' | 'desc' | '') || '');
  }

  selectRow(row: InventoryCategory): void {
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
      parentCategoryId: row.parentCategoryId ?? null,
      code: row.code,
      descriptionAr: row.descriptionAr ?? '',
      descriptionEn: row.descriptionEn,
      icon: row.icon,
      imageUrl: row.imageUrl,
      color: row.color,
      sortOrder: row.sortOrder
    };
    this.formError.set(null);
    this.showForm.set(true);
  }

  closeForm(): void {
    this.showForm.set(false);
  }

  save(): void {
    if (!this.form.nameAr?.trim()) {
      this.formError.set(this.t('inv.categories.validation.nameRequired'));
      return;
    }
    this.saving.set(true);
    this.formError.set(null);
    const id = this.editingId();
    const payload: CreateInventoryCategoryPayload = {
      ...this.form,
      nameAr: this.form.nameAr.trim(),
      nameEn: this.form.nameEn?.trim() || undefined,
      code: this.form.code?.trim() || undefined,
      descriptionAr: this.form.descriptionAr?.trim() || undefined,
      parentCategoryId: this.form.parentCategoryId || null
    };

    const onDone = () => {
      this.saving.set(false);
      this.showForm.set(false);
      this.inventory.loadCategories();
    };
    const onError = (err: { error?: { error?: string } }) => {
      this.saving.set(false);
      this.formError.set(err?.error?.error ?? this.t('inv.saveFailed'));
    };

    if (id) {
      this.inventory.updateCategory(id, payload).subscribe({ next: onDone, error: onError });
    } else {
      this.inventory.createCategory(payload).subscribe({ next: onDone, error: onError });
    }
  }

  deleteSelected(): void {
    const row = this.selected();
    if (!row || !this.canManage()) return;
    if (!confirm(this.t('inv.categories.confirmDelete'))) return;
    this.inventory.deleteCategory(row.id).subscribe({
      next: () => {
        this.selectedId.set(null);
        this.inventory.loadCategories();
      },
      error: (err: { error?: { error?: string } }) => {
        this.inventory.error.set(err?.error?.error ?? this.t('inv.categories.deleteFailed'));
      }
    });
  }

  parentName(id?: string | null): string {
    if (!id) return '—';
    return this.inventory.categories().find(c => c.id === id)?.nameAr ?? '—';
  }

  private sortValue(row: InventoryCategory, active: string): string | number {
    switch (active) {
      case 'number': return row.sortOrder ?? 0;
      case 'code': return (row.code ?? '').toLowerCase();
      case 'nameAr': return row.nameAr.toLowerCase();
      case 'parent': return this.parentName(row.parentCategoryId).toLowerCase();
      case 'description': return (row.descriptionAr ?? '').toLowerCase();
      default: return row.nameAr.toLowerCase();
    }
  }

  private emptyForm(): CreateInventoryCategoryPayload {
    return {
      nameAr: '',
      nameEn: '',
      parentCategoryId: null,
      code: '',
      descriptionAr: '',
      sortOrder: 0
    };
  }
}
