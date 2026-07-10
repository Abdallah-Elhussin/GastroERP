import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-kitchen-layout',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="h-full flex flex-col gap-6 -m-6 p-6">
      <!-- Toolbar station actions -->
      <div class="flex flex-col sm:flex-row justify-between items-center gap-4 flex-shrink-0">
        <ng-content select="[actions]"></ng-content>
      </div>

      <!-- Columns lanes container grid -->
      <div class="flex-1 grid grid-cols-1 md:grid-cols-3 gap-6 min-h-0">
        <div class="flex flex-col h-full bg-[var(--bg-surface)] border border-[var(--border-color)] rounded-2xl overflow-hidden shadow-sm">
          <ng-content select="[lane-new]"></ng-content>
        </div>
        <div class="flex flex-col h-full bg-[var(--bg-surface)] border border-[var(--border-color)] rounded-2xl overflow-hidden shadow-sm">
          <ng-content select="[lane-preparing]"></ng-content>
        </div>
        <div class="flex flex-col h-full bg-[var(--bg-surface)] border border-[var(--border-color)] rounded-2xl overflow-hidden shadow-sm">
          <ng-content select="[lane-ready]"></ng-content>
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      width: 100%;
      height: 100%;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class KitchenLayoutComponent {}
