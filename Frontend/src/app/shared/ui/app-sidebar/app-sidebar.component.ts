import { Component, ChangeDetectionStrategy, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../../core/services/language.service';
import { DataService } from '../../../core/services/data.service';

interface SidebarNavChild {
  path: string;
  icon: string;
  labelKey: string;
}

interface SidebarNavItem {
  path?: string;
  icon: string;
  labelKey: string;
  children?: SidebarNavChild[];
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './app-sidebar.component.html',
  styles: [`
    :host {
      display: block;
      height: 100%;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppSidebarComponent {
  langService = inject(LanguageService);
  dataService = inject(DataService);

  isCollapsed = signal<boolean>(false);
  inventoryExpanded = signal<boolean>(true);

  navItems: SidebarNavItem[] = [
    { path: '/dashboard', icon: 'dashboard', labelKey: 'nav.dashboard' },
    { path: '/pos', icon: 'grid_view', labelKey: 'nav.pos' },
    { path: '/kds', icon: 'flat_ware', labelKey: 'nav.kitchen' },
    { path: '/kitchen-display', icon: 'tv', labelKey: 'nav.kitchenDisplay' },
    {
      icon: 'inventory_2',
      labelKey: 'nav.inventory',
      children: [
        { path: '/inventory', icon: 'list_alt', labelKey: 'nav.inventory.list' },
        { path: '/catalog', icon: 'category', labelKey: 'nav.catalog.engine' },
        { path: '/catalog/wizard', icon: 'edit_note', labelKey: 'nav.catalog.new' },
        { path: '/inventory/items/new', icon: 'post_add', labelKey: 'nav.inventory.items' }
      ]
    },
    { path: '/menu', icon: 'restaurant_menu', labelKey: 'nav.menu' },
    { path: '/hr', icon: 'badge', labelKey: 'nav.hr' },
    { path: '/workflow', icon: 'view_week', labelKey: 'nav.workflow' },
    { path: '/reporting', icon: 'bar_chart', labelKey: 'nav.reporting' },
    { path: '/finance', icon: 'account_balance', labelKey: 'nav.finance' },
    { path: '/crm', icon: 'people', labelKey: 'nav.crm' },
    { path: '/branding', icon: 'palette', labelKey: 'nav.branding' },
    { path: '/media', icon: 'photo_library', labelKey: 'nav.media' },
    { path: '/settings', icon: 'settings', labelKey: 'nav.settings' }
  ];

  inventoryActive = computed(() => {
    const url = typeof window !== 'undefined' ? window.location.pathname : '';
    return url.startsWith('/inventory') || url.startsWith('/catalog');
  });

  toggleCollapse(): void {
    this.isCollapsed.update(val => !val);
  }

  toggleInventory(): void {
    this.inventoryExpanded.update(v => !v);
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
