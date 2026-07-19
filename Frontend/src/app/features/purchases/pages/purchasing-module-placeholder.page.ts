import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../../core/services/language.service';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';

@Component({
  selector: 'app-purchasing-module-placeholder-page',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="placeholder">
      <h1>{{ t(moduleKey()) }}</h1>
      <p>{{ t(noteKey()) }}</p>
      <p class="muted">{{ t('pur.placeholder.subtitle') }}</p>
      <a routerLink="/purchases/dashboard" class="btn primary">{{ t('pur.nav.dashboard') }}</a>
    </div>
  `,
  styles: `
    .placeholder {
      padding: 24px 8px;
      display: flex;
      flex-direction: column;
      gap: 10px;
      max-width: 640px;
    }
    h1 {
      margin: 0;
    }
    .muted {
      color: var(--text-secondary, #64748b);
    }
    .btn {
      display: inline-flex;
      width: fit-content;
      padding: 8px 14px;
      border-radius: 8px;
      background: #2563eb;
      color: #fff;
      text-decoration: none;
    }
  `
})
export class PurchasingModulePlaceholderPage {
  private route = inject(ActivatedRoute);
  lang = inject(LanguageService);

  moduleKey = toSignal(
    this.route.data.pipe(map(d => (d['moduleKey'] as string) || 'pur.nav.dashboard')),
    { initialValue: 'pur.nav.dashboard' }
  );
  noteKey = toSignal(
    this.route.data.pipe(map(d => (d['noteKey'] as string) || 'pur.placeholder.subtitle')),
    { initialValue: 'pur.placeholder.subtitle' }
  );

  t = (key: string) => this.lang.t(key);
}
