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
import { GoodsIssueRepository } from '../../../core/repositories/goods-issue.repository';
import { GoodsIssueDoc } from '../../../core/models/goods-issue.models';
import { InventoryPageShellComponent } from '../shared/inventory-page-shell.component';
import { InventoryEmptyStateComponent } from '../shared/inventory-empty-state.component';
import { InventoryErrorStateComponent } from '../shared/inventory-error-state.component';
import { InventorySkeletonComponent } from '../shared/inventory-skeleton.component';

type StatusFilter = 'all' | 'Draft' | 'Posted' | 'Approved' | 'Cancelled';
type DateFilter = 'all' | 'today' | '7d' | '30d';

@Component({
  selector: 'app-inventory-goods-issues-page',
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
  templateUrl: './inventory-goods-issues.page.html',
  styleUrl: './inventory-goods-issues.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryGoodsIssuesPage implements OnInit {
  private repo = inject(GoodsIssueRepository);
  private router = inject(Router);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  acting = signal(false);
  error = signal<string | null>(null);
  rows = signal<GoodsIssueDoc[]>([]);
  selectedId = signal<string | null>(null);
  search = signal('');
  statusFilter = signal<StatusFilter>('all');
  dateFilter = signal<DateFilter>('all');
  showDeleted = signal(false);
  pageSize = signal(50);

  breadcrumbs = [
    { labelKey: 'nav.inventory', path: '/inventory/dashboard' },
    { labelKey: 'inv.nav.goodsIssue' }
  ];

  canManage = computed(() => this.auth.hasPermission('Inventory.Manage'));
  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);

  filteredRows = computed(() => {
    let list = this.rows();
    if (!this.showDeleted()) {
      list = list.filter(r => r.status !== 'Cancelled');
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

    const statusMap: Record<StatusFilter, number | null> = {
      all: null,
      Draft: 1,
      Approved: 2,
      Posted: 3,
      Cancelled: 4
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
          this.error.set(err?.error?.error ?? this.t('inv.gi.loadFailed'));
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

  select(row: GoodsIssueDoc): void {
    this.selectedId.set(row.id);
  }

  createNew(): void {
    void this.router.navigate(['/inventory/goods-issues/new']);
  }

  edit(): void {
    const row = this.selected();
    if (!row) return;
    void this.router.navigate(['/inventory/goods-issues', row.id]);
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

  cancel(): void {
    const row = this.selected();
    if (!row || !this.canManage()) return;
    if (!confirm(this.t('inv.gi.confirmCancel'))) return;
    this.runAction(() => this.repo.cancel(row.id));
  }

  statusLabel(status: string): string {
    switch (status) {
      case 'Approved':
        return this.t('inv.gi.status.approved');
      case 'Posted':
        return this.t('inv.gi.status.posted');
      case 'Cancelled':
        return this.t('inv.gi.status.cancelled');
      default:
        return this.t('inv.gi.status.draft');
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
        this.acting.set(false);
        this.error.set(err?.error?.error ?? this.t('inv.gi.actionFailed'));
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
