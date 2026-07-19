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
import { AccountClassificationRepository } from '../../../core/repositories/account-classification.repository';
import {
  AccountClassification,
  AccountMainClassification
} from '../../../core/models/account-classification.models';

@Component({
  selector: 'app-account-classifications-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatIconModule, MatTooltipModule],
  templateUrl: './account-classifications.page.html',
  styleUrl: './account-classifications.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AccountClassificationsPage implements OnInit {
  private repo = inject(AccountClassificationRepository);
  private fb = inject(FormBuilder);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  rows = signal<AccountClassification[]>([]);
  mains = signal<AccountMainClassification[]>([]);
  selectedId = signal<string | null>(null);
  search = signal('');
  showModal = signal(false);
  editingId = signal<string | null>(null);

  form = this.fb.nonNullable.group({
    nameAr: ['', [Validators.required, Validators.maxLength(200)]],
    nameEn: ['', [Validators.required, Validators.maxLength(200)]],
    mainClassificationId: ['', Validators.required]
  });

  canView = computed(
    () =>
      this.auth.hasPermission('Accounting.Classifications.View') ||
      this.auth.hasPermission('Accounting.View') ||
      this.auth.hasPermission('VIEW_FINANCE')
  );
  canCreate = computed(
    () =>
      this.auth.hasPermission('Accounting.Classifications.Create') ||
      this.auth.hasPermission('Accounting.Create') ||
      this.auth.hasPermission('Accounting.Update')
  );
  canEdit = computed(
    () =>
      this.auth.hasPermission('Accounting.Classifications.Update') ||
      this.auth.hasPermission('Accounting.Update')
  );
  canDelete = computed(
    () =>
      this.auth.hasPermission('Accounting.Classifications.Delete') ||
      this.auth.hasPermission('Accounting.Delete')
  );

  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);
  isEditing = computed(() => !!this.editingId());
  audit = computed(() => this.selected());

  t = (key: string) => this.lang.t(key);

  ngOnInit(): void {
    this.repo.getMains().subscribe({
      next: m => this.mains.set(m),
      error: () => this.mains.set([])
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
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.cls.loadError'));
        this.loading.set(false);
      }
    });
  }

  select(row: AccountClassification): void {
    this.selectedId.set(row.id);
  }

  openCreate(): void {
    if (!this.canCreate()) return;
    this.editingId.set(null);
    this.form.reset({
      nameAr: '',
      nameEn: '',
      mainClassificationId: this.mains()[0]?.id ?? ''
    });
    this.showModal.set(true);
  }

  openEdit(): void {
    const row = this.selected();
    if (!row || !this.canEdit()) return;
    this.editingId.set(row.id);
    this.form.setValue({
      nameAr: row.nameAr,
      nameEn: row.nameEn,
      mainClassificationId: row.mainClassificationId
    });
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
    this.editingId.set(null);
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    this.saving.set(true);
    this.error.set(null);
    const req = this.editingId()
      ? this.repo.update(this.editingId()!, value)
      : this.repo.create(value);

    req.subscribe({
      next: saved => {
        this.saving.set(false);
        this.showModal.set(false);
        this.selectedId.set(saved.id);
        this.load();
      },
      error: err => {
        this.saving.set(false);
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.cls.saveError'));
      }
    });
  }

  remove(): void {
    const row = this.selected();
    if (!row || !this.canDelete() || row.isDefault || row.isSystem) return;
    if (!confirm(this.t('fin.cls.confirmDelete'))) return;
    this.saving.set(true);
    this.repo.delete(row.id).subscribe({
      next: () => {
        this.saving.set(false);
        this.selectedId.set(null);
        this.load();
      },
      error: err => {
        this.saving.set(false);
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.cls.deleteError'));
      }
    });
  }

  formatDate(value?: string | null): string {
    if (!value) return '—';
    try {
      return new Date(value).toLocaleString(this.lang.language() === 'ar' ? 'ar-SA' : 'en-GB');
    } catch {
      return value;
    }
  }
}
