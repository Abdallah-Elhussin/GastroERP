import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-finance-ops-coming-soon-page',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <div class="coming-soon">
      <mat-icon>construction</mat-icon>
      <h1>{{ t('fin.ops.comingSoon.title') }}</h1>
      <p>{{ t('fin.ops.comingSoon.body') }}</p>
    </div>
  `,
  styles: `
    .coming-soon {
      display: grid;
      place-items: center;
      gap: 8px;
      padding: 64px 24px;
      text-align: center;
      color: var(--text-secondary);
    }
    mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      color: var(--primary, #2563eb);
    }
    h1 {
      margin: 0;
      color: var(--text-primary);
      font-size: 1.4rem;
    }
    p {
      margin: 0;
      max-width: 420px;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FinanceOpsComingSoonPage {
  private lang = inject(LanguageService);
  t = (key: string) => this.lang.t(key);
}
