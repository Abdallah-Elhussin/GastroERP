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
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { CurrencyRepository } from '../../../core/repositories/currency.repository';
import { CURRENCY_DECIMAL_OPTIONS, Currency } from '../../../core/models/currency.models';

@Component({
  selector: 'app-currencies-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatIconModule, MatTooltipModule],
  templateUrl: './currencies.page.html',
  styleUrl: './currencies.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CurrenciesPage implements OnInit {
  private repo = inject(CurrencyRepository);
  private fb = inject(FormBuilder);
  private router = inject(Router);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  rows = signal<Currency[]>([]);
  selectedId = signal<string | null>(null);
  search = signal('');
  showModal = signal(false);
  editingId = signal<string | null>(null);

  decimalOptions = CURRENCY_DECIMAL_OPTIONS;

  form = this.fb.nonNullable.group({
    code: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(3)]],
    nameAr: ['', [Validators.required, Validators.maxLength(200)]],
    nameEn: ['', [Validators.required, Validators.maxLength(200)]],
    symbol: [''],
    decimalPlaces: [2 as number, Validators.required],
    subUnitNameAr: [''],
    subUnitNameEn: [''],
    currentExchangeRate: [1 as number, [Validators.required, Validators.min(0.000001)]],
    isCompanyCurrency: [false],
    isActive: [true],
    sortOrder: [0 as number]
  });

  canView = computed(
    () =>
      this.auth.hasPermission('Currency.View') ||
      this.auth.hasPermission('Accounting.View') ||
      this.auth.hasPermission('VIEW_FINANCE')
  );
  canCreate = computed(
    () => this.auth.hasPermission('Currency.Create') || this.auth.hasPermission('Accounting.Create')
  );
  canEdit = computed(
    () => this.auth.hasPermission('Currency.Update') || this.auth.hasPermission('Accounting.Update')
  );
  canDelete = computed(
    () => this.auth.hasPermission('Currency.Delete') || this.auth.hasPermission('Accounting.Delete')
  );
  canExport = computed(
    () => this.auth.hasPermission('Currency.Export') || this.auth.hasPermission('Currency.View')
  );
  canSetCompany = computed(
    () => this.auth.hasPermission('Currency.SetCompany') || this.auth.hasPermission('Accounting.Update')
  );

  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);
  isEditing = computed(() => !!this.editingId());

  t = (key: string) => this.lang.t(key);

  ngOnInit(): void {
    this.load();
    this.form.controls.isCompanyCurrency.valueChanges.subscribe(isCompany => {
      if (isCompany) {
        this.form.controls.currentExchangeRate.setValue(1, { emitEvent: false });
        this.form.controls.currentExchangeRate.disable({ emitEvent: false });
      } else if (!this.isEditing() || !this.selected()?.isCompanyCurrency) {
        this.form.controls.currentExchangeRate.enable({ emitEvent: false });
      }
    });
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
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.cur.loadError'));
        this.loading.set(false);
      }
    });
  }

  select(row: Currency): void {
    this.selectedId.set(row.id);
  }

  openCreate(): void {
    this.editingId.set(null);
    this.error.set(null);
    this.form.reset({
      code: '',
      nameAr: '',
      nameEn: '',
      symbol: '',
      decimalPlaces: 2,
      subUnitNameAr: '',
      subUnitNameEn: '',
      currentExchangeRate: 1,
      isCompanyCurrency: false,
      isActive: true,
      sortOrder: 0
    });
    this.form.controls.code.enable();
    this.form.controls.isCompanyCurrency.enable();
    this.form.controls.currentExchangeRate.enable();
    this.showModal.set(true);
  }

  openEdit(): void {
    const row = this.selected();
    if (!row || !this.canEdit()) return;
    this.editingId.set(row.id);
    this.error.set(null);
    this.form.reset({
      code: row.code,
      nameAr: row.nameAr,
      nameEn: row.nameEn,
      symbol: row.symbol ?? '',
      decimalPlaces: row.decimalPlaces,
      subUnitNameAr: row.subUnitNameAr ?? '',
      subUnitNameEn: row.subUnitNameEn ?? '',
      currentExchangeRate: row.currentExchangeRate,
      isCompanyCurrency: row.isCompanyCurrency,
      isActive: row.isActive,
      sortOrder: row.sortOrder
    });
    this.form.controls.code.disable();
    this.form.controls.isCompanyCurrency.disable();
    if (row.isCompanyCurrency) {
      this.form.controls.currentExchangeRate.disable();
    } else {
      this.form.controls.currentExchangeRate.enable();
    }
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
    this.editingId.set(null);
    this.error.set(null);
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    const payload = {
      code: raw.code.trim().toUpperCase(),
      nameAr: raw.nameAr.trim(),
      nameEn: raw.nameEn.trim(),
      symbol: raw.symbol.trim() || null,
      decimalPlaces: Number(raw.decimalPlaces),
      subUnitNameAr: raw.subUnitNameAr.trim() || null,
      subUnitNameEn: raw.subUnitNameEn.trim() || null,
      currentExchangeRate: Number(raw.currentExchangeRate),
      isCompanyCurrency: !!raw.isCompanyCurrency,
      isActive: !!raw.isActive,
      sortOrder: Number(raw.sortOrder) || 0
    };

    this.saving.set(true);
    this.error.set(null);
    const req = this.editingId()
      ? this.repo.update(this.editingId()!, payload)
      : this.repo.create(payload);

    req.subscribe({
      next: () => {
        this.saving.set(false);
        this.closeModal();
        this.load();
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.cur.saveError'));
        this.saving.set(false);
      }
    });
  }

  remove(): void {
    const row = this.selected();
    if (!row || !this.canDelete() || row.isSystem || row.isCompanyCurrency) return;
    if (!confirm(this.t('fin.cur.confirmDelete'))) return;
    this.repo.delete(row.id).subscribe({
      next: () => {
        this.selectedId.set(null);
        this.load();
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.cur.deleteError'));
      }
    });
  }

  setAsCompany(): void {
    const row = this.selected();
    if (!row || !this.canSetCompany() || row.isCompanyCurrency) return;
    if (!confirm(this.t('fin.cur.confirmSetCompany'))) return;
    this.repo.setCompany(row.id).subscribe({
      next: () => this.load(),
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.cur.saveError'));
      }
    });
  }

  openRates(row?: Currency | null): void {
    const id = row?.id ?? this.selectedId();
    if (!id) {
      void this.router.navigate(['/finance/exchange-rates']);
      return;
    }
    void this.router.navigate(['/finance/exchange-rates'], { queryParams: { currencyId: id } });
  }

  exportExcel(): void {
    this.repo.exportCsv(this.search().trim() || undefined).subscribe({
      next: blob => {
        const a = document.createElement('a');
        a.href = URL.createObjectURL(blob);
        a.download = 'currencies.csv';
        a.click();
        URL.revokeObjectURL(a.href);
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.cur.loadError'));
      }
    });
  }

  yesNo(value: boolean): string {
    return value ? this.t('common.yes') : this.t('common.no');
  }
}
