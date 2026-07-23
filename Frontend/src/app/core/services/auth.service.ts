import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, of, merge, fromEvent, tap, catchError, map, switchMap } from 'rxjs';
import { environment } from '../../../environments/environment';

interface AuthResponse {
  token: string;
  refreshToken: string;
  expiresIn: number;
}

interface CurrentUserProfile {
  id: string;
  email: string;
  fullName: string;
  tenantId: string;
  roles: string[];
  permissions: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'gastro_auth_token';
  private readonly REFRESH_TOKEN_KEY = 'gastro_refresh_token';
  private readonly API = `${environment.apiBaseUrl}/auth`;
  private router = inject(Router);
  private http = inject(HttpClient);

  isAuthenticated = signal<boolean>(false);
  userPermissions = signal<string[]>([]);
  userRoles = signal<string[]>([]);
  currentUser = signal<CurrentUserProfile | null>(null);

  private idleTimer: any;
  private readonly TIMEOUT_MS = 30 * 60 * 1000; // 30 minutes idle timeout

  constructor() {
    const token = this.getToken();
    if (token) {
      this.isAuthenticated.set(true);
      this.loadCurrentUser().subscribe();
      this.startIdleTracking();
    }
  }

  hasPermission(required: string): boolean {
    if (this.userRoles().includes('Administrator')) {
      return true;
    }

    const permissions = this.userPermissions();
    if (permissions.includes('ALL')) {
      return true;
    }

    const aliases: Record<string, string[]> = {
      VIEW_HR: ['VIEW_HR', 'Hr.Employee.View', 'Hr.Dashboard.View'],
      'Dashboard.View': [
        'Dashboard.View',
        'Reports.View',
        'Sales.Dashboard.View',
        'Sales.View',
        'Inventory.View',
        'Finance.View',
        'Accounting.View'
      ],
      VIEW_FINANCE: ['VIEW_FINANCE', 'Finance.View', 'Finance.Account.View', 'Accounting.View'],
      'Accounting.View': ['Accounting.View', 'VIEW_FINANCE', 'Finance.View', 'Finance.Account.View'],
      'Accounting.Create': ['Accounting.Create', 'Accounting.Update'],
      'Accounting.Update': ['Accounting.Update'],
      'Accounting.Delete': ['Accounting.Delete'],
      'Accounting.Classifications.View': [
        'Accounting.Classifications.View',
        'Accounting.View',
        'VIEW_FINANCE',
        'Finance.View'
      ],
      'Accounting.Classifications.Create': [
        'Accounting.Classifications.Create',
        'Accounting.Create',
        'Accounting.Update'
      ],
      'Accounting.Classifications.Update': [
        'Accounting.Classifications.Update',
        'Accounting.Update'
      ],
      'Accounting.Classifications.Delete': [
        'Accounting.Classifications.Delete',
        'Accounting.Delete'
      ],
      'CostCenter.View': ['CostCenter.View', 'Accounting.View', 'VIEW_FINANCE', 'Finance.View'],
      'CostCenter.Create': ['CostCenter.Create', 'Accounting.Create', 'Accounting.Update'],
      'CostCenter.Update': ['CostCenter.Update', 'Accounting.Update'],
      'CostCenter.Delete': ['CostCenter.Delete', 'Accounting.Delete'],
      'CostCenter.Export': ['CostCenter.Export', 'CostCenter.View', 'Accounting.View'],
      'CostCenter.Activate': ['CostCenter.Activate', 'CostCenter.Update', 'Accounting.Update'],
      'Currency.View': ['Currency.View', 'Accounting.View', 'VIEW_FINANCE', 'Finance.View'],
      'Currency.Create': ['Currency.Create', 'Accounting.Create', 'Accounting.Update'],
      'Currency.Update': ['Currency.Update', 'Accounting.Update'],
      'Currency.Delete': ['Currency.Delete', 'Accounting.Delete'],
      'Currency.Export': ['Currency.Export', 'Currency.View', 'Accounting.View'],
      'Currency.Activate': ['Currency.Activate', 'Currency.Update', 'Accounting.Update'],
      'Currency.SetCompany': ['Currency.SetCompany', 'Accounting.Update'],
      'Currency.ManageRates': ['Currency.ManageRates', 'Currency.Update', 'Accounting.Update'],
      'DocumentType.View': ['DocumentType.View', 'Accounting.View', 'VIEW_FINANCE', 'Finance.View'],
      'DocumentType.Create': ['DocumentType.Create', 'Accounting.Create', 'Accounting.Update'],
      'DocumentType.Update': ['DocumentType.Update', 'Accounting.Update'],
      'DocumentType.Delete': ['DocumentType.Delete', 'Accounting.Delete'],
      'DocumentType.Activate': ['DocumentType.Activate', 'DocumentType.Update', 'Accounting.Update'],
      'DocumentType.Export': ['DocumentType.Export', 'DocumentType.View', 'Accounting.View'],
      'Bank.View': ['Bank.View', 'Accounting.View', 'VIEW_FINANCE', 'Finance.View'],
      'Bank.Create': ['Bank.Create', 'Accounting.Create', 'Accounting.Update'],
      'Bank.Update': ['Bank.Update', 'Accounting.Update'],
      'Bank.Delete': ['Bank.Delete', 'Accounting.Delete'],
      'Bank.Activate': ['Bank.Activate', 'Bank.Update', 'Accounting.Update'],
      'Bank.Export': ['Bank.Export', 'Bank.View', 'Accounting.View'],
      'CashBox.View': ['CashBox.View', 'Accounting.View', 'VIEW_FINANCE', 'Finance.View'],
      'CashBox.Create': ['CashBox.Create', 'Accounting.Create', 'Accounting.Update'],
      'CashBox.Update': ['CashBox.Update', 'Accounting.Update'],
      'CashBox.Delete': ['CashBox.Delete', 'Accounting.Delete'],
      'CashBox.Activate': ['CashBox.Activate', 'CashBox.Update', 'Accounting.Update'],
      'CashBox.Export': ['CashBox.Export', 'CashBox.View', 'Accounting.View'],
      'TaxRegistration.View': [
        'TaxRegistration.View',
        'Accounting.View',
        'VIEW_FINANCE',
        'Finance.View'
      ],
      'TaxRegistration.Create': ['TaxRegistration.Create', 'Accounting.Create', 'Accounting.Update'],
      'TaxRegistration.Update': ['TaxRegistration.Update', 'Accounting.Update'],
      'TaxRegistration.Delete': ['TaxRegistration.Delete', 'Accounting.Delete'],
      'TaxRegistration.UploadCertificate': [
        'TaxRegistration.UploadCertificate',
        'TaxRegistration.Update',
        'Accounting.Update'
      ],
      'TaxRegistration.DownloadCertificate': [
        'TaxRegistration.DownloadCertificate',
        'TaxRegistration.View',
        'Accounting.View'
      ],
      'TaxRegistration.Print': ['TaxRegistration.Print', 'TaxRegistration.View', 'Accounting.View'],
      'TaxRegistration.Export': ['TaxRegistration.Export', 'TaxRegistration.View', 'Accounting.View'],
      'Finance.GeneralLedgerSettings.View': [
        'Finance.GeneralLedgerSettings.View',
        'Accounting.View',
        'VIEW_FINANCE',
        'Finance.View'
      ],
      'Finance.GeneralLedgerSettings.Create': [
        'Finance.GeneralLedgerSettings.Create',
        'Accounting.Create',
        'Accounting.Update'
      ],
      'Finance.GeneralLedgerSettings.Edit': [
        'Finance.GeneralLedgerSettings.Edit',
        'Accounting.Update'
      ],
      'Finance.GeneralLedgerSettings.Delete': [
        'Finance.GeneralLedgerSettings.Delete',
        'Accounting.Delete'
      ],
      'Finance.GeneralLedgerSettings.Export': [
        'Finance.GeneralLedgerSettings.Export',
        'Finance.GeneralLedgerSettings.View',
        'Accounting.View'
      ],
      'Finance.GeneralLedgerSettings.Print': [
        'Finance.GeneralLedgerSettings.Print',
        'Finance.GeneralLedgerSettings.View',
        'Accounting.View'
      ],
      'Finance.GeneralLedger.View': [
        'Finance.GeneralLedger.View',
        'Reports.Accounting.View',
        'Accounting.View',
        'VIEW_FINANCE',
        'Finance.View',
        'Reports.View'
      ],
      'Finance.GeneralLedger.Export': [
        'Finance.GeneralLedger.Export',
        'Finance.GeneralLedger.View',
        'Reports.Export',
        'Reports.Accounting.View',
        'Accounting.View'
      ],
      'Finance.GeneralLedger.Print': [
        'Finance.GeneralLedger.Print',
        'Finance.GeneralLedger.View',
        'Reports.Accounting.View',
        'Accounting.View'
      ],
      'Finance.GeneralLedger.ViewAllBranches': [
        'Finance.GeneralLedger.ViewAllBranches',
        'Finance.GeneralLedger.View',
        'Accounting.View'
      ],
      'Branch.View': [
        'Branch.View',
        'Settings.Branches.View',
        'Accounting.View',
        'VIEW_FINANCE',
        'Finance.View'
      ],
      'Branch.Create': [
        'Branch.Create',
        'Settings.Branches.Create',
        'Accounting.Create',
        'Accounting.Update'
      ],
      'Branch.Update': [
        'Branch.Update',
        'Settings.Branches.Edit',
        'Accounting.Update'
      ],
      'Branch.Delete': [
        'Branch.Delete',
        'Settings.Branches.Delete',
        'Accounting.Delete'
      ],
      'Branch.Export': [
        'Branch.Export',
        'Settings.Branches.Export',
        'Branch.View',
        'Settings.Branches.View'
      ],
      'Branch.Print': [
        'Branch.Print',
        'Settings.Branches.Print',
        'Branch.View',
        'Settings.Branches.View'
      ],
      'Settings.Branches.View': [
        'Settings.Branches.View',
        'Branch.View',
        'Accounting.View',
        'VIEW_FINANCE',
        'Finance.View'
      ],
      'Settings.Branches.Create': [
        'Settings.Branches.Create',
        'Branch.Create',
        'Accounting.Create',
        'Accounting.Update'
      ],
      'Settings.Branches.Edit': [
        'Settings.Branches.Edit',
        'Branch.Update',
        'Accounting.Update'
      ],
      'Settings.Branches.Delete': [
        'Settings.Branches.Delete',
        'Branch.Delete',
        'Accounting.Delete'
      ],
      'Settings.Branches.Export': [
        'Settings.Branches.Export',
        'Branch.Export',
        'Settings.Branches.View',
        'Branch.View'
      ],
      'Settings.Branches.Print': [
        'Settings.Branches.Print',
        'Branch.Print',
        'Settings.Branches.View',
        'Branch.View'
      ],
      'Settings.TaxCodes.View': [
        'Settings.TaxCodes.View',
        'TaxCode.View',
        'Accounting.View',
        'VIEW_FINANCE',
        'Finance.View'
      ],
      'Settings.TaxCodes.Create': [
        'Settings.TaxCodes.Create',
        'TaxCode.Create',
        'Accounting.Create',
        'Accounting.Update'
      ],
      'Settings.TaxCodes.Edit': [
        'Settings.TaxCodes.Edit',
        'TaxCode.Update',
        'Accounting.Update'
      ],
      'Settings.TaxCodes.Delete': [
        'Settings.TaxCodes.Delete',
        'TaxCode.Delete',
        'Accounting.Delete'
      ],
      'Settings.TaxCodes.Export': [
        'Settings.TaxCodes.Export',
        'Settings.TaxCodes.View',
        'Accounting.View'
      ],
      'Settings.TaxCodes.Print': [
        'Settings.TaxCodes.Print',
        'Settings.TaxCodes.View',
        'Accounting.View'
      ],
      'Settings.NotificationReasons.View': [
        'Settings.NotificationReasons.View',
        'NotificationReason.View',
        'Accounting.View',
        'VIEW_FINANCE',
        'Finance.View'
      ],
      'Settings.NotificationReasons.Create': [
        'Settings.NotificationReasons.Create',
        'NotificationReason.Create',
        'Accounting.Create',
        'Accounting.Update'
      ],
      'Settings.NotificationReasons.Edit': [
        'Settings.NotificationReasons.Edit',
        'NotificationReason.Update',
        'Accounting.Update'
      ],
      'Settings.NotificationReasons.Delete': [
        'Settings.NotificationReasons.Delete',
        'NotificationReason.Delete',
        'Accounting.Delete'
      ],
      'Settings.NotificationReasons.Export': [
        'Settings.NotificationReasons.Export',
        'Settings.NotificationReasons.View',
        'Accounting.View'
      ],
      'Settings.NotificationReasons.Print': [
        'Settings.NotificationReasons.Print',
        'Settings.NotificationReasons.View',
        'Accounting.View'
      ],
      'Finance.OpeningBalances.View': [
        'Finance.OpeningBalances.View',
        'Journal.View',
        'Accounting.View',
        'VIEW_FINANCE',
        'Finance.View'
      ],
      'Finance.OpeningBalances.Create': [
        'Finance.OpeningBalances.Create',
        'Journal.Create',
        'Accounting.Create',
        'Accounting.Update'
      ],
      'Finance.OpeningBalances.Edit': [
        'Finance.OpeningBalances.Edit',
        'Accounting.Update'
      ],
      'Finance.OpeningBalances.Delete': [
        'Finance.OpeningBalances.Delete',
        'Accounting.Delete'
      ],
      'Finance.OpeningBalances.Post': [
        'Finance.OpeningBalances.Post',
        'Journal.Post',
        'Accounting.Update'
      ],
      'Finance.OpeningBalances.Reverse': [
        'Finance.OpeningBalances.Reverse',
        'Journal.Reverse',
        'Accounting.Update'
      ],
      'Finance.OpeningBalances.Export': [
        'Finance.OpeningBalances.Export',
        'Finance.OpeningBalances.View',
        'Accounting.View'
      ],
      'Finance.OpeningBalances.Print': [
        'Finance.OpeningBalances.Print',
        'Finance.OpeningBalances.View',
        'Accounting.View'
      ],
      'Finance.ReceiptVouchers.View': [
        'Finance.ReceiptVouchers.View',
        'Journal.View',
        'Accounting.View',
        'VIEW_FINANCE',
        'Finance.View'
      ],
      'Finance.ReceiptVouchers.Create': [
        'Finance.ReceiptVouchers.Create',
        'Journal.Create',
        'Accounting.Create',
        'Accounting.Update'
      ],
      'Finance.ReceiptVouchers.Edit': [
        'Finance.ReceiptVouchers.Edit',
        'Accounting.Update'
      ],
      'Finance.ReceiptVouchers.Delete': [
        'Finance.ReceiptVouchers.Delete',
        'Accounting.Delete'
      ],
      'Finance.ReceiptVouchers.Approve': [
        'Finance.ReceiptVouchers.Approve',
        'Finance.ReceiptVouchers.Post',
        'Journal.Post',
        'Accounting.Update'
      ],
      'Finance.ReceiptVouchers.Post': [
        'Finance.ReceiptVouchers.Post',
        'Journal.Post',
        'Accounting.Update'
      ],
      'Finance.ReceiptVouchers.Reverse': [
        'Finance.ReceiptVouchers.Reverse',
        'Journal.Reverse',
        'Accounting.Update'
      ],
      'Finance.ReceiptVouchers.Cancel': [
        'Finance.ReceiptVouchers.Cancel',
        'Finance.ReceiptVouchers.Edit',
        'Accounting.Update'
      ],
      'Finance.ReceiptVouchers.Export': [
        'Finance.ReceiptVouchers.Export',
        'Finance.ReceiptVouchers.View',
        'Accounting.View'
      ],
      'Finance.ReceiptVouchers.Print': [
        'Finance.ReceiptVouchers.Print',
        'Finance.ReceiptVouchers.View',
        'Accounting.View'
      ],
      'Finance.FinancialNotes.View': [
        'Finance.FinancialNotes.View',
        'Journal.View',
        'Accounting.View',
        'VIEW_FINANCE',
        'Finance.View'
      ],
      'Finance.FinancialNotes.Create': [
        'Finance.FinancialNotes.Create',
        'Journal.Create',
        'Accounting.Create',
        'Accounting.Update'
      ],
      'Finance.FinancialNotes.Edit': [
        'Finance.FinancialNotes.Edit',
        'Accounting.Update'
      ],
      'Finance.FinancialNotes.Delete': [
        'Finance.FinancialNotes.Delete',
        'Accounting.Delete'
      ],
      'Finance.FinancialNotes.Approve': [
        'Finance.FinancialNotes.Approve',
        'Finance.FinancialNotes.Post',
        'Journal.Post',
        'Accounting.Update'
      ],
      'Finance.FinancialNotes.Post': [
        'Finance.FinancialNotes.Post',
        'Journal.Post',
        'Accounting.Update'
      ],
      'Finance.FinancialNotes.Reverse': [
        'Finance.FinancialNotes.Reverse',
        'Journal.Reverse',
        'Accounting.Update'
      ],
      'Finance.FinancialNotes.Cancel': [
        'Finance.FinancialNotes.Cancel',
        'Finance.FinancialNotes.Edit',
        'Accounting.Update'
      ],
      'Journal.View': [
        'Journal.View',
        'Accounting.View',
        'VIEW_FINANCE',
        'Finance.View'
      ],
      'Journal.Create': [
        'Journal.Create',
        'Accounting.Create',
        'Accounting.Update'
      ],
      'Journal.Edit': [
        'Journal.Edit',
        'Journal.Create',
        'Accounting.Update',
        'Accounting.Create'
      ],
      'Journal.Approve': [
        'Journal.Approve',
        'Journal.Post',
        'Accounting.Update'
      ],
      'Journal.Post': ['Journal.Post', 'Accounting.Update'],
      'Journal.Reverse': ['Journal.Reverse', 'Accounting.Update'],
      'Journal.Delete': [
        'Journal.Delete',
        'Journal.Create',
        'Accounting.Delete'
      ],
      'Journal.Export': ['Journal.Export', 'Journal.View', 'Accounting.View'],
      'Journal.Print': ['Journal.Print', 'Journal.View', 'Accounting.View'],
      'Settings.Users.View': [
        'Settings.Users.View',
        'Identity.Users.View',
        'Identity.View',
        'Settings.View'
      ],
      'Settings.Users.Create': [
        'Settings.Users.Create',
        'Identity.Users.Create',
        'Identity.Users.Manage'
      ],
      'Settings.Users.Edit': [
        'Settings.Users.Edit',
        'Identity.Users.Edit',
        'Identity.Users.Manage'
      ],
      'Settings.Users.Delete': [
        'Settings.Users.Delete',
        'Identity.Users.Delete',
        'Identity.Users.Manage'
      ],
      'Settings.Users.ResetPassword': [
        'Settings.Users.ResetPassword',
        'Identity.Users.ResetPassword',
        'Identity.Users.Manage'
      ],
      'Settings.Users.LockUnlock': [
        'Settings.Users.LockUnlock',
        'Identity.Users.LockUnlock',
        'Identity.Users.Manage'
      ],
      'Settings.Users.Export': [
        'Settings.Users.Export',
        'Identity.Users.Export',
        'Settings.Users.View',
        'Identity.Users.View'
      ],
      'Settings.Users.Print': [
        'Settings.Users.Print',
        'Identity.Users.Print',
        'Settings.Users.View',
        'Identity.Users.View'
      ],
      'Identity.Users.View': [
        'Identity.Users.View',
        'Settings.Users.View',
        'Identity.View',
        'Settings.View'
      ],
      'Identity.Users.Create': [
        'Identity.Users.Create',
        'Settings.Users.Create',
        'Identity.Users.Manage'
      ],
      'Identity.Users.Edit': [
        'Identity.Users.Edit',
        'Settings.Users.Edit',
        'Identity.Users.Manage'
      ],
      'Identity.Users.Delete': [
        'Identity.Users.Delete',
        'Settings.Users.Delete',
        'Identity.Users.Manage'
      ],
      'FiscalPeriod.View': [
        'FiscalPeriod.View',
        'Accounting.View',
        'VIEW_FINANCE',
        'Finance.View'
      ],
      'FiscalPeriod.Create': [
        'FiscalPeriod.Create',
        'Accounting.Create',
        'Accounting.Update'
      ],
      'FiscalPeriod.Edit': [
        'FiscalPeriod.Edit',
        'FiscalPeriod.Create',
        'Accounting.Update'
      ],
      'FiscalPeriod.Delete': ['FiscalPeriod.Delete', 'Accounting.Delete'],
      'FiscalPeriod.Close': [
        'FiscalPeriod.Close',
        'Accounting.Update',
        'FiscalPeriod.Lock'
      ],
      'FiscalPeriod.Lock': [
        'FiscalPeriod.Lock',
        'Accounting.Update',
        'FiscalPeriod.Close'
      ],
      'FiscalPeriod.Reopen': [
        'FiscalPeriod.Reopen',
        'Accounting.Update',
        'FiscalPeriod.Close'
      ],
      EDIT_SETTINGS: ['EDIT_SETTINGS', 'Organization.Update', 'Tenant.Manage'],
      'Inventory.View': ['Inventory.View', 'Inventory.Manage', 'Catalog.View', 'Inventory.ItemTypes.View'],
      'Inventory.Manage': ['Inventory.Manage', 'Inventory.View'],
      'Inventory.ItemTypes.View': ['Inventory.ItemTypes.View', 'Inventory.View', 'Inventory.Manage'],
      'Inventory.ItemTypes.Create': ['Inventory.ItemTypes.Create', 'Inventory.Manage'],
      'Inventory.ItemTypes.Edit': ['Inventory.ItemTypes.Edit', 'Inventory.Manage'],
      'Inventory.ItemTypes.Delete': ['Inventory.ItemTypes.Delete', 'Inventory.Manage'],
      'Inventory.ItemTypes.Export': ['Inventory.ItemTypes.Export', 'Inventory.Manage', 'Inventory.View'],
      'Inventory.ValuationGroups.View': [
        'Inventory.ValuationGroups.View',
        'Inventory.View',
        'Inventory.Manage'
      ],
      'Inventory.ValuationGroups.Create': ['Inventory.ValuationGroups.Create', 'Inventory.Manage'],
      'Inventory.ValuationGroups.Edit': ['Inventory.ValuationGroups.Edit', 'Inventory.Manage'],
      'Inventory.ValuationGroups.Delete': ['Inventory.ValuationGroups.Delete', 'Inventory.Manage'],
      'Inventory.ValuationGroups.Export': [
        'Inventory.ValuationGroups.Export',
        'Inventory.Manage',
        'Inventory.View'
      ],
      'Inventory.Settings.View': [
        'Inventory.Settings.View',
        'Inventory.View',
        'Inventory.Manage'
      ],
      'Inventory.Settings.Edit': ['Inventory.Settings.Edit', 'Inventory.Manage'],
      'Inventory.Settings.Reset': ['Inventory.Settings.Reset', 'Inventory.Manage'],
      'Sales.ProductPricing.View': [
        'Sales.ProductPricing.View',
        'Inventory.View',
        'Inventory.Manage',
        'Sales.View'
      ],
      'Sales.ProductPricing.Create': [
        'Sales.ProductPricing.Create',
        'Inventory.Manage',
        'Sales.Create'
      ],
      'Sales.ProductPricing.Edit': [
        'Sales.ProductPricing.Edit',
        'Inventory.Manage',
        'Sales.Update'
      ],
      'Sales.ProductPricing.Delete': [
        'Sales.ProductPricing.Delete',
        'Inventory.Manage',
        'Sales.Update'
      ],
      'Sales.ProductPricing.Export': [
        'Sales.ProductPricing.Export',
        'Inventory.Manage',
        'Inventory.View',
        'Sales.View'
      ],
      'Sales.ProductPricing.Copy': [
        'Sales.ProductPricing.Copy',
        'Inventory.Manage',
        'Sales.Create'
      ],
      'Inventory.ProductInquiry.View': [
        'Inventory.ProductInquiry.View',
        'Inventory.View',
        'Inventory.Manage'
      ],
      'Inventory.ProductInquiry.Export': [
        'Inventory.ProductInquiry.Export',
        'Inventory.View',
        'Inventory.Manage'
      ],
      'Inventory.ProductInquiry.ViewCost': [
        'Inventory.ProductInquiry.ViewCost',
        'Inventory.ProductInquiry.View',
        'Inventory.View',
        'Inventory.Manage'
      ],
      'Inventory.ProductInquiry.ViewPrices': [
        'Inventory.ProductInquiry.ViewPrices',
        'Inventory.ProductInquiry.View',
        'Inventory.View',
        'Inventory.Manage'
      ],
      'Inventory.ProductInquiry.ViewMovements': [
        'Inventory.ProductInquiry.ViewMovements',
        'Inventory.ProductInquiry.View',
        'Inventory.View',
        'Inventory.Manage'
      ],
      'Inventory.ProductInquiry.ViewSuppliers': [
        'Inventory.ProductInquiry.ViewSuppliers',
        'Inventory.ProductInquiry.View',
        'Inventory.View',
        'Inventory.Manage'
      ],
      'Warehouse.View': ['Warehouse.View', 'Warehouse.Create', 'Warehouse.Update', 'Inventory.View', 'Inventory.Warehouses.View'],
      'Inventory.Warehouses.View': [
        'Inventory.Warehouses.View',
        'Warehouse.View',
        'Warehouse.Create',
        'Warehouse.Update',
        'Inventory.View',
        'Inventory.Manage'
      ],
      'Inventory.Warehouses.Create': [
        'Inventory.Warehouses.Create',
        'Warehouse.Create',
        'Inventory.Manage'
      ],
      'Inventory.Warehouses.Edit': [
        'Inventory.Warehouses.Edit',
        'Warehouse.Update',
        'Warehouse.Create',
        'Inventory.Manage'
      ],
      'Inventory.Warehouses.Delete': [
        'Inventory.Warehouses.Delete',
        'Warehouse.Delete',
        'Warehouse.Update',
        'Inventory.Manage'
      ],
      'Inventory.Warehouses.Export': [
        'Inventory.Warehouses.Export',
        'Warehouse.Export',
        'Warehouse.View',
        'Inventory.View',
        'Inventory.Manage'
      ],
      'Stock.View': ['Stock.View', 'Stock.Transfer', 'Stock.Adjust', 'Stock.Waste', 'Inventory.View'],
      'Stock.Transfer': ['Stock.Transfer', 'Stock.View', 'Inventory.Manage'],
      'Stock.Adjust': ['Stock.Adjust', 'Stock.View', 'Inventory.Manage'],
      'Stock.Waste': ['Stock.Waste', 'Stock.View', 'Inventory.Manage'],
      'Purchase.View': ['Purchase.View', 'Purchase.Create', 'Inventory.View'],
      'InventoryReports.View': ['InventoryReports.View', 'Reports.View', 'Inventory.View', 'Stock.View'],
      'Tax.View': ['Tax.View', 'Tax.Manage', 'Inventory.Manage', 'Inventory.View'],
      'Tax.Manage': ['Tax.Manage', 'Inventory.Manage'],
      'Supplier.View': [
        'Supplier.View',
        'Supplier.Manage',
        'Inventory.View',
        'Inventory.Manage'
      ],
      'Supplier.Create': ['Supplier.Create', 'Supplier.Manage', 'Inventory.Manage'],
      'Supplier.Update': ['Supplier.Update', 'Supplier.Manage', 'Inventory.Manage'],
      'Supplier.Delete': ['Supplier.Delete', 'Supplier.Manage', 'Inventory.Manage'],
      'Supplier.Activate': ['Supplier.Activate', 'Supplier.Manage', 'Inventory.Manage'],
      'Supplier.Blacklist': ['Supplier.Blacklist', 'Supplier.Manage', 'Inventory.Manage']
    };

    const candidates = aliases[required] ?? [required];
    return candidates.some(p => permissions.includes(p));
  }

