import { Observable } from 'rxjs';

export interface MediaFolder {
  id: string;
  name: string;
  parentId: string | null;
  isFavorite?: boolean;
}

export interface MediaFile {
  id: string;
  name: string;
  url: string;
  folderId: string | null;
  type: 'image' | 'video' | 'pdf' | 'document' | 'svg';
  size: string;
  createdDate: string;
  tags: string[];
}

export abstract class MediaRepository {
  abstract getFolders(): Observable<MediaFolder[]>;
  abstract getFiles(): Observable<MediaFile[]>;
  
  abstract createFolder(name: string, parentId: string | null): Observable<MediaFolder>;
  abstract renameFolder(folderId: string, name: string): Observable<void>;
  abstract deleteFolder(folderId: string): Observable<void>;
  
  abstract uploadFiles(files: File[], folderId: string | null): Observable<MediaFile[]>;
  abstract deleteFiles(fileIds: string[]): Observable<void>;
  abstract moveFiles(fileIds: string[], targetFolderId: string | null): Observable<void>;
  
  abstract processImage(fileId: string, actions: { crop?: any; rotate?: number; compress?: boolean }): Observable<MediaFile>;
}
