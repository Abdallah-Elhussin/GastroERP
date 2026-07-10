import { Component, ChangeDetectionStrategy, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MediaFolder } from '../../../../core/repositories/media.repository';
import { LanguageService } from '../../../../core/services/language.service';

@Component({
  selector: 'app-media-folders',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  template: `
    <div class="flex flex-col gap-4">
      <div class="flex justify-between items-center px-2">
        <span class="text-[10px] font-bold text-[var(--text-muted)] uppercase tracking-wider">{{ t('media.folders') }}</span>
        <button mat-icon-button (click)="onCreateFolder()" class="w-7 h-7 rounded-lg hover:bg-[var(--bg-canvas)]" [title]="t('media.folders.create')">
          <mat-icon class="text-sm">create_new_folder</mat-icon>
        </button>
      </div>

      <div class="flex flex-col gap-1">
        <!-- Root / All Files -->
        <div 
          (click)="onSelectFolder(null)"
          [class.bg-[var(--bg-surface-hover)]]="selectedFolderId === null"
          class="flex items-center gap-3 px-3 py-2 rounded-xl cursor-pointer hover:bg-[var(--bg-surface-hover)] transition-colors"
        >
          <mat-icon class="text-base text-[var(--text-muted)]">folder_special</mat-icon>
          <span class="text-xs font-semibold text-[var(--text-primary)]">{{ t('media.allAssets') }}</span>
        </div>

        <!-- Folders List -->
        <div 
          *ngFor="let folder of folders"
          (click)="onSelectFolder(folder.id)"
          [class.bg-[var(--bg-surface-hover)]]="selectedFolderId === folder.id"
          class="group flex items-center justify-between px-3 py-2 rounded-xl cursor-pointer hover:bg-[var(--bg-surface-hover)] transition-colors"
        >
          <div class="flex items-center gap-3">
            <mat-icon class="text-base" [class.text-amber-500]="folder.isFavorite" [class.text-blue-500]="!folder.isFavorite">
              {{ folder.isFavorite ? 'folder_special' : 'folder' }}
            </mat-icon>
            <span class="text-xs font-semibold text-[var(--text-primary)]">{{ folder.name }}</span>
          </div>

          <!-- Actions -->
          <div class="hidden group-hover:flex items-center gap-1">
            <button mat-icon-button (click)="$event.stopPropagation(); onRename(folder)" class="w-6 h-6 rounded hover:bg-[var(--bg-canvas)]" [title]="t('media.folders.rename')">
              <mat-icon class="text-xs">edit</mat-icon>
            </button>
            <button mat-icon-button (click)="$event.stopPropagation(); onDelete(folder.id)" class="w-6 h-6 rounded hover:bg-[var(--bg-canvas)] text-red-500" [title]="t('common.delete')">
              <mat-icon class="text-xs">delete</mat-icon>
            </button>
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
export class MediaFoldersComponent {
  langService = inject(LanguageService);

  @Input() folders: MediaFolder[] = [];
  @Input() selectedFolderId: string | null = null;

  @Output() selectFolder = new EventEmitter<string | null>();
  @Output() createFolder = new EventEmitter<string>();
  @Output() renameFolder = new EventEmitter<{ id: string; name: string }>();
  @Output() deleteFolder = new EventEmitter<string>();

  onSelectFolder(folderId: string | null): void {
    this.selectFolder.emit(folderId);
  }

  onCreateFolder(): void {
    const name = prompt(this.t('media.folders.create') + ':');
    if (name) {
      this.createFolder.emit(name);
    }
  }

  onRename(folder: MediaFolder): void {
    const name = prompt(this.t('media.folders.rename') + ':', folder.name);
    if (name) {
      this.renameFolder.emit({ id: folder.id, name });
    }
  }

  onDelete(folderId: string): void {
    if (confirm(this.t('common.delete') + '?')) {
      this.deleteFolder.emit(folderId);
    }
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
