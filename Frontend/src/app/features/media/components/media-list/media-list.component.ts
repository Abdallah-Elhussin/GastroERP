import { Component, ChangeDetectionStrategy, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MediaFile } from '../../../../core/repositories/media.repository';

@Component({
  selector: 'app-media-list',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatCheckboxModule],
  template: `
    <div class="bg-[var(--bg-surface)] border border-[var(--border-color)] rounded-2xl overflow-hidden shadow-sm">
      <table class="w-full text-left border-collapse">
        <thead>
          <tr class="bg-[var(--bg-canvas)] border-b border-[var(--border-color)] text-[10px] font-bold text-[var(--text-muted)] uppercase tracking-wider select-none">
            <th class="p-3 w-10">Select</th>
            <th class="p-3">Asset</th>
            <th class="p-3">Type</th>
            <th class="p-3">Size</th>
            <th class="p-3">Created Date</th>
            <th class="p-3 text-right">Tags</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-[var(--border-color-muted)]">
          <tr 
            *ngFor="let file of files"
            (click)="onSelectFile(file)"
            [class.bg-[var(--bg-canvas)]]="selectedFileId === file.id"
            [class.bg-opacity-40]="selectedFileId === file.id"
            class="hover:bg-[var(--bg-surface-hover)] cursor-pointer text-xs transition-colors"
          >
            <td class="p-3" (click)="$event.stopPropagation()">
              <mat-checkbox 
                [checked]="isBulkSelected(file.id)"
                (change)="onToggleBulk(file.id)"
                color="primary"
              ></mat-checkbox>
            </td>
            
            <td class="p-3 font-semibold text-[var(--text-primary)]">
              <div class="flex items-center gap-3">
                <span class="w-8 h-8 rounded-lg bg-[var(--bg-canvas)] border border-[var(--border-color)] overflow-hidden flex items-center justify-center p-0.5">
                  <img 
                    *ngIf="file.type === 'image' || file.type === 'svg'" 
                    [src]="file.url" 
                    class="h-full w-full object-cover rounded" 
                  />
                  <mat-icon *ngIf="file.type !== 'image' && file.type !== 'svg'" class="text-sm text-[var(--text-muted)]">description</mat-icon>
                </span>
                <span class="truncate max-w-[200px]">{{ file.name }}</span>
              </div>
            </td>

            <td class="p-3 uppercase text-[var(--text-secondary)] font-bold text-[10px]">{{ file.type }}</td>
            <td class="p-3 text-[var(--text-secondary)] font-medium">{{ file.size }}</td>
            <td class="p-3 text-[var(--text-muted)]">{{ file.createdDate }}</td>
            
            <td class="p-3 text-right">
              <div class="flex justify-end gap-1 select-none">
                <span 
                  *ngFor="let tag of file.tags" 
                  class="bg-[var(--bg-canvas)] border border-[var(--border-color)] text-[var(--text-secondary)] px-1.5 py-0.5 rounded text-[8px] font-bold"
                >
                  {{ tag }}
                </span>
              </div>
            </td>
          </tr>

          <tr *ngIf="files.length === 0">
            <td colspan="6" class="p-10 text-center text-xs text-[var(--text-muted)] font-normal select-none">
              No files in this folder matching filters.
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  `,
  styles: [`
    :host { display: block; width: 100%; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MediaListComponent {
  @Input() files: MediaFile[] = [];
  @Input() selectedFileId: string | null = null;
  @Input() bulkSelectedIds: string[] = [];

  @Output() selectFile = new EventEmitter<MediaFile>();
  @Output() toggleBulkSelect = new EventEmitter<string>();

  onSelectFile(file: MediaFile): void {
    this.selectFile.emit(file);
  }

  isBulkSelected(fileId: string): boolean {
    return this.bulkSelectedIds.includes(fileId);
  }

  onToggleBulk(fileId: string): void {
    this.toggleBulkSelect.emit(fileId);
  }
}
