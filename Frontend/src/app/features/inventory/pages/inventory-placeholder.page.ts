import { Component, ChangeDetectionStrategy, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { toSignal } from '@angular/core/rxjs-interop';
import { LanguageService } from '../../../core/services/language.service';
import { InventoryPageShellComponent } from '../shared/inventory-page-shell.component';
import { InventoryEmptyStateComponent } from '../shared/inventory-empty-state.component';

@Component({
  selector: 'app-inventory-placeholder-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatIconModule,
    InventoryPageShellComponent,
    InventoryEmptyStateComponent
  ],
  template: `
    <app-inventory-page-shell
      [breadcrumbs]="breadcrumbs()"
      [titleKey]="titleKey()"
      subtitleKey="inv.phase.comingSoonSubtitle"
    >
      <a shellActions routerLink="/inventory/dashboard" class="btn-secondary">
        <mat-icon>arrow_back</mat-icon>
        {{ t('inv.nav.dashboard') }}
      </a>

      <app-inventory-empty-state
        icon="construction"
        [title]="phaseTitle()"
        [message]="phaseMessage()"
        actionLabelKey="inv.nav.dashboard"
        (action)="goDashboard()"
      />
    </app-inventory-page-shell>
  `,
  styles: [`
    .btn-secondary {
      display: inline-flex; align-items: center; gap: 0.35rem;
      padding: 0.5rem 0.85rem; border-radius: 0.65rem; font-size: 0.75rem; font-weight: 700;
      text-decoration: none; border: 1px solid var(--border-color); background: var(--bg-canvas); color: var(--text-primary);
    }
    .btn-secondary mat-icon { font-size: 1rem; width: 1rem; height: 1rem; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryPlaceholderPage {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private lang = inject(LanguageService);

  private data = toSignal(this.route.data, { initialValue: this.route.snapshot.data });

  titleKey = computed(() => (this.data()['titleKey'] as string) ?? 'inv.phase.comingSoon');
  phase = computed(() => (this.data()['phase'] as string) ?? '?');

  breadcrumbs = computed(() => {
    const keys = (this.data()['breadcrumb'] as string[]) ?? ['nav.inventory'];
    return keys.map((labelKey, index) => ({
      labelKey,
      path: index === 0 ? '/inventory/dashboard' : undefined
    }));
  });

  phaseTitle = computed(() =>
    `${this.t(this.titleKey())} — ${this.t('inv.phase.label').replace('{phase}', this.phase())}`
  );

  phaseMessage = computed(() => this.t('inv.phase.comingSoonMessage'));

  goDashboard(): void {
    void this.router.navigate(['/inventory/dashboard']);
  }

  t(key: string): string {
    return this.lang.t(key);
  }
}
