import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnChanges,
  SimpleChanges,
  computed,
  inject,
  input,
  output,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import {
  FormArray,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { Subscription, startWith } from 'rxjs';
import { LanguageService } from '../../../core/services/language.service';
import {
  FinanceVoucherAccountOption,
  FinanceVoucherCostCenterOption,
  FinanceVoucherCurrencyOption,
  FinanceVoucherDetailsTotals,
  FinanceVoucherLineFormValue
} from '../../../core/models/finance-voucher-details.models';

/**
 * Unified voucher details grid for Financial Operations documents.
 * Columns: #, Account, Account Name, Cost Center, Analytical (optional),
 * Currency, Exchange Rate, Amount, Description.
 * No Project column — restaurants use Cost Center for branch/dept analysis.
 */
@Component({
  selector: 'app-voucher-details-grid',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatIconModule],
  templateUrl: './voucher-details-grid.component.html',
  styleUrl: './voucher-details-grid.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class VoucherDetailsGridComponent implements OnChanges {
  private fb = inject(FormBuilder);
  private destroyRef = inject(DestroyRef);
  lang = inject(LanguageService);

  lines = input.required<FormArray>();
  accounts = input<FinanceVoucherAccountOption[]>([]);
  costCenters = input<FinanceVoucherCostCenterOption[]>([]);
  currencies = input<FinanceVoucherCurrencyOption[]>([]);
  analyticalAccounts = input<FinanceVoucherAccountOption[]>([]);
  companyCurrencyCode = input('SAR');
  showAnalyticalAccounts = input(false);
  readonly = input(false);
  minLines = input(1);
  selectedIndex = input(-1);

  lineSelected = output<number>();
  totalsChange = output<FinanceVoucherDetailsTotals>();

  private tick = signal(0);
  private watchSub?: Subscription;

  totals = computed(() => {
    this.tick();
    return this.computeTotals();
  });

  t = (key: string) => this.lang.t(key);

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['lines']) {
      this.bindLinesWatch();
    }
  }

  static createLineGroup(
    fb: FormBuilder,
    line?: Partial<FinanceVoucherLineFormValue>,
    companyCurrency = 'SAR'
  ): FormGroup {
    return fb.nonNullable.group({
      id: [line?.id ?? (null as string | null)],
      chartOfAccountId: [line?.chartOfAccountId ?? '', Validators.required],
      costCenterId: [line?.costCenterId ?? ''],
      analyticalAccountId: [line?.analyticalAccountId ?? ''],
      currency: [line?.currency || companyCurrency, Validators.required],
      exchangeRate: [line?.exchangeRate ?? 1, [Validators.required, Validators.min(0.000001)]],
      amount: [line?.amount ?? 0, [Validators.required, Validators.min(0.01)]],
      description: [line?.description ?? '']
    });
  }

  get controls(): FormGroup[] {
    return this.lines().controls as FormGroup[];
  }

  accountName(i: number): string {
    const id = this.controls[i]?.get('chartOfAccountId')?.value as string;
    if (!id) return '';
    const a = this.accounts().find(x => x.id === id);
    if (!a) return '';
    return this.lang.language() === 'ar' ? a.nameAr : a.nameEn || a.nameAr;
  }

  addLine(): void {
    if (this.readonly()) return;
    this.lines().push(
      VoucherDetailsGridComponent.createLineGroup(
        this.fb,
        { currency: this.companyCurrencyCode(), exchangeRate: 1 },
        this.companyCurrencyCode()
      )
    );
    this.refresh();
  }

  removeSelectedOrLast(): void {
    if (this.readonly()) return;
    const idx = this.selectedIndex() >= 0 ? this.selectedIndex() : this.lines().length - 1;
    this.removeLine(idx);
  }

  removeLine(index: number): void {
    if (this.readonly()) return;
    if (this.lines().length <= this.minLines()) return;
    if (index < 0 || index >= this.lines().length) return;
    this.lines().removeAt(index);
    this.refresh();
  }

  copyLine(index: number): void {
    if (this.readonly() || index < 0) return;
    const src = this.controls[index];
    if (!src) return;
    const v = src.getRawValue() as FinanceVoucherLineFormValue;
    this.lines().insert(
      index + 1,
      VoucherDetailsGridComponent.createLineGroup(
        this.fb,
        { ...v, id: null },
        this.companyCurrencyCode()
      )
    );
    this.refresh();
  }

  selectRow(index: number): void {
    this.lineSelected.emit(index);
  }

  onCurrencyChange(index: number): void {
    const g = this.controls[index];
    if (!g) return;
    const code = (g.get('currency')?.value as string) || this.companyCurrencyCode();
    const cur = this.currencies().find(c => c.code === code);
    const company = this.companyCurrencyCode();
    if (code === company) {
      g.patchValue({ exchangeRate: 1 }, { emitEvent: true });
    } else if (cur?.currentExchangeRate) {
      g.patchValue({ exchangeRate: cur.currentExchangeRate }, { emitEvent: true });
    }
    this.refresh();
  }

  analyticalOptions(): FinanceVoucherAccountOption[] {
    const extra = this.analyticalAccounts();
    return extra.length ? extra : this.accounts();
  }

  private bindLinesWatch(): void {
    this.watchSub?.unsubscribe();
    const fa = this.lines();
    this.watchSub = fa.valueChanges
      .pipe(startWith(fa.getRawValue()), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.refresh());
  }

  private refresh(): void {
    this.tick.update(v => v + 1);
    this.totalsChange.emit(this.computeTotals());
  }

  private computeTotals(): FinanceVoucherDetailsTotals {
    const company = this.companyCurrencyCode();
    let totalAmount = 0;
    let totalLocal = 0;
    let totalForeign = 0;
    let hasForeign = false;

    for (const ctrl of this.controls) {
      const v = ctrl.getRawValue() as FinanceVoucherLineFormValue;
      const amount = Number(v.amount) || 0;
      const rate = Number(v.exchangeRate) || 1;
      const local = Math.round(amount * rate * 100) / 100;
      totalAmount += amount;
      totalLocal += local;
      if ((v.currency || company) !== company) {
        hasForeign = true;
        totalForeign += amount;
      }
    }

    return {
      lineCount: this.controls.length,
      totalAmount: Math.round(totalAmount * 100) / 100,
      totalLocal: Math.round(totalLocal * 100) / 100,
      totalForeign: Math.round(totalForeign * 100) / 100,
      hasForeign
    };
  }
}
