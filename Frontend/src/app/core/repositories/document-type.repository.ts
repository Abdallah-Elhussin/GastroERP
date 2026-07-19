import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DocumentType, UpsertDocumentTypePayload } from '../models/document-type.models';

@Injectable({ providedIn: 'root' })
export class DocumentTypeRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/finance/document-types`;

  getList(options: { search?: string; module?: number | null; isActive?: boolean | null } = {}): Observable<DocumentType[]> {
    let params = new HttpParams().set('page', '1').set('pageSize', '300');
    if (options.search?.trim()) params = params.set('search', options.search.trim());
    if (options.module != null) params = params.set('module', String(options.module));
    if (options.isActive != null) params = params.set('isActive', String(options.isActive));
    return this.http.get<DocumentType[]>(this.base, { params }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<DocumentType> {
    return this.http.get<DocumentType>(`${this.base}/${id}`);
  }

  create(payload: UpsertDocumentTypePayload): Observable<DocumentType> {
    return this.http.post<DocumentType>(this.base, payload);
  }

  update(id: string, payload: UpsertDocumentTypePayload): Observable<DocumentType> {
    return this.http.put<DocumentType>(`${this.base}/${id}`, payload);
  }

  activate(id: string): Observable<unknown> {
    return this.http.post(`${this.base}/${id}/activate`, {});
  }

  deactivate(id: string): Observable<unknown> {
    return this.http.post(`${this.base}/${id}/deactivate`, {});
  }

  copy(id: string, payload: { newCode: string; nameAr: string; nameEn: string; prefix: string }): Observable<DocumentType> {
    return this.http.post<DocumentType>(`${this.base}/${id}/copy`, payload);
  }

  delete(id: string): Observable<unknown> {
    return this.http.delete(`${this.base}/${id}`);
  }
}
