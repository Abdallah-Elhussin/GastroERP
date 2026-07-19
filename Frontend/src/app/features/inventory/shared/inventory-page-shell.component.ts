import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../../core/services/language.service';
import { inject } from '@angular/core';

export interface InventoryBreadcrumbItem {
  labelKey: string;
  path?: string;
}

@Component({
  selector: 'app-inventory-page-shell',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule],
  template: `
    <div class="inv-shell">
      <nav class="inv-breadcrumb" aria-label="Breadcrumb">
        <a routerLink="/inventory/dashboard" class="crumb-home" [title]="t('nav.inventory')">
          <mat-icon>inventory_2</mat-icon>
        </a>
        <ng-container *ngFor="let crumb of breadcrumbs; let last = last">
          <mat-icon class="sep">chevron_right</mat-icon>
          <a *ngIf="crumb.path && !last" [routerLink]="crumb.path" class="crumb">{{ t(crumb.labelKey) }}</a>
          <span *ngIf="!crumb.path || last" class="crumb current">{{ t(crumb.labelKey) }}</span>
        </ng-container>
      </nav>

      <header class="inv-header" *ngIf="titleKey || title">
        <div class="inv-header-text">
          <h1>{{ title || t(titleKey!) }}</h1>
          <p *ngIf="subtitleKey || subtitle">{{ subtitle || t(subtitleKey!) }}</p>
        </div>
        <div class="inv-header-actions">
          <ng-content select="[shellActions]"></ng-content>
        </div>
      </header>

      <div class="inv-body">
        <ng-content></ng-content>
      </div>
    </div>
  `,
  styles: [`
    .inv-shell { display: flex; flex-direction: column; gap: 1rem; padding: 1.25rem 1.5rem; min-height: 100%; }
    .inv-breadcrumb { display: flex; align-items: center; flex-wrap: wrap; gap: 0.15rem; font-size: 0.7rem; }
    .crumb-home { display: inline-flex; color: var(--text-muted); text-decoration: none; }
    .crumb-home mat-icon { font-size: 1rem; width: 1rem; height: 1rem; }
    .sep { font-size: 0.85rem; width: 0.85rem; height: 0.85rem; color: var(--text-muted); }
    .crumb { color: var(--text-muted); text-decoration: none; font-weight: 600; }
    .crumb.current { color: var(--text-primary); }
    .inv-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; flex-wrap: wrap; }
    .inv-header h1 { margin: 0; font-size: 1.35rem; font-weight: 800; }
    .inv-header p { margin: 0.25rem 0 0; font-size: 0.75rem; color: var(--text-muted); }
    .inv-header-actions { display: flex; gap: 0.5rem; flex-wrap: wrap; align-items: center; }
    .inv-body { flex: 1; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryPageShellComponent {
  private lang = inject(LanguageService);

  @Input() breadcrumbs: InventoryBreadcrumbItem[] = [];
  @Input() titleKey?: string;
  @Input() subtitleKey?: string;
  @Input() title?: string;
  @Input() subtitle?: string;

  t(key: string): string {
    return this.lang.t(key);
  }
}
