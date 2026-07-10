import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      [ngClass]="[
        'rounded-2xl border border-[var(--border-color)] overflow-hidden transition-all duration-200 text-left flex flex-col',
        glass ? 'glass-effect' : 'bg-[var(--bg-surface)]',
        hoverElevation ? 'hover:shadow-md hover:translate-y-[-1px]' : 'shadow-sm'
      ]"
    >
      <!-- Optional Title Header -->
      <div *ngIf="title || hasHeader" class="px-5 py-4 border-b border-[var(--border-color-muted)] flex justify-between items-center select-none bg-[var(--bg-canvas)] bg-opacity-10">
        <div class="flex flex-col gap-0.5">
          <h3 *ngIf="title" class="font-extrabold text-xs text-[var(--text-primary)] uppercase tracking-wide">{{ title }}</h3>
          <span *ngIf="subtitle" class="text-[9px] text-[var(--text-secondary)]">{{ subtitle }}</span>
        </div>
        <ng-content select="[header-actions]"></ng-content>
      </div>

      <!-- Card Content Body -->
      <div class="p-5 flex-1">
        <ng-content></ng-content>
      </div>

      <!-- Card Footer -->
      <div *ngIf="hasFooter" class="px-5 py-3 border-t border-[var(--border-color-muted)] bg-[var(--bg-canvas)] bg-opacity-5 select-none">
        <ng-content select="[footer]"></ng-content>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppCardComponent {
  @Input() title?: string;
  @Input() subtitle?: string;
  @Input() hoverElevation = false;
  @Input() glass = false;

  @Input() hasHeader = false;
  @Input() hasFooter = false;
}
