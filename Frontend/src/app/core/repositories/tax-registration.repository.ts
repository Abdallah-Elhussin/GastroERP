import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  OrgBranchLookup,
  OrgCompanyLookup,
  TaxRegistrationListFilter,
  TaxRegistrationProfile,
  UpsertTaxRegistrationPayload
} from '../models/tax-registration.models';

@Injectable({ providedIn: 'root' })
export class TaxRegistrationRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/finance/tax-registrations`;

  getList(options: TaxRegistrationListFilter = {}): Observable<TaxRegistrationProfile[]> {
    let params = new HttpParams()
      .set('page', '1')
      .set('pageSize', String(options.pageSize ?? 200));
    if (options.search?.trim()) params = params.set('search', options.search.trim());
    if (options.companyId) params = params.set('companyId', options.companyId);
    if (options.branchId) params = params.set('branchId', options.branchId);
    if (options.status != null) params = params.set('status', String(options.status));
    return this.http.get<TaxRegistrationProfile[]>(this.base, { params }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<TaxRegistrationProfile> {
    return this.http.get<TaxRegistrationProfile>(`${this.base}/${id}`);
  }

  create(payload: UpsertTaxRegistrationPayload): Observable<TaxRegistrationProfile> {
    return this.http.post<TaxRegistrationProfile>(this.base, payload);
  }

  update(id: string, payload: UpsertTaxRegistrationPayload): Observable<TaxRegistrationProfile> {
    return this.http.put<TaxRegistrationProfile>(`${this.base}/${id}`, payload);
  }

  delete(id: string): Observable<unknown> {
    return this.http.delete(`${this.base}/${id}`);
  }

  uploadCertificate(
    id: string,
    file: File,
    meta: {
      documentNumber?: string | null;
      issueDate?: string | null;
      expiryDate?: string | null;
      notes?: string | null;
    } = {}
  ): Observable<TaxRegistrationProfile> {
    const form = new FormData();
    form.append('file', file, file.name);
    if (meta.documentNumber) form.append('documentNumber', meta.documentNumber);
    if (meta.issueDate) form.append('issueDate', meta.issueDate);
    if (meta.expiryDate) form.append('expiryDate', meta.expiryDate);
    if (meta.notes) form.append('notes', meta.notes);
    return this.http.post<TaxRegistrationProfile>(`${this.base}/${id}/certificate`, form);
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
