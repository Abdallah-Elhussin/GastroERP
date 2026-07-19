import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  FinancialOpeningBalance,
  FinancialOpeningBalanceListFilter,
  FiscalPeriodLookup,
  OrgBranchLookup,
  OrgCompanyLookup,
  UpsertFinancialOpeningBalancePayload
} from '../models/financial-opening-balance.models';

@Injectable({ providedIn: 'root' })
export class FinancialOpeningBalanceRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/finance/opening-balances`;

  getList(options: FinancialOpeningBalanceListFilter = {}): Observable<FinancialOpeningBalance[]> {
    let params = new HttpParams()
      .set('page', '1')
      .set('pageSize', String(options.pageSize ?? 200));
    if (options.search?.trim()) params = params.set('search', options.search.trim());
    if (options.companyId) params = params.set('companyId', options.companyId);
    if (options.branchId) params = params.set('branchId', options.branchId);
    if (options.fiscalPeriodId) params = params.set('fiscalPeriodId', options.fiscalPeriodId);
    if (options.status != null) params = params.set('status', String(options.status));
    return this.http
      .get<FinancialOpeningBalance[]>(this.base, { params })
      .pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<FinancialOpeningBalance> {
    return this.http.get<FinancialOpeningBalance>(`${this.base}/${id}`);
  }

  create(payload: UpsertFinancialOpeningBalancePayload): Observable<FinancialOpeningBalance> {
    return this.http.post<FinancialOpeningBalance>(this.base, payload);
  }

  update(id: string, payload: UpsertFinancialOpeningBalancePayload): Observable<FinancialOpeningBalance> {
    return this.http.put<FinancialOpeningBalance>(`${this.base}/${id}`, payload);
  }

  post(id: string): Observable<FinancialOpeningBalance> {
    return this.http.post<FinancialOpeningBalance>(`${this.base}/${id}/post`, {});
  }

  reverse(id: string): Observable<FinancialOpeningBalance> {
    return this.http.post<FinancialOpeningBalance>(`${this.base}/${id}/reverse`, {});
  }

  delete(id: string): Observable<unknown> {
    return this.http.delete(`${this.base}/${id}`);
  }

  getCompanies(): Observable<OrgCompanyLookup[]> {
    const params = new HttpParams().set('page', '1').set('pageSize', '200');
    return this.http
      .get<OrgCompanyLookup[]>(`${environment.apiBaseUrl}/organization/companies`, { params })
      .pipe(map(r => r ?? []));
  }

  getBranches(companyId?: string | null): Observable<OrgBranchLookup[]> {
    let params = new HttpParams().set('page', '1').set('pageSize', '200');
    if (companyId) params = params.set('companyId', companyId);
    return this.http
      .get<OrgBranchLookup[]>(`${environment.apiBaseUrl}/organization/branches`, { params })
      .pipe(map(r => r ?? []));
  }

  getFiscalPeriods(): Observable<FiscalPeriodLookup[]> {
    const params = new HttpParams().set('page', '1').set('pageSize', '200');
    return this.http
      .get<FiscalPeriodLookup[]>(`${environment.apiBaseUrl}/finance/fiscal-periods`, { params })
      .pipe(map(r => r ?? []));
  }
}
