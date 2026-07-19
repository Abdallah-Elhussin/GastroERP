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
import { DocumentTypeRepository } from '../../../core/repositories/document-type.repository';
import {
  APPROVAL_MODES,
  DOCUMENT_MODULES,
  DocumentType,
  POSTING_MODES
} from '../../../core/models/document-type.models';

type FormTab = 'basic' | 'numbering' | 'lifecycle' | 'approval' | 'posting' | 'impact' | 'permissions' | 'extras';

@Component({
  selector: 'app-document-types-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatIconModule, MatTooltipModule],
  templateUrl: './document-types.page.html',
  styleUrl: './document-types.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DocumentTypesPage implements OnInit {
  private repo = inject(DocumentTypeRepository);
  private fb = inject(FormBuilder);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  rows = signal<DocumentType[]>([]);
  selectedId = signal<string | null>(null);
  search = signal('');
  showModal = signal(false);
  editingId = signal<string | null>(null);
  formTab = signal<FormTab>('basic');

  modules = DOCUMENT_MODULES;
  approvalModes = APPROVAL_MODES;
  postingModes = POSTING_MODES;

  form = this.fb.nonNullable.group({
    code: ['', [Validators.required, Validators.maxLength(30)]],
    nameAr: ['', [Validators.required, Validators.maxLength(200)]],
    nameEn: ['', [Validators.required, Validators.maxLength(200)]],
    module: [1 as number, Validators.required],
    description: [''],
    prefix: ['', [Validators.required, Validators.maxLength(20)]],
    suffix: [''],
    startingNumber: [1 as number],
    lastNumber: [0 as number],
    numberLength: [6 as number],
    resetYearly: [false],
    resetMonthly: [false],
    numberPerBranch: [false],
    numberPerCompany: [false],
    approvalMode: [0 as number],
    requiresApproval: [false],
    usesWorkflow: [false],
    postingMode: [0 as number],
    autoPost: [false],
    postAfterApproval: [false],
    affectsInventory: [false],
    affectsCost: [false],
    affectsAccounting: [false],
    affectsCash: [false],
    affectsCustomers: [false],
    affectsSuppliers: [false],
    affectsAssets: [false],
    affectsPayroll: [false],
    allowCreate: [true],
    allowUpdate: [true],
    allowApprove: [true],
    allowPost: [true],
    allowCancel: [true],
    allowDelete: [false],
    allowAttachments: [true],
    allowPrint: [true],
    allowEditAfterSave: [true],
    allowDeleteDocuments: [false],
    allowCancelDocuments: [true],
    allowCopy: [true],
    allowReopen: [false],
    showInReports: [true],
    showInDashboard: [false],
    isActive: [true],
    sortOrder: [0 as number],
    lifecycleStages: this.fb.array([])
  });

  canView = computed(
    () =>
      this.auth.hasPermission('DocumentType.View') ||
      this.auth.hasPermission('Accounting.View') ||
      this.auth.hasPermission('VIEW_FINANCE')
  );
  canCreate = computed(
    () => this.auth.hasPermission('DocumentType.Create') || this.auth.hasPermission('Accounting.Create')
  );
  canEdit = computed(
    () => this.auth.hasPermission('DocumentType.Update') || this.auth.hasPermission('Accounting.Update')
  );
  canDelete = computed(
    () => this.auth.hasPermission('DocumentType.Delete') || this.auth.hasPermission('Accounting.Delete')
  );

  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);
  isEditing = computed(() => !!this.editingId());
  lifecycle = computed(() => this.form.controls.lifecycleStages as FormArray);

  t = (key: string) => this.lang.t(key);

  ngOnInit(): void {
    this.load();
  }

  get stages(): FormArray {
    return this.form.controls.lifecycleStages as FormArray;
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo.getList({ search: this.search().trim() || undefined }).subscribe({
      next: rows => {
        this.rows.set(rows);
        this.loading.set(false);
        if (this.selectedId() && !rows.some(r => r.id === this.selectedId())) this.selectedId.set(null);
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.doc.loadError'));
        this.loading.set(false);
      }
    });
  }

  select(row: DocumentType): void {
    this.selectedId.set(row.id);
  }

  openCreate(): void {
    if (!this.canCreate()) return;
    this.editingId.set(null);
    this.formTab.set('basic');
    this.resetForm();
    this.setDefaultStages();
    this.form.controls.code.enable();
    this.form.controls.module.enable();
    this.showModal.set(true);
  }

  openEdit(): void {
    const row = this.selected();
    if (!row || !this.canEdit()) return;
    this.editingId.set(row.id);
    this.formTab.set('basic');
    this.patchForm(row);
    this.form.controls.code.disable();
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
    this.editingId.set(null);
    this.error.set(null);
  }

  addStage(): void {
    this.stages.push(
      this.fb.nonNullable.group({
        code: ['', Validators.required],
        nameAr: ['', Validators.required],
        nameEn: [''],
        sortOrder: [this.stages.length],
        isTerminal: [false]
      })
    );
  }

  removeStage(i: number): void {
    this.stages.removeAt(i);
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.formTab.set('basic');
      return;
    }
    const raw = this.form.getRawValue();
    const stages = (raw.lifecycleStages as Array<{
      code: string;
      nameAr: string;
      nameEn: string;
      sortOrder: number;
      isTerminal: boolean;
    }>).map((s, i) => ({
      code: s.code.trim(),
      nameAr: s.nameAr.trim(),
      nameEn: (s.nameEn || s.code).trim(),
      sortOrder: s.sortOrder ?? i,
      isTerminal: !!s.isTerminal
    }));
    const payload = {
      ...raw,
      code: raw.code.trim().toUpperCase(),
      prefix: raw.prefix.trim().toUpperCase(),
      suffix: raw.suffix.trim() || null,
      description: raw.description.trim() || null,
      lifecycleStages: stages
    };

    this.saving.set(true);
    const req = this.editingId()
      ? this.repo.update(this.editingId()!, payload as never)
      : this.repo.create(payload as never);

    req.subscribe({
      next: () => {
        this.saving.set(false);
        this.closeModal();
        this.load();
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.doc.saveError'));
        this.saving.set(false);
      }
    });
  }

  remove(): void {
    const row = this.selected();
    if (!row || !this.canDelete() || row.isSystem) return;
    if (!confirm(this.t('fin.doc.confirmDelete'))) return;
    this.repo.delete(row.id).subscribe({
      next: () => {
        this.selectedId.set(null);
        this.load();
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.doc.deleteError'));
      }
    });
  }

  copySelected(): void {
    const row = this.selected();
    if (!row || !this.canCreate()) return;
    const code = prompt(this.t('fin.doc.copyCodePrompt'), `${row.code}_COPY`);
    if (!code?.trim()) return;
    this.repo
      .copy(row.id, {
        newCode: code.trim().toUpperCase(),
        nameAr: `${row.nameAr} (نسخة)`,
        nameEn: `${row.nameEn} (Copy)`,
        prefix: `${row.prefix}X`
      })
      .subscribe({
        next: () => this.load(),
        error: err => {
          this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.doc.saveError'));
        }
      });
  }

  yesNo(v: boolean): string {
    return v ? this.t('common.yes') : this.t('common.no');
  }

  private resetForm(): void {
    this.form.reset({
      code: '',
      nameAr: '',
      nameEn: '',
      module: 1,
      description: '',
      prefix: '',
      suffix: '',
      startingNumber: 1,
      lastNumber: 0,
      numberLength: 6,
      resetYearly: false,
      resetMonthly: false,
      numberPerBranch: false,
      numberPerCompany: false,
      approvalMode: 0,
      requiresApproval: false,
      usesWorkflow: false,
      postingMode: 0,
      autoPost: false,
      postAfterApproval: false,
      affectsInventory: false,
      affectsCost: false,
      affectsAccounting: false,
      affectsCash: false,
      affectsCustomers: false,
      affectsSuppliers: false,
      affectsAssets: false,
      affectsPayroll: false,
      allowCreate: true,
      allowUpdate: true,
      allowApprove: true,
      allowPost: true,
      allowCancel: true,
      allowDelete: false,
      allowAttachments: true,
      allowPrint: true,
      allowEditAfterSave: true,
      allowDeleteDocuments: false,
      allowCancelDocuments: true,
      allowCopy: true,
      allowReopen: false,
      showInReports: true,
      showInDashboard: false,
      isActive: true,
      sortOrder: 0
    });
    this.stages.clear();
  }

  private patchForm(row: DocumentType): void {
    this.form.patchValue({
      code: row.code,
      nameAr: row.nameAr,
      nameEn: row.nameEn,
      module: row.module,
      description: row.description ?? '',
      prefix: row.prefix,
      suffix: row.suffix ?? '',
      startingNumber: row.startingNumber,
      lastNumber: row.lastNumber,
      numberLength: row.numberLength,
      resetYearly: row.resetYearly,
      resetMonthly: row.resetMonthly,
      numberPerBranch: row.numberPerBranch,
      numberPerCompany: row.numberPerCompany,
      approvalMode: row.approvalMode,
      requiresApproval: row.requiresApproval,
      usesWorkflow: row.usesWorkflow,
      postingMode: row.postingMode,
      autoPost: row.autoPost,
      postAfterApproval: row.postAfterApproval,
      affectsInventory: row.affectsInventory,
      affectsCost: row.affectsCost,
      affectsAccounting: row.affectsAccounting,
      affectsCash: row.affectsCash,
      affectsCustomers: row.affectsCustomers,
      affectsSuppliers: row.affectsSuppliers,
      affectsAssets: row.affectsAssets,
      affectsPayroll: row.affectsPayroll,
      allowCreate: row.allowCreate,
      allowUpdate: row.allowUpdate,
      allowApprove: row.allowApprove,
      allowPost: row.allowPost,
      allowCancel: row.allowCancel,
      allowDelete: row.allowDelete,
      allowAttachments: row.allowAttachments,
      allowPrint: row.allowPrint,
      allowEditAfterSave: row.allowEditAfterSave,
      allowDeleteDocuments: row.allowDeleteDocuments,
      allowCancelDocuments: row.allowCancelDocuments,
      allowCopy: row.allowCopy,
      allowReopen: row.allowReopen,
      showInReports: row.showInReports,
      showInDashboard: row.showInDashboard,
      isActive: row.isActive,
      sortOrder: row.sortOrder
    });
    this.stages.clear();
    for (const s of row.lifecycleStages ?? []) {
      this.stages.push(
        this.fb.nonNullable.group({
          code: [s.code, Validators.required],
          nameAr: [s.nameAr, Validators.required],
          nameEn: [s.nameEn],
          sortOrder: [s.sortOrder],
          isTerminal: [s.isTerminal]
        })
      );
    }
  }

  private setDefaultStages(): void {
    const defaults = [
      { code: 'Draft', nameAr: 'مسودة', nameEn: 'Draft', sortOrder: 0, isTerminal: false },
      { code: 'Submitted', nameAr: 'مقدم', nameEn: 'Submitted', sortOrder: 1, isTerminal: false },
      { code: 'Approved', nameAr: 'معتمد', nameEn: 'Approved', sortOrder: 2, isTerminal: false },
      { code: 'Posted', nameAr: 'مرحّل', nameEn: 'Posted', sortOrder: 3, isTerminal: false },
      { code: 'Closed', nameAr: 'مغلق', nameEn: 'Closed', sortOrder: 4, isTerminal: true },
      { code: 'Cancelled', nameAr: 'ملغى', nameEn: 'Cancelled', sortOrder: 5, isTerminal: true }
    ];
    for (const s of defaults) {
      this.stages.push(
        this.fb.nonNullable.group({
          code: [s.code, Validators.required],
          nameAr: [s.nameAr, Validators.required],
          nameEn: [s.nameEn],
          sortOrder: [s.sortOrder],
          isTerminal: [s.isTerminal]
        })
      );
    }
  }
}
