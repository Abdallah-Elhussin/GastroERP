import { Component, ChangeDetectionStrategy, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatSelectModule } from '@angular/material/select';
import { MatDividerModule } from '@angular/material/divider';
import { Router, RouterModule } from '@angular/router';
import { ThemeService } from '../../../core/services/theme.service';
import { LanguageService } from '../../../core/services/language.service';
import { DataService } from '../../../core/services/data.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-toolbar',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatSelectModule,
    MatDividerModule,
    RouterModule
  ],
  templateUrl: './app-toolbar.component.html',
  styles: [`
    :host {
      display: block;
      width: 100%;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppToolbarComponent {
  themeService = inject(ThemeService);
  langService = inject(LanguageService);
  dataService = inject(DataService);
  authService = inject(AuthService);
  router = inject(Router);

  bookmarks = signal<string[]>(['/dashboard', '/pos']);
  activeUrl = signal<string>('/dashboard');
  
  notifications = signal<{ textKey: string; time: string; read: boolean }[]>([
    { textKey: 'toolbar.notify.lowStock', time: '5m ago', read: false },
    { textKey: 'toolbar.notify.kds', time: '12m ago', read: false },
    { textKey: 'toolbar.notify.cash', time: '1h ago', read: true }
  ]);

  unreadCount = computed(() => this.notifications().filter((n: { read: boolean }) => !n.read).length);

  constructor() {
    this.router.events.subscribe(() => {
      this.activeUrl.set(this.router.url);
    });
  }

  hasBookmark(): boolean {
    return this.bookmarks().includes(this.activeUrl());
  }

  toggleBookmark(): void {
    const url = this.activeUrl();
    this.bookmarks.update((current: string[]) => {
      if (current.includes(url)) {
        return current.filter((path: string) => path !== url);
      } else {
        return [...current, url];
      }
    });
  }

  markAllRead(): void {
    this.notifications.update((list: { textKey: string; time: string; read: boolean }[]) => 
      list.map((n: { textKey: string; time: string; read: boolean }) => ({ ...n, read: true }))
    );
  }

  getRouteLabel(path: string): string {
    if (path.includes('dashboard')) return this.t('nav.dashboard');
    if (path.includes('pos')) return this.t('nav.pos');
    if (path.includes('kds')) return this.t('nav.kitchen');
    if (path.includes('inventory')) return this.t('nav.inventory');
    if (path.includes('menu')) return this.t('nav.menu');
    if (path.includes('hr')) return this.t('nav.hr');
    if (path.includes('reporting')) return this.t('nav.reporting');
    if (path.includes('finance')) return this.t('nav.finance');
    if (path.includes('crm')) return this.t('nav.crm');
    if (path.includes('branding')) return this.t('nav.branding');
    if (path.includes('media')) return this.t('nav.media');
    if (path.includes('settings')) return this.t('nav.settings');
    return path;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  openPaletteCommand(): void {
    window.dispatchEvent(new KeyboardEvent('keydown', { key: 'k', ctrlKey: true }));
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
