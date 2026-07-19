import {
  ChangeDetectionStrategy,
  Component,
  OnDestroy,
  OnInit,
  computed,
  inject,
  signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { catchError, of } from 'rxjs';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { SalesDashboardRepository } from '../../../core/repositories/sales-dashboard.repository';
import {
  SalesDashboardAlert,
  SalesDashboardNamedValue,
  SalesDashboardSummary
} from '../../../core/models/sales-dashboard.models';

type Tone = 'green' | 'blue' | 'orange' | 'red' | 'violet';

interface KpiCard {
  key: string;
  value: string;
  change?: number;
  icon: string;
  tone: Tone;
  path: string;
  meta?: string;
}

interface QuickAction {
  key: string;
  icon: string;
  path: string;
}

@Component({
  selector: 'app-sales-dashboard-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, MatIconModule],
  templateUrl: './sales-dashboard.page.html',
  styleUrl: './sales-dashboard.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SalesDashboardPage implements OnInit, OnDestroy {
  private repo = inject(SalesDashboardRepository);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(true);
  error = signal<string | null>(null);
  data = signal<SalesDashboardSummary | null>(null);
  fromDate = signal(this.isoDaysAgo(30));
  toDate = signal(this.todayIso());
  autoRefreshSec = signal<0 | 30 | 60>(0);
  isFullscreen = signal(false);
  private timer: ReturnType<typeof setInterval> | null = null;

  kpiCards = signal<KpiCard[]>([]);
  alerts = signal<SalesDashboardAlert[]>([]);

  quickActions: QuickAction[] = [
    { key: 'sal.dash.quick.newInvoice', icon: 'receipt_long', path: '/sales/invoices/new' },
    { key: 'sal.dash.quick.order', icon: 'shopping_bag', path: '/sales/invoices' },
    { key: 'sal.dash.quick.return', icon: 'assignment_return', path: '/sales/returns' },
    { key: 'sal.dash.quick.customer', icon: 'person_add', path: '/crm' },
    { key: 'sal.dash.quick.report', icon: 'bar_chart', path: '/reporting' }
  ];

  dayBars = computed(() => this.normalizeBars(this.data()?.charts.salesByDay ?? []));
  branchBars = computed(() => this.normalizeBars(this.data()?.charts.salesByBranch ?? []));
  cashierBars = computed(() => this.normalizeBars(this.data()?.charts.salesByCashier ?? []));
  posBars = computed(() => this.normalizeBars(this.data()?.charts.salesByPosDevice ?? []));
  itemBars = computed(() => this.normalizeBars(this.data()?.charts.topItems ?? []));
  payBars = computed(() => this.normalizeBars(this.data()?.charts.paymentMethods ?? []));
  categoryBars = computed(() => this.normalizeBars(this.data()?.charts.salesByCategory ?? []));

  heatMax = computed(() => {
    const cells = this.data()?.charts.salesByHour ?? [];
    return Math.max(1, ...cells.map(c => c.value));
  });

  hours = Array.from({ length: 24 }, (_, i) => i);
  days = [0, 1, 2, 3, 4, 5, 6];

  ngOnInit(): void {
    this.load();
  }

  ngOnDestroy(): void {
    this.clearTimer();
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  isRtl(): boolean {
    return this.lang.language() === 'ar';
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo
      .getDashboard({ fromDate: this.fromDate(), toDate: this.toDate() })
      .pipe(
        catchError(err => {
          this.error.set(err?.error?.error ?? this.t('sal.dash.loadFailed'));
          this.loading.set(false);
          return of(null);
        })
      )
      .subscribe(summary => {
        if (!summary) return;
        this.data.set(summary);
        this.alerts.set(summary.alerts ?? []);
        this.kpiCards.set(this.buildKpis(summary));
        this.loading.set(false);
      });
  }

  applyFilters(): void {
    this.load();
  }

  setAutoRefresh(sec: 0 | 30 | 60): void {
    this.autoRefreshSec.set(sec);
    this.clearTimer();
    if (sec > 0) {
      this.timer = setInterval(() => this.load(), sec * 1000);
    }
  }

  toggleFullscreen(): void {
    const el = document.documentElement;
    if (!document.fullscreenElement) {
      void el.requestFullscreen?.();
      this.isFullscreen.set(true);
    } else {
      void document.exitFullscreen?.();
      this.isFullscreen.set(false);
    }
  }

  alertMessage(a: SalesDashboardAlert): string {
    return this.isRtl() ? a.messageAr : a.messageEn;
  }

  heatValue(day: number, hour: number): number {
    return this.data()?.charts.salesByHour.find(c => c.dayOfWeek === day && c.hour === hour)?.value ?? 0;
  }

  heatOpacity(day: number, hour: number): number {
    return this.heatValue(day, hour) / this.heatMax();
  }

  dayLabel(d: number): string {
    const keys = [
      'sal.dash.dow.sun',
      'sal.dash.dow.mon',
      'sal.dash.dow.tue',
      'sal.dash.dow.wed',
      'sal.dash.dow.thu',
      'sal.dash.dow.fri',
      'sal.dash.dow.sat'
    ];
    return this.t(keys[d] ?? 'sal.dash.dow.sun');
  }

  money(n: number | undefined | null): string {
    const v = Number(n) || 0;
    return v.toLocaleString(this.isRtl() ? 'ar-SA' : 'en-SA', {
      minimumFractionDigits: 0,
      maximumFractionDigits: 2
    });
  }

  private buildKpis(s: SalesDashboardSummary): KpiCard[] {
    const k = s.kpis;
    return [
      {
        key: 'sal.dash.kpi.salesToday',
        value: this.money(k.salesToday),
        change: k.salesTodayChangePercent,
        icon: 'today',
        tone: 'green',
        path: '/sales/invoices'
      },
      {
        key: 'sal.dash.kpi.salesWeek',
        value: this.money(k.salesWeek),
        change: k.salesWeekChangePercent,
        icon: 'date_range',
        tone: 'green',
        path: '/sales/invoices'
      },
      {
        key: 'sal.dash.kpi.salesMonth',
        value: this.money(k.salesMonth),
        change: k.salesMonthChangePercent,
        icon: 'calendar_month',
        tone: 'green',
        path: '/sales/invoices'
      },
      {
        key: 'sal.dash.kpi.salesYear',
        value: this.money(k.salesYear),
        icon: 'event',
        tone: 'blue',
        path: '/sales/invoices'
      },
      {
        key: 'sal.dash.kpi.invoiceCount',
        value: String(k.invoiceCount),
        icon: 'receipt',
        tone: 'blue',
        path: '/sales/invoices',
        meta: `${this.t('sal.dash.kpi.avgTicket')}: ${this.money(k.averageInvoiceValue)}`
      },
      {
        key: 'sal.dash.kpi.activeCustomers',
        value: String(k.activeCustomers),
        icon: 'groups',
        tone: 'blue',
        path: '/crm',
        meta: `${this.t('sal.dash.kpi.newCustomers')}: ${k.newCustomers}`
      },
      {
        key: 'sal.dash.kpi.returns',
        value: this.money(k.returnsTotal),
        icon: 'undo',
        tone: k.returnsRatioPercent >= 10 ? 'red' : 'orange',
        path: '/sales/returns',
        meta: `${k.returnsRatioPercent.toFixed(1)}%`
      },
      {
        key: 'sal.dash.kpi.posCount',
        value: String(k.posInvoiceCount),
        icon: 'receipt_long',
        tone: 'violet',
        path: '/sales/invoices',
        meta: this.t('bos.inv.status.posted')
      },
      {
        key: 'sal.dash.kpi.grossProfit',
        value: this.money(k.grossProfit),
        icon: 'payments',
        tone: 'green',
        path: '/sales/invoices',
        meta: this.t('bos.inv.payment.cash')
      },
      {
        key: 'sal.dash.kpi.netProfit',
        value: this.money(k.netProfit),
        icon: 'account_balance',
        tone: 'green',
        path: '/sales/invoices',
        meta: this.t('bos.inv.payment.credit')
      },
      {
        key: 'sal.dash.kpi.discounts',
        value: this.money(k.cogs),
        icon: 'credit_score',
        tone: 'orange',
        path: '/sales/invoices',
        meta: this.t('bos.dash.creditOutstanding')
      },
      {
        key: 'sal.dash.kpi.cancellations',
        value: this.money(k.cancellationsTotal),
        icon: 'cancel',
        tone: 'red',
        path: '/sales/invoices'
      }
    ];
  }

  private normalizeBars(rows: SalesDashboardNamedValue[]): { label: string; value: number; pct: number }[] {
    const max = Math.max(1, ...rows.map(r => r.value));
    return rows.map(r => ({ label: r.label, value: r.value, pct: (r.value / max) * 100 }));
  }

  private clearTimer(): void {
    if (this.timer) {
      clearInterval(this.timer);
      this.timer = null;
    }
  }

  private todayIso(): string {
    return new Date().toISOString().slice(0, 10);
  }

  private isoDaysAgo(days: number): string {
    const d = new Date();
    d.setDate(d.getDate() - days);
    return d.toISOString().slice(0, 10);
  }
}
