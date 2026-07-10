import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-pos-layout',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="h-full flex overflow-hidden -m-6 relative">
      <!-- Left Side: Catalog grids -->
      <div class="flex-1 flex flex-col overflow-hidden p-4 gap-4 min-w-0">
        <ng-content select="[catalog]"></ng-content>
      </div>

      <!-- Right Side: Order side drawer -->
      <aside class="pos-sidebar border-l border-[var(--border-color)] bg-[var(--bg-surface)] flex flex-col overflow-hidden flex-shrink-0">
        <ng-content select="[drawer]"></ng-content>
      </aside>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      width: 100%;
      height: 100%;
    }

    .pos-sidebar {
      width: min(100%, 26.5rem);
      min-width: 22rem;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class POSLayoutComponent {}
