import { Component, ChangeDetectionStrategy, Input, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-upload',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <div 
      [class.border-[var(--accent-color)]]="isDragOver()"
      [class.bg-[var(--bg-surface-hover)]]="isDragOver()"
      (dragover)="onDragOver($event)"
      (dragleave)="onDragLeave()"
      (drop)="onDrop($event)"
      class="border-2 border-dashed border-[var(--border-color)] rounded-2xl p-8 text-center flex flex-col items-center gap-3 transition-colors cursor-pointer select-none"
      (click)="fileInput.click()"
    >
      <input 
        #fileInput 
        type="file" 
        [accept]="accept" 
        [multiple]="multiple" 
        (change)="onFileSelected($event)" 
        class="hidden" 
      />
      
      <div class="w-12 h-12 rounded-full bg-[var(--bg-canvas)] border border-[var(--border-color)] flex items-center justify-center text-[var(--text-secondary)]">
        <mat-icon class="text-xl">cloud_upload</mat-icon>
      </div>

      <div class="flex flex-col gap-1">
        <span class="text-xs font-bold text-[var(--text-primary)]">Drag and drop file here, or click to browse</span>
        <span class="text-[10px] text-[var(--text-muted)]">{{ hint }}</span>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      width: 100%;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppUploadComponent {
  @Input() accept = '*';
  @Input() multiple = false;
  @Input() hint = 'Maximum file size: 5MB';
  
  @Output() fileUploaded = new EventEmitter<File[]>();

  isDragOver = signal<boolean>(false);

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver.set(true);
  }

  onDragLeave(): void {
    this.isDragOver.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver.set(false);
    
    if (event.dataTransfer?.files) {
      const files: File[] = [];
      for (let i = 0; i < event.dataTransfer.files.length; i++) {
        files.push(event.dataTransfer.files[i]);
      }
      this.fileUploaded.emit(files);
    }
  }

  onFileSelected(event: Event): void {
    const target = event.target as HTMLInputElement;
    if (target.files) {
      const files: File[] = [];
      for (let i = 0; i < target.files.length; i++) {
        files.push(target.files[i]);
      }
      this.fileUploaded.emit(files);
    }
  }
}
