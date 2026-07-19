import {
  ChangeDetectionStrategy,
  Component,
  HostListener,
  OnInit,
  computed,
  inject,
  signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { TaxCodeRepository } from '../../../core/repositories/tax-code.repository';
import { ChartOfAccountRepository } from '../../../core/repositories/chart-of-account.repository';
import {
  TAX_APPLIES_TO,
  TAX_CALC_METHODS,
  OrgBranchLookup,
  OrgCompanyLookup,
  TaxCode,
  UpsertTaxCodePayload
} from '../../../core/models/tax-code.models';
import { ChartAccount } from '../../../core/models/chart-of-account.models';
import { flattenTreeAccounts } from './coa-tree.util';

@Component({
  selector: 'app-tax-codes-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatIconModule, MatTooltipModule],
  templateUrl: './tax-codes.page.html',
  styleUrl: './tax-codes.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TaxCodesPage implements OnInit {
  private repo = inject(TaxCodeRepository);
  private accountsRepo = inject(ChartOfAccountRepository);
  private fb = inject(FormBuilder);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  rows = signal<TaxCode[]>([]);
  companies = signal<OrgCompanyLookup[]>([]);
  branches = signal<OrgBranchLookup[]>([]);
  accounts = signal<ChartAccount[]>([]);

  selectedId = signal<string | null>(null);
  search = signal('');
  filterCompanyId = signal('');
  filterBranchId = signal('');
  filterAppliesTo = signal('');
  filterActive = signal('');
  showModal = signal(false);
  editingId = signal<string | null>(null);
  formCompanyId = signal('');
  closeAfterSave = signal(false);
  selectedRateIndex = signal<number | null>(null);

  appliesOptions = TAX_APPLIES_TO;
  methodOptions = TAX_CALC_METHODS;

  form = this.fb.nonNullable.group({
    companyId: ['', Validators.required],
    branchId: [''],
    code: ['', [Validators.required, Validators.maxLength(20)]],
    nameAr: ['', [Validators.required, Validators.maxLength(150)]],
    nameEn: ['', [Validators.maxLength(150)]],
    appliesTo: [3 as number, Validators.required],
    calculationMethod: [1 as number, Validators.required],
    salesAccountId: [''],
    purchaseAccountId: [''],
    priceIncludesTax: [false],
    isActive: [true],
    rates: this.fb.array([])
  });

  canView = computed(
    () =>
      this.auth.hasPermission('Settings.TaxCodes.View') ||
      this.auth.hasPermission('Accounting.View') ||
      this.auth.hasPermission('VIEW_FINANCE')
  );
  canCreate = computed(
    () =>
      this.auth.hasPermission('Settings.TaxCodes.Create') ||
      this.auth.hasPermission('Accounting.Create')
  );
  canEdit = computed(
    () =>
      this.auth.hasPermission('Settings.TaxCodes.Edit') ||
      this.auth.hasPermission('Accounting.Update')
  );
  canDelete = computed(
    () =>
      this.auth.hasPermission('Settings.TaxCodes.Delete') ||
      this.auth.hasPermission('Accounting.Delete')
  );

  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);
  isEditing = computed(() => !!this.editingId());

  filteredBranches = computed(() => {
    const companyId = this.filterCompanyId();
    const all = this.branches();
    if (!companyId) return all;
    return all.filter(b => b.companyId === companyId);
  });

  modalBranches = computed(() => {
    const companyId = this.formCompanyId();
    if (!companyId) return [] as OrgBranchLookup[];
    return this.branches().filter(b => b.companyId === companyId);
  });

  postingAccounts = computed(() =>
    this.accounts().filter(a => a.isPostingAllowed && !a.isSummaryAccount && a.isActive)
  );

  ratesFA = this.form.controls.rates as FormArray;

  t = (key: string) => this.lang.t(key);

  ngOnInit(): void {
    this.repo.getCompanies().subscribe({
      next: rows => this.companies.set(rows.filter(c => c.isActive !== false)),
      error: () => this.companies.set([])
    });
    this.repo.getBranches().subscribe({
      next: rows => this.branches.set(rows),
      error: () => this.branches.set([])
    });
    this.accountsRepo.getTree({ includeInactive: false }).subscribe({
      next: tree => this.accounts.set(flattenTreeAccounts(tree)),
      error: () => this.accounts.set([])
    });

    this.form.controls.companyId.valueChanges.subscribe(companyId => {
      this.formCompanyId.set(companyId || '');
      const branchId = this.form.controls.branchId.value;
      if (branchId && !this.branches().some(b => b.id === branchId && b.companyId === companyId)) {
        this.form.controls.branchId.setValue('');
      }
    });

    this.form.controls.calculationMethod.valueChanges.subscribe(method => {
      if (method === 2 || method === 3) {
        for (const ctrl of this.ratesFA.controls) {
          ctrl.get('rate')?.setValue(0, { emitEvent: false });
        }
      }
    });

    this.load();
  }

  @HostListener('document:keydown', ['$event'])
  onKey(e: KeyboardEvent): void {
    if (e.key === 'Escape' && this.showModal() && !this.saving()) {
      this.closeModal();
      return;
    }
    if (!e.ctrlKey && !e.metaKey) {
      if (e.key === 'F5') {
        e.preventDefault();
        this.load();
      }
      if (e.key === 'Delete' && !this.showModal() && this.selected() && this.canDelete()) {
        this.remove();
      }
      return;
    }
    const key = e.key.toLowerCase();
    if (key === 'n' && this.canCreate()) {
      e.preventDefault();
      this.openCreate();
    }
    if (key === 's' && this.showModal()) {
      e.preventDefault();
      this.closeAfterSave.set(e.shiftKey);
      this.save();
    }
    if (key === 'f') {
      e.preventDefault();
      document.querySelector<HTMLInputElement>('.search-box input')?.focus();
    }
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    const active = this.filterActive();
    this.repo
      .getList({
        search: this.search().trim() || undefined,
        companyId: this.filterCompanyId() || null,
        branchId: this.filterBranchId() || null,
        appliesTo: this.filterAppliesTo() ? Number(this.filterAppliesTo()) : null,
        isActive: active === '' ? null : active === 'true'
      })
      .subscribe({
        next: rows => {
          this.rows.set(rows);
          this.loading.set(false);
          if (this.selectedId() && !rows.some(r => r.id === this.selectedId())) {
            this.selectedId.set(null);
          }
        },
        error: err => {
          this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.taxCode.loadError'));
          this.loading.set(false);
        }
      });
  }

  resetFilters(): void {
    this.search.set('');
    this.filterCompanyId.set('');
    this.filterBranchId.set('');
    this.filterAppliesTo.set('');
    this.filterActive.set('');
    this.load();
  }

  select(row: TaxCode): void {
    this.selectedId.set(row.id);
  }

  appliesLabel(value: number | string): string {
    const item = this.appliesOptions.find(a => a.value === Number(value));
    return item ? this.t(item.labelKey) : String(value);
  }

  methodLabel(value: number | string): string {
    const item = this.methodOptions.find(m => m.value === Number(value));
    return item ? this.t(item.labelKey) : String(value);
  }

  formatDate(value?: string | null): string {
    if (!value) return '—';
    return value.slice(0, 16).replace('T', ' ');
  }

  accountLabel(id: string | null | undefined): string {
    if (!id) return '—';
    const a = this.accounts().find(x => x.id === id);
    return a ? `${a.accountNumber} — ${a.nameAr}` : id;
  }

  openCreate(): void {
    if (!this.canCreate()) return;
    this.editingId.set(null);
    this.error.set(null);
    this.closeAfterSave.set(false);
    this.selectedRateIndex.set(null);
    const company = this.companies()[0];
    this.ratesFA.clear();
    this.form.reset({
      companyId: company?.id ?? '',
      branchId: '',
      code: '',
      nameAr: '',
      nameEn: '',
      appliesTo: 3,
      calculationMethod: 1,
      salesAccountId: '',
      purchaseAccountId: '',
      priceIncludesTax: false,
      isActive: true
    });
    this.formCompanyId.set(company?.id ?? '');
    this.showModal.set(true);
  }

  openEdit(row?: TaxCode): void {
    const target = row ?? this.selected();
    if (!target || !this.canEdit()) return;
    this.editingId.set(target.id);
    this.error.set(null);
    this.closeAfterSave.set(false);
    this.selectedRateIndex.set(null);
    this.ratesFA.clear();
    for (const rate of target.rates ?? []) {
      this.ratesFA.push(this.createRateGroup(rate.fromDate, rate.toDate, rate.rate, rate.id));
    }
    this.form.reset({
      companyId: target.companyId,
      branchId: target.branchId ?? '',
      code: target.code,
      nameAr: target.nameAr,
      nameEn: target.nameEn ?? '',
      appliesTo: Number(target.appliesTo),
      calculationMethod: Number(target.calculationMethod),
      salesAccountId: target.salesAccountId ?? '',
      purchaseAccountId: target.purchaseAccountId ?? '',
      priceIncludesTax: target.priceIncludesTax,
      isActive: target.isActive
    });
    this.formCompanyId.set(target.companyId);
    this.showModal.set(true);
  }

  closeModal(): void {
    if (this.saving()) return;
    if (this.form.dirty && !confirm(this.t('fin.taxCode.confirmCancel'))) return;
    this.showModal.set(false);
  }

  createRateGroup(
    fromDate = new Date().toISOString().slice(0, 10),
    toDate: string | null | undefined = '',
    rate = 15,
    id: string | null | undefined = null
  ) {
    const method = this.form.controls.calculationMethod.value;
    const fixed = method === 2 || method === 3 ? 0 : rate;
    return this.fb.nonNullable.group({
      id: [id ?? ''],
      fromDate: [fromDate, Validators.required],
      toDate: [toDate ?? ''],
      rate: [fixed as number, [Validators.required, Validators.min(0), Validators.max(100)]]
    });
  }

  addRate(): void {
    this.ratesFA.push(this.createRateGroup());
    this.selectedRateIndex.set(this.ratesFA.length - 1);
  }

  removeSelectedRate(): void {
    const idx = this.selectedRateIndex();
    if (idx == null || idx < 0 || idx >= this.ratesFA.length) return;
    this.ratesFA.removeAt(idx);
    this.selectedRateIndex.set(null);
  }

  selectRate(index: number): void {
    this.selectedRateIndex.set(index);
  }

  buildPayload(): UpsertTaxCodePayload {
    const v = this.form.getRawValue();
    return {
      companyId: v.companyId,
      branchId: v.branchId || null,
      code: v.code.trim(),
      nameAr: v.nameAr.trim(),
      nameEn: v.nameEn.trim() || null,
      appliesTo: Number(v.appliesTo),
      calculationMethod: Number(v.calculationMethod),
      salesAccountId: v.salesAccountId || null,
      purchaseAccountId: v.purchaseAccountId || null,
      priceIncludesTax: v.priceIncludesTax,
      isActive: v.isActive,
      rates: this.ratesFA.controls.map(c => {
        const rv = c.getRawValue() as {
          id: string;
          fromDate: string;
          toDate: string;
          rate: number;
        };
        return {
          id: rv.id || null,
          fromDate: rv.fromDate,
          toDate: rv.toDate || null,
          rate: Number(rv.rate)
        };
      })
    };
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.error.set(this.t('fin.taxCode.validationRequired'));
      return;
    }
    if (this.hasOverlappingRates()) {
      this.error.set(this.t('fin.taxCode.rateOverlap'));
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    const payload = this.buildPayload();
    const id = this.editingId();
    const req = id ? this.repo.update(id, payload) : this.repo.create(payload);
    req.subscribe({
      next: saved => {
        this.saving.set(false);
        this.form.markAsPristine();
        this.load();
        this.selectedId.set(saved.id);
        if (this.closeAfterSave() || !id) {
          this.showModal.set(false);
        } else {
          this.openEdit(saved);
          this.form.markAsPristine();
        }
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.taxCode.saveError'));
        this.saving.set(false);
      }
    });
  }

  saveAndClose(): void {
    this.closeAfterSave.set(true);
    this.save();
  }

  remove(): void {
    const row = this.selected();
    if (!row || !this.canDelete()) return;
    if (row.hasBeenUsed) {
      this.error.set(this.t('fin.taxCode.inUse'));
      return;
    }
    if (!confirm(this.t('fin.taxCode.confirmDelete'))) return;
    this.repo.delete(row.id).subscribe({
      next: () => {
        this.selectedId.set(null);
        this.load();
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.taxCode.deleteError'));
      }
    });
  }

  private hasOverlappingRates(): boolean {
    const periods = this.ratesFA.controls.map(c => {
      const v = c.getRawValue() as { fromDate: string; toDate: string };
      return {
        from: v.fromDate,
        to: v.toDate || '9999-12-31'
      };
    });
    for (let i = 0; i < periods.length; i++) {
      for (let j = i + 1; j < periods.length; j++) {
        if (periods[i].from <= periods[j].to && periods[j].from <= periods[i].to) return true;
      }
    }
    return false;
  }
}
