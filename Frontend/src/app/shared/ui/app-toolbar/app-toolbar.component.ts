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

    /* Premium Glassmorphism Header */
    .premium-header {
      backdrop-filter: blur(16px);
      background: rgba(255, 255, 255, 0.8) !important;
      border-bottom: 1px solid rgba(227, 229, 235, 0.8) !important;
      box-shadow: 0 4px 20px -2px rgba(0, 0, 0, 0.03) !important;
      transition: all 0.3s ease;
    }

    :host-context(.dark) .premium-header {
      background: rgba(15, 23, 42, 0.85) !important;
      border-bottom: 1px solid rgba(51, 65, 85, 0.5) !important;
      box-shadow: 0 4px 20px -2px rgba(0, 0, 0, 0.15) !important;
    }

    /* Sleek Translucent Search Bar */
    .premium-search {
      background: rgba(241, 243, 246, 0.5) !important;
      border: 1px solid rgba(226, 232, 240, 0.8) !important;
      transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
    }

    :host-context(.dark) .premium-search {
      background: rgba(30, 41, 59, 0.5) !important;
      border: 1px solid rgba(51, 65, 85, 0.7) !important;
    }

    .premium-search:hover {
      background: rgba(241, 243, 246, 0.8) !important;
      border-color: var(--accent-color) !important;
      box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1) !important;
    }

    :host-context(.dark) .premium-search:hover {
      background: rgba(30, 41, 59, 0.75) !important;
      border-color: var(--accent-color) !important;
      box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.2) !important;
    }

    /* Action Icon Buttons */
    .icon-btn {
      width: 36px;
      height: 36px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 10px;
      border: 1px solid rgba(226, 232, 240, 0.8);
      background: rgba(255, 255, 255, 0.4);
      color: var(--text-secondary);
      transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
      cursor: pointer;
    }

    :host-context(.dark) .icon-btn {
      border-color: rgba(51, 65, 85, 0.5);
      background: rgba(30, 41, 59, 0.3);
      color: rgba(148, 163, 184, 0.9);
    }

    .icon-btn:hover {
      background: var(--bg-surface-hover);
      color: var(--text-primary);
      transform: translateY(-1.5px);
      box-shadow: 0 4px 10px rgba(0, 0, 0, 0.05);
    }

    :host-context(.dark) .icon-btn:hover {
      background: rgba(30, 41, 59, 0.7);
      color: #ffffff;
      box-shadow: 0 4px 10px rgba(0, 0, 0, 0.2);
    }

    /* Translucent Soft Color Status Pills */
    .status-pill {
      display: flex;
      align-items: center;
      gap: 6px;
      padding: 4px 12px;
      border-radius: 9999px;
      font-size: 10px;
      font-weight: 700;
      transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
      border: 1px solid transparent;
      user-select: none;
    }

    .status-pill:hover {
      transform: translateY(-0.5px);
      box-shadow: 0 2px 6px rgba(0, 0, 0, 0.02);
    }

    /* Custom Angular Material form-field height overrides inside header */
    .custom-branch-trigger {
      display: flex;
      align-items: center;
      gap: 4px;
      padding: 4px 8px;
      border-radius: 6px;
      transition: all 0.2s ease;
    }

    .custom-branch-trigger:hover {
      background: rgba(var(--accent-color-rgb), 0.08);
    }

    /* Scrollbar hide utility */
    .scrollbar-hide::-webkit-scrollbar {
      display: none;
    }
    .scrollbar-hide {
      -ms-overflow-style: none;
      scrollbar-width: none;
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
  
  // Connection and AI states signals
  connectionState = signal<'online' | 'offline' | 'demo'>('online');
  aiState = signal<'connected' | 'connecting' | 'offline'>('connected');

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

  changeBranch(branch: string): void {
    this.dataService.selectedBranch.set(branch);
  }

  toggleLanguage(): void {
    this.langService.toggleLanguage();
  }

  quickCreate(type: string): void {
    if (type === 'sales-invoice') this.router.navigate(['/pos']);
    else if (type === 'product') this.router.navigate(['/menu']);
    else if (type === 'customer') this.router.navigate(['/crm']);
    else if (type === 'journal-entry') this.router.navigate(['/finance']);
  }

  getBreadcrumbs(): { label: string; url?: string }[] {
    const url = this.activeUrl();
    const segments = url.split('/').filter(s => s);
    const list: { label: string; url?: string }[] = [];
    
    // Add home
    list.push({ label: this.t('nav.dashboard'), url: '/dashboard' });
    
    let path = '';
    for (const segment of segments) {
      path += '/' + segment;
      if (segment === 'dashboard') continue; // Avoid duplicate home
      
      const label = this.getRouteLabel(path);
      if (label !== path) {
        list.push({ label, url: path });
      }
    }
    return list;
  }

  getActiveRouteLabel(): string {
    return this.getRouteLabel(this.activeUrl());
  }

  getRouteLabel(path: string): string {
    const cleanPath = path.split('?')[0]; // Remove query params
    if (cleanPath === '/dashboard') return this.t('nav.dashboard');
    if (cleanPath === '/pos') return this.t('nav.pos');
    if (cleanPath === '/kitchen' || cleanPath === '/kds') return this.t('nav.kitchen');
    if (cleanPath === '/inventory') return this.t('nav.inventory');
    if (cleanPath === '/inventory/items') return this.t('nav.inventory.items') || 'تعريف الأصناف';
    if (cleanPath === '/menu') return this.t('nav.menu');
    if (cleanPath === '/hr') return this.t('nav.hr');
    if (cleanPath === '/reporting') return this.t('nav.reporting');
    if (cleanPath === '/finance') return this.t('nav.finance');
    if (cleanPath === '/crm') return this.t('nav.crm');
    if (cleanPath === '/branding') return this.t('nav.branding');
    if (cleanPath === '/media') return this.t('nav.media');
    if (cleanPath === '/settings') return this.t('nav.settings');
    if (cleanPath === '/workflow') return this.t('nav.workflow');
    
    // Default fallback check
    if (cleanPath.includes('dashboard')) return this.t('nav.dashboard');
    if (cleanPath.includes('pos')) return this.t('nav.pos');
    if (cleanPath.includes('inventory')) return this.t('nav.inventory');
    if (cleanPath.includes('menu')) return this.t('nav.menu');
    if (cleanPath.includes('hr')) return this.t('nav.hr');
    if (cleanPath.includes('finance')) return this.t('nav.finance');
    if (cleanPath.includes('crm')) return this.t('nav.crm');
    if (cleanPath.includes('branding')) return this.t('nav.branding');
    if (cleanPath.includes('media')) return this.t('nav.media');
    if (cleanPath.includes('settings')) return this.t('nav.settings');
    
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
