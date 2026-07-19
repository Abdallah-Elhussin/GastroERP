import { Routes } from '@angular/router';
import { permissionGuard } from '../../core/guards/permission.guard';

/**
 * Inventory feature routes (lazy-loaded).
 * Architecture: InventoryItem → Recipe → Product (via Catalog coordinator).
 */
export const INVENTORY_ROUTES: Routes = [
  {
    path: '',
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Inventory.View' },
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'dashboard'
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./pages/inventory-dashboard.page').then(m => m.InventoryDashboardPage),
        data: {
          breadcrumb: ['nav.inventory', 'inv.nav.dashboard'],
          requiredPermission: 'Inventory.View'
        }
      },
      {
        path: 'items',
        loadComponent: () =>
          import('./inventory.component').then(m => m.InventoryComponent),
        data: {
          breadcrumb: ['nav.inventory', 'inv.nav.items'],
          requiredPermission: 'Inventory.View'
        }
      },
      {
        path: 'items/new',
        loadComponent: () =>
          import('./pages/inventory-item-quick-setup.page').then(m => m.InventoryItemQuickSetupPage),
        canActivate: [permissionGuard],
        data: {
          breadcrumb: ['nav.inventory', 'inv.nav.items', 'inv.nav.newItem'],
          requiredPermission: 'Inventory.Manage'
        }
      },
      {
        path: 'items/:id',
        loadComponent: () =>
          import('./inventory-item-form.component').then(m => m.InventoryItemFormComponent),
        canActivate: [permissionGuard],
        data: {
          breadcrumb: ['nav.inventory', 'inv.nav.items', 'inv.nav.editItem'],
          requiredPermission: 'Inventory.Manage'
        }
      },
      {
        path: 'items/:id/details',
        loadComponent: () =>
          import('./pages/inventory-product-details.page').then(m => m.InventoryProductDetailsPage),
        data: {
          breadcrumb: ['nav.inventory', 'inv.nav.items', 'inv.nav.details'],
          requiredPermission: 'Inventory.View'
        }
      },
      {
        path: 'products',
        pathMatch: 'full',
        redirectTo: '/catalog/master'
      },
      {
        path: 'categories',
        loadComponent: () =>
          import('./pages/inventory-categories.page').then(m => m.InventoryCategoriesPage),
        canActivate: [permissionGuard],
        data: {
          breadcrumb: ['nav.inventory', 'inv.nav.categories'],
          requiredPermission: 'Inventory.View'
        }
      },
      {
        path: 'units',
        loadComponent: () =>
          import('./pages/inventory-units.page').then(m => m.InventoryUnitsPage),
        canActivate: [permissionGuard],
        data: {
          breadcrumb: ['nav.inventory', 'inv.nav.units'],
          requiredPermission: 'Inventory.View'
        }
      },
      {
        path: 'warehouses',
        loadComponent: () =>
          import('./pages/inventory-warehouses.page').then(m => m.InventoryWarehousesPage),
        canActivate: [permissionGuard],
        data: {
          breadcrumb: ['nav.inventory', 'inv.nav.warehouses'],
          requiredPermission: 'Inventory.Warehouses.View'
        }
      },
      {
        path: 'extensions',
        loadComponent: () =>
          import('./pages/inventory-extensions.page').then(m => m.InventoryExtensionsPage),
        canActivate: [permissionGuard],
        data: {
          breadcrumb: ['nav.inventory', 'inv.nav.extensions'],
          requiredPermission: 'Inventory.Manage'
        }
      },
      {
        path: 'transactions',
        loadComponent: () =>
          import('./pages/inventory-operations.page').then(m => m.InventoryOperationsPage),
        canActivate: [permissionGuard],
        data: {
          breadcrumb: ['nav.inventory', 'inv.nav.transactions'],
          requiredPermission: 'Stock.View'
        }
      },
      {
        path: 'opening-balances',
        loadComponent: () =>
          import('./pages/inventory-opening-balance.page').then(m => m.InventoryOpeningBalancePage),
        canActivate: [permissionGuard],
        data: {
          breadcrumb: ['nav.inventory', 'inv.nav.openingBalance'],
          titleKey: 'inv.nav.openingBalance',
          requiredPermission: 'Inventory.View'
        }
      },
      {
        path: 'goods-issues',
        loadComponent: () =>
          import('./pages/inventory-goods-issues.page').then(m => m.InventoryGoodsIssuesPage),
        canActivate: [permissionGuard],
        data: {
          breadcrumb: ['nav.inventory', 'inv.nav.goodsIssue'],
          titleKey: 'inv.nav.goodsIssue',
          requiredPermission: 'Inventory.View'
        }
      },
      {
        path: 'goods-issues/new',
        loadComponent: () =>
          import('./pages/inventory-goods-issue-form.page').then(m => m.InventoryGoodsIssueFormPage),
        canActivate: [permissionGuard],
        data: {
          breadcrumb: ['nav.inventory', 'inv.nav.goodsIssue', 'inv.gi.createTitle'],
          titleKey: 'inv.gi.createTitle',
          requiredPermission: 'Inventory.Manage'
        }
      },
      {
        path: 'goods-issues/:id',
        loadComponent: () =>
          import('./pages/inventory-goods-issue-form.page').then(m => m.InventoryGoodsIssueFormPage),
        canActivate: [permissionGuard],
        data: {
          breadcrumb: ['nav.inventory', 'inv.nav.goodsIssue', 'inv.gi.editTitle'],
          titleKey: 'inv.gi.editTitle',
          requiredPermission: 'Inventory.View'
        }
      },
      {
        path: 'issue-destinations',
        loadComponent: () =>
          import('./pages/inventory-issue-destinations.page').then(m => m.InventoryIssueDestinationsPage),
        canActivate: [permissionGuard],
        data: {
          breadcrumb: ['nav.inventory', 'inv.nav.issueDestinations'],
          titleKey: 'inv.nav.issueDestinations',
          requiredPermission: 'Inventory.IssueDestinations.View'
        }
      },
      {
        path: 'stock-transfers',
        loadComponent: () =>
          import('./pages/inventory-stock-transfers.page').then(m => m.InventoryStockTransfersPage),
        canActivate: [permissionGuard],
        data: {
          breadcrumb: ['nav.inventory', 'inv.nav.stockTransfer'],
          titleKey: 'inv.nav.stockTransfer',
          requiredPermission: 'Stock.View'
        }
      },
      {
        path: 'stock-transfers/new',
        loadComponent: () =>
          import('./pages/inventory-stock-transfer-form.page').then(m => m.InventoryStockTransferFormPage),
        canActivate: [permissionGuard],
        data: {
          breadcrumb: ['nav.inventory', 'inv.nav.stockTransfer', 'inv.st.createTitle'],
          titleKey: 'inv.st.createTitle',
          requiredPermission: 'Stock.Transfer'
        }
      },
      {
        path: 'stock-transfers/:id',
        loadComponent: () =>
          import('./pages/inventory-stock-transfer-form.page').then(m => m.InventoryStockTransferFormPage),
        canActivate: [permissionGuard],
        data: {
          breadcrumb: ['nav.inventory', 'inv.nav.stockTransfer'],
          titleKey: 'inv.nav.stockTransfer',
          requiredPermission: 'Stock.View'
        }
      },
      {
        path: 'reports',
        loadComponent: () =>
          import('./pages/inventory-reports.page').then(m => m.InventoryReportsPage),
        canActivate: [permissionGuard],
        data: {
          breadcrumb: ['nav.inventory', 'inv.nav.reports'],
          requiredPermission: 'InventoryReports.View'
        }
      },
      {
        path: 'settings',
        loadComponent: () =>
          import('./pages/inventory-settings.page').then(m => m.InventorySettingsPage),
        canActivate: [permissionGuard],
        data: {
          breadcrumb: ['nav.inventory', 'inv.nav.settings'],
          requiredPermission: 'Inventory.Settings.View',
          titleKey: 'inv.nav.settings'
        }
      },
      {
        path: 'item-types',
        loadComponent: () =>
          import('./pages/inventory-item-types.page').then(m => m.InventoryItemTypesPage),
        canActivate: [permissionGuard],
        data: {
          breadcrumb: ['nav.inventory', 'inv.nav.itemTypes'],
          titleKey: 'inv.nav.itemTypes',
          requiredPermission: 'Inventory.ItemTypes.View'
        }
      },
      {
        path: 'valuation',
        loadComponent: () =>
          import('./pages/inventory-valuation-groups.page').then(m => m.InventoryValuationGroupsPage),
        canActivate: [permissionGuard],
        data: {
          breadcrumb: ['nav.inventory', 'inv.nav.valuation'],
          titleKey: 'inv.nav.valuation',
          requiredPermission: 'Inventory.ValuationGroups.View'
        }
      },
      {
        path: 'prices',
        loadComponent: () =>
          import('./pages/inventory-product-pricing.page').then(m => m.InventoryProductPricingPage),
        canActivate: [permissionGuard],
        data: {
          breadcrumb: ['nav.inventory', 'inv.nav.prices'],
          titleKey: 'inv.nav.prices',
          requiredPermission: 'Sales.ProductPricing.View'
        }
      },
      {
        path: 'inquiry',
        loadComponent: () =>
          import('./pages/inventory-product-inquiry.page').then(m => m.InventoryProductInquiryPage),
        canActivate: [permissionGuard],
        data: {
          breadcrumb: ['nav.inventory', 'inv.nav.inquiry'],
          titleKey: 'inv.nav.inquiry',
          requiredPermission: 'Inventory.ProductInquiry.View'
        }
      }
    ]
  }
];
