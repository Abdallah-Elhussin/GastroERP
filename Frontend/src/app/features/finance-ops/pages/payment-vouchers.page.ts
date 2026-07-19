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
import { ChartOfAccountRepository } from '../../../core/repositories/chart-of-account.repository';
import { CostCenterRepository } from '../../../core/repositories/cost-center.repository';
import { CurrencyRepository } from '../../../core/repositories/currency.repository';
import { GeneralLedgerSettingRepository } from '../../../core/repositories/general-ledger-setting.repository';
import { BranchRepository } from '../../../core/repositories/branch.repository';
import { DocumentTypeRepository } from '../../../core/repositories/document-type.repository';
import { CashBoxRepository } from '../../../core/repositories/cash-box.repository';
import { BankRepository } from '../../../core/repositories/bank.repository';
import {
  FinanceVoucherAccountOption,
  FinanceVoucherCostCenterOption,
  FinanceVoucherCurrencyOption,
  FinanceVoucherDetailsTotals
} from '../../../core/models/finance-voucher-details.models';
import { VoucherDetailsGridComponent } from '../components/voucher-details-grid/voucher-details-grid.component';
import { flattenTreeAccounts } from '../../finance/pages/coa-tree.util';

/** Local list row until Payment Voucher API is wired. */
export interface PaymentVoucherListRow {
  id: string;
  documentNumber: string;
  voucherDate: string;
  branchName: string;
  paymentType: string;
  beneficiary: string;
  source: string;
  localAmount: number;
  foreignAmount: number;
  status: 'Draft' | 'Posted' | 'Reversed';
}

@Component({
  selector: 'app-payment-vouchers-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatIconModule,
    MatTooltipModule,
    VoucherDetailsGridComponent
  ],
  templateUrl: './payment-vouchers.page.html',
  styleUrl: './payment-vouchers.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PaymentVouchersPage implements OnInit {
  private fb = inject(FormBuilder);
  lang = inject(LanguageService);
  auth = inject(AuthService);
  private accountsRepo = inject(ChartOfAccountRepository);
  private costCentersRepo = inject(CostCenterRepository);
  private currencyRepo = inject(CurrencyRepository);
  private glRepo = inject(GeneralLedgerSettingRepository);
  private branchRepo = inject(BranchRepository);
  private documentTypeRepo = inject(DocumentTypeRepository);
  private cashBoxRepo = inject(CashBoxRepository);
  private bankRepo = inject(BankRepository);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  rows = signal<PaymentVoucherListRow[]>([]);
  selectedId = signal<string | null>(null);
  search = signal('');
  showDeleted = signal(false);
  editorOpen = signal(false);
  editingId = signal<string | null>(null);
  selectedLineIndex = signal(-1);
  showAnalytical = signal(false);
  companyCurrency = signal('SAR');
  summary = signal<FinanceVoucherDetailsTotals | null>(null);

  accounts = signal<FinanceVoucherAccountOption[]>([]);
  costCenters = signal<FinanceVoucherCostCenterOption[]>([]);
  currencies = signal<FinanceVoucherCurrencyOption[]>([]);
  branches = signal<{ id: string; nameAr: string }[]>([]);
  documentTypes = signal<{ id: string; nameAr: string }[]>([]);
  cashBoxes = signal<{ id: string; nameAr: string }[]>([]);
  banks = signal<{ id: string; nameAr: string }[]>([]);

  paymentTypes = [
    { value: 'Cash', labelKey: 'fin.ops.pv.type.cash' },
    { value: 'Cheque', labelKey: 'fin.ops.pv.type.cheque' },
    { value: 'Transfer', labelKey: 'fin.ops.pv.type.transfer' },
    { value: 'Card', labelKey: 'fin.ops.pv.type.card' },
    { value: 'Wallet', labelKey: 'fin.ops.pv.type.wallet' }
  ] as const;

  headerForm = this.fb.nonNullable.group({
    branchId: ['', Validators.required],
    paymentType: ['Cash', Validators.required],
    cashBoxId: [''],
    bankId: [''],
    currency: ['SAR', Validators.required],
    documentTypeId: ['', Validators.required],
    documentNumber: [''],
    voucherDate: [new Date().toISOString().slice(0, 10), Validators.required],
    description: ['']
  });

  linesFA = this.fb.array([
    VoucherDetailsGridComponent.createLineGroup(this.fb, { currency: 'SAR', exchangeRate: 1 })
  ]);

  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);
  canCreate = computed(
    () =>
      this.auth.hasPermission('Accounting.Create') ||
      this.auth.hasPermission('Journal.Create') ||
      this.auth.hasPermission('Finance.ReceiptVouchers.Create')
  );

  t = (key: string) => this.lang.t(key);

  get linesArray(): FormArray {
    return this.linesFA;
  }

  ngOnInit(): void {
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
          this.headerForm.patchValue({ currency: company.code });
        }
      },
      error: () => this.currencies.set([])
    });

    this.branchRepo.getList().subscribe({
      next: rows => this.branches.set(rows.map(b => ({ id: b.id, nameAr: b.nameAr }))),
      error: () => this.branches.set([])
    });

    this.documentTypeRepo.getList().subscribe({
      next: rows => this.documentTypes.set(rows.map(d => ({ id: d.id, nameAr: d.nameAr }))),
      error: () => this.documentTypes.set([])
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
    // API not registered yet — keep empty operational list.
    this.rows.set([]);
    this.loading.set(false);
  }

  select(row: PaymentVoucherListRow): void {
    this.selectedId.set(row.id);
  }

  openCreate(): void {
    if (!this.canCreate()) return;
    this.editingId.set(null);
    this.error.set(null);
    this.selectedLineIndex.set(-1);
    this.headerForm.reset({
      branchId: this.branches()[0]?.id ?? '',
      paymentType: 'Cash',
      cashBoxId: '',
      bankId: '',
      currency: this.companyCurrency(),
      documentTypeId: this.documentTypes()[0]?.id ?? '',
      documentNumber: '',
      voucherDate: new Date().toISOString().slice(0, 10),
      description: ''
    });
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

  openEdit(): void {
    const row = this.selected();
    if (!row) return;
    this.error.set(this.t('fin.ops.pv.apiPending'));
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

  save(): void {
    if (this.headerForm.invalid || this.linesFA.length < 1 || this.linesFA.invalid) {
      this.headerForm.markAllAsTouched();
      this.linesFA.markAllAsTouched();
      this.error.set(this.t('fin.ops.voucher.validationRequired'));
      return;
    }
    this.error.set(this.t('fin.ops.pv.apiPending'));
  }

  isCashType(): boolean {
    return this.headerForm.controls.paymentType.value === 'Cash';
  }
}
