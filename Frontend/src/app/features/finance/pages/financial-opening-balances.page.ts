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
import { FinancialOpeningBalanceRepository } from '../../../core/repositories/financial-opening-balance.repository';
import { ChartOfAccountRepository } from '../../../core/repositories/chart-of-account.repository';
import { CostCenterRepository } from '../../../core/repositories/cost-center.repository';
import {
  OB_STATUSES,
  FinancialOpeningBalance,
  FiscalPeriodLookup,
  OrgBranchLookup,
  OrgCompanyLookup,
  UpsertFinancialOpeningBalancePayload
} from '../../../core/models/financial-opening-balance.models';
import { ChartAccount } from '../../../core/models/chart-of-account.models';
import { flattenTreeAccounts } from './coa-tree.util';

@Component({
  selector: 'app-financial-opening-balances-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatIconModule, MatTooltipModule],
  templateUrl: './financial-opening-balances.page.html',
  styleUrl: './financial-opening-balances.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FinancialOpeningBalancesPage implements OnInit {
  private repo = inject(FinancialOpeningBalanceRepository);
  private accountsRepo = inject(ChartOfAccountRepository);
  private costCentersRepo = inject(CostCenterRepository);
  private fb = inject(FormBuilder);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  rows = signal<FinancialOpeningBalance[]>([]);
  companies = signal<OrgCompanyLookup[]>([]);
  branches = signal<OrgBranchLookup[]>([]);
  periods = signal<FiscalPeriodLookup[]>([]);
  accounts = signal<ChartAccount[]>([]);
  costCenters = signal<{ id: string; nameAr: string }[]>([]);

  selectedId = signal<string | null>(null);
  search = signal('');
  filterCompanyId = signal('');
  filterStatus = signal('');
  showModal = signal(false);
  editingId = signal<string | null>(null);
  formCompanyId = signal('');
  statuses = OB_STATUSES;

  form = this.fb.nonNullable.group({
    companyId: ['', Validators.required],
    branchId: [''],
    openingDate: [new Date().toISOString().slice(0, 10), Validators.required],
    fiscalPeriodId: ['', Validators.required],
    description: [''],
    equityAccountId: [''],
    lines: this.fb.array([])
  });

  canView = computed(
    () =>
      this.auth.hasPermission('Finance.OpeningBalances.View') ||
      this.auth.hasPermission('Accounting.View') ||
      this.auth.hasPermission('VIEW_FINANCE')
  );
  canCreate = computed(
    () =>
      this.auth.hasPermission('Finance.OpeningBalances.Create') ||
      this.auth.hasPermission('Accounting.Create') ||
      this.auth.hasPermission('Journal.Create')
  );
  canEdit = computed(
    () =>
      this.auth.hasPermission('Finance.OpeningBalances.Edit') ||
      this.auth.hasPermission('Accounting.Update')
  );
  canDelete = computed(
    () =>
      this.auth.hasPermission('Finance.OpeningBalances.Delete') ||
      this.auth.hasPermission('Accounting.Delete')
  );
  canPost = computed(
    () =>
      this.auth.hasPermission('Finance.OpeningBalances.Post') ||
      this.auth.hasPermission('Journal.Post') ||
      this.auth.hasPermission('Accounting.Update')
  );

  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);
  isEditing = computed(() => !!this.editingId());
  isDraftSelected = computed(() => Number(this.selected()?.status) === 1);

  modalBranches = computed(() => {
    const companyId = this.formCompanyId();
    if (!companyId) return [] as OrgBranchLookup[];
    return this.branches().filter(b => b.companyId === companyId);
  });

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
      next: rows => this.costCenters.set(rows.map(c => ({ id: c.id, nameAr: c.nameAr }))),
      error: () => this.costCenters.set([])
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
    if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 's' && this.showModal()) {
      e.preventDefault();
      this.save();
    }
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo
      .getList({
        search: this.search().trim() || undefined,
        companyId: this.filterCompanyId() || null,
        status: this.filterStatus() ? Number(this.filterStatus()) : null
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
          this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.ob.loadError'));
          this.loading.set(false);
        }
      });
  }

  select(row: FinancialOpeningBalance): void {
    this.selectedId.set(row.id);
  }

  statusLabel(value: number | string): string {
    const item = this.statuses.find(s => s.value === Number(value));
    return item ? this.t(item.labelKey) : String(value);
  }

  openCreate(): void {
    if (!this.canCreate()) return;
    this.editingId.set(null);
    this.error.set(null);
    this.linesFA.clear();
    const company = this.companies()[0];
    const period = this.periods().find(p => Number(p.status) === 1) ?? this.periods()[0];
    this.form.reset({
      companyId: company?.id ?? '',
      branchId: '',
      openingDate: new Date().toISOString().slice(0, 10),
      fiscalPeriodId: period?.id ?? '',
      description: '',
      equityAccountId: ''
    });
    this.formCompanyId.set(company?.id ?? '');
    this.addLine();
    this.showModal.set(true);
  }

  openEdit(row?: FinancialOpeningBalance): void {
    const target = row ?? this.selected();
    if (!target || !this.canEdit() || Number(target.status) !== 1) return;
    this.editingId.set(target.id);
    this.error.set(null);
    this.linesFA.clear();
    for (const line of target.lines ?? []) {
      this.linesFA.push(this.createLineGroup(line));
    }
    if (this.linesFA.length === 0) this.addLine();
    this.form.reset({
      companyId: target.companyId,
      branchId: target.branchId ?? '',
      openingDate: target.openingDate,
      fiscalPeriodId: target.fiscalPeriodId,
      description: target.description ?? '',
      equityAccountId: target.equityAccountId ?? ''
    });
    this.formCompanyId.set(target.companyId);
    this.showModal.set(true);
  }

  closeModal(): void {
    if (this.saving()) return;
    if (this.form.dirty && !confirm(this.t('fin.ops.ob.confirmCancel'))) return;
    this.showModal.set(false);
  }

  createLineGroup(line?: {
    chartOfAccountId?: string;
    costCenterId?: string | null;
    debit?: number;
    credit?: number;
    currency?: string;
    description?: string | null;
  }) {
    return this.fb.nonNullable.group({
      chartOfAccountId: [line?.chartOfAccountId ?? '', Validators.required],
      costCenterId: [line?.costCenterId ?? ''],
      debit: [line?.debit ?? 0, [Validators.min(0)]],
      credit: [line?.credit ?? 0, [Validators.min(0)]],
      currency: [line?.currency ?? 'SAR'],
      description: [line?.description ?? '']
    });
  }

  addLine(): void {
    this.linesFA.push(this.createLineGroup());
  }

  removeLine(index: number): void {
    this.linesFA.removeAt(index);
  }

  buildPayload(): UpsertFinancialOpeningBalancePayload {
    const v = this.form.getRawValue();
    return {
      companyId: v.companyId,
      branchId: v.branchId || null,
      openingDate: v.openingDate,
      fiscalPeriodId: v.fiscalPeriodId,
      description: v.description.trim() || null,
      equityAccountId: v.equityAccountId || null,
      lines: this.linesFA.controls.map(c => {
        const lv = c.getRawValue() as {
          chartOfAccountId: string;
          costCenterId: string;
          debit: number;
          credit: number;
          currency: string;
          description: string;
        };
        return {
          chartOfAccountId: lv.chartOfAccountId,
          costCenterId: lv.costCenterId || null,
          debit: Number(lv.debit) || 0,
          credit: Number(lv.credit) || 0,
          currency: lv.currency || 'SAR',
          description: lv.description.trim() || null
        };
      })
    };
  }

  save(): void {
    if (this.form.invalid || this.linesFA.length === 0) {
      this.form.markAllAsTouched();
      this.error.set(this.t('fin.ops.ob.validationRequired'));
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
        this.showModal.set(false);
        this.load();
        this.selectedId.set(saved.id);
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.ob.saveError'));
        this.saving.set(false);
      }
    });
  }

  postSelected(): void {
    const row = this.selected();
    if (!row || !this.canPost() || Number(row.status) !== 1) return;
    if (!confirm(this.t('fin.ops.ob.confirmPost'))) return;
    this.repo.post(row.id).subscribe({
      next: () => this.load(),
      error: err =>
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.ob.postError'))
    });
  }

  remove(): void {
    const row = this.selected();
    if (!row || !this.canDelete() || Number(row.status) !== 1) return;
    if (!confirm(this.t('fin.ops.ob.confirmDelete'))) return;
    this.repo.delete(row.id).subscribe({
      next: () => {
        this.selectedId.set(null);
        this.load();
      },
      error: err =>
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.ob.deleteError'))
    });
  }
}
