import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-stat-card',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <div class="bg-[var(--bg-surface)] border border-[var(--border-color)] rounded-2xl p-6 flex items-center justify-between shadow-sm select-none">
      <div class="flex flex-col gap-2 text-left">
        <span class="text-xs text-[var(--text-muted)] font-semibold uppercase tracking-wider">
          {{ label }}
        </span>
        <span class="text-2xl font-black text-[var(--text-primary)]">
          {{ value }}
        </span>
        <span *ngIf="subtitle" class="text-[10px] text-[var(--text-muted)] font-medium">{{ subtitle }}</span>
        <span *ngIf="trend" [ngClass]="['text-[10px] font-semibold flex items-center gap-0.5', trendColor]">
          <mat-icon *ngIf="trendIcon" class="text-xs">{{ trendIcon }}</mat-icon>
          <span>{{ trend }}</span>
        </span>
      </div>
      <div *ngIf="icon" [ngClass]="['w-12 h-12 rounded-xl flex items-center justify-center', iconBgClass]">
        <mat-icon>{{ icon }}</mat-icon>
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
export class AppStatCardComponent {
  @Input() label = '';
  @Input() value = '';
  @Input() subtitle = '';
  @Input() trend = '';
  @Input() trendIcon = '';
  @Input() trendColor = 'text-emerald-500';
  @Input() icon = '';
  @Input() iconBgClass = 'bg-blue-500 bg-opacity-10 text-[var(--accent-color)]';
}
