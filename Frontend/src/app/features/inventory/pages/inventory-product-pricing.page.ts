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
import { ProductPricingRepository } from '../../../core/repositories/product-pricing.repository';
import {
  CreateProductPricesBatchPayload,
  InventoryItemLookup,
  PricingMethod,
  ProductCostType,
  ProductPrice,
  ProductPriceUnitLinePayload,
  SalesPriceList,
  UpdateProductPricePayload
} from '../../../core/models/product-pricing.models';
import { InventoryPageShellComponent } from '../shared/inventory-page-shell.component';
import { InventorySkeletonComponent } from '../shared/inventory-skeleton.component';
import { InventoryEmptyStateComponent } from '../shared/inventory-empty-state.component';
import { InventoryErrorStateComponent } from '../shared/inventory-error-state.component';

interface UnitPriceDraft {
  unitId: string;
  unitNameAr: string;
  unitFactor: number;
  cost: number;
  profitMargin: number;
  profitAmount: number;
  sellingPrice: number;
  save: boolean;
}

@Component({
  selector: 'app-inventory-product-pricing-page',
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
  templateUrl: './inventory-product-pricing.page.html',
  styleUrl: './inventory-product-pricing.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryProductPricingPage implements OnInit {
  private repo = inject(ProductPricingRepository);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  error = signal<string | null>(null);
  rows = signal<ProductPrice[]>([]);
  priceLists = signal<SalesPriceList[]>([]);
  itemResults = signal<InventoryItemLookup[]>([]);
  selectedId = signal<string | null>(null);
  showHelp = signal(false);
  showModal = signal(false);
  editingId = signal<string | null>(null);
  saving = signal(false);
  formError = signal<string | null>(null);
  search = signal('');
  itemFilter = signal('');
  sortActive = signal('startDate');
  sortDirection = signal<'asc' | 'desc' | ''>('desc');

  // Modal state
  productId = signal<string | null>(null);
  productLabel = signal('');
  itemSearch = signal('');
  priceListId = signal<string | null>(null);
  costType = signal<ProductCostType>(1);
  pricingMethod = signal<PricingMethod>(4);
  startDate = signal(this.todayIso());
  unitLines = signal<UnitPriceDraft[]>([]);
  unitsLoading = signal(false);

  displayedColumns = [
    'index',
    'product',
    'unit',
    'costType',
    'priceList',
    'cost',
    'profitAmount',
    'profitMargin',
    'sellingPrice',
    'startDate',
    'endDate'
  ];

  modalUnitColumns = ['unit', 'factor', 'cost', 'margin', 'sellingPrice', 'save'];

  breadcrumbs = [
    { labelKey: 'nav.inventory', path: '/inventory/dashboard' },
    { labelKey: 'inv.nav.prices' }
  ];

  canCreate = computed(() => this.auth.hasPermission('Sales.ProductPricing.Create'));
  canEdit = computed(() => this.auth.hasPermission('Sales.ProductPricing.Edit'));
  canDelete = computed(() => this.auth.hasPermission('Sales.ProductPricing.Delete'));

  selected = computed(() => {
    const id = this.selectedId();
    return this.rows().find(r => r.id === id) ?? null;
  });

  filtered = computed(() => {
    const q = this.itemFilter().trim().toLowerCase();
    let list = [...this.rows()];
    if (q) {
      list = list.filter(r =>
        (r.productNameAr?.toLowerCase().includes(q) ?? false) ||
        (r.productSku?.toLowerCase().includes(q) ?? false) ||
        (r.unitNameAr?.toLowerCase().includes(q) ?? false) ||
        (r.priceListNameAr?.toLowerCase().includes(q) ?? false)
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

  defaultPriceListId = computed(() => {
    const lists = this.priceLists();
    return lists.find(l => l.isDefault)?.id ?? lists[0]?.id ?? null;
  });

  ngOnInit(): void {
    this.load();
    this.repo.getPriceLists(true).subscribe({
      next: rows => {
        this.priceLists.set(rows ?? []);
        if (!this.priceListId()) this.priceListId.set(this.defaultPriceListId());
      },
      error: () => this.priceLists.set([])
    });
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo.getPrices({
      search: this.search().trim() || undefined,
      sortBy: this.sortActive() || 'startDate',
      sortDesc: this.sortDirection() !== 'asc',
      pageSize: 200
    }).subscribe({
      next: rows => {
        this.rows.set(rows ?? []);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('inv.prices.loadFailed'));
        this.loading.set(false);
      }
    });
  }

  onSearch(): void {
    this.load();
  }

  clearItemFilter(): void {
    this.itemFilter.set('');
  }

  onSort(sort: Sort): void {
    this.sortActive.set(sort.active || 'startDate');
    this.sortDirection.set((sort.direction as 'asc' | 'desc' | '') || '');
  }

  selectRow(row: ProductPrice): void {
    this.selectedId.set(row.id);
  }

  openCreate(): void {
    if (!this.canCreate()) return;
    this.editingId.set(null);
    this.formError.set(null);
    this.productId.set(null);
    this.productLabel.set('');
    this.itemSearch.set('');
    this.itemResults.set([]);
    this.priceListId.set(this.defaultPriceListId());
    this.costType.set(1);
    this.pricingMethod.set(4);
    this.startDate.set(this.todayIso());
    this.unitLines.set([]);
    this.showModal.set(true);
  }

  openEdit(): void {
    const row = this.selected();
    if (!row || !this.canEdit()) return;
    this.editingId.set(row.id);
    this.formError.set(null);
    this.productId.set(row.productId);
    this.productLabel.set(row.productNameAr ?? row.productId);
    this.priceListId.set(row.priceListId);
    this.costType.set(row.costType);
    this.pricingMethod.set(row.pricingMethod);
    this.startDate.set(row.startDate.slice(0, 10));
    this.unitLines.set([{
      unitId: row.unitId,
      unitNameAr: row.unitNameAr ?? '—',
      unitFactor: row.unitFactor || 1,
      cost: row.cost,
      profitMargin: row.profitMargin,
      profitAmount: row.profitAmount,
      sellingPrice: row.sellingPrice,
      save: true
    }]);
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
    this.formError.set(null);
  }

  searchItems(): void {
    const q = this.itemSearch().trim();
    this.repo.searchItems(q || undefined).subscribe({
      next: rows => this.itemResults.set((rows ?? []).filter(r => r.isActive !== false)),
      error: () => this.itemResults.set([])
    });
  }

  pickItem(item: InventoryItemLookup): void {
    this.productId.set(item.id);
    this.productLabel.set(item.nameAr + (item.sku ? ` (${item.sku})` : ''));
    this.itemResults.set([]);
    this.itemSearch.set('');
    this.loadUnits();
  }

  onCostTypeOrMethodChange(): void {
    if (this.productId()) this.loadUnits(true);
    else this.recalcAll();
  }

  loadUnits(preserveMargins = false): void {
    const id = this.productId();
    if (!id) {
      this.unitLines.set([]);
      return;
    }
    this.unitsLoading.set(true);
    const prev = preserveMargins
      ? new Map(this.unitLines().map(l => [l.unitId, l]))
      : new Map<string, UnitPriceDraft>();

    this.repo.getProductUnits(id, this.costType()).subscribe({
      next: rows => {
        const method = this.pricingMethod();
        const lines: UnitPriceDraft[] = (rows ?? []).map(r => {
          const old = prev.get(r.unitId);
          const cost = r.cost;
          const profitMargin = old?.profitMargin ?? 0;
          const profitAmount = old?.profitAmount ?? 0;
          const sellingPrice = old
            ? this.calcPrice(method, cost, profitMargin, profitAmount, old.sellingPrice)
            : this.calcPrice(method, cost, 0, 0, cost);
          return {
            unitId: r.unitId,
            unitNameAr: r.unitNameAr,
            unitFactor: r.factor,
            cost,
            profitMargin,
            profitAmount,
            sellingPrice,
            save: old?.save ?? true
          };
        });
        this.unitLines.set(lines);
        this.unitsLoading.set(false);
      },
      error: err => {
        this.formError.set(err?.error?.error ?? this.t('inv.prices.unitsFailed'));
        this.unitsLoading.set(false);
      }
    });
  }

  updateCosts(): void {
    this.loadUnits(true);
  }

  applyToAll(): void {
    const lines = this.unitLines();
    if (lines.length === 0) return;
    const first = lines[0];
    const method = this.pricingMethod();
    this.unitLines.set(lines.map(l => ({
      ...l,
      profitMargin: first.profitMargin,
      profitAmount: first.profitAmount,
      sellingPrice: this.calcPrice(method, l.cost, first.profitMargin, first.profitAmount, first.sellingPrice)
    })));
  }

  onLineChange(index: number, field: keyof UnitPriceDraft, value: number | boolean): void {
    const lines = [...this.unitLines()];
    const line = { ...lines[index], [field]: value } as UnitPriceDraft;
    if (field === 'cost' || field === 'profitMargin' || field === 'profitAmount' || field === 'sellingPrice') {
      line.sellingPrice = this.calcPrice(
        this.pricingMethod(),
        line.cost,
        line.profitMargin,
        line.profitAmount,
        field === 'sellingPrice' ? Number(value) : line.sellingPrice
      );
    }
    lines[index] = line;
    this.unitLines.set(lines);
  }

  recalcAll(): void {
    const method = this.pricingMethod();
    this.unitLines.set(this.unitLines().map(l => ({
      ...l,
      sellingPrice: this.calcPrice(method, l.cost, l.profitMargin, l.profitAmount, l.sellingPrice)
    })));
  }

  saveModal(): void {
    if (this.editingId()) {
      this.saveEdit();
      return;
    }
    this.saveCreate();
  }

  private saveCreate(): void {
    const productId = this.productId();
    const priceListId = this.priceListId();
    if (!productId || !priceListId) {
      this.formError.set(this.t('inv.prices.validation.required'));
      return;
    }
    const lines = this.unitLines().filter(l => l.save);
    if (lines.length === 0) {
      this.formError.set(this.t('inv.prices.validation.lines'));
      return;
    }

    const payload: CreateProductPricesBatchPayload = {
      productId,
      priceListId,
      pricingMethod: this.pricingMethod(),
      costType: this.costType(),
      startDate: new Date(this.startDate()).toISOString(),
      salesChannel: 0,
      lines: lines.map(l => this.toLinePayload(l))
    };

    this.saving.set(true);
    this.formError.set(null);
    this.repo.createBatch(payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeModal();
        this.load();
      },
      error: err => {
        this.formError.set(err?.error?.error ?? this.t('inv.prices.saveFailed'));
        this.saving.set(false);
      }
    });
  }

  private saveEdit(): void {
    const id = this.editingId();
    const line = this.unitLines()[0];
    const priceListId = this.priceListId();
    if (!id || !line || !priceListId) {
      this.formError.set(this.t('inv.prices.validation.required'));
      return;
    }
    const existing = this.rows().find(r => r.id === id);
    const payload: UpdateProductPricePayload = {
      branchId: existing?.branchId ?? null,
      priceListId,
      salesChannel: existing?.salesChannel ?? 0,
      unitId: line.unitId,
      pricingMethod: this.pricingMethod(),
      costType: this.costType(),
      cost: line.cost,
      profitMargin: line.profitMargin,
      profitAmount: line.profitAmount,
      sellingPrice: line.sellingPrice,
      minimumPrice: existing?.minimumPrice ?? null,
      maximumDiscount: existing?.maximumDiscount ?? null,
      startDate: new Date(this.startDate()).toISOString(),
      endDate: existing?.endDate ?? null,
      priority: existing?.priority ?? 0,
      currencyId: existing?.currencyId ?? null,
      isDefault: existing?.isDefault ?? false,
      isActive: existing?.isActive ?? true,
      notes: existing?.notes ?? null
    };

    this.saving.set(true);
    this.formError.set(null);
    this.repo.update(id, payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeModal();
        this.load();
      },
      error: err => {
        this.formError.set(err?.error?.error ?? this.t('inv.prices.saveFailed'));
        this.saving.set(false);
      }
    });
  }

  deleteSelected(): void {
    const row = this.selected();
    if (!row || !this.canDelete()) return;
    if (!confirm(this.t('inv.prices.confirmDelete'))) return;
    this.repo.delete(row.id).subscribe({
      next: () => {
        this.selectedId.set(null);
        this.load();
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('inv.prices.deleteFailed'));
      }
    });
  }

  costTypeLabel(v: ProductCostType): string {
    switch (v) {
      case 1: return this.t('inv.prices.costType.average');
      case 2: return this.t('inv.prices.costType.last');
      case 3: return this.t('inv.prices.costType.standard');
      default: return String(v);
    }
  }

  methodLabel(v: PricingMethod): string {
    switch (v) {
      case 1: return this.t('inv.prices.method.fixed');
      case 2: return this.t('inv.prices.method.margin');
      case 3: return this.t('inv.prices.method.profit');
      case 4: return this.t('inv.prices.method.manual');
      default: return String(v);
    }
  }

  formatDate(value?: string | null): string {
    if (!value) return '—';
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return '—';
    return d.toLocaleDateString(this.lang.language() === 'ar' ? 'ar-EG' : 'en-GB');
  }

  private toLinePayload(l: UnitPriceDraft): ProductPriceUnitLinePayload {
    return {
      unitId: l.unitId,
      unitFactor: l.unitFactor,
      cost: l.cost,
      profitMargin: l.profitMargin,
      profitAmount: l.profitAmount,
      sellingPrice: l.sellingPrice,
      save: l.save
    };
  }

  private calcPrice(
    method: PricingMethod,
    cost: number,
    margin: number,
    profit: number,
    manual: number
  ): number {
    switch (method) {
      case 2: return Math.round((cost + cost * (margin / 100)) * 10000) / 10000;
      case 3: return Math.round((cost + profit) * 10000) / 10000;
      case 1:
      case 4:
      default: return Math.round(manual * 10000) / 10000;
    }
  }

  private todayIso(): string {
    const d = new Date();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${d.getFullYear()}-${m}-${day}`;
  }

  private sortValue(row: ProductPrice, key: string): string | number {
    switch (key) {
      case 'product': return (row.productNameAr ?? '').toLowerCase();
      case 'unit': return (row.unitNameAr ?? '').toLowerCase();
      case 'costType': return row.costType;
      case 'priceList': return (row.priceListNameAr ?? '').toLowerCase();
      case 'cost': return row.cost;
      case 'profitAmount': return row.profitAmount;
      case 'profitMargin': return row.profitMargin;
      case 'sellingPrice': return row.sellingPrice;
      case 'startDate': return row.startDate;
      case 'endDate': return row.endDate ?? '';
      default: return row.createdAt;
    }
  }
}
