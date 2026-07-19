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
import { TaxRegistrationRepository } from '../../../core/repositories/tax-registration.repository';
import {
  TAX_ACTIVITY_CODES,
  TAX_REG_STATUSES,
  TAXPAYER_TYPES,
  OrgBranchLookup,
  OrgCompanyLookup,
  TaxRegistrationProfile,
  UpsertTaxRegistrationPayload
} from '../../../core/models/tax-registration.models';

@Component({
  selector: 'app-tax-registrations-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatIconModule, MatTooltipModule],
  templateUrl: './tax-registrations.page.html',
  styleUrl: './tax-registrations.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TaxRegistrationsPage implements OnInit {
  private repo = inject(TaxRegistrationRepository);
  private fb = inject(FormBuilder);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  rows = signal<TaxRegistrationProfile[]>([]);
  companies = signal<OrgCompanyLookup[]>([]);
  branches = signal<OrgBranchLookup[]>([]);

  selectedId = signal<string | null>(null);
  search = signal('');
  filterCompanyId = signal('');
  filterBranchId = signal('');
  showModal = signal(false);
  editingId = signal<string | null>(null);
  formCompanyId = signal('');
  pendingFile = signal<File | null>(null);

  taxpayerTypes = TAXPAYER_TYPES;
  statuses = TAX_REG_STATUSES;
  activityCodes = TAX_ACTIVITY_CODES;

  form = this.fb.nonNullable.group({
    companyId: ['', Validators.required],
    branchId: [''],
    vatNumber: ['', [Validators.required, Validators.maxLength(30)]],
    branchVatNumber: [''],
    taxOffice: [''],
    taxpayerType: [1 as number, Validators.required],
    activityCode: [''],
    activityNameAr: [''],
    activityNameEn: [''],
    defaultTaxRate: [15 as number, [Validators.required, Validators.min(0), Validators.max(100)]],
    registrationDate: ['' as string],
    expiryDate: ['' as string],
    status: [1 as number, Validators.required],
    notes: [''],
    certificateDocumentNumber: [''],
    certificateIssueDate: ['' as string],
    certificateExpiryDate: ['' as string],
    certificateNotes: [''],
    sortOrder: [0 as number]
  });

  canView = computed(
    () =>
      this.auth.hasPermission('TaxRegistration.View') ||
      this.auth.hasPermission('Accounting.View') ||
      this.auth.hasPermission('VIEW_FINANCE')
  );
  canCreate = computed(
    () =>
      this.auth.hasPermission('TaxRegistration.Create') || this.auth.hasPermission('Accounting.Create')
  );
  canEdit = computed(
    () =>
      this.auth.hasPermission('TaxRegistration.Update') || this.auth.hasPermission('Accounting.Update')
  );
  canDelete = computed(
    () =>
      this.auth.hasPermission('TaxRegistration.Delete') || this.auth.hasPermission('Accounting.Delete')
  );
  canUpload = computed(
    () =>
      this.auth.hasPermission('TaxRegistration.UploadCertificate') ||
      this.auth.hasPermission('TaxRegistration.Update') ||
      this.auth.hasPermission('Accounting.Update')
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

    this.form.controls.companyId.valueChanges.subscribe(companyId => {
      this.formCompanyId.set(companyId || '');
      const branchId = this.form.controls.branchId.value;
      if (branchId && !this.branches().some(b => b.id === branchId && b.companyId === companyId)) {
        this.form.controls.branchId.setValue('');
      }
    });

    this.form.controls.activityCode.valueChanges.subscribe(code => {
      const match = this.activityCodes.find(a => a.code === code);
      if (match) {
        this.form.controls.activityNameAr.setValue(match.nameAr, { emitEvent: false });
        this.form.controls.activityNameEn.setValue(match.nameEn, { emitEvent: false });
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
          this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.tax.loadError'));
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

  select(row: TaxRegistrationProfile): void {
    this.selectedId.set(row.id);
  }

  typeLabel(value: number | string): string {
    const item = this.taxpayerTypes.find(t => t.value === Number(value));
    return item ? this.t(item.labelKey) : String(value);
  }

  statusLabel(value: number | string): string {
    const item = this.statuses.find(s => s.value === Number(value));
    return item ? this.t(item.labelKey) : String(value);
  }

  formatDate(value?: string | null): string {
    if (!value) return '—';
    return value.slice(0, 16).replace('T', ' ');
  }

  openCreate(): void {
    if (!this.canCreate()) return;
    this.editingId.set(null);
    this.error.set(null);
    this.pendingFile.set(null);
    const company = this.companies()[0];
    this.form.reset({
      companyId: company?.id ?? '',
      branchId: '',
      vatNumber: '',
      branchVatNumber: '',
      taxOffice: '',
      taxpayerType: 1,
      activityCode: '561001',
      activityNameAr: 'أنشطة المطاعم',
      activityNameEn: 'Restaurants activities',
      defaultTaxRate: 15,
      registrationDate: new Date().toISOString().slice(0, 10),
      expiryDate: '',
      status: 1,
      notes: '',
      certificateDocumentNumber: '',
      certificateIssueDate: '',
      certificateExpiryDate: '',
      certificateNotes: '',
      sortOrder: 0
    });
    this.formCompanyId.set(company?.id ?? '');
    this.showModal.set(true);
  }

  openEdit(): void {
    const row = this.selected();
    if (!row || !this.canEdit()) return;
    this.editingId.set(row.id);
    this.error.set(null);
    this.pendingFile.set(null);
    const cert = row.currentCertificate;
    this.form.reset({
      companyId: row.companyId,
      branchId: row.branchId ?? '',
      vatNumber: row.vatNumber,
      branchVatNumber: row.branchVatNumber ?? '',
      taxOffice: row.taxOffice ?? '',
      taxpayerType: Number(row.taxpayerType),
      activityCode: row.activityCode ?? '',
      activityNameAr: row.activityNameAr ?? '',
      activityNameEn: row.activityNameEn ?? '',
      defaultTaxRate: row.defaultTaxRate,
      registrationDate: row.registrationDate ?? '',
      expiryDate: row.expiryDate ?? '',
      status: Number(row.status),
      notes: row.notes ?? '',
      certificateDocumentNumber: cert?.documentNumber ?? '',
      certificateIssueDate: cert?.issueDate ?? '',
      certificateExpiryDate: cert?.expiryDate ?? '',
      certificateNotes: cert?.notes ?? '',
      sortOrder: row.sortOrder
    });
    this.formCompanyId.set(row.companyId);
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
    this.editingId.set(null);
    this.pendingFile.set(null);
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    if (!file) return;
    const ok = /\.(pdf|jpg|jpeg|png)$/i.test(file.name);
    if (!ok) {
      this.error.set(this.t('fin.tax.certificateInvalid'));
      input.value = '';
      return;
    }
    this.pendingFile.set(file);
    this.error.set(null);
  }

  clearFile(): void {
    this.pendingFile.set(null);
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    const payload: UpsertTaxRegistrationPayload = {
      companyId: raw.companyId,
      branchId: raw.branchId || null,
      vatNumber: raw.vatNumber.trim(),
      branchVatNumber: raw.branchVatNumber.trim() || null,
      taxOffice: raw.taxOffice.trim() || null,
      taxpayerType: Number(raw.taxpayerType),
      activityCode: raw.activityCode.trim() || null,
      activityNameAr: raw.activityNameAr.trim() || null,
      activityNameEn: raw.activityNameEn.trim() || null,
      defaultTaxRate: Number(raw.defaultTaxRate),
      registrationDate: raw.registrationDate || null,
      expiryDate: raw.expiryDate || null,
      status: Number(raw.status),
      notes: raw.notes.trim() || null,
      sortOrder: Number(raw.sortOrder) || 0,
      certificateDocumentNumber: raw.certificateDocumentNumber.trim() || null,
      certificateIssueDate: raw.certificateIssueDate || null,
      certificateExpiryDate: raw.certificateExpiryDate || null,
      certificateNotes: raw.certificateNotes.trim() || null
    };

    this.saving.set(true);
    this.error.set(null);
    const req = this.editingId()
      ? this.repo.update(this.editingId()!, payload)
      : this.repo.create(payload);

    req.subscribe({
      next: saved => {
        const file = this.pendingFile();
        if (file && this.canUpload()) {
          this.repo
            .uploadCertificate(saved.id, file, {
              documentNumber: payload.certificateDocumentNumber,
              issueDate: payload.certificateIssueDate,
              expiryDate: payload.certificateExpiryDate,
              notes: payload.certificateNotes
            })
            .subscribe({
              next: () => this.afterSave(saved.id),
              error: err => {
                this.saving.set(false);
                this.error.set(
                  err?.error?.detail || err?.error?.error || this.t('fin.tax.certificateUploadError')
                );
                this.afterSave(saved.id);
              }
            });
          return;
        }
        this.afterSave(saved.id);
      },
      error: err => {
        this.saving.set(false);
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.tax.saveError'));
      }
    });
  }

  private afterSave(id: string): void {
    this.saving.set(false);
    this.closeModal();
    this.selectedId.set(id);
    this.load();
  }

  remove(): void {
    const row = this.selected();
    if (!row || !this.canDelete() || row.isSystem || row.hasBeenUsed) return;
    if (!confirm(this.t('fin.tax.confirmDelete'))) return;
    this.repo.delete(row.id).subscribe({
      next: () => {
        this.selectedId.set(null);
        this.load();
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.tax.deleteError'));
      }
    });
  }
}
