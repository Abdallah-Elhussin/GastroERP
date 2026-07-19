import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  BackOfficeSalesOrder,
  BackOfficeSalesOrderListParams,
  ConvertOrderToInvoicePayload,
  ConvertOrderToInvoiceResult,
  CreateBackOfficeSalesOrderPayload,
  UpdateBackOfficeSalesOrderPayload
} from '../models/back-office-sales-order.models';

@Injectable({ providedIn: 'root' })
export class BackOfficeSalesOrderRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/back-office-sales/orders`;

  getList(params: BackOfficeSalesOrderListParams = {}): Observable<BackOfficeSalesOrder[]> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 50);
    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.status != null) httpParams = httpParams.set('status', params.status);
    if (params.fulfillmentStatus != null) httpParams = httpParams.set('fulfillmentStatus', params.fulfillmentStatus);
    if (params.customerId) httpParams = httpParams.set('customerId', params.customerId);
    if (params.from) httpParams = httpParams.set('from', params.from);
    if (params.to) httpParams = httpParams.set('to', params.to);
    return this.http.get<BackOfficeSalesOrder[]>(this.base, { params: httpParams }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<BackOfficeSalesOrder> {
    return this.http.get<BackOfficeSalesOrder>(`${this.base}/${id}`);
  }

  create(payload: CreateBackOfficeSalesOrderPayload): Observable<BackOfficeSalesOrder> {
    return this.http.post<BackOfficeSalesOrder>(this.base, payload);
  }

  update(id: string, payload: UpdateBackOfficeSalesOrderPayload): Observable<BackOfficeSalesOrder> {
    return this.http.put<BackOfficeSalesOrder>(`${this.base}/${id}`, payload);
  }

  approve(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/approve`, {});
  }

  unapprove(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/unapprove`, {});
  }

  close(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/close`, {});
  }

  cancel(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/cancel`, {});
  }

  convertToInvoice(id: string, payload: ConvertOrderToInvoicePayload): Observable<ConvertOrderToInvoiceResult> {
    return this.http.post<string | ConvertOrderToInvoiceResult>(`${this.base}/${id}/convert-to-invoice`, payload).pipe(
      map(r => (typeof r === 'string' ? { invoiceId: r } : { invoiceId: (r as ConvertOrderToInvoiceResult).invoiceId ?? String(r) }))
    );
  }
}