  loadCurrentUser(): Observable<CurrentUserProfile | null> {
    return this.http.get<CurrentUserProfile>(`${this.API}/me`).pipe(
      tap((profile) => {
        this.currentUser.set(profile);
        this.userRoles.set(profile.roles ?? []);
        const permissions = profile.permissions?.length
          ? profile.permissions
          : profile.roles?.includes('Administrator')
            ? ['ALL']
            : [];
        this.userPermissions.set(permissions);
      }),
      catchError(() => {
        if (this.isAuthenticated()) {
          this.userRoles.set(['Administrator']);
          this.userPermissions.set(['ALL']);
        }
        this.currentUser.set(null);
        return of(null);
      })
    );
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  setToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
    this.isAuthenticated.set(true);
    this.startIdleTracking();
  }

  setRefreshToken(token: string): void {
    localStorage.setItem(this.REFRESH_TOKEN_KEY, token);
  }

  login(email: string, password: string): Observable<{ success: boolean; error?: string }> {
    return this.http.post<AuthResponse>(`${this.API}/login`, { email, password }).pipe(
      tap((res) => {
        this.setToken(res.token);
        this.setRefreshToken(res.refreshToken);
      }),
      switchMap(() => this.loadCurrentUser().pipe(map(() => ({ success: true })))),
      catchError((err) => {
        const status = err?.status ?? err?.error?.status;
        const message = status === 0
          ? 'Cannot reach API. Start Backend on http://localhost:5162 and ensure ng serve uses proxy.conf.json'
          : status === 401 || status === 422
            ? 'Invalid email or password.'
            : (err?.message?.includes('Cannot reach') || err?.message?.includes('network'))
              ? 'Cannot reach API. Start Backend on http://localhost:5162'
              : 'Login failed. Please try again.';
        return of({ success: false, error: message });
      })
    );
  }

