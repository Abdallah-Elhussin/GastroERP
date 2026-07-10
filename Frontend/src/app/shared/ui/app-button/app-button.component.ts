import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-button',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <button
      [type]="type"
      [disabled]="disabled || loading"
      [attr.aria-label]="ariaLabel"
      [ngClass]="[
        'inline-flex items-center justify-center font-bold transition-all rounded-xl cursor-pointer select-none',
        variantClasses[variant],
        sizeClasses[size],
        disabled || loading ? 'opacity-50 cursor-not-allowed' : 'hover:opacity-95 active:scale-[0.98]'
      ]"
    >
      <mat-icon *ngIf="icon && !loading" class="text-base me-1.5">{{ icon }}</mat-icon>
      
      <!-- Spinner loader if loading -->
      <svg *ngIf="loading" class="animate-spin -ml-1 mr-2 h-4 w-4 text-current" fill="none" viewBox="0 0 24 24">
        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
      </svg>
      
      <ng-content></ng-content>
    </button>
  `,
  styles: [`
    :host {
      display: inline-block;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppButtonComponent {
  @Input() variant: 'primary' | 'secondary' | 'danger' | 'outline' = 'primary';
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  @Input() disabled = false;
  @Input() loading = false;
  @Input() icon?: string;
  @Input() type: 'button' | 'submit' | 'reset' = 'button';
  @Input() ariaLabel?: string;

  variantClasses = {
    primary: 'bg-[var(--primary-color)] text-[var(--primary-contrast)] shadow-sm',
    secondary: 'bg-[var(--bg-canvas)] text-[var(--text-primary)] border border-[var(--border-color)]',
    danger: 'bg-[var(--danger-color)] text-white shadow-sm',
    outline: 'bg-transparent text-[var(--text-primary)] border border-[var(--border-color)] hover:bg-[var(--bg-surface-hover)]'
  };

  sizeClasses = {
    sm: 'px-3 py-1.5 text-[10px]',
    md: 'px-4.5 py-2.5 text-xs',
    lg: 'px-6 py-3.5 text-sm'
  };
}
