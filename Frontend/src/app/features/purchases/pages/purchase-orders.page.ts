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
import { Observable, catchError, of } from 'rxjs';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { PurchaseOrderRepository } from '../../../core/repositories/purchase-order.repository';
import {
  PURCHASE_ORDER_STATUS,
  PurchaseOrderDashboardDto,
  PurchaseOrderDto
} from '../../../core/models/purchase-order.models';
import { InventoryPageShellComponent } from '../../inventory/shared/inventory-page-shell.component';
import { InventoryEmptyStateComponent } from '../../inventory/shared/inventory-empty-state.component';
import { InventoryErrorStateComponent } from '../../inventory/shared/inventory-error-state.component';
import { InventorySkeletonComponent } from '../../inventory/shared/inventory-skeleton.component';

type StatusFilter = 'all' | 'Draft' | 'Approved' | 'PartiallyReceived' | 'Closed' | 'Cancelled';
type DateFilter = 'all' | 'today' | '7d' | '30d';

const STATUS_API: Record<Exclude<StatusFilter, 'all'>, number> = {
  Draft: PURCHASE_ORDER_STATUS.Draft,
  Approved: PURCHASE_ORDER_STATUS.Approved,
  PartiallyReceived: PURCHASE_ORDER_STATUS.PartiallyReceived,
  Closed: PURCHASE_ORDER_STATUS.Closed,
  Cancelled: PURCHASE_ORDER_STATUS.Cancelled
};

interface PoKpiCard {
  key: string;
  icon: string;
  value: number;
  tone: 'violet' | 'green' | 'amber' | 'rose' | 'blue' | 'slate';
  isCurrency?: boolean;
}

