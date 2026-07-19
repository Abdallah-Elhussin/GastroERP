import { Routes } from '@angular/router';
import { permissionGuard } from '../../core/guards/permission.guard';

export const FINANCE_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./finance.component').then(m => m.FinanceComponent),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'VIEW_FINANCE' }
  },
  {
    path: 'chart-of-accounts',
    loadComponent: () =>
      import('./pages/chart-of-accounts.page').then(m => m.ChartOfAccountsPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Accounting.View' }
  },
  {
    path: 'account-classifications',
    loadComponent: () =>
      import('./pages/account-classifications.page').then(m => m.AccountClassificationsPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Accounting.Classifications.View' }
  },
  {
    path: 'cost-centers',
    loadComponent: () =>
      import('./pages/cost-centers.page').then(m => m.CostCentersPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'CostCenter.View' }
  },
  {
    path: 'currencies',
    loadComponent: () =>
      import('./pages/currencies.page').then(m => m.CurrenciesPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Currency.View' }
  },
  {
    path: 'exchange-rates',
    loadComponent: () =>
      import('./pages/exchange-rates.page').then(m => m.ExchangeRatesPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Currency.View' }
  },
  {
    path: 'document-types',
    loadComponent: () =>
      import('./pages/document-types.page').then(m => m.DocumentTypesPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'DocumentType.View' }
  },
  {
    path: 'banks',
    loadComponent: () => import('./pages/banks.page').then(m => m.BanksPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Bank.View' }
  },
  {
    path: 'cash-boxes',
    loadComponent: () => import('./pages/cash-boxes.page').then(m => m.CashBoxesPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'CashBox.View' }
  },
  {
    path: 'tax-registrations',
    loadComponent: () =>
      import('./pages/tax-registrations.page').then(m => m.TaxRegistrationsPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'TaxRegistration.View' }
  },
  {
    path: 'tax-codes',
    loadComponent: () => import('./pages/tax-codes.page').then(m => m.TaxCodesPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Settings.TaxCodes.View' }
  },
  {
    path: 'notification-reasons',
    loadComponent: () =>
      import('./pages/notification-reasons.page').then(m => m.NotificationReasonsPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Settings.NotificationReasons.View' }
  },
  {
    path: 'general-ledger-settings',
    loadComponent: () =>
      import('./pages/general-ledger-settings.page').then(m => m.GeneralLedgerSettingsPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Finance.GeneralLedgerSettings.View' }
  },
  {
    path: 'branches',
    loadComponent: () => import('./pages/branches.page').then(m => m.BranchesPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Settings.Branches.View' }
  },
  {
    path: 'users',
    loadComponent: () => import('./pages/users.page').then(m => m.UsersPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Settings.Users.View' }
  },
  {
    path: 'roles',
    loadComponent: () =>
      import('./pages/roles-permissions.page').then(m => m.RolesPermissionsPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Settings.Users.View' }
  },
  {
    path: 'user-permissions',
    loadComponent: () =>
      import('./pages/user-permissions.page').then(m => m.UserPermissionsPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Settings.Users.View' }
  },
  {
    path: 'fiscal-periods',
    loadComponent: () =>
      import('./pages/fiscal-periods.page').then(m => m.FiscalPeriodsPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'FiscalPeriod.View' }
  },
  {
    path: 'settings',
    loadComponent: () =>
      import('./pages/accounting-settings.page').then(m => m.AccountingSettingsPage),
    canActivate: [permissionGuard],
    data: { requiredPermission: 'Accounting.View' }
  }
];
