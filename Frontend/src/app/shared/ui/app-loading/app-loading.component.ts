import { Component, ChangeDetectionStrategy, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-loading',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="flex items-center justify-center p-6 w-full select-none">
      
      <!-- Loading Spinner option -->
      <div *ngIf="type === 'spinner'" class="flex flex-col items-center gap-2">
        <svg class="animate-spin h-6.5 w-6.5 text-[var(--accent-color)]" fill="none" viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
        </svg>
        <span class="text-[9px] font-bold uppercase tracking-wider text-[var(--text-muted)]">{{ t('loading.text') }}</span>
      </div>

      <!-- Card Skeleton templates -->
      <div *ngIf="type === 'skeleton'" class="w-full flex flex-col gap-4 animate-skeleton">
        <div class="h-6 bg-[var(--border-color)] rounded-xl w-3/4"></div>
        <div class="h-4 bg-[var(--border-color-muted)] rounded-lg w-full"></div>
        <div class="h-4 bg-[var(--border-color-muted)] rounded-lg w-5/6"></div>
      </div>

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
export class AppLoadingComponent {
  private langService = inject(LanguageService);

  @Input() type: 'spinner' | 'skeleton' = 'spinner';

  t(key: string): string {
    return this.langService.t(key);
  }
}
