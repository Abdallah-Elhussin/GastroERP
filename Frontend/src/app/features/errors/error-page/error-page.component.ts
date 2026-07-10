import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AppButtonComponent } from '../../../shared/ui/app-button/app-button.component';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-error-page',
  standalone: true,
  imports: [CommonModule, MatIconModule, AppButtonComponent],
  template: `
    <div class="min-h-screen bg-[var(--bg-canvas)] flex flex-col items-center justify-center p-6 text-center select-none animate-fade-in">
      <div class="max-w-md w-full bg-[var(--bg-surface)] border border-[var(--border-color)] rounded-3xl p-8 shadow-xl flex flex-col items-center gap-6">
        
        <!-- Icon based on code status -->
        <div 
          [ngClass]="[
            'w-24 h-24 rounded-full flex items-center justify-center animate-pulse',
            code() === '403' ? 'bg-red-500 bg-opacity-10 text-red-500' : 'bg-rose-500 bg-opacity-10 text-rose-500'
          ]"
        >
          <mat-icon class="text-5xl" style="width: 48px; height: 48px; font-size: 48px;">
            {{ code() === '403' ? 'gavel' : 'cloud_off' }}
          </mat-icon>
        </div>

        <div class="flex flex-col gap-2">
          <h1 class="text-4xl font-extrabold text-[var(--text-primary)] tracking-tight">{{ code() }}</h1>
          <h2 class="text-lg font-bold text-[var(--text-secondary)]">
            {{ code() === '403' ? t('error.forbidden.title') : t('error.server.title') }}
          </h2>
          <p class="text-xs text-[var(--text-muted)] leading-relaxed mt-1">
            {{ code() === '403' ? t('error.forbidden.desc') : t('error.server.desc') }}
          </p>
        </div>

        <div class="flex flex-col gap-2 w-full mt-2">
          <app-button 
            variant="primary" 
            icon="home" 
            (click)="goHome()"
            [ariaLabel]="t('error.returnDashboard')"
            class="w-full"
          >
            {{ t('error.returnDashboard') }}
          </app-button>
          
          <app-button 
            variant="outline" 
            icon="refresh" 
            (click)="retry()"
            [ariaLabel]="t('error.retry')"
            class="w-full"
          >
            {{ t('error.retry') }}
          </app-button>
        </div>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ErrorPageComponent {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private langService = inject(LanguageService);

  code = signal<string>('500');

  constructor() {
    this.route.queryParams.subscribe(params => {
      if (params['code']) {
        this.code.set(params['code']);
      }
    });
  }

  goHome(): void {
    this.router.navigate(['/dashboard']);
  }

  retry(): void {
    window.location.reload();
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
