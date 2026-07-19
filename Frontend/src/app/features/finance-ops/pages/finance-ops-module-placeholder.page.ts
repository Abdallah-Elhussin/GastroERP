import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { LanguageService } from '../../core/services/language.service';

@Component({
  selector: 'app-finance-ops-module-placeholder-page',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule],
  template: `
    <div class="ops-placeholder">
      <header>
        <mat-icon>construction</mat-icon>
        <div>
          <h1>{{ t(titleKey()) }}</h1>
          <p>{{ t('fin.ops.placeholder.subtitle') }}</p>
        </div>
      </header>

      <section class="card">
        <h2>{{ t('fin.ops.placeholder.scopeTitle') }}</h2>
        <ul>
          <li>{{ t('fin.ops.placeholder.scope.list') }}</li>
          <li>{{ t('fin.ops.placeholder.scope.create') }}</li>
          <li>{{ t('fin.ops.placeholder.scope.approve') }}</li>
          <li>{{ t('fin.ops.placeholder.scope.post') }}</li>
          <li>{{ t('fin.ops.placeholder.scope.audit') }}</li>
        </ul>
        <p class="muted">{{ t('fin.ops.placeholder.note') }}</p>
        <a routerLink="/finance-ops/opening-balances" class="btn primary">{{ t('fin.ops.nav.openingBalances') }}</a>
      </section>
    </div>
  `,
  styles: `
    .ops-placeholder {
      display: flex;
      flex-direction: column;
      gap: 16px;
      padding: 8px 4px 24px;
    }
    header {
      display: flex;
      gap: 12px;
      align-items: flex-start;
    }
    header mat-icon {
      font-size: 36px;
      width: 36px;
      height: 36px;
      color: var(--primary, #2563eb);
    }
    h1 {
      margin: 0;
      font-size: 1.35rem;
    }
    header p {
      margin: 4px 0 0;
      color: var(--text-secondary);
    }
    .card {
      border: 1px solid var(--border-color, #e2e8f0);
      border-radius: 12px;
      background: #fff;
      padding: 16px;
      max-width: 720px;
    }
    h2 {
      margin: 0 0 8px;
      font-size: 1rem;
    }
    ul {
      margin: 0 0 12px;
      padding-inline-start: 1.25rem;
      color: var(--text-secondary);
    }
    .muted {
      color: var(--text-secondary);
      font-size: 0.9rem;
    }
    .btn.primary {
      display: inline-flex;
      margin-top: 8px;
      padding: 8px 14px;
      border-radius: 8px;
      background: var(--primary, #2563eb);
      color: #fff;
      text-decoration: none;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FinanceOpsModulePlaceholderPage {
  private lang = inject(LanguageService);
  private route = inject(ActivatedRoute);

  titleKey = toSignal(
    this.route.data.pipe(map(d => (d['moduleKey'] as string) || 'fin.ops.comingSoon.title')),
    { initialValue: 'fin.ops.comingSoon.title' }
  );

  t = (key: string) => this.lang.t(key);
}
