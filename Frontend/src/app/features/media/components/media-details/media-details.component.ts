import { Component, ChangeDetectionStrategy, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MediaFile } from '../../../../core/repositories/media.repository';
import { LanguageService } from '../../../../core/services/language.service';

@Component({
  selector: 'app-media-details',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  template: `
    <div class="bg-[var(--bg-surface)] border border-[var(--border-color)] rounded-2xl p-5 flex flex-col gap-5 text-left h-full">
      <span class="text-[10px] font-bold text-[var(--text-muted)] uppercase tracking-wider">{{ t('media.details.title') }}</span>

      <ng-container *ngIf="file; else noSelection">
        <!-- Visual thumbnail -->
        <div class="h-40 rounded-xl bg-[var(--bg-canvas)] border border-[var(--border-color)] overflow-hidden flex items-center justify-center p-2">
          <img 
            *ngIf="file.type === 'image' || file.type === 'svg'"
            [src]="file.url" 
            [alt]="file.name" 
            class="max-h-full max-w-full object-contain rounded-lg shadow-sm"
          />
          <mat-icon *ngIf="file.type !== 'image' && file.type !== 'svg'" class="text-4xl text-[var(--text-muted)]">description</mat-icon>
        </div>

        <!-- File Properties table -->
        <div class="flex flex-col gap-2">
          <div class="flex justify-between border-b border-[var(--border-color-muted)] pb-1.5 text-xs">
            <span class="text-[var(--text-secondary)] font-medium">{{ t('media.details.name') }}</span>
            <span class="text-[var(--text-primary)] font-bold truncate max-w-[150px]" [title]="file.name">{{ file.name }}</span>
          </div>
          <div class="flex justify-between border-b border-[var(--border-color-muted)] pb-1.5 text-xs">
            <span class="text-[var(--text-secondary)] font-medium">{{ t('media.details.type') }}</span>
            <span class="text-[var(--text-primary)] font-bold uppercase">{{ file.type }}</span>
          </div>
          <div class="flex justify-between border-b border-[var(--border-color-muted)] pb-1.5 text-xs">
            <span class="text-[var(--text-secondary)] font-medium">{{ t('media.details.size') }}</span>
            <span class="text-[var(--text-primary)] font-bold">{{ file.size }}</span>
          </div>
          <div class="flex justify-between border-b border-[var(--border-color-muted)] pb-1.5 text-xs">
            <span class="text-[var(--text-secondary)] font-medium">{{ t('media.details.created') }}</span>
            <span class="text-[var(--text-primary)] font-bold">{{ file.createdDate }}</span>
          </div>
        </div>

        <!-- Tags badge list -->
        <div class="flex flex-col gap-1.5">
          <span class="text-[10px] text-[var(--text-muted)] font-bold uppercase tracking-wider">{{ t('media.details.tags') }}</span>
          <div class="flex flex-wrap gap-1 select-none">
            <span 
              *ngFor="let tag of file.tags" 
              class="bg-[var(--bg-canvas)] border border-[var(--border-color)] text-[var(--text-secondary)] px-2 py-0.5 rounded text-[9px] font-bold"
            >
              {{ tag }}
            </span>
          </div>
        </div>

        <!-- Image Editor Actions -->
        <div class="flex flex-col gap-2 select-none border-t border-[var(--border-color-muted)] pt-4 mt-auto">
          <button 
            type="button"
            (click)="onRotate()"
            class="w-full bg-[var(--bg-canvas)] text-[var(--text-primary)] border border-[var(--border-color)] hover:bg-[var(--bg-surface-hover)] py-2 rounded-xl text-[10px] font-bold shadow-sm cursor-pointer flex items-center justify-center gap-1.5"
          >
            <mat-icon class="text-sm">rotate_right</mat-icon>
            <span>{{ t('media.details.rotate') }}</span>
          </button>
          
          <button 
            type="button"
            (click)="onCompress()"
            class="w-full bg-[var(--bg-canvas)] text-[var(--text-primary)] border border-[var(--border-color)] hover:bg-[var(--bg-surface-hover)] py-2 rounded-xl text-[10px] font-bold shadow-sm cursor-pointer flex items-center justify-center gap-1.5"
            [disabled]="file.tags.includes('webp')"
          >
            <mat-icon class="text-sm">compress</mat-icon>
            <span>{{ t('media.details.convert') }}</span>
          </button>

          <button 
            type="button"
            (click)="onCrop()"
            class="w-full bg-[var(--bg-canvas)] text-[var(--text-primary)] border border-[var(--border-color)] hover:bg-[var(--bg-surface-hover)] py-2 rounded-xl text-[10px] font-bold shadow-sm cursor-pointer flex items-center justify-center gap-1.5"
          >
            <mat-icon class="text-sm">crop</mat-icon>
            <span>{{ t('media.details.crop') }}</span>
          </button>

          <button 
            type="button"
            (click)="onDelete()"
            class="w-full bg-red-500 hover:bg-red-600 text-white py-2 rounded-xl text-[10px] font-bold shadow-sm cursor-pointer flex items-center justify-center gap-1.5"
          >
            <mat-icon class="text-sm">delete</mat-icon>
            <span>{{ t('media.details.delete') }}</span>
          </button>
        </div>
      </ng-container>

      <ng-template #noSelection>
        <div class="flex-1 flex flex-col items-center justify-center text-[var(--text-muted)] text-xs gap-1 select-none py-10">
          <mat-icon class="text-2xl">info</mat-icon>
          <span>{{ t('media.details.select') }}</span>
        </div>
      </ng-template>
    </div>
  `,
  styles: [`
    :host { display: block; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MediaDetailsComponent {
  langService = inject(LanguageService);

  @Input() file: MediaFile | null = null;

  @Output() rotate = new EventEmitter<string>();
  @Output() compress = new EventEmitter<string>();
  @Output() crop = new EventEmitter<string>();
  @Output() delete = new EventEmitter<string>();

  onRotate(): void {
    if (this.file) this.rotate.emit(this.file.id);
  }

  onCompress(): void {
    if (this.file) this.compress.emit(this.file.id);
  }

  onCrop(): void {
    if (this.file) this.crop.emit(this.file.id);
  }

  onDelete(): void {
    if (this.file && confirm(`${this.t('common.delete')}: ${this.file.name}?`)) {
      this.delete.emit(this.file.id);
    }
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
