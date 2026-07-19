import { Component, ChangeDetectionStrategy, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-inventory-empty-state',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <div class="empty">
      <mat-icon>{{ icon }}</mat-icon>
      <h3>{{ title || t(titleKey) }}</h3>
      <p *ngIf="message || messageKey">{{ message || t(messageKey!) }}</p>
      <button *ngIf="actionLabelKey || actionLabel" type="button" class="btn-primary" (click)="action.emit()">
        {{ actionLabel || t(actionLabelKey!) }}
      </button>
    </div>
  `,
  styles: [`
    .empty {
      display: flex; flex-direction: column; align-items: center; justify-content: center;
      gap: 0.5rem; padding: 3rem 1.5rem; text-align: center;
      border: 1px dashed var(--border-color); border-radius: 1rem; background: var(--bg-surface);
    }
    mat-icon { font-size: 2.5rem; width: 2.5rem; height: 2.5rem; color: var(--text-muted); }
    h3 { margin: 0; font-size: 1rem; font-weight: 700; }
    p { margin: 0; font-size: 0.75rem; color: var(--text-muted); max-width: 28rem; }
    .btn-primary {
      margin-top: 0.75rem; padding: 0.55rem 1rem; border: none; border-radius: 0.65rem;
      background: var(--primary-color); color: var(--primary-contrast); font-size: 0.75rem; font-weight: 700; cursor: pointer;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryEmptyStateComponent {
  private lang = inject(LanguageService);

  @Input() icon = 'inventory_2';
  @Input() titleKey = 'inv.empty.title';
  @Input() messageKey?: string;
  @Input() actionLabelKey?: string;
  @Input() title?: string;
  @Input() message?: string;
  @Input() actionLabel?: string;
  @Output() action = new EventEmitter<void>();

  t(key: string): string {
    return this.lang.t(key);
  }
}
