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
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { GeneralLedgerSettingRepository } from '../../../core/repositories/general-ledger-setting.repository';
import {
  CLOSING_METHODS,
  GeneralLedgerSetting,
  OrgBranchLookup,
  OrgCompanyLookup,
  UpsertGeneralLedgerSettingPayload
} from '../../../core/models/general-ledger-setting.models';

const FILTERS_KEY = 'gastro.gl-settings.filters';

@Component({
  selector: 'app-general-ledger-settings-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatIconModule, MatTooltipModule],
  templateUrl: './general-ledger-settings.page.html',
  styleUrl: './general-ledger-settings.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GeneralLedgerSettingsPage implements OnInit {
  private repo = inject(GeneralLedgerSettingRepository);
  private fb = inject(FormBuilder);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  rows = signal<GeneralLedgerSetting[]>([]);
  companies = signal<OrgCompanyLookup[]>([]);
  branches = signal<OrgBranchLookup[]>([]);

  selectedId = signal<string | null>(null);
  search = signal('');
  filterCompanyId = signal('');
  filterBranchId = signal('');
  showModal = signal(false);
  editingId = signal<string | null>(null);
  formCompanyId = signal('');
  pageIndex = signal(0);

  closingMethods = CLOSING_METHODS;

  form = this.fb.nonNullable.group({
    companyId: ['', Validators.required],
    branchId: ['', Validators.required],
    voucherNumberLength: [8 as number, [Validators.required, Validators.min(4), Validators.max(12)]],
    decimalPlaces: [2 as number, [Validators.required, Validators.min(0), Validators.max(4)]],
    showDateInReports: [true],
    showPostingIndicator: [true],
    autoPostReceiptChecks: [false],
    autoPostPaymentChecks: [false],
    useBudgetPerCurrency: [false],
    useAnalyticalAccounts: [false],
    allowZeroEffectEntries: [false],
    requireJournalType: [false],
    allowManualTaxEntries: [false],
    requireReferenceNumber: [false],
    closingMethod: [1 as number, Validators.required]
  });

  canView = computed(
    () =>
      this.auth.hasPermission('Finance.GeneralLedgerSettings.View') ||
      this.auth.hasPermission('Accounting.View') ||
      this.auth.hasPermission('VIEW_FINANCE')
  );
  canCreate = computed(
    () =>
      this.auth.hasPermission('Finance.GeneralLedgerSettings.Create') ||
      this.auth.hasPermission('Accounting.Create')
  );
  canEdit = computed(
    () =>
      this.auth.hasPermission('Finance.GeneralLedgerSettings.Edit') ||
      this.auth.hasPermission('Accounting.Update')
  );
  canDelete = computed(
    () =>
      this.auth.hasPermission('Finance.GeneralLedgerSettings.Delete') ||
      this.auth.hasPermission('Accounting.Delete')
  );

  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);
  isEditing = computed(() => !!this.editingId());
  pageSize = 20;
  pagedRows = computed(() => {
    const start = this.pageIndex() * this.pageSize;
    return this.rows().slice(start, start + this.pageSize);
  });
  totalPages = computed(() => Math.max(1, Math.ceil(this.rows().length / this.pageSize)));

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

  t = (key: string) => this.lang.t(key);

  ngOnInit(): void {
    this.restoreFilters();
    this.repo.getCompanies().subscribe({
      next: rows => this.companies.set(rows.filter(c => c.isActive !== false)),
      error: () => this.companies.set([])
    });
    this.repo.getBranches().subscribe({
      next: rows => this.branches.set(rows),
      error: () => this.branches.set([])
    });

    this.form.controls.companyId.valueChanges.subscribe(companyId => {
      this.formCompanyId.set(companyId || '');
      const branchId = this.form.controls.branchId.value;
      if (branchId && !this.branches().some(b => b.id === branchId && b.companyId === companyId)) {
        this.form.controls.branchId.setValue('');
      }
    });

    this.load();
  }

  @HostListener('document:keydown', ['$event'])
  onKey(e: KeyboardEvent): void {
    if (e.ctrlKey && e.key.toLowerCase() === 'n') {
      e.preventDefault();
      this.openCreate();
    }
    if (e.ctrlKey && e.key.toLowerCase() === 's' && this.showModal()) {
      e.preventDefault();
      this.save();
    }
    if (e.ctrlKey && e.key.toLowerCase() === 'f') {
      e.preventDefault();
      document.querySelector<HTMLInputElement>('.search-box input')?.focus();
    }
    if (e.key === 'F5') {
      e.preventDefault();
      this.load();
    }
    if (e.key === 'Delete' && this.selected() && !this.showModal()) {
      this.remove();
    }
  }

  private restoreFilters(): void {
    try {
      const raw = localStorage.getItem(FILTERS_KEY);
      if (!raw) return;
      const parsed = JSON.parse(raw) as { search?: string; companyId?: string; branchId?: string };
      this.search.set(parsed.search ?? '');
      this.filterCompanyId.set(parsed.companyId ?? '');
      this.filterBranchId.set(parsed.branchId ?? '');
    } catch {
      /* ignore */
    }
  }

  private persistFilters(): void {
    localStorage.setItem(
      FILTERS_KEY,
      JSON.stringify({
        search: this.search(),
        companyId: this.filterCompanyId(),
        branchId: this.filterBranchId()
      })
    );
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.persistFilters();
    this.repo
      .getList({
        search: this.search().trim() || undefined,
        companyId: this.filterCompanyId() || null,
        branchId: this.filterBranchId() || null
      })
      .subscribe({
        next: rows => {
          this.rows.set(rows);
          this.loading.set(false);
          this.pageIndex.set(0);
          if (this.selectedId() && !rows.some(r => r.id === this.selectedId())) {
            this.selectedId.set(null);
          }
        },
        error: err => {
          this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.gl.loadError'));
          this.loading.set(false);
        }
      });
  }

  resetFilters(): void {
    this.search.set('');
    this.filterCompanyId.set('');
    this.filterBranchId.set('');
    this.load();
  }

  select(row: GeneralLedgerSetting): void {
    this.selectedId.set(row.id);
  }

  closingLabel(value: number | string, code?: string): string {
    const byValue = this.closingMethods.find(m => m.value === Number(value));
    if (byValue) return this.t(byValue.labelKey);
    if (code) {
      const byCode = this.closingMethods.find(m => m.code === code);
      if (byCode) return this.t(byCode.labelKey);
    }
    return String(code || value);
  }

  formatDate(value?: string | null): string {
    if (!value) return '—';
    return value.slice(0, 10);
  }

  openCreate(): void {
    if (!this.canCreate()) return;
    this.editingId.set(null);
    this.error.set(null);
    const company = this.companies()[0];
    const companyBranches = company
      ? this.branches().filter(b => b.companyId === company.id)
      : [];
    this.form.reset({
      companyId: company?.id ?? '',
      branchId: companyBranches[0]?.id ?? '',
      voucherNumberLength: 8,
      decimalPlaces: 2,
      showDateInReports: true,
      showPostingIndicator: true,
      autoPostReceiptChecks: false,
      autoPostPaymentChecks: false,
      useBudgetPerCurrency: false,
      useAnalyticalAccounts: false,
      allowZeroEffectEntries: false,
      requireJournalType: false,
      allowManualTaxEntries: false,
      requireReferenceNumber: false,
      closingMethod: 1
    });
    this.formCompanyId.set(company?.id ?? '');
    this.showModal.set(true);
  }

  openEdit(): void {
    const row = this.selected();
    if (!row || !this.canEdit()) return;
    this.editingId.set(row.id);
    this.error.set(null);
    this.form.reset({
      companyId: row.companyId,
      branchId: row.branchId,
      voucherNumberLength: row.voucherNumberLength,
      decimalPlaces: row.decimalPlaces,
      showDateInReports: row.showDateInReports,
      showPostingIndicator: row.showPostingIndicator,
      autoPostReceiptChecks: row.autoPostReceiptChecks,
      autoPostPaymentChecks: row.autoPostPaymentChecks,
      useBudgetPerCurrency: row.useBudgetPerCurrency,
      useAnalyticalAccounts: row.useAnalyticalAccounts ?? false,
      allowZeroEffectEntries: row.allowZeroEffectEntries,
      requireJournalType: row.requireJournalType,
      allowManualTaxEntries: row.allowManualTaxEntries,
      requireReferenceNumber: row.requireReferenceNumber,
      closingMethod: Number(row.closingMethod)
    });
    this.formCompanyId.set(row.companyId);
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
    this.editingId.set(null);
  }

  save(closeAfter = true): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    const payload: UpsertGeneralLedgerSettingPayload = {
      companyId: raw.companyId,
      branchId: raw.branchId,
      voucherNumberLength: Number(raw.voucherNumberLength),
      decimalPlaces: Number(raw.decimalPlaces),
      showDateInReports: raw.showDateInReports,
      showPostingIndicator: raw.showPostingIndicator,
      autoPostReceiptChecks: raw.autoPostReceiptChecks,
      autoPostPaymentChecks: raw.autoPostPaymentChecks,
      useBudgetPerCurrency: raw.useBudgetPerCurrency,
      useAnalyticalAccounts: raw.useAnalyticalAccounts,
      allowZeroEffectEntries: raw.allowZeroEffectEntries,
      requireJournalType: raw.requireJournalType,
      allowManualTaxEntries: raw.allowManualTaxEntries,
      requireReferenceNumber: raw.requireReferenceNumber,
      closingMethod: Number(raw.closingMethod)
    };

    this.saving.set(true);
    this.error.set(null);
    const req = this.editingId()
      ? this.repo.update(this.editingId()!, payload)
      : this.repo.create(payload);

    req.subscribe({
      next: saved => {
        this.saving.set(false);
        this.selectedId.set(saved.id);
        if (closeAfter) this.closeModal();
        else this.editingId.set(saved.id);
        this.load();
      },
      error: err => {
        this.saving.set(false);
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.gl.saveError'));
      }
    });
  }

  remove(): void {
    const row = this.selected();
    if (!row || !this.canDelete() || row.isSystem) return;
    if (!confirm(this.t('fin.gl.confirmDelete'))) return;
    this.repo.delete(row.id).subscribe({
      next: () => {
        this.selectedId.set(null);
        this.load();
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.gl.deleteError'));
      }
    });
  }

  exportCsv(): void {
    const header = [
      'Number',
      'Company',
      'Branch',
      'VoucherLength',
      'DecimalPlaces',
      'ClosingMethod',
      'CreatedAt',
      'UpdatedAt'
    ];
    const lines = this.rows().map(r =>
      [
        r.number,
        r.companyNameAr ?? '',
        r.branchNameAr ?? '',
        r.voucherNumberLength,
        r.decimalPlaces,
        r.closingMethodCode,
        r.createdAt,
        r.updatedAt ?? ''
      ]
        .map(v => `"${String(v).replace(/"/g, '""')}"`)
        .join(',')
    );
    const blob = new Blob([[header.join(','), ...lines].join('\n')], {
      type: 'text/csv;charset=utf-8;'
    });
    const a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = 'general-ledger-settings.csv';
    a.click();
    URL.revokeObjectURL(a.href);
  }

  printPage(): void {
    window.print();
  }

  prevPage(): void {
    this.pageIndex.update(i => Math.max(0, i - 1));
  }

  nextPage(): void {
    this.pageIndex.update(i => Math.min(this.totalPages() - 1, i + 1));
  }

  firstPage(): void {
    this.pageIndex.set(0);
  }

  lastPage(): void {
    this.pageIndex.set(this.totalPages() - 1);
  }

  firstRecord(): void {
    const first = this.rows()[0];
    if (first) this.selectedId.set(first.id);
  }

  lastRecord(): void {
    const rows = this.rows();
    if (rows.length) this.selectedId.set(rows[rows.length - 1].id);
  }
}
