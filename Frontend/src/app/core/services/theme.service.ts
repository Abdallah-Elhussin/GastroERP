import { Injectable, signal, effect, computed } from '@angular/core';

export type AppTheme = 'light' | 'dark';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly THEME_KEY = 'gastro-erp-theme';
  
  // Signals state
  theme = signal<AppTheme>('light');
  isDarkMode = computed(() => this.theme() === 'dark');

  constructor() {
    // Load default theme
    const savedTheme = localStorage.getItem(this.THEME_KEY) as AppTheme;
    if (savedTheme) {
      this.theme.set(savedTheme);
    } else {
      // System preference
      const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
      this.theme.set(prefersDark ? 'dark' : 'light');
    }

    // Effect to apply class on HTML body
    effect(() => {
      const currentTheme = this.theme();
      localStorage.setItem(this.THEME_KEY, currentTheme);
      
      const body = document.body;
      if (currentTheme === 'dark') {
        body.classList.add('dark');
      } else {
        body.classList.remove('dark');
      }
    });
  }

  toggleTheme(): void {
    this.theme.update(current => current === 'light' ? 'dark' : 'light');
  }

  setTheme(theme: AppTheme): void {
    this.theme.set(theme);
  }
}
