import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpParams } from '@angular/common/http';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { environment } from '../../../../environments/environment';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { InventoryService } from '../../../core/services/inventory.service';
import {
  BranchLookup,
  INVENTORY_COSTING_METHODS,
  INVENTORY_DOCUMENT_SERIES_TYPES,
  InventoryDocumentNumberSeries,
  InventorySetting,
  UpsertInventorySettingPayload
} from '../../../core/models/inventory.models';
import { InventoryPageShellComponent } from '../shared/inventory-page-shell.component';
import { InventorySkeletonComponent } from '../shared/inventory-skeleton.component';
import { InventoryErrorStateComponent } from '../shared/inventory-error-state.component';

type SettingsTab =
  | 'general'
  | 'costing'
  | 'control'
  | 'posting'
  | 'numbering'
  | 'integrations'
  | 'notifications'
  | 'advanced';

@Component({
  selector: 'app-inventory-settings-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatTooltipModule,
    InventoryPageShellComponent,
    InventorySkeletonComponent,
    InventoryErrorStateComponent
  ],
  templateUrl: './inventory-settings.page.html',
  styleUrl: './inventory-settings.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventorySettingsPage implements OnInit {
  private http = inject(HttpClient);
  lang = inject(LanguageService);
  auth = inject(AuthService);
  inventory = inject(InventoryService);

  costingMethods = INVENTORY_COSTING_METHODS;
  docTypes = INVENTORY_DOCUMENT_SERIES_TYPES;

  loading = signal(true);
  loaded = signal(false);
  saving = signal(false);
  branchesLoading = signal(false);
  error = signal<string | null>(null);
  success = signal(false);
  activeTab = signal<SettingsTab>('numbering');
  branchId = signal<string | null>(null);
  branches = signal<BranchLookup[]>([]);
  settingId = signal<string | null>(null);

  form: UpsertInventorySettingPayload = this.emptyForm();

  tabs: { id: SettingsTab; labelKey: string }[] = [
    { id: 'general', labelKey: 'inv.settings.tab.general' },
    { id: 'costing', labelKey: 'inv.settings.tab.costing' },
    { id: 'control', labelKey: 'inv.settings.tab.control' },
    { id: 'posting', labelKey: 'inv.settings.tab.posting' },
    { id: 'numbering', labelKey: 'inv.settings.tab.numbering' },
    { id: 'integrations', labelKey: 'inv.settings.tab.integrations' },
    { id: 'notifications', labelKey: 'inv.settings.tab.notifications' },
    { id: 'advanced', labelKey: 'inv.settings.tab.advanced' }
  ];

  breadcrumbs = [
    { labelKey: 'nav.inventory', path: '/inventory/dashboard' },
    { labelKey: 'inv.nav.settings' }
  ];

  canEdit = computed(() => this.auth.hasPermission('Inventory.Settings.Edit'));
  canReset = computed(() => this.auth.hasPermission('Inventory.Settings.Reset'));

  selectedBranchName = computed(() => {
    const id = this.branchId();
    if (!id) return this.t('inv.settings.allBranches');
    return this.branches().find(b => b.id === id)?.nameAr ?? '—';
  });

  ngOnInit(): void {
    this.inventory.loadWarehouses();
    this.inventory.loadUnits();
    this.loadBranches();
    this.reload();
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  selectTab(tab: SettingsTab): void {
    this.activeTab.set(tab);
  }

  loadBranches(): void {
    this.branchesLoading.set(true);
    const params = new HttpParams().set('page', 1).set('pageSize', 200);
    this.http.get<BranchLookup[]>(`${environment.apiBaseUrl}/organization/branches`, { params }).subscribe({
      next: rows => {
        this.branches.set(rows ?? []);
        this.branchesLoading.set(false);
      },
      error: () => {
        this.branches.set([]);
        this.branchesLoading.set(false);
      }
    });
  }

  reload(): void {
    this.loading.set(true);
    this.error.set(null);
    this.success.set(false);
    this.inventory.getSettings(this.branchId()).subscribe({
      next: s => {
        this.applySetting(s);
        this.loaded.set(true);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.error ?? err?.message ?? this.t('inv.settings.loadError'));
        this.loaded.set(false);
        this.loading.set(false);
      }
    });
  }

  save(): void {
    if (!this.canEdit()) return;
    this.saving.set(true);
    this.error.set(null);
    this.success.set(false);
    const payload: UpsertInventorySettingPayload = {
      ...this.form,
      branchId: this.branchId(),
      defaultWarehouseId: this.form.defaultWarehouseId || null,
      defaultUnitId: this.form.defaultUnitId || null,
      costingMethod: Number(this.form.costingMethod) || 2,
      documentSeries: this.form.documentSeries ?? []
    };
    this.inventory.upsertSettings(payload).subscribe({
      next: s => {
        this.applySetting(s);
        this.saving.set(false);
        this.success.set(true);
      },
      error: err => {
        this.error.set(err?.error?.error ?? err?.message ?? this.t('inv.settings.saveError'));
        this.saving.set(false);
      }
    });
  }

  resetDefaults(): void {
    if (!this.canReset()) return;
    if (!confirm(this.t('inv.settings.confirmReset'))) return;
    this.saving.set(true);
    this.error.set(null);
    this.inventory.resetSettings(this.branchId()).subscribe({
      next: s => {
        this.applySetting(s);
        this.saving.set(false);
        this.success.set(true);
      },
      error: err => {
        this.error.set(err?.error?.error ?? err?.message ?? this.t('inv.settings.saveError'));
        this.saving.set(false);
      }
    });
  }

  seriesFor(type: number): InventoryDocumentNumberSeries {
    let row = this.form.documentSeries.find(s => Number(s.documentType) === type);
    if (!row) {
      row = {
        documentType: type,
        prefix: 'DOC',
        numberLength: 6,
        nextNumber: 1,
        autoIncrement: true
      };
      this.form.documentSeries = [...this.form.documentSeries, row];
    }
    return row;
  }

  private applySetting(s: InventorySetting): void {
    this.settingId.set(s.id && s.id !== '00000000-0000-0000-0000-000000000000' ? s.id : null);
    this.form = {
      companyId: s.companyId ?? null,
      branchId: s.branchId ?? this.branchId(),
      defaultWarehouseId: s.defaultWarehouseId ?? null,
      defaultUnitId: s.defaultUnitId ?? null,
      defaultCurrencyCode: s.defaultCurrencyCode ?? 'SAR',
      autoGenerateItemCode: !!s.autoGenerateItemCode,
      enableMultiWarehouse: s.enableMultiWarehouse ?? true,
      enableWarehouseHierarchy: s.enableWarehouseHierarchy ?? true,
      enableBatchTracking: !!s.enableBatchTracking,
      enableSerialTracking: !!s.enableSerialTracking,
      enableExpiryTracking: !!s.enableExpiryTracking,
      enableBarcode: s.enableBarcode ?? true,
      enableQrCode: !!s.enableQrCode,
      costingMethod: this.normalizeCosting(s.costingMethod),
      costPrecision: s.costPrecision ?? 4,
      roundCost: s.roundCost ?? true,
      autoRecalculateCost: s.autoRecalculateCost ?? true,
      allowNegativeStock: !!s.allowNegativeStock,
      checkAvailableQuantity: s.checkAvailableQuantity ?? true,
      enableReservation: s.enableReservation ?? true,
      autoReleaseReservation: s.autoReleaseReservation ?? true,
      freezeDuringCount: s.freezeDuringCount ?? true,
      allowZeroCost: !!s.allowZeroCost,
      allowNegativeCost: !!s.allowNegativeCost,
      validateWarehouseBeforePosting: s.validateWarehouseBeforePosting ?? true,
      autoIssueRecipe: s.autoIssueRecipe ?? true,
      requireApprovalBeforePosting: !!s.requireApprovalBeforePosting,
      autoPostAfterApproval: s.autoPostAfterApproval ?? true,
      allowUnpost: !!s.allowUnpost,
      createReverseEntry: s.createReverseEntry ?? true,
      lockPostedDocuments: s.lockPostedDocuments ?? true,
      allowEditDraft: s.allowEditDraft ?? true,
      allowDeleteDraft: s.allowDeleteDraft ?? true,
      enablePurchasingIntegration: s.enablePurchasingIntegration ?? true,
      enablePosIntegration: s.enablePosIntegration ?? true,
      enableProductionIntegration: s.enableProductionIntegration ?? true,
      enableAccountingIntegration: s.enableAccountingIntegration ?? true,
      enableKitchenIntegration: s.enableKitchenIntegration ?? true,
      enableDeliveryIntegration: !!s.enableDeliveryIntegration,
      lowStockAlert: s.lowStockAlert ?? true,
      outOfStockAlert: s.outOfStockAlert ?? true,
      nearExpiryAlert: s.nearExpiryAlert ?? true,
      expiredItemsAlert: s.expiredItemsAlert ?? true,
      cycleCountReminder: !!s.cycleCountReminder,
      emailNotifications: !!s.emailNotifications,
      pushNotifications: s.pushNotifications ?? true,
      enableMultiCompany: !!s.enableMultiCompany,
      enableMultiBranch: s.enableMultiBranch ?? true,
      enableWarehouseZones: s.enableWarehouseZones ?? true,
      enableShelves: s.enableShelves ?? true,
      enableBins: s.enableBins ?? true,
      enableRfid: !!s.enableRfid,
      enableMobileScanner: s.enableMobileScanner ?? true,
      documentSeries: (s.documentSeries?.length
        ? s.documentSeries
        : INVENTORY_DOCUMENT_SERIES_TYPES.map(t => ({
            documentType: t.value,
            prefix: 'DOC',
            numberLength: 6,
            nextNumber: 1,
            autoIncrement: true
          }))).map(d => ({ ...d, documentType: Number(d.documentType) }))
    };
  }

  private normalizeCosting(value: InventorySetting['costingMethod']): number {
    if (typeof value === 'number') return value;
    const found = INVENTORY_COSTING_METHODS.find(m => m.key === value);
    return found?.value ?? 2;
  }

  private emptyForm(): UpsertInventorySettingPayload {
    return {
      companyId: null,
      branchId: null,
      defaultWarehouseId: null,
      defaultUnitId: null,
      defaultCurrencyCode: 'SAR',
      autoGenerateItemCode: true,
      enableMultiWarehouse: true,
      enableWarehouseHierarchy: true,
      enableBatchTracking: false,
      enableSerialTracking: false,
      enableExpiryTracking: false,
      enableBarcode: true,
      enableQrCode: false,
      costingMethod: 2,
      costPrecision: 4,
      roundCost: true,
      autoRecalculateCost: true,
      allowNegativeStock: false,
      checkAvailableQuantity: true,
      enableReservation: true,
      autoReleaseReservation: true,
      freezeDuringCount: true,
      allowZeroCost: false,
      allowNegativeCost: false,
      validateWarehouseBeforePosting: true,
      autoIssueRecipe: true,
      requireApprovalBeforePosting: false,
      autoPostAfterApproval: true,
      allowUnpost: false,
      createReverseEntry: true,
      lockPostedDocuments: true,
      allowEditDraft: true,
      allowDeleteDraft: true,
      enablePurchasingIntegration: true,
      enablePosIntegration: true,
      enableProductionIntegration: true,
      enableAccountingIntegration: true,
      enableKitchenIntegration: true,
      enableDeliveryIntegration: false,
      lowStockAlert: true,
      outOfStockAlert: true,
      nearExpiryAlert: true,
      expiredItemsAlert: true,
      cycleCountReminder: false,
      emailNotifications: false,
      pushNotifications: true,
      enableMultiCompany: false,
      enableMultiBranch: true,
      enableWarehouseZones: true,
      enableShelves: true,
      enableBins: true,
      enableRfid: false,
      enableMobileScanner: true,
      documentSeries: INVENTORY_DOCUMENT_SERIES_TYPES.map(t => ({
        documentType: t.value,
        prefix: 'DOC',
        numberLength: 6,
        nextNumber: 1,
        autoIncrement: true
      }))
    };
  }
}
