import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { KitchenComponent } from '../kitchen/kitchen.component';

@Component({
  selector: 'app-kitchen-display',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, KitchenComponent],
  template: `
    <div class="kitchen-display-shell">
      <header class="kitchen-display-header">
        <div class="kitchen-display-brand">
          <mat-icon>flatware</mat-icon>
          <span>GastroERP — KDS</span>
        </div>
        <a routerLink="/dashboard" class="kitchen-display-back">
          <mat-icon>arrow_back</mat-icon>
        </a>
      </header>
      <main class="kitchen-display-main">
        <app-kitchen />
      </main>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      width: 100vw;
      height: 100vh;
      overflow: hidden;
      background: var(--bg-canvas);
    }

    .kitchen-display-shell {
      display: flex;
      flex-direction: column;
      height: 100%;
    }

    .kitchen-display-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0.75rem 1.25rem;
      border-bottom: 1px solid var(--border-color);
      background: var(--bg-surface);
      flex-shrink: 0;
    }

    .kitchen-display-brand {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-weight: 800;
      font-size: 0.875rem;
    }

    .kitchen-display-back {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 2rem;
      height: 2rem;
      border-radius: 0.5rem;
      color: var(--text-muted);
    }

    .kitchen-display-main {
      flex: 1;
      min-height: 0;
      padding: 1rem;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class KitchenDisplayComponent {}
