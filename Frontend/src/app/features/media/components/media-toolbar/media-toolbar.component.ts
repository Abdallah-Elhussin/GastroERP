import { Component, ChangeDetectionStrategy, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../../../core/services/language.service';

@Component({
  selector: 'app-media-toolbar',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  template: `
    <div class="bg-[var(--bg-surface)] border border-[var(--border-color)] rounded-2xl p-4 flex flex-col md:flex-row gap-4 items-center justify-between shadow-sm select-none">
      
      <!-- Left Side: Search input -->
      <div class="w-full md:w-80 flex items-center bg-[var(--bg-canvas)] border border-[var(--border-color)] rounded-xl px-3 py-2 gap-2">
        <mat-icon class="text-base text-[var(--text-muted)]">search</mat-icon>
        <input 
          type="text" 
          [value]="searchQuery"
          (input)="onSearchChange($event)"
          [placeholder]="t('media.searchPlaceholder')"
          class="bg-transparent border-none text-[var(--text-primary)] text-xs focus:outline-none placeholder-[var(--text-muted)] w-full"
        />
      </div>

      <!-- Center: Type Filters -->
      <div class="flex gap-2 items-center overflow-x-auto w-full md:w-auto">
        <button 
          *ngFor="let filter of filtersList"
          (click)="onFilterChange(filter.value)"
          [class.bg-[var(--primary-color)]]="activeFilter === filter.value"
          [class.text-[var(--primary-contrast)]]="activeFilter === filter.value"
          [class.bg-[var(--bg-canvas)]]="activeFilter !== filter.value"
          [class.text-[var(--text-secondary)]]="activeFilter !== filter.value"
          [class.border]="activeFilter !== filter.value"
          [class.border-[var(--border-color)]]="activeFilter !== filter.value"
          class="px-4 py-1.5 rounded-xl text-xs font-semibold hover:opacity-90 transition-all cursor-pointer whitespace-nowrap"
        >
          {{ t(filter.labelKey) }}
        </button>
      </div>

      <!-- Right Side: Bulk Operations -->
      <div class="flex items-center gap-2 justify-end w-full md:w-auto">
        <ng-container *ngIf="bulkSelectedIds.length > 0">
          <button 
            type="button"
            (click)="onBulkCompress()"
            class="bg-[var(--bg-canvas)] text-[var(--text-primary)] border border-[var(--border-color)] hover:bg-[var(--bg-surface-hover)] px-3 py-1.5 rounded-xl text-[10px] font-bold shadow-sm cursor-pointer flex items-center gap-1"
            [title]="t('media.details.convert')"
          >
            <mat-icon class="text-sm">compress</mat-icon>
            <span>{{ t('media.convertWebp') }} ({{ bulkSelectedIds.length }})</span>
          </button>
          
          <button 
            type="button"
            (click)="onBulkDelete()"
            class="bg-red-500 hover:bg-red-600 text-white px-3 py-1.5 rounded-xl text-[10px] font-bold shadow-sm cursor-pointer flex items-center gap-1"
            [title]="t('media.details.delete')"
          >
            <mat-icon class="text-sm">delete</mat-icon>
            <span>{{ t('common.delete') }} ({{ bulkSelectedIds.length }})</span>
          </button>
        </ng-container>

        <!-- Density grid switcher -->
        <div class="flex border border-[var(--border-color)] rounded-xl overflow-hidden bg-[var(--bg-canvas)]">
          <button 
            (click)="onToggleView('grid')"
            [class.bg-[var(--bg-surface)]]="viewMode === 'grid'"
            class="p-1.5 hover:bg-[var(--bg-surface-hover)] cursor-pointer text-[var(--text-secondary)] flex items-center"
          >
            <mat-icon class="text-sm">grid_view</mat-icon>
          </button>
          <button 
            (click)="onToggleView('list')"
            [class.bg-[var(--bg-surface)]]="viewMode === 'list'"
            class="p-1.5 hover:bg-[var(--bg-surface-hover)] cursor-pointer text-[var(--text-secondary)] flex items-center"
          >
            <mat-icon class="text-sm">view_list</mat-icon>
          </button>
        </div>
      </div>

    </div>
  `,
  styles: [`
    :host { display: block; width: 100%; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MediaToolbarComponent {
  langService = inject(LanguageService);

  @Input() searchQuery = '';
  @Input() activeFilter = 'all';
  @Input() bulkSelectedIds: string[] = [];
  @Input() viewMode: 'grid' | 'list' = 'grid';

  @Output() search = new EventEmitter<string>();
  @Output() filter = new EventEmitter<string>();
  @Output() bulkCompress = new EventEmitter<void>();
  @Output() bulkDelete = new EventEmitter<void>();
  @Output() toggleView = new EventEmitter<'grid' | 'list'>();

  filtersList = [
    { labelKey: 'media.filter.all', value: 'all' },
    { labelKey: 'media.filter.images', value: 'image' },
    { labelKey: 'media.filter.svg', value: 'svg' },
    { labelKey: 'media.filter.docs', value: 'document' }
  ];

  onSearchChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.search.emit(input.value);
  }

  onFilterChange(value: string): void {
    this.filter.emit(value);
  }

  onBulkCompress(): void {
    this.bulkCompress.emit();
  }

  onBulkDelete(): void {
    if (confirm(`${this.t('common.delete')} (${this.bulkSelectedIds.length})?`)) {
      this.bulkDelete.emit();
    }
  }

  onToggleView(mode: 'grid' | 'list'): void {
    this.toggleView.emit(mode);
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
