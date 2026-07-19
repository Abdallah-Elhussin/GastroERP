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
import { CashBoxRepository } from '../../../core/repositories/cash-box.repository';
import { CurrencyRepository } from '../../../core/repositories/currency.repository';
import { ChartOfAccountRepository } from '../../../core/repositories/chart-of-account.repository';
import { AccountClassificationRepository } from '../../../core/repositories/account-classification.repository';
import {
  CASH_BOX_DEVICE_ROLES,
  CashBox,
  OrgBranchLookup,
  OrgCompanyLookup,
  OrgDeviceLookup,
  UpsertCashBoxPayload,
  UserLookup
} from '../../../core/models/cash-box.models';
import { Currency } from '../../../core/models/currency.models';
import { ChartAccount } from '../../../core/models/chart-of-account.models';
import { flattenTreeAccounts } from './coa-tree.util';

@Component({
  selector: 'app-cash-boxes-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatIconModule, MatTooltipModule],
  templateUrl: './cash-boxes.page.html',
  styleUrl: './cash-boxes.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CashBoxesPage implements OnInit {
  private repo = inject(CashBoxRepository);
  private currenciesRepo = inject(CurrencyRepository);
  private accountsRepo = inject(ChartOfAccountRepository);
  private classificationsRepo = inject(AccountClassificationRepository);
  private fb = inject(FormBuilder);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  rows = signal<CashBox[]>([]);
  companies = signal<OrgCompanyLookup[]>([]);
  branches = signal<OrgBranchLookup[]>([]);
  currencies = signal<Currency[]>([]);
  accounts = signal<ChartAccount[]>([]);
  devices = signal<OrgDeviceLookup[]>([]);
  users = signal<UserLookup[]>([]);
  cashClassIds = signal<Set<string>>(new Set());

  selectedId = signal<string | null>(null);
  search = signal('');
  filterCompanyId = signal('');
  filterBranchId = signal('');
  showModal = signal(false);
  editingId = signal<string | null>(null);
  formTab = signal<'basic' | 'ops' | 'users' | 'devices' | 'system'>('basic');
  formCompanyId = signal('');

  deviceRoles = CASH_BOX_DEVICE_ROLES;

  form = this.fb.nonNullable.group({
    nameAr: ['', [Validators.required, Validators.maxLength(200)]],
    nameEn: ['', [Validators.maxLength(200)]],
    companyId: ['', Validators.required],
    branchId: ['', Validators.required],
    locationName: [''],
    posDeviceId: [''],
    chartOfAccountId: ['', Validators.required],
    currencyId: ['', Validators.required],
    openingBalance: [0 as number, [Validators.required, Validators.min(0)]],
    openingDate: ['' as string],
    description: [''],
    isActive: [true],
    allowReceive: [true],
    allowPay: [true],
    allowDeposit: [true],
    allowWithdraw: [true],
    allowTransfer: [true],
    requireShiftBeforeUse: [true],
    allowNegativeBalance: [false],
    minBalance: [null as number | null],
    maxBalance: [null as number | null],
    sortOrder: [0 as number],
    authorizedUsers: this.fb.array([]),
    devices: this.fb.array([])
  });

  canView = computed(
    () =>
      this.auth.hasPermission('CashBox.View') ||
      this.auth.hasPermission('Accounting.View') ||
      this.auth.hasPermission('VIEW_FINANCE')
  );
  canCreate = computed(
    () => this.auth.hasPermission('CashBox.Create') || this.auth.hasPermission('Accounting.Create')
  );
  canEdit = computed(
    () => this.auth.hasPermission('CashBox.Update') || this.auth.hasPermission('Accounting.Update')
  );
  canDelete = computed(
    () => this.auth.hasPermission('CashBox.Delete') || this.auth.hasPermission('Accounting.Delete')
  );

  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);
  isEditing = computed(() => !!this.editingId());
  openingLocked = computed(() => !!this.selected()?.hasHadMovement && this.isEditing());

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

  cashGlAccounts = computed(() => {
    const classIds = this.cashClassIds();
    return this.accounts().filter(a => {
      if (!a.isPostingAllowed || a.isSummaryAccount || !a.isActive) return false;
      if (Number(a.accountType) !== 1) return false;
      if (!a.accountClassificationId) return true;
      if (classIds.size === 0) return true;
      return classIds.has(a.accountClassificationId);
    });
  });

  usersFA = this.form.controls.authorizedUsers as FormArray;
  devicesFA = this.form.controls.devices as FormArray;

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
    this.repo.getDevices().subscribe({
      next: rows => this.devices.set((rows ?? []).filter(d => d.isActive !== false)),
      error: () => this.devices.set([])
    });
    this.repo.getUsers().subscribe({
      next: rows => this.users.set(rows ?? []),
      error: () => this.users.set([])
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
          list.filter(c => ['cash', 'bank'].includes((c.code || '').toLowerCase())).map(c => c.id)
        );
        this.cashClassIds.set(ids);
      },
      error: () => this.cashClassIds.set(new Set())
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

  load(): void {
    this.loading.set(true);
    this.error.set(null);
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
          if (this.selectedId() && !rows.some(r => r.id === this.selectedId())) {
            this.selectedId.set(null);
          }
        },
        error: err => {
          this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.cash.loadError'));
          this.loading.set(false);
        }
      });
  }

  select(row: CashBox): void {
    this.selectedId.set(row.id);
  }

  userLabel(u: UserLookup): string {
    const name = `${u.firstName ?? ''} ${u.lastName ?? ''}`.trim();
    return name || u.userName || u.email || u.id;
  }

  private clearArray(fa: FormArray): void {
    while (fa.length) fa.removeAt(0);
  }

  private userRow() {
    return this.fb.nonNullable.group({
      id: [null as string | null],
      userId: ['', Validators.required],
      roleName: [''],
      isDefault: [false],
      isManager: [false],
      isCustodian: [false]
    });
  }

  private deviceRow() {
    return this.fb.nonNullable.group({
      id: [null as string | null],
      deviceId: [''],
      deviceRole: [1 as number, Validators.required],
      label: ['']
    });
  }

  addUserRow(): void {
    this.usersFA.push(this.userRow());
  }

  removeUserRow(i: number): void {
    this.usersFA.removeAt(i);
  }

  addDeviceRow(): void {
    this.devicesFA.push(this.deviceRow());
  }

  removeDeviceRow(i: number): void {
    this.devicesFA.removeAt(i);
  }

  openCreate(): void {
    if (!this.canCreate()) return;
    this.editingId.set(null);
    this.formTab.set('basic');
    this.error.set(null);
    this.clearArray(this.usersFA);
    this.clearArray(this.devicesFA);
    const company = this.companies()[0];
    const companyBranches = company
      ? this.branches().filter(b => b.companyId === company.id)
      : [];
    const sar =
      this.currencies().find(c => c.code === 'SAR') ??
      this.currencies().find(c => c.isCompanyCurrency);
    this.form.reset({
      nameAr: '',
      nameEn: '',
      companyId: company?.id ?? '',
      branchId: companyBranches[0]?.id ?? '',
      locationName: '',
      posDeviceId: '',
      chartOfAccountId: '',
      currencyId: sar?.id ?? '',
      openingBalance: 0,
      openingDate: new Date().toISOString().slice(0, 10),
      description: '',
      isActive: true,
      allowReceive: true,
      allowPay: true,
      allowDeposit: true,
      allowWithdraw: true,
      allowTransfer: true,
      requireShiftBeforeUse: true,
      allowNegativeBalance: false,
      minBalance: null,
      maxBalance: null,
      sortOrder: 0
    });
    this.formCompanyId.set(company?.id ?? '');
    this.form.controls.openingBalance.enable();
    this.form.controls.chartOfAccountId.enable();
    this.showModal.set(true);
  }

  openEdit(): void {
    const row = this.selected();
    if (!row || !this.canEdit()) return;
    this.editingId.set(row.id);
    this.formTab.set('basic');
    this.error.set(null);
    this.clearArray(this.usersFA);
    this.clearArray(this.devicesFA);
    this.form.reset({
      nameAr: row.nameAr,
      nameEn: row.nameEn ?? '',
      companyId: row.companyId,
      branchId: row.branchId,
      locationName: row.locationName ?? '',
      posDeviceId: row.posDeviceId ?? '',
      chartOfAccountId: row.chartOfAccountId,
      currencyId: row.currencyId,
      openingBalance: row.openingBalance,
      openingDate: row.openingDate ?? '',
      description: row.description ?? '',
      isActive: row.isActive,
      allowReceive: row.allowReceive,
      allowPay: row.allowPay,
      allowDeposit: row.allowDeposit,
      allowWithdraw: row.allowWithdraw,
      allowTransfer: row.allowTransfer,
      requireShiftBeforeUse: row.requireShiftBeforeUse,
      allowNegativeBalance: row.allowNegativeBalance,
      minBalance: row.minBalance ?? null,
      maxBalance: row.maxBalance ?? null,
      sortOrder: row.sortOrder
    });
    this.formCompanyId.set(row.companyId);
    if (row.hasHadMovement) {
      this.form.controls.openingBalance.disable();
      this.form.controls.chartOfAccountId.disable();
    } else {
      this.form.controls.openingBalance.enable();
      this.form.controls.chartOfAccountId.enable();
    }
    for (const u of row.authorizedUsers ?? []) {
      const g = this.userRow();
      g.reset({
        id: u.id ?? null,
        userId: u.userId,
        roleName: u.roleName ?? '',
        isDefault: u.isDefault,
        isManager: u.isManager,
        isCustodian: u.isCustodian
      });
      this.usersFA.push(g);
    }
    for (const d of row.devices ?? []) {
      const g = this.deviceRow();
      g.reset({
        id: d.id ?? null,
        deviceId: d.deviceId ?? '',
        deviceRole: Number(d.deviceRole) || 1,
        label: d.label ?? ''
      });
      this.devicesFA.push(g);
    }
    this.showModal.set(true);
  }

  closeModal(): void {
    this.form.controls.openingBalance.enable();
    this.form.controls.chartOfAccountId.enable();
    this.showModal.set(false);
    this.editingId.set(null);
  }

  private toNullableNumber(v: unknown): number | null {
    if (v === null || v === undefined || v === '') return null;
    const n = Number(v);
    return Number.isFinite(n) ? n : null;
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.formTab.set('basic');
      return;
    }
    const raw = this.form.getRawValue();
    const payload: UpsertCashBoxPayload = {
      nameAr: raw.nameAr.trim(),
      nameEn: raw.nameEn.trim() || null,
      companyId: raw.companyId,
      branchId: raw.branchId,
      locationName: raw.locationName.trim() || null,
      posDeviceId: raw.posDeviceId || null,
      chartOfAccountId: raw.chartOfAccountId,
      currencyId: raw.currencyId,
      openingBalance: Number(raw.openingBalance) || 0,
      openingDate: raw.openingDate || null,
      description: raw.description.trim() || null,
      isActive: raw.isActive,
      allowReceive: raw.allowReceive,
      allowPay: raw.allowPay,
      allowDeposit: raw.allowDeposit,
      allowWithdraw: raw.allowWithdraw,
      allowTransfer: raw.allowTransfer,
      requireShiftBeforeUse: raw.requireShiftBeforeUse,
      allowNegativeBalance: raw.allowNegativeBalance,
      minBalance: this.toNullableNumber(raw.minBalance),
      maxBalance: this.toNullableNumber(raw.maxBalance),
      sortOrder: Number(raw.sortOrder) || 0,
      authorizedUsers: this.usersFA.controls.map(c => {
        const v = c.getRawValue() as Record<string, unknown>;
        return {
          id: (v['id'] as string | null) ?? null,
          userId: String(v['userId']),
          roleName: String(v['roleName'] ?? '').trim() || null,
          isDefault: !!v['isDefault'],
          isManager: !!v['isManager'],
          isCustodian: !!v['isCustodian']
        };
      }),
      devices: this.devicesFA.controls.map(c => {
        const v = c.getRawValue() as Record<string, unknown>;
        return {
          id: (v['id'] as string | null) ?? null,
          deviceId: String(v['deviceId'] ?? '') || null,
          deviceRole: Number(v['deviceRole']) || 1,
          label: String(v['label'] ?? '').trim() || null
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
        this.closeModal();
        this.selectedId.set(saved.id);
        this.load();
      },
      error: err => {
        this.saving.set(false);
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.cash.saveError'));
      }
    });
  }

  remove(): void {
    const row = this.selected();
    if (!row || !this.canDelete() || row.isSystem || row.hasHadMovement) return;
    if (!confirm(this.t('fin.cash.confirmDelete'))) return;
    this.repo.delete(row.id).subscribe({
      next: () => {
        this.selectedId.set(null);
        this.load();
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.cash.deleteError'));
      }
    });
  }

  glLabel(a: ChartAccount): string {
    return `${a.accountNumber} — ${a.nameAr}`;
  }

  formatDate(value?: string | null): string {
    if (!value) return '—';
    return value.slice(0, 16).replace('T', ' ');
  }
}
