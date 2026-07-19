import { Component, ChangeDetectionStrategy, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { catchError, of } from 'rxjs';
import { LanguageService } from '../../../core/services/language.service';
import { InventoryService } from '../../../core/services/inventory.service';
import { CatalogService } from '../../../core/services/catalog.service';
import { InventoryItemDefinition, ItemPurchaseHistory, ItemSalesHistory, ItemStockMovement, WarehouseStockBalance } from '../../../core/models/inventory.models';
import { CatalogPriceHistoryEntry, ProductCatalogDefinition } from '../../../core/models/catalog.models';
import { InventoryPageShellComponent } from '../shared/inventory-page-shell.component';
import { InventorySkeletonComponent } from '../shared/inventory-skeleton.component';
import { InventoryErrorStateComponent } from '../shared/inventory-error-state.component';
import { InventoryEmptyStateComponent } from '../shared/inventory-empty-state.component';

type DetailsTab = 'overview' | 'inventory' | 'movements' | 'purchases' | 'sales' | 'prices' | 'attachments';

@Component({
  selector: 'app-inventory-product-details-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatIconModule,
    InventoryPageShellComponent,
    InventorySkeletonComponent,
    InventoryErrorStateComponent,
    InventoryEmptyStateComponent
  ],
  templateUrl: './inventory-product-details.page.html',
  styleUrl: './inventory-product-details.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryProductDetailsPage implements OnInit {
  private route = inject(ActivatedRoute);
  lang = inject(LanguageService);
  inventory = inject(InventoryService);
  catalog = inject(CatalogService);

  itemId = signal<string | null>(null);
  item = signal<InventoryItemDefinition | null>(null);
  catalogDef = signal<ProductCatalogDefinition | null>(null);
  stock = signal<WarehouseStockBalance[]>([]);
  movements = signal<ItemStockMovement[]>([]);
  purchases = signal<ItemPurchaseHistory[]>([]);
  sales = signal<ItemSalesHistory[]>([]);
  prices = signal<CatalogPriceHistoryEntry[]>([]);

  loading = signal(true);
  sectionLoading = signal(false);
  error = signal<string | null>(null);
  activeTab = signal<DetailsTab>('overview');

  tabs: { id: DetailsTab; labelKey: string; icon: string }[] = [
    { id: 'overview', labelKey: 'pd.tab.overview', icon: 'info' },
    { id: 'inventory', labelKey: 'pd.tab.inventory', icon: 'warehouse' },
    { id: 'movements', labelKey: 'pd.tab.movements', icon: 'swap_horiz' },
    { id: 'purchases', labelKey: 'pd.tab.purchases', icon: 'local_shipping' },
    { id: 'sales', labelKey: 'pd.tab.sales', icon: 'point_of_sale' },
    { id: 'prices', labelKey: 'pd.tab.prices', icon: 'payments' },
    { id: 'attachments', labelKey: 'pd.tab.attachments', icon: 'attach_file' }
  ];

  breadcrumbs = computed(() => [
    { labelKey: 'nav.inventory', path: '/inventory/dashboard' },
    { labelKey: 'inv.nav.items', path: '/inventory/items' },
    { labelKey: 'inv.nav.details' }
  ]);

  displayName = computed(() => {
    const cat = this.catalogDef();
    const item = this.item();
    if (this.lang.language() === 'ar') return cat?.nameAr || item?.nameAr || '—';
    return cat?.nameEn || item?.nameEn || cat?.nameAr || item?.nameAr || '—';
  });

  barcode = computed(() => this.catalogDef()?.barcode || this.item()?.barcode || '');
  sku = computed(() => this.catalogDef()?.sku || this.item()?.sku || '');
  primaryImage = computed(() => this.catalogDef()?.primaryImageUrl || this.item()?.imageUrl || '');
  mediaUrls = computed(() => {
    const urls = [...(this.catalogDef()?.mediaUrls ?? [])];
    const primary = this.primaryImage();
    if (primary && !urls.includes(primary)) urls.unshift(primary);
    return urls;
  });

  qrUrl = computed(() => {
    const data = this.barcode() || this.sku() || this.itemId() || '';
    if (!data) return '';
    return `https://api.qrserver.com/v1/create-qr-code/?size=140x140&data=${encodeURIComponent(data)}`;
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.error.set(this.t('pd.missingId'));
      this.loading.set(false);
      return;
    }
    this.itemId.set(id);
    this.loadOverview(id);
  }

  selectTab(tab: DetailsTab): void {
    this.activeTab.set(tab);
    const id = this.itemId();
    if (!id) return;
    if (tab === 'inventory' && this.stock().length === 0) this.loadStock(id);
    if (tab === 'movements' && this.movements().length === 0) this.loadMovements(id);
    if (tab === 'purchases' && this.purchases().length === 0) this.loadPurchases(id);
    if (tab === 'sales' && this.sales().length === 0) this.loadSales(id);
    if (tab === 'prices' && this.prices().length === 0) this.loadPrices();
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  formatDate(value: string): string {
    try {
      return new Date(value).toLocaleString(this.lang.language() === 'ar' ? 'ar' : 'en');
    } catch {
      return value;
    }
  }

  isImageUrl(url: string): boolean {
    return /\.(png|jpe?g|gif|webp|svg)(\?|$)/i.test(url) || url.startsWith('data:image');
  }

  fileName(url: string): string {
    return url.split('/').pop() || url;
  }

  private loadOverview(id: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.inventory.getItem(id).subscribe({
      next: item => {
        this.item.set(item);
        this.catalog.getDefinitionByInventoryItemId(id).pipe(
          catchError(() => of(null))
        ).subscribe(def => {
          this.catalogDef.set(def);
          this.loading.set(false);
          this.loadStock(id);
        });
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('pd.loadFailed'));
        this.loading.set(false);
      }
    });
  }

  private loadStock(id: string): void {
    this.sectionLoading.set(true);
    this.inventory.getStockByWarehouse(id).pipe(
      catchError(() => of([] as WarehouseStockBalance[]))
    ).subscribe(rows => {
      this.stock.set(rows);
      this.sectionLoading.set(false);
    });
  }

  private loadMovements(id: string): void {
    this.sectionLoading.set(true);
    this.inventory.getItemMovements(id).pipe(
      catchError(() => of([] as ItemStockMovement[]))
    ).subscribe(rows => {
      this.movements.set(rows);
      this.sectionLoading.set(false);
    });
  }

  private loadPurchases(id: string): void {
    this.sectionLoading.set(true);
    this.inventory.getItemPurchaseHistory(id).pipe(
      catchError(() => of([] as ItemPurchaseHistory[]))
    ).subscribe(rows => {
      this.purchases.set(rows);
      this.sectionLoading.set(false);
    });
  }

  private loadSales(id: string): void {
    this.sectionLoading.set(true);
    this.inventory.getItemSalesHistory(id).pipe(
      catchError(() => of([] as ItemSalesHistory[]))
    ).subscribe(rows => {
      this.sales.set(rows);
      this.sectionLoading.set(false);
    });
  }

  private loadPrices(): void {
    const catalogId = this.catalogDef()?.id;
    if (!catalogId) {
      this.prices.set([]);
      return;
    }
    this.sectionLoading.set(true);
    this.catalog.getPriceHistory(catalogId).pipe(
      catchError(() => of([] as CatalogPriceHistoryEntry[]))
    ).subscribe(rows => {
      this.prices.set(rows);
      this.sectionLoading.set(false);
    });
  }
}
