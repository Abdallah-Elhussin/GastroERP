import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  FinancialNote,
  FinancialNoteListFilter,
  UpsertFinancialNotePayload
} from '../models/financial-note.models';

@Injectable({ providedIn: 'root' })
export class FinancialNoteRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/finance/financial-notes`;

  getList(options: FinancialNoteListFilter = {}): Observable<FinancialNote[]> {
    let params = new HttpParams().set('pageSize', String(options.pageSize ?? 200));
    if (options.search) params = params.set('search', options.search);
    if (options.companyId) params = params.set('companyId', options.companyId);
    if (options.branchId) params = params.set('branchId', options.branchId);
    if (options.noteKind != null) params = params.set('noteKind', String(options.noteKind));
    if (options.status != null) params = params.set('status', String(options.status));
    if (options.partyType != null) params = params.set('partyType', String(options.partyType));
    if (options.fromDate) params = params.set('fromDate', options.fromDate);
    if (options.toDate) params = params.set('toDate', options.toDate);
    return this.http.get<FinancialNote[]>(this.base, { params }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<FinancialNote> {
    return this.http.get<FinancialNote>(`${this.base}/${id}`);
  }

  create(payload: UpsertFinancialNotePayload): Observable<FinancialNote> {
    return this.http.post<FinancialNote>(this.base, payload);
  }

  update(id: string, payload: UpsertFinancialNotePayload): Observable<FinancialNote> {
    return this.http.put<FinancialNote>(`${this.base}/${id}`, payload);
  }

  approve(id: string): Observable<FinancialNote> {
    return this.http.post<FinancialNote>(`${this.base}/${id}/approve`, {});
  }

  post(id: string): Observable<FinancialNote> {
    return this.http.post<FinancialNote>(`${this.base}/${id}/post`, {});
  }

  reverse(id: string): Observable<FinancialNote> {
    return this.http.post<FinancialNote>(`${this.base}/${id}/reverse`, {});
  }

  cancel(id: string): Observable<FinancialNote> {
    return this.http.post<FinancialNote>(`${this.base}/${id}/cancel`, {});
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
