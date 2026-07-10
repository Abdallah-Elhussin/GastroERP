import { Component, ChangeDetectionStrategy, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { AppButtonComponent } from '../../shared/ui/app-button/app-button.component';
import { AppCardComponent } from '../../shared/ui/app-card/app-card.component';
import { AppChartComponent } from '../../shared/ui/app-chart/app-chart.component';
import { AppStatCardComponent } from '../../shared/ui/app-stat-card/app-stat-card.component';
import { AppTableComponent } from '../../shared/ui/app-table/app-table.component';
import { LanguageService } from '../../core/services/language.service';
import { FinanceRepository } from '../../core/repositories/finance.repository';

@Component({
  selector: 'app-finance',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    AppButtonComponent,
    AppCardComponent,
    AppChartComponent,
    AppStatCardComponent,
    AppTableComponent
  ],
  templateUrl: './finance.component.html',
  styleUrl: './finance.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FinanceComponent implements OnInit {
  langService = inject(LanguageService);
  financeRepo = inject(FinanceRepository);

  ledgerData = signal<any[]>([
    { code: 'TX-9821', desc: 'Cash Drawer Daily Seeding', type: 'Revenue', amount: 150.00, date: '2026-07-08' },
    { code: 'TX-9822', desc: 'Wagyu Beef Supply Batch #3', type: 'Expense', amount: 4800.00, date: '2026-07-07' },
    { code: 'TX-9823', desc: 'Table 12 Dine-in Credit Payment', type: 'Revenue', amount: 320.50, date: '2026-07-06' },
    { code: 'TX-9824', desc: 'Truffle Ingredient Purchasing', type: 'Expense', amount: 1200.00, date: '2026-07-05' }
  ]);

  columns = computed(() => {
    this.langService.language();
    return [
      { key: 'code', label: this.langService.t('finance.col.code'), sortable: true },
      { key: 'desc', label: this.langService.t('finance.col.desc'), sortable: true },
      { key: 'type', label: this.langService.t('finance.col.type'), sortable: true },
      { key: 'amount', label: this.langService.t('finance.col.amount'), sortable: true },
      { key: 'date', label: this.langService.t('finance.col.date'), sortable: true }
    ];
  });

  ngOnInit(): void {
    this.financeRepo.getLedgerEntries().subscribe(data => {
      if (data && data.length > 0) {
        this.ledgerData.set(data);
      }
    });
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
