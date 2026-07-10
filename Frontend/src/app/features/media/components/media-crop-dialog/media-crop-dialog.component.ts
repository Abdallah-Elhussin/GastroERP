import { Component, ChangeDetectionStrategy, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MediaFile } from '../../../../core/repositories/media.repository';
import { LanguageService } from '../../../../core/services/language.service';

@Component({
  selector: 'app-media-crop-dialog',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  template: `
    <div class="fixed inset-0 bg-black bg-opacity-50 backdrop-blur-sm z-[999] flex items-center justify-center p-4">
      <div class="bg-[var(--bg-surface)] border border-[var(--border-color)] max-w-lg w-full rounded-2xl shadow-2xl p-6 flex flex-col gap-5 text-left">
        
        <!-- Header -->
        <div class="flex justify-between items-center select-none">
          <span class="text-sm font-bold text-[var(--text-primary)]">{{ t('media.crop.title') }}</span>
          <button mat-icon-button (click)="onClose()" class="w-8 h-8 rounded-lg hover:bg-[var(--bg-canvas)]">
            <mat-icon class="text-sm">close</mat-icon>
          </button>
        </div>

        <!-- Crop Preview with mock overlay border -->
        <div class="relative bg-slate-950 border border-[var(--border-color)] rounded-xl aspect-[4/3] overflow-hidden flex items-center justify-center p-4">
          <img [src]="file.url" [alt]="file.name" class="max-h-full max-w-full object-contain opacity-50 z-0" />
          
          <!-- Mock Crop Frame -->
          <div class="absolute w-32 h-32 border-2 border-dashed border-[var(--primary-color)] flex items-center justify-center z-10 shadow-[0_0_0_9999px_rgba(0,0,0,0.6)] cursor-move">
            <span class="text-[9px] text-white font-extrabold uppercase bg-[var(--primary-color)] px-1 rounded-sm select-none">{{ t('media.crop.ratio') }}</span>
            <!-- Handles -->
            <span class="absolute -top-1 -left-1 w-2.5 h-2.5 bg-[var(--primary-color)]"></span>
            <span class="absolute -top-1 -right-1 w-2.5 h-2.5 bg-[var(--primary-color)]"></span>
            <span class="absolute -bottom-1 -left-1 w-2.5 h-2.5 bg-[var(--primary-color)]"></span>
            <span class="absolute -bottom-1 -right-1 w-2.5 h-2.5 bg-[var(--primary-color)]"></span>
          </div>
        </div>

        <!-- Guide details -->
        <p class="text-[10px] text-[var(--text-muted)] select-none">{{ t('media.crop.guide') }}</p>

        <!-- Actions -->
        <div class="flex justify-end gap-2 select-none">
          <button 
            type="button" 
            (click)="onClose()" 
            class="bg-[var(--bg-canvas)] text-[var(--text-primary)] border border-[var(--border-color)] hover:bg-[var(--bg-surface-hover)] px-4 py-2 rounded-xl text-xs font-bold shadow-sm cursor-pointer"
          >
            {{ t('common.cancel') }}
          </button>
          <button 
            type="button" 
            (click)="onSave()" 
            class="bg-[var(--primary-color)] text-[var(--primary-contrast)] hover:bg-[var(--primary-color-hover)] px-5 py-2 rounded-xl text-xs font-bold shadow-md cursor-pointer flex items-center gap-1"
          >
            <mat-icon class="text-sm">crop</mat-icon>
            <span>{{ t('media.crop.apply') }}</span>
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MediaCropDialogComponent {
  langService = inject(LanguageService);

  @Input() file!: MediaFile;

  @Output() close = new EventEmitter<void>();
  @Output() cropApplied = new EventEmitter<string>();

  onClose(): void {
    this.close.emit();
  }

  onSave(): void {
    this.cropApplied.emit(this.file.id);
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
