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
      import('./pages/purchase-orders.page').then(m => m.PurchaseOrdersPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Inventory.View' }
  },
  {
    path: 'purchase-orders/new',
    loadComponent: () =>
      import('./pages/purchase-order-form.page').then(m => m.PurchaseOrderFormPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Inventory.Manage' }
  },
  {
    path: 'purchase-orders/:id',
    loadComponent: () =>
      import('./pages/purchase-order-form.page').then(m => m.PurchaseOrderFormPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Inventory.View' }
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
      import('./pages/purchase-invoices.page').then(m => m.PurchaseInvoicesPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Inventory.View' }
  },
  {
    path: 'purchase-invoices/new',
    loadComponent: () =>
      import('./pages/purchase-invoice-form.page').then(m => m.PurchaseInvoiceFormPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Inventory.Manage' }
  },
  {
    path: 'purchase-invoices/:id',
    loadComponent: () =>
      import('./pages/purchase-invoice-form.page').then(m => m.PurchaseInvoiceFormPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Inventory.View' }
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
    path: 'invoice-returns',
    loadComponent: () =>
      import('./pages/purchase-returns.page').then(m => m.PurchaseReturnsPage),
    canActivate: [permissionGuard],
    data: {
      requiredPermission: 'Inventory.View',
      invoiceReturnsMode: true
    }
  },
  {
    path: 'invoice-returns/new',
    loadComponent: () =>
      import('./pages/purchase-return-form.page').then(m => m.PurchaseReturnFormPage),
    canActivate: [permissionGuard],
    data: {
      requiredPermission: 'Inventory.Manage',
      invoiceReturnsMode: true
    }
  },
  {
    path: 'invoice-returns/:id',
    loadComponent: () =>
      import('./pages/purchase-return-form.page').then(m => m.PurchaseReturnFormPage),
    canActivate: [permissionGuard],
    data: {
      requiredPermission: 'Inventory.View',
      invoiceReturnsMode: true
    }
  },
  // Legacy aliases → invoice returns (FromReceipt + Direct invoices)
  { path: 'direct-returns', pathMatch: 'full', redirectTo: 'invoice-returns' },
  { path: 'direct-returns/new', pathMatch: 'full', redirectTo: 'invoice-returns/new' },
  { path: 'direct-returns/:id', redirectTo: 'invoice-returns/:id' },
  {
    path: 'suppliers',
    loadComponent: () =>
      import('./pages/suppliers.page').then(m => m.SuppliersPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Supplier.View' }
  },
  {
    path: 'suppliers/new',
    loadComponent: () =>
      import('./pages/supplier-form.page').then(m => m.SupplierFormPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Supplier.Create' }
  },
  {
    path: 'suppliers/:id',
    loadComponent: () =>
      import('./pages/supplier-form.page').then(m => m.SupplierFormPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Supplier.View' }
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
