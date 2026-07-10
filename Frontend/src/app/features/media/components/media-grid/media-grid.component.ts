import { Component, ChangeDetectionStrategy, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MediaFile } from '../../../../core/repositories/media.repository';

@Component({
  selector: 'app-media-grid',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatCheckboxModule],
  template: `
    <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4 p-2">
      <div 
        *ngFor="let file of files"
        [class.ring-2]="selectedFileId === file.id"
        [class.ring-[var(--primary-color)]]="selectedFileId === file.id"
        (click)="onSelectFile(file)"
        class="group relative bg-[var(--bg-surface)] border border-[var(--border-color)] rounded-2xl overflow-hidden hover:shadow-md transition-all cursor-pointer flex flex-col"
      >
        <!-- Checkbox overlay for bulk actions -->
        <div class="absolute top-2 left-2 z-10 opacity-0 group-hover:opacity-100 transition-opacity" (click)="$event.stopPropagation()">
          <mat-checkbox 
            [checked]="isBulkSelected(file.id)"
            (change)="onToggleBulk(file.id)"
            color="primary"
          ></mat-checkbox>
        </div>

        <!-- File Preview aspect ratio 4:3 -->
        <div class="aspect-[4/3] bg-[var(--bg-canvas)] relative overflow-hidden flex items-center justify-center border-b border-[var(--border-color)]">
          <img 
            *ngIf="file.type === 'image' || file.type === 'svg'"
            [src]="file.url" 
            [alt]="file.name" 
            class="h-full w-full object-cover"
            loading="lazy"
          />
          <div *ngIf="file.type !== 'image' && file.type !== 'svg'" class="flex flex-col items-center gap-1 text-[var(--text-muted)]">
            <mat-icon class="text-3xl">description</mat-icon>
            <span class="text-[9px] uppercase font-bold">{{ file.type }}</span>
          </div>
        </div>

        <!-- File Details footer -->
        <div class="p-3 flex flex-col text-left leading-tight">
          <span class="text-xs font-bold text-[var(--text-primary)] truncate" [title]="file.name">{{ file.name }}</span>
          <div class="flex justify-between items-center mt-1 select-none">
            <span class="text-[9px] text-[var(--text-muted)] font-semibold">{{ file.size }}</span>
            
            <!-- WebP/Edited Tag indicator -->
            <div class="flex gap-1">
              <span *ngIf="file.tags.includes('webp')" class="bg-emerald-500 text-white font-extrabold text-[7px] px-1 rounded">WEBP</span>
              <span *ngIf="file.tags.includes('processed')" class="bg-blue-500 text-white font-extrabold text-[7px] px-1 rounded">EDITED</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MediaGridComponent {
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
