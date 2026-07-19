import {
  ChangeDetectionStrategy,
  Component,
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
import { BankRepository } from '../../../core/repositories/bank.repository';
import { CurrencyRepository } from '../../../core/repositories/currency.repository';
import { ChartOfAccountRepository } from '../../../core/repositories/chart-of-account.repository';
import { AccountClassificationRepository } from '../../../core/repositories/account-classification.repository';
import {
  Bank,
  OrgBranchLookup,
  OrgCompanyLookup,
  UpsertBankPayload
} from '../../../core/models/bank.models';
import { Currency } from '../../../core/models/currency.models';
import { ChartAccount } from '../../../core/models/chart-of-account.models';
import { flattenTreeAccounts } from './coa-tree.util';

@Component({
  selector: 'app-banks-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatIconModule, MatTooltipModule],
  templateUrl: './banks.page.html',
  styleUrl: './banks.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BanksPage implements OnInit {
  private repo = inject(BankRepository);
  private currenciesRepo = inject(CurrencyRepository);
  private accountsRepo = inject(ChartOfAccountRepository);
  private classificationsRepo = inject(AccountClassificationRepository);
  private fb = inject(FormBuilder);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  rows = signal<Bank[]>([]);
  companies = signal<OrgCompanyLookup[]>([]);
  branches = signal<OrgBranchLookup[]>([]);
  currencies = signal<Currency[]>([]);
  accounts = signal<ChartAccount[]>([]);
  bankOrCashClassIds = signal<Set<string>>(new Set());

  selectedId = signal<string | null>(null);
  search = signal('');
  filterCompanyId = signal('');
  filterBranchId = signal('');
  filterCurrencyId = signal('');
  showModal = signal(false);
  editingId = signal<string | null>(null);
  formCompanyId = signal('');

  form = this.fb.nonNullable.group({
    nameAr: ['', [Validators.required, Validators.maxLength(200)]],
    nameEn: ['', [Validators.maxLength(200)]],
    code: ['', [Validators.maxLength(30)]],
    swiftCode: ['', [Validators.maxLength(20)]],
    defaultIban: ['', [Validators.maxLength(50)]],
    companyId: ['', Validators.required],
    branchId: ['', Validators.required],
    chartOfAccountId: ['', Validators.required],
    baseCurrencyId: ['', Validators.required],
    isActive: [true],
    deactivatedAt: ['' as string],
    deactivationReason: ['', [Validators.maxLength(500)]],
    sortOrder: [0 as number],
    accounts: this.fb.array([])
  });

  canView = computed(
    () =>
      this.auth.hasPermission('Bank.View') ||
      this.auth.hasPermission('Accounting.View') ||
      this.auth.hasPermission('VIEW_FINANCE')
  );
  canCreate = computed(
    () => this.auth.hasPermission('Bank.Create') || this.auth.hasPermission('Accounting.Create')
  );
  canEdit = computed(
    () => this.auth.hasPermission('Bank.Update') || this.auth.hasPermission('Accounting.Update')
  );
  canDelete = computed(
    () => this.auth.hasPermission('Bank.Delete') || this.auth.hasPermission('Accounting.Delete')
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

  bankGlAccounts = computed(() => {
    const classIds = this.bankOrCashClassIds();
    return this.accounts().filter(a => {
      if (!a.isPostingAllowed || a.isSummaryAccount || !a.isActive) return false;
      if (Number(a.accountType) !== 1) return false;
      if (!a.accountClassificationId) return true;
      if (classIds.size === 0) return true;
      return classIds.has(a.accountClassificationId);
    });
  });

  accountsFA = this.form.controls.accounts as FormArray;

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
    this.currenciesRepo.getList().subscribe({
      next: rows => this.currencies.set(rows.filter(c => c.isActive)),
      error: () => this.currencies.set([])
    });
    this.accountsRepo.getTree({ includeInactive: false }).subscribe({
      next: tree => this.accounts.set(flattenTreeAccounts(tree)),
      error: () => this.accounts.set([])
    });
    this.classificationsRepo.getList().subscribe({
      next: list => {
        const ids = new Set(
          list.filter(c => ['bank', 'cash'].includes((c.code || '').toLowerCase())).map(c => c.id)
        );
        this.bankOrCashClassIds.set(ids);
      },
      error: () => this.bankOrCashClassIds.set(new Set())
    });

    this.form.controls.companyId.valueChanges.subscribe(companyId => {
      this.formCompanyId.set(companyId || '');
      const branchId = this.form.controls.branchId.value;
      if (branchId && !this.branches().some(b => b.id === branchId && b.companyId === companyId)) {
        this.form.controls.branchId.setValue('');
      }
    });

    this.form.controls.isActive.valueChanges.subscribe(active => {
      if (active) {
        this.form.controls.deactivatedAt.setValue('');
        this.form.controls.deactivationReason.setValue('');
      }
    });

    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo
      .getList({
        search: this.search().trim() || undefined,
        companyId: this.filterCompanyId() || null,
        branchId: this.filterBranchId() || null,
        currencyId: this.filterCurrencyId() || null
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
          this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.bank.loadError'));
          this.loading.set(false);
        }
      });
  }

  select(row: Bank): void {
    this.selectedId.set(row.id);
  }

  private accountRow() {
    return this.fb.nonNullable.group({
      id: [null as string | null],
      currencyId: ['', Validators.required],
      accountNumber: ['', [Validators.required, Validators.maxLength(50)]],
      iban: ['', Validators.maxLength(50)],
      minBalance: [null as number | null],
      maxBalance: [null as number | null],
      minTransaction: [null as number | null],
      maxTransaction: [null as number | null],
      dailyTransferLimit: [null as number | null],
      allowExceedLimits: [false],
      allowWithdraw: [true],
      allowDeposit: [true],
      allowTransfer: [true],
      isDefault: [false],
      isActive: [true],
      sortOrder: [0 as number]
    });
  }

  addAccountRow(preferDefault = false): void {
    const g = this.accountRow();
    const companyCurrency = this.currencies().find(c => c.isCompanyCurrency);
    if (companyCurrency) g.controls.currencyId.setValue(companyCurrency.id);
    if (preferDefault || this.accountsFA.length === 0) g.controls.isDefault.setValue(true);
    this.accountsFA.push(g);
  }

  removeAccountRow(index: number): void {
    this.accountsFA.removeAt(index);
  }

  clearAccounts(): void {
    while (this.accountsFA.length) this.accountsFA.removeAt(0);
  }

  openCreate(): void {
    if (!this.canCreate()) return;
    this.editingId.set(null);
    this.error.set(null);
    this.clearAccounts();
    const company = this.companies()[0];
    const companyBranches = company
      ? this.branches().filter(b => b.companyId === company.id)
      : [];
    const sar = this.currencies().find(c => c.code === 'SAR') ?? this.currencies().find(c => c.isCompanyCurrency);
    this.form.reset({
      nameAr: '',
      nameEn: '',
      code: '',
      swiftCode: '',
      defaultIban: '',
      companyId: company?.id ?? '',
      branchId: companyBranches[0]?.id ?? '',
      chartOfAccountId: '',
      baseCurrencyId: sar?.id ?? '',
      isActive: true,
      deactivatedAt: '',
      deactivationReason: '',
      sortOrder: 0
    });
    this.formCompanyId.set(company?.id ?? '');
    this.addAccountRow(true);
    this.showModal.set(true);
  }

  openEdit(): void {
    const row = this.selected();
    if (!row || !this.canEdit()) return;
    this.editingId.set(row.id);
    this.error.set(null);
    this.clearAccounts();
    this.form.reset({
      nameAr: row.nameAr,
      nameEn: row.nameEn ?? '',
      code: row.code ?? '',
      swiftCode: row.swiftCode ?? '',
      defaultIban: row.defaultIban ?? '',
      companyId: row.companyId,
      branchId: row.branchId,
      chartOfAccountId: row.chartOfAccountId,
      baseCurrencyId: row.baseCurrencyId,
      isActive: row.isActive,
      deactivatedAt: row.deactivatedAt ?? '',
      deactivationReason: row.deactivationReason ?? '',
      sortOrder: row.sortOrder
    });
    this.formCompanyId.set(row.companyId);
    const accounts = row.accounts?.length ? row.accounts : [];
    if (accounts.length === 0) {
      this.addAccountRow(true);
    } else {
      for (const a of accounts) {
        const g = this.accountRow();
        g.reset({
          id: a.id ?? null,
          currencyId: a.currencyId,
          accountNumber: a.accountNumber,
          iban: a.iban ?? '',
          minBalance: a.minBalance ?? null,
          maxBalance: a.maxBalance ?? null,
          minTransaction: a.minTransaction ?? null,
          maxTransaction: a.maxTransaction ?? null,
          dailyTransferLimit: a.dailyTransferLimit ?? null,
          allowExceedLimits: a.allowExceedLimits,
          allowWithdraw: a.allowWithdraw,
          allowDeposit: a.allowDeposit,
          allowTransfer: a.allowTransfer,
          isDefault: a.isDefault,
          isActive: a.isActive,
          sortOrder: a.sortOrder
        });
        this.accountsFA.push(g);
      }
    }
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
    this.editingId.set(null);
  }

  private toNullableNumber(v: unknown): number | null {
    if (v === null || v === undefined || v === '') return null;
    const n = Number(v);
    return Number.isFinite(n) ? n : null;
  }

  save(): void {
    if (this.form.invalid || this.accountsFA.invalid) {
      this.form.markAllAsTouched();
      this.accountsFA.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    const payload: UpsertBankPayload = {
      nameAr: raw.nameAr.trim(),
      nameEn: raw.nameEn.trim() || null,
      code: raw.code.trim() || null,
      swiftCode: raw.swiftCode.trim() || null,
      defaultIban: raw.defaultIban.trim() || null,
      companyId: raw.companyId,
      branchId: raw.branchId,
      chartOfAccountId: raw.chartOfAccountId,
      baseCurrencyId: raw.baseCurrencyId,
      isActive: raw.isActive,
      deactivatedAt: raw.isActive ? null : raw.deactivatedAt || null,
      deactivationReason: raw.isActive ? null : raw.deactivationReason.trim() || null,
      sortOrder: Number(raw.sortOrder) || 0,
      accounts: this.accountsFA.controls.map((c, i) => {
        const v = c.getRawValue() as Record<string, unknown>;
        return {
          id: (v['id'] as string | null) ?? null,
          currencyId: String(v['currencyId']),
          accountNumber: String(v['accountNumber']).trim(),
          iban: String(v['iban'] ?? '').trim() || null,
          minBalance: this.toNullableNumber(v['minBalance']),
          maxBalance: this.toNullableNumber(v['maxBalance']),
          minTransaction: this.toNullableNumber(v['minTransaction']),
          maxTransaction: this.toNullableNumber(v['maxTransaction']),
          dailyTransferLimit: this.toNullableNumber(v['dailyTransferLimit']),
          allowExceedLimits: !!v['allowExceedLimits'],
          allowWithdraw: !!v['allowWithdraw'],
          allowDeposit: !!v['allowDeposit'],
          allowTransfer: !!v['allowTransfer'],
          isDefault: !!v['isDefault'],
          isActive: !!v['isActive'],
          sortOrder: Number(v['sortOrder']) || i
        };
      })
    };

    this.saving.set(true);
    this.error.set(null);
    const req = this.editingId()
      ? this.repo.update(this.editingId()!, payload)
      : this.repo.create(payload);

    req.subscribe({
      next: saved => {
        this.saving.set(false);
        this.afterSave(saved.id);
      },
      error: err => this.fail(err)
    });
  }

  private afterSave(id: string): void {
    this.closeModal();
    this.selectedId.set(id);
    this.load();
  }

  private fail(err: { error?: { detail?: string; error?: string } }): void {
    this.saving.set(false);
    this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.bank.saveError'));
  }

  remove(): void {
    const row = this.selected();
    if (!row || !this.canDelete() || row.isSystem) return;
    if (!confirm(this.t('fin.bank.confirmDelete'))) return;
    this.repo.delete(row.id).subscribe({
      next: () => {
        this.selectedId.set(null);
        this.load();
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.bank.deleteError'));
      }
    });
  }

  glLabel(a: ChartAccount): string {
    return `${a.accountNumber} — ${a.nameAr}`;
  }
}
