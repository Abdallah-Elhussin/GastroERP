import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Bank,
  BankListFilter,
  OrgBranchLookup,
  OrgCompanyLookup,
  UpsertBankPayload
} from '../models/bank.models';

@Injectable({ providedIn: 'root' })
export class BankRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/finance/banks`;

  getList(options: BankListFilter = {}): Observable<Bank[]> {
    let params = new HttpParams()
      .set('page', '1')
      .set('pageSize', String(options.pageSize ?? 200));
    if (options.search?.trim()) params = params.set('search', options.search.trim());
    if (options.companyId) params = params.set('companyId', options.companyId);
    if (options.branchId) params = params.set('branchId', options.branchId);
    if (options.currencyId) params = params.set('currencyId', options.currencyId);
    if (options.isActive != null) params = params.set('isActive', String(options.isActive));
    return this.http.get<Bank[]>(this.base, { params }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<Bank> {
    return this.http.get<Bank>(`${this.base}/${id}`);
  }

  create(payload: UpsertBankPayload): Observable<Bank> {
    return this.http.post<Bank>(this.base, payload);
  }

  update(id: string, payload: UpsertBankPayload): Observable<Bank> {
    return this.http.put<Bank>(`${this.base}/${id}`, payload);
  }

  activate(id: string): Observable<unknown> {
    return this.http.post(`${this.base}/${id}/activate`, {});
  }

  deactivate(id: string, body?: { deactivatedAt?: string | null; reason?: string | null }): Observable<unknown> {
    return this.http.post(`${this.base}/${id}/deactivate`, body ?? {});
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
}
