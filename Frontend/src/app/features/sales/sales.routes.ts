import { Routes } from '@angular/router';
import { permissionGuard } from '../../core/guards/permission.guard';

export const SALES_ROUTES: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./pages/sales-dashboard.page').then(m => m.SalesDashboardPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Sales.View' }
  },
  {
    path: 'invoices',
    loadComponent: () =>
      import('./pages/sales-invoices.page').then(m => m.SalesInvoicesPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Sales.View' }
  },
  {
    path: 'invoices/new',
    loadComponent: () =>
      import('./pages/sales-invoice-form.page').then(m => m.SalesInvoiceFormPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Sales.View' }
  },
  {
    path: 'invoices/:id',
    loadComponent: () =>
      import('./pages/sales-invoice-form.page').then(m => m.SalesInvoiceFormPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Sales.View' }
  },
  {
    path: 'orders',
    loadComponent: () =>
      import('./pages/sales-orders.page').then(m => m.SalesOrdersPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Sales.View' }
  },
  {
    path: 'orders/new',
    loadComponent: () =>
      import('./pages/sales-order-form.page').then(m => m.SalesOrderFormPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Sales.View' }
  },
  {
    path: 'orders/:id',
    loadComponent: () =>
      import('./pages/sales-order-form.page').then(m => m.SalesOrderFormPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Sales.View' }
  },
  {
    path: 'quotations',
    loadComponent: () =>
      import('./pages/sales-quotations.page').then(m => m.SalesQuotationsPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Sales.View' }
  },
  {
    path: 'quotations/new',
    loadComponent: () =>
      import('./pages/sales-quotation-form.page').then(m => m.SalesQuotationFormPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Sales.View' }
  },
  {
    path: 'quotations/:id',
    loadComponent: () =>
      import('./pages/sales-quotation-form.page').then(m => m.SalesQuotationFormPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Sales.View' }
  },
  {
    path: 'delivery-notes',
    loadComponent: () =>
      import('./pages/sales-delivery-notes.page').then(m => m.SalesDeliveryNotesPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Sales.View' }
  },
  {
    path: 'delivery-notes/new',
    loadComponent: () =>
      import('./pages/sales-delivery-note-form.page').then(m => m.SalesDeliveryNoteFormPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Sales.View' }
  },
  {
    path: 'delivery-notes/:id',
    loadComponent: () =>
      import('./pages/sales-delivery-note-form.page').then(m => m.SalesDeliveryNoteFormPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Sales.View' }
  },
  {
    path: 'returns',
    loadComponent: () =>
      import('./pages/sales-returns.page').then(m => m.SalesReturnsPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Sales.View' }
  },
  {
    path: 'returns/new',
    loadComponent: () =>
      import('./pages/sales-return-form.page').then(m => m.SalesReturnFormPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Sales.View' }
  },
  {
    path: 'returns/:id',
    loadComponent: () =>
      import('./pages/sales-return-form.page').then(m => m.SalesReturnFormPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Sales.View' }
  },
  {
    path: 'debit-notes',
    loadComponent: () =>
      import('./pages/sales-debit-notes.page').then(m => m.SalesDebitNotesPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Sales.View' }
  },
  {
    path: 'debit-notes/new',
    loadComponent: () =>
      import('./pages/sales-debit-note-form.page').then(m => m.SalesDebitNoteFormPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Sales.View' }
  },
  {
    path: 'debit-notes/:id',
    loadComponent: () =>
      import('./pages/sales-debit-note-form.page').then(m => m.SalesDebitNoteFormPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Sales.View' }
  },
  {
    path: 'reports',
    loadComponent: () =>
      import('./pages/sales-reports.page').then(m => m.SalesReportsPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Sales.View' }
  },
  {
    path: 'customers',
    loadComponent: () =>
      import('./pages/sales-module-placeholder.page').then(m => m.SalesModulePlaceholderPage),
    canActivate: [permissionGuard],
    data: {
      requiredPermission: 'Sales.View',
      moduleKey: 'sal.nav.customers',
      noteKey: 'sal.note.customers'
    }
  }
];
