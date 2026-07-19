import { Component, ChangeDetectionStrategy, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { AppButtonComponent } from '../../shared/ui/app-button/app-button.component';
import { AppCardComponent } from '../../shared/ui/app-card/app-card.component';
import { AppStatCardComponent } from '../../shared/ui/app-stat-card/app-stat-card.component';
import { AppTableComponent } from '../../shared/ui/app-table/app-table.component';
import { LanguageService } from '../../core/services/language.service';
import { CrmRepository } from '../../core/repositories/crm.repository';

@Component({
  selector: 'app-crm',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    AppButtonComponent,
    AppCardComponent,
    AppStatCardComponent,
    AppTableComponent
  ],
  templateUrl: './crm.component.html',
  styleUrl: './crm.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CrmComponent implements OnInit {
  langService = inject(LanguageService);
  crmRepo = inject(CrmRepository);

  customersData = signal<any[]>([
    { id: 'C-001', name: 'Eleanore Sterling', points: 2450, tier: 'VIP Gold', phone: '+1 (555) 091-2382' },
    { id: 'C-002', name: 'Robert Finch', points: 850, tier: 'Silver Member', phone: '+1 (555) 890-3472' },
    { id: 'C-003', name: 'Alba Sterling', points: 5200, tier: 'VIP Platinum', phone: '+1 (555) 124-9081' }
  ]);

  columns = computed(() => {
    this.langService.language();
    return [
      { key: 'id', label: this.langService.t('crm.col.id'), sortable: true },
      { key: 'name', label: this.langService.t('crm.col.name'), sortable: true },
      { key: 'points', label: this.langService.t('crm.col.points'), sortable: true },
      { key: 'tier', label: this.langService.t('crm.col.tier'), sortable: true },
      { key: 'phone', label: this.langService.t('crm.col.phone'), sortable: true },
      { key: 'actions', label: this.langService.t('crm.col.actions'), sortable: false }
    ];
  });

  ngOnInit(): void {
    this.crmRepo.getCustomers(1, 200).subscribe(data => {
      if (data && data.length > 0) {
        this.customersData.set(
          data.map(c => ({
            id: c.customerNumber,
            name: c.fullName,
            points: c.creditLimit ?? 0,
            tier: c.paymentTerms || '—',
            phone: c.mobile
          }))
        );
      }
    });
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