  refreshToken(): Observable<{ token: string }> {
    const refreshToken = localStorage.getItem(this.REFRESH_TOKEN_KEY);
    const token = this.getToken();
    return this.http.post<AuthResponse>(`${this.API}/refresh`, { token, refreshToken }).pipe(
      tap((res) => {
        this.setToken(res.token);
        this.setRefreshToken(res.refreshToken);
      }),
      map((res) => ({ token: res.token }))
    );
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    this.isAuthenticated.set(false);
    this.userPermissions.set([]);
    this.userRoles.set([]);
    this.currentUser.set(null);
    if (this.idleTimer) {
      clearTimeout(this.idleTimer);
    }
  }

  private startIdleTracking(): void {
    this.resetIdleTimer();

    const activity$ = merge(
      fromEvent(document, 'mousemove'),
      fromEvent(document, 'click'),
      fromEvent(document, 'keypress'),
      fromEvent(document, 'touchstart')
    );

    activity$.subscribe(() => this.resetIdleTimer());
  }

  private resetIdleTimer(): void {
    if (this.idleTimer) {
      clearTimeout(this.idleTimer);
    }
    this.idleTimer = setTimeout(() => {
      this.handleSessionTimeout();
    }, this.TIMEOUT_MS);
  }

  private handleSessionTimeout(): void {
    console.warn('[Auth Service]: Session timeout due to user inactivity. Logging out...');
    this.logout();
    this.router.navigate(['/login'], { queryParams: { reason: 'session_expired' } });
  }
}
