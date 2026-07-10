import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-public-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="min-h-screen bg-[var(--bg-canvas)] flex flex-col justify-between text-left">
      <!-- Public header navigation -->
      <header class="h-16 px-8 bg-[var(--bg-surface)] border-b border-[var(--border-color)] flex items-center justify-between select-none">
        <div class="flex items-center gap-3">
          <img src="https://images.unsplash.com/photo-1543007630-9710e4a00a20?w=80&auto=format&fit=crop&q=80" alt="Logo" class="h-7 w-7 rounded-lg object-cover" />
          <span class="font-black text-sm tracking-wider uppercase text-[var(--text-primary)]">GastroERP</span>
        </div>
        <div class="flex items-center gap-4">
          <a routerLink="/login" class="text-xs font-bold text-[var(--text-secondary)] hover:text-[var(--text-primary)]">{{ t('layout.public.login') }}</a>
          <a routerLink="/setup" class="bg-[var(--primary-color)] text-[var(--primary-contrast)] hover:bg-[var(--primary-color-hover)] px-4 py-2 rounded-xl text-xs font-bold shadow-sm transition-all">{{ t('layout.public.getStarted') }}</a>
        </div>
      </header>

      <!-- Page Content -->
      <main class="flex-1">
        <ng-content></ng-content>
      </main>

      <!-- Footer -->
      <footer class="py-6 border-t border-[var(--border-color)] text-center text-[10px] text-[var(--text-muted)] select-none">
        {{ t('layout.public.copyright') }}
      </footer>
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
export class PublicLayoutComponent {
  langService = inject(LanguageService);

  t(key: string): string {
    return this.langService.t(key);
  }
}
