import { Component, ChangeDetectionStrategy, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { AppButtonComponent } from '../../shared/ui/app-button/app-button.component';
import { AppCardComponent } from '../../shared/ui/app-card/app-card.component';
import { AppChartComponent } from '../../shared/ui/app-chart/app-chart.component';
import { AppStatCardComponent } from '../../shared/ui/app-stat-card/app-stat-card.component';
import { AppTableComponent } from '../../shared/ui/app-table/app-table.component';
import { LanguageService } from '../../core/services/language.service';
import { ReportsRepository } from '../../core/repositories/reports.repository';

@Component({
  selector: 'app-reports',
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
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReportsComponent implements OnInit {
  langService = inject(LanguageService);
  reportsRepo = inject(ReportsRepository);

  reportsData = signal<any[]>([
    { id: 'R-1092', name: 'Daily Sales & Invoices Summary', category: 'POS Terminal', format: 'PDF', createdDate: '2026-07-08' },
    { id: 'R-1093', name: 'Active Inventory Variance Statement', category: 'Warehouse', format: 'CSV', createdDate: '2026-07-07' },
    { id: 'R-1094', name: 'VAT & Tax Liability Report', category: 'Taxation', format: 'PDF', createdDate: '2026-07-06' },
    { id: 'R-1095', name: 'Employee Punch Clock Summary', category: 'HR Log', format: 'Excel', createdDate: '2026-07-05' }
  ]);

  columns = computed(() => {
    this.langService.language();
    return [
      { key: 'id', label: this.langService.t('reports.col.code'), sortable: true },
      { key: 'name', label: this.langService.t('reports.col.name'), sortable: true },
      { key: 'category', label: this.langService.t('reports.col.moduleCategory'), sortable: true },
      { key: 'format', label: this.langService.t('reports.col.exportFormat'), sortable: true },
      { key: 'createdDate', label: this.langService.t('reports.col.generatedDate'), sortable: true },
      { key: 'actions', label: this.langService.t('common.actions'), sortable: false }
    ];
  });

  channelStats = computed(() => {
    this.langService.language();
    return [
      { name: this.langService.t('reports.channel.pos'), share: '68%', color: 'bg-[var(--accent-color)]' },
      { name: this.langService.t('reports.channel.directDelivery'), share: '18%', color: 'bg-emerald-500' },
      { name: this.langService.t('reports.channel.onlineApps'), share: '14%', color: 'bg-blue-500' }
    ];
  });

  expandedCategories = signal<string[]>([]);

  pivotData = signal<any[]>([
    {
      category: 'Burgers',
      totalSales: 12480.00,
      quantity: 640,
      items: [
        { name: 'Gold Wagyu Burger', sales: 9600.00, qty: 400 },
        { name: 'Classic Beef Burger', sales: 2880.00, qty: 240 }
      ]
    },
    {
      category: 'Beverages',
      totalSales: 4200.00,
      quantity: 840,
      items: [
        { name: 'Organic Green Soda', sales: 3200.00, qty: 640 },
        { name: 'Mineral Water', sales: 1000.00, qty: 200 }
      ]
    }
  ]);

  ngOnInit(): void {
    this.reportsRepo.getReportsKPIs().subscribe(() => {});

    this.reportsRepo.getPivotSalesData().subscribe(pivot => {
      if (pivot && pivot.length > 0) {
        this.pivotData.set(pivot);
      }
    });
  }

  toggleCategory(categoryName: string): void {
    this.expandedCategories.update(current => {
      if (current.includes(categoryName)) {
        return current.filter(name => name !== categoryName);
      } else {
        return [...current, categoryName];
      }
    });
  }

  isExpanded(categoryName: string): boolean {
    return this.expandedCategories().includes(categoryName);
  }

  showScheduler = signal<boolean>(false);
  selectedReportType = signal<string>('Sales & Revenue');
  selectedScheduleFrequency = signal<string>('Daily');
  recipientEmails = signal<string>('manager@gastroerp.com');

  saveSchedule(): void {
    const payload = {
      reportType: this.selectedReportType(),
      frequency: this.selectedScheduleFrequency(),
      email: this.recipientEmails()
    };
    this.reportsRepo.scheduleReport(payload).subscribe(() => {
      alert(
        this.t('reports.scheduleSaved')
          .replace('{type}', this.selectedReportType())
          .replace('{frequency}', this.selectedScheduleFrequency())
          .replace('{email}', this.recipientEmails())
      );
      this.showScheduler.set(false);
    });
  }

  triggerPdfExport(reportName: string): void {
    alert(this.t('reports.pdfExported').replace('{name}', reportName));
  }

  triggerExcelExport(reportName: string): void {
    alert(this.t('reports.excelExported').replace('{name}', reportName));
  }

  triggerPrint(reportName: string): void {
    alert(this.t('reports.printInitialized').replace('{name}', reportName));
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
