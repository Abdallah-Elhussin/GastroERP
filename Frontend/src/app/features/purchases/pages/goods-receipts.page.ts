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
import { Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { GoodsReceiptRepository } from '../../../core/repositories/goods-receipt.repository';
import { GoodsReceiptDoc } from '../../../core/models/goods-receipt.models';
import { InventoryPageShellComponent } from '../../inventory/shared/inventory-page-shell.component';
import { InventoryEmptyStateComponent } from '../../inventory/shared/inventory-empty-state.component';
import { InventoryErrorStateComponent } from '../../inventory/shared/inventory-error-state.component';
import { InventorySkeletonComponent } from '../../inventory/shared/inventory-skeleton.component';

type StatusFilter = 'all' | 'Draft' | 'Approved' | 'Posted' | 'Cancelled';
type DateFilter = 'all' | 'today' | '7d' | '30d';

/** Backend GoodsReceiptStatus enum values */
const STATUS_API: Record<Exclude<StatusFilter, 'all'>, number> = {
  Draft: 1,
  Posted: 2,
  Cancelled: 3,
  Approved: 4
};

@Component({
  selector: 'app-goods-receipts-page',
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
  templateUrl: './goods-receipts.page.html',
  styleUrl: './goods-receipts.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GoodsReceiptsPage implements OnInit {
  private repo = inject(GoodsReceiptRepository);
  private router = inject(Router);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  acting = signal(false);
  error = signal<string | null>(null);
  rows = signal<GoodsReceiptDoc[]>([]);
  selectedId = signal<string | null>(null);
  search = signal('');
  statusFilter = signal<StatusFilter>('all');
  dateFilter = signal<DateFilter>('all');
  showDeleted = signal(false);
  pageSize = signal(50);

  breadcrumbs = [
    { labelKey: 'nav.purchases', path: '/purchases/dashboard' },
    { labelKey: 'pur.nav.goodsReceipts' }
  ];

  canManage = computed(() => this.auth.hasPermission('Inventory.Manage'));
  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);

  filteredRows = computed(() => {
    let list = this.rows();
    if (!this.showDeleted()) {
      list = list.filter(r => this.statusName(r) !== 'Cancelled');
    }
    return list;
  });

  ngOnInit(): void {
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
          this.error.set(err?.error?.error ?? this.t('pur.grn.loadFailed'));
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

  select(row: GoodsReceiptDoc): void {
    this.selectedId.set(row.id);
  }

  createNew(): void {
    void this.router.navigate(['/purchases/goods-receipts/new']);
  }

  edit(): void {
    const row = this.selected();
    if (!row) return;
    void this.router.navigate(['/purchases/goods-receipts', row.id]);
  }

  approve(): void {
    const row = this.selected();
    if (!row || !this.canManage()) return;
    this.runAction(() => this.repo.approve(row.id));
  }

  post(): void {
    const row = this.selected();
    if (!row || !this.canManage()) return;
    this.runAction(() => this.repo.post(row.id));
  }

  unpost(): void {
    const row = this.selected();
    if (!row || !this.canManage()) return;
    this.runAction(() => this.repo.unpost(row.id));
  }

  cancel(): void {
    const row = this.selected();
    if (!row || !this.canManage()) return;
    if (!confirm(this.t('pur.grn.confirmCancel'))) return;
    this.runAction(() => this.repo.cancel(row.id));
  }

  createInvoice(): void {
    const row = this.selected();
    if (!row || this.statusName(row) !== 'Posted') return;
    void this.router.navigate(['/purchases/purchase-invoices'], {
      queryParams: { goodsReceiptId: row.id }
    });
  }

  statusName(row: GoodsReceiptDoc): string {
    const code = typeof row.status === 'number' ? row.status : row.unifiedStatusCode;
    if (typeof row.status === 'string' && isNaN(Number(row.status))) return row.status;
    switch (Number(code)) {
      case 1:
        return 'Draft';
      case 4:
        return 'Approved';
      case 2:
        return 'Posted';
      case 8:
        return 'Reversed';
      case 3:
      case 9:
        return 'Cancelled';
      default:
        // unified codes on DTO
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
  }

  statusLabel(row: GoodsReceiptDoc): string {
    switch (this.statusName(row)) {
      case 'Approved':
        return this.t('pur.grn.status.approved');
      case 'Posted':
        return this.t('pur.grn.status.posted');
      case 'Reversed':
        return this.t('pur.grn.status.reversed');
      case 'Cancelled':
        return this.t('pur.grn.status.cancelled');
      default:
        return this.t('pur.grn.status.draft');
    }
  }

  invoicedLabel(row: GoodsReceiptDoc): string {
    if (row.isInvoiced) return this.t('pur.grn.invoiced.yes');
    if (row.isPartiallyInvoiced) return this.t('pur.grn.invoiced.partial');
    return this.t('pur.grn.invoiced.no');
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
        this.error.set(err?.error?.error ?? this.t('pur.grn.actionFailed'));
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
