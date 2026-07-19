import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-inventory-skeleton',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="skel" [class.cards]="variant === 'cards'" [class.table]="variant === 'table'" [attr.aria-busy]="true">
      <ng-container *ngIf="variant === 'cards'">
        <div class="skel-card" *ngFor="let i of cards"></div>
      </ng-container>
      <ng-container *ngIf="variant === 'table'">
        <div class="skel-row header"></div>
        <div class="skel-row" *ngFor="let i of rows"></div>
      </ng-container>
      <ng-container *ngIf="variant === 'form'">
        <div class="skel-block" *ngFor="let i of formBlocks"></div>
      </ng-container>
    </div>
  `,
  styles: [`
    .skel { display: grid; gap: 0.75rem; }
    .skel.cards { grid-template-columns: repeat(auto-fill, minmax(160px, 1fr)); }
    .skel-card, .skel-row, .skel-block {
      background: linear-gradient(90deg, var(--bg-canvas) 25%, var(--bg-surface-hover, var(--bg-canvas)) 50%, var(--bg-canvas) 75%);
      background-size: 200% 100%;
      animation: shimmer 1.2s ease-in-out infinite;
      border-radius: 0.75rem;
    }
    .skel-card { height: 88px; }
    .skel-row { height: 44px; }
    .skel-row.header { height: 36px; opacity: 0.7; }
    .skel-block { height: 72px; }
    @keyframes shimmer {
      0% { background-position: 200% 0; }
      100% { background-position: -200% 0; }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventorySkeletonComponent {
  @Input() variant: 'cards' | 'table' | 'form' = 'table';
  @Input() count = 6;

  get cards(): number[] {
    return Array.from({ length: this.count }, (_, i) => i);
  }

  get rows(): number[] {
    return Array.from({ length: this.count }, (_, i) => i);
  }

  get formBlocks(): number[] {
    return Array.from({ length: Math.max(3, this.count) }, (_, i) => i);
  }
}
