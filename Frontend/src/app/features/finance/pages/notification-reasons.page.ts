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
import { NotificationReasonRepository } from '../../../core/repositories/notification-reason.repository';
import { ChartOfAccountRepository } from '../../../core/repositories/chart-of-account.repository';
import { AccountClassificationRepository } from '../../../core/repositories/account-classification.repository';
import {
  NOTIFICATION_NOTE_TYPES,
  NOTIFICATION_PARTY_TYPES,
  PARTY_CLASSIFICATION_CODE,
  NotificationPartyType,
  NotificationReason,
  UpsertNotificationReasonPayload
} from '../../../core/models/notification-reason.models';
import { ChartAccount } from '../../../core/models/chart-of-account.models';
import { flattenTreeAccounts } from './coa-tree.util';

@Component({
  selector: 'app-notification-reasons-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatIconModule, MatTooltipModule],
  templateUrl: './notification-reasons.page.html',
  styleUrl: './notification-reasons.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NotificationReasonsPage implements OnInit {
  private repo = inject(NotificationReasonRepository);
  private accountsRepo = inject(ChartOfAccountRepository);
  private classificationsRepo = inject(AccountClassificationRepository);
  private fb = inject(FormBuilder);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  rows = signal<NotificationReason[]>([]);
  accounts = signal<ChartAccount[]>([]);
  classificationCodeById = signal<Map<string, string>>(new Map());

  selectedId = signal<string | null>(null);
  search = signal('');
  filterNoteType = signal('');
  filterPartyType = signal('');
  filterActive = signal('');
  showModal = signal(false);
  editingId = signal<string | null>(null);
  closeAfterSave = signal(false);
  formPartyType = signal<NotificationPartyType>(1);

  noteTypes = NOTIFICATION_NOTE_TYPES;
  partyTypes = NOTIFICATION_PARTY_TYPES;

  form = this.fb.nonNullable.group({
    code: ['', [Validators.required, Validators.maxLength(30)]],
    nameAr: ['', [Validators.required, Validators.maxLength(200)]],
    nameEn: ['', [Validators.maxLength(200)]],
    noteType: [2 as number, Validators.required],
    partyType: [1 as number, Validators.required],
    counterpartAccountId: ['', Validators.required],
    usesTax: [false],
    isActive: [true]
  });

  canView = computed(
    () =>
      this.auth.hasPermission('Settings.NotificationReasons.View') ||
      this.auth.hasPermission('Accounting.View') ||
      this.auth.hasPermission('VIEW_FINANCE')
  );
  canCreate = computed(
    () =>
      this.auth.hasPermission('Settings.NotificationReasons.Create') ||
      this.auth.hasPermission('Accounting.Create')
  );
  canEdit = computed(
    () =>
      this.auth.hasPermission('Settings.NotificationReasons.Edit') ||
      this.auth.hasPermission('Accounting.Update')
  );
  canDelete = computed(
    () =>
      this.auth.hasPermission('Settings.NotificationReasons.Delete') ||
      this.auth.hasPermission('Accounting.Delete')
  );

  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);
  isEditing = computed(() => !!this.editingId());

  filteredAccounts = computed(() => {
    const party = this.formPartyType();
    const classMap = this.classificationCodeById();
    const expected = PARTY_CLASSIFICATION_CODE[party];
    return this.accounts().filter(a => {
      if (!a.isPostingAllowed || a.isSummaryAccount || !a.isActive) return false;
      if (!expected) return true;
      if (!a.accountClassificationId) return true;
      return classMap.get(a.accountClassificationId)?.toLowerCase() === expected;
    });
  });

  accountNumberDisplay = computed(() => {
    const id = this.form.controls.counterpartAccountId.value;
    const a = this.accounts().find(x => x.id === id);
    return a?.accountNumber ?? '';
  });

  t = (key: string) => this.lang.t(key);

  ngOnInit(): void {
    this.accountsRepo.getTree({ includeInactive: false }).subscribe({
      next: tree => this.accounts.set(flattenTreeAccounts(tree)),
      error: () => this.accounts.set([])
    });
    this.classificationsRepo.getList().subscribe({
      next: list => {
        const map = new Map<string, string>();
        for (const c of list) {
          if (c.id && c.code) map.set(c.id, c.code);
        }
        this.classificationCodeById.set(map);
      },
      error: () => this.classificationCodeById.set(new Map())
    });

    this.form.controls.partyType.valueChanges.subscribe(v => {
      this.formPartyType.set(Number(v) as NotificationPartyType);
      const current = this.form.controls.counterpartAccountId.value;
      if (current && !this.filteredAccounts().some(a => a.id === current)) {
        this.form.controls.counterpartAccountId.setValue('');
      }
    });

    this.load();
  }

  @HostListener('document:keydown', ['$event'])
  onKey(e: KeyboardEvent): void {
    if (e.key === 'Escape' && this.showModal() && !this.saving()) {
      this.closeModal();
      return;
    }
    if (!e.ctrlKey && !e.metaKey) {
      if (e.key === 'F5') {
        e.preventDefault();
        this.load();
      }
      if (e.key === 'Delete' && !this.showModal() && this.selected() && this.canDelete()) {
        this.remove();
      }
      return;
    }
    const key = e.key.toLowerCase();
    if (key === 'n' && this.canCreate()) {
      e.preventDefault();
      this.openCreate();
    }
    if (key === 's' && this.showModal()) {
      e.preventDefault();
      this.closeAfterSave.set(e.shiftKey);
      this.save();
    }
    if (key === 'f') {
      e.preventDefault();
      document.querySelector<HTMLInputElement>('.search-box input')?.focus();
    }
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    const active = this.filterActive();
    this.repo
      .getList({
        search: this.search().trim() || undefined,
        noteType: this.filterNoteType() ? Number(this.filterNoteType()) : null,
        partyType: this.filterPartyType() ? Number(this.filterPartyType()) : null,
        isActive: active === '' ? null : active === 'true'
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
          this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.reason.loadError'));
          this.loading.set(false);
        }
      });
  }

  resetFilters(): void {
    this.search.set('');
    this.filterNoteType.set('');
    this.filterPartyType.set('');
    this.filterActive.set('');
    this.load();
  }

  select(row: NotificationReason): void {
    this.selectedId.set(row.id);
  }

  noteLabel(value: number | string): string {
    const item = this.noteTypes.find(n => n.value === Number(value));
    return item ? this.t(item.labelKey) : String(value);
  }

  partyLabel(value: number | string): string {
    const item = this.partyTypes.find(p => p.value === Number(value));
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
    this.closeAfterSave.set(false);
    this.form.reset({
      code: '',
      nameAr: '',
      nameEn: '',
      noteType: 2,
      partyType: 1,
      counterpartAccountId: '',
      usesTax: false,
      isActive: true
    });
    this.formPartyType.set(1);
    this.showModal.set(true);
    setTimeout(() => document.querySelector<HTMLInputElement>('input[formcontrolname="code"]')?.focus(), 0);
  }

  openEdit(row?: NotificationReason): void {
    const target = row ?? this.selected();
    if (!target || !this.canEdit()) return;
    this.editingId.set(target.id);
    this.error.set(null);
    this.closeAfterSave.set(false);
    this.form.reset({
      code: target.code,
      nameAr: target.nameAr,
      nameEn: target.nameEn ?? '',
      noteType: Number(target.noteType),
      partyType: Number(target.partyType),
      counterpartAccountId: target.counterpartAccountId,
      usesTax: target.usesTax,
      isActive: target.isActive
    });
    this.formPartyType.set(Number(target.partyType) as NotificationPartyType);
    this.showModal.set(true);
  }

  closeModal(): void {
    if (this.saving()) return;
    if (this.form.dirty && !confirm(this.t('fin.reason.confirmCancel'))) return;
    this.showModal.set(false);
  }

  buildPayload(): UpsertNotificationReasonPayload {
    const v = this.form.getRawValue();
    return {
      code: v.code.trim(),
      nameAr: v.nameAr.trim(),
      nameEn: v.nameEn.trim() || null,
      noteType: Number(v.noteType),
      partyType: Number(v.partyType),
      counterpartAccountId: v.counterpartAccountId,
      usesTax: v.usesTax,
      isActive: v.isActive
    };
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.error.set(this.t('fin.reason.validationRequired'));
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
        this.load();
        this.selectedId.set(saved.id);
        if (this.closeAfterSave() || !id) {
          this.showModal.set(false);
        } else {
          this.openEdit(saved);
          this.form.markAsPristine();
        }
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.reason.saveError'));
        this.saving.set(false);
      }
    });
  }

  saveAndClose(): void {
    this.closeAfterSave.set(true);
    this.save();
  }

  remove(): void {
    const row = this.selected();
    if (!row || !this.canDelete()) return;
    if (row.hasBeenUsed) {
      this.error.set(this.t('fin.reason.inUse'));
      return;
    }
    if (!confirm(this.t('fin.reason.confirmDelete'))) return;
    this.repo.delete(row.id).subscribe({
      next: () => {
        this.selectedId.set(null);
        this.load();
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.reason.deleteError'));
      }
    });
  }
}
