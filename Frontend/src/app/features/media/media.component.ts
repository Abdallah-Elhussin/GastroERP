import { Component, ChangeDetectionStrategy, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MediaRepository, MediaFolder, MediaFile } from '../../core/repositories/media.repository';
import { MediaFoldersComponent } from './components/media-folders/media-folders.component';
import { MediaGridComponent } from './components/media-grid/media-grid.component';
import { MediaListComponent } from './components/media-list/media-list.component';
import { MediaUploadComponent } from './components/media-upload/media-upload.component';
import { MediaToolbarComponent } from './components/media-toolbar/media-toolbar.component';
import { MediaDetailsComponent } from './components/media-details/media-details.component';
import { MediaCropDialogComponent } from './components/media-crop-dialog/media-crop-dialog.component';

import { LanguageService } from '../../core/services/language.service';

@Component({
  selector: 'app-media',
  standalone: true,
  imports: [
    CommonModule,
    MediaFoldersComponent,
    MediaGridComponent,
    MediaListComponent,
    MediaUploadComponent,
    MediaToolbarComponent,
    MediaDetailsComponent,
    MediaCropDialogComponent
  ],
  templateUrl: './media.component.html',
  styleUrl: './media.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MediaComponent implements OnInit {
  mediaRepository = inject(MediaRepository);
  langService = inject(LanguageService);

  folders = signal<MediaFolder[]>([]);
  files = signal<MediaFile[]>([]);

  selectedFolderId = signal<string | null>(null);
  selectedFile = signal<MediaFile | null>(null);
  bulkSelectedIds = signal<string[]>([]);
  
  searchQuery = signal<string>('');
  activeFilter = signal<string>('all');
  viewMode = signal<'grid' | 'list'>('grid');

  isCropping = signal<boolean>(false);

  ngOnInit(): void {
    this.loadMedia();
  }

  loadMedia(): void {
    this.mediaRepository.getFolders().subscribe(list => this.folders.set(list));
    this.mediaRepository.getFiles().subscribe(list => this.files.set(list));
  }

  // Filtered files computed signal
  filteredFiles = computed(() => {
    let result = this.files();

    // 1. Filter by folder
    const folderId = this.selectedFolderId();
    if (folderId !== null) {
      result = result.filter(f => f.folderId === folderId);
    }

    // 2. Filter by search query
    const query = this.searchQuery().toLowerCase();
    if (query) {
      result = result.filter(f => f.name.toLowerCase().includes(query) || f.tags.some(t => t.toLowerCase().includes(query)));
    }

    // 3. Filter by type
    const filter = this.activeFilter();
    if (filter !== 'all') {
      if (filter === 'image') {
        result = result.filter(f => f.type === 'image');
      } else if (filter === 'svg') {
        result = result.filter(f => f.type === 'svg');
      } else if (filter === 'document') {
        result = result.filter(f => f.type === 'document' || f.type === 'pdf');
      }
    }

    return result;
  });

  // Folder Operations
  onSelectFolder(folderId: string | null): void {
    this.selectedFolderId.set(folderId);
    this.selectedFile.set(null);
    this.bulkSelectedIds.set([]);
  }

  onCreateFolder(name: string): void {
    this.mediaRepository.createFolder(name, this.selectedFolderId()).subscribe(() => {
      this.loadMedia();
    });
  }

  onRenameFolder(event: { id: string; name: string }): void {
    this.mediaRepository.renameFolder(event.id, event.name).subscribe(() => {
      this.loadMedia();
    });
  }

  onDeleteFolder(folderId: string): void {
    this.mediaRepository.deleteFolder(folderId).subscribe(() => {
      if (this.selectedFolderId() === folderId) {
        this.selectedFolderId.set(null);
      }
      this.loadMedia();
    });
  }

  // File Selection
  onSelectFile(file: MediaFile): void {
    this.selectedFile.set(file);
  }

  onToggleBulk(fileId: string): void {
    this.bulkSelectedIds.update(current => {
      if (current.includes(fileId)) {
        return current.filter(id => id !== fileId);
      } else {
        return [...current, fileId];
      }
    });
  }

  // File Uploads
  onFilesUploaded(files: File[]): void {
    this.mediaRepository.uploadFiles(files, this.selectedFolderId()).subscribe(() => {
      this.loadMedia();
    });
  }

  // Single Actions
  onRotateImage(fileId: string): void {
    this.mediaRepository.processImage(fileId, { rotate: 90 }).subscribe(() => {
      this.loadMedia();
      // refresh selected file properties
      const updated = this.files().find(f => f.id === fileId);
      if (updated) this.selectedFile.set(updated);
    });
  }

  onCompressToWebP(fileId: string): void {
    this.mediaRepository.processImage(fileId, { compress: true }).subscribe(() => {
      this.loadMedia();
      const updated = this.files().find(f => f.id === fileId);
      if (updated) this.selectedFile.set(updated);
    });
  }

  onCropImage(fileId: string): void {
    this.isCropping.set(true);
  }

  onCropApplied(fileId: string): void {
    this.mediaRepository.processImage(fileId, { crop: { aspect: '1:1' } }).subscribe(() => {
      this.isCropping.set(false);
      this.loadMedia();
      const updated = this.files().find(f => f.id === fileId);
      if (updated) this.selectedFile.set(updated);
    });
  }

  onDeleteFile(fileId: string): void {
    this.mediaRepository.deleteFiles([fileId]).subscribe(() => {
      this.selectedFile.set(null);
      this.bulkSelectedIds.update(curr => curr.filter(id => id !== fileId));
      this.loadMedia();
    });
  }

  // Bulk Actions
  onBulkCompress(): void {
    const ids = this.bulkSelectedIds();
    let count = 0;
    ids.forEach(id => {
      this.mediaRepository.processImage(id, { compress: true }).subscribe(() => {
        count++;
        if (count === ids.length) {
          this.loadMedia();
          this.bulkSelectedIds.set([]);
          this.selectedFile.set(null);
          alert('Selected files compressed to WebP format.');
        }
      });
    });
  }

  onBulkDelete(): void {
    this.mediaRepository.deleteFiles(this.bulkSelectedIds()).subscribe(() => {
      this.bulkSelectedIds.set([]);
      this.selectedFile.set(null);
      this.loadMedia();
    });
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
