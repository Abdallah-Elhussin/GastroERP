import {
  ChangeDetectionStrategy,
  Component,
  OnDestroy,
  OnInit,
  computed,
  effect,
  inject,
  signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CdkDragDrop, DragDropModule, moveItemInArray } from '@angular/cdk/drag-drop';
import { MatIconModule } from '@angular/material/icon';
import { Subscription, interval, of } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { LanguageService } from '../../core/services/language.service';
import { AuthService } from '../../core/services/auth.service';
import { DataService } from '../../core/services/data.service';
import { DashboardRepository } from '../../core/repositories/dashboard.repository';
import {
  DashboardActivity,
  DashboardInsight,
  DashboardKpi,
  DashboardNamedValue,
  DashboardNotification,
  DashboardPeriod,
  DashboardQuickAction,
  DashboardSeriesPoint,
  DashboardTableRow,
  DashboardWidgetLayout,
  EnterpriseDashboardFilter,
  EnterpriseDashboardFinance,
  DashboardHeader
} from '../../core/models/enterprise-dashboard.models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, DragDropModule, MatIconModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardComponent implements OnInit, OnDestroy {
  private repo = inject(DashboardRepository);
  private lang = inject(LanguageService);
  private auth = inject(AuthService);
  private data = inject(DataService);

  private readonly LAYOUT_KEY = 'gastro-enterprise-dashboard-layout-v1';
  private clockSub?: Subscription;
  private refreshSub?: Subscription;

  loading = signal(true);
  error = signal<string | null>(null);
  isCustomizing = signal(false);
  now = signal(new Date());
  period = signal<DashboardPeriod>(0);
  customFrom = signal('');
  customTo = signal('');

  header = signal<DashboardHeader | null>(null);
  kpis = signal<DashboardKpi[]>([]);
  notifications = signal<DashboardNotification[]>([]);
  insights = signal<DashboardInsight[]>([]);
  quickActions = signal<DashboardQuickAction[]>([]);
  trend = signal<DashboardSeriesPoint[]>([]);
  revenueSources = signal<DashboardNamedValue[]>([]);
  paymentMethods = signal<DashboardNamedValue[]>([]);
  topItems = signal<DashboardTableRow[]>([]);
  worstItems = signal<DashboardTableRow[]>([]);
  topCustomers = signal<DashboardTableRow[]>([]);
  lowStock = signal<DashboardTableRow[]>([]);
  kitchen = signal({ pending: 0, preparing: 0, ready: 0, served: 0, delayed: 0, avgPrepMinutes: 0 });
  delivery = signal({ inProgress: 0, delivered: 0, delayed: 0, avgDeliveryMinutes: 0 });
  hr = signal({ present: 0, absent: 0, late: 0, workedHours: 0 });
  finance = signal<EnterpriseDashboardFinance['snapshot'] | null>(null);
  activities = signal<DashboardActivity[]>([]);
  lastSyncedAt = signal<Date | null>(null);

  private readonly defaultWidgets: DashboardWidgetLayout[] = [
    { id: 'kpis', titleKey: 'ed.widget.kpis', visible: true, size: 'full' },
    { id: 'sales', titleKey: 'ed.widget.sales', visible: true, size: 'full' },
    { id: 'revenueSources', titleKey: 'ed.widget.revenueSources', visible: true, size: 'half' },
    { id: 'payments', titleKey: 'ed.widget.payments', visible: true, size: 'half' },
    { id: 'topItems', titleKey: 'ed.widget.topItems', visible: true, size: 'half' },
    { id: 'worstItems', titleKey: 'ed.widget.worstItems', visible: true, size: 'half' },
    { id: 'topCustomers', titleKey: 'ed.widget.topCustomers', visible: true, size: 'half' },
    { id: 'lowInventory', titleKey: 'ed.widget.lowInventory', visible: true, size: 'half' },
    { id: 'kitchen', titleKey: 'ed.widget.kitchen', visible: true, size: 'third' },
    { id: 'delivery', titleKey: 'ed.widget.delivery', visible: true, size: 'third' },
    { id: 'hr', titleKey: 'ed.widget.hr', visible: true, size: 'third' },
    { id: 'finance', titleKey: 'ed.widget.finance', visible: true, size: 'half' },
    { id: 'activities', titleKey: 'ed.widget.activities', visible: true, size: 'half' },
    { id: 'notifications', titleKey: 'ed.widget.notifications', visible: true, size: 'half' },
    { id: 'insights', titleKey: 'ed.widget.insights', visible: true, size: 'half' },
    { id: 'quickActions', titleKey: 'ed.widget.quickActions', visible: true, size: 'full' }
  ];

  widgets = signal<DashboardWidgetLayout[]>([...this.defaultWidgets]);
  activeWidgets = computed(() => this.widgets().filter(w => w.visible));

  branchLabel = computed(() => this.header()?.branchName || this.data.selectedBranch());
  companyLabel = computed(() => this.header()?.companyName || 'GastroERP');
  userLabel = computed(
    () => this.header()?.userName || this.auth.currentUser()?.fullName || '—'
  );
  isRtl = computed(() => this.lang.language() === 'ar');

  periods: { value: DashboardPeriod; key: string }[] = [
    { value: 0, key: 'ed.period.today' },
    { value: 1, key: 'ed.period.yesterday' },
    { value: 2, key: 'ed.period.week' },
    { value: 3, key: 'ed.period.month' },
    { value: 4, key: 'ed.period.custom' }
  ];

  constructor() {
    const saved = localStorage.getItem(this.LAYOUT_KEY);
    if (saved) {
      try {
        const parsed = JSON.parse(saved) as DashboardWidgetLayout[];
        if (Array.isArray(parsed) && parsed.length) {
          const byId = new Map(parsed.map(w => [w.id, w]));
          this.widgets.set(
            this.defaultWidgets.map(d => ({
              ...d,
              ...(byId.get(d.id) ?? {}),
              titleKey: d.titleKey
            }))
          );
        }
      } catch {
        /* ignore */
      }
    }

    effect(() => {
      localStorage.setItem(this.LAYOUT_KEY, JSON.stringify(this.widgets()));
    });
  }

  ngOnInit(): void {
    this.clockSub = interval(1000).subscribe(() => this.now.set(new Date()));
    this.loadAll();
    this.refreshSub = interval(60_000)
      .pipe(switchMap(() => of(null)))
      .subscribe(() => this.loadAll(true));
  }

  ngOnDestroy(): void {
    this.clockSub?.unsubscribe();
    this.refreshSub?.unsubscribe();
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  setPeriod(p: DashboardPeriod): void {
    this.period.set(p);
    if (p !== 4) this.loadAll();
  }

  applyCustomRange(): void {
    if (!this.customFrom() || !this.customTo()) return;
    this.period.set(4);
    this.loadAll();
  }

  toggleCustomize(): void {
    this.isCustomizing.update(v => !v);
  }

  toggleWidget(id: string): void {
    this.widgets.update(list => list.map(w => (w.id === id ? { ...w, visible: !w.visible } : w)));
  }

  resetLayout(): void {
    this.widgets.set(this.defaultWidgets.map(w => ({ ...w })));
    this.isCustomizing.set(false);
  }

  drop(event: CdkDragDrop<DashboardWidgetLayout[]>): void {
    if (!this.isCustomizing()) return;
    const visible = this.widgets().filter(w => w.visible);
    const hidden = this.widgets().filter(w => !w.visible);
    moveItemInArray(visible, event.previousIndex, event.currentIndex);
    this.widgets.set([...visible, ...hidden]);
  }

  kpiLabel(kpi: DashboardKpi): string {
    const key = `ed.kpi.${kpi.key}`;
    const translated = this.t(key);
    return translated === key ? kpi.label : translated;
  }

  qaLabel(a: DashboardQuickAction): string {
    const key = `ed.qa.${a.key}`;
    const translated = this.t(key);
    return translated === key ? a.label : translated;
  }

  formatMoney(value: number, unit?: string): string {
    const currency = unit || this.header()?.currency || 'SAR';
    try {
      return new Intl.NumberFormat(this.isRtl() ? 'ar-SA' : 'en-SA', {
        style: 'currency',
        currency,
        maximumFractionDigits: 0
      }).format(value);
    } catch {
      return `${value.toLocaleString()} ${currency}`;
    }
  }

  formatNumber(value: number): string {
    return new Intl.NumberFormat(this.isRtl() ? 'ar-SA' : 'en-US').format(value);
  }

  kpiValue(kpi: DashboardKpi): string {
    if (kpi.unit) return this.formatMoney(kpi.value, kpi.unit);
    return this.formatNumber(kpi.value);
  }

  kpiTrendClass(kpi: DashboardKpi): string {
    const change = kpi.changePercent;
    if (change == null) return 'flat';
    const up = change >= 0;
    const good = kpi.isHigherBetter ? up : !up;
    return good ? 'up' : 'down';
  }

  sparkHeights(spark: number[]): number[] {
    const max = Math.max(...spark, 1);
    return spark.map(v => Math.max(8, Math.round((v / max) * 100)));
  }

  pieSegments(items: DashboardNamedValue[]): { name: string; value: number; pct: number; color: string }[] {
    const total = items.reduce((s, i) => s + Number(i.value || 0), 0) || 1;
    const colors = ['#e67e22', '#3498db', '#2ecc71', '#9b59b6', '#e74c3c', '#1abc9c', '#f39c12', '#7f8c8d'];
    return items.map((i, idx) => ({
      name: i.name,
      value: i.value,
      pct: Math.round((Number(i.value) / total) * 100),
      color: colors[idx % colors.length]
    }));
  }

  donutStyle(items: DashboardNamedValue[]): string {
    const segs = this.pieSegments(items);
    if (!segs.length) return 'conic-gradient(#e5e7eb 0 100%)';
    let acc = 0;
    const parts = segs.map(s => {
      const start = acc;
      acc += s.pct;
      return `${s.color} ${start}% ${acc}%`;
    });
    return `conic-gradient(${parts.join(', ')})`;
  }

  barHeights(points: DashboardSeriesPoint[]): { label: string; h: number; sales: number }[] {
    const max = Math.max(...points.map(p => p.sales), 1);
    return points.slice(-14).map(p => ({
      label: p.label.slice(-5),
      h: Math.max(6, Math.round((p.sales / max) * 100)),
      sales: p.sales
    }));
  }

  private filter(): EnterpriseDashboardFilter {
    return {
      period: this.period(),
      fromDate: this.period() === 4 ? this.customFrom() || null : null,
      toDate: this.period() === 4 ? this.customTo() || null : null
    };
  }

  private loadAll(silent = false): void {
    if (!silent) {
      this.loading.set(true);
      this.error.set(null);
    }
    const f = this.filter();

    // Overview is required; other widgets load independently so one failure doesn't blank the page.
    this.repo.getOverview(f).subscribe({
      next: overview => {
        if (!overview?.header) {
          this.error.set(this.t('ed.loadFailed'));
          this.loading.set(false);
          return;
        }
        this.header.set(overview.header);
        this.kpis.set(overview.kpis ?? []);
        this.notifications.set(overview.notifications ?? []);
        this.insights.set(overview.insights ?? []);
        this.quickActions.set(overview.quickActions ?? []);
        this.lastSyncedAt.set(new Date());
        this.loading.set(false);
        this.error.set(null);
        this.loadWidgets(f);
      },
      error: () => {
        this.error.set(this.t('ed.loadFailed'));
        this.loading.set(false);
      }
    });
  }

  private loadWidgets(f: EnterpriseDashboardFilter): void {
    this.repo.getSales(f).subscribe(s => {
      if (!s) return;
      this.trend.set(s.trend ?? []);
      this.revenueSources.set(s.revenueSources ?? []);
      this.paymentMethods.set(s.paymentMethods ?? []);
    });
    this.repo.getProducts(f).subscribe(p => {
      if (!p) return;
      this.topItems.set(p.topSelling ?? []);
      this.worstItems.set(p.worstSelling ?? []);
    });
    this.repo.getCustomers(f).subscribe(c => {
      if (c) this.topCustomers.set(c.topCustomers ?? []);
    });
    this.repo.getInventory(f).subscribe(i => {
      if (i) this.lowStock.set(i.lowStockItems ?? []);
    });
    this.repo.getKitchen(f).subscribe(k => {
      if (k?.status) this.kitchen.set(k.status);
    });
    this.repo.getDelivery(f).subscribe(d => {
      if (d?.status) this.delivery.set(d.status);
    });
    this.repo.getHr(f).subscribe(h => {
      if (h?.snapshot) this.hr.set(h.snapshot);
    });
    this.repo.getFinance(f).subscribe(fin => {
      if (fin?.snapshot) this.finance.set(fin.snapshot);
    });
    this.repo.getActivities(f).subscribe(a => {
      if (a) this.activities.set(a.items ?? []);
    });
  }
}
