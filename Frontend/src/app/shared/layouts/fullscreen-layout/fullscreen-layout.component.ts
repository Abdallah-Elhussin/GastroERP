import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-fullscreen-layout',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen w-screen bg-[var(--bg-canvas)] flex flex-col justify-between overflow-y-auto p-4 sm:p-8 text-left">
      <!-- Fullscreen banner logo -->
      <header class="max-w-4xl mx-auto w-full flex items-center justify-between py-4 select-none">
        <div class="flex items-center gap-3">
          <img src="https://images.unsplash.com/photo-1543007630-9710e4a00a20?w=80&auto=format&fit=crop&q=80" alt="Logo" class="h-8 w-8 rounded-lg object-cover" />
          <span class="font-black text-sm tracking-wider uppercase text-[var(--text-primary)]">GastroERP setup wizard</span>
        </div>
      </header>

      <!-- Center Wizard forms -->
      <main class="flex-1 flex items-center justify-center py-6 w-full">
        <div class="max-w-4xl w-full bg-[var(--bg-surface)] border border-[var(--border-color)] rounded-2xl shadow-xl p-8">
          <ng-content></ng-content>
        </div>
      </main>

      <!-- Bottom metadata footer -->
      <footer class="max-w-4xl mx-auto w-full text-center text-[10px] text-[var(--text-muted)] py-4 select-none">
        Step-by-step setup guides to finalize deployment parameters.
      </footer>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      width: 100vw;
      height: 100vh;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FullscreenLayoutComponent {}