@Component({
  selector: 'app-purchase-orders-page',
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
  templateUrl: './purchase-orders.page.html',
  styleUrl: './purchase-orders.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PurchaseOrdersPage implements OnInit {
  private repo = inject(PurchaseOrderRepository);
  private router = inject(Router);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  dashboardLoading = signal(false);
  acting = signal(false);
  error = signal<string | null>(null);
  rows = signal<PurchaseOrderDto[]>([]);
  totalCount = signal(0);
  dashboard = signal<PurchaseOrderDashboardDto | null>(null);
  selectedId = signal<string | null>(null);
  search = signal('');
  statusFilter = signal<StatusFilter>('all');
  dateFilter = signal<DateFilter>('all');
  showCancelled = signal(false);
  pageSize = signal(50);

  breadcrumbs = [
    { labelKey: 'nav.purchases', path: '/purchases/dashboard' },
    { labelKey: 'pur.nav.purchaseOrders' }
  ];

  canCreate = computed(() => this.auth.hasPermission('Purchase.Create') || this.auth.hasPermission('Inventory.Manage'));
  canApprove = computed(() => this.auth.hasPermission('Purchase.Approve') || this.auth.hasPermission('Inventory.Manage'));
  canCancel = computed(() => this.auth.hasPermission('Purchase.Cancel') || this.auth.hasPermission('Inventory.Manage'));

  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);

  kpiCards = computed<PoKpiCard[]>(() => {
    const d = this.dashboard();
    return [
      { key: 'pur.po.kpi.ordersToday', icon: 'today', value: d?.ordersToday ?? 0, tone: 'violet' },
      { key: 'pur.po.kpi.approvedCount', icon: 'check_circle', value: d?.approvedCount ?? 0, tone: 'green' },
      { key: 'pur.po.kpi.awaitingReceipt', icon: 'local_shipping', value: d?.awaitingReceiptCount ?? 0, tone: 'blue' },
      { key: 'pur.po.kpi.closedCount', icon: 'task_alt', value: d?.closedCount ?? 0, tone: 'slate' },
      { key: 'pur.po.kpi.overdueCount', icon: 'warning', value: d?.overdueCount ?? 0, tone: 'rose' },
      { key: 'pur.po.kpi.totalValue', icon: 'payments', value: d?.totalValue ?? 0, tone: 'amber', isCurrency: true }
    ];
  });

  filteredRows = computed(() => {
    let list = this.rows();
    if (!this.showCancelled()) {
      list = list.filter(r => this.statusName(r) !== 'Cancelled');
    }
    return list;
  });

  ngOnInit(): void {
    this.loadDashboard();
    this.load();
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  loadDashboard(): void {
    this.dashboardLoading.set(true);
    this.repo
      .getDashboard()
      .pipe(catchError(() => of(null)))
      .subscribe(d => {
        this.dashboard.set(d);
        this.dashboardLoading.set(false);
      });
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
        next: page => {
          this.rows.set(page.items);
          this.totalCount.set(page.totalCount);
          this.loading.set(false);
          if (this.selectedId() && !page.items.some(r => r.id === this.selectedId())) {
            this.selectedId.set(null);
          }
        },
        error: err => {
          this.error.set(err?.error?.error ?? this.t('pur.po.loadFailed'));
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

  select(row: PurchaseOrderDto): void {
    this.selectedId.set(row.id);
  }

  createNew(): void {
    void this.router.navigate(['/purchases/purchase-orders/new']);
  }

  edit(): void {
    const row = this.selected();
    if (!row) return;
    void this.router.navigate(['/purchases/purchase-orders', row.id]);
  }

  approve(): void {
    const row = this.selected();
    if (!row || !this.canApprove() || !this.canApproveDoc(row)) return;
    this.runAction(() => this.repo.approve(row.id));
  }

  send(): void {
    const row = this.selected();
    if (!row || !this.canApprove() || !this.canSendDoc(row)) return;
    this.runAction(() => this.repo.send(row.id));
  }

  close(): void {
    const row = this.selected();
    if (!row || !this.canApprove() || !this.canCloseDoc(row)) return;
    if (!confirm(this.t('pur.po.confirmClose'))) return;
    this.runAction(() => this.repo.close(row.id));
  }

  cancel(): void {
    const row = this.selected();
    if (!row || !this.canCancel() || !this.canCancelDoc(row)) return;
    if (!confirm(this.t('pur.po.confirmCancel'))) return;
    this.runAction(() => this.repo.cancel(row.id));
  }

  copy(): void {
    const row = this.selected();
    if (!row || !this.canCreate()) return;
    this.acting.set(true);
    this.error.set(null);
    this.repo.copy(row.id).subscribe({
      next: doc => {
        this.acting.set(false);
        void this.router.navigate(['/purchases/purchase-orders', doc.id]);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('pur.po.actionFailed'));
        this.acting.set(false);
      }
    });
  }

  createGrn(): void {
    const row = this.selected();
    if (!row || !this.canCreateGrn(row)) return;
    void this.router.navigate(['/purchases/goods-receipts/new'], { queryParams: { poId: row.id } });
  }

  deleteDoc(): void {
    const row = this.selected();
    if (!row || !this.canCancel() || this.statusName(row) !== 'Draft') return;
    if (!confirm(this.t('pur.po.confirmDelete'))) return;
    this.acting.set(true);
    this.error.set(null);
    this.repo.delete(row.id).subscribe({
      next: () => {
        this.acting.set(false);
        this.selectedId.set(null);
        this.load();
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('pur.po.deleteFailed'));
        this.acting.set(false);
      }
    });
  }

  canApproveDoc(row: PurchaseOrderDto): boolean {
    const s = this.statusName(row);
    return s === 'Draft' || s === 'PendingApproval';
  }

  canSendDoc(row: PurchaseOrderDto): boolean {
    return this.statusName(row) === 'Approved';
  }

  canCloseDoc(row: PurchaseOrderDto): boolean {
    const s = this.statusName(row);
    return !['Draft', 'Cancelled', 'Closed', 'Rejected'].includes(s);
  }

  canCancelDoc(row: PurchaseOrderDto): boolean {
    const s = this.statusName(row);
    return !['Closed', 'Cancelled', 'FullyReceived'].includes(s) && Number(row.completionPercent) <= 0;
  }

  canCreateGrn(row: PurchaseOrderDto): boolean {
    const s = this.statusName(row);
    return ['Approved', 'SentToSupplier', 'PartiallyReceived'].includes(s) && Number(row.remainingQuantity) > 0;
  }

  statusName(row: PurchaseOrderDto): string {
    const code = Number(row.statusCode ?? row.status);
    switch (code) {
      case PURCHASE_ORDER_STATUS.Draft:
        return 'Draft';
      case PURCHASE_ORDER_STATUS.Approved:
        return 'Approved';
      case PURCHASE_ORDER_STATUS.SentToSupplier:
        return 'SentToSupplier';
      case PURCHASE_ORDER_STATUS.PartiallyReceived:
        return 'PartiallyReceived';
      case PURCHASE_ORDER_STATUS.FullyReceived:
        return 'FullyReceived';
      case PURCHASE_ORDER_STATUS.Cancelled:
        return 'Cancelled';
      case PURCHASE_ORDER_STATUS.Closed:
        return 'Closed';
      case PURCHASE_ORDER_STATUS.Rejected:
        return 'Rejected';
      case PURCHASE_ORDER_STATUS.PendingApproval:
        return 'PendingApproval';
      default:
        return 'Draft';
    }
  }

  statusLabel(row: PurchaseOrderDto): string {
    return this.t(`pur.po.status.${this.statusKeySuffix(this.statusName(row))}`);
  }

  private statusKeySuffix(name: string): string {
    switch (name) {
      case 'Draft':
        return 'draft';
      case 'Approved':
        return 'approved';
      case 'SentToSupplier':
        return 'sentToSupplier';
      case 'PartiallyReceived':
        return 'partiallyReceived';
      case 'FullyReceived':
        return 'fullyReceived';
      case 'Cancelled':
        return 'cancelled';
      case 'Closed':
        return 'closed';
      case 'Rejected':
        return 'rejected';
      case 'PendingApproval':
        return 'pendingApproval';
      default:
        return 'draft';
    }
  }

  private runAction(action: () => Observable<void>): void {
    this.acting.set(true);
    this.error.set(null);
    action().subscribe({
      next: () => {
        this.acting.set(false);
        this.load();
        this.loadDashboard();
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('pur.po.actionFailed'));
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
