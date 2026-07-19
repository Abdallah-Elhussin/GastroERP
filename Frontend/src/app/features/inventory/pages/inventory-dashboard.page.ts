import { Component, ChangeDetectionStrategy, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { catchError, of } from 'rxjs';
import { LanguageService } from '../../../core/services/language.service';
import { InventoryService } from '../../../core/services/inventory.service';
import {
  InventoryDashboardAlert,
  InventoryDashboardCategorySlice,
  InventoryDashboardSummary,
  InventoryDashboardTopMover
} from '../../../core/models/inventory.models';
import { InventoryPageShellComponent } from '../shared/inventory-page-shell.component';
import { InventorySkeletonComponent } from '../shared/inventory-skeleton.component';
import { InventoryErrorStateComponent } from '../shared/inventory-error-state.component';

interface OverviewKpi {
  key: string;
  hintKey: string;
  icon: string;
  value: number;
  tone: 'violet' | 'green' | 'rose' | 'amber';
  path: string;
}

@Component({
  selector: 'app-inventory-dashboard-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatIconModule,
    InventoryPageShellComponent,
    InventorySkeletonComponent,
    InventoryErrorStateComponent
  ],
  templateUrl: './inventory-dashboard.page.html',
  styleUrl: './inventory-dashboard.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryDashboardPage implements OnInit {
  lang = inject(LanguageService);
  inventoryService = inject(InventoryService);

  loading = signal(true);
  error = signal<string | null>(null);
  summary = signal<InventoryDashboardSummary | null>(null);
  kpis = signal<OverviewKpi[]>([]);
  alerts = signal<InventoryDashboardAlert[]>([]);
  topMovers = signal<InventoryDashboardTopMover[]>([]);
  categories = signal<InventoryDashboardCategorySlice[]>([]);

  breadcrumbs = [
    { labelKey: 'nav.inventory', path: '/inventory/dashboard' },
    { labelKey: 'inv.nav.dashboard' }
  ];

  maxMoverQty = computed(() => {
    const movers = this.topMovers();
    if (!movers.length) return 1;
    return Math.max(1, ...movers.map(m => Math.max(m.inQuantity, m.outQuantity)));
  });

  maxCategoryCount = computed(() => {
    const cats = this.categories();
    if (!cats.length) return 1;
    return Math.max(1, ...cats.map(c => c.itemCount));
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.inventoryService.getDashboard().pipe(
      catchError(err => {
        this.error.set(err?.error?.error ?? this.t('inv.error.message'));
        this.loading.set(false);
        return of(null);
      })
    ).subscribe(data => {
      if (!data) return;
      this.summary.set(data);
      this.alerts.set(data.alerts ?? []);
      this.topMovers.set(data.topMovers ?? []);
      this.categories.set(data.categoryDistribution ?? []);
      this.kpis.set([
        {
          key: 'inv.kpi.totalProducts',
          hintKey: 'inv.dashboard.kpi.totalHint',
          icon: 'inventory_2',
          value: data.totalItems,
          tone: 'violet',
          path: '/inventory/items'
        },
        {
          key: 'inv.kpi.activeProducts',
          hintKey: 'inv.dashboard.kpi.activeHint',
          icon: 'check_box',
          value: data.activeItems,
          tone: 'green',
          path: '/inventory/items'
        },
        {
          key: 'inv.kpi.inactiveProducts',
          hintKey: 'inv.dashboard.kpi.inactiveHint',
          icon: 'pause_circle',
          value: data.inactiveItems ?? Math.max(0, data.totalItems - data.activeItems),
          tone: 'rose',
          path: '/inventory/items'
        },
        {
          key: 'inv.kpi.categories',
          hintKey: 'inv.dashboard.kpi.categoriesHint',
          icon: 'folder',
          value: data.categoryCount ?? 0,
          tone: 'amber',
          path: '/inventory/categories'
        }
      ]);
      this.loading.set(false);
    });
  }

  itemName(item: InventoryDashboardTopMover): string {
    return this.lang.language() === 'ar' ? item.nameAr : (item.nameEn || item.nameAr);
  }

  categoryName(cat: InventoryDashboardCategorySlice): string {
    return this.lang.language() === 'ar' ? cat.nameAr : (cat.nameEn || cat.nameAr);
  }

  alertMessage(alert: InventoryDashboardAlert): string {
    return this.lang.language() === 'ar' ? alert.messageAr : alert.messageEn;
  }

  barPct(value: number, max: number): number {
    if (max <= 0) return 0;
    return Math.round((value / max) * 100);
  }

  t(key: string): string {
    return this.lang.t(key);
  }
}
