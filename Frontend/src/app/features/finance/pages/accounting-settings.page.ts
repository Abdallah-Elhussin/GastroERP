import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { ChartOfAccountRepository } from '../../../core/repositories/chart-of-account.repository';
import { AccountingSettings, ChartAccount } from '../../../core/models/chart-of-account.models';
import { flattenTreeAccounts } from './coa-tree.util';

@Component({
  selector: 'app-accounting-settings-page',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, RouterLink],
  templateUrl: './accounting-settings.page.html',
  styleUrl: './accounting-settings.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AccountingSettingsPage implements OnInit {
  private repo = inject(ChartOfAccountRepository);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  settings = signal<AccountingSettings | null>(null);
  accounts = signal<ChartAccount[]>([]);
  tab = signal<'numbering' | 'mapping' | 'posting'>('numbering');

  canEdit = computed(() => this.auth.hasPermission('Accounting.Update'));
  t = (key: string) => this.lang.t(key);

  mappingFields: { key: keyof AccountingSettings; labelKey: string }[] = [
    { key: 'cashAccountId', labelKey: 'fin.settings.map.cash' },
    { key: 'bankAccountId', labelKey: 'fin.settings.map.bank' },
    { key: 'inventoryAccountId', labelKey: 'fin.settings.map.inventory' },
    { key: 'grniAccountId', labelKey: 'fin.settings.map.grni' },
    { key: 'cogsAccountId', labelKey: 'fin.settings.map.cogs' },
    { key: 'salesRevenueAccountId', labelKey: 'fin.settings.map.sales' },
    { key: 'purchaseAccountId', labelKey: 'fin.settings.map.purchase' },
    { key: 'accountsReceivableAccountId', labelKey: 'fin.settings.map.ar' },
    { key: 'accountsPayableAccountId', labelKey: 'fin.settings.map.ap' },
    { key: 'vatInputAccountId', labelKey: 'fin.settings.map.vatIn' },
    { key: 'vatOutputAccountId', labelKey: 'fin.settings.map.vatOut' },
    { key: 'discountAccountId', labelKey: 'fin.settings.map.discount' },
    { key: 'roundOffAccountId', labelKey: 'fin.settings.map.roundOff' },
    { key: 'openingBalanceAccountId', labelKey: 'fin.settings.map.opening' },
    { key: 'retainedEarningsAccountId', labelKey: 'fin.settings.map.retained' },
    { key: 'payrollExpenseAccountId', labelKey: 'fin.settings.map.payrollExp' },
    { key: 'payrollLiabilityAccountId', labelKey: 'fin.settings.map.payrollLiab' },
    { key: 'productionVarianceAccountId', labelKey: 'fin.settings.map.prodVar' },
    { key: 'inventoryAdjustmentAccountId', labelKey: 'fin.settings.map.invAdj' },
    { key: 'wasteAccountId', labelKey: 'fin.settings.map.waste' },
    { key: 'deliveryRevenueAccountId', labelKey: 'fin.settings.map.delRev' },
    { key: 'deliveryExpenseAccountId', labelKey: 'fin.settings.map.delExp' },
    { key: 'kitchenConsumptionAccountId', labelKey: 'fin.settings.map.kitchen' },
    { key: 'customerAdvancesAccountId', labelKey: 'fin.settings.map.custAdv' },
    { key: 'supplierAdvancesAccountId', labelKey: 'fin.settings.map.supAdv' },
    { key: 'exchangeDifferenceAccountId', labelKey: 'fin.settings.map.fx' }
  ];

  postingFields: { key: keyof AccountingSettings; labelKey: string }[] = [
    { key: 'autoPostSales', labelKey: 'fin.settings.post.sales' },
    { key: 'autoPostPurchases', labelKey: 'fin.settings.post.purchases' },
    { key: 'autoPostGoodsReceipt', labelKey: 'fin.settings.post.gr' },
    { key: 'autoPostGoodsIssue', labelKey: 'fin.settings.post.gi' },
    { key: 'autoPostStockTransfer', labelKey: 'fin.settings.post.transfer' },
    { key: 'autoPostWaste', labelKey: 'fin.settings.post.waste' },
    { key: 'autoPostProduction', labelKey: 'fin.settings.post.production' },
    { key: 'autoPostPayroll', labelKey: 'fin.settings.post.payroll' }
  ];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo.getSettings().subscribe({
      next: s => {
        this.settings.set(s);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.message || this.t('fin.settings.loadError'));
        this.loading.set(false);
      }
    });
    this.repo.getTree({ includeInactive: false }).subscribe({
      next: tree => this.accounts.set(flattenTreeAccounts(tree).filter(a => !a.isSummaryAccount)),
      error: () => undefined
    });
  }

  setAccount(key: keyof AccountingSettings, value: string | null): void {
    const s = this.settings();
    if (!s) return;
    this.settings.set({ ...s, [key]: value || null });
  }

  setFlag(key: keyof AccountingSettings, value: boolean): void {
    const s = this.settings();
    if (!s) return;
    this.settings.set({ ...s, [key]: value });
  }

  setNumbering(field: 'accountNumberMaxLength' | 'maxTreeLevels' | 'levelLengthsCsv' | 'levelSeparator', value: string | number): void {
    const s = this.settings();
    if (!s) return;
    this.settings.set({ ...s, [field]: value });
  }

  accountLabel(id: string | null | undefined): string {
    if (!id) return '—';
    const a = this.accounts().find(x => x.id === id);
    return a ? `${a.accountNumber} — ${a.nameAr}` : id;
  }

  save(): void {
    const s = this.settings();
    if (!s || !this.canEdit()) return;
    this.saving.set(true);
    this.repo.saveSettings(s).subscribe({
      next: updated => {
        this.settings.set(updated);
        this.saving.set(false);
      },
      error: err => {
        this.error.set(err?.error?.detail || err?.message || this.t('fin.settings.saveError'));
        this.saving.set(false);
      }
    });
  }
}
