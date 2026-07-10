import { Component, ChangeDetectionStrategy, Input, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-media-picker',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <div class="flex flex-col gap-4 text-left select-none">
      <span class="text-[10px] font-bold text-[var(--text-muted)] uppercase tracking-wider">Select Media Asset</span>
      
      <!-- List Grid -->
      <div class="grid grid-cols-3 sm:grid-cols-4 gap-3 max-h-64 overflow-y-auto p-1">
        <div 
          *ngFor="let img of images"
          (click)="selectImage(img)"
          [class.border-[var(--accent-color)]]="selectedImage() === img"
          [class.ring-2]="selectedImage() === img"
          [class.ring-[var(--accent-color)]]="selectedImage() === img"
          class="border border-[var(--border-color)] rounded-xl overflow-hidden cursor-pointer hover:shadow-sm transition-all h-20 bg-gray-50 flex-shrink-0 relative group"
        >
          <img [src]="img" class="w-full h-full object-cover" />
          <div class="absolute inset-0 bg-black bg-opacity-10 opacity-0 group-hover:opacity-100 transition-opacity"></div>
        </div>
      </div>

      <!-- Action buttons -->
      <div class="flex justify-end gap-2 border-t border-[var(--border-color-muted)] pt-3">
        <button 
          (click)="confirmSelection()"
          [disabled]="!selectedImage()"
          class="bg-[var(--primary-color)] text-[var(--primary-contrast)] hover:bg-[var(--primary-color-hover)] disabled:bg-gray-200 disabled:text-gray-400 disabled:cursor-not-allowed px-4 py-2 rounded-xl text-xs font-bold shadow-sm cursor-pointer"
        >
          Confirm Select
        </button>
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
export class AppMediaPickerComponent {
  @Input() images: string[] = [
    'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=150&auto=format&fit=crop&q=80',
    'https://images.unsplash.com/photo-1573080496219-bb080dd4f877?w=150&auto=format&fit=crop&q=80',
    'https://images.unsplash.com/photo-1514362545857-3bc16c4c7d1b?w=150&auto=format&fit=crop&q=80',
    'https://images.unsplash.com/photo-1532636875304-0c8fe1197e1d?w=150&auto=format&fit=crop&q=80',
    'https://images.unsplash.com/photo-1587314168485-3236d6710814?w=150&auto=format&fit=crop&q=80'
  ];

  @Output() imageSelected = new EventEmitter<string>();

  selectedImage = signal<string | null>(null);

  selectImage(img: string): void {
    this.selectedImage.set(img);
  }

  confirmSelection(): void {
    const selected = this.selectedImage();
    if (selected) {
      this.imageSelected.emit(selected);
    }
  }
}
