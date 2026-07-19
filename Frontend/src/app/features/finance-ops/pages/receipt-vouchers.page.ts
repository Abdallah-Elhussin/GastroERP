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
import { ReceiptVoucherRepository } from '../../../core/repositories/receipt-voucher.repository';
import { FinancialOpeningBalanceRepository } from '../../../core/repositories/financial-opening-balance.repository';
import { ChartOfAccountRepository } from '../../../core/repositories/chart-of-account.repository';
import { CostCenterRepository } from '../../../core/repositories/cost-center.repository';
import { CurrencyRepository } from '../../../core/repositories/currency.repository';
import { GeneralLedgerSettingRepository } from '../../../core/repositories/general-ledger-setting.repository';
import { CashBoxRepository } from '../../../core/repositories/cash-box.repository';
import { BankRepository } from '../../../core/repositories/bank.repository';
import {
  FinanceVoucherAccountOption,
  FinanceVoucherCostCenterOption,
  FinanceVoucherCurrencyOption,
  FinanceVoucherDetailsTotals,
  FinanceVoucherLineFormValue
} from '../../../core/models/finance-voucher-details.models';
import {
  RECEIPT_METHODS,
  RECEIPT_PARTY_TYPES,
  RECEIPT_STATUSES,
  ReceiptMethod,
  ReceiptVoucher,
  UpsertReceiptVoucherPayload
} from '../../../core/models/receipt-voucher.models';
import { FiscalPeriodLookup, OrgBranchLookup, OrgCompanyLookup } from '../../../core/models/financial-opening-balance.models';
import { VoucherDetailsGridComponent } from '../components/voucher-details-grid/voucher-details-grid.component';
import { flattenTreeAccounts } from '../../finance/pages/coa-tree.util';

