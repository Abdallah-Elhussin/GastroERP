import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MediaRepository, MediaFolder, MediaFile } from './media.repository';

@Injectable({
  providedIn: 'root'
})
export class RestMediaRepository extends MediaRepository {
  private http = inject(HttpClient);
  private apiUrl = '/api/media';

  getFolders(): Observable<MediaFolder[]> {
    return this.http.get<MediaFolder[]>(`${this.apiUrl}/folders`);
  }

  getFiles(): Observable<MediaFile[]> {
    return this.http.get<MediaFile[]>(`${this.apiUrl}/files`);
  }

  createFolder(name: string, parentId: string | null): Observable<MediaFolder> {
    return this.http.post<MediaFolder>(`${this.apiUrl}/folders`, { name, parentId });
  }

  renameFolder(folderId: string, name: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/folders/${folderId}`, { name });
  }

  deleteFolder(folderId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/folders/${folderId}`);
  }

  uploadFiles(files: File[], folderId: string | null): Observable<MediaFile[]> {
    const formData = new FormData();
    files.forEach(file => formData.append('files', file));
    if (folderId) {
      formData.append('folderId', folderId);
    }
    return this.http.post<MediaFile[]>(`${this.apiUrl}/upload`, formData);
  }

  deleteFiles(fileIds: string[]): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/delete-files`, { fileIds });
  }

  moveFiles(fileIds: string[], targetFolderId: string | null): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/move-files`, { fileIds, targetFolderId });
  }

  processImage(fileId: string, actions: { crop?: any; rotate?: number; compress?: boolean }): Observable<MediaFile> {
    return this.http.post<MediaFile>(`${this.apiUrl}/process-image/${fileId}`, actions);
  }
}
