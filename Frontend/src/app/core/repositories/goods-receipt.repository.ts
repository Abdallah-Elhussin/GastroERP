import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateGoodsReceiptPayload,
  GoodsReceiptDoc,
  GoodsReceiptListParams,
  UpdateGoodsReceiptPayload
} from '../models/goods-receipt.models';

@Injectable({ providedIn: 'root' })
export class GoodsReceiptRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/inventory/goods-receipts`;

  getList(params: GoodsReceiptListParams = {}): Observable<GoodsReceiptDoc[]> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 50);
    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.status != null) httpParams = httpParams.set('status', params.status);
    if (params.from) httpParams = httpParams.set('from', params.from);
    if (params.to) httpParams = httpParams.set('to', params.to);
    if (params.supplierId) httpParams = httpParams.set('supplierId', params.supplierId);
    return this.http.get<GoodsReceiptDoc[]>(this.base, { params: httpParams }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<GoodsReceiptDoc> {
    return this.http.get<GoodsReceiptDoc>(`${this.base}/${id}`);
  }

  previewFromPo(purchaseOrderId: string): Observable<GoodsReceiptDoc> {
    return this.http.get<GoodsReceiptDoc>(`${this.base}/preview-from-po/${purchaseOrderId}`);
  }

  getNextNumber(): Observable<string> {
    return this.http.get(`${this.base}/next-number`, { responseType: 'text' }).pipe(
      map(raw => {
        const value = String(raw ?? '')
          .trim()
          .replace(/^"|"$/g, '');
        return /^GRN\d{10}$/.test(value) ? value : '';
      })
    );
  }

  create(payload: CreateGoodsReceiptPayload): Observable<GoodsReceiptDoc> {
    return this.http.post<GoodsReceiptDoc>(this.base, {
      tenantId: '00000000-0000-0000-0000-000000000000',
      ...payload
    });
  }

  update(id: string, payload: UpdateGoodsReceiptPayload): Observable<GoodsReceiptDoc> {
    return this.http.put<GoodsReceiptDoc>(`${this.base}/${id}`, payload);
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
