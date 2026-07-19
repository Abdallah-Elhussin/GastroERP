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
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppSidebarComponent {
  langService = inject(LanguageService);
  dataService = inject(DataService);
  favoritesService = inject(InventoryFavoritesService);
  private router = inject(Router);

  isCollapsed = signal<boolean>(false);
  expandedGroups = signal<Record<string, boolean>>({
    'nav.inventory': true,
    'nav.purchases': true,
    'nav.sales': true,
    'nav.finance': true,
    'nav.financialOps': true
  });

  /**
   * Inventory sidebar — matches ERP warehouse menu structure.
   * Empty routes are scaffolded; detail screens filled per page specs later.
   */
  navItems: SidebarNavItem[] = [
    { path: '/dashboard', icon: 'dashboard', labelKey: 'nav.dashboard' },
    { path: '/pos', icon: 'grid_view', labelKey: 'nav.pos' },
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
    { path: '/kds', icon: 'flat_ware', labelKey: 'nav.kitchen' },
    { path: '/kitchen-display', icon: 'tv', labelKey: 'nav.kitchenDisplay' },
    {
      icon: 'local_shipping',
      labelKey: 'nav.purchases',
      children: [
        { path: '/purchases/dashboard', icon: 'analytics', labelKey: 'pur.nav.dashboard' },
        { path: '/purchases/purchase-orders', icon: 'shopping_cart', labelKey: 'pur.nav.purchaseOrders' },
        { path: '/purchases/goods-receipts', icon: 'inventory', labelKey: 'pur.nav.goodsReceipts' },
        { path: '/purchases/purchase-returns', icon: 'assignment_return', labelKey: 'pur.nav.purchaseReturns' },
        { path: '/purchases/purchase-invoices', icon: 'receipt_long', labelKey: 'pur.nav.purchaseInvoices' },
        { path: '/purchases/direct-invoices', icon: 'flash_on', labelKey: 'pur.nav.directInvoices' },
        { path: '/purchases/direct-returns', icon: 'undo', labelKey: 'pur.nav.directReturns' },
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
    { path: '/menu', icon: 'restaurant_menu', labelKey: 'nav.menu' },
    { path: '/hr', icon: 'badge', labelKey: 'nav.hr' },
    { path: '/workflow', icon: 'view_week', labelKey: 'nav.workflow' },
    { path: '/reporting', icon: 'bar_chart', labelKey: 'nav.reporting' },
    {
      icon: 'receipt_long',
      labelKey: 'nav.financialOps',
      children: [
        {
          path: '/finance-ops/opening-balances',
          icon: 'account_balance_wallet',
          labelKey: 'fin.ops.nav.openingBalances'
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
        }
      ]
    },
    {
      icon: 'account_balance',
      labelKey: 'nav.finance',
      children: [
        { path: '/finance', icon: 'dashboard', labelKey: 'fin.nav.dashboard' },
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
        { path: '/finance/users', icon: 'manage_accounts', labelKey: 'fin.nav.users' },
        { path: '/finance/fiscal-periods', icon: 'calendar_month', labelKey: 'fin.nav.fiscalPeriods' },
        { path: '/finance/settings', icon: 'tune', labelKey: 'fin.nav.settings' }
      ]
    },
    { path: '/crm', icon: 'people', labelKey: 'nav.crm' },
    { path: '/branding', icon: 'palette', labelKey: 'nav.branding' },
    { path: '/media', icon: 'photo_library', labelKey: 'nav.media' },
    { path: '/settings', icon: 'settings', labelKey: 'nav.settings' }
  ];

  favoriteLinks = computed(() => this.favoritesService.favorites());

  toggleCollapse(): void {
    this.isCollapsed.update(val => !val);
  }

  toggleGroup(key: string): void {
    this.expandedGroups.update(m => ({ ...m, [key]: !this.isGroupExpanded(key) }));
  }

  isGroupExpanded(key: string): boolean {
    return this.expandedGroups()[key] !== false;
  }

  isGroupActive(item: SidebarNavItem): boolean {
    const url = this.router.url ?? '';
    return !!item.children?.some(c => url === c.path || url.startsWith(c.path + '/'));
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
