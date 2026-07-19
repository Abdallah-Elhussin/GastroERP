import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  inject,
  signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpParams } from '@angular/common/http';
import { MatIconModule } from '@angular/material/icon';
import { catchError, of } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { LanguageService } from '../../../core/services/language.service';
import { InventoryPageShellComponent } from '../shared/inventory-page-shell.component';
import { InventorySkeletonComponent } from '../shared/inventory-skeleton.component';
import { InventoryErrorStateComponent } from '../shared/inventory-error-state.component';
import { InventoryEmptyStateComponent } from '../shared/inventory-empty-state.component';

type ReportTab =
  | 'balance'
  | 'valuation'
  | 'movements'
  | 'waste'
  | 'adjustments'
  | 'consumption'
  | 'purchases'
  | 'suppliers';

interface ReportTabDef {
  id: ReportTab;
  labelKey: string;
  icon: string;
  path: string;
}

@Component({
  selector: 'app-inventory-reports-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    InventoryPageShellComponent,
    InventorySkeletonComponent,
    InventoryErrorStateComponent,
    InventoryEmptyStateComponent
  ],
  templateUrl: './inventory-reports.page.html',
  styleUrl: './inventory-reports.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryReportsPage implements OnInit {
  private http = inject(HttpClient);
  lang = inject(LanguageService);

  private readonly base = `${environment.apiBaseUrl}/reports/inventory`;

  tab = signal<ReportTab>('balance');
  loading = signal(false);
  error = signal<string | null>(null);
  rows = signal<Record<string, unknown>[]>([]);
  valuationTotal = signal<number | null>(null);
  valuationItemCount = signal<number | null>(null);

  fromDate = '';
  toDate = '';

  tabs: ReportTabDef[] = [
    { id: 'balance', labelKey: 'inv.rpt.tab.balance', icon: 'inventory_2', path: 'stock-balance' },
    { id: 'valuation', labelKey: 'inv.rpt.tab.valuation', icon: 'payments', path: 'valuation' },
    { id: 'movements', labelKey: 'inv.rpt.tab.movements', icon: 'timeline', path: 'movements' },
    { id: 'waste', labelKey: 'inv.rpt.tab.waste', icon: 'delete_forever', path: 'waste' },
    { id: 'adjustments', labelKey: 'inv.rpt.tab.adjustments', icon: 'tune', path: 'adjustments' },
    { id: 'consumption', labelKey: 'inv.rpt.tab.consumption', icon: 'restaurant', path: 'consumption' },
    { id: 'purchases', labelKey: 'inv.rpt.tab.purchases', icon: 'shopping_cart', path: 'purchases' },
    { id: 'suppliers', labelKey: 'inv.rpt.tab.suppliers', icon: 'local_shipping', path: 'suppliers' }
  ];

  breadcrumbs = [
    { labelKey: 'nav.inventory', path: '/inventory/dashboard' },
    { labelKey: 'inv.nav.reports' }
  ];

  ngOnInit(): void {
    const to = new Date();
    const from = new Date();
    from.setDate(to.getDate() - 30);
    this.toDate = to.toISOString().slice(0, 10);
    this.fromDate = from.toISOString().slice(0, 10);
    this.load();
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  selectTab(id: ReportTab): void {
    this.tab.set(id);
    this.load();
  }

  load(): void {
    const def = this.tabs.find(x => x.id === this.tab());
    if (!def) return;

    this.loading.set(true);
    this.error.set(null);
    this.valuationTotal.set(null);
    this.valuationItemCount.set(null);

    let params = new HttpParams().set('page', 1).set('pageSize', 100);
    if (this.fromDate) params = params.set('fromDate', this.fromDate);
    if (this.toDate) params = params.set('toDate', this.toDate);

    this.http.get<unknown>(`${this.base}/${def.path}`, { params }).pipe(
      catchError(err => {
        this.error.set(err?.error?.error ?? this.t('inv.error.message'));
        this.loading.set(false);
        this.rows.set([]);
        return of(null);
      })
    ).subscribe(data => {
      if (data == null) return;
      this.mapResponse(this.tab(), data);
      this.loading.set(false);
    });
  }

  columns(): string[] {
    const sample = this.rows()[0];
    return sample ? Object.keys(sample) : [];
  }

  cell(row: Record<string, unknown>, key: string): string {
    const v = row[key];
    if (v == null) return '—';
    if (typeof v === 'number') return Number.isInteger(v) ? String(v) : v.toFixed(2);
    if (typeof v === 'string' && /^\d{4}-\d{2}-\d{2}/.test(v)) {
      try { return new Date(v).toLocaleString(); } catch { return v; }
    }
    return String(v);
  }

  columnLabel(key: string): string {
    const map: Record<string, string> = {
      itemName: 'inv.ops.col.item',
      sku: 'inv.field.code',
      warehouseName: 'inv.ops.col.warehouse',
      quantity: 'inv.ops.col.qty',
      unitCost: 'inv.ops.col.cost',
      totalValue: 'inv.rpt.col.totalValue',
      date: 'inv.ops.col.date',
      transactionType: 'inv.ops.col.type',
      quantityChange: 'inv.ops.col.qty',
      reason: 'inv.field.notes',
      recordCount: 'inv.rpt.col.count',
      totalQuantity: 'inv.ops.col.qty',
      totalCost: 'inv.rpt.col.totalValue',
      count: 'inv.rpt.col.count',
      totalAdjustment: 'inv.ops.col.qty',
      consumedQuantity: 'inv.ops.col.qty',
      supplierName: 'inv.ops.col.supplier',
      orderCount: 'inv.rpt.col.count',
      totalAmount: 'inv.rpt.col.totalValue',
      onTimeRate: 'inv.rpt.col.onTime'
    };
    return map[key] ? this.t(map[key]) : key;
  }

  exportCsv(): void {
    const cols = this.columns();
    const data = this.rows();
    if (!cols.length || !data.length) return;
    const lines = [
      cols.join(','),
      ...data.map(r => cols.map(c => JSON.stringify(this.cell(r, c))).join(','))
    ];
    const blob = new Blob([lines.join('\n')], { type: 'text/csv;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `inventory-${this.tab()}-${this.toDate || 'export'}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  }

  private mapResponse(tab: ReportTab, data: unknown): void {
    if (tab === 'valuation' && data && typeof data === 'object') {
      const v = data as { totalValue?: number; itemCount?: number; topItems?: Record<string, unknown>[] };
      this.valuationTotal.set(v.totalValue ?? 0);
      this.valuationItemCount.set(v.itemCount ?? 0);
      this.rows.set(v.topItems ?? []);
      return;
    }
    this.rows.set(Array.isArray(data) ? (data as Record<string, unknown>[]) : []);
  }
}
