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
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { BranchRepository } from '../../../core/repositories/branch.repository';
import { Branch, OrgCompanyLookup, UpsertBranchPayload } from '../../../core/models/branch.models';

const FILTERS_KEY = 'gastro.branches.filters';

@Component({
  selector: 'app-branches-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatIconModule, MatTooltipModule],
  templateUrl: './branches.page.html',
  styleUrl: './branches.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BranchesPage implements OnInit {
  @ViewChild('nameInput') nameInput?: ElementRef<HTMLInputElement>;

  private repo = inject(BranchRepository);
  private fb = inject(FormBuilder);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  success = signal<string | null>(null);
  rows = signal<Branch[]>([]);
  companies = signal<OrgCompanyLookup[]>([]);

  selectedId = signal<string | null>(null);
  search = signal('');
  filterCompanyId = signal('');
  filterActiveValue = signal<'' | 'true' | 'false'>('');
  showModal = signal(false);
  editingId = signal<string | null>(null);
  pageIndex = signal(0);
  private formDirtySnapshot = '';

  form = this.fb.nonNullable.group({
    nameAr: ['', [Validators.required, Validators.maxLength(150)]],
    location: ['', [Validators.maxLength(250)]],
    companyId: ['', Validators.required],
    isActive: [true]
  });

  canView = computed(
    () =>
      this.auth.hasPermission('Settings.Branches.View') ||
      this.auth.hasPermission('Branch.View') ||
      this.auth.hasPermission('VIEW_FINANCE')
  );
  canCreate = computed(
    () =>
      this.auth.hasPermission('Settings.Branches.Create') ||
      this.auth.hasPermission('Branch.Create')
  );
  canEdit = computed(
    () =>
      this.auth.hasPermission('Settings.Branches.Edit') ||
      this.auth.hasPermission('Branch.Update')
  );
  canDelete = computed(
    () =>
      this.auth.hasPermission('Settings.Branches.Delete') ||
      this.auth.hasPermission('Branch.Delete')
  );
  canExport = computed(
    () =>
      this.auth.hasPermission('Settings.Branches.Export') ||
      this.auth.hasPermission('Branch.Export') ||
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

  t = (key: string) => this.lang.t(key);

  ngOnInit(): void {
    this.restoreFilters();
    this.repo.getCompanies().subscribe({
      next: rows => this.companies.set(rows.filter(c => c.isActive !== false)),
      error: () => this.companies.set([])
    });
    this.load();
  }

  @HostListener('document:keydown', ['$event'])
  onKey(e: KeyboardEvent): void {
    if (e.key === 'Escape' && this.showModal() && !this.saving()) {
      e.preventDefault();
      this.closeModal();
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
        companyId?: string;
        isActive?: '' | 'true' | 'false';
      };
      this.search.set(parsed.search ?? '');
      this.filterCompanyId.set(parsed.companyId ?? '');
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
        companyId: this.filterCompanyId(),
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
        companyId: this.filterCompanyId() || null,
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
          this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.branch.loadError'));
          this.loading.set(false);
        }
      });
  }

  resetFilters(): void {
    this.search.set('');
    this.filterCompanyId.set('');
    this.filterActiveValue.set('');
    this.load();
  }

  select(row: Branch): void {
    this.selectedId.set(row.id);
  }

  openCreate(): void {
    if (!this.canCreate()) return;
    this.editingId.set(null);
    this.error.set(null);
    this.success.set(null);
    this.form.reset({
      nameAr: '',
      location: '',
      companyId: this.filterCompanyId() || '',
      isActive: true
    });
    this.form.controls.companyId.enable();
    this.formDirtySnapshot = JSON.stringify(this.form.getRawValue());
    this.showModal.set(true);
    queueMicrotask(() => this.nameInput?.nativeElement.focus());
  }

  openEdit(): void {
    const row = this.selected();
    if (!row || !this.canEdit()) return;
    this.editingId.set(row.id);
    this.error.set(null);
    this.success.set(null);
    this.form.reset({
      nameAr: row.nameAr,
      location: row.cityAr ?? '',
      companyId: row.companyId,
      isActive: row.isActive
    });
    this.form.controls.companyId.disable();
    this.formDirtySnapshot = JSON.stringify(this.form.getRawValue());
    this.showModal.set(true);
    queueMicrotask(() => this.nameInput?.nativeElement.focus());
  }

  closeModal(): void {
    if (this.saving()) return;
    const dirty = JSON.stringify(this.form.getRawValue()) !== this.formDirtySnapshot;
    if (dirty && !confirm(this.t('fin.branch.confirmDiscard'))) return;
    this.showModal.set(false);
    this.editingId.set(null);
    this.error.set(null);
  }

  save(closeAfter: boolean): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      if (this.form.controls.nameAr.invalid) {
        this.error.set(this.t('fin.branch.err.nameRequired'));
      } else if (this.form.controls.companyId.invalid) {
        this.error.set(this.t('fin.branch.err.companyRequired'));
      }
      return;
    }

    const raw = this.form.getRawValue();
    const payload: UpsertBranchPayload = {
      companyId: raw.companyId,
      nameAr: raw.nameAr.trim(),
      location: raw.location.trim() || null,
      isActive: raw.isActive
    };

    this.saving.set(true);
    this.error.set(null);
    const req = this.editingId()
      ? this.repo.update(this.editingId()!, payload)
      : this.repo.create(payload);

    req.subscribe({
      next: saved => {
        this.saving.set(false);
        this.success.set(this.t('fin.branch.saveSuccess'));
        this.formDirtySnapshot = JSON.stringify(this.form.getRawValue());
        if (closeAfter) {
          this.showModal.set(false);
          this.editingId.set(null);
        } else if (!this.editingId()) {
          this.editingId.set(saved.id);
          this.selectedId.set(saved.id);
          this.form.controls.companyId.disable();
        }
        this.load();
      },
      error: err => {
        const msg =
          err?.error?.detail ||
          err?.error?.error ||
          (err?.error?.code === 'Organization.BranchNameDuplicate'
            ? this.t('fin.branch.err.duplicate')
            : this.t('fin.branch.saveError'));
        this.error.set(msg);
        this.saving.set(false);
      }
    });
  }

  remove(): void {
    const row = this.selected();
    if (!row || !this.canDelete()) return;
    if (!confirm(this.t('fin.branch.confirmDelete'))) return;
    this.repo.delete(row.id).subscribe({
      next: () => {
        this.selectedId.set(null);
        this.load();
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.branch.deleteError'));
      }
    });
  }

  exportCsv(): void {
    if (!this.canExport()) return;
    const headers = [
      this.t('fin.branch.col.code'),
      this.t('fin.branch.col.name'),
      this.t('fin.branch.col.location'),
      this.t('fin.branch.col.active'),
      this.t('fin.branch.col.company'),
      this.t('fin.branch.col.created'),
      this.t('fin.branch.col.updated')
    ];
    const lines = this.rows().map(r =>
      [
        r.code ?? '',
        r.nameAr,
        r.cityAr ?? '',
        r.isActive ? '1' : '0',
        r.companyNameAr ?? '',
        r.createdAt ?? '',
        r.updatedAt ?? ''
      ]
        .map(v => `"${String(v).replace(/"/g, '""')}"`)
        .join(',')
    );
    const blob = new Blob([[headers.join(','), ...lines].join('\n')], {
      type: 'text/csv;charset=utf-8;'
    });
    const a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = 'branches.csv';
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

  prevPage(): void {
    this.pageIndex.update(i => Math.max(0, i - 1));
  }

  nextPage(): void {
    this.pageIndex.update(i => Math.min(this.totalPages() - 1, i + 1));
  }

  onEnterField(e: Event, next?: HTMLElement | null): void {
    e.preventDefault();
    next?.focus();
  }
}
