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
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatTooltipModule } from '@angular/material/tooltip';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { InventoryRepository } from '../../../core/repositories/inventory.repository';
import {
  CreateInventoryItemTypePayload,
  INVENTORY_ITEM_TYPE_CATEGORIES,
  InventoryItemType,
  UpdateInventoryItemTypePayload
} from '../../../core/models/inventory-item-type.models';
import { InventoryPageShellComponent } from '../shared/inventory-page-shell.component';
import { InventorySkeletonComponent } from '../shared/inventory-skeleton.component';
import { InventoryEmptyStateComponent } from '../shared/inventory-empty-state.component';
import { InventoryErrorStateComponent } from '../shared/inventory-error-state.component';

@Component({
  selector: 'app-inventory-item-types-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatTooltipModule,
    InventoryPageShellComponent,
    InventorySkeletonComponent,
    InventoryEmptyStateComponent,
    InventoryErrorStateComponent
  ],
  templateUrl: './inventory-item-types.page.html',
  styleUrl: './inventory-item-types.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryItemTypesPage implements OnInit {
  private repo = inject(InventoryRepository);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  error = signal<string | null>(null);
  rows = signal<InventoryItemType[]>([]);
  totalCount = signal(0);
  selectedId = signal<string | null>(null);
  showForm = signal(false);
  showFilters = signal(false);
  showHelp = signal(false);
  editingId = signal<string | null>(null);
  saving = signal(false);
  formError = signal<string | null>(null);

  search = signal('');
  pageIndex = signal(0);
  pageSize = signal(25);
  sortBy = signal('sortOrder');
  sortDesc = signal(false);

  filterInventory = signal<boolean | null>(null);
  filterSellable = signal<boolean | null>(null);
  filterPurchasable = signal<boolean | null>(null);
  filterProduction = signal<boolean | null>(null);
  filterRecipe = signal<boolean | null>(null);
  filterActive = signal<boolean | null>(null);
  filterCategory = signal<number | null>(null);

  categories = INVENTORY_ITEM_TYPE_CATEGORIES;
  dataSource = new MatTableDataSource<InventoryItemType>([]);

  displayedColumns = [
    'select',
    'code',
    'nameAr',
    'nameEn',
    'codeStart',
    'codeEnd',
    'category',
    'isInventory',
    'canSell',
    'canPurchase',
    'isRecipe',
    'isProduction',
    'allowNegativeStock',
    'isActive'
  ];

  breadcrumbs = [
    { labelKey: 'nav.inventory', path: '/inventory/dashboard' },
    { labelKey: 'inv.nav.itemTypes' }
  ];

  selected = computed(() => {
    const id = this.selectedId();
    return this.rows().find(r => r.id === id) ?? null;
  });

  canCreate = computed(() => this.auth.hasPermission('Inventory.ItemTypes.Create'));
  canEdit = computed(() => this.auth.hasPermission('Inventory.ItemTypes.Edit'));
  canDelete = computed(() => this.auth.hasPermission('Inventory.ItemTypes.Delete'));
  canExport = computed(() => this.auth.hasPermission('Inventory.ItemTypes.Export'));

  form: CreateInventoryItemTypePayload & { isActive: boolean } = this.emptyForm();

  ngOnInit(): void {
    this.load();
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo
      .getItemTypes({
        search: this.search().trim() || undefined,
        category: this.filterCategory(),
        isActive: this.filterActive(),
        isInventory: this.filterInventory(),
        canSell: this.filterSellable(),
        canPurchase: this.filterPurchasable(),
        isRecipe: this.filterRecipe(),
        isProduction: this.filterProduction(),
        sortBy: this.sortBy(),
        sortDesc: this.sortDesc(),
        page: this.pageIndex() + 1,
        pageSize: this.pageSize()
      })
      .subscribe({
        next: page => {
          this.rows.set(page.items);
          this.dataSource.data = page.items;
          this.totalCount.set(page.totalCount);
          this.loading.set(false);
          if (this.selectedId() && !page.items.some(i => i.id === this.selectedId())) {
            this.selectedId.set(null);
          }
        },
        error: err => {
          this.error.set(err?.error?.error ?? this.t('inv.itemTypes.loadFailed'));
          this.loading.set(false);
        }
      });
  }

  onSearch(): void {
    this.pageIndex.set(0);
    this.load();
  }

  onPage(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.load();
  }

  onSort(sort: Sort): void {
    this.sortBy.set(sort.active || 'sortOrder');
    this.sortDesc.set(sort.direction === 'desc');
    this.load();
  }

  selectRow(row: InventoryItemType): void {
    this.selectedId.set(row.id === this.selectedId() ? null : row.id);
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
      nameEn: row.nameEn ?? '',
      description: row.description ?? '',
      category: Number(row.category),
      codeStart: row.codeStart ?? null,
      codeEnd: row.codeEnd ?? null,
      isInventory: row.isInventory,
      canSell: row.canSell,
      canPurchase: row.canPurchase,
      isRecipe: row.isRecipe,
      isProduction: row.isProduction,
      allowNegativeStock: row.allowNegativeStock,
      color: row.color,
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
    if (!this.form.nameAr?.trim() || (!this.editingId() && !this.form.code?.trim())) {
      this.formError.set(this.t('inv.itemTypes.validation.required'));
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
      const payload: UpdateInventoryItemTypePayload = {
        nameAr: this.form.nameAr.trim(),
        nameEn: this.form.nameEn || null,
        description: this.form.description || null,
        category: Number(this.form.category),
        codeStart: this.form.codeStart ?? null,
        codeEnd: this.form.codeEnd ?? null,
        isInventory: this.form.isInventory,
        canSell: this.form.canSell,
        canPurchase: this.form.canPurchase,
        isRecipe: this.form.isRecipe,
        isProduction: this.form.isProduction,
        allowNegativeStock: this.form.allowNegativeStock,
        color: this.form.color || null,
        sortOrder: this.form.sortOrder ?? 0,
        isActive: this.form.isActive
      };
      this.repo.updateItemType(id, payload).subscribe({ next: onDone, error: onError });
    } else {
      const payload: CreateInventoryItemTypePayload = {
        code: this.form.code.trim(),
        nameAr: this.form.nameAr.trim(),
        nameEn: this.form.nameEn || null,
        description: this.form.description || null,
        category: Number(this.form.category),
        codeStart: this.form.codeStart ?? null,
        codeEnd: this.form.codeEnd ?? null,
        isInventory: this.form.isInventory,
        canSell: this.form.canSell,
        canPurchase: this.form.canPurchase,
        isRecipe: this.form.isRecipe,
        isProduction: this.form.isProduction,
        allowNegativeStock: this.form.allowNegativeStock,
        color: this.form.color || null,
        sortOrder: this.form.sortOrder ?? 0
      };
      this.repo.createItemType(payload).subscribe({ next: onDone, error: onError });
    }
  }

  deleteSelected(): void {
    const row = this.selected();
    if (!row || !this.canDelete() || row.isSystem) return;
    if (!confirm(this.t('inv.itemTypes.confirmDelete'))) return;
    this.repo.deleteItemType(row.id).subscribe({
      next: () => {
        this.selectedId.set(null);
        this.load();
      },
      error: err => this.error.set(err?.error?.error ?? this.t('inv.itemTypes.deleteFailed'))
    });
  }

  exportExcel(): void {
    if (!this.canExport()) return;
    this.downloadCsv('item-types.csv');
  }

  exportPdf(): void {
    if (!this.canExport()) return;
    window.print();
  }

  categoryLabel(category: number): string {
    const found = this.categories.find(c => c.value === Number(category));
    return found ? this.t(found.labelKey) : String(category);
  }

  private downloadCsv(filename: string): void {
    const header = [
      'Code', 'NameAr', 'NameEn', 'CodeStart', 'CodeEnd', 'Category',
      'Inventory', 'Sellable', 'Purchasable', 'Recipe', 'Production', 'NegativeStock', 'Active'
    ];
    const lines = this.rows().map(r =>
      [
        r.code, r.nameAr, r.nameEn ?? '', r.codeStart ?? '', r.codeEnd ?? '', r.categoryName,
        r.isInventory, r.canSell, r.canPurchase, r.isRecipe, r.isProduction, r.allowNegativeStock, r.isActive
      ]
        .map(v => `"${String(v).replace(/"/g, '""')}"`)
        .join(',')
    );
    const blob = new Blob([[header.join(','), ...lines].join('\n')], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
  }

  private emptyForm(): CreateInventoryItemTypePayload & { isActive: boolean } {
    return {
      code: '',
      nameAr: '',
      nameEn: '',
      description: '',
      category: 1,
      codeStart: null,
      codeEnd: null,
      isInventory: true,
      canSell: false,
      canPurchase: true,
      isRecipe: false,
      isProduction: false,
      allowNegativeStock: false,
      color: '#FFF5E6',
      sortOrder: 0,
      isActive: true
    };
  }
}
