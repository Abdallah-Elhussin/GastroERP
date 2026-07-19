import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  BackOfficeSalesReturn,
  BackOfficeSalesReturnListParams,
  CreateBackOfficeSalesReturnPayload,
  UpdateBackOfficeSalesReturnPayload
} from '../models/back-office-sales-return.models';

@Injectable({ providedIn: 'root' })
export class BackOfficeSalesReturnRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/back-office-sales/returns`;

  getList(params: BackOfficeSalesReturnListParams = {}): Observable<BackOfficeSalesReturn[]> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 50);
    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.status != null) httpParams = httpParams.set('status', params.status);
    if (params.customerId) httpParams = httpParams.set('customerId', params.customerId);
    if (params.invoiceId) httpParams = httpParams.set('invoiceId', params.invoiceId);
    if (params.from) httpParams = httpParams.set('from', params.from);
    if (params.to) httpParams = httpParams.set('to', params.to);
    return this.http.get<BackOfficeSalesReturn[]>(this.base, { params: httpParams }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<BackOfficeSalesReturn> {
    return this.http.get<BackOfficeSalesReturn>(`${this.base}/${id}`);
  }

  create(payload: CreateBackOfficeSalesReturnPayload): Observable<BackOfficeSalesReturn> {
    return this.http.post<BackOfficeSalesReturn>(this.base, payload);
  }

  update(id: string, payload: UpdateBackOfficeSalesReturnPayload): Observable<BackOfficeSalesReturn> {
    return this.http.put<BackOfficeSalesReturn>(`${this.base}/${id}`, payload);
  }

  approve(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/approve`, {});
  }

  unapprove(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/unapprove`, {});
  }

  post(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/post`, {});
  }

  unpost(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/unpost`, {});
  }

  cancel(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/cancel`, {});
  }
}