@Component({
  selector: 'app-receipt-vouchers-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatIconModule,
    MatTooltipModule,
    VoucherDetailsGridComponent
  ],
  templateUrl: './receipt-vouchers.page.html',
  styleUrl: './receipt-vouchers.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReceiptVouchersPage implements OnInit {
  private repo = inject(ReceiptVoucherRepository);
  private lookups = inject(FinancialOpeningBalanceRepository);
  private fb = inject(FormBuilder);
  lang = inject(LanguageService);
  auth = inject(AuthService);
  private accountsRepo = inject(ChartOfAccountRepository);
  private costCentersRepo = inject(CostCenterRepository);
  private currencyRepo = inject(CurrencyRepository);
  private glRepo = inject(GeneralLedgerSettingRepository);
  private cashBoxRepo = inject(CashBoxRepository);
  private bankRepo = inject(BankRepository);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  rows = signal<ReceiptVoucher[]>([]);
  selectedId = signal<string | null>(null);
  search = signal('');
  filterStatus = signal('');
  filterMethod = signal('');
  fromDate = signal('');
  toDate = signal('');
  showDeleted = signal(false);
  editorOpen = signal(false);
  editingId = signal<string | null>(null);
  viewOnly = signal(false);
  selectedLineIndex = signal(-1);
  showAnalytical = signal(false);
  companyCurrency = signal('SAR');
  summary = signal<FinanceVoucherDetailsTotals | null>(null);

  companies = signal<OrgCompanyLookup[]>([]);
  branches = signal<OrgBranchLookup[]>([]);
  periods = signal<FiscalPeriodLookup[]>([]);
  accounts = signal<FinanceVoucherAccountOption[]>([]);
  costCenters = signal<FinanceVoucherCostCenterOption[]>([]);
  currencies = signal<FinanceVoucherCurrencyOption[]>([]);
  cashBoxes = signal<{ id: string; nameAr: string }[]>([]);
  banks = signal<{ id: string; nameAr: string }[]>([]);

  methods = RECEIPT_METHODS;
  statuses = RECEIPT_STATUSES;
  partyTypes = RECEIPT_PARTY_TYPES;

  headerForm = this.fb.nonNullable.group({
    companyId: ['', Validators.required],
    branchId: ['', Validators.required],
    fiscalPeriodId: ['', Validators.required],
    receiptMethod: [1 as ReceiptMethod, Validators.required],
    cashBoxId: [''],
    bankId: [''],
    partyType: [2 as number, Validators.required],
    partyName: ['', Validators.required],
    currency: ['SAR', Validators.required],
    exchangeRate: [1, [Validators.required, Validators.min(0.000001)]],
    voucherDate: [new Date().toISOString().slice(0, 10), Validators.required],
    reference: [''],
    chequeNumber: [''],
    chequeDate: [''],
    description: [''],
    notes: ['']
  });

  linesFA = this.fb.array([
    VoucherDetailsGridComponent.createLineGroup(this.fb, { currency: 'SAR', exchangeRate: 1 })
  ]);

  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);
  modalBranches = computed(() => {
    const companyId = this.headerForm.controls.companyId.value;
    return this.branches().filter(b => !companyId || b.companyId === companyId);
  });

  canView = computed(
    () =>
      this.auth.hasPermission('Finance.ReceiptVouchers.View') ||
      this.auth.hasPermission('Accounting.View')
  );
  canCreate = computed(
    () =>
      this.auth.hasPermission('Finance.ReceiptVouchers.Create') ||
      this.auth.hasPermission('Accounting.Create')
  );
  canEdit = computed(
    () =>
      this.auth.hasPermission('Finance.ReceiptVouchers.Edit') ||
      this.auth.hasPermission('Accounting.Update')
  );
  canDelete = computed(() => this.auth.hasPermission('Finance.ReceiptVouchers.Delete'));
  canApprove = computed(() => this.auth.hasPermission('Finance.ReceiptVouchers.Approve'));
  canPost = computed(() => this.auth.hasPermission('Finance.ReceiptVouchers.Post'));
  canReverse = computed(() => this.auth.hasPermission('Finance.ReceiptVouchers.Reverse'));

  t = (key: string) => this.lang.t(key);
  get linesArray(): FormArray {
    return this.linesFA;
  }

  ngOnInit(): void {
    this.lookups.getCompanies().subscribe({
      next: rows => this.companies.set(rows.filter(c => c.isActive !== false)),
      error: () => this.companies.set([])
    });
    this.lookups.getBranches().subscribe({
      next: rows => this.branches.set(rows),
      error: () => this.branches.set([])
    });
    this.lookups.getFiscalPeriods().subscribe({
      next: rows => this.periods.set(rows),
      error: () => this.periods.set([])
    });
    this.accountsRepo.getTree({ includeInactive: false }).subscribe({
      next: tree => {
        const flat = flattenTreeAccounts(tree).filter(
          a => a.isPostingAllowed && !a.isSummaryAccount && a.isActive
        );
        this.accounts.set(
          flat.map(a => ({
            id: a.id,
            accountNumber: a.accountNumber,
            nameAr: a.nameAr,
            nameEn: a.nameEn
          }))
        );
      },
      error: () => this.accounts.set([])
    });
    this.costCentersRepo.getList().subscribe({
      next: rows =>
        this.costCenters.set(rows.map(c => ({ id: c.id, code: c.code, nameAr: c.nameAr }))),
      error: () => this.costCenters.set([])
    });
    this.currencyRepo.getList().subscribe({
      next: rows => {
        const active = rows.filter(c => c.isActive);
        this.currencies.set(
          active.map(c => ({
            code: c.code,
            nameAr: c.nameAr,
            currentExchangeRate: c.currentExchangeRate,
            isCompanyCurrency: c.isCompanyCurrency,
            decimalPlaces: c.decimalPlaces
          }))
        );
        const company = active.find(c => c.isCompanyCurrency);
        if (company) {
          this.companyCurrency.set(company.code);
          this.headerForm.patchValue({ currency: company.code, exchangeRate: 1 });
        }
      },
      error: () => this.currencies.set([])
    });
    this.cashBoxRepo.getList().subscribe({
      next: rows => this.cashBoxes.set(rows.map(c => ({ id: c.id, nameAr: c.nameAr }))),
      error: () => this.cashBoxes.set([])
    });
    this.bankRepo.getList().subscribe({
      next: rows => this.banks.set(rows.map(b => ({ id: b.id, nameAr: b.nameAr }))),
      error: () => this.banks.set([])
    });
    this.glRepo.getList({ pageSize: 1 }).subscribe({
      next: rows => this.showAnalytical.set(!!rows[0]?.useAnalyticalAccounts),
      error: () => this.showAnalytical.set(false)
    });

    this.headerForm.controls.companyId.valueChanges.subscribe(id => {
      const branchId = this.headerForm.controls.branchId.value;
      if (branchId && !this.branches().some(b => b.id === branchId && b.companyId === id)) {
        this.headerForm.controls.branchId.setValue('');
      }
    });
    this.headerForm.controls.currency.valueChanges.subscribe(code => {
      const cur = this.currencies().find(c => c.code === code);
      if (code === this.companyCurrency()) this.headerForm.controls.exchangeRate.setValue(1);
      else if (cur) this.headerForm.controls.exchangeRate.setValue(cur.currentExchangeRate || 1);
    });

    this.load();
  }

  @HostListener('document:keydown', ['$event'])
  onKey(e: KeyboardEvent): void {
    if (e.key === 'Escape' && this.editorOpen() && !this.saving()) this.closeEditor();
    if (e.key === 'F5') {
      e.preventDefault();
      this.load();
    }
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo
      .getList({
        search: this.search().trim() || undefined,
        status: this.filterStatus() ? Number(this.filterStatus()) : null,
        receiptMethod: this.filterMethod() ? Number(this.filterMethod()) : null,
        fromDate: this.fromDate() || null,
        toDate: this.toDate() || null
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
          this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.rv.loadError'));
          this.loading.set(false);
        }
      });
  }

  select(row: ReceiptVoucher): void {
    this.selectedId.set(row.id);
  }

  openCreate(): void {
    if (!this.canCreate()) return;
    this.editingId.set(null);
    this.viewOnly.set(false);
    this.error.set(null);
    this.selectedLineIndex.set(-1);
    const company = this.companies()[0];
    const branch = this.branches().find(b => b.companyId === company?.id) ?? this.branches()[0];
    const period = this.periods()[0];
    this.headerForm.reset({
      companyId: company?.id ?? '',
      branchId: branch?.id ?? '',
      fiscalPeriodId: period?.id ?? '',
      receiptMethod: 1,
      cashBoxId: '',
      bankId: '',
      partyType: 2,
      partyName: '',
      currency: this.companyCurrency(),
      exchangeRate: 1,
      voucherDate: new Date().toISOString().slice(0, 10),
      reference: '',
      chequeNumber: '',
      chequeDate: '',
      description: '',
      notes: ''
    });
    this.headerForm.enable();
    while (this.linesFA.length) this.linesFA.removeAt(0);
    this.linesFA.push(
      VoucherDetailsGridComponent.createLineGroup(
        this.fb,
        { currency: this.companyCurrency(), exchangeRate: 1 },
        this.companyCurrency()
      )
    );
    this.editorOpen.set(true);
  }

  openEdit(row?: ReceiptVoucher): void {
    const target = row ?? this.selected();
    if (!target) return;
    this.loading.set(true);
    this.repo.getById(target.id).subscribe({
      next: doc => {
        this.editingId.set(doc.id);
        this.viewOnly.set(doc.status !== 1 && doc.status !== 2);
        this.error.set(null);
        this.selectedLineIndex.set(-1);
        this.headerForm.reset({
          companyId: doc.companyId,
          branchId: doc.branchId,
          fiscalPeriodId: doc.fiscalPeriodId,
          receiptMethod: doc.receiptMethod,
          cashBoxId: doc.cashBoxId ?? '',
          bankId: doc.bankId ?? '',
          partyType: doc.partyType,
          partyName: doc.partyName ?? '',
          currency: doc.currency,
          exchangeRate: doc.exchangeRate,
          voucherDate: doc.voucherDate,
          reference: doc.reference ?? '',
          chequeNumber: doc.chequeNumber ?? '',
          chequeDate: doc.chequeDate ?? '',
          description: doc.description ?? '',
          notes: doc.notes ?? ''
        });
        while (this.linesFA.length) this.linesFA.removeAt(0);
        for (const line of doc.lines) {
          this.linesFA.push(
            VoucherDetailsGridComponent.createLineGroup(
              this.fb,
              {
                id: line.id,
                chartOfAccountId: line.chartOfAccountId,
                costCenterId: line.costCenterId ?? '',
                analyticalAccountId: line.analyticalAccountId ?? '',
                currency: line.currency,
                exchangeRate: line.exchangeRate,
                amount: line.amount,
                description: line.description ?? ''
              },
              this.companyCurrency()
            )
          );
        }
        if (!doc.lines.length) {
          this.linesFA.push(
            VoucherDetailsGridComponent.createLineGroup(
              this.fb,
              { currency: doc.currency, exchangeRate: doc.exchangeRate },
              this.companyCurrency()
            )
          );
        }
        if (this.viewOnly()) this.headerForm.disable();
        else this.headerForm.enable();
        this.editorOpen.set(true);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.rv.loadError'));
        this.loading.set(false);
      }
    });
  }

  closeEditor(): void {
    if (this.saving()) return;
    this.editorOpen.set(false);
  }

  onLineSelected(index: number): void {
    this.selectedLineIndex.set(index);
  }

  onTotals(totals: FinanceVoucherDetailsTotals): void {
    this.summary.set(totals);
  }

  needsCashBox(): boolean {
    const m = Number(this.headerForm.controls.receiptMethod.value);
    return m === 1 || m === 6 || m === 7;
  }

  needsBank(): boolean {
    const m = Number(this.headerForm.controls.receiptMethod.value);
    return m === 2 || m === 3 || m === 4 || m === 5 || m === 6 || m === 7;
  }

  isCheque(): boolean {
    return Number(this.headerForm.controls.receiptMethod.value) === 3;
  }

  save(): void {
    if (this.viewOnly()) return;
    if (this.headerForm.invalid || this.linesFA.length < 1 || this.linesFA.invalid) {
      this.headerForm.markAllAsTouched();
      this.linesFA.markAllAsTouched();
      this.error.set(this.t('fin.ops.voucher.validationRequired'));
      return;
    }

    const v = this.headerForm.getRawValue();
    const method = Number(v.receiptMethod) as ReceiptMethod;
    const payload: UpsertReceiptVoucherPayload = {
      companyId: v.companyId,
      branchId: v.branchId,
      voucherDate: v.voucherDate,
      fiscalPeriodId: v.fiscalPeriodId,
      receiptMethod: method,
      partyType: Number(v.partyType) as 1 | 2 | 3,
      cashBoxId:
        method === 1 || ((method === 6 || method === 7) && !!v.cashBoxId)
          ? v.cashBoxId || null
          : null,
      bankId:
        method === 1
          ? null
          : method === 6 || method === 7
            ? v.cashBoxId
              ? null
              : v.bankId || null
            : v.bankId || null,
      partyId: null,
      partyName: v.partyName.trim(),
      currency: v.currency,
      exchangeRate: Number(v.exchangeRate) || 1,
      costCenterId: null,
      reference: v.reference.trim() || null,
      description: v.description.trim() || null,
      notes: v.notes.trim() || null,
      chequeNumber: method === 3 ? v.chequeNumber.trim() || null : null,
      chequeDate: method === 3 && v.chequeDate ? v.chequeDate : null,
      lines: this.linesFA.controls.map(c => {
        const lv = c.getRawValue() as FinanceVoucherLineFormValue;
        return {
          chartOfAccountId: lv.chartOfAccountId,
          costCenterId: lv.costCenterId || null,
          analyticalAccountId: lv.analyticalAccountId || null,
          currency: lv.currency,
          exchangeRate: Number(lv.exchangeRate) || 1,
          amount: Number(lv.amount) || 0,
          description: lv.description?.trim() || null
        };
      })
    };

    if (method === 1 && !payload.cashBoxId) {
      this.error.set(this.t('fin.ops.rv.cashBoxRequired'));
      return;
    }
    if ([2, 3, 4, 5].includes(method) && !payload.bankId) {
      this.error.set(this.t('fin.ops.rv.bankRequired'));
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    const id = this.editingId();
    const req = id ? this.repo.update(id, payload) : this.repo.create(payload);
    req.subscribe({
      next: saved => {
        this.saving.set(false);
        this.editorOpen.set(false);
        this.selectedId.set(saved.id);
        this.load();
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.rv.saveError'));
        this.saving.set(false);
      }
    });
  }

  approveSelected(): void {
    const row = this.selected();
    if (!row || !this.canApprove() || (row.status !== 1 && row.status !== 2)) return;
    this.repo.approve(row.id).subscribe({
      next: () => this.load(),
      error: err =>
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.rv.saveError'))
    });
  }

  postSelected(): void {
    const row = this.selected();
    if (!row || !this.canPost() || (row.status !== 1 && row.status !== 3)) return;
    this.repo.post(row.id).subscribe({
      next: () => this.load(),
      error: err =>
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.rv.saveError'))
    });
  }

  reverseSelected(): void {
    const row = this.selected();
    if (!row || !this.canReverse() || row.status !== 4) return;
    this.repo.reverse(row.id).subscribe({
      next: () => this.load(),
      error: err =>
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.rv.saveError'))
    });
  }

  remove(): void {
    const row = this.selected();
    if (!row || !this.canDelete() || row.status !== 1) return;
    if (!confirm(this.t('fin.ops.rv.confirmDelete'))) return;
    this.repo.delete(row.id).subscribe({
      next: () => {
        this.selectedId.set(null);
        this.load();
      },
      error: err =>
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.rv.saveError'))
    });
  }

  statusLabel(status: number): string {
    return this.t(this.statuses.find(s => s.value === status)?.labelKey ?? 'fin.ops.rv.status.draft');
  }

  methodLabel(method: number): string {
    return this.t(this.methods.find(m => m.value === method)?.labelKey ?? 'fin.ops.rv.method.cash');
  }

  sourceLabel(row: ReceiptVoucher): string {
    return row.cashBoxNameAr || row.bankNameAr || '—';
  }

  setQuickRange(days: number): void {
    const to = new Date();
    const from = new Date();
    from.setDate(to.getDate() - (days - 1));
    this.fromDate.set(from.toISOString().slice(0, 10));
    this.toDate.set(to.toISOString().slice(0, 10));
    this.load();
  }

  resetFilters(): void {
    this.search.set('');
    this.filterStatus.set('');
    this.filterMethod.set('');
    this.fromDate.set('');
    this.toDate.set('');
    this.load();
  }
}
