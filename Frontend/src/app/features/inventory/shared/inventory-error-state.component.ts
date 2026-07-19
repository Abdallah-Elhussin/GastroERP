import { Component, ChangeDetectionStrategy, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-inventory-error-state',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <div class="error" role="alert">
      <mat-icon>error_outline</mat-icon>
      <h3>{{ title || t(titleKey) }}</h3>
      <p>{{ message || t(messageKey) }}</p>
      <button *ngIf="showRetry" type="button" class="btn-secondary" (click)="retry.emit()">
        {{ t('common.retry') }}
      </button>
    </div>
  `,
  styles: [`
    .error {
      display: flex; flex-direction: column; align-items: center; gap: 0.4rem;
      padding: 1.5rem; border-radius: 0.85rem;
      background: var(--danger-color-bg, #fef2f2); color: var(--danger-color-text, #b91c1c);
      text-align: center;
    }
    mat-icon { font-size: 1.75rem; width: 1.75rem; height: 1.75rem; }
    h3 { margin: 0; font-size: 0.9rem; font-weight: 700; }
    p { margin: 0; font-size: 0.75rem; opacity: 0.9; }
    .btn-secondary {
      margin-top: 0.5rem; padding: 0.45rem 0.9rem; border-radius: 0.55rem; cursor: pointer;
      border: 1px solid currentColor; background: transparent; font-size: 0.7rem; font-weight: 700;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryErrorStateComponent {
  private lang = inject(LanguageService);

  @Input() titleKey = 'inv.error.title';
  @Input() messageKey = 'inv.error.message';
  @Input() title?: string;
  @Input() message?: string;
  @Input() showRetry = true;
  @Output() retry = new EventEmitter<void>();

  t(key: string): string {
    return this.lang.t(key);
  }
}
