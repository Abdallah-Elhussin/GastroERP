import { Routes } from '@angular/router';
import { permissionGuard } from '../../core/guards/permission.guard';

/**
 * Phase 31.2 — Financial Operations (مستقل عن /finance للترميزات).
 * لا يحتوي على Dashboard — أول شاشة تشغيلية مباشرة.
 * المسار الجذري: /finance-ops
 */
export const FINANCE_OPS_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'opening-balances'
  },
  // bookmark قديم
  { path: 'dashboard', redirectTo: 'opening-balances', pathMatch: 'full' },
  {
    path: 'opening-balances',
    loadComponent: () =>
      import('../finance/pages/financial-opening-balances.page').then(
        m => m.FinancialOpeningBalancesPage
      ),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Finance.OpeningBalances.View' }
  },
  {
    path: 'journal-vouchers',
    loadComponent: () =>
      import('../finance/pages/journal-entries.page').then(m => m.JournalEntriesPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Journal.View' }
  },
  { path: 'journal-entries', redirectTo: 'journal-vouchers', pathMatch: 'full' },
  {
    path: 'receipt-vouchers',
    loadComponent: () =>
      import('./pages/receipt-vouchers.page').then(m => m.ReceiptVouchersPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Finance.ReceiptVouchers.View' }
  },
  {
    path: 'payment-vouchers',
    loadComponent: () =>
      import('./pages/payment-vouchers.page').then(m => m.PaymentVouchersPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Accounting.View' }
  },
  {
    path: 'debit-credit-notes',
    loadComponent: () =>
      import('./pages/debit-credit-notes.page').then(m => m.DebitCreditNotesPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Finance.FinancialNotes.View' }
  },
  {
    path: 'general-ledger',
    loadComponent: () =>
      import('./pages/general-ledger.page').then(m => m.GeneralLedgerPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Finance.GeneralLedger.View' }
  },
  { path: 'debit-notes', redirectTo: 'debit-credit-notes', pathMatch: 'full' },
  { path: 'credit-notes', redirectTo: 'debit-credit-notes', pathMatch: 'full' },
  {
    path: 'invoice-allocations',
    loadComponent: () =>
      import('./pages/finance-ops-module-placeholder.page').then(
        m => m.FinanceOpsModulePlaceholderPage
      ),
    canActivate: [permissionGuard],
    data: {
      requiredPermission: 'Accounting.View',
      moduleKey: 'fin.ops.nav.allocations',
      moduleCode: 'InvoiceAllocations'
    }
  },
  {
    path: 'posting-center',
    loadComponent: () =>
      import('./pages/finance-ops-module-placeholder.page').then(
        m => m.FinanceOpsModulePlaceholderPage
      ),
    canActivate: [permissionGuard],
    data: {
      requiredPermission: 'Journal.Post',
      moduleKey: 'fin.ops.nav.postingCenter',
      moduleCode: 'PostingCenter'
    }
  },
  {
    path: 'reverse-documents',
    loadComponent: () =>
      import('./pages/finance-ops-module-placeholder.page').then(
        m => m.FinanceOpsModulePlaceholderPage
      ),
    canActivate: [permissionGuard],
    data: {
      requiredPermission: 'Journal.Reverse',
      moduleKey: 'fin.ops.nav.reverseDocuments',
      moduleCode: 'ReverseDocuments'
    }
  },
  {
    path: 'cancel-unpost',
    loadComponent: () =>
      import('./pages/finance-ops-module-placeholder.page').then(
        m => m.FinanceOpsModulePlaceholderPage
      ),
    canActivate: [permissionGuard],
    data: {
      requiredPermission: 'Accounting.Update',
      moduleKey: 'fin.ops.nav.cancelUnpost',
      moduleCode: 'CancelUnpost'
    }
  },
  {
    path: 'bank-reconciliation',
    loadComponent: () =>
      import('./pages/finance-ops-module-placeholder.page').then(
        m => m.FinanceOpsModulePlaceholderPage
      ),
    canActivate: [permissionGuard],
    data: {
      requiredPermission: 'Accounting.View',
      moduleKey: 'fin.ops.nav.bankReconciliation',
      moduleCode: 'BankReconciliation'
    }
  },
  {
    path: 'reports',
    loadComponent: () =>
      import('./pages/finance-ops-module-placeholder.page').then(
        m => m.FinanceOpsModulePlaceholderPage
      ),
    canActivate: [permissionGuard],
    data: {
      requiredPermission: 'Accounting.View',
      moduleKey: 'fin.ops.nav.reports',
      moduleCode: 'FinancialReports'
    }
  },
  { path: 'posting', redirectTo: 'posting-center', pathMatch: 'full' },
  { path: 'reverse-journals', redirectTo: 'reverse-documents', pathMatch: 'full' }
];
