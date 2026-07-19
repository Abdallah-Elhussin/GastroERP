import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AccountLookup,
  CreateOpeningBalancePayload,
  OpeningBalanceDoc,
  UpdateOpeningBalancePayload
} from '../models/opening-balance.models';
import { CostCenterLookup } from '../models/inventory-valuation-group.models';

@Injectable({ providedIn: 'root' })
export class OpeningBalanceRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/inventory/opening-balances`;

  getList(page = 1, pageSize = 50): Observable<OpeningBalanceDoc[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<OpeningBalanceDoc[]>(this.base, { params }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<OpeningBalanceDoc> {
    return this.http.get<OpeningBalanceDoc>(`${this.base}/${id}`);
  }

  nextNumber(): Observable<string> {
    return this.http.post<string>(`${this.base}/next-number`, {});
  }

  create(payload: CreateOpeningBalancePayload): Observable<OpeningBalanceDoc> {
    return this.http.post<OpeningBalanceDoc>(this.base, {
      tenantId: '00000000-0000-0000-0000-000000000000',
      ...payload
    });
  }

  update(id: string, payload: UpdateOpeningBalancePayload): Observable<OpeningBalanceDoc> {
    return this.http.put<OpeningBalanceDoc>(`${this.base}/${id}`, {
      tenantId: '00000000-0000-0000-0000-000000000000',
      ...payload
    });
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

  downloadTemplate(): Observable<Blob> {
    return this.http.get(`${this.base}/excel-template`, { responseType: 'blob' });
  }

  getCostCenters(): Observable<CostCenterLookup[]> {
    const params = new HttpParams().set('page', 1).set('pageSize', 200);
    return this.http
      .get<CostCenterLookup[]>(`${environment.apiBaseUrl}/finance/cost-centers`, { params })
      .pipe(map(rows => rows ?? []));
  }

  getAccounts(): Observable<AccountLookup[]> {
    const params = new HttpParams().set('page', 1).set('pageSize', 200).set('isActive', 'true');
    return this.http
      .get<AccountLookup[]>(`${environment.apiBaseUrl}/finance/accounts`, { params })
      .pipe(map(rows => rows ?? []));
  }
}
