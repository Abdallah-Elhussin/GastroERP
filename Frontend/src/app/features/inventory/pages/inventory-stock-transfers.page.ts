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
import { StockTransferRepository } from '../../../core/repositories/stock-transfer.repository';
import { StockTransferDoc } from '../../../core/models/stock-transfer.models';
import { InventoryPageShellComponent } from '../shared/inventory-page-shell.component';
import { InventoryEmptyStateComponent } from '../shared/inventory-empty-state.component';
import { InventoryErrorStateComponent } from '../shared/inventory-error-state.component';
import { InventorySkeletonComponent } from '../shared/inventory-skeleton.component';

type StatusFilter = 'all' | 'Draft' | 'Posted' | 'Approved' | 'Cancelled' | 'Completed';
type DateFilter = 'all' | 'today' | '7d' | '30d';

@Component({
  selector: 'app-inventory-stock-transfers-page',
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
  templateUrl: './inventory-stock-transfers.page.html',
  styleUrl: './inventory-stock-transfers.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryStockTransfersPage implements OnInit {
  private repo = inject(StockTransferRepository);
  private router = inject(Router);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  acting = signal(false);
  error = signal<string | null>(null);
  rows = signal<StockTransferDoc[]>([]);
  selectedId = signal<string | null>(null);
  search = signal('');
  statusFilter = signal<StatusFilter>('all');
  dateFilter = signal<DateFilter>('all');
  showDeleted = signal(false);
  pageSize = signal(50);

  breadcrumbs = [
    { labelKey: 'nav.inventory', path: '/inventory/dashboard' },
    { labelKey: 'inv.nav.stockTransfer' }
  ];

  canManage = computed(() =>
    this.auth.hasPermission('Stock.Transfer') || this.auth.hasPermission('Inventory.Manage')
  );
  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);

  filteredRows = computed(() => {
    let list = this.rows();
    if (!this.showDeleted()) list = list.filter(r => r.status !== 'Cancelled');
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
    const statusMap: Record<StatusFilter, number | null> = {
      all: null,
      Draft: 1,
      Posted: 2,
      Completed: 3,
      Cancelled: 4,
      Approved: 5
    };
    const { from, to } = this.dateRange();
    this.repo
      .getList({
        page: 1,
        pageSize: this.pageSize(),
        search: this.search().trim() || undefined,
        status: statusMap[this.statusFilter()],
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
          this.error.set(err?.error?.error ?? this.t('inv.st.loadFailed'));
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

  resetFilters(): void {
    this.statusFilter.set('all');
    this.dateFilter.set('all');
    this.search.set('');
    this.load();
  }

  select(row: StockTransferDoc): void {
    this.selectedId.set(row.id);
  }

  createNew(): void {
    void this.router.navigate(['/inventory/stock-transfers/new']);
  }

  edit(): void {
    const row = this.selected();
    if (!row) return;
    void this.router.navigate(['/inventory/stock-transfers', row.id]);
  }

  remove(): void {
    const row = this.selected();
    if (!row || !this.canManage() || row.status !== 'Draft') return;
    if (!confirm(this.t('inv.st.confirmDelete'))) return;
    this.runAction(() => this.repo.delete(row.id));
  }

  post(): void {
    const row = this.selected();
    if (!row || !this.canManage()) return;
    this.runAction(() => this.repo.post(row.id));
  }

  receive(): void {
    const row = this.selected();
    if (!row || !this.canManage()) return;
    this.runAction(() => this.repo.receive(row.id));
  }

  cancel(): void {
    const row = this.selected();
    if (!row || !this.canManage()) return;
    if (!confirm(this.t('inv.st.confirmCancel'))) return;
    this.runAction(() => this.repo.cancel(row.id));
  }

  statusLabel(status: string): string {
    switch (status) {
      case 'Approved':
        return this.t('inv.st.status.approved');
      case 'InTransit':
        return this.t('inv.st.status.posted');
      case 'Completed':
        return this.t('inv.st.status.received');
      case 'Cancelled':
        return this.t('inv.st.status.cancelled');
      default:
        return this.t('inv.st.status.draft');
    }
  }

  typeLabel(code: number): string {
    return code === 2 ? this.t('inv.st.type.inbound') : this.t('inv.st.type.outbound');
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
        this.acting.set(false);
        this.error.set(err?.error?.error ?? this.t('inv.st.actionFailed'));
      }
    });
  }

  private dateRange(): { from: string | null; to: string | null } {
    const filter = this.dateFilter();
    if (filter === 'all') return { from: null, to: null };
    const now = new Date();
    const end = new Date(now);
    end.setHours(23, 59, 59, 999);
    const start = new Date(now);
    start.setHours(0, 0, 0, 0);
    if (filter === '7d') start.setDate(start.getDate() - 6);
    if (filter === '30d') start.setDate(start.getDate() - 29);
    return { from: start.toISOString(), to: end.toISOString() };
  }
}
