import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ReceiptVoucher,
  ReceiptVoucherListFilter,
  UpsertReceiptVoucherPayload
} from '../models/receipt-voucher.models';

@Injectable({ providedIn: 'root' })
export class ReceiptVoucherRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/finance/receipt-vouchers`;

  getList(options: ReceiptVoucherListFilter = {}): Observable<ReceiptVoucher[]> {
    let params = new HttpParams().set('pageSize', String(options.pageSize ?? 200));
    if (options.search) params = params.set('search', options.search);
    if (options.companyId) params = params.set('companyId', options.companyId);
    if (options.branchId) params = params.set('branchId', options.branchId);
    if (options.fiscalPeriodId) params = params.set('fiscalPeriodId', options.fiscalPeriodId);
    if (options.status != null) params = params.set('status', String(options.status));
    if (options.receiptMethod != null) params = params.set('receiptMethod', String(options.receiptMethod));
    if (options.cashBoxId) params = params.set('cashBoxId', options.cashBoxId);
    if (options.bankId) params = params.set('bankId', options.bankId);
    if (options.currency) params = params.set('currency', options.currency);
    if (options.fromDate) params = params.set('fromDate', options.fromDate);
    if (options.toDate) params = params.set('toDate', options.toDate);
    return this.http.get<ReceiptVoucher[]>(this.base, { params }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<ReceiptVoucher> {
    return this.http.get<ReceiptVoucher>(`${this.base}/${id}`);
  }

  create(payload: UpsertReceiptVoucherPayload): Observable<ReceiptVoucher> {
    return this.http.post<ReceiptVoucher>(this.base, payload);
  }

  update(id: string, payload: UpsertReceiptVoucherPayload): Observable<ReceiptVoucher> {
    return this.http.put<ReceiptVoucher>(`${this.base}/${id}`, payload);
  }

  approve(id: string): Observable<ReceiptVoucher> {
    return this.http.post<ReceiptVoucher>(`${this.base}/${id}/approve`, {});
  }

  post(id: string): Observable<ReceiptVoucher> {
    return this.http.post<ReceiptVoucher>(`${this.base}/${id}/post`, {});
  }

  reverse(id: string): Observable<ReceiptVoucher> {
    return this.http.post<ReceiptVoucher>(`${this.base}/${id}/reverse`, {});
  }

  cancel(id: string): Observable<ReceiptVoucher> {
    return this.http.post<ReceiptVoucher>(`${this.base}/${id}/cancel`, {});
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
