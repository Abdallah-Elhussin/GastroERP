import { Component, ChangeDetectionStrategy, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../../core/services/language.service';
import { DataService } from '../../../core/services/data.service';
import { InventoryFavoritesService } from '../../../features/inventory/shared/inventory-favorites.service';

interface SidebarNavChild {
  path: string;
  icon: string;
  labelKey: string;
  /** When set, renders a section header before this link (once per group). */
  sectionKey?: string;
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

    nav::-webkit-scrollbar {
      width: 4px;
    }
    nav::-webkit-scrollbar-track {
      background: transparent;
    }
    nav::-webkit-scrollbar-thumb {
      background: var(--border-color);
      border-radius: 999px;
    }

    .active-nav-item {
      background: #f1f5f9 !important;
      color: #0f172a !important;
      border-l-color: #f97316 !important;
    }

    :host-context(.dark) .active-nav-item {
      background: rgba(255, 255, 255, 0.08) !important;
      color: #ffffff !important;
      border-l-color: #f97316 !important;
    }

    .active-nav-group {
      color: #0f172a !important;
      font-weight: 750 !important;
    }

    :host-context(.dark) .active-nav-group {
      color: #ffffff !important;
      font-weight: 750 !important;
    }

    .active-sub-item {
      background: #f1f5f9 !important;
      color: #0f172a !important;
      font-weight: 850 !important;
    }

    :host-context(.dark) .active-sub-item {
      background: rgba(255, 255, 255, 0.08) !important;
      color: #ffffff !important;
      font-weight: 850 !important;
    }

    .submenu-container {
      margin-inline-end: 32px;
      padding-inline-end: 8px;
      border-inline-end: 1.5px solid #e2e8f0;
    }

    :host-context(.dark) .submenu-container {
      border-inline-end-color: #334155;
    }

    .nav-section-label {
      padding: 0.5rem 1rem 0.25rem;
      font-size: 0.65rem;
      font-weight: 800;
      letter-spacing: 0.04em;
      text-transform: uppercase;
      color: #94a3b8;
    }

    .collapsed-active-item {
      background: #f1f5f9 !important;
      color: #f97316 !important;
    }

    :host-context(.dark) .collapsed-active-item {
      background: rgba(255, 255, 255, 0.08) !important;
      color: #f97316 !important;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppSidebarComponent {
  langService = inject(LanguageService);
  dataService = inject(DataService);
  favoritesService = inject(InventoryFavoritesService);
  private router = inject(Router);

  isCollapsed = signal<boolean>(false);
  /** Groups start collapsed; open only after the user clicks. */
  expandedGroups = signal<Record<string, boolean>>({});

  /**
   * Main ERP navigation — order and labels follow the main-nav reorganization plan.
   * Child screens/routes are preserved; branding/media/users/roles live under Settings.
   */
  navItems: SidebarNavItem[] = [
    { path: '/dashboard', icon: 'space_dashboard', labelKey: 'nav.dashboard' },
    { path: '/pos', icon: 'point_of_sale', labelKey: 'nav.pos' },
    {
      icon: 'storefront',
      labelKey: 'nav.sales',
      children: [
        { path: '/sales/dashboard', icon: 'analytics', labelKey: 'sal.nav.dashboard' },
        { path: '/sales/invoices', icon: 'receipt_long', labelKey: 'bos.nav.invoices' },
        { path: '/sales/quotations', icon: 'request_quote', labelKey: 'bos.nav.quotations' },
        { path: '/sales/orders', icon: 'shopping_bag', labelKey: 'bos.nav.orders' },
        { path: '/sales/delivery-notes', icon: 'local_shipping', labelKey: 'bos.nav.deliveryNotes' },
        { path: '/sales/returns', icon: 'assignment_return', labelKey: 'bos.nav.returns' },
        { path: '/sales/debit-notes', icon: 'sticky_note_2', labelKey: 'bos.nav.debitNotes' },
        { path: '/sales/reports', icon: 'bar_chart', labelKey: 'bos.nav.reports' },
        { path: '/crm', icon: 'people', labelKey: 'sal.nav.customers' }
      ]
    },
    {
      icon: 'local_shipping',
      labelKey: 'nav.purchases',
      children: [
        { path: '/purchases/dashboard', icon: 'analytics', labelKey: 'pur.nav.dashboard' },
        { path: '/purchases/purchase-orders', icon: 'shopping_cart', labelKey: 'pur.nav.purchaseOrders' },
        { path: '/purchases/goods-receipts', icon: 'inventory', labelKey: 'pur.nav.goodsReceipts' },
        { path: '/purchases/purchase-invoices', icon: 'receipt_long', labelKey: 'pur.nav.purchaseInvoices' },
        { path: '/purchases/direct-invoices', icon: 'flash_on', labelKey: 'pur.nav.directInvoices' },
        { path: '/purchases/invoice-returns', icon: 'undo', labelKey: 'pur.nav.invoiceReturns' },
        { path: '/purchases/suppliers', icon: 'storefront', labelKey: 'pur.nav.suppliers' },
        { path: '/purchases/triple-match', icon: 'compare_arrows', labelKey: 'pur.nav.tripleMatch' }
      ]
    },
    {
      icon: 'inventory_2',
      labelKey: 'nav.inventory',
      children: [
        { path: '/inventory/dashboard', icon: 'analytics', labelKey: 'inv.nav.dashboard' },
        { path: '/inventory/items', icon: 'inventory', labelKey: 'inv.nav.items' },
        { path: '/inventory/item-types', icon: 'category', labelKey: 'inv.nav.itemTypes' },
        { path: '/inventory/categories', icon: 'folder', labelKey: 'inv.nav.categories' },
        { path: '/inventory/units', icon: 'straighten', labelKey: 'inv.nav.units' },
        { path: '/inventory/warehouses', icon: 'warehouse', labelKey: 'inv.nav.warehouses' },
        { path: '/inventory/valuation', icon: 'payments', labelKey: 'inv.nav.valuation' },
        { path: '/inventory/settings', icon: 'tune', labelKey: 'inv.nav.settings' },
        { path: '/inventory/prices', icon: 'sell', labelKey: 'inv.nav.prices' },
        { path: '/inventory/inquiry', icon: 'search', labelKey: 'inv.nav.inquiry' },
        { path: '/inventory/opening-balances', icon: 'account_balance_wallet', labelKey: 'inv.nav.openingBalance' },
        { path: '/inventory/goods-issues', icon: 'outbox', labelKey: 'inv.nav.goodsIssue' },
        { path: '/inventory/stock-transfers', icon: 'swap_horiz', labelKey: 'inv.nav.stockTransfer' },
        { path: '/inventory/issue-destinations', icon: 'place', labelKey: 'inv.nav.issueDestinations' },
        { path: '/inventory/transactions', icon: 'receipt_long', labelKey: 'inv.nav.transactions' }
      ]
    },
    {
      icon: 'settings_suggest',
      labelKey: 'nav.operations',
      children: [
        { path: '/workflow', icon: 'view_week', labelKey: 'nav.workflow' },
        { path: '/kds', icon: 'soup_kitchen', labelKey: 'nav.kitchen' },
        { path: '/kitchen-display', icon: 'tv', labelKey: 'nav.kitchenDisplay' }
      ]
    },
    {
      icon: 'account_balance',
      labelKey: 'nav.finance',
      children: [
        {
          path: '/finance-ops/opening-balances',
          icon: 'account_balance_wallet',
          labelKey: 'fin.ops.nav.openingBalances',
          sectionKey: 'nav.finance.section.operations'
        },
        {
          path: '/finance-ops/journal-vouchers',
          icon: 'menu_book',
          labelKey: 'fin.ops.nav.journalVouchers'
        },
        {
          path: '/finance-ops/receipt-vouchers',
          icon: 'call_received',
          labelKey: 'fin.ops.nav.receiptVouchers'
        },
        {
          path: '/finance-ops/payment-vouchers',
          icon: 'call_made',
          labelKey: 'fin.ops.nav.paymentVouchers'
        },
        {
          path: '/finance-ops/debit-credit-notes',
          icon: 'sticky_note_2',
          labelKey: 'fin.ops.nav.debitCreditNotes'
        },
        {
          path: '/finance-ops/general-ledger',
          icon: 'library_books',
          labelKey: 'fin.ops.nav.generalLedger'
        },
        {
          path: '/finance-ops/invoice-allocations',
          icon: 'link',
          labelKey: 'fin.ops.nav.allocations'
        },
        {
          path: '/finance-ops/posting-center',
          icon: 'publish',
          labelKey: 'fin.ops.nav.postingCenter'
        },
        {
          path: '/finance-ops/reverse-documents',
          icon: 'undo',
          labelKey: 'fin.ops.nav.reverseDocuments'
        },
        {
          path: '/finance-ops/cancel-unpost',
          icon: 'cancel',
          labelKey: 'fin.ops.nav.cancelUnpost'
        },
        {
          path: '/finance-ops/bank-reconciliation',
          icon: 'account_balance',
          labelKey: 'fin.ops.nav.bankReconciliation'
        },
        {
          path: '/finance-ops/reports',
          icon: 'assessment',
          labelKey: 'fin.ops.nav.reports'
        },
        {
          path: '/finance',
          icon: 'dashboard',
          labelKey: 'fin.nav.dashboard',
          sectionKey: 'nav.finance.section.coding'
        },
        { path: '/finance/chart-of-accounts', icon: 'account_tree', labelKey: 'fin.nav.coa' },
        { path: '/finance/account-classifications', icon: 'category', labelKey: 'fin.nav.classifications' },
        { path: '/finance/cost-centers', icon: 'hub', labelKey: 'fin.nav.costCenters' },
        { path: '/finance/currencies', icon: 'payments', labelKey: 'fin.nav.currencies' },
        { path: '/finance/exchange-rates', icon: 'currency_exchange', labelKey: 'fin.nav.exchangeRates' },
        { path: '/finance/document-types', icon: 'description', labelKey: 'fin.nav.documentTypes' },
        { path: '/finance/banks', icon: 'account_balance', labelKey: 'fin.nav.banks' },
        { path: '/finance/cash-boxes', icon: 'point_of_sale', labelKey: 'fin.nav.cashBoxes' },
        { path: '/finance/tax-registrations', icon: 'receipt_long', labelKey: 'fin.nav.taxRegistrations' },
        { path: '/finance/tax-codes', icon: 'percent', labelKey: 'fin.nav.taxCodes' },
        {
          path: '/finance/notification-reasons',
          icon: 'fact_check',
          labelKey: 'fin.nav.notificationReasons'
        },
        { path: '/finance/general-ledger-settings', icon: 'settings_applications', labelKey: 'fin.nav.glSettings' },
        { path: '/finance/branches', icon: 'store', labelKey: 'fin.nav.branches' },
        { path: '/finance/fiscal-periods', icon: 'calendar_month', labelKey: 'fin.nav.fiscalPeriods' },
        { path: '/finance/settings', icon: 'tune', labelKey: 'fin.nav.settings' }
      ]
    },
    { path: '/hr', icon: 'groups', labelKey: 'nav.hr' },
    { path: '/reporting', icon: 'assessment', labelKey: 'nav.reporting' },
    { path: '/crm', icon: 'handshake', labelKey: 'nav.crm' },
    { path: '/branding', icon: 'campaign', labelKey: 'nav.marketing' },
    { path: '/menu', icon: 'restaurant_menu', labelKey: 'nav.menu' },
    {
      icon: 'settings',
      labelKey: 'nav.settings',
      children: [
        { path: '/settings', icon: 'tune', labelKey: 'nav.settings.system' },
        { path: '/branding', icon: 'palette', labelKey: 'nav.branding' },
        { path: '/media', icon: 'photo_library', labelKey: 'nav.media' },
        { path: '/settings', icon: 'print', labelKey: 'nav.settings.printing' },
        { path: '/settings', icon: 'smart_toy', labelKey: 'nav.settings.ai' },
        { path: '/settings', icon: 'verified', labelKey: 'nav.settings.zatca' },
        { path: '/finance/roles', icon: 'admin_panel_settings', labelKey: 'nav.settings.permissions' },
        { path: '/finance/user-permissions', icon: 'verified_user', labelKey: 'auth.tab.userPermissions' },
        { path: '/finance/users', icon: 'manage_accounts', labelKey: 'nav.settings.users' },
        { path: '/settings', icon: 'workspace_premium', labelKey: 'nav.settings.licenses' },
        { path: '/settings', icon: 'devices', labelKey: 'nav.settings.devices' },
        { path: '/settings', icon: 'settings_applications', labelKey: 'nav.settings.general' }
      ]
    }
  ];

  favoriteLinks = computed(() => this.favoritesService.favorites());

  toggleCollapse(): void {
    this.isCollapsed.update(val => !val);
  }

  toggleGroup(key: string): void {
    this.expandedGroups.update(m => ({ ...m, [key]: !this.isGroupExpanded(key) }));
  }

  isGroupExpanded(key: string): boolean {
    return this.expandedGroups()[key] === true;
  }

  isGroupActive(item: SidebarNavItem): boolean {
    const url = this.router.url ?? '';
    return !!item.children?.some(c => url === c.path || url.startsWith(c.path + '/'));
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
