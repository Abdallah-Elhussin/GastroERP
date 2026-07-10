import { Component, ChangeDetectionStrategy, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../core/services/language.service';
import { PublicLayoutComponent } from '../../shared/layouts/public-layout/public-layout.component';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    PublicLayoutComponent
  ],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LandingComponent {
  langService = inject(LanguageService);

  mockStats = computed(() => {
    this.langService.language();
    return [
      { label: this.langService.t('landing.todaySales'), val: '$12,482.00', color: 'text-emerald-500' },
      { label: this.langService.t('landing.activeTables'), val: '18 / 24', color: 'text-[var(--accent-color)]' },
      { label: this.langService.t('landing.pendingOrders'), val: '04', color: 'text-amber-500' }
    ];
  });

  t(key: string): string {
    return this.langService.t(key);
  }
}
