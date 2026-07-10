import { Injectable } from '@angular/core';
import { Observable, of, BehaviorSubject } from 'rxjs';
import { MediaRepository, MediaFolder, MediaFile } from './media.repository';

@Injectable({
  providedIn: 'root'
})
export class MockMediaRepository extends MediaRepository {
  private folders$ = new BehaviorSubject<MediaFolder[]>([
    { id: 'f-logos', name: 'Logos & Identity', parentId: null, isFavorite: true },
    { id: 'f-products', name: 'Product Menu Assets', parentId: null },
    { id: 'f-bg', name: 'Background Banners', parentId: null, isFavorite: true },
    { id: 'f-staff', name: 'Staff Profiles', parentId: null }
  ]);

  private files$ = new BehaviorSubject<MediaFile[]>([
    { 
      id: 'm-logo-main', 
      name: 'corporate_logo_main.png', 
      url: 'https://images.unsplash.com/photo-1543007630-9710e4a00a20?w=300&auto=format&fit=crop&q=80', 
      folderId: 'f-logos', 
      type: 'image', 
      size: '42 KB', 
      createdDate: '2026-07-01', 
      tags: ['logo', 'brand'] 
    },
    { 
      id: 'm-bg-login', 
      name: 'dining_table_landscape.jpg', 
      url: 'https://images.unsplash.com/photo-1544025162-d76694265947?w=1200&auto=format&fit=crop&q=80', 
      folderId: 'f-bg', 
      type: 'image', 
      size: '412 KB', 
      createdDate: '2026-07-02', 
      tags: ['background', 'login'] 
    },
    { 
      id: 'm-prod-wagyu', 
      name: 'wagyu_gold_burger.jpg', 
      url: 'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=600&auto=format&fit=crop&q=80', 
      folderId: 'f-products', 
      type: 'image', 
      size: '180 KB', 
      createdDate: '2026-07-03', 
      tags: ['burger', 'wagyu'] 
    },
    { 
      id: 'm-prod-fries', 
      name: 'truffle_fries_crispy.jpg', 
      url: 'https://images.unsplash.com/photo-1573080496219-bb080dd4f877?w=600&auto=format&fit=crop&q=80', 
      folderId: 'f-products', 
      type: 'image', 
      size: '95 KB', 
      createdDate: '2026-07-04', 
      tags: ['fries', 'truffle'] 
    },
    { 
      id: 'm-avatar-julian', 
      name: 'julian_sterling_avatar.jpg', 
      url: 'https://images.unsplash.com/photo-1577219491135-ce391730fb2c?w=150&auto=format&fit=crop&q=80', 
      folderId: 'f-staff', 
      type: 'image', 
      size: '28 KB', 
      createdDate: '2026-07-05', 
      tags: ['staff', 'avatar', 'julian'] 
    }
  ]);

  getFolders(): Observable<MediaFolder[]> {
    return this.folders$.asObservable();
  }

  getFiles(): Observable<MediaFile[]> {
    return this.files$.asObservable();
  }

  createFolder(name: string, parentId: string | null): Observable<MediaFolder> {
    const newFolder: MediaFolder = {
      id: `f-${Math.floor(Math.random() * 9000) + 1000}`,
      name,
      parentId
    };
    this.folders$.next([...this.folders$.value, newFolder]);
    return of(newFolder);
  }

  renameFolder(folderId: string, name: string): Observable<void> {
    const list = this.folders$.value.map(f => f.id === folderId ? { ...f, name } : f);
    this.folders$.next(list);
    return of(undefined);
  }

  deleteFolder(folderId: string): Observable<void> {
    const list = this.folders$.value.filter(f => f.id !== folderId);
    this.folders$.next(list);
    // Un-assign files folderId
    const filesList = this.files$.value.map(f => f.folderId === folderId ? { ...f, folderId: null } : f);
    this.files$.next(filesList);
    return of(undefined);
  }

  uploadFiles(files: File[], folderId: string | null): Observable<MediaFile[]> {
    const newFiles: MediaFile[] = files.map((file, index) => ({
      id: `m-upload-${Math.floor(Math.random() * 9000) + 1000}-${index}`,
      name: file.name,
      url: 'https://images.unsplash.com/photo-1543007630-9710e4a00a20?w=300&auto=format&fit=crop&q=80', // mock uploaded URL
      folderId,
      type: 'image',
      size: `${Math.ceil(file.size / 1024)} KB`,
      createdDate: new Date().toISOString().split('T')[0],
      tags: ['upload']
    }));
    this.files$.next([...this.files$.value, ...newFiles]);
    return of(newFiles);
  }

  deleteFiles(fileIds: string[]): Observable<void> {
    const list = this.files$.value.filter(f => !fileIds.includes(f.id));
    this.files$.value.filter(f => fileIds.includes(f.id)).forEach(f => {
      // Clean temporary object links if needed
    });
    this.files$.next(list);
    return of(undefined);
  }

  moveFiles(fileIds: string[], targetFolderId: string | null): Observable<void> {
    const list = this.files$.value.map(f => fileIds.includes(f.id) ? { ...f, folderId: targetFolderId } : f);
    this.files$.next(list);
    return of(undefined);
  }

  processImage(fileId: string, actions: { crop?: any; rotate?: number; compress?: boolean }): Observable<MediaFile> {
    const list = this.files$.value.map(f => {
      if (f.id === fileId) {
        // mock image rotate or compress to webp
        const extension = actions.compress ? 'webp' : f.name.split('.').pop();
        const baseName = f.name.split('.').shift();
        return {
          ...f,
          name: `${baseName}_edited.${extension}`,
          tags: [...f.tags, 'processed', 'webp']
        };
      }
      return f;
    });
    this.files$.next(list);
    const updated = this.files$.value.find(f => f.id === fileId);
    return of(updated!);
  }
}
