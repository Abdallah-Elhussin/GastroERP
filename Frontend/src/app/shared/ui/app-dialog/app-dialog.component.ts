import { Component, ChangeDetectionStrategy, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-dialog',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <div 
      *ngIf="isOpen"
      class="fixed inset-0 bg-black bg-opacity-50 backdrop-blur-sm z-50 flex items-center justify-center p-4 animate-fade-in"
    >
      <div 
        [ngClass]="[
          'bg-[var(--bg-surface)] border border-[var(--border-color)] w-full rounded-2xl shadow-2xl p-6 flex flex-col gap-6 animate-scale-up',
          maxWidth
        ]"
      >
        <!-- Modal Dialog Header -->
        <div class="flex justify-between items-center text-left border-b border-[var(--border-color)] pb-4 select-none">
          <div class="flex flex-col gap-0.5">
            <h3 *ngIf="title" class="font-extrabold text-base text-[var(--text-primary)]">{{ title }}</h3>
            <p *ngIf="subtitle" class="text-[11px] text-[var(--text-secondary)]">{{ subtitle }}</p>
          </div>
          <button (click)="onClose()" class="text-[var(--text-muted)] hover:text-[var(--text-primary)] cursor-pointer">
            <mat-icon>close</mat-icon>
          </button>
        </div>

        <!-- Dialog Body -->
        <div class="flex-1">
          <ng-content></ng-content>
        </div>
      </div>
    </div>
  `,
  styles: [`
    /* Keyframes are imported from styles.scss globally */
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppDialogComponent {
  @Input() isOpen = false;
  @Input() title = '';
  @Input() subtitle = '';
  @Input() maxWidth = 'max-w-md';
  @Output() close = new EventEmitter<void>();

  onClose(): void {
    this.close.emit();
  }
}
