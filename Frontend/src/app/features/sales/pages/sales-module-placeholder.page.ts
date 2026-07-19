import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-sales-module-placeholder-page',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule],
  template: `
    <section class="wrap">
      <a routerLink="/sales/dashboard" class="back">
        <mat-icon>arrow_back</mat-icon>
        {{ t('sal.dash.back') }}
      </a>
      <h1>{{ t(moduleKey()) }}</h1>
      <p>{{ t(noteKey()) }}</p>
    </section>
  `,
  styles: [
    `
      .wrap {
        padding: 1.5rem;
        max-width: 40rem;
      }
      .back {
        display: inline-flex;
        align-items: center;
        gap: 0.35rem;
        margin-bottom: 1rem;
        color: var(--text-muted);
        text-decoration: none;
        font-size: 0.85rem;
      }
      h1 {
        margin: 0 0 0.5rem;
        font-size: 1.25rem;
      }
      p {
        color: var(--text-muted);
        line-height: 1.5;
      }
    `
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SalesModulePlaceholderPage implements OnInit {
  private route = inject(ActivatedRoute);
  lang = inject(LanguageService);
  moduleKey = signal('sal.nav.orders');
  noteKey = signal('sal.note.orders');

  ngOnInit(): void {
    this.moduleKey.set((this.route.snapshot.data['moduleKey'] as string) || 'sal.nav.orders');
    this.noteKey.set((this.route.snapshot.data['noteKey'] as string) || 'sal.note.orders');
  }

  t(key: string): string {
    return this.lang.t(key);
  }
}
