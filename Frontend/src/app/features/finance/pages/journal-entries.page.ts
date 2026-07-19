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
import { JournalEntryRepository } from '../../../core/repositories/journal-entry.repository';
import { ChartOfAccountRepository } from '../../../core/repositories/chart-of-account.repository';
import { CostCenterRepository } from '../../../core/repositories/cost-center.repository';
import { CurrencyRepository } from '../../../core/repositories/currency.repository';
import { GeneralLedgerSettingRepository } from '../../../core/repositories/general-ledger-setting.repository';
import {
  JOURNAL_STATUSES,
  JOURNAL_VOUCHER_TYPES,
  CreateJournalPayload,
  JournalEntry,
  UpdateJournalPayload
} from '../../../core/models/journal-entry.models';
import {
  FiscalPeriodLookup,
  OrgBranchLookup,
  OrgCompanyLookup
} from '../../../core/models/financial-opening-balance.models';
import { ChartAccount } from '../../../core/models/chart-of-account.models';
import { flattenTreeAccounts } from './coa-tree.util';

@Component({
  selector: 'app-journal-entries-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatIconModule, MatTooltipModule],
  templateUrl: './journal-entries.page.html',
  styleUrl: './journal-entries.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class JournalEntriesPage implements OnInit {
  private repo = inject(JournalEntryRepository);
  private accountsRepo = inject(ChartOfAccountRepository);
  private costCentersRepo = inject(CostCenterRepository);
  private currencyRepo = inject(CurrencyRepository);
  private glRepo = inject(GeneralLedgerSettingRepository);
  private fb = inject(FormBuilder);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  rows = signal<JournalEntry[]>([]);
  companies = signal<OrgCompanyLookup[]>([]);
  branches = signal<OrgBranchLookup[]>([]);
  periods = signal<FiscalPeriodLookup[]>([]);
  accounts = signal<ChartAccount[]>([]);
  costCenters = signal<{ id: string; code?: string; nameAr: string }[]>([]);
  currencies = signal<{ code: string; nameAr: string; rate: number }[]>([]);
  showAnalytical = signal(false);
  companyCurrency = signal('SAR');

  selectedId = signal<string | null>(null);
  search = signal('');
  filterStatus = signal('');
  filterVoucherType = signal('');
  filterCompanyId = signal('');
  filterBranchId = signal('');
  filterFiscalYear = signal<number | ''>('');
  filterFiscalPeriodId = signal('');
  fromDate = signal(this.isoDate(new Date()));
  toDate = signal(this.isoDate(new Date()));
  showModal = signal(false);
  editingId = signal<string | null>(null);
  formCompanyId = signal('');
  viewOnly = signal(false);
  editorEntryNumber = signal('');
  editorStatus = signal(1);
  selectedLineIndex = signal(-1);

  statuses = JOURNAL_STATUSES;
  voucherTypes = JOURNAL_VOUCHER_TYPES;
  selectableTypes = JOURNAL_VOUCHER_TYPES.filter(t => t.selectable);

  form = this.fb.nonNullable.group({
    companyId: ['', Validators.required],
    branchId: [''],
    fiscalPeriodId: [''],
    voucherType: [1 as number, Validators.required],
    postingDate: [new Date().toISOString().slice(0, 10), Validators.required],
    description: ['', Validators.required],
    reference: [''],
    lines: this.fb.array([])
  });

  canView = computed(
    () =>
      this.auth.hasPermission('Journal.View') ||
      this.auth.hasPermission('Accounting.View') ||
      this.auth.hasPermission('VIEW_FINANCE')
  );
  canCreate = computed(
    () =>
      this.auth.hasPermission('Journal.Create') || this.auth.hasPermission('Accounting.Create')
  );
  canEdit = computed(
    () =>
      this.auth.hasPermission('Journal.Edit') ||
      this.auth.hasPermission('Journal.Create') ||
      this.auth.hasPermission('Accounting.Update')
  );
  canApprove = computed(
    () =>
      this.auth.hasPermission('Journal.Approve') ||
      this.auth.hasPermission('Journal.Post') ||
      this.auth.hasPermission('Accounting.Update')
  );
  canPost = computed(
    () => this.auth.hasPermission('Journal.Post') || this.auth.hasPermission('Accounting.Update')
  );
  canReverse = computed(
    () =>
      this.auth.hasPermission('Journal.Reverse') || this.auth.hasPermission('Accounting.Update')
  );
  canDelete = computed(
    () =>
      this.auth.hasPermission('Journal.Delete') ||
      this.auth.hasPermission('Journal.Create') ||
      this.auth.hasPermission('Accounting.Delete')
  );
  canExport = computed(
    () =>
      this.auth.hasPermission('Journal.Export') || this.auth.hasPermission('Journal.View')
  );
  canPrint = computed(
    () => this.auth.hasPermission('Journal.Print') || this.auth.hasPermission('Journal.View')
  );

  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);
  isEditing = computed(() => !!this.editingId());
  isDraftSelected = computed(() => Number(this.selected()?.status) === 1);
  isApprovedSelected = computed(() => Number(this.selected()?.status) === 4);
  isPostedSelected = computed(() => Number(this.selected()?.status) === 2);
  isManualDraft = computed(
    () => this.isDraftSelected() && Number(this.selected()?.sourceModule) === 1
  );

  filteredBranches = computed(() => {
    const companyId = this.filterCompanyId() || this.formCompanyId();
    const all = this.branches();
    return companyId ? all.filter(b => b.companyId === companyId) : all;
  });
  modalBranches = computed(() => {
    const companyId = this.formCompanyId();
    return companyId ? this.branches().filter(b => b.companyId === companyId) : this.branches();
  });
  fiscalYears = computed(() =>
    [...new Set(this.periods().map(p => p.fiscalYear))].sort((a, b) => b - a)
  );
  filteredPeriods = computed(() => {
    const year = this.filterFiscalYear();
    return year === '' ? this.periods() : this.periods().filter(p => p.fiscalYear === year);
  });
  modalPeriods = computed(() => this.periods());

  postingAccounts = computed(() =>
    this.accounts().filter(a => a.isPostingAllowed && !a.isSummaryAccount && a.isActive)
  );

  linesFA = this.form.controls.lines as FormArray;
  totalDebit = computed(() =>
    this.linesFA.controls.reduce((s, c) => s + Number(c.get('debit')?.value || 0), 0)
  );
  totalCredit = computed(() =>
    this.linesFA.controls.reduce((s, c) => s + Number(c.get('credit')?.value || 0), 0)
  );
  balanceDiff = computed(() => this.totalDebit() - this.totalCredit());
  isBalanced = computed(() => Math.abs(this.balanceDiff()) < 0.005 && this.totalDebit() > 0);

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
    this.repo.getFiscalPeriods().subscribe({
      next: rows => this.periods.set(rows),
      error: () => this.periods.set([])
    });
    this.accountsRepo.getTree({ includeInactive: false }).subscribe({
      next: tree => this.accounts.set(flattenTreeAccounts(tree)),
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
            rate: c.currentExchangeRate || 1
          }))
        );
        const company = active.find(c => c.isCompanyCurrency);
        if (company) this.companyCurrency.set(company.code);
      },
      error: () => this.currencies.set([])
    });
    this.glRepo.getList({ pageSize: 1 }).subscribe({
      next: rows => this.showAnalytical.set(!!rows?.[0]?.useAnalyticalAccounts),
      error: () => this.showAnalytical.set(false)
    });

    this.form.controls.companyId.valueChanges.subscribe(id => {
      this.formCompanyId.set(id || '');
      const branchId = this.form.controls.branchId.value;
      if (branchId && !this.branches().some(b => b.id === branchId && b.companyId === id)) {
        this.form.controls.branchId.setValue('');
      }
    });

    this.load();
  }

  @HostListener('document:keydown', ['$event'])
  onKey(e: KeyboardEvent): void {
    if (e.key === 'Escape' && this.showModal() && !this.saving()) this.closeModal();
    if (e.key === 'F5') {
      e.preventDefault();
      this.load();
    }
    if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'n' && this.canCreate()) {
      e.preventDefault();
      this.openCreate();
    }
    if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 's' && this.showModal() && !this.viewOnly()) {
      e.preventDefault();
      this.save();
    }
  }

  applyFilters(): void {
    this.load();
  }

  resetFilters(): void {
    const today = this.isoDate(new Date());
    this.search.set('');
    this.filterStatus.set('');
    this.filterVoucherType.set('');
    this.filterCompanyId.set('');
    this.filterBranchId.set('');
    this.filterFiscalYear.set('');
    this.filterFiscalPeriodId.set('');
    this.fromDate.set(today);
    this.toDate.set(today);
    this.load();
  }

  setPreset(days: number): void {
    const to = new Date();
    const from = new Date();
    from.setDate(to.getDate() - (days - 1));
    this.fromDate.set(this.isoDate(from));
    this.toDate.set(this.isoDate(to));
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo
      .getList({
        search: this.search().trim() || undefined,
        status: this.filterStatus() ? Number(this.filterStatus()) : null,
        voucherType: this.filterVoucherType() ? Number(this.filterVoucherType()) : null,
        companyId: this.filterCompanyId() || null,
        branchId: this.filterBranchId() || null,
        fiscalPeriodId: this.filterFiscalPeriodId() || null,
        fiscalYear: this.filterFiscalYear() === '' ? null : Number(this.filterFiscalYear()),
        fromDate: this.fromDate() || null,
        toDate: this.toDate() || null,
        pageSize: 200
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
          this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.je.loadError'));
          this.loading.set(false);
        }
      });
  }

  select(row: JournalEntry): void {
    this.selectedId.set(row.id);
  }

  statusLabel(value: number | string): string {
    const item = this.statuses.find(s => s.value === Number(value));
    return item ? this.t(item.labelKey) : String(value);
  }

  voucherTypeLabel(value?: number | string | null): string {
    const item = this.voucherTypes.find(s => s.value === Number(value ?? 1));
    return item ? this.t(item.labelKey) : '—';
  }

  openCreate(): void {
    if (!this.canCreate()) return;
    this.editingId.set(null);
    this.viewOnly.set(false);
    this.error.set(null);
    this.editorEntryNumber.set('');
    this.editorStatus.set(1);
    this.selectedLineIndex.set(-1);
    this.linesFA.clear();
    const company = this.companies()[0];
    const period = this.periods()[0];
    this.form.reset({
      companyId: company?.id ?? '',
      branchId: '',
      fiscalPeriodId: period?.id ?? '',
      voucherType: 1,
      postingDate: new Date().toISOString().slice(0, 10),
      description: '',
      reference: ''
    });
    this.form.enable();
    this.formCompanyId.set(company?.id ?? '');
    this.addLine();
    this.addLine();
    this.showModal.set(true);
  }

  openEdit(row?: JournalEntry): void {
    const target = row ?? this.selected();
    if (!target) return;
    const draftManual = Number(target.status) === 1 && Number(target.sourceModule) === 1;
    const canEditDraft = draftManual && this.canEdit();
    if (!canEditDraft && !this.canView()) return;

    this.viewOnly.set(!canEditDraft);
    this.editingId.set(target.id);
    this.error.set(null);
    this.loading.set(true);

    this.repo.getById(target.id).subscribe({
      next: detail => {
        this.editorEntryNumber.set(detail.entryNumber);
        this.editorStatus.set(Number(detail.status));
        this.linesFA.clear();
        for (const line of detail.lines ?? []) {
          this.linesFA.push(this.createLineGroup(line));
        }
        if (this.linesFA.length === 0) {
          this.addLine();
          this.addLine();
        }
        this.form.reset({
          companyId: detail.companyId ?? '',
          branchId: detail.branchId ?? '',
          fiscalPeriodId: detail.fiscalPeriodId ?? '',
          voucherType: Number(detail.voucherType ?? 1),
          postingDate: detail.postingDate,
          description: detail.description,
          reference: detail.reference ?? ''
        });
        this.formCompanyId.set(detail.companyId ?? '');
        if (this.viewOnly()) this.form.disable();
        else this.form.enable();
        this.loading.set(false);
        this.showModal.set(true);
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.je.loadError'));
        this.loading.set(false);
      }
    });
  }

  copySelected(): void {
    const row = this.selected();
    if (!row || !this.canCreate()) return;
    this.repo.getById(row.id).subscribe({
      next: detail => {
        this.editingId.set(null);
        this.viewOnly.set(false);
        this.editorEntryNumber.set('');
        this.editorStatus.set(1);
        this.linesFA.clear();
        for (const line of detail.lines ?? []) {
          this.linesFA.push(this.createLineGroup(line));
        }
        this.form.reset({
          companyId: detail.companyId ?? this.companies()[0]?.id ?? '',
          branchId: detail.branchId ?? '',
          fiscalPeriodId: detail.fiscalPeriodId ?? this.periods()[0]?.id ?? '',
          voucherType: Number(detail.voucherType ?? 1) <= 3 ? Number(detail.voucherType ?? 1) : 1,
          postingDate: new Date().toISOString().slice(0, 10),
          description: `${detail.description} (${this.t('fin.ops.je.copySuffix')})`,
          reference: detail.reference ?? ''
        });
        this.form.enable();
        this.formCompanyId.set(this.form.controls.companyId.value);
        this.showModal.set(true);
      }
    });
  }

  closeModal(): void {
    if (this.saving()) return;
    if (!this.viewOnly() && this.form.dirty && !confirm(this.t('fin.ops.je.confirmCancel'))) return;
    this.showModal.set(false);
  }

  createLineGroup(line?: {
    chartOfAccountId?: string;
    costCenterId?: string | null;
    analyticalAccountId?: string | null;
    debit?: number;
    credit?: number;
    currency?: string;
    exchangeRate?: number;
    description?: string | null;
  }) {
    return this.fb.nonNullable.group({
      chartOfAccountId: [line?.chartOfAccountId ?? '', Validators.required],
      costCenterId: [line?.costCenterId ?? ''],
      analyticalAccountId: [line?.analyticalAccountId ?? ''],
      currency: [line?.currency || this.companyCurrency()],
      exchangeRate: [line?.exchangeRate ?? 1, [Validators.required, Validators.min(0.000001)]],
      debit: [line?.debit ?? 0, [Validators.min(0)]],
      credit: [line?.credit ?? 0, [Validators.min(0)]],
      description: [line?.description ?? '']
    });
  }

  addLine(): void {
    this.linesFA.push(this.createLineGroup());
    this.selectedLineIndex.set(this.linesFA.length - 1);
  }

  removeLine(index?: number): void {
    const i = index ?? this.selectedLineIndex();
    if (i < 0 || this.linesFA.length <= 2) return;
    this.linesFA.removeAt(i);
    this.selectedLineIndex.set(-1);
  }

  copyLine(): void {
    const i = this.selectedLineIndex();
    if (i < 0) return;
    const raw = this.linesFA.at(i).getRawValue();
    this.linesFA.push(this.createLineGroup(raw));
    this.selectedLineIndex.set(this.linesFA.length - 1);
  }

  selectLine(index: number): void {
    this.selectedLineIndex.set(index);
  }

  private mapLines() {
    return this.linesFA.controls.map(c => {
      const lv = c.getRawValue() as {
        chartOfAccountId: string;
        costCenterId: string;
        analyticalAccountId: string;
        currency: string;
        exchangeRate: number;
        debit: number;
        credit: number;
        description: string;
      };
      return {
        chartOfAccountId: lv.chartOfAccountId,
        costCenterId: lv.costCenterId || null,
        analyticalAccountId: lv.analyticalAccountId || null,
        currency: lv.currency || this.companyCurrency(),
        exchangeRate: Number(lv.exchangeRate) || 1,
        debit: Number(lv.debit) || 0,
        credit: Number(lv.credit) || 0,
        description: lv.description.trim() || null
      };
    });
  }

  private validateLinesXor(): boolean {
    return this.mapLines().every(l => {
      const hasDebit = l.debit > 0;
      const hasCredit = l.credit > 0;
      return (hasDebit || hasCredit) && !(hasDebit && hasCredit);
    });
  }

  save(): void {
    if (this.viewOnly()) return;
    if (this.form.invalid || this.linesFA.length < 2) {
      this.form.markAllAsTouched();
      this.error.set(this.t('fin.ops.je.validationRequired'));
      return;
    }
    if (!this.validateLinesXor()) {
      this.error.set(this.t('fin.ops.je.lineXor'));
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    const v = this.form.getRawValue();
    const id = this.editingId();
    const voucherType = Number(v.voucherType);

    if (id) {
      const payload: UpdateJournalPayload = {
        postingDate: v.postingDate,
        description: v.description.trim(),
        companyId: v.companyId || null,
        branchId: v.branchId || null,
        reference: v.reference.trim() || null,
        voucherType,
        fiscalPeriodId: v.fiscalPeriodId || null,
        lines: this.mapLines()
      };
      this.repo.update(id, payload).subscribe({
        next: saved => this.afterSave(saved),
        error: err => this.onSaveError(err)
      });
    } else {
      const payload: CreateJournalPayload = {
        postingDate: v.postingDate,
        description: v.description.trim(),
        sourceModule: 1,
        companyId: v.companyId || null,
        branchId: v.branchId || null,
        reference: v.reference.trim() || null,
        voucherType,
        fiscalPeriodId: v.fiscalPeriodId || null,
        lines: this.mapLines()
      };
      this.repo.create(payload).subscribe({
        next: saved => this.afterSave(saved),
        error: err => this.onSaveError(err)
      });
    }
  }

  private afterSave(saved: JournalEntry): void {
    this.saving.set(false);
    this.form.markAsPristine();
    this.showModal.set(false);
    this.load();
    this.selectedId.set(saved.id);
  }

  private onSaveError(err: { error?: { detail?: string; error?: string } }): void {
    this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.je.saveError'));
    this.saving.set(false);
  }

  approveSelected(): void {
    const row = this.selected();
    if (!row || !this.canApprove() || Number(row.status) !== 1) return;
    if (!confirm(this.t('fin.ops.je.confirmApprove'))) return;
    this.repo.approve(row.id).subscribe({
      next: () => this.load(),
      error: err =>
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.je.approveError'))
    });
  }

  postSelected(): void {
    const row = this.selected();
    if (!row || !this.canPost() || Number(row.status) !== 4) return;
    if (!confirm(this.t('fin.ops.je.confirmPost'))) return;
    this.repo.post(row.id).subscribe({
      next: () => this.load(),
      error: err =>
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.je.postError'))
    });
  }

  reverseSelected(): void {
    const row = this.selected();
    if (!row || !this.canReverse() || Number(row.status) !== 2) return;
    if (!confirm(this.t('fin.ops.je.confirmReverse'))) return;
    this.repo.reverse(row.id).subscribe({
      next: () => this.load(),
      error: err =>
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.je.reverseError'))
    });
  }

  remove(): void {
    const row = this.selected();
    if (!row || !this.canDelete() || !this.isManualDraft()) return;
    if (!confirm(this.t('fin.ops.je.confirmDelete'))) return;
    this.repo.delete(row.id).subscribe({
      next: () => {
        this.selectedId.set(null);
        this.load();
      },
      error: err =>
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.je.deleteError'))
    });
  }

  exportExcel(): void {
    if (!this.canExport()) return;
    const header = [
      this.t('fin.ops.je.col.number'),
      this.t('fin.ops.je.col.date'),
      this.t('fin.ops.je.col.type'),
      this.t('fin.ops.je.col.description'),
      this.t('fin.ops.je.col.debit'),
      this.t('fin.ops.je.col.credit'),
      this.t('fin.ops.je.col.status'),
      this.t('fin.ops.je.col.user'),
      this.t('fin.ops.je.col.createdAt')
    ];
    const csv = [
      header.join(','),
      ...this.rows().map(r =>
        [
          r.entryNumber,
          r.postingDate,
          this.voucherTypeLabel(r.voucherType),
          r.description,
          r.totalDebit,
          r.totalCredit,
          this.statusLabel(r.status),
          r.createdBy ?? '',
          r.createdAt ?? ''
        ]
          .map(v => `"${String(v).replace(/"/g, '""')}"`)
          .join(',')
      )
    ].join('\n');
    const blob = new Blob(['\ufeff' + csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'journal-vouchers.csv';
    a.click();
    URL.revokeObjectURL(url);
  }

  printSelected(): void {
    if (!this.canPrint()) return;
    window.print();
  }

  private isoDate(d: Date): string {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }
}
