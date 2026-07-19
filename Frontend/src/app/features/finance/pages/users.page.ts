import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  HostListener,
  OnInit,
  ViewChild,
  computed,
  inject,
  signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { SystemUserRepository } from '../../../core/repositories/system-user.repository';
import {
  BranchLookup,
  RoleLookup,
  SystemUser,
  UpsertSystemUserPayload,
  UserLicenseStatus
} from '../../../core/models/system-user.models';

const FILTERS_KEY = 'gastro.users.filters';
type FormTab = 'basic' | 'security';

function matchPasswords(group: AbstractControl): ValidationErrors | null {
  const password = group.get('password')?.value as string;
  const confirm = group.get('confirmPassword')?.value as string;
  if (!password && !confirm) return null;
  return password === confirm ? null : { passwordMismatch: true };
}

@Component({
  selector: 'app-users-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink, MatIconModule, MatTooltipModule],
  templateUrl: './users.page.html',
  styleUrl: './users.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UsersPage implements OnInit {
  @ViewChild('userNameInput') userNameInput?: ElementRef<HTMLInputElement>;

  private repo = inject(SystemUserRepository);
  private fb = inject(FormBuilder);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  success = signal<string | null>(null);
  rows = signal<SystemUser[]>([]);
  roles = signal<RoleLookup[]>([]);
  branches = signal<BranchLookup[]>([]);
  license = signal<UserLicenseStatus | null>(null);
  roleSearch = signal('');

  selectedId = signal<string | null>(null);
  search = signal('');
  filterBranchId = signal('');
  filterRoleId = signal('');
  filterActiveValue = signal<'' | 'true' | 'false'>('');
  showModal = signal(false);
  showResetModal = signal(false);
  editingId = signal<string | null>(null);
  formTab = signal<FormTab>('basic');
  pageIndex = signal(0);
  private formDirtySnapshot = '';

  form = this.fb.nonNullable.group(
    {
      userName: ['', [Validators.required, Validators.maxLength(100)]],
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.maxLength(100)]],
      email: ['', [Validators.email, Validators.maxLength(256)]],
      mobileNumber: ['', [Validators.maxLength(20)]],
      phoneNumber: ['', [Validators.maxLength(20)]],
      branchId: ['', Validators.required],
      roleId: ['', Validators.required],
      password: [''],
      confirmPassword: [''],
      isActive: [true],
      isPosUser: [false],
      mustChangePassword: [true],
      isLocked: [false]
    },
    { validators: matchPasswords }
  );

  resetForm = this.fb.nonNullable.group(
    {
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', Validators.required]
    },
    { validators: matchPasswords }
  );

  canView = computed(
    () =>
      this.auth.hasPermission('Settings.Users.View') ||
      this.auth.hasPermission('Identity.Users.View')
  );
  canCreate = computed(
    () =>
      this.auth.hasPermission('Settings.Users.Create') ||
      this.auth.hasPermission('Identity.Users.Create')
  );
  canEdit = computed(
    () =>
      this.auth.hasPermission('Settings.Users.Edit') ||
      this.auth.hasPermission('Identity.Users.Edit')
  );
  canDelete = computed(
    () =>
      this.auth.hasPermission('Settings.Users.Delete') ||
      this.auth.hasPermission('Identity.Users.Delete')
  );
  canLock = computed(
    () =>
      this.auth.hasPermission('Settings.Users.LockUnlock') ||
      this.auth.hasPermission('Identity.Users.LockUnlock') ||
      this.canEdit()
  );
  canReset = computed(
    () =>
      this.auth.hasPermission('Settings.Users.ResetPassword') ||
      this.auth.hasPermission('Identity.Users.ResetPassword') ||
      this.canEdit()
  );
  canExport = computed(
    () =>
      this.auth.hasPermission('Settings.Users.Export') ||
      this.auth.hasPermission('Identity.Users.Export') ||
      this.canView()
  );

  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);
  isEditing = computed(() => !!this.editingId());
  pageSize = 20;
  pagedRows = computed(() => {
    const start = this.pageIndex() * this.pageSize;
    return this.rows().slice(start, start + this.pageSize);
  });
  totalPages = computed(() => Math.max(1, Math.ceil(this.rows().length / this.pageSize)));
  filteredRoles = computed(() => {
    const q = this.roleSearch().trim().toLowerCase();
    const all = this.roles().filter(r => r.isActive !== false);
    if (!q) return all;
    return all.filter(
      r =>
        r.name.toLowerCase().includes(q) ||
        (r.nameAr ?? '').toLowerCase().includes(q)
    );
  });

  t = (key: string) => this.lang.t(key);

  ngOnInit(): void {
    this.restoreFilters();
    this.repo.getRoles().subscribe({
      next: rows => this.roles.set(rows),
      error: () => this.roles.set([])
    });
    this.repo.getBranches().subscribe({
      next: rows => this.branches.set(rows),
      error: () => this.branches.set([])
    });
    this.load();
  }

  @HostListener('document:keydown', ['$event'])
  onKey(e: KeyboardEvent): void {
    if (e.key === 'Escape' && !this.saving()) {
      e.preventDefault();
      if (this.showResetModal()) this.showResetModal.set(false);
      else if (this.showModal()) this.closeModal();
      return;
    }
    if (e.ctrlKey && e.key.toLowerCase() === 'n') {
      e.preventDefault();
      this.openCreate();
    }
    if (e.ctrlKey && e.shiftKey && e.key.toLowerCase() === 's' && this.showModal()) {
      e.preventDefault();
      this.save(true);
    } else if (e.ctrlKey && e.key.toLowerCase() === 's' && this.showModal()) {
      e.preventDefault();
      this.save(false);
    }
    if (e.ctrlKey && e.key.toLowerCase() === 'f') {
      e.preventDefault();
      document.querySelector<HTMLInputElement>('.search-box input')?.focus();
    }
    if (e.ctrlKey && e.key.toLowerCase() === 'l' && this.selected()) {
      e.preventDefault();
      this.toggleLock();
    }
    if (e.ctrlKey && e.key.toLowerCase() === 'r' && this.selected()) {
      e.preventDefault();
      this.openResetPassword();
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
      const parsed = JSON.parse(raw) as {
        search?: string;
        branchId?: string;
        roleId?: string;
        isActive?: '' | 'true' | 'false';
      };
      this.search.set(parsed.search ?? '');
      this.filterBranchId.set(parsed.branchId ?? '');
      this.filterRoleId.set(parsed.roleId ?? '');
      this.filterActiveValue.set(parsed.isActive ?? '');
    } catch {
      /* ignore */
    }
  }

  private persistFilters(): void {
    localStorage.setItem(
      FILTERS_KEY,
      JSON.stringify({
        search: this.search(),
        branchId: this.filterBranchId(),
        roleId: this.filterRoleId(),
        isActive: this.filterActiveValue()
      })
    );
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.persistFilters();
    const active = this.filterActiveValue();
    this.repo
      .getList({
        search: this.search().trim() || undefined,
        branchId: this.filterBranchId() || null,
        roleId: this.filterRoleId() || null,
        isActive: active === '' ? null : active === 'true'
      })
      .subscribe({
        next: rows => {
          this.rows.set(rows);
          this.pageIndex.set(0);
          this.loading.set(false);
          if (this.selectedId() && !rows.some(r => r.id === this.selectedId())) {
            this.selectedId.set(null);
          }
        },
        error: err => {
          this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.user.loadError'));
          this.loading.set(false);
        }
      });

    this.repo.getLicenseStatus().subscribe({
      next: status => this.license.set(status),
      error: () => this.license.set(null)
    });
  }

  resetFilters(): void {
    this.search.set('');
    this.filterBranchId.set('');
    this.filterRoleId.set('');
    this.filterActiveValue.set('');
    this.load();
  }

  select(row: SystemUser): void {
    this.selectedId.set(row.id);
  }

  openCreate(): void {
    if (!this.canCreate()) return;
    this.editingId.set(null);
    this.formTab.set('basic');
    this.error.set(null);
    this.success.set(null);
    this.form.reset({
      userName: '',
      firstName: '',
      lastName: '',
      email: '',
      mobileNumber: '',
      phoneNumber: '',
      branchId: this.filterBranchId() || '',
      roleId: this.filterRoleId() || '',
      password: '',
      confirmPassword: '',
      isActive: true,
      isPosUser: false,
      mustChangePassword: true,
      isLocked: false
    });
    this.form.controls.password.setValidators([Validators.required, Validators.minLength(8)]);
    this.form.controls.confirmPassword.setValidators([Validators.required]);
    this.form.controls.password.updateValueAndValidity();
    this.form.controls.confirmPassword.updateValueAndValidity();
    this.formDirtySnapshot = JSON.stringify(this.form.getRawValue());
    this.showModal.set(true);
    queueMicrotask(() => this.userNameInput?.nativeElement.focus());
  }

  openEdit(): void {
    const row = this.selected();
    if (!row || !this.canEdit()) return;
    this.editingId.set(row.id);
    this.formTab.set('basic');
    this.error.set(null);
    this.success.set(null);
    this.form.reset({
      userName: row.userName,
      firstName: row.firstName,
      lastName: row.lastName ?? '',
      email: row.email?.endsWith('@users.local') ? '' : row.email,
      mobileNumber: row.mobileNumber ?? '',
      phoneNumber: row.phoneNumber ?? '',
      branchId: row.branchId ?? '',
      roleId: row.roleId ?? '',
      password: '',
      confirmPassword: '',
      isActive: row.isActive,
      isPosUser: row.isPosUser,
      mustChangePassword: row.mustChangePassword,
      isLocked: row.isLocked
    });
    this.form.controls.password.clearValidators();
    this.form.controls.confirmPassword.clearValidators();
    this.form.controls.password.updateValueAndValidity();
    this.form.controls.confirmPassword.updateValueAndValidity();
    this.formDirtySnapshot = JSON.stringify(this.form.getRawValue());
    this.showModal.set(true);
    queueMicrotask(() => this.userNameInput?.nativeElement.focus());
  }

  closeModal(): void {
    if (this.saving()) return;
    const dirty = JSON.stringify(this.form.getRawValue()) !== this.formDirtySnapshot;
    if (dirty && !confirm(this.t('fin.user.confirmDiscard'))) return;
    this.showModal.set(false);
    this.editingId.set(null);
    this.error.set(null);
  }

  save(closeAfter: boolean): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      if (this.form.controls.userName.invalid) this.error.set(this.t('fin.user.err.userName'));
      else if (this.form.hasError('passwordMismatch')) this.error.set(this.t('fin.user.err.passwordMatch'));
      else if (this.form.controls.password.invalid) this.error.set(this.t('fin.user.err.password'));
      else this.error.set(this.t('fin.user.saveError'));
      if (this.form.controls.password.invalid || this.form.hasError('passwordMismatch') || this.form.controls.roleId.invalid) {
        this.formTab.set('security');
      } else {
        this.formTab.set('basic');
      }
      return;
    }

    const raw = this.form.getRawValue();
    const payload: UpsertSystemUserPayload = {
      userName: raw.userName.trim(),
      firstName: raw.firstName.trim(),
      lastName: raw.lastName.trim() || null,
      email: raw.email.trim() || null,
      mobileNumber: raw.mobileNumber.trim() || null,
      phoneNumber: raw.phoneNumber.trim() || null,
      branchId: raw.branchId,
      roleId: raw.roleId,
      password: raw.password || null,
      isActive: raw.isActive,
      isPosUser: raw.isPosUser,
      mustChangePassword: raw.mustChangePassword,
      isLocked: raw.isLocked,
      preferredLanguage: 'ar'
    };

    this.saving.set(true);
    this.error.set(null);
    const req = this.editingId()
      ? this.repo.update(this.editingId()!, payload)
      : this.repo.create(payload);

    req.subscribe({
      next: result => {
        this.saving.set(false);
        this.success.set(this.t('fin.user.saveSuccess'));
        this.formDirtySnapshot = JSON.stringify(this.form.getRawValue());
        if (closeAfter) {
          this.showModal.set(false);
          this.editingId.set(null);
        } else if (!this.editingId() && typeof result === 'string') {
          this.editingId.set(result);
          this.selectedId.set(result);
        }
        this.load();
      },
      error: err => {
        const code = err?.error?.code as string | undefined;
        let msg = err?.error?.detail || err?.error?.error || this.t('fin.user.saveError');
        if (code === 'Identity.UserNameDuplicate') msg = this.t('fin.user.err.userNameDuplicate');
        if (code === 'Identity.UserEmailDuplicate') msg = this.t('fin.user.err.emailDuplicate');
        this.error.set(msg);
        this.saving.set(false);
      }
    });
  }

  remove(): void {
    const row = this.selected();
    if (!row || !this.canDelete()) return;
    if (!confirm(this.t('fin.user.confirmDelete'))) return;
    this.repo.delete(row.id).subscribe({
      next: () => {
        this.selectedId.set(null);
        this.load();
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.user.deleteError'));
      }
    });
  }

  toggleLock(): void {
    const row = this.selected();
    if (!row || !this.canLock()) return;
    const req = row.isLocked ? this.repo.unlock(row.id) : this.repo.lock(row.id);
    req.subscribe({
      next: () => this.load(),
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.user.lockError'));
      }
    });
  }

  openResetPassword(): void {
    if (!this.selected() || !this.canReset()) return;
    this.resetForm.reset({ password: '', confirmPassword: '' });
    this.showResetModal.set(true);
  }

  submitResetPassword(): void {
    const row = this.selected();
    if (!row || this.resetForm.invalid) {
      this.resetForm.markAllAsTouched();
      return;
    }
    const password = this.resetForm.controls.password.value;
    this.saving.set(true);
    this.repo.resetPassword(row.id, password).subscribe({
      next: () => {
        this.saving.set(false);
        this.showResetModal.set(false);
        this.success.set(this.t('fin.user.resetSuccess'));
      },
      error: err => {
        this.saving.set(false);
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.user.resetError'));
      }
    });
  }

  exportCsv(): void {
    if (!this.canExport()) return;
    const headers = [
      this.t('fin.user.col.number'),
      this.t('fin.user.col.code'),
      this.t('fin.user.col.userName'),
      this.t('fin.user.col.firstName'),
      this.t('fin.user.col.lastName'),
      this.t('fin.user.col.email'),
      this.t('fin.user.col.mobile'),
      this.t('fin.user.col.phone'),
      this.t('fin.user.col.branch'),
      this.t('fin.user.col.role'),
      this.t('fin.user.col.active'),
      this.t('fin.user.col.lastLogin'),
      this.t('fin.user.col.created')
    ];
    const lines = this.rows().map(r =>
      [
        r.number,
        r.code ?? '',
        r.userName,
        r.firstName,
        r.lastName,
        r.email,
        r.mobileNumber ?? '',
        r.phoneNumber ?? '',
        r.branchNameAr ?? '',
        r.roleNameAr || r.roleName || '',
        r.isActive ? '1' : '0',
        r.lastLoginAt ?? '',
        r.createdAt ?? ''
      ]
        .map(v => `"${String(v).replace(/"/g, '""')}"`)
        .join(',')
    );
    const blob = new Blob([[headers.join(','), ...lines].join('\n')], {
      type: 'text/csv;charset=utf-8;'
    });
    const a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = 'users.csv';
    a.click();
    URL.revokeObjectURL(a.href);
  }

  printPage(): void {
    window.print();
  }

  formatDate(value?: string | null): string {
    if (!value) return '—';
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return '—';
    return d.toLocaleString(this.lang.language() === 'ar' ? 'ar' : 'en');
  }

  roleLabel(r: RoleLookup): string {
    return this.lang.language() === 'ar' ? r.nameAr || r.name : r.name;
  }

  prevPage(): void {
    this.pageIndex.update(i => Math.max(0, i - 1));
  }

  nextPage(): void {
    this.pageIndex.update(i => Math.min(this.totalPages() - 1, i + 1));
  }

  licenseLabel(): string {
    const lic = this.license();
    if (!lic) return '';
    if (this.lang.language() === 'ar') {
      return lic.isTrial
        ? `المستخدمون: ${lic.currentUsers} / ${lic.maxUsers} (وضع تجريبي محلي)`
        : `المستخدمون: ${lic.currentUsers} / ${lic.maxUsers}`;
    }
    return lic.label;
  }
}
