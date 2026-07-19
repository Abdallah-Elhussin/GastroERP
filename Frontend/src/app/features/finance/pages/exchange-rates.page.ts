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
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { CurrencyRepository } from '../../../core/repositories/currency.repository';
import { Currency, CurrencyExchangeRate } from '../../../core/models/currency.models';

@Component({
  selector: 'app-exchange-rates-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatIconModule, MatTooltipModule, RouterLink],
  templateUrl: './exchange-rates.page.html',
  styleUrl: './exchange-rates.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExchangeRatesPage implements OnInit {
  private repo = inject(CurrencyRepository);
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  currencies = signal<Currency[]>([]);
  rows = signal<CurrencyExchangeRate[]>([]);
  selectedId = signal<string | null>(null);
  currencyId = signal('');
  asOfDate = signal('');
  activeOnly = signal(false);
  search = signal('');
  showModal = signal(false);
  editingId = signal<string | null>(null);

  form = this.fb.nonNullable.group({
    currencyId: ['', Validators.required],
    rate: [1 as number, [Validators.required, Validators.min(0.000001)]],
    startDate: [new Date().toISOString().slice(0, 10), Validators.required],
    endDate: ['' as string],
    isActive: [true],
    changeReason: [''],
    autoClosePreviousOpen: [true]
  });

  canManage = computed(
    () =>
      this.auth.hasPermission('Currency.ManageRates') ||
      this.auth.hasPermission('Currency.Update') ||
      this.auth.hasPermission('Accounting.Update')
  );
  canExport = computed(
    () => this.auth.hasPermission('Currency.Export') || this.auth.hasPermission('Currency.View')
  );
  canDelete = computed(
    () => this.auth.hasPermission('Currency.Delete') || this.auth.hasPermission('Accounting.Delete')
  );

  foreignCurrencies = computed(() => this.currencies().filter(c => !c.isCompanyCurrency && c.isActive));
  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);
  isEditing = computed(() => !!this.editingId());

  t = (key: string) => this.lang.t(key);

  ngOnInit(): void {
    const q = this.route.snapshot.queryParamMap.get('currencyId');
    if (q) this.currencyId.set(q);

    this.repo.getList().subscribe({
      next: list => {
        this.currencies.set(list);
        this.load();
      },
      error: () => {
        this.currencies.set([]);
        this.load();
      }
    });
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo
      .getExchangeRates({
        currencyId: this.currencyId() || undefined,
        asOfDate: this.asOfDate() || undefined,
        activeOnly: this.activeOnly() || undefined,
        search: this.search().trim() || undefined
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
          this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.rate.loadError'));
          this.loading.set(false);
        }
      });
  }

  select(row: CurrencyExchangeRate): void {
    this.selectedId.set(row.id);
  }

  openCreate(): void {
    if (!this.canManage()) return;
    const id = this.currencyId() || this.foreignCurrencies()[0]?.id || '';
    const cur = this.currencies().find(c => c.id === id);
    this.editingId.set(null);
    this.form.reset({
      currencyId: id,
      rate: cur?.currentExchangeRate ?? 1,
      startDate: new Date().toISOString().slice(0, 10),
      endDate: '',
      isActive: true,
      changeReason: '',
      autoClosePreviousOpen: true
    });
    this.form.controls.currencyId.enable();
    this.error.set(null);
    this.showModal.set(true);
  }

  openEdit(): void {
    const row = this.selected();
    if (!row || !this.canManage()) return;
    this.editingId.set(row.id);
    this.form.reset({
      currencyId: row.currencyId,
      rate: row.rate,
      startDate: row.startDate,
      endDate: row.endDate ?? '',
      isActive: row.isActive,
      changeReason: row.changeReason ?? '',
      autoClosePreviousOpen: true
    });
    this.form.controls.currencyId.disable();
    this.error.set(null);
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
      currencyId: raw.currencyId,
      rate: Number(raw.rate),
      startDate: raw.startDate,
      endDate: raw.endDate?.trim() || null,
      isActive: !!raw.isActive,
      changeReason: raw.changeReason.trim() || null,
      autoClosePreviousOpen: !!raw.autoClosePreviousOpen
    };

    this.saving.set(true);
    const req = this.editingId()
      ? this.repo.updateExchangeRate(this.editingId()!, payload)
      : this.repo.createExchangeRate(payload);

    req.subscribe({
      next: () => {
        this.saving.set(false);
        this.closeModal();
        if (raw.currencyId) this.currencyId.set(raw.currencyId);
        this.load();
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.rate.saveError'));
        this.saving.set(false);
      }
    });
  }

  deactivateSelected(): void {
    const row = this.selected();
    if (!row || !this.canManage()) return;
    this.repo.deactivateExchangeRate(row.id).subscribe({
      next: () => this.load(),
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.rate.saveError'));
      }
    });
  }

  remove(): void {
    const row = this.selected();
    if (!row || !this.canDelete()) return;
    if (!confirm(this.t('fin.rate.confirmDelete'))) return;
    this.repo.deleteExchangeRate(row.id).subscribe({
      next: () => {
        this.selectedId.set(null);
        this.load();
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.rate.deleteError'));
      }
    });
  }

  exportExcel(): void {
    this.repo
      .exportExchangeRatesCsv({
        currencyId: this.currencyId() || undefined,
        activeOnly: this.activeOnly() || undefined,
        search: this.search().trim() || undefined
      })
      .subscribe({
        next: blob => {
          const a = document.createElement('a');
          a.href = URL.createObjectURL(blob);
          a.download = 'exchange-rates.csv';
          a.click();
          URL.revokeObjectURL(a.href);
        },
        error: err => {
          this.error.set(err?.error?.detail || err?.error?.error || this.t('fin.rate.loadError'));
        }
      });
  }
}
