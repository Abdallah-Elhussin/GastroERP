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
import { FinancialNoteRepository } from '../../../core/repositories/financial-note.repository';
import { FinancialOpeningBalanceRepository } from '../../../core/repositories/financial-opening-balance.repository';
import { ChartOfAccountRepository } from '../../../core/repositories/chart-of-account.repository';
import { CostCenterRepository } from '../../../core/repositories/cost-center.repository';
import { CurrencyRepository } from '../../../core/repositories/currency.repository';
import { NotificationReasonRepository } from '../../../core/repositories/notification-reason.repository';
import {
  NOTE_KINDS,
  NOTE_STATUSES,
  FinancialNote,
  FinancialNoteKind,
  UpsertFinancialNotePayload
} from '../../../core/models/financial-note.models';
import {
  NOTIFICATION_PARTY_TYPES,
  NotificationReason
} from '../../../core/models/notification-reason.models';
import {
  FiscalPeriodLookup,
  OrgBranchLookup,
  OrgCompanyLookup
} from '../../../core/models/financial-opening-balance.models';
import { FinanceVoucherAccountOption } from '../../../core/models/finance-voucher-details.models';
import { flattenTreeAccounts } from '../../finance/pages/coa-tree.util';

@Component({
  selector: 'app-debit-credit-notes-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatIconModule, MatTooltipModule],
  templateUrl: './debit-credit-notes.page.html',
  styleUrl: './debit-credit-notes.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DebitCreditNotesPage implements OnInit {
  private repo = inject(FinancialNoteRepository);
  private lookups = inject(FinancialOpeningBalanceRepository);
  private fb = inject(FormBuilder);
  lang = inject(LanguageService);
  auth = inject(AuthService);
  private accountsRepo = inject(ChartOfAccountRepository);
  private costCentersRepo = inject(CostCenterRepository);
  private currencyRepo = inject(CurrencyRepository);
  private reasonsRepo = inject(NotificationReasonRepository);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  rows = signal<FinancialNote[]>([]);
  selectedId = signal<string | null>(null);
  search = signal('');
  filterKind = signal('');
  filterStatus = signal('');
  filterBranch = signal('');
  fromDate = signal('');
  toDate = signal('');
  editorOpen = signal(false);
  editingId = signal<string | null>(null);
  viewOnly = signal(false);
  selectedLineIndex = signal(-1);
  companyCurrency = signal('SAR');

  companies = signal<OrgCompanyLookup[]>([]);
  branches = signal<OrgBranchLookup[]>([]);
  periods = signal<FiscalPeriodLookup[]>([]);
  accounts = signal<FinanceVoucherAccountOption[]>([]);
  costCenters = signal<{ id: string; nameAr: string }[]>([]);
  currencies = signal<{ code: string; nameAr: string; rate: number }[]>([]);
  reasons = signal<NotificationReason[]>([]);

  kinds = NOTE_KINDS;
  statuses = NOTE_STATUSES;
  partyTypes = NOTIFICATION_PARTY_TYPES;

  headerForm = this.fb.nonNullable.group({
    companyId: ['', Validators.required],
    branchId: ['', Validators.required],
    noteKind: [1 as FinancialNoteKind, Validators.required],
    partyType: [1, Validators.required],
    partyName: ['', Validators.required],
    currency: ['SAR', Validators.required],
    exchangeRate: [1, [Validators.required, Validators.min(0.000001)]],
    mainAccountId: ['', Validators.required],
    fiscalPeriodId: ['', Validators.required],
    noteDate: [new Date().toISOString().slice(0, 10), Validators.required],
    referenceNumber: [''],
    description: ['']
  });

  linesFA = this.fb.array([this.createLineGroup()]) as FormArray;

  get lineControls() {
    return this.linesFA.controls as import('@angular/forms').FormGroup[];
  }

  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);
  modalBranches = computed(() => {
    const companyId = this.headerForm.controls.companyId.value;
    return this.branches().filter(b => !companyId || b.companyId === companyId);
  });
  filteredReasons = computed(() => {
    const kind = Number(this.headerForm.controls.noteKind.value);
    const party = Number(this.headerForm.controls.partyType.value);
    return this.reasons().filter(
      r => r.isActive && Number(r.noteType) === kind && Number(r.partyType) === party
    );
  });

  totals = computed(() => {
    let total = 0;
    let local = 0;
    for (const c of this.linesFA.controls) {
      const amount = Number(c.get('amount')?.value) || 0;
      const rate = Number(c.get('exchangeRate')?.value) || 1;
      total += amount;
      local += amount * rate;
    }
    return {
      lineCount: this.linesFA.length,
      total: Math.round(total * 100) / 100,
      local: Math.round(local * 100) / 100
    };
  });

  canCreate = computed(
    () =>
      this.auth.hasPermission('Finance.FinancialNotes.Create') ||
      this.auth.hasPermission('Accounting.Create')
  );
  canEdit = computed(
    () =>
      this.auth.hasPermission('Finance.FinancialNotes.Edit') ||
      this.auth.hasPermission('Accounting.Update')
  );
  canDelete = computed(() => this.auth.hasPermission('Finance.FinancialNotes.Delete'));
  canApprove = computed(() => this.auth.hasPermission('Finance.FinancialNotes.Approve'));
  canPost = computed(() => this.auth.hasPermission('Finance.FinancialNotes.Post'));
  canReverse = computed(() => this.auth.hasPermission('Finance.FinancialNotes.Reverse'));

  t = (key: string) => this.lang.t(key);

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
      next: rows => this.costCenters.set(rows.map(c => ({ id: c.id, nameAr: c.nameAr }))),
      error: () => this.costCenters.set([])
    });
    this.currencyRepo.getList().subscribe({
      next: rows => {
        const active = rows.filter(c => c.isActive);
        this.currencies.set(
          active.map(c => ({
            code: c.code,
            nameAr: c.nameAr,
            rate: c.currentExchangeRate
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
    this.reasonsRepo.getList({ isActive: true, pageSize: 500 }).subscribe({
      next: rows => this.reasons.set(rows),
      error: () => this.reasons.set([])
    });

    this.headerForm.controls.noteKind.valueChanges.subscribe(() => this.onKindOrPartyChanged());
    this.headerForm.controls.partyType.valueChanges.subscribe(() => this.onKindOrPartyChanged());
    this.headerForm.controls.currency.valueChanges.subscribe(code => {
      const cur = this.currencies().find(c => c.code === code);
      if (code === this.companyCurrency()) this.headerForm.controls.exchangeRate.setValue(1);
      else if (cur) this.headerForm.controls.exchangeRate.setValue(cur.rate || 1);
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

  createLineGroup(line?: {
    notificationReasonId?: string;
    offsetAccountId?: string;
    costCenterId?: string | null;
    currency?: string;
    exchangeRate?: number;
    amount?: number;
    description?: string | null;
  }) {
    return this.fb.nonNullable.group({
      notificationReasonId: [line?.notificationReasonId ?? '', Validators.required],
      offsetAccountId: [line?.offsetAccountId ?? '', Validators.required],
      costCenterId: [line?.costCenterId ?? ''],
      currency: [line?.currency || this.companyCurrency(), Validators.required],
      exchangeRate: [line?.exchangeRate ?? 1, [Validators.required, Validators.min(0.000001)]],
      amount: [line?.amount ?? 0, [Validators.required, Validators.min(0.01)]],
      description: [line?.description ?? '']
    });
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo
      .getList({
        search: this.search().trim() || undefined,
        noteKind: this.filterKind() ? Number(this.filterKind()) : null,
        status: this.filterStatus() ? Number(this.filterStatus()) : null,
        branchId: this.filterBranch() || null,
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
          this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.note.loadError'));
          this.loading.set(false);
        }
      });
  }

  select(row: FinancialNote): void {
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
      noteKind: 1,
      partyType: 1,
      partyName: '',
      currency: this.companyCurrency(),
      exchangeRate: 1,
      mainAccountId: '',
      fiscalPeriodId: period?.id ?? '',
      noteDate: new Date().toISOString().slice(0, 10),
      referenceNumber: '',
      description: ''
    });
    this.headerForm.enable();
    while (this.linesFA.length) this.linesFA.removeAt(0);
    this.linesFA.push(this.createLineGroup());
    this.editorOpen.set(true);
  }

  openEdit(row?: FinancialNote): void {
    const target = row ?? this.selected();
    if (!target) return;
    this.loading.set(true);
    this.repo.getById(target.id).subscribe({
      next: doc => {
        this.editingId.set(doc.id);
        this.viewOnly.set(doc.status !== 1 && doc.status !== 2);
        this.error.set(null);
        this.headerForm.reset({
          companyId: doc.companyId,
          branchId: doc.branchId,
          noteKind: doc.noteKind,
          partyType: doc.partyType,
          partyName: doc.partyName ?? '',
          currency: doc.currency,
          exchangeRate: doc.exchangeRate,
          mainAccountId: doc.mainAccountId,
          fiscalPeriodId: doc.fiscalPeriodId,
          noteDate: doc.noteDate,
          referenceNumber: doc.referenceNumber ?? '',
          description: doc.description ?? ''
        });
        while (this.linesFA.length) this.linesFA.removeAt(0);
        for (const line of doc.lines) {
          this.linesFA.push(
            this.createLineGroup({
              notificationReasonId: line.notificationReasonId,
              offsetAccountId: line.offsetAccountId,
              costCenterId: line.costCenterId,
              currency: line.currency,
              exchangeRate: line.exchangeRate,
              amount: line.amount,
              description: line.description
            })
          );
        }
        if (!doc.lines.length) this.linesFA.push(this.createLineGroup());
        if (this.viewOnly()) this.headerForm.disable();
        else this.headerForm.enable();
        this.editorOpen.set(true);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.note.loadError'));
        this.loading.set(false);
      }
    });
  }

  closeEditor(): void {
    if (this.saving()) return;
    this.editorOpen.set(false);
  }

  addLine(): void {
    if (this.viewOnly()) return;
    this.linesFA.push(
      this.createLineGroup({
        currency: this.headerForm.controls.currency.value,
        exchangeRate: this.headerForm.controls.exchangeRate.value
      })
    );
  }

  removeLine(index?: number): void {
    if (this.viewOnly() || this.linesFA.length <= 1) return;
    const idx = index ?? (this.selectedLineIndex() >= 0 ? this.selectedLineIndex() : this.linesFA.length - 1);
    if (idx < 0 || idx >= this.linesFA.length) return;
    this.linesFA.removeAt(idx);
    this.selectedLineIndex.set(-1);
  }

  onReasonChange(index: number): void {
    const ctrl = this.linesFA.at(index);
    const reasonId = ctrl.get('notificationReasonId')?.value as string;
    const reason = this.reasons().find(r => r.id === reasonId);
    if (reason?.counterpartAccountId) {
      ctrl.patchValue({ offsetAccountId: reason.counterpartAccountId });
    }
  }

  onKindOrPartyChanged(): void {
    for (const c of this.linesFA.controls) {
      const reasonId = c.get('notificationReasonId')?.value as string;
      if (reasonId && !this.filteredReasons().some(r => r.id === reasonId)) {
        c.patchValue({ notificationReasonId: '', offsetAccountId: '' });
      }
    }
  }

  localAmount(index: number): number {
    const c = this.linesFA.at(index);
    const amount = Number(c.get('amount')?.value) || 0;
    const rate = Number(c.get('exchangeRate')?.value) || 1;
    return Math.round(amount * rate * 100) / 100;
  }

  accountLabel(id: string): string {
    const a = this.accounts().find(x => x.id === id);
    return a ? `${a.accountNumber} — ${a.nameAr}` : '';
  }

  save(): void {
    if (this.viewOnly()) return;
    if (this.headerForm.invalid || this.linesFA.invalid || this.linesFA.length < 1) {
      this.headerForm.markAllAsTouched();
      this.linesFA.markAllAsTouched();
      this.error.set(this.t('fin.ops.voucher.validationRequired'));
      return;
    }

    const v = this.headerForm.getRawValue();
    const payload: UpsertFinancialNotePayload = {
      noteKind: Number(v.noteKind) as FinancialNoteKind,
      companyId: v.companyId,
      branchId: v.branchId,
      noteDate: v.noteDate,
      fiscalPeriodId: v.fiscalPeriodId,
      partyType: Number(v.partyType),
      mainAccountId: v.mainAccountId,
      currency: v.currency,
      exchangeRate: Number(v.exchangeRate) || 1,
      partyName: v.partyName.trim(),
      referenceNumber: v.referenceNumber.trim() || null,
      description: v.description.trim() || null,
      lines: this.linesFA.controls.map(c => {
        const lv = c.getRawValue() as {
          notificationReasonId: string;
          offsetAccountId: string;
          costCenterId: string;
          currency: string;
          exchangeRate: number;
          amount: number;
          description: string;
        };
        return {
          notificationReasonId: lv.notificationReasonId,
          offsetAccountId: lv.offsetAccountId,
          costCenterId: lv.costCenterId || null,
          currency: lv.currency,
          exchangeRate: Number(lv.exchangeRate) || 1,
          amount: Number(lv.amount) || 0,
          description: lv.description?.trim() || null
        };
      })
    };

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
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.note.saveError'));
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
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.note.saveError'))
    });
  }

  postSelected(): void {
    const row = this.selected();
    if (!row || !this.canPost() || (row.status !== 1 && row.status !== 3)) return;
    this.repo.post(row.id).subscribe({
      next: () => this.load(),
      error: err =>
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.note.saveError'))
    });
  }

  reverseSelected(): void {
    const row = this.selected();
    if (!row || !this.canReverse() || row.status !== 4) return;
    this.repo.reverse(row.id).subscribe({
      next: () => this.load(),
      error: err =>
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.note.saveError'))
    });
  }

  remove(): void {
    const row = this.selected();
    if (!row || !this.canDelete() || row.status !== 1) return;
    if (!confirm(this.t('fin.ops.note.confirmDelete'))) return;
    this.repo.delete(row.id).subscribe({
      next: () => {
        this.selectedId.set(null);
        this.load();
      },
      error: err =>
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.ops.note.saveError'))
    });
  }

  statusLabel(status: number): string {
    return this.t(this.statuses.find(s => s.value === status)?.labelKey ?? 'fin.ops.note.status.draft');
  }

  kindLabel(kind: number): string {
    return this.t(this.kinds.find(k => k.value === kind)?.labelKey ?? 'fin.ops.note.kind.debit');
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
    this.filterKind.set('');
    this.filterStatus.set('');
    this.filterBranch.set('');
    this.fromDate.set('');
    this.toDate.set('');
    this.load();
  }
}
