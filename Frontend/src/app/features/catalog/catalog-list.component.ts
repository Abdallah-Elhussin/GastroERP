import { Component, ChangeDetectionStrategy, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../core/services/language.service';
import { CatalogService } from '../../core/services/catalog.service';
import { CatalogImportRow } from '../../core/models/catalog.models';

@Component({
  selector: 'app-catalog-list',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule],
  templateUrl: './catalog-list.component.html',
  styleUrl: './catalog-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CatalogListComponent implements OnInit {
  langService = inject(LanguageService);
  catalogService = inject(CatalogService);
  search = signal('');
  importMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.catalogService.loadTypes();
    this.catalogService.loadDefinitions();
  }

  onSearch(value: string): void {
    this.search.set(value);
    this.catalogService.loadDefinitions(value);
  }

  exportCsv(): void {
    this.catalogService.exportCsv(this.search()).subscribe({
      next: blob => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `catalog-export-${Date.now()}.csv`;
        a.click();
        URL.revokeObjectURL(url);
      }
    });
  }

  onImportFile(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => {
      const text = String(reader.result ?? '');
      const rows = this.parseCsv(text);
      if (!rows.length) {
        this.importMessage.set(this.t('catalog.importEmpty'));
        return;
      }
      this.catalogService.importRows(rows).subscribe({
        next: count => {
          this.importMessage.set(this.t('catalog.importSuccess').replace('{count}', String(count)));
          this.catalogService.loadDefinitions(this.search());
        },
        error: () => this.importMessage.set(this.t('catalog.importFailed'))
      });
    };
    reader.readAsText(file);
    input.value = '';
  }

  private parseCsv(text: string): CatalogImportRow[] {
    const lines = text.split(/\r?\n/).filter(l => l.trim());
    if (lines.length < 2) return [];
    return lines.slice(1).map(line => {
      const cols = line.split(',').map(c => c.trim().replace(/^"|"$/g, ''));
      return {
        catalogType: Number(cols[1]) || 4,
        nameAr: cols[2] ?? '',
        nameEn: cols[3] || undefined,
        sku: cols[4] || undefined,
        barcode: cols[5] || undefined,
        basePrice: Number(cols[6]) || 0
      } satisfies CatalogImportRow;
    }).filter(r => r.nameAr);
  }

  typeLabel(type: number): string {
    const meta = this.catalogService.types().find(t => t.type === type);
    if (!meta) return String(type);
    return this.langService.language() === 'ar' ? meta.nameAr : meta.nameEn;
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
