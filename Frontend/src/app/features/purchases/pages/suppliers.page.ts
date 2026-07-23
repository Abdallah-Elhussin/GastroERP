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
import { SupplierRepository } from '../../../core/repositories/supplier.repository';
import { SupplierListItem } from '../../../core/models/supplier.models';
import { InventoryPageShellComponent } from '../../inventory/shared/inventory-page-shell.component';

@Component({
  selector: 'app-suppliers-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, MatIconModule, InventoryPageShellComponent],
  templateUrl: './suppliers.page.html',
  styleUrl: './suppliers.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SuppliersPage implements OnInit {
  private repo = inject(SupplierRepository);
  private router = inject(Router);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  error = signal<string | null>(null);
  rows = signal<SupplierListItem[]>([]);
  search = signal('');
  activeOnly = signal(true);
  selectedId = signal<string | null>(null);

  breadcrumbs = [
    { labelKey: 'nav.purchases', path: '/purchases/dashboard' },
    { labelKey: 'pur.nav.suppliers' }
  ];

  canView = computed(() =>
    this.auth.hasPermission('Supplier.View') || this.auth.hasPermission('Inventory.View')
  );
  canCreate = computed(() =>
    this.auth.hasPermission('Supplier.Create') || this.auth.hasPermission('Inventory.Manage')
  );
  canEdit = computed(() =>
    this.auth.hasPermission('Supplier.Update') || this.auth.hasPermission('Inventory.Manage')
  );
  canDelete = computed(() =>
    this.auth.hasPermission('Supplier.Delete') || this.auth.hasPermission('Inventory.Manage')
  );

  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);

  filtered = computed(() => {
    const q = this.search().trim().toLowerCase();
    const rows = this.rows();
    let list = Array.isArray(rows) ? rows : [];
    if (this.activeOnly()) {
      list = list.filter(r => r.isActive !== false && r.isBlacklisted !== true);
    }
    if (!q) return list;
    return list.filter(r => {
      const code = (r.code ?? '').toLowerCase();
      const nameAr = (r.nameAr ?? '').toLowerCase();
      const nameEn = (r.nameEn ?? '').toLowerCase();
      const tax = (r.taxNumber ?? '').toLowerCase();
      const city = (r.city ?? '').toLowerCase();
      return (
        code.includes(q) ||
        nameAr.includes(q) ||
        nameEn.includes(q) ||
        tax.includes(q) ||
        city.includes(q)
      );
    });
  });

  ngOnInit(): void {
    this.refresh();
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  refresh(): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo.getList({ page: 1, pageSize: 200 }).subscribe({
      next: rows => {
        this.rows.set(Array.isArray(rows) ? rows : []);
        this.loading.set(false);
        if (this.selectedId() && !this.rows().some(r => r.id === this.selectedId())) {
          this.selectedId.set(null);
        }
      },
      error: err => {
        const msg =
          err?.error?.error ??
          err?.error?.message ??
          err?.message ??
          this.t('pur.sup.loadFailed');
        this.error.set(msg);
        this.rows.set([]);
        this.loading.set(false);
      }
    });
  }

  select(row: SupplierListItem): void {
    this.selectedId.set(row.id === this.selectedId() ? null : row.id);
  }

  openNew(): void {
    if (!this.canCreate()) return;
    void this.router.navigate(['/purchases/suppliers/new']);
  }

  openEdit(): void {
    const id = this.selectedId();
    if (!id || !this.canEdit()) return;
    void this.router.navigate(['/purchases/suppliers', id]);
  }

  deleteSelected(): void {
    const row = this.selected();
    if (!row || !this.canDelete()) return;
    if (!confirm(this.t('pur.sup.confirmDelete'))) return;
    this.repo.delete(row.id).subscribe({
      next: () => {
        this.selectedId.set(null);
        this.refresh();
      },
      error: err => {
        this.error.set(err?.error?.error ?? err?.error?.message ?? this.t('pur.sup.deleteFailed'));
      }
    });
  }
}
