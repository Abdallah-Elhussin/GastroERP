import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AccountClassification,
  AccountMainClassification,
  UpsertAccountClassificationPayload
} from '../models/account-classification.models';

@Injectable({ providedIn: 'root' })
export class AccountClassificationRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/finance/account-classifications`;
  private readonly mains = `${environment.apiBaseUrl}/finance/account-main-classifications`;

  getMains(): Observable<AccountMainClassification[]> {
    return this.http.get<AccountMainClassification[]>(this.mains).pipe(map(r => r ?? []));
  }

  getList(options: { search?: string; mainClassificationId?: string | null } = {}): Observable<AccountClassification[]> {
    let params = new HttpParams();
    if (options.search?.trim()) params = params.set('search', options.search.trim());
    if (options.mainClassificationId) params = params.set('mainClassificationId', options.mainClassificationId);
    return this.http.get<AccountClassification[]>(this.base, { params }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<AccountClassification> {
    return this.http.get<AccountClassification>(`${this.base}/${id}`);
  }

  create(payload: UpsertAccountClassificationPayload): Observable<AccountClassification> {
    return this.http.post<AccountClassification>(this.base, payload);
  }

  update(id: string, payload: UpsertAccountClassificationPayload): Observable<AccountClassification> {
    return this.http.put<AccountClassification>(`${this.base}/${id}`, {
      nameAr: payload.nameAr,
      nameEn: payload.nameEn,
      mainClassificationId: payload.mainClassificationId
    });
  }

  delete(id: string): Observable<unknown> {
    return this.http.delete(`${this.base}/${id}`);
  }
}
