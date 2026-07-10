import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { permissionGuard } from './core/guards/permission.guard';

export const routes: Routes = [
  // Public marketing landing page
  {
    path: 'landing',
    loadComponent: () => import('./features/landing/landing.component').then(m => m.LandingComponent)
  },
  
  // Standalone split-screen login page
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent)
  },

  // Onboarding Setup Wizard
  {
    path: 'setup',
    loadComponent: () => import('./features/setup-wizard/setup-wizard.component').then(m => m.SetupWizardComponent)
  },

  // Full-screen kitchen display for cooks (tablet / wall monitor)
  {
    path: 'kitchen-display',
    canActivate: [authGuard],
    loadComponent: () => import('./features/kitchen/kitchen-display.component').then(m => m.KitchenDisplayComponent)
  },

  // Internal Portal Dashboards loaded within Main App Layout
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./shared/components/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'pos',
        loadComponent: () => import('./features/pos/pos.component').then(m => m.PosComponent)
      },
      {
        path: 'kds',
        loadComponent: () => import('./features/kitchen/kitchen.component').then(m => m.KitchenComponent)
      },
      {
        path: 'catalog',
        loadComponent: () => import('./features/catalog/catalog-list.component').then(m => m.CatalogListComponent)
      },
      {
        path: 'catalog/wizard',
        loadComponent: () => import('./features/catalog/catalog-wizard.component').then(m => m.CatalogWizardComponent)
      },
      {
        path: 'catalog/wizard/:id',
        loadComponent: () => import('./features/catalog/catalog-wizard.component').then(m => m.CatalogWizardComponent)
      },
      {
        path: 'inventory',
        loadComponent: () => import('./features/inventory/inventory.component').then(m => m.InventoryComponent)
      },
      {
        path: 'inventory/items/new',
        loadComponent: () => import('./features/inventory/inventory-item-form.component').then(m => m.InventoryItemFormComponent)
      },
      {
        path: 'inventory/items/:id',
        loadComponent: () => import('./features/inventory/inventory-item-form.component').then(m => m.InventoryItemFormComponent)
      },
      {
        path: 'menu',
        loadComponent: () => import('./features/menu/menu-operations.component').then(m => m.MenuOperationsComponent)
      },
      {
        path: 'hr',
        loadComponent: () => import('./features/employees/employees.component').then(m => m.EmployeesComponent),
        canActivate: [permissionGuard],
        data: { requiredPermission: 'VIEW_HR' }
      },
      {
        path: 'branding',
        loadComponent: () => import('./features/branding/branding.component').then(m => m.BrandingComponent)
      },
      {
        path: 'media',
        loadComponent: () => import('./features/media/media.component').then(m => m.MediaComponent)
      },
      {
        path: 'reporting',
        loadComponent: () => import('./features/reports/reports.component').then(m => m.ReportsComponent)
      },
      {
        path: 'finance',
        loadComponent: () => import('./features/finance/finance.component').then(m => m.FinanceComponent),
        canActivate: [permissionGuard],
        data: { requiredPermission: 'VIEW_FINANCE' }
      },
      {
        path: 'crm',
        loadComponent: () => import('./features/crm/crm.component').then(m => m.CrmComponent)
      },
      {
        path: 'settings',
        loadComponent: () => import('./features/settings/settings.component').then(m => m.SettingsComponent),
        canActivate: [permissionGuard],
        data: { requiredPermission: 'EDIT_SETTINGS' }
      },
      {
        path: 'workflow',
        loadComponent: () => import('./features/workflow/workflow.component').then(m => m.WorkflowComponent)
      },
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      }
    ]
  },

  // Error Pages
  {
    path: 'error',
    loadComponent: () => import('./features/errors/error-page/error-page.component').then(m => m.ErrorPageComponent)
  },
  {
    path: '404',
    loadComponent: () => import('./features/errors/not-found/not-found.component').then(m => m.NotFoundComponent)
  },

  {
    path: '**',
    redirectTo: '404'
  }
];
