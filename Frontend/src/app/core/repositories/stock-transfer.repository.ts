import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateStockTransferPayload,
  StockTransferDoc,
  StockTransferListParams,
  UpdateStockTransferPayload
} from '../models/stock-transfer.models';

@Injectable({ providedIn: 'root' })
export class StockTransferRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/inventory/stock-transfers`;

  getList(params: StockTransferListParams = {}): Observable<StockTransferDoc[]> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 50);
    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.status != null) httpParams = httpParams.set('status', params.status);
    if (params.from) httpParams = httpParams.set('from', params.from);
    if (params.to) httpParams = httpParams.set('to', params.to);
    return this.http.get<StockTransferDoc[]>(this.base, { params: httpParams }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<StockTransferDoc> {
    return this.http.get<StockTransferDoc>(`${this.base}/${id}`);
  }

  nextNumber(): Observable<string> {
    return this.http.post<string>(`${this.base}/next-number`, {});
  }

  create(payload: CreateStockTransferPayload): Observable<StockTransferDoc> {
    return this.http.post<StockTransferDoc>(this.base, {
      tenantId: '00000000-0000-0000-0000-000000000000',
      ...payload
    });
  }

  update(id: string, payload: UpdateStockTransferPayload): Observable<StockTransferDoc> {
    return this.http.put<StockTransferDoc>(`${this.base}/${id}`, {
      tenantId: '00000000-0000-0000-0000-000000000000',
      ...payload
    });
  }

  approve(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/approve`, {});
  }

  post(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/post`, {});
  }

  receive(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/receive`, {});
  }

  cancel(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/cancel`, {});
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
