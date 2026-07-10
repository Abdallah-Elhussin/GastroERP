import { Component, ChangeDetectionStrategy, Input, ContentChild, TemplateRef, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonModule } from '@angular/material/button';
import { LanguageService } from '../../../core/services/language.service';

export interface AppTableColumn {
  key: string;
  label: string;
  width?: string;
  sortable?: boolean;
}

@Component({
  selector: 'app-table',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatMenuModule,
    MatButtonModule
  ],
  templateUrl: './app-table.component.html',
  styleUrl: './app-table.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppTableComponent implements OnInit {
  private langService = inject(LanguageService);

  @Input() columns: AppTableColumn[] = [];
  @Input() data: any[] = [];
  @Input() pagination = true;
  @Input() defaultPageSize = 5;

  // Content Template to allow row customization
  @ContentChild('rowTemplate', { static: false }) rowTemplate?: TemplateRef<any>;

  // Advanced states
  density = signal<'compact' | 'standard' | 'relaxed'>('standard');
  pageSize = signal<number>(5);
  pageIndex = signal<number>(0);
  
  sortKey = signal<string>('');
  sortDir = signal<'asc' | 'desc' | ''>('');
  
  visibleColumns = signal<string[]>([]);

  // Computed columns configuration
  filteredColumns = computed(() => {
    const visible = this.visibleColumns();
    return this.columns.filter(col => visible.includes(col.key));
  });

  // Filtered, sorted and paginated dataset
  processedData = computed(() => {
    let result = [...this.data];

    // Sorting
    const key = this.sortKey();
    const dir = this.sortDir();
    if (key && dir) {
      result.sort((a, b) => {
        const valA = a[key];
        const valB = b[key];
        if (valA === undefined || valA === null) return 1;
        if (valB === undefined || valB === null) return -1;
        if (valA < valB) return dir === 'asc' ? -1 : 1;
        if (valA > valB) return dir === 'asc' ? 1 : -1;
        return 0;
      });
    }

    // Pagination
    if (this.pagination) {
      const idx = this.pageIndex();
      const size = this.pageSize();
      const start = idx * size;
      return result.slice(start, start + size);
    }

    return result;
  });

  totalPages = computed(() => {
    return Math.ceil(this.data.length / this.pageSize());
  });

  showingStart = computed(() => {
    if (this.data.length === 0) return 0;
    return this.pageIndex() * this.pageSize() + 1;
  });

  showingEnd = computed(() => {
    return Math.min((this.pageIndex() + 1) * this.pageSize(), this.data.length);
  });

  densityLabel = computed(() => {
    this.langService.language();
    const d = this.density();
    const key = d === 'compact' ? 'table.compact' : d === 'standard' ? 'table.standard' : 'table.relaxed';
    return this.t(key);
  });

  ngOnInit(): void {
    this.pageSize.set(this.defaultPageSize);
    // Initially display all columns
    this.visibleColumns.set(this.columns.map(c => c.key));
  }

  toggleSort(column: AppTableColumn): void {
    if (!column.sortable) return;
    const currentKey = this.sortKey();
    const currentDir = this.sortDir();

    if (currentKey === column.key) {
      if (currentDir === 'asc') {
        this.sortDir.set('desc');
      } else if (currentDir === 'desc') {
        this.sortKey.set('');
        this.sortDir.set('');
      } else {
        this.sortDir.set('asc');
      }
    } else {
      this.sortKey.set(column.key);
      this.sortDir.set('asc');
    }
    this.pageIndex.set(0); // reset page on sort change
  }

  toggleColumn(key: string): void {
    this.visibleColumns.update(list => {
      if (list.includes(key)) {
        // Keep at least 1 column visible
        if (list.length <= 1) return list;
        return list.filter(item => item !== key);
      } else {
        return [...list, key];
      }
    });
  }

  prevPage(): void {
    if (this.pageIndex() > 0) {
      this.pageIndex.update(idx => idx - 1);
    }
  }

  nextPage(): void {
    if (this.pageIndex() < this.totalPages() - 1) {
      this.pageIndex.update(idx => idx + 1);
    }
  }

  changePageSize(size: number): void {
    this.pageSize.set(size);
    this.pageIndex.set(0);
  }

  exportCSV(): void {
    if (this.data.length === 0) return;
    const visibleCols = this.filteredColumns();
    
    // Header row
    const headers = visibleCols.map(col => `"${col.label}"`).join(',');
    
    // Data rows
    const rows = this.data.map(row => {
      return visibleCols.map(col => {
        const val = row[col.key] !== undefined ? row[col.key] : '';
        return `"${val.toString().replace(/"/g, '""')}"`;
      }).join(',');
    });

    const csvContent = 'data:text/csv;charset=utf-8,' + [headers, ...rows].join('\n');
    const encodedUri = encodeURI(csvContent);
    const link = document.createElement('a');
    link.setAttribute('href', encodedUri);
    link.setAttribute('download', `table-export-${Date.now()}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  printTable(): void {
    window.print();
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
