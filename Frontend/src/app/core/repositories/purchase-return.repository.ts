import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreatePurchaseReturnPayload,
  PurchaseReturnDoc,
  PurchaseReturnListParams,
  PurchaseReturnReason,
  UpdatePurchaseReturnPayload
} from '../models/purchase-return.models';

@Injectable({ providedIn: 'root' })
export class PurchaseReturnRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/inventory/purchase-returns`;

  getList(params: PurchaseReturnListParams = {}): Observable<PurchaseReturnDoc[]> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 50);
    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.status != null) httpParams = httpParams.set('status', params.status);
    if (params.returnType != null) httpParams = httpParams.set('returnType', params.returnType);
    if (params.from) httpParams = httpParams.set('from', params.from);
    if (params.to) httpParams = httpParams.set('to', params.to);
    if (params.supplierId) httpParams = httpParams.set('supplierId', params.supplierId);
    return this.http.get<PurchaseReturnDoc[]>(this.base, { params: httpParams }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<PurchaseReturnDoc> {
    return this.http.get<PurchaseReturnDoc>(`${this.base}/${id}`);
  }

  previewFromGrn(goodsReceiptId: string): Observable<PurchaseReturnDoc> {
    return this.http.get<PurchaseReturnDoc>(`${this.base}/preview-from-grn/${goodsReceiptId}`);
  }

  previewFromInvoice(purchaseInvoiceId: string): Observable<PurchaseReturnDoc> {
    return this.http.get<PurchaseReturnDoc>(`${this.base}/preview-from-invoice/${purchaseInvoiceId}`);
  }

  getReasons(activeOnly = true): Observable<PurchaseReturnReason[]> {
    const params = new HttpParams().set('activeOnly', activeOnly);
    return this.http.get<PurchaseReturnReason[]>(`${this.base}/reasons`, { params }).pipe(map(r => r ?? []));
  }

  seedReasons(): Observable<PurchaseReturnReason[]> {
    return this.http.post<PurchaseReturnReason[]>(`${this.base}/reasons/seed`, {}).pipe(map(r => r ?? []));
  }

  create(payload: CreatePurchaseReturnPayload): Observable<PurchaseReturnDoc> {
    return this.http.post<PurchaseReturnDoc>(this.base, payload);
  }

  update(id: string, payload: UpdatePurchaseReturnPayload): Observable<PurchaseReturnDoc> {
    return this.http.put<PurchaseReturnDoc>(`${this.base}/${id}`, payload);
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
