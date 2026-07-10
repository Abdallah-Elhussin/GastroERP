import { Component, ChangeDetectionStrategy, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <div class="flex flex-col items-center justify-center p-8 text-center text-[var(--text-muted)] gap-3 select-none">
      <div class="w-12 h-12 rounded-full bg-[var(--bg-canvas)] border border-[var(--border-color)] flex items-center justify-center text-[var(--text-muted)]">
        <mat-icon class="text-xl">{{ icon }}</mat-icon>
      </div>
      <div class="flex flex-col gap-1">
        <h4 class="text-xs font-bold text-[var(--text-primary)]">{{ displayTitle }}</h4>
        <p class="text-[10px] text-[var(--text-secondary)] max-w-xs leading-relaxed">{{ displayMessage }}</p>
      </div>
      <ng-content></ng-content>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      width: 100%;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppEmptyStateComponent {
  private langService = inject(LanguageService);

  @Input() icon = 'sentiment_neutral';
  @Input() title?: string;
  @Input() message?: string;

  get displayTitle(): string {
    this.langService.language();
    return this.title ?? this.t('empty.title');
  }

  get displayMessage(): string {
    this.langService.language();
    return this.message ?? this.t('empty.subtitle');
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
