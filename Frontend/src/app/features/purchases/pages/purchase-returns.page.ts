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
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { PurchaseReturnRepository } from '../../../core/repositories/purchase-return.repository';
import { PurchaseReturnDoc } from '../../../core/models/purchase-return.models';
import { InventoryPageShellComponent } from '../../inventory/shared/inventory-page-shell.component';
import { InventoryEmptyStateComponent } from '../../inventory/shared/inventory-empty-state.component';
import { InventoryErrorStateComponent } from '../../inventory/shared/inventory-error-state.component';
import { InventorySkeletonComponent } from '../../inventory/shared/inventory-skeleton.component';

type StatusFilter = 'all' | 'Draft' | 'Approved' | 'Posted' | 'Cancelled';
type DateFilter = 'all' | 'today' | '7d' | '30d';

/** PurchasingDocumentStatus: Draft=0, Approved=1, Posted=2, Reversed=8, Cancelled=9 */
const STATUS_API: Record<Exclude<StatusFilter, 'all'>, number> = {
  Draft: 0,
  Approved: 1,
  Posted: 2,
  Cancelled: 9
};

@Component({
  selector: 'app-purchase-returns-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    MatIconModule,
    MatTooltipModule,
    InventoryPageShellComponent,
    InventoryEmptyStateComponent,
    InventoryErrorStateComponent,
    InventorySkeletonComponent
  ],
  templateUrl: './purchase-returns.page.html',
  styleUrl: './purchase-returns.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PurchaseReturnsPage implements OnInit {
  private repo = inject(PurchaseReturnRepository);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  acting = signal(false);
  error = signal<string | null>(null);
  rows = signal<PurchaseReturnDoc[]>([]);
  selectedId = signal<string | null>(null);
  search = signal('');
  statusFilter = signal<StatusFilter>('all');
  dateFilter = signal<DateFilter>('all');
  showDeleted = signal(false);
  pageSize = signal(50);

  /** When set (e.g. legacy single-type filter), API list is filtered to this returnType. */
  returnTypeFilter = signal<number | null>(null);
  /** Invoice-based returns: FromReceipt (AfterInvoice) + Direct. */
  isInvoiceReturnsMode = signal(false);

  breadcrumbs = computed(() => [
    { labelKey: 'nav.purchases', path: '/purchases/dashboard' },
    {
      labelKey: this.isInvoiceReturnsMode() ? 'pur.nav.invoiceReturns' : 'pur.nav.purchaseReturns'
    }
  ]);

  titleKey = computed(() =>
    this.isInvoiceReturnsMode() ? 'pur.nav.invoiceReturns' : 'pur.nav.purchaseReturns'
  );
  subtitleKey = computed(() =>
    this.isInvoiceReturnsMode() ? 'pur.pr.invoiceSubtitle' : 'pur.pr.subtitle'
  );
  listBasePath = computed(() =>
    this.isInvoiceReturnsMode() ? '/purchases/invoice-returns' : '/purchases/purchase-returns'
  );

  canManage = computed(() => this.auth.hasPermission('Inventory.Manage'));
  isDirectMode = computed(() => this.returnTypeFilter() != null);
  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);

  filteredRows = computed(() => {
    let list = this.rows();
    if (!this.showDeleted()) {
      list = list.filter(r => this.statusName(r) !== 'Cancelled');
    }
    return list;
  });

  ngOnInit(): void {
    const data = this.route.snapshot.data;
    const path = this.route.snapshot.routeConfig?.path ?? '';
    const invoiceMode =
      data['invoiceReturnsMode'] === true ||
      path.startsWith('invoice-returns') ||
      path.startsWith('direct-returns');
    this.isInvoiceReturnsMode.set(invoiceMode);
    const dataFilter = data['returnTypeFilter'] as number | undefined;
    this.returnTypeFilter.set(!invoiceMode && dataFilter != null ? dataFilter : null);
    this.load();
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    const { from, to } = this.dateRange();
    const filter = this.statusFilter();
    this.repo
      .getList({
        page: 1,
        pageSize: this.pageSize(),
        search: this.search().trim() || undefined,
        status: filter === 'all' ? null : STATUS_API[filter],
        returnType: this.returnTypeFilter(),
        invoiceBasedOnly: this.isInvoiceReturnsMode() && this.returnTypeFilter() == null,
        from,
        to
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
          this.error.set(err?.error?.error ?? this.t('pur.pr.loadFailed'));
          this.loading.set(false);
        }
      });
  }

  setStatus(filter: StatusFilter): void {
    this.statusFilter.set(filter);
    this.load();
  }

  setDate(filter: DateFilter): void {
    this.dateFilter.set(filter);
    this.load();
  }

  select(row: PurchaseReturnDoc): void {
    this.selectedId.set(row.id);
  }

  createNew(): void {
    void this.router.navigate([`${this.listBasePath()}/new`]);
  }

  edit(): void {
    const row = this.selected();
    if (!row) return;
    void this.router.navigate([this.listBasePath(), row.id]);
  }

  post(): void {
    const row = this.selected();
    if (!row || !this.canManage()) return;
    this.runAction(() => this.repo.post(row.id));
  }

  approve(): void {
    const row = this.selected();
    if (!row || !this.canManage()) return;
    if (!confirm(this.t('pur.pr.confirmApprove'))) return;
    this.runAction(() => this.repo.approve(row.id));
  }

  cancel(): void {
    const row = this.selected();
    if (!row || !this.canManage()) return;
    if (!confirm(this.t('pur.pr.confirmCancel'))) return;
    this.runAction(() => this.repo.cancel(row.id));
  }

  statusName(row: PurchaseReturnDoc): string {
    if (typeof row.status === 'string' && isNaN(Number(row.status))) return row.status;
    const code = typeof row.status === 'number' ? row.status : Number(row.status);
    if (!isNaN(code)) {
      switch (code) {
        case 0:
          return 'Draft';
        case 1:
          return 'Approved';
        case 2:
          return 'Posted';
        case 8:
          return 'Reversed';
        case 9:
          return 'Cancelled';
      }
    }
    switch (row.unifiedStatusCode) {
      case 0:
        return 'Draft';
      case 1:
        return 'Approved';
      case 2:
        return 'Posted';
      case 8:
        return 'Reversed';
      case 9:
        return 'Cancelled';
      default:
        return 'Draft';
    }
  }

  statusLabel(row: PurchaseReturnDoc): string {
    switch (this.statusName(row)) {
      case 'Approved':
        return this.t('pur.pr.status.approved');
      case 'Posted':
        return this.t('pur.pr.status.posted');
      case 'Reversed':
        return this.t('pur.pr.status.reversed');
      case 'Cancelled':
        return this.t('pur.pr.status.cancelled');
      default:
        return this.t('pur.pr.status.draft');
    }
  }

  returnTypeLabel(row: PurchaseReturnDoc): string {
    switch (Number(row.returnType)) {
      case 1:
        return this.t('pur.pr.type.beforeInvoice');
      case 2:
        return this.t('pur.pr.type.afterInvoice');
      case 3:
        return this.t('pur.pr.type.direct');
      default:
        return '—';
    }
  }

  private runAction(action: () => import('rxjs').Observable<void>): void {
    this.acting.set(true);
    this.error.set(null);
    action().subscribe({
      next: () => {
        this.acting.set(false);
        this.load();
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('pur.pr.actionFailed'));
        this.acting.set(false);
      }
    });
  }

  private dateRange(): { from: string | null; to: string | null } {
    const now = new Date();
    const end = now.toISOString();
    if (this.dateFilter() === 'today') {
      const start = new Date(now.getFullYear(), now.getMonth(), now.getDate());
      return { from: start.toISOString(), to: end };
    }
    if (this.dateFilter() === '7d') {
      const start = new Date(now);
      start.setDate(start.getDate() - 7);
      return { from: start.toISOString(), to: end };
    }
    if (this.dateFilter() === '30d') {
      const start = new Date(now);
      start.setDate(start.getDate() - 30);
      return { from: start.toISOString(), to: end };
    }
    return { from: null, to: null };
  }
}
