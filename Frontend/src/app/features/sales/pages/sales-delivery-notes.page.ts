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
import { catchError, of } from 'rxjs';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { BackOfficeSalesDeliveryNoteRepository } from '../../../core/repositories/back-office-sales-delivery-note.repository';
import { BackOfficeSalesDeliveryNote } from '../../../core/models/back-office-sales-delivery-note.models';
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
  selector: 'app-sales-delivery-notes-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, MatIconModule],
  templateUrl: './sales-delivery-notes.page.html',
  styleUrl: './sales-delivery-notes.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SalesDeliveryNotesPage implements OnInit {
  private repo = inject(BackOfficeSalesDeliveryNoteRepository);
  private orderRepo = inject(BackOfficeSalesOrderRepository);
  private router = inject(Router);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  acting = signal(false);
  error = signal<string | null>(null);
  rows = signal<BackOfficeSalesDeliveryNote[]>([]);
  selectedId = signal<string | null>(null);
  search = signal('');
  statusFilter = signal<StatusFilter>('all');
  orderNumbers = signal<Record<string, string>>({});

  canManage = computed(
    () =>
      this.auth.hasPermission('BackOfficeSales.Create') ||
      this.auth.hasPermission('BackOfficeSales.Update') ||
      this.auth.hasPermission('Sales.View')
  );
  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);

  ngOnInit(): void {
    this.orderRepo
      .getList({ pageSize: 500 })
      .pipe(catchError(() => of([] as BackOfficeSalesOrder[])))
      .subscribe(orders => {
        const map: Record<string, string> = {};
        for (const o of orders) map[o.id] = o.orderNumber;
        this.orderNumbers.set(map);
      });
    this.load();
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  orderNumber(orderId: string): string {
    return this.orderNumbers()[orderId] ?? orderId.slice(0, 8);
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
          this.error.set(err?.error?.error ?? this.t('bos.dn.loadFailed'));
          this.loading.set(false);
        }
      });
  }

  setStatus(filter: StatusFilter): void {
    this.statusFilter.set(filter);
    this.load();
  }

  select(row: BackOfficeSalesDeliveryNote): void {
    this.selectedId.set(row.id);
  }

  createNew(): void {
    void this.router.navigate(['/sales/delivery-notes/new']);
  }

  edit(): void {
    const row = this.selected();
    if (!row) return;
    void this.router.navigate(['/sales/delivery-notes', row.id]);
  }

  approve(): void {
    const row = this.selected();
    if (!row) return;
    this.run(() => this.repo.approve(row.id));
  }

  post(): void {
    const row = this.selected();
    if (!row) return;
    this.run(() => this.repo.post(row.id));
  }

  unpost(): void {
    const row = this.selected();
    if (!row) return;
    this.run(() => this.repo.unpost(row.id));
  }

  cancel(): void {
    const row = this.selected();
    if (!row) return;
    if (!confirm(this.t('bos.dn.confirmCancel'))) return;
    this.run(() => this.repo.cancel(row.id));
  }

  statusName(row: BackOfficeSalesDeliveryNote): string {
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

  statusLabel(row: BackOfficeSalesDeliveryNote): string {
    switch (this.statusName(row)) {
      case 'Approved':
        return this.t('bos.dn.status.approved');
      case 'Posted':
        return this.t('bos.dn.status.posted');
      case 'Cancelled':
        return this.t('bos.dn.status.cancelled');
      default:
        return this.t('bos.dn.status.draft');
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
        this.error.set(err?.error?.error ?? this.t('bos.dn.actionFailed'));
        this.acting.set(false);
      }
    });
  }
}
