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
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { BackOfficeSalesOrderRepository } from '../../../core/repositories/back-office-sales-order.repository';
import { BackOfficeSalesOrder } from '../../../core/models/back-office-sales-order.models';

type StatusFilter = 'all' | 'Draft' | 'Approved' | 'Posted' | 'Cancelled';

const STATUS_API: Record<Exclude<StatusFilter, 'all'>, number> = {
  Draft: 0,
  Approved: 1,
  Posted: 2,
  Cancelled: 9
};

@Component({
  selector: 'app-sales-orders-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, MatIconModule],
  templateUrl: './sales-orders.page.html',
  styleUrl: './sales-orders.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SalesOrdersPage implements OnInit {
  private repo = inject(BackOfficeSalesOrderRepository);
  private router = inject(Router);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  acting = signal(false);
  error = signal<string | null>(null);
  rows = signal<BackOfficeSalesOrder[]>([]);
  selectedId = signal<string | null>(null);
  search = signal('');
  statusFilter = signal<StatusFilter>('all');

  canManage = computed(
    () =>
      this.auth.hasPermission('BackOfficeSales.Create') ||
      this.auth.hasPermission('BackOfficeSales.Update') ||
      this.auth.hasPermission('Sales.View')
  );
  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);

  ngOnInit(): void {
    this.load();
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    const filter = this.statusFilter();
    this.repo
      .getList({
        page: 1,
        pageSize: 50,
        search: this.search().trim() || undefined,
        status: filter === 'all' ? null : STATUS_API[filter]
      })
      .subscribe({
        next: rows => {
          this.rows.set(rows);
          this.loading.set(false);
        },
        error: err => {
          this.error.set(err?.error?.error ?? this.t('bos.ord.loadFailed'));
          this.loading.set(false);
        }
      });
  }

  setStatus(filter: StatusFilter): void {
    this.statusFilter.set(filter);
    this.load();
  }

  select(row: BackOfficeSalesOrder): void {
    this.selectedId.set(row.id);
  }

  createNew(): void {
    void this.router.navigate(['/sales/orders/new']);
  }

  edit(): void {
    const row = this.selected();
    if (!row) return;
    void this.router.navigate(['/sales/orders', row.id]);
  }

  approve(): void {
    const row = this.selected();
    if (!row) return;
    this.run(() => this.repo.approve(row.id));
  }

  close(): void {
    const row = this.selected();
    if (!row) return;
    this.run(() => this.repo.close(row.id));
  }

  cancel(): void {
    const row = this.selected();
    if (!row) return;
    if (!confirm(this.t('bos.ord.confirmCancel'))) return;
    this.run(() => this.repo.cancel(row.id));
  }

  statusName(row: BackOfficeSalesOrder): string {
    if (typeof row.status === 'string' && isNaN(Number(row.status))) return row.status;
    switch (Number(row.status)) {
      case 1:
        return 'Approved';
      case 2:
        return 'Posted';
      case 9:
        return 'Cancelled';
      default:
        return 'Draft';
    }
  }

  statusLabel(row: BackOfficeSalesOrder): string {
    switch (this.statusName(row)) {
      case 'Approved':
        return this.t('bos.ord.status.approved');
      case 'Posted':
        return this.t('bos.ord.status.posted');
      case 'Cancelled':
        return this.t('bos.ord.status.cancelled');
      default:
        return this.t('bos.ord.status.draft');
    }
  }

  fulfillmentLabel(row: BackOfficeSalesOrder): string {
    switch (Number(row.fulfillmentStatus)) {
      case 1:
        return this.t('bos.ord.fulfillment.partiallyDelivered');
      case 2:
        return this.t('bos.ord.fulfillment.fullyDelivered');
      case 3:
        return this.t('bos.ord.fulfillment.partiallyInvoiced');
      case 4:
        return this.t('bos.ord.fulfillment.fullyInvoiced');
      case 5:
        return this.t('bos.ord.fulfillment.closed');
      default:
        return this.t('bos.ord.fulfillment.open');
    }
  }

  money(n: number): string {
    return (Number(n) || 0).toLocaleString(this.lang.language() === 'ar' ? 'ar-SA' : 'en-SA', {
      maximumFractionDigits: 2
    });
  }

  private run(action: () => import('rxjs').Observable<void>): void {
    this.acting.set(true);
    this.error.set(null);
    action().subscribe({
      next: () => {
        this.acting.set(false);
        this.load();
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('bos.ord.actionFailed'));
        this.acting.set(false);
      }
    });
  }
}
