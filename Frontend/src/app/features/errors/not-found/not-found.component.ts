import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AppButtonComponent } from '../../../shared/ui/app-button/app-button.component';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [CommonModule, MatIconModule, AppButtonComponent],
  template: `
    <div class="min-h-screen bg-[var(--bg-canvas)] flex flex-col items-center justify-center p-6 text-center select-none animate-fade-in">
      <div class="max-w-md w-full bg-[var(--bg-surface)] border border-[var(--border-color)] rounded-3xl p-8 shadow-xl flex flex-col items-center gap-6">
        
        <!-- Large animated 404 graphic icon -->
        <div class="w-24 h-24 rounded-full bg-amber-500 bg-opacity-10 text-amber-500 flex items-center justify-center animate-pulse">
          <mat-icon class="text-5xl" style="width: 48px; height: 48px; font-size: 48px;">search_off</mat-icon>
        </div>

        <div class="flex flex-col gap-2">
          <h1 class="text-4xl font-extrabold text-[var(--text-primary)] tracking-tight">404</h1>
          <h2 class="text-lg font-bold text-[var(--text-secondary)]">{{ t('error.notFound.title') }}</h2>
          <p class="text-xs text-[var(--text-muted)] leading-relaxed mt-1">
            {{ t('error.notFound.desc') }}
          </p>
        </div>

        <app-button 
          variant="primary" 
          icon="home" 
          (click)="goHome()"
          [ariaLabel]="t('error.returnDashboard')"
          class="w-full mt-2"
        >
          {{ t('error.returnDashboard') }}
        </app-button>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NotFoundComponent {
  private router = inject(Router);
  private langService = inject(LanguageService);

  goHome(): void {
    this.router.navigate(['/dashboard']);
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
