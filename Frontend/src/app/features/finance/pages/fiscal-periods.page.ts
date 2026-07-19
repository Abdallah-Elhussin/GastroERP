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
import { FiscalPeriodRepository } from '../../../core/repositories/fiscal-period.repository';
import {
  FISCAL_STATUS,
  FiscalPeriod,
  FiscalPeriodDetail
} from '../../../core/models/fiscal-period.models';

@Component({
  selector: 'app-fiscal-periods-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatIconModule, MatTooltipModule],
  templateUrl: './fiscal-periods.page.html',
  styleUrl: './fiscal-periods.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FiscalPeriodsPage implements OnInit {
  private repo = inject(FiscalPeriodRepository);
  private fb = inject(FormBuilder);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  generating = signal(false);
  error = signal<string | null>(null);
  rows = signal<FiscalPeriod[]>([]);
  selectedId = signal<string | null>(null);
  search = signal('');
  showModal = signal(false);
  editingId = signal<string | null>(null);

  months = Array.from({ length: 12 }, (_, i) => i + 1);
  statusOptions = [
    { value: FISCAL_STATUS.Open, labelKey: 'fin.period.status.open' },
    { value: FISCAL_STATUS.Closed, labelKey: 'fin.period.status.closed' },
    { value: FISCAL_STATUS.Locked, labelKey: 'fin.period.status.locked' }
  ];

  form = this.fb.nonNullable.group({
    fiscalYear: [new Date().getFullYear(), [Validators.required, Validators.min(2000), Validators.max(2100)]],
    startMonth: [1 as number, [Validators.required, Validators.min(1), Validators.max(12)]],
    startDate: [{ value: '', disabled: true }],
    endDate: [{ value: '', disabled: true }],
    notes: [''],
    details: this.fb.array([])
  });

  canView = computed(
    () =>
      this.auth.hasPermission('FiscalPeriod.View') ||
      this.auth.hasPermission('Accounting.View') ||
      this.auth.hasPermission('VIEW_FINANCE')
  );
  canCreate = computed(
    () =>
      this.auth.hasPermission('FiscalPeriod.Create') ||
      this.auth.hasPermission('Accounting.Create')
  );
  canEdit = computed(
    () =>
      this.auth.hasPermission('FiscalPeriod.Edit') ||
      this.auth.hasPermission('FiscalPeriod.Create') ||
      this.auth.hasPermission('Accounting.Update')
  );
  canDelete = computed(
    () =>
      this.auth.hasPermission('FiscalPeriod.Delete') ||
      this.auth.hasPermission('Accounting.Delete')
  );

  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);
  isEditing = computed(() => !!this.editingId());
  details = computed(() => this.form.controls.details as FormArray);

  t = (key: string) => this.lang.t(key);

  ngOnInit(): void {
    this.form.controls.fiscalYear.valueChanges.subscribe(() => this.refreshRangePreview());
    this.form.controls.startMonth.valueChanges.subscribe(() => this.refreshRangePreview());
    this.load();
  }

  get detailsFA(): FormArray {
    return this.form.controls.details as FormArray;
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
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.period.loadError'));
        this.loading.set(false);
      }
    });
  }

  select(row: FiscalPeriod): void {
    this.selectedId.set(row.id);
  }

  openCreate(): void {
    if (!this.canCreate()) return;
    this.editingId.set(null);
    this.error.set(null);
    this.clearDetails();
    const year = new Date().getFullYear();
    this.form.reset({
      fiscalYear: year,
      startMonth: 1,
      startDate: '',
      endDate: '',
      notes: ''
    });
    this.form.controls.fiscalYear.enable();
    this.refreshRangePreview();
    this.showModal.set(true);
  }

  openEdit(): void {
    const row = this.selected();
    if (!row || !this.canEdit()) return;
    this.editingId.set(row.id);
    this.error.set(null);
    this.form.controls.fiscalYear.disable();
    this.form.patchValue({
      fiscalYear: row.fiscalYear,
      startMonth: row.startMonth || 1,
      startDate: row.startDate,
      endDate: row.endDate,
      notes: row.notes ?? ''
    });
    this.setDetails(row.details ?? []);
    this.showModal.set(true);
  }

  closeModal(): void {
    if (this.saving() || this.generating()) return;
    this.showModal.set(false);
    this.editingId.set(null);
    this.error.set(null);
  }

  generateDetails(): void {
    this.refreshRangePreview();
    if (this.editingId()) {
      this.generating.set(true);
      this.repo.generateDetails(this.editingId()!).subscribe({
        next: period => {
          this.setDetails(period.details ?? []);
          this.form.patchValue({
            startDate: period.startDate,
            endDate: period.endDate,
            startMonth: period.startMonth
          });
          this.generating.set(false);
        },
        error: err => {
          this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.period.generateError'));
          this.generating.set(false);
        }
      });
      return;
    }

    const year = Number(this.form.controls.fiscalYear.value);
    const startMonth = Number(this.form.controls.startMonth.value);
    const details = this.buildLocalMonthlyDetails(year, startMonth);
    this.setDetails(details);
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const details = (raw.details as Array<{
      id: string | null;
      periodNumber: number;
      nameAr: string;
      nameEn: string;
      startDate: string;
      endDate: string;
      status: number;
    }>).map(d => ({
      id: d.id,
      periodNumber: d.periodNumber,
      nameAr: d.nameAr.trim(),
      nameEn: d.nameEn.trim(),
      startDate: d.startDate,
      endDate: d.endDate,
      status: Number(d.status)
    }));

    this.saving.set(true);
    this.error.set(null);

    const req = this.editingId()
      ? this.repo.update(this.editingId()!, {
          startMonth: Number(raw.startMonth),
          notes: raw.notes.trim() || null,
          details
        })
      : this.repo.create({
          fiscalYear: Number(raw.fiscalYear),
          startMonth: Number(raw.startMonth),
          notes: raw.notes.trim() || null,
          periodPolicy: 1,
          generateDetails: details.length === 0
        });

    req.subscribe({
      next: saved => {
        if (!this.editingId() && details.length > 0) {
          this.repo
            .update(saved.id, {
              startMonth: Number(raw.startMonth),
              notes: raw.notes.trim() || null,
              details
            })
            .subscribe({
              next: () => {
                this.saving.set(false);
                this.closeModal();
                this.load();
              },
              error: err => {
                this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.period.saveError'));
                this.saving.set(false);
                this.load();
              }
            });
          return;
        }
        this.saving.set(false);
        this.closeModal();
        this.load();
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.period.saveError'));
        this.saving.set(false);
      }
    });
  }

  remove(): void {
    const row = this.selected();
    if (!row || !this.canDelete()) return;
    if (!confirm(this.t('fin.period.confirmDelete'))) return;
    this.repo.delete(row.id).subscribe({
      next: () => {
        this.selectedId.set(null);
        this.load();
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.period.deleteError'));
      }
    });
  }

  statusLabel(status: number): string {
    if (status === FISCAL_STATUS.Closed) return this.t('fin.period.status.closed');
    if (status === FISCAL_STATUS.Locked) return this.t('fin.period.status.locked');
    return this.t('fin.period.status.open');
  }

  statusClass(status: number): string {
    if (status === FISCAL_STATUS.Closed) return 'closed';
    if (status === FISCAL_STATUS.Locked) return 'locked';
    return 'open';
  }

  private refreshRangePreview(): void {
    const year = Number(this.form.controls.fiscalYear.value);
    const startMonth = Number(this.form.controls.startMonth.value);
    if (!year || !startMonth) return;
    const { start, end } = this.calculateRange(year, startMonth);
    this.form.patchValue({ startDate: start, endDate: end }, { emitEvent: false });
  }

  private calculateRange(year: number, startMonth: number): { start: string; end: string } {
    const start = new Date(Date.UTC(year, startMonth - 1, 1));
    const endMonth = startMonth === 1 ? 12 : startMonth - 1;
    const endYear = startMonth === 1 ? year : year + 1;
    const endDay = new Date(Date.UTC(endYear, endMonth, 0)).getUTCDate();
    const end = new Date(Date.UTC(endYear, endMonth - 1, endDay));
    return { start: this.toIsoDate(start), end: this.toIsoDate(end) };
  }

  private buildLocalMonthlyDetails(year: number, startMonth: number): FiscalPeriodDetail[] {
    const { start, end } = this.calculateRange(year, startMonth);
    const details: FiscalPeriodDetail[] = [];
    let cursor = new Date(start + 'T00:00:00Z');
    const endDate = new Date(end + 'T00:00:00Z');
    for (let i = 1; i <= 12; i++) {
      const monthEnd = new Date(Date.UTC(cursor.getUTCFullYear(), cursor.getUTCMonth() + 1, 0));
      if (monthEnd > endDate) monthEnd.setTime(endDate.getTime());
      details.push({
        id: '',
        periodNumber: i,
        nameAr: `الفترة المالية ${i}`,
        nameEn: `Financial Period ${i}`,
        startDate: this.toIsoDate(cursor),
        endDate: this.toIsoDate(monthEnd),
        status: FISCAL_STATUS.Open
      });
      cursor = new Date(Date.UTC(monthEnd.getUTCFullYear(), monthEnd.getUTCMonth(), monthEnd.getUTCDate() + 1));
      if (cursor > endDate) break;
    }
    return details;
  }

  private setDetails(details: FiscalPeriodDetail[]): void {
    this.clearDetails();
    for (const d of details) {
      this.detailsFA.push(
        this.fb.nonNullable.group({
          id: [d.id || null as string | null],
          periodNumber: [d.periodNumber],
          nameAr: [d.nameAr, Validators.required],
          nameEn: [d.nameEn, Validators.required],
          startDate: [d.startDate],
          endDate: [d.endDate],
          status: [Number(d.status)]
        })
      );
    }
  }

  private clearDetails(): void {
    while (this.detailsFA.length) this.detailsFA.removeAt(0);
  }

  private toIsoDate(d: Date): string {
    const y = d.getUTCFullYear();
    const m = String(d.getUTCMonth() + 1).padStart(2, '0');
    const day = String(d.getUTCDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  formatDisplayDate(value?: string | null): string {
    if (!value) return '—';
    const d = new Date(value + (value.includes('T') ? '' : 'T00:00:00'));
    if (Number.isNaN(d.getTime())) return value;
    return d.toLocaleDateString(this.lang.language() === 'ar' ? 'ar' : 'en', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }
}
