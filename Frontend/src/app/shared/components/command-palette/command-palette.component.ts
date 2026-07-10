import { Component, ChangeDetectionStrategy, inject, signal, computed, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { DataService } from '../../../core/services/data.service';
import { LanguageService } from '../../../core/services/language.service';

export interface CommandItem {
  id: string;
  nameKey?: string;
  displayName?: string;
  categoryKey: string;
  icon: string;
  action: () => void;
}

@Component({
  selector: 'app-command-palette',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule
  ],
  templateUrl: './command-palette.component.html',
  styleUrl: './command-palette.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CommandPaletteComponent {
  router = inject(Router);
  dataService = inject(DataService);
  langService = inject(LanguageService);

  isOpen = signal<boolean>(false);
  searchQuery = signal<string>('');
  selectedIndex = signal<number>(0);

  // Global key listener for Ctrl+K
  @HostListener('window:keydown', ['$event'])
  handleKeyDown(event: KeyboardEvent): void {
    if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') {
      event.preventDefault();
      this.isOpen.update(val => !val);
      this.searchQuery.set('');
      this.selectedIndex.set(0);
    } else if (this.isOpen()) {
      if (event.key === 'Escape') {
        this.isOpen.set(false);
      } else if (event.key === 'ArrowDown') {
        event.preventDefault();
        const len = this.filteredItems().length;
        if (len > 0) {
          this.selectedIndex.update(idx => (idx + 1) % len);
        }
      } else if (event.key === 'ArrowUp') {
        event.preventDefault();
        const len = this.filteredItems().length;
        if (len > 0) {
          this.selectedIndex.update(idx => (idx - 1 + len) % len);
        }
      } else if (event.key === 'Enter') {
        event.preventDefault();
        const selected = this.filteredItems()[this.selectedIndex()];
        if (selected) {
          selected.action();
        }
      }
    }
  }

  onSearchInput(value: string): void {
    this.searchQuery.set(value);
    this.selectedIndex.set(0);
  }

  // Base list of searchable commands and items
  commandsList = computed<CommandItem[]>(() => {
    const list: CommandItem[] = [
      { id: 'nav-dashboard', nameKey: 'cmd.nav.dashboard', categoryKey: 'cmd.cat.navigation', icon: 'dashboard', action: () => this.navigate('/dashboard') },
      { id: 'nav-pos', nameKey: 'cmd.nav.pos', categoryKey: 'cmd.cat.navigation', icon: 'grid_view', action: () => this.navigate('/pos') },
      { id: 'nav-kds', nameKey: 'cmd.nav.kds', categoryKey: 'cmd.cat.navigation', icon: 'flat_ware', action: () => this.navigate('/kds') },
      { id: 'nav-inventory', nameKey: 'cmd.nav.inventory', categoryKey: 'cmd.cat.navigation', icon: 'inventory_2', action: () => this.navigate('/inventory') },
      { id: 'nav-menu', nameKey: 'cmd.nav.menu', categoryKey: 'cmd.cat.navigation', icon: 'restaurant_menu', action: () => this.navigate('/menu') },
      { id: 'nav-hr', nameKey: 'cmd.nav.hr', categoryKey: 'cmd.cat.navigation', icon: 'badge', action: () => this.navigate('/hr') },
      { id: 'nav-reports', nameKey: 'cmd.nav.reports', categoryKey: 'cmd.cat.navigation', icon: 'bar_chart', action: () => this.navigate('/reporting') },
      { id: 'nav-finance', nameKey: 'cmd.nav.finance', categoryKey: 'cmd.cat.navigation', icon: 'account_balance', action: () => this.navigate('/finance') },
      { id: 'nav-crm', nameKey: 'cmd.nav.crm', categoryKey: 'cmd.cat.navigation', icon: 'people', action: () => this.navigate('/crm') },
      { id: 'nav-settings', nameKey: 'cmd.nav.settings', categoryKey: 'cmd.cat.navigation', icon: 'settings', action: () => this.navigate('/settings') }
    ];

    this.dataService.products.forEach(p => {
      list.push({
        id: `prod-${p.id}`,
        displayName: `${this.t('cmd.product')}: ${p.name} ($${p.price.toFixed(2)})`,
        categoryKey: 'cmd.cat.products',
        icon: 'restaurant_menu',
        action: () => {
          this.dataService.addToCart(p);
          this.navigate('/pos');
        }
      });
    });

    this.dataService.kdsTickets().forEach(t => {
      list.push({
        id: `ticket-${t.id}`,
        displayName: `${this.t('cmd.activeOrder')} #${t.id} (${t.tableNo})`,
        categoryKey: 'cmd.cat.orders',
        icon: 'receipt_long',
        action: () => this.navigate('/kds')
      });
    });

    this.dataService.employees.forEach(e => {
      list.push({
        id: `emp-${e.id}`,
        displayName: `${this.t('cmd.employeeProfile')}: ${e.name} (${e.title})`,
        categoryKey: 'cmd.cat.employees',
        icon: 'account_box',
        action: () => this.navigate('/hr')
      });
    });

    return list;
  });

  // Filtered results
  filteredItems = computed(() => {
    const query = this.searchQuery().toLowerCase();
    if (!query) return this.commandsList().slice(0, 8);
    return this.commandsList().filter(item => {
      const name = item.nameKey ? this.t(item.nameKey) : (item.displayName ?? '');
      const category = this.t(item.categoryKey);
      return name.toLowerCase().includes(query) || category.toLowerCase().includes(query);
    });
  });

  getItemName(item: CommandItem): string {
    return item.nameKey ? this.t(item.nameKey) : (item.displayName ?? '');
  }

  navigate(path: string): void {
    this.router.navigate([path]);
    this.isOpen.set(false);
  }

  closePalette(): void {
    this.isOpen.set(false);
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
