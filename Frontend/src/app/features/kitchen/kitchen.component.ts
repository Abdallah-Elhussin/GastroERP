import { Component, ChangeDetectionStrategy, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../core/services/language.service';
import { KitchenLayoutComponent } from '../../shared/layouts/kitchen-layout/kitchen-layout.component';
import { KdsTicket } from '../../core/models/erp.models';
import { KdsService } from '../../core/services/kds.service';
import { KdsAlertService } from '../../core/services/kds-alert.service';

@Component({
  selector: 'app-kitchen',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    KitchenLayoutComponent
  ],
  templateUrl: './kitchen.component.html',
  styleUrl: './kitchen.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class KitchenComponent {
  kdsService = inject(KdsService);
  langService = inject(LanguageService);
  alertService = inject(KdsAlertService);

  constructor() {
    effect(() => {
      this.alertService.playOverdueAlert(this.kdsService.tickets());
    });
  }

  onBump(ticket: KdsTicket): void {
    this.kdsService.bumpTicket(ticket);
  }

  isOverdue(ticket: KdsTicket): boolean {
    return this.alertService.isOverdue(ticket);
  }

  formatTime(totalSeconds: number): string {
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;
    return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
