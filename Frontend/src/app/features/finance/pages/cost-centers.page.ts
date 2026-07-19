import {
  ChangeDetectionStrategy,
  Component,
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
import { CostCenterRepository } from '../../../core/repositories/cost-center.repository';
import { ChartOfAccountRepository } from '../../../core/repositories/chart-of-account.repository';
import { COST_CENTER_TYPES, CostCenter } from '../../../core/models/cost-center.models';
import { flattenTreeAccounts } from './coa-tree.util';
import { ChartAccount } from '../../../core/models/chart-of-account.models';

@Component({
  selector: 'app-cost-centers-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatIconModule, MatTooltipModule],
  templateUrl: './cost-centers.page.html',
  styleUrl: './cost-centers.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CostCentersPage implements OnInit {
  private repo = inject(CostCenterRepository);
  private accountsRepo = inject(ChartOfAccountRepository);
  private fb = inject(FormBuilder);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  rows = signal<CostCenter[]>([]);
  accounts = signal<ChartAccount[]>([]);
  selectedId = signal<string | null>(null);
  search = signal('');
  showModal = signal(false);
  editingId = signal<string | null>(null);
  formTab = signal<'basic' | 'accounts' | 'modules'>('basic');

  types = COST_CENTER_TYPES;

  form = this.fb.nonNullable.group({
    nameAr: ['', [Validators.required, Validators.maxLength(200)]],
    nameEn: ['', [Validators.maxLength(200)]],
    code: [''],
    costCenterType: [1 as number, Validators.required],
    parentCostCenterId: ['' as string],
    description: [''],
    isActive: [true],
    useInPurchases: [true],
    useInInventory: [true],
    useInProduction: [true],
    useInSales: [true],
    useInPayroll: [true],
    useInAssets: [true],
    useInMaintenance: [true],
    useInJournals: [true],
    allowedAccountIds: [[] as string[]]
  });

  canView = computed(
    () =>
      this.auth.hasPermission('CostCenter.View') ||
      this.auth.hasPermission('Accounting.View') ||
      this.auth.hasPermission('VIEW_FINANCE')
  );
  canCreate = computed(
    () => this.auth.hasPermission('CostCenter.Create') || this.auth.hasPermission('Accounting.Create')
  );
  canEdit = computed(
    () => this.auth.hasPermission('CostCenter.Update') || this.auth.hasPermission('Accounting.Update')
  );
  canDelete = computed(
    () => this.auth.hasPermission('CostCenter.Delete') || this.auth.hasPermission('Accounting.Delete')
  );
  canExport = computed(
    () => this.auth.hasPermission('CostCenter.Export') || this.auth.hasPermission('CostCenter.View')
  );

  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);
  isEditing = computed(() => !!this.editingId());
  parentOptions = computed(() => this.rows().filter(r => r.id !== this.editingId()));

  postingAccounts = computed(() => this.accounts().filter(a => a.isPostingAllowed && !a.isSummaryAccount));

  t = (key: string) => this.lang.t(key);

  ngOnInit(): void {
    this.accountsRepo.getTree({ includeInactive: false }).subscribe({
      next: tree => this.accounts.set(flattenTreeAccounts(tree)),
      error: () => this.accounts.set([])
    });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo.getList({ search: this.search().trim() || undefined }).subscribe({
      next: rows => {
        this.rows.set(rows);
        this.loading.set(false);
        if (this.selectedId() && !rows.some(r => r.id === this.selectedId())) {
          this.selectedId.set(null);
        }
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.cc.loadError'));
        this.loading.set(false);
      }
    });
  }

  select(row: CostCenter): void {
    this.selectedId.set(row.id);
  }

  typeLabel(type: number | string): string {
    const byValue = COST_CENTER_TYPES.find(t => t.value === Number(type));
    if (byValue) return this.t(byValue.labelKey);
    const byCode = COST_CENTER_TYPES.find(t => t.code === String(type));
    return byCode ? this.t(byCode.labelKey) : String(type);
  }

  openCreate(): void {
    if (!this.canCreate()) return;
    this.editingId.set(null);
    this.formTab.set('basic');
    this.form.controls.code.enable();
    this.form.reset({
      nameAr: '',
      nameEn: '',
      code: '',
      costCenterType: 1,
      parentCostCenterId: '',
      description: '',
      isActive: true,
      useInPurchases: true,
      useInInventory: true,
      useInProduction: true,
      useInSales: true,
      useInPayroll: true,
      useInAssets: true,
      useInMaintenance: true,
      useInJournals: true,
      allowedAccountIds: []
    });
    this.showModal.set(true);
  }

  openEdit(): void {
    const row = this.selected();
    if (!row || !this.canEdit()) return;
    this.editingId.set(row.id);
    this.formTab.set('basic');
    this.form.setValue({
      nameAr: row.nameAr,
      nameEn: row.nameEn ?? '',
      code: row.code,
      costCenterType: Number(row.costCenterType),
      parentCostCenterId: row.parentCostCenterId ?? '',
      description: row.description ?? '',
      isActive: row.isActive,
      useInPurchases: row.useInPurchases,
      useInInventory: row.useInInventory,
      useInProduction: row.useInProduction,
      useInSales: row.useInSales,
      useInPayroll: row.useInPayroll,
      useInAssets: row.useInAssets,
      useInMaintenance: row.useInMaintenance,
      useInJournals: row.useInJournals,
      allowedAccountIds: [...(row.allowedAccountIds ?? [])]
    });
    this.form.controls.code.disable();
    this.showModal.set(true);
  }

  closeModal(): void {
    this.form.controls.code.enable();
    this.showModal.set(false);
    this.editingId.set(null);
  }

  toggleAccount(id: string, checked: boolean): void {
    const current = [...this.form.controls.allowedAccountIds.value];
    if (checked && !current.includes(id)) current.push(id);
    if (!checked) {
      const i = current.indexOf(id);
      if (i >= 0) current.splice(i, 1);
    }
    this.form.controls.allowedAccountIds.setValue(current);
  }

  isAccountSelected(id: string): boolean {
    return this.form.controls.allowedAccountIds.value.includes(id);
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.formTab.set('basic');
      return;
    }
    const raw = this.form.getRawValue();
    const payload = {
      nameAr: raw.nameAr.trim(),
      nameEn: raw.nameEn.trim() || null,
      code: raw.code.trim() || null,
      costCenterType: Number(raw.costCenterType),
      parentCostCenterId: raw.parentCostCenterId || null,
      description: raw.description.trim() || null,
      useInPurchases: raw.useInPurchases,
      useInInventory: raw.useInInventory,
      useInProduction: raw.useInProduction,
      useInSales: raw.useInSales,
      useInPayroll: raw.useInPayroll,
      useInAssets: raw.useInAssets,
      useInMaintenance: raw.useInMaintenance,
      useInJournals: raw.useInJournals,
      allowedAccountIds: raw.allowedAccountIds
    };

    this.saving.set(true);
    this.error.set(null);

    const req = this.editingId()
      ? this.repo.update(this.editingId()!, payload)
      : this.repo.create(payload);

    req.subscribe({
      next: saved => {
        this.saving.set(false);
        const id = saved.id;
        if (this.editingId() && !raw.isActive && saved.isActive) {
          this.repo.deactivate(id).subscribe({ next: () => this.afterSave(id), error: err => this.fail(err) });
          return;
        }
        if (this.editingId() && raw.isActive && !saved.isActive) {
          this.repo.activate(id).subscribe({ next: () => this.afterSave(id), error: err => this.fail(err) });
          return;
        }
        this.afterSave(id);
      },
      error: err => this.fail(err)
    });
  }

  remove(): void {
    const row = this.selected();
    if (!row || !this.canDelete() || row.isSystem) return;
    if (!confirm(this.t('fin.cc.confirmDelete'))) return;
    this.saving.set(true);
    this.repo.delete(row.id).subscribe({
      next: () => {
        this.saving.set(false);
        this.selectedId.set(null);
        this.load();
      },
      error: err => {
        this.saving.set(false);
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.cc.deleteError'));
      }
    });
  }

  deactivateSelected(): void {
    const row = this.selected();
    if (!row || !this.canEdit()) return;
    this.repo.deactivate(row.id).subscribe({
      next: () => this.load(),
      error: err => this.fail(err)
    });
  }

  activateSelected(): void {
    const row = this.selected();
    if (!row || !this.canEdit()) return;
    this.repo.activate(row.id).subscribe({
      next: () => this.load(),
      error: err => this.fail(err)
    });
  }

  exportExcel(): void {
    this.repo.exportCsv(this.search()).subscribe({
      next: blob => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'cost-centers.csv';
        a.click();
        URL.revokeObjectURL(url);
      },
      error: err => this.fail(err)
    });
  }

  private afterSave(id: string): void {
    this.saving.set(false);
    this.closeModal();
    this.selectedId.set(id);
    this.load();
  }

  private fail(err: unknown): void {
    this.saving.set(false);
    const e = err as { error?: { detail?: string; title?: string; error?: string }; message?: string };
    this.error.set(e?.error?.detail || e?.error?.title || e?.error?.error || e?.message || this.t('fin.cc.saveError'));
  }
}
