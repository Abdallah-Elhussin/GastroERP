import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  OrgBranchLookup,
  OrgCompanyLookup,
  TaxCode,
  TaxCodeListFilter,
  UpsertTaxCodePayload
} from '../models/tax-code.models';

@Injectable({ providedIn: 'root' })
export class TaxCodeRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/finance/tax-codes`;

  getList(options: TaxCodeListFilter = {}): Observable<TaxCode[]> {
    let params = new HttpParams()
      .set('page', '1')
      .set('pageSize', String(options.pageSize ?? 200));
    if (options.search?.trim()) params = params.set('search', options.search.trim());
    if (options.companyId) params = params.set('companyId', options.companyId);
    if (options.branchId) params = params.set('branchId', options.branchId);
    if (options.appliesTo != null) params = params.set('appliesTo', String(options.appliesTo));
    if (options.isActive != null) params = params.set('isActive', String(options.isActive));
    return this.http.get<TaxCode[]>(this.base, { params }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<TaxCode> {
    return this.http.get<TaxCode>(`${this.base}/${id}`);
  }

  create(payload: UpsertTaxCodePayload): Observable<TaxCode> {
    return this.http.post<TaxCode>(this.base, payload);
  }

  update(id: string, payload: UpsertTaxCodePayload): Observable<TaxCode> {
    return this.http.put<TaxCode>(`${this.base}/${id}`, payload);
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
