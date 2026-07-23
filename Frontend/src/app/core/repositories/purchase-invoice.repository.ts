import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreatePurchaseInvoicePayload,
  PurchaseInvoiceDoc,
  PurchaseInvoiceListParams,
  UpdatePurchaseInvoicePayload
} from '../models/purchase-invoice.models';

@Injectable({ providedIn: 'root' })
export class PurchaseInvoiceRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/inventory/purchase-invoices`;

  getList(params: PurchaseInvoiceListParams = {}): Observable<PurchaseInvoiceDoc[]> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 50);
    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.kind != null) httpParams = httpParams.set('kind', params.kind);
    if (params.status != null) httpParams = httpParams.set('status', params.status);
    if (params.supplierId) httpParams = httpParams.set('supplierId', params.supplierId);
    if (params.warehouseId) httpParams = httpParams.set('warehouseId', params.warehouseId);
    if (params.paymentMode != null) httpParams = httpParams.set('paymentMode', params.paymentMode);
    if (params.nature != null) httpParams = httpParams.set('nature', params.nature);
    if (params.from) httpParams = httpParams.set('from', params.from);
    if (params.to) httpParams = httpParams.set('to', params.to);
    return this.http.get<PurchaseInvoiceDoc[]>(this.base, { params: httpParams }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<PurchaseInvoiceDoc> {
    return this.http.get<PurchaseInvoiceDoc>(`${this.base}/${id}`);
  }

  getNextNumber(kind: number = 1): Observable<string> {
    const params = new HttpParams().set('kind', String(kind));
    return this.http
      .get(`${this.base}/next-number`, { params, responseType: 'text' })
      .pipe(
        map(raw => {
          const value = String(raw ?? '')
            .trim()
            .replace(/^"|"$/g, '');
          // FromReceipt: PI + yyyyMM + #### ; Direct: DPI + yyyyMM + ####
          if (kind === 2) return /^DPI\d{10}$/.test(value) ? value : '';
          return /^PI\d{10}$/.test(value) ? value : '';
        })
      );
  }

  create(payload: CreatePurchaseInvoicePayload): Observable<PurchaseInvoiceDoc> {
    return this.http.post<PurchaseInvoiceDoc>(this.base, payload);
  }

  update(id: string, payload: UpdatePurchaseInvoicePayload): Observable<PurchaseInvoiceDoc> {
    return this.http.put<PurchaseInvoiceDoc>(`${this.base}/${id}`, payload);
  }

  approve(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/approve`, {});
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
