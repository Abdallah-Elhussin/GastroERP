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
import { ProductInquiryRepository } from '../../../core/repositories/product-inquiry.repository';
import {
  ProductInquiryDetail,
  ProductInquiryListItem
} from '../../../core/models/product-inquiry.models';
import { InventoryPageShellComponent } from '../shared/inventory-page-shell.component';
import { InventorySkeletonComponent } from '../shared/inventory-skeleton.component';
import { InventoryEmptyStateComponent } from '../shared/inventory-empty-state.component';
import { InventoryErrorStateComponent } from '../shared/inventory-error-state.component';

@Component({
  selector: 'app-inventory-product-inquiry-page',
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
  templateUrl: './inventory-product-inquiry.page.html',
  styleUrl: './inventory-product-inquiry.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryProductInquiryPage implements OnInit {
  private repo = inject(ProductInquiryRepository);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  detailLoading = signal(false);
  error = signal<string | null>(null);
  detailError = signal<string | null>(null);
  rows = signal<ProductInquiryListItem[]>([]);
  selectedId = signal<string | null>(null);
  detail = signal<ProductInquiryDetail | null>(null);
  showHelp = signal(false);

  search = signal('');
  activeOnly = signal(true);
  inventoryOnly = signal(false);
  sortActive = signal('nameAr');
  sortDirection = signal<'asc' | 'desc' | ''>('asc');

  displayedColumns = [
    'sku',
    'nameAr',
    'category',
    'type',
    'unit',
    'sellingPrice',
    'lastPurchase',
    'stock',
    'status'
  ];

  breadcrumbs = [
    { labelKey: 'nav.inventory', path: '/inventory/dashboard' },
    { labelKey: 'inv.nav.inquiry' }
  ];

  canView = computed(() => this.auth.hasPermission('Inventory.ProductInquiry.View'));

  selected = computed(() => {
    const id = this.selectedId();
    return this.rows().find(r => r.id === id) ?? null;
  });

  ngOnInit(): void {
    this.load();
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo.getList({
      search: this.search().trim() || undefined,
      activeOnly: this.activeOnly(),
      inventoryOnly: this.inventoryOnly(),
      sortBy: this.sortActive() || 'nameAr',
      sortDesc: this.sortDirection() === 'desc',
      pageSize: 200
    }).subscribe({
      next: rows => {
        this.rows.set(rows ?? []);
        this.loading.set(false);
        const sel = this.selectedId();
        if (sel && !(rows ?? []).some(r => r.id === sel)) {
          this.selectedId.set(null);
          this.detail.set(null);
        }
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('inv.inquiry.loadFailed'));
        this.loading.set(false);
      }
    });
  }

  clearFilters(): void {
    this.search.set('');
    this.activeOnly.set(true);
    this.inventoryOnly.set(false);
    this.load();
  }

  onSort(sort: Sort): void {
    this.sortActive.set(sort.active || 'nameAr');
    this.sortDirection.set((sort.direction as 'asc' | 'desc' | '') || '');
  }

  selectRow(row: ProductInquiryListItem): void {
    this.selectedId.set(row.id);
    this.loadDetail(row.id);
  }

  loadDetail(id: string): void {
    this.detailLoading.set(true);
    this.detailError.set(null);
    this.repo.getDetail(id).subscribe({
      next: d => {
        this.detail.set(d);
        this.detailLoading.set(false);
      },
      error: err => {
        this.detail.set(null);
        this.detailError.set(err?.error?.error ?? this.t('inv.inquiry.detailFailed'));
        this.detailLoading.set(false);
      }
    });
  }

  formatDate(value?: string | null): string {
    if (!value) return '—';
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return '—';
    return d.toLocaleDateString(this.lang.language() === 'ar' ? 'ar-EG' : 'en-GB');
  }

  formatMoney(value?: number | null): string {
    if (value == null) return '—';
    return value.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 4 });
  }

  stockStatusLabel(status: string): string {
    switch (status) {
      case 'OutOfStock': return this.t('inv.inquiry.stock.out');
      case 'BelowReorder': return this.t('inv.inquiry.stock.low');
      default: return this.t('inv.inquiry.stock.ok');
    }
  }

  movementTypeLabel(type: string): string {
    const key = `inv.inquiry.movement.${type}`;
    const translated = this.t(key);
    return translated === key ? type : translated;
  }
}
