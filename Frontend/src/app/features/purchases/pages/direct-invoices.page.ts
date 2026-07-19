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
import { catchError, of } from 'rxjs';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { InventoryService } from '../../../core/services/inventory.service';
import { PurchaseInvoiceRepository } from '../../../core/repositories/purchase-invoice.repository';
import { PurchaseInvoiceDoc } from '../../../core/models/purchase-invoice.models';
import { SupplierSummary } from '../../../core/models/inventory.models';
import { InventoryPageShellComponent } from '../../inventory/shared/inventory-page-shell.component';
import { InventoryEmptyStateComponent } from '../../inventory/shared/inventory-empty-state.component';
import { InventoryErrorStateComponent } from '../../inventory/shared/inventory-error-state.component';
import { InventorySkeletonComponent } from '../../inventory/shared/inventory-skeleton.component';

type StatusFilter = 'all' | 'Draft' | 'Approved' | 'Posted' | 'Cancelled';
type DateFilter = 'all' | 'today' | '7d' | '30d';

const DIRECT_KIND = 2;

const STATUS_API: Record<Exclude<StatusFilter, 'all'>, number> = {
  Draft: 0,
  Approved: 1,
  Posted: 2,
  Cancelled: 9
};

@Component({
  selector: 'app-direct-invoices-page',
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
  templateUrl: './direct-invoices.page.html',
  styleUrl: './direct-invoices.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DirectInvoicesPage implements OnInit {
  private repo = inject(PurchaseInvoiceRepository);
  private inventory = inject(InventoryService);
  private router = inject(Router);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  acting = signal(false);
  error = signal<string | null>(null);
  rows = signal<PurchaseInvoiceDoc[]>([]);
  suppliers = signal<SupplierSummary[]>([]);
  selectedId = signal<string | null>(null);
  search = signal('');
  statusFilter = signal<StatusFilter>('all');
  dateFilter = signal<DateFilter>('all');
  showDeleted = signal(false);
  pageSize = signal(50);

  breadcrumbs = [
    { labelKey: 'nav.purchases', path: '/purchases/dashboard' },
    { labelKey: 'pur.nav.directInvoices' }
  ];

  canManage = computed(() => this.auth.hasPermission('Inventory.Manage'));
  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);

  supplierMap = computed(() => {
    const map = new Map<string, string>();
    for (const s of this.suppliers()) {
      map.set(s.id, s.nameAr);
    }
    return map;
  });

  filteredRows = computed(() => {
    let list = this.rows();
    if (!this.showDeleted()) {
      list = list.filter(r => this.statusName(r) !== 'Cancelled');
    }
    return list;
  });

  ngOnInit(): void {
    this.inventory
      .getSuppliers(1, 500)
      .pipe(catchError(() => of([] as SupplierSummary[])))
      .subscribe(s => this.suppliers.set(s));
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
        kind: DIRECT_KIND,
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
          this.error.set(err?.error?.error ?? this.t('pur.dpi.loadFailed'));
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

  select(row: PurchaseInvoiceDoc): void {
    this.selectedId.set(row.id);
  }

  createNew(): void {
    void this.router.navigate(['/purchases/direct-invoices/new']);
  }

  edit(): void {
    const row = this.selected();
    if (!row) return;
    void this.router.navigate(['/purchases/direct-invoices', row.id]);
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
    if (!confirm(this.t('pur.dpi.confirmCancel'))) return;
    this.runAction(() => this.repo.cancel(row.id));
  }

  supplierName(row: PurchaseInvoiceDoc): string {
    return this.supplierMap().get(row.supplierId) ?? '—';
  }

  statusName(row: PurchaseInvoiceDoc): string {
    if (typeof row.status === 'string' && isNaN(Number(row.status))) return row.status;
    const code = typeof row.status === 'number' ? row.status : Number(row.status);
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
      default:
        return 'Draft';
    }
  }

  statusLabel(row: PurchaseInvoiceDoc): string {
    switch (this.statusName(row)) {
      case 'Approved':
        return this.t('pur.dpi.status.approved');
      case 'Posted':
        return this.t('pur.dpi.status.posted');
      case 'Reversed':
        return this.t('pur.dpi.status.reversed');
      case 'Cancelled':
        return this.t('pur.dpi.status.cancelled');
      default:
        return this.t('pur.dpi.status.draft');
    }
  }

  natureLabel(row: PurchaseInvoiceDoc): string {
    switch (Number(row.nature)) {
      case 1:
        return this.t('pur.dpi.nature.inventory');
      case 2:
        return this.t('pur.dpi.nature.services');
      case 3:
        return this.t('pur.dpi.nature.fixedAssets');
      default:
        return '—';
    }
  }

  paymentModeLabel(row: PurchaseInvoiceDoc): string {
    switch (Number(row.paymentMode)) {
      case 1:
        return this.t('pur.dpi.paymentMode.credit');
      case 2:
        return this.t('pur.dpi.paymentMode.cash');
      default:
        return '—';
    }
  }

  paymentStatusLabel(row: PurchaseInvoiceDoc): string {
    switch (Number(row.paymentStatus)) {
      case 2:
        return this.t('pur.dpi.paymentStatus.partiallyPaid');
      case 3:
        return this.t('pur.dpi.paymentStatus.fullyPaid');
      case 4:
        return this.t('pur.dpi.paymentStatus.fullyReturned');
      default:
        return this.t('pur.dpi.paymentStatus.unpaid');
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
        this.error.set(err?.error?.error ?? this.t('pur.dpi.actionFailed'));
        this.acting.set(false);
      }
    });
  }

  private dateRange(): { from: string | null; to: string | null } {
    const now = new Date();
    const end = now.toISOString().slice(0, 10);
    if (this.dateFilter() === 'today') {
      const start = new Date(now.getFullYear(), now.getMonth(), now.getDate());
      return { from: start.toISOString().slice(0, 10), to: end };
    }
    if (this.dateFilter() === '7d') {
      const start = new Date(now);
      start.setDate(start.getDate() - 7);
      return { from: start.toISOString().slice(0, 10), to: end };
    }
    if (this.dateFilter() === '30d') {
      const start = new Date(now);
      start.setDate(start.getDate() - 30);
      return { from: start.toISOString().slice(0, 10), to: end };
    }
    return { from: null, to: null };
  }
}
