import { Routes } from '@angular/router';
import { permissionGuard } from '../../core/guards/permission.guard';

/**
 * Purchasing cycle (July 2026 redesign)
 * Path 1: PO → GRN → AP Invoice → Payment
 * Path 2: Direct Purchase Invoice
 */
export const PURCHASES_ROUTES: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./pages/purchasing-dashboard.page').then(m => m.PurchasingDashboardPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Inventory.View' }
  },
  {
    path: 'purchase-orders',
    loadComponent: () =>
      import('./pages/purchasing-module-placeholder.page').then(m => m.PurchasingModulePlaceholderPage),
    canActivate: [permissionGuard],
    data: {
      requiredPermission: 'Inventory.View',
      moduleKey: 'pur.nav.purchaseOrders',
      moduleCode: 'PurchaseOrders',
      noteKey: 'pur.note.po'
    }
  },
  {
    path: 'goods-receipts',
    loadComponent: () =>
      import('./pages/goods-receipts.page').then(m => m.GoodsReceiptsPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Inventory.View' }
  },
  {
    path: 'goods-receipts/new',
    loadComponent: () =>
      import('./pages/goods-receipt-form.page').then(m => m.GoodsReceiptFormPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Inventory.Manage' }
  },
  {
    path: 'goods-receipts/:id',
    loadComponent: () =>
      import('./pages/goods-receipt-form.page').then(m => m.GoodsReceiptFormPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Inventory.View' }
  },
  {
    path: 'purchase-returns',
    loadComponent: () =>
      import('./pages/purchase-returns.page').then(m => m.PurchaseReturnsPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Inventory.View' }
  },
  {
    path: 'purchase-returns/new',
    loadComponent: () =>
      import('./pages/purchase-return-form.page').then(m => m.PurchaseReturnFormPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Inventory.Manage' }
  },
  {
    path: 'purchase-returns/:id',
    loadComponent: () =>
      import('./pages/purchase-return-form.page').then(m => m.PurchaseReturnFormPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Inventory.View' }
  },
  {
    path: 'purchase-invoices',
    loadComponent: () =>
      import('./pages/purchasing-module-placeholder.page').then(m => m.PurchasingModulePlaceholderPage),
    canActivate: [permissionGuard],
    data: {
      requiredPermission: 'Inventory.View',
      moduleKey: 'pur.nav.purchaseInvoices',
      moduleCode: 'PurchaseInvoices',
      noteKey: 'pur.note.invoiceFromGrn'
    }
  },
  {
    path: 'direct-invoices',
    loadComponent: () =>
      import('./pages/direct-invoices.page').then(m => m.DirectInvoicesPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Inventory.View' }
  },
  {
    path: 'direct-invoices/new',
    loadComponent: () =>
      import('./pages/direct-invoice-form.page').then(m => m.DirectInvoiceFormPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Inventory.Manage' }
  },
  {
    path: 'direct-invoices/:id',
    loadComponent: () =>
      import('./pages/direct-invoice-form.page').then(m => m.DirectInvoiceFormPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Inventory.View' }
  },
  {
    path: 'direct-returns',
    loadComponent: () =>
      import('./pages/purchase-returns.page').then(m => m.PurchaseReturnsPage),
    canActivate: [permissionGuard],
    data: {
      requiredPermission: 'Inventory.View',
      returnTypeFilter: 3
    }
  },
  {
    path: 'direct-returns/new',
    loadComponent: () =>
      import('./pages/purchase-return-form.page').then(m => m.PurchaseReturnFormPage),
    canActivate: [permissionGuard],
    data: {
      requiredPermission: 'Inventory.Manage',
      defaultReturnType: 3
    }
  },
  {
    path: 'direct-returns/:id',
    loadComponent: () =>
      import('./pages/purchase-return-form.page').then(m => m.PurchaseReturnFormPage),
    canActivate: [permissionGuard],
    data: {
      requiredPermission: 'Inventory.View',
      defaultReturnType: 3
    }
  },
  {
    path: 'suppliers',
    loadComponent: () =>
      import('./pages/purchasing-module-placeholder.page').then(m => m.PurchasingModulePlaceholderPage),
    canActivate: [permissionGuard],
    data: {
      requiredPermission: 'Inventory.View',
      moduleKey: 'pur.nav.suppliers',
      moduleCode: 'Suppliers',
      noteKey: 'pur.note.suppliers'
    }
  },
  {
    path: 'triple-match',
    loadComponent: () =>
      import('./pages/purchasing-module-placeholder.page').then(m => m.PurchasingModulePlaceholderPage),
    canActivate: [permissionGuard],
    data: {
      requiredPermission: 'Inventory.View',
      moduleKey: 'pur.nav.tripleMatch',
      moduleCode: 'TripleMatch',
      noteKey: 'pur.note.tripleMatch'
    }
  }
];
