import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  BackOfficeSalesInvoice,
  BackOfficeSalesInvoiceListParams,
  CreateBackOfficeSalesInvoicePayload,
  UpdateBackOfficeSalesInvoicePayload
} from '../models/back-office-sales-invoice.models';

@Injectable({ providedIn: 'root' })
export class BackOfficeSalesInvoiceRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/back-office-sales/invoices`;

  getList(params: BackOfficeSalesInvoiceListParams = {}): Observable<BackOfficeSalesInvoice[]> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 50);
    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.status != null) httpParams = httpParams.set('status', params.status);
    if (params.customerId) httpParams = httpParams.set('customerId', params.customerId);
    if (params.from) httpParams = httpParams.set('from', params.from);
    if (params.to) httpParams = httpParams.set('to', params.to);
    return this.http.get<BackOfficeSalesInvoice[]>(this.base, { params: httpParams }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<BackOfficeSalesInvoice> {
    return this.http.get<BackOfficeSalesInvoice>(`${this.base}/${id}`);
  }

  create(payload: CreateBackOfficeSalesInvoicePayload): Observable<BackOfficeSalesInvoice> {
    return this.http.post<BackOfficeSalesInvoice>(this.base, payload);
  }

  update(id: string, payload: UpdateBackOfficeSalesInvoicePayload): Observable<BackOfficeSalesInvoice> {
    return this.http.put<BackOfficeSalesInvoice>(`${this.base}/${id}`, payload);
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
