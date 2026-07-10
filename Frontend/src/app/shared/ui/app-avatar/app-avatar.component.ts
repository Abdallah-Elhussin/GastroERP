import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-avatar',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="relative inline-block select-none">
      <div 
        [ngClass]="[
          'rounded-xl overflow-hidden border border-[var(--border-color)] bg-[var(--bg-canvas)] flex items-center justify-center font-bold text-[var(--text-primary)]',
          sizeClasses[size]
        ]"
      >
        <img 
          *ngIf="src; else initialsFallback" 
          [src]="src" 
          [alt]="name" 
          class="h-full w-full object-cover"
        />
        <ng-template #initialsFallback>
          <span>{{ getInitials() }}</span>
        </ng-template>
      </div>

      <!-- Optional status dot -->
      <span 
        *ngIf="status"
        [ngClass]="[
          'absolute bottom-[-2px] right-[-2px] w-2.5 h-2.5 rounded-full border border-[var(--bg-surface)] shadow-sm',
          statusColor[status]
        ]"
      ></span>
    </div>
  `,
  styles: [`
    :host {
      display: inline-block;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppAvatarComponent {
  @Input() src?: string;
  @Input() name = '';
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  @Input() status?: 'online' | 'offline' | 'busy';

  sizeClasses = {
    sm: 'w-8 h-8 text-[10px]',
    md: 'w-10 h-10 text-xs',
    lg: 'w-14 h-14 text-sm'
  };

  statusColor = {
    online: 'bg-[var(--success-color)]',
    offline: 'bg-[var(--text-muted)]',
    busy: 'bg-[var(--danger-color)]'
  };

  getInitials(): string {
    if (!this.name) return '';
    const parts = this.name.split(' ');
    if (parts.length > 1) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return this.name[0].toUpperCase();
  }
}
