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
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { GeneralLedgerRepository } from '../../../core/repositories/general-ledger.repository';
import { FinancialOpeningBalanceRepository } from '../../../core/repositories/financial-opening-balance.repository';
import { ChartOfAccountRepository } from '../../../core/repositories/chart-of-account.repository';
import { CostCenterRepository } from '../../../core/repositories/cost-center.repository';
import { CurrencyRepository } from '../../../core/repositories/currency.repository';
import {
  GL_POSTING_SOURCES,
  GeneralLedgerLine,
  GeneralLedgerResult,
  GlPostingSource
} from '../../../core/models/general-ledger.models';
import {
  FiscalPeriodLookup,
  OrgBranchLookup,
  OrgCompanyLookup
} from '../../../core/models/financial-opening-balance.models';
import { flattenTreeAccounts } from '../../finance/pages/coa-tree.util';

type AccountOption = { id: string; accountNumber: string; nameAr: string; nameEn?: string | null };
type CostCenterOption = { id: string; code: string; nameAr: string };
type CurrencyOption = { code: string; nameAr: string };

@Component({
  selector: 'app-general-ledger-page',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatTooltipModule],
  templateUrl: './general-ledger.page.html',
  styleUrl: './general-ledger.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GeneralLedgerPage implements OnInit {
  private repo = inject(GeneralLedgerRepository);
  private lookups = inject(FinancialOpeningBalanceRepository);
  private accountsRepo = inject(ChartOfAccountRepository);
  private costCentersRepo = inject(CostCenterRepository);
  private currencyRepo = inject(CurrencyRepository);
  private router = inject(Router);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  error = signal<string | null>(null);
  result = signal<GeneralLedgerResult | null>(null);

  search = signal('');
  companyId = signal('');
  branchId = signal('');
  fiscalYear = signal<number | ''>('');
  fiscalPeriodId = signal('');
  fromDate = signal(this.isoDate(new Date()));
  toDate = signal(this.isoDate(new Date()));
  accountId = signal('');
  costCenterId = signal('');
  sourceModule = signal<GlPostingSource | ''>('');
  documentNumber = signal('');
  currency = signal('');
  page = signal(1);
  pageSize = signal(50);

  companies = signal<OrgCompanyLookup[]>([]);
  branches = signal<OrgBranchLookup[]>([]);
  periods = signal<FiscalPeriodLookup[]>([]);
  accounts = signal<AccountOption[]>([]);
  costCenters = signal<CostCenterOption[]>([]);
  currencies = signal<CurrencyOption[]>([]);

  sources = GL_POSTING_SOURCES;

  canView = computed(
    () =>
      this.auth.hasPermission('Finance.GeneralLedger.View') ||
      this.auth.hasPermission('Reports.Accounting.View') ||
      this.auth.hasPermission('Accounting.View')
  );
  canExport = computed(
    () =>
      this.auth.hasPermission('Finance.GeneralLedger.Export') ||
      this.auth.hasPermission('Reports.Export') ||
      this.canView()
  );
  canPrint = computed(
    () =>
      this.auth.hasPermission('Finance.GeneralLedger.Print') ||
      this.canView()
  );
  canViewAllBranches = computed(
    () =>
      this.auth.hasPermission('Finance.GeneralLedger.ViewAllBranches') ||
      this.auth.hasPermission('Accounting.View')
  );

  lines = computed(() => this.result()?.lines ?? []);
  totalPages = computed(() => {
    const r = this.result();
    if (!r || r.pageSize <= 0) return 1;
    return Math.max(1, Math.ceil(r.totalCount / r.pageSize));
  });
  filteredBranches = computed(() => {
    const companyId = this.companyId();
    const all = this.branches();
    return companyId ? all.filter(b => b.companyId === companyId) : all;
  });
  fiscalYears = computed(() => {
    const years = [...new Set(this.periods().map(p => p.fiscalYear))].sort((a, b) => b - a);
    return years;
  });
  filteredPeriods = computed(() => {
    const year = this.fiscalYear();
    const all = this.periods();
    return year === '' ? all : all.filter(p => p.fiscalYear === year);
  });

  t = (key: string) => this.lang.t(key);

  ngOnInit(): void {
    if (!this.canView()) {
      this.error.set(this.t('fin.ops.gl.unauthorized'));
      return;
    }
    this.loadLookups();
    this.load();
  }

  apply(): void {
    this.page.set(1);
    this.load();
  }

  reset(): void {
    const today = this.isoDate(new Date());
    this.search.set('');
    this.companyId.set('');
    this.branchId.set('');
    this.fiscalYear.set('');
    this.fiscalPeriodId.set('');
    this.fromDate.set(today);
    this.toDate.set(today);
    this.accountId.set('');
    this.costCenterId.set('');
    this.sourceModule.set('');
    this.documentNumber.set('');
    this.currency.set('');
    this.page.set(1);
    this.load();
  }

  refresh(): void {
    this.load();
  }

  setPreset(days: number): void {
    const to = new Date();
    const from = new Date();
    from.setDate(to.getDate() - (days - 1));
    this.fromDate.set(this.isoDate(from));
    this.toDate.set(this.isoDate(to));
    this.page.set(1);
    this.load();
  }

  prevPage(): void {
    if (this.page() <= 1) return;
    this.page.update(p => p - 1);
    this.load();
  }

  nextPage(): void {
    if (this.page() >= this.totalPages()) return;
    this.page.update(p => p + 1);
    this.load();
  }

  changePageSize(size: number): void {
    this.pageSize.set(size);
    this.page.set(1);
    this.load();
  }

  sourceLabel(source?: number | null): string {
    if (source == null) return '—';
    const found = this.sources.find(s => s.value === source);
    return found ? this.t(found.labelKey) : String(source);
  }

  openSource(row: GeneralLedgerLine): void {
    if (row.isOpeningBalance) return;
    const docId = row.sourceDocumentId || row.journalEntryId;
    if (!docId) return;

    const source = row.sourceModule ?? 1;
    switch (source) {
      case 9:
        void this.router.navigate(['/finance-ops/receipt-vouchers'], { queryParams: { id: docId, view: 1 } });
        break;
      case 10:
        void this.router.navigate(['/finance-ops/payment-vouchers'], { queryParams: { id: docId, view: 1 } });
        break;
      case 8:
        void this.router.navigate(['/finance-ops/opening-balances'], { queryParams: { id: docId, view: 1 } });
        break;
      case 11:
      case 12:
        void this.router.navigate(['/finance-ops/debit-credit-notes'], { queryParams: { id: docId, view: 1 } });
        break;
      default:
        void this.router.navigate(['/finance-ops/journal-vouchers'], {
          queryParams: { id: row.journalEntryId ?? docId, view: 1 }
        });
        break;
    }
  }

  exportExcel(): void {
    if (!this.canExport()) return;
    const rows = this.lines();
    const header = [
      this.t('fin.ops.gl.col.date'),
      this.t('fin.ops.gl.col.docNumber'),
      this.t('fin.ops.gl.col.docType'),
      this.t('fin.ops.gl.col.reference'),
      this.t('fin.ops.gl.col.description'),
      this.t('fin.ops.gl.col.costCenter'),
      this.t('fin.ops.gl.col.debit'),
      this.t('fin.ops.gl.col.credit'),
      this.t('fin.ops.gl.col.runningBalance')
    ];
    const csv = [
      header.join(','),
      ...rows.map(r =>
        [
          r.postingDate,
          r.entryNumber,
          r.isOpeningBalance ? this.t('fin.ops.gl.openingBalance') : this.sourceLabel(r.sourceModule),
          r.reference ?? '',
          r.isOpeningBalance ? this.t('fin.ops.gl.openingBalance') : r.description,
          r.costCenterNameAr ?? '',
          r.debit,
          r.credit,
          r.runningBalance
        ]
          .map(v => `"${String(v).replace(/"/g, '""')}"`)
          .join(',')
      )
    ].join('\n');

    const blob = new Blob(['\ufeff' + csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `general-ledger-${this.fromDate()}_${this.toDate()}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  }

  exportPdf(): void {
    this.print();
  }

  print(): void {
    if (!this.canPrint()) return;
    window.print();
  }

  private load(): void {
    if (!this.canView()) return;
    this.loading.set(true);
    this.error.set(null);

    this.repo
      .inquire({
        accountId: this.accountId() || undefined,
        companyId: this.companyId() || undefined,
        branchId: this.canViewAllBranches() ? this.branchId() || undefined : this.branchId() || undefined,
        fiscalPeriodId: this.fiscalPeriodId() || undefined,
        fiscalYear: this.fiscalYear() === '' ? undefined : Number(this.fiscalYear()),
        fromDate: this.fromDate() || undefined,
        toDate: this.toDate() || undefined,
        costCenterId: this.costCenterId() || undefined,
        currency: this.currency() || undefined,
        sourceModule: this.sourceModule() === '' ? undefined : this.sourceModule(),
        documentNumber: this.documentNumber() || undefined,
        search: this.search() || undefined,
        includeOpeningBalance: !!this.accountId() && !!this.fromDate(),
        page: this.page(),
        pageSize: this.pageSize()
      })
      .subscribe({
        next: res => {
          this.result.set(res);
          this.loading.set(false);
        },
        error: err => {
          this.error.set(err?.error?.detail || err?.error?.error || err?.message || this.t('fin.ops.gl.loadFailed'));
          this.loading.set(false);
        }
      });
  }

  private loadLookups(): void {
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
        const flat = flattenTreeAccounts(tree).filter(a => a.isPostingAllowed && !a.isSummaryAccount);
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
      next: rows =>
        this.currencies.set(
          rows.filter(c => c.isActive).map(c => ({ code: c.code, nameAr: c.nameAr }))
        ),
      error: () => this.currencies.set([])
    });
  }

  private isoDate(d: Date): string {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }
}
