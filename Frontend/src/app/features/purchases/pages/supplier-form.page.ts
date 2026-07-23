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
import { ActivatedRoute, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { catchError, forkJoin, map, of, switchMap } from 'rxjs';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { SupplierRepository } from '../../../core/repositories/supplier.repository';
import { ChartOfAccountRepository } from '../../../core/repositories/chart-of-account.repository';
import { AccountClassificationRepository } from '../../../core/repositories/account-classification.repository';
import {
  PAYMENT_METHOD_KINDS,
  PAYMENT_TERM_PRESETS,
  SupplierPaymentMethod,
  SupplierPaymentMethodKind,
  UpsertSupplierMasterPayload
} from '../../../core/models/supplier.models';
import { ChartAccount } from '../../../core/models/chart-of-account.models';
import { CostCenterLookup } from '../../../core/models/inventory-valuation-group.models';
import { InventoryPageShellComponent } from '../../inventory/shared/inventory-page-shell.component';
import { AppDialogComponent } from '../../../shared/ui/app-dialog/app-dialog.component';
import { collectSubtreeIds, flattenTreeAccounts } from '../../finance/pages/coa-tree.util';

/** AccountClassification.code for Accounts Payable / الموردون. */
const SUPPLIER_AP_CLASSIFICATION_CODE = 'payable';

/** SUP + yyyyMM + #### — digits only after the prefix (e.g. SUP2026070001). */
function supplierPeriodPrefix(at = new Date()): string {
  return `SUP${at.getFullYear()}${String(at.getMonth() + 1).padStart(2, '0')}`;
}

function previewSupplierCode(sequence = 1, at = new Date()): string {
  const seq = String(Math.max(1, sequence)).padStart(4, '0');
  return `${supplierPeriodPrefix(at)}${seq}`;
}

function isRealSupplierCode(code: string | null | undefined): boolean {
  return /^SUP\d{10}$/.test((code ?? '').trim());
}

type SupplierTab = 'data' | 'contact' | 'payments';

@Component({
  selector: 'app-supplier-form-page',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, InventoryPageShellComponent, AppDialogComponent],
  templateUrl: './supplier-form.page.html',
  styleUrl: './supplier-form.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SupplierFormPage implements OnInit {
  private repo = inject(SupplierRepository);
  private accountsRepo = inject(ChartOfAccountRepository);
  private classificationsRepo = inject(AccountClassificationRepository);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  tab = signal<SupplierTab>('data');
  showSavedDialog = signal(false);

  docId = signal<string | null>(null);
  /** Always start with a real numeric period code (never YYYY/MM placeholders). */
  code = signal(previewSupplierCode(1));
  nameAr = signal('');
  nameEn = signal('');
  taxNumber = signal('');
  isActive = signal(true);
  isBlacklisted = signal(false);

  // Contact
  contactPerson = signal('');
  mobile = signal('');
  phone = signal('');
  email = signal('');
  website = signal('');
  country = signal('');
  city = signal('');
  address = signal('');
  postalCode = signal('');
  notes = signal('');

  // Financial
  apAccountId = signal<string | null>(null);
  costCenterId = signal<string | null>(null);
  paymentTerms = signal('30 Days');
  paymentDueDays = signal(30);
  creditLimit = signal(0);
  currency = signal('SAR');

  paymentMethods = signal<SupplierPaymentMethod[]>([]);
  selectedPmIndex = signal<number | null>(null);
  addPmKind = signal<SupplierPaymentMethodKind>(1);

  accounts = signal<ChartAccount[]>([]);
  payableClassificationId = signal<string | null>(null);
  apSubtreeIds = signal<Set<string>>(new Set());
  costCenters = signal<CostCenterLookup[]>([]);

  /** Posting accounts under الموردون / AP tree only. */
  supplierAccounts = computed(() => {
    const payableClassId = this.payableClassificationId();
    const subtree = this.apSubtreeIds();
    const selected = this.apAccountId();
    return this.accounts().filter(a => {
      if (selected && a.id === selected) return true;
      if (!a.isActive || a.isSummaryAccount || !a.isPostingAllowed) return false;
      if (payableClassId && a.accountClassificationId === payableClassId) return true;
      if (subtree.has(a.id)) return true;
      return false;
    });
  });

  paymentTermPresets = PAYMENT_TERM_PRESETS;
  paymentMethodKinds = PAYMENT_METHOD_KINDS;

  breadcrumbs = [
    { labelKey: 'nav.purchases', path: '/purchases/dashboard' },
    { labelKey: 'pur.nav.suppliers', path: '/purchases/suppliers' },
    { labelKey: 'pur.sup.formBreadcrumb' }
  ];

  canSave = computed(
    () =>
      this.auth.hasPermission('Supplier.Create') ||
      this.auth.hasPermission('Supplier.Update') ||
      this.auth.hasPermission('Inventory.Manage')
  );
  isNew = computed(() => !this.docId());
  pageTitle = computed(() =>
    this.isNew() ? this.t('pur.sup.createTitle') : this.t('pur.sup.editTitle')
  );

  ngOnInit(): void {
    this.loadSupplierAccounts();
    this.repo
      .getCostCenters()
      .pipe(catchError(() => of([] as CostCenterLookup[])))
      .subscribe(c => this.costCenters.set(c));

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.docId.set(id);
      this.load(id);
    } else {
      this.loadNextCode();
    }
  }

  private loadNextCode(): void {
    this.code.set(previewSupplierCode(1));

    this.repo
      .getNextCode()
      .pipe(
        map(code => (isRealSupplierCode(code) ? code.trim() : '')),
        catchError(() =>
          this.repo.getList({ page: 1, pageSize: 200 }).pipe(
            map(list => this.nextCodeFromList(list.map(s => s.code))),
            catchError(() => of(previewSupplierCode(1)))
          )
        )
      )
      .subscribe(code => {
        this.code.set(isRealSupplierCode(code) ? code.trim() : previewSupplierCode(1));
      });
  }

  private nextCodeFromList(codes: Array<string | null | undefined>): string {
    const period = supplierPeriodPrefix();
    let max = 0;
    for (const raw of codes) {
      const c = (raw ?? '').trim().toUpperCase();
      if (!c.startsWith(period) || !isRealSupplierCode(c)) continue;
      const n = Number.parseInt(c.slice(period.length), 10);
      if (!Number.isNaN(n) && n > max) max = n;
    }
    return previewSupplierCode(max + 1);
  }

  private loadSupplierAccounts(): void {
    forkJoin({
      tree: this.accountsRepo.getTree({ includeInactive: false, accountType: 2 }).pipe(catchError(() => of([]))),
      classifications: this.classificationsRepo.getList().pipe(catchError(() => of([]))),
      settings: this.accountsRepo.getSettings().pipe(catchError(() => of(null)))
    }).subscribe(({ tree, classifications, settings }) => {
      this.accounts.set(flattenTreeAccounts(tree));

      const payable = classifications.find(
        c => (c.code || '').toLowerCase() === SUPPLIER_AP_CLASSIFICATION_CODE
      );
      this.payableClassificationId.set(payable?.id ?? null);
      this.apSubtreeIds.set(collectSubtreeIds(tree, settings?.accountsPayableAccountId));
    });
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  setTab(tab: SupplierTab): void {
    this.tab.set(tab);
  }

  onPaymentTermChange(value: string): void {
    this.paymentTerms.set(value);
    const preset = PAYMENT_TERM_PRESETS.find(p => p.value === value);
    if (preset) this.paymentDueDays.set(preset.dueDays);
  }

  onAddPmKindChange(value: number | string): void {
    this.addPmKind.set(Number(value) as SupplierPaymentMethodKind);
  }

  addPaymentMethod(): void {
    const kind = this.addPmKind();
    if (kind !== 1 && kind !== 2) {
      this.error.set(this.t('pur.sup.validation.pmKind'));
      return;
    }
    const methods = [...this.paymentMethods()];
    if (methods.some(m => m.kind === kind)) {
      this.error.set(this.t('pur.sup.validation.pmDuplicate'));
      return;
    }
    methods.push({
      kind,
      currency: this.currency() || 'SAR',
      isDefault: methods.length === 0
    });
    this.paymentMethods.set(methods);
    this.selectedPmIndex.set(methods.length - 1);
    this.error.set(null);
  }

  removeSelectedPaymentMethod(): void {
    const idx = this.selectedPmIndex();
    if (idx == null) return;
    const methods = this.paymentMethods().filter((_, i) => i !== idx);
    if (methods.length && !methods.some(m => m.isDefault)) {
      methods[0] = { ...methods[0], isDefault: true };
    }
    this.paymentMethods.set(methods);
    this.selectedPmIndex.set(null);
  }

  setDefaultPaymentMethod(): void {
    const idx = this.selectedPmIndex();
    if (idx == null) return;
    this.paymentMethods.set(
      this.paymentMethods().map((m, i) => ({ ...m, isDefault: i === idx }))
    );
  }

  pmLabel(kind: SupplierPaymentMethodKind): string {
    const found = PAYMENT_METHOD_KINDS.find(k => k.value === kind);
    if (found) return this.t(found.labelKey);
    // Legacy kinds (bank/cheque/card) if present on older records
    const legacy: Partial<Record<number, string>> = {
      3: 'pur.sup.pm.bank',
      4: 'pur.sup.pm.cheque',
      5: 'pur.sup.pm.card'
    };
    const key = legacy[kind];
    return key ? this.t(key) : String(kind);
  }

  accountLabel(a: ChartAccount): string {
    const code = a.accountNumber || '';
    return code ? `${code} — ${a.nameAr}` : a.nameAr;
  }

  back(): void {
    void this.router.navigate(['/purchases/suppliers']);
  }

  acknowledgeSaved(): void {
    this.showSavedDialog.set(false);
    void this.router.navigate(['/purchases/suppliers']);
  }

  private onSaveSuccess(): void {
    this.saving.set(false);
    this.showSavedDialog.set(true);
  }

  save(): void {
    if (!this.canSave() || this.saving()) return;
    const name = this.nameAr().trim();
    if (!name) {
      this.error.set(this.t('pur.sup.validation.name'));
      this.tab.set('data');
      return;
    }
    if (!this.apAccountId()) {
      this.error.set(this.t('pur.sup.validation.account'));
      this.tab.set('data');
      return;
    }
    const email = this.email().trim();
    if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      this.error.set(this.t('pur.sup.validation.email'));
      this.tab.set('contact');
      return;
    }
    if (this.creditLimit() < 0) {
      this.error.set(this.t('pur.sup.validation.creditLimit'));
      this.tab.set('data');
      return;
    }

    this.saving.set(true);
    this.error.set(null);

    const master = this.buildMasterPayload();
    const defaultKind =
      this.paymentMethods().find(m => m.isDefault)?.kind ??
      this.paymentMethods()[0]?.kind ??
      2;

    if (this.docId()) {
      this.persistExisting(master, defaultKind);
      return;
    }

    this.repo
      .create({
        nameAr: name,
        nameEn: this.nameEn().trim() || null,
        code: this.code().trim() || null,
        apAccountId: this.apAccountId()!,
        currency: this.currency() || 'SAR',
        taxNumber: this.taxNumber().trim() || null,
        phone: this.phone().trim() || null,
        email: email || null,
        city: this.city().trim() || null,
        country: this.country().trim() || null,
        address: this.address().trim() || null,
        notes: this.notes().trim() || null,
        paymentDueDays: this.paymentDueDays(),
        creditLimit: this.creditLimit(),
        defaultPaymentMethod: defaultKind
      })
      .pipe(
        switchMap(created =>
          this.repo.upsertMaster(created.id, { ...master, defaultPaymentMethod: defaultKind }).pipe(
            catchError(err => {
              this.error.set(err?.error?.error ?? this.t('pur.sup.saveFailed'));
              return of(created);
            })
          )
        )
      )
      .subscribe({
        next: () => this.onSaveSuccess(),
        error: err => {
          this.error.set(err?.error?.error ?? this.t('pur.sup.saveFailed'));
          this.saving.set(false);
        }
      });
  }

  private persistExisting(master: UpsertSupplierMasterPayload, defaultKind: SupplierPaymentMethodKind): void {
    const id = this.docId()!;
    this.repo.upsertMaster(id, { ...master, defaultPaymentMethod: defaultKind }).subscribe({
      next: doc => {
        const afterStatus = () => this.onSaveSuccess();

        if (this.isBlacklisted() && !doc.isBlacklisted) {
          this.repo.blacklist(id).subscribe({ next: afterStatus, error: afterStatus });
          return;
        }
        if (!this.isBlacklisted() && doc.isBlacklisted) {
          this.repo.clearBlacklist(id).subscribe({ next: afterStatus, error: afterStatus });
          return;
        }
        if (this.isActive() && !doc.isActive && !this.isBlacklisted()) {
          this.repo.activate(id).subscribe({ next: afterStatus, error: afterStatus });
          return;
        }
        if (!this.isActive() && doc.isActive && !this.isBlacklisted()) {
          this.repo.deactivate(id).subscribe({ next: afterStatus, error: afterStatus });
          return;
        }
        afterStatus();
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('pur.sup.saveFailed'));
        this.saving.set(false);
      }
    });
  }

  private load(id: string): void {
    this.loading.set(true);
    this.repo.getById(id).subscribe({
      next: doc => {
        this.applyDoc(doc);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('pur.sup.loadFailed'));
        this.loading.set(false);
      }
    });
  }

  private applyDoc(doc: {
    id: string;
    code: string;
    nameAr: string;
    nameEn?: string | null;
    taxNumber?: string | null;
    isActive: boolean;
    isBlacklisted: boolean;
    contactPerson?: string | null;
    phone?: string | null;
    mobile?: string | null;
    email?: string | null;
    website?: string | null;
    country?: string | null;
    city?: string | null;
    address?: string | null;
    postalCode?: string | null;
    notes?: string | null;
    apAccountId?: string | null;
    paymentTerms?: string | null;
    paymentDueDays: number;
    creditLimit: number;
    currency: string;
    paymentMethods?: SupplierPaymentMethod[];
  }): void {
    this.docId.set(doc.id);
    this.code.set(doc.code);
    this.nameAr.set(doc.nameAr);
    this.nameEn.set(doc.nameEn || '');
    this.taxNumber.set(doc.taxNumber || '');
    this.isActive.set(doc.isActive);
    this.isBlacklisted.set(doc.isBlacklisted);
    this.contactPerson.set(doc.contactPerson || '');
    this.phone.set(doc.phone || '');
    this.mobile.set(doc.mobile || '');
    this.email.set(doc.email || '');
    this.website.set(doc.website || '');
    this.country.set(doc.country || '');
    this.city.set(doc.city || '');
    this.address.set(doc.address || '');
    this.postalCode.set(doc.postalCode || '');
    this.notes.set(doc.notes || '');
    this.apAccountId.set(doc.apAccountId ?? null);
    this.paymentTerms.set(doc.paymentTerms || '30 Days');
    this.paymentDueDays.set(doc.paymentDueDays ?? 0);
    this.creditLimit.set(doc.creditLimit ?? 0);
    this.currency.set(doc.currency || 'SAR');
    this.paymentMethods.set(
      (doc.paymentMethods || []).map(m => ({
        id: m.id,
        kind: m.kind,
        bankName: m.bankName,
        iban: m.iban,
        swift: m.swift,
        accountNumber: m.accountNumber,
        beneficiaryName: m.beneficiaryName,
        currency: m.currency || 'SAR',
        isDefault: m.isDefault,
        notes: m.notes
      }))
    );
  }

  private buildMasterPayload(): UpsertSupplierMasterPayload {
    const methods = this.paymentMethods();
    return {
      nameAr: this.nameAr().trim(),
      nameEn: this.nameEn().trim() || null,
      supplierType: 1,
      category: 9,
      companyId: null,
      branchId: null,
      taxNumber: this.taxNumber().trim() || null,
      commercialRegister: null,
      establishmentNumber: null,
      taxRegistrationCountry: null,
      taxType: null,
      defaultTaxPercent: 0,
      taxCertificateExpiry: null,
      commercialRegisterExpiry: null,
      contactPerson: this.contactPerson().trim() || null,
      contactJobTitle: null,
      phone: this.phone().trim() || null,
      mobile: this.mobile().trim() || null,
      email: this.email().trim() || null,
      website: this.website().trim() || null,
      city: this.city().trim() || null,
      region: null,
      country: this.country().trim() || null,
      postalCode: this.postalCode().trim() || null,
      address: this.address().trim() || null,
      apAccountId: this.apAccountId()!,
      discountAccountId: null,
      purchaseReturnAccountId: null,
      exchangeDifferenceAccountId: null,
      currency: this.currency() || 'SAR',
      defaultPaymentMethod: methods.find(m => m.isDefault)?.kind ?? methods[0]?.kind ?? 2,
      paymentDueDays: Number(this.paymentDueDays()) || 0,
      paymentTerms: this.paymentTerms() || null,
      creditLimit: Number(this.creditLimit()) || 0,
      openingBalance: 0,
      openingBalanceDate: null,
      vatEvaluation: 1,
      leadTimeDays: 0,
      isPreferred: false,
      rating: 0,
      notes: this.notes().trim() || null,
      paymentMethods: methods.map(m => ({
        kind: m.kind,
        bankName: m.bankName,
        iban: m.iban,
        swift: m.swift,
        accountNumber: m.accountNumber,
        beneficiaryName: m.beneficiaryName,
        currency: m.currency || 'SAR',
        isDefault: m.isDefault,
        notes: m.notes
      }))
    };
  }
}
