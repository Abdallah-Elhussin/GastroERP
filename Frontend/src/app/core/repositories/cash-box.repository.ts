import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CashBox,
  CashBoxListFilter,
  OrgBranchLookup,
  OrgCompanyLookup,
  OrgDeviceLookup,
  UpsertCashBoxPayload,
  UserLookup
} from '../models/cash-box.models';

@Injectable({ providedIn: 'root' })
export class CashBoxRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/finance/cash-boxes`;

  getList(options: CashBoxListFilter = {}): Observable<CashBox[]> {
    let params = new HttpParams()
      .set('page', '1')
      .set('pageSize', String(options.pageSize ?? 200));
    if (options.search?.trim()) params = params.set('search', options.search.trim());
    if (options.companyId) params = params.set('companyId', options.companyId);
    if (options.branchId) params = params.set('branchId', options.branchId);
    if (options.currencyId) params = params.set('currencyId', options.currencyId);
    if (options.isActive != null) params = params.set('isActive', String(options.isActive));
    return this.http.get<CashBox[]>(this.base, { params }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<CashBox> {
    return this.http.get<CashBox>(`${this.base}/${id}`);
  }

  create(payload: UpsertCashBoxPayload): Observable<CashBox> {
    return this.http.post<CashBox>(this.base, payload);
  }

  update(id: string, payload: UpsertCashBoxPayload): Observable<CashBox> {
    return this.http.put<CashBox>(`${this.base}/${id}`, payload);
  }

  activate(id: string): Observable<unknown> {
    return this.http.post(`${this.base}/${id}/activate`, {});
  }

  deactivate(id: string): Observable<unknown> {
    return this.http.post(`${this.base}/${id}/deactivate`, {});
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

  getDevices(): Observable<OrgDeviceLookup[]> {
    const params = new HttpParams().set('page', '1').set('pageSize', '200');
    return this.http
      .get<OrgDeviceLookup[]>(`${environment.apiBaseUrl}/organization/devices`, { params })
      .pipe(map(r => r ?? []));
  }

  getUsers(): Observable<UserLookup[]> {
    const params = new HttpParams().set('pageNumber', '1').set('pageSize', '200');
    return this.http
      .get<UserLookup[] | { items?: UserLookup[] }>(`${environment.apiBaseUrl}/identity/users`, { params })
      .pipe(
        map(r => {
          if (Array.isArray(r)) return r;
          return r?.items ?? [];
        })
      );
  }
}
