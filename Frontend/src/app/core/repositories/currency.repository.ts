import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Currency,
  CurrencyExchangeRate,
  UpsertCurrencyPayload,
  UpsertExchangeRatePayload
} from '../models/currency.models';

@Injectable({ providedIn: 'root' })
export class CurrencyRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/finance/currencies`;
  private readonly ratesBase = `${environment.apiBaseUrl}/finance/exchange-rates`;

  getList(options: { search?: string; status?: number | null; pageSize?: number } = {}): Observable<Currency[]> {
    let params = new HttpParams()
      .set('page', '1')
      .set('pageSize', String(options.pageSize ?? 200));
    if (options.search?.trim()) params = params.set('search', options.search.trim());
    if (options.status != null) params = params.set('status', String(options.status));
    return this.http.get<Currency[]>(this.base, { params }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<Currency> {
    return this.http.get<Currency>(`${this.base}/${id}`);
  }

  create(payload: UpsertCurrencyPayload): Observable<Currency> {
    return this.http.post<Currency>(this.base, payload);
  }

  update(id: string, payload: UpsertCurrencyPayload): Observable<Currency> {
    return this.http.put<Currency>(`${this.base}/${id}`, payload);
  }

  activate(id: string): Observable<unknown> {
    return this.http.post(`${this.base}/${id}/activate`, {});
  }

  deactivate(id: string): Observable<unknown> {
    return this.http.post(`${this.base}/${id}/deactivate`, {});
  }

  setCompany(id: string): Observable<Currency> {
    return this.http.post<Currency>(`${this.base}/${id}/set-company`, {});
  }

  delete(id: string): Observable<unknown> {
    return this.http.delete(`${this.base}/${id}`);
  }

  exportCsv(search?: string): Observable<Blob> {
    let params = new HttpParams();
    if (search?.trim()) params = params.set('search', search.trim());
    return this.http.get(`${this.base}/export`, { params, responseType: 'blob' });
  }

  getExchangeRates(options: {
    currencyId?: string;
    asOfDate?: string;
    activeOnly?: boolean;
    search?: string;
    pageSize?: number;
  } = {}): Observable<CurrencyExchangeRate[]> {
    let params = new HttpParams()
      .set('page', '1')
      .set('pageSize', String(options.pageSize ?? 200));
    if (options.currencyId) params = params.set('currencyId', options.currencyId);
    if (options.asOfDate) params = params.set('asOfDate', options.asOfDate);
    if (options.activeOnly) params = params.set('activeOnly', 'true');
    if (options.search?.trim()) params = params.set('search', options.search.trim());
    return this.http.get<CurrencyExchangeRate[]>(this.ratesBase, { params }).pipe(map(r => r ?? []));
  }

  createExchangeRate(payload: UpsertExchangeRatePayload): Observable<CurrencyExchangeRate> {
    return this.http.post<CurrencyExchangeRate>(this.ratesBase, payload);
  }

  updateExchangeRate(id: string, payload: UpsertExchangeRatePayload): Observable<CurrencyExchangeRate> {
    return this.http.put<CurrencyExchangeRate>(`${this.ratesBase}/${id}`, payload);
  }

  activateExchangeRate(id: string): Observable<unknown> {
    return this.http.post(`${this.ratesBase}/${id}/activate`, {});
  }

  deactivateExchangeRate(id: string): Observable<unknown> {
    return this.http.post(`${this.ratesBase}/${id}/deactivate`, {});
  }

  deleteExchangeRate(id: string): Observable<unknown> {
    return this.http.delete(`${this.ratesBase}/${id}`);
  }

  exportExchangeRatesCsv(options: { currencyId?: string; activeOnly?: boolean; search?: string } = {}): Observable<Blob> {
    let params = new HttpParams();
    if (options.currencyId) params = params.set('currencyId', options.currencyId);
    if (options.activeOnly) params = params.set('activeOnly', 'true');
    if (options.search?.trim()) params = params.set('search', options.search.trim());
    return this.http.get(`${this.ratesBase}/export`, { params, responseType: 'blob' });
  }
}
