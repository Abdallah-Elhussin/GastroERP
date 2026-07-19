import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  BackOfficeSalesQuotation,
  BackOfficeSalesQuotationListParams,
  ConvertQuotationToOrderPayload,
  ConvertQuotationToOrderResult,
  CreateBackOfficeSalesQuotationPayload,
  UpdateBackOfficeSalesQuotationPayload
} from '../models/back-office-sales-quotation.models';

@Injectable({ providedIn: 'root' })
export class BackOfficeSalesQuotationRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/back-office-sales/quotations`;

  getList(params: BackOfficeSalesQuotationListParams = {}): Observable<BackOfficeSalesQuotation[]> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 50);
    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.status != null) httpParams = httpParams.set('status', params.status);
    if (params.customerId) httpParams = httpParams.set('customerId', params.customerId);
    if (params.from) httpParams = httpParams.set('from', params.from);
    if (params.to) httpParams = httpParams.set('to', params.to);
    return this.http.get<BackOfficeSalesQuotation[]>(this.base, { params: httpParams }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<BackOfficeSalesQuotation> {
    return this.http.get<BackOfficeSalesQuotation>(`${this.base}/${id}`);
  }

  create(payload: CreateBackOfficeSalesQuotationPayload): Observable<BackOfficeSalesQuotation> {
    return this.http.post<BackOfficeSalesQuotation>(this.base, payload);
  }

  update(id: string, payload: UpdateBackOfficeSalesQuotationPayload): Observable<BackOfficeSalesQuotation> {
    return this.http.put<BackOfficeSalesQuotation>(`${this.base}/${id}`, payload);
  }

  approve(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/approve`, {});
  }

  unapprove(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/unapprove`, {});
  }

  cancel(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/cancel`, {});
  }

  convertToOrder(id: string, payload: ConvertQuotationToOrderPayload): Observable<ConvertQuotationToOrderResult> {
    return this.http.post<string | ConvertQuotationToOrderResult>(`${this.base}/${id}/convert-to-order`, payload).pipe(
      map(r => (typeof r === 'string' ? { orderId: r } : { orderId: (r as ConvertQuotationToOrderResult).orderId ?? String(r) }))
    );
  }
}
