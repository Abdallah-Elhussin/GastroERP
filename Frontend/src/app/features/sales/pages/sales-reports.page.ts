import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../../core/services/language.service';
import { BackOfficeSalesReportRepository } from '../../../core/repositories/back-office-sales-report.repository';
import {
  BackOfficeSalesReport,
  BackOfficeSalesReportNamedValue
} from '../../../core/models/back-office-sales-report.models';

interface BarRow {
  label: string;
  value: number;
  count: number;
  pct: number;
}

interface DocCountGroup {
  titleKey: string;
  rows: { status: string; count: number }[];
}

@Component({
  selector: 'app-sales-reports-page',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule],
  templateUrl: './sales-reports.page.html',
  styleUrl: './sales-reports.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SalesReportsPage implements OnInit {
  private repo = inject(BackOfficeSalesReportRepository);
  lang = inject(LanguageService);

  loading = signal(false);
  error = signal<string | null>(null);
  report = signal<BackOfficeSalesReport | null>(null);
  fromDate = signal(this.isoDaysAgo(30));
  toDate = signal(this.todayIso());

  customerBars = computed(() => this.toBars(this.report()?.salesByCustomer ?? []));
  itemBars = computed(() => this.toBars(this.report()?.salesByItem ?? []));
  dayBars = computed(() => this.toBars(this.report()?.salesByDay ?? []));

  docCountGroups = computed<DocCountGroup[]>(() => {
    const dc = this.report()?.documentCounts;
    if (!dc) return [];
    const toRows = (rec: Record<string, number>) =>
      Object.entries(rec).map(([status, count]) => ({ status, count }));
    return [
      { titleKey: 'bos.rpt.doc.invoices', rows: toRows(dc.invoices) },
      { titleKey: 'bos.rpt.doc.orders', rows: toRows(dc.orders) },
      { titleKey: 'bos.rpt.doc.quotations', rows: toRows(dc.quotations) },
      { titleKey: 'bos.rpt.doc.deliveryNotes', rows: toRows(dc.deliveryNotes) },
      { titleKey: 'bos.rpt.doc.returns', rows: toRows(dc.returns) },
      { titleKey: 'bos.rpt.doc.debitNotes', rows: toRows(dc.debitNotes) }
    ].filter(g => g.rows.length > 0);
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
    this.repo
      .getReport({ from: this.fromDate() || null, to: this.toDate() || null, topCustomers: 10, topItems: 10 })
      .subscribe({
        next: report => {
          this.report.set(report);
          this.loading.set(false);
        },
        error: err => {
          this.error.set(err?.error?.error ?? this.t('bos.rpt.loadFailed'));
          this.loading.set(false);
        }
      });
  }

  applyFilters(): void {
    this.load();
  }

  money(n: number | undefined | null): string {
    const v = Number(n) || 0;
    return v.toLocaleString(this.lang.language() === 'ar' ? 'ar-SA' : 'en-SA', {
      maximumFractionDigits: 2
    });
  }

  private toBars(rows: BackOfficeSalesReportNamedValue[]): BarRow[] {
    const max = Math.max(1, ...rows.map(r => r.value));
    return rows.map(r => ({ label: r.label, value: r.value, count: r.count, pct: (r.value / max) * 100 }));
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
