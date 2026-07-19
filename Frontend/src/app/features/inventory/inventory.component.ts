import {
  Component,
  ChangeDetectionStrategy,
  inject,
  signal,
  computed,
  OnInit,
  HostListener
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../core/services/language.service';
import { InventoryService } from '../../core/services/inventory.service';
import { InventoryItemDefinition } from '../../core/models/inventory.models';

interface InventoryListRow {
  id: string;
  code: string;
  name: string;
  barcode: string;
  category: string;
  unit: string;
  isActive: boolean;
}

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, MatIconModule],
  templateUrl: './inventory.component.html',
  styleUrl: './inventory.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryComponent implements OnInit {
  langService = inject(LanguageService);
  inventoryService = inject(InventoryService);
  private router = inject(Router);

  searchQuery = signal('');
  categoryFilter = signal('');
  showDeleted = signal(false);
  pageSize = signal(50);
  selectedId = signal<string | null>(null);

  tableRows = computed(() => {
    this.langService.language();
    return this.inventoryService.items().map(item => this.toRow(item));
  });

  filteredItems = computed(() => {
    const q = this.searchQuery().toLowerCase().trim();
    const cat = this.categoryFilter();
    return this.tableRows().filter(row => {
      if (cat && row.category !== cat) return false;
      if (!this.showDeleted() && !row.isActive) return false;
      if (!q) return true;
      return (
        row.name.toLowerCase().includes(q) ||
        row.code.toLowerCase().includes(q) ||
        row.barcode.toLowerCase().includes(q) ||
        row.category.toLowerCase().includes(q)
      );
    });
  });

  pagedItems = computed(() => this.filteredItems().slice(0, this.pageSize()));

  categoryFilterNames = computed(() => {
    const names = new Set(this.tableRows().map(r => r.category).filter(Boolean));
    return Array.from(names).sort((a, b) => a.localeCompare(b, 'ar'));
  });

  ngOnInit(): void {
    this.inventoryService.loadMasterData();
    this.inventoryService.loadItems();
  }

  @HostListener('document:keydown', ['$event'])
  onKeydown(event: KeyboardEvent): void {
    const tag = (event.target as HTMLElement)?.tagName?.toLowerCase();
    const typing = tag === 'input' || tag === 'textarea' || tag === 'select';
    if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'n' && !typing) {
      event.preventDefault();
      this.openNew();
    }
  }

  openNew(): void {
    void this.router.navigate(['/inventory/items/new']);
  }

  selectRow(id: string): void {
    this.selectedId.set(id === this.selectedId() ? null : id);
  }

  editSelected(): void {
    const id = this.selectedId();
    if (!id) return;
    void this.router.navigate(['/inventory/items', id]);
  }

  refresh(): void {
    this.inventoryService.loadItems(this.searchQuery() || undefined);
  }

  onSearch(): void {
    this.inventoryService.loadItems(this.searchQuery() || undefined);
  }

  goCategories(): void {
    void this.router.navigate(['/inventory/categories']);
  }

  goPricing(): void {
    void this.router.navigate(['/inventory/prices']);
  }

  private toRow(item: InventoryItemDefinition): InventoryListRow {
    const name = this.langService.language() === 'ar'
      ? item.nameAr
      : (item.nameEn || item.nameAr);
    return {
      id: item.id,
      code: item.sku || '—',
      name,
      barcode: item.barcode || '—',
      category: item.categoryNameAr,
      unit: item.baseUnitNameAr,
      isActive: item.isActive
    };
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
