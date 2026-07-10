import { Component, ChangeDetectionStrategy, Output, EventEmitter, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../../../core/services/language.service';

@Component({
  selector: 'app-media-upload',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <div 
      class="border-2 border-dashed border-[var(--border-color)] hover:border-[var(--primary-color)] rounded-2xl p-6 text-center transition-colors cursor-pointer flex flex-col items-center justify-center gap-2 bg-[var(--bg-canvas)] bg-opacity-40"
      (click)="fileInput.click()"
      (dragover)="onDragOver($event)"
      (dragleave)="onDragLeave($event)"
      (drop)="onDrop($event)"
      [class.border-[var(--primary-color)]]="isDragOver()"
      [class.bg-[var(--bg-surface-hover)]]="isDragOver()"
    >
      <input 
        type="file" 
        multiple 
        #fileInput 
        (change)="onFileSelected($event)" 
        class="hidden" 
      />

      <mat-icon class="text-3xl text-[var(--text-muted)] animate-bounce">cloud_upload</mat-icon>
      <div class="flex flex-col select-none">
        <span class="text-xs font-bold text-[var(--text-primary)]">{{ t('media.upload.drag') }}</span>
        <span class="text-[10px] text-[var(--text-muted)] mt-0.5">{{ t('media.upload.browse') }}</span>
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MediaUploadComponent {
  langService = inject(LanguageService);

  @Output() filesUploaded = new EventEmitter<File[]>();

  isDragOver = signal<boolean>(false);

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver.set(false);
    
    if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
      const files: File[] = [];
      for (let i = 0; i < event.dataTransfer.files.length; i++) {
        files.push(event.dataTransfer.files[i]);
      }
      this.filesUploaded.emit(files);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const files: File[] = [];
      for (let i = 0; i < input.files.length; i++) {
        files.push(input.files[i]);
      }
      this.filesUploaded.emit(files);
      input.value = '';
    }
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
