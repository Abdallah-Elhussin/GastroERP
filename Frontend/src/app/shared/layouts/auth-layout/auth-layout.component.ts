import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LanguageService } from '../../../core/services/language.service';
import { DataService } from '../../../core/services/data.service';

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="h-screen w-screen flex overflow-hidden">
      <!-- Left Split Panel: Promo graphics (hidden on mobile) -->
      <div class="hidden lg:flex flex-1 relative flex-col justify-between p-12 text-white text-left">
        <!-- Background image -->
        <img 
          [src]="dataService.branding().loginBgUrl" 
          alt="Login Background" 
          class="absolute inset-0 w-full h-full object-cover z-0"
        />
        <!-- Glass overlay -->
        <div class="absolute inset-0 bg-slate-950 bg-opacity-70 z-0"></div>

        <!-- Upper logo -->
        <div class="relative z-10 flex items-center gap-3 select-none">
          <img 
            [src]="dataService.branding().logoUrl" 
            alt="Logo" 
            class="h-8 w-8 rounded-lg object-cover"
          />
          <span class="font-black text-sm tracking-wider uppercase">{{ dataService.branding().name }}</span>
        </div>

        <!-- Bottom promo note -->
        <div class="relative z-10 flex flex-col gap-4 select-none">
          <h2 class="text-3xl font-extrabold tracking-tight leading-tight">
            {{ t('layout.auth.tagline') }}
          </h2>
          <p class="text-xs text-gray-300 max-w-md font-light leading-relaxed">
            {{ t('layout.auth.promo') }}
          </p>
        </div>

        <!-- Footer watermark -->
        <div class="relative z-10 text-[9px] text-gray-400 select-none">
          {{ t('layout.auth.copyright') }}
        </div>
      </div>

      <!-- Right Split Panel: Auth gate card -->
      <div class="w-full lg:w-[480px] bg-[var(--bg-canvas)] flex flex-col justify-center items-center p-8 overflow-y-auto">
        <ng-content></ng-content>
      </div>
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
export class AuthLayoutComponent {
  langService = inject(LanguageService);
  dataService = inject(DataService);

  t(key: string): string {
    return this.langService.t(key);
  }
}
