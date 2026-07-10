import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MediaRepository, MediaFile } from '../../../core/repositories/media.repository';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-media-picker',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatIconModule,
    MatButtonModule
  ],
  template: `
    <div class="p-6 flex flex-col gap-4 max-w-2xl w-full text-left">
      <div class="flex justify-between items-center select-none">
        <h3 class="text-base font-bold text-[var(--text-primary)]">{{ t('media.picker.title') }}</h3>
        <button mat-icon-button (click)="onClose()" class="w-8 h-8 rounded-lg hover:bg-[var(--bg-canvas)]">
          <mat-icon class="text-sm">close</mat-icon>
        </button>
      </div>

      <!-- Assets grid -->
      <div class="grid grid-cols-3 sm:grid-cols-4 gap-3 max-h-80 overflow-y-auto p-1">
        <div 
          *ngFor="let file of files()"
          [class.ring-2]="selectedFile?.id === file.id"
          [class.ring-[var(--primary-color)]]="selectedFile?.id === file.id"
          (click)="onSelect(file)"
          class="bg-[var(--bg-canvas)] border border-[var(--border-color)] rounded-xl overflow-hidden cursor-pointer hover:shadow transition-all relative aspect-square flex flex-col items-center justify-center p-1"
        >
          <img 
            *ngIf="file.type === 'image' || file.type === 'svg'" 
            [src]="file.url" 
            class="h-full w-full object-cover rounded-lg" 
          />
          <div *ngIf="file.type !== 'image' && file.type !== 'svg'" class="flex flex-col items-center text-[var(--text-muted)]">
            <mat-icon class="text-2xl">description</mat-icon>
            <span class="text-[8px] font-bold uppercase mt-1">{{ file.type }}</span>
          </div>
          
          <!-- Selected check icon -->
          <span 
            *ngIf="selectedFile?.id === file.id" 
            class="absolute top-1 right-1 w-4 h-4 rounded-full bg-[var(--primary-color)] text-[var(--primary-contrast)] flex items-center justify-center text-[9px]"
          >
            ✓
          </span>
        </div>

        <div *ngIf="files().length === 0" class="col-span-full py-10 text-center text-xs text-[var(--text-muted)] select-none">
          {{ t('media.picker.empty') }}
        </div>
      </div>

      <!-- Actions footer -->
      <div class="flex justify-end gap-2 mt-4 select-none">
        <button 
          type="button" 
          (click)="onClose()" 
          class="bg-[var(--bg-canvas)] text-[var(--text-primary)] border border-[var(--border-color)] hover:bg-[var(--bg-surface-hover)] px-4 py-2 rounded-xl text-xs font-bold shadow-sm cursor-pointer"
        >
          {{ t('common.cancel') }}
        </button>
        <button 
          type="button" 
          (click)="onConfirm()" 
          [disabled]="!selectedFile"
          class="bg-[var(--primary-color)] text-[var(--primary-contrast)] hover:bg-[var(--primary-color-hover)] px-5 py-2 rounded-xl text-xs font-bold shadow-md cursor-pointer"
        >
          {{ t('media.picker.select') }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MediaPickerComponent implements OnInit {
  dialogRef = inject(MatDialogRef<MediaPickerComponent>);
  mediaRepository = inject(MediaRepository);
  langService = inject(LanguageService);

  files = signal<MediaFile[]>([]);
  selectedFile: MediaFile | null = null;

  ngOnInit(): void {
    this.mediaRepository.getFiles().subscribe(list => {
      this.files.set(list);
    });
  }

  onSelect(file: MediaFile): void {
    this.selectedFile = file;
  }

  onClose(): void {
    this.dialogRef.close();
  }

  onConfirm(): void {
    if (this.selectedFile) {
      this.dialogRef.close(this.selectedFile);
    }
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
