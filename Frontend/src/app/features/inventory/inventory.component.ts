import { Component, ChangeDetectionStrategy, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../core/services/language.service';
import { InventoryService } from '../../core/services/inventory.service';
import { AppTableComponent } from '../../shared/ui/app-table/app-table.component';
import { InventoryItemDefinition } from '../../core/models/inventory.models';

interface InventoryListRow {
  id: string;
  name: string;
  category: string;
  sku: string;
  kind: string;
  unit: string;
  unitPrice: number;
  totalValue: number;
  status: 'in_stock' | 'low_stock' | 'out_of_stock';
  imageUrl?: string;
}

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatButtonModule,
    MatIconModule,
    AppTableComponent
  ],
  templateUrl: './inventory.component.html',
  styleUrl: './inventory.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryComponent implements OnInit {
  langService = inject(LanguageService);
  inventoryService = inject(InventoryService);

  searchQuery = signal<string>('');

  columns = computed(() => {
    this.langService.language();
    return [
      { key: 'name', label: this.t('inventory.col.item'), sortable: true },
      { key: 'category', label: this.t('inventory.col.category'), sortable: true },
      { key: 'sku', label: this.t('inventory.col.sku'), sortable: true },
      { key: 'kind', label: this.t('inventory.col.kind'), sortable: true },
      { key: 'unit', label: this.t('inventory.col.stock'), sortable: false },
      { key: 'unitPrice', label: this.t('inventory.col.price'), sortable: true },
      { key: 'status', label: this.t('inventory.col.status'), sortable: true },
      { key: 'actions', label: this.t('inventory.col.actions'), sortable: false }
    ];
  });

  tableRows = computed(() => {
    this.langService.language();
    return this.inventoryService.items().map(item => this.toRow(item));
  });

  filteredItems = computed(() => {
    const query = this.searchQuery().toLowerCase();
    return this.tableRows().filter(item =>
      item.name.toLowerCase().includes(query) ||
      item.category.toLowerCase().includes(query) ||
      item.sku.toLowerCase().includes(query)
    );
  });

  totalValue = computed(() =>
    this.tableRows().reduce((sum, row) => sum + row.totalValue, 0)
  );

  lowStockCount = computed(() =>
    this.tableRows().filter(row => row.status === 'low_stock').length
  );

  ngOnInit(): void {
    this.inventoryService.loadItems();
  }

  onSearch(value: string): void {
    this.searchQuery.set(value);
    this.inventoryService.loadItems(value);
  }

  exportCSV(): void {
    alert(this.t('inventory.exporting'));
  }

  private toRow(item: InventoryItemDefinition): InventoryListRow {
    const name = this.langService.language() === 'ar'
      ? item.nameAr
      : (item.nameEn || item.nameAr);
    const unitPrice = item.averageUnitCost ?? item.lastPurchaseUnitCost ?? 0;
    const status = item.reorderLevel > 0 && unitPrice === 0
      ? 'low_stock'
      : 'in_stock';

    return {
      id: item.id,
      name,
      category: item.categoryNameAr,
      sku: [item.sku, item.barcode].filter(Boolean).join(' · ') || '—',
      kind: item.itemKind === 'manufactured'
        ? this.t('inventory.kind.manufactured')
        : this.t('inventory.kind.raw'),
      unit: item.baseUnitNameAr,
      unitPrice,
      totalValue: unitPrice,
      status,
      imageUrl: item.imageUrl
    };
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
