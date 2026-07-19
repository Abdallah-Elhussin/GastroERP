import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Branch,
  BranchListFilter,
  OrgCompanyLookup,
  UpsertBranchPayload
} from '../models/branch.models';

@Injectable({ providedIn: 'root' })
export class BranchRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/organization/branches`;

  getList(options: BranchListFilter = {}): Observable<Branch[]> {
    let params = new HttpParams()
      .set('page', '1')
      .set('pageSize', String(options.pageSize ?? 200));
    if (options.search?.trim()) params = params.set('search', options.search.trim());
    if (options.companyId) params = params.set('companyId', options.companyId);
    if (options.isActive != null) params = params.set('isActive', String(options.isActive));
    return this.http.get<Branch[]>(this.base, { params }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<Branch> {
    return this.http.get<Branch>(`${this.base}/${id}`);
  }

  create(payload: UpsertBranchPayload): Observable<Branch> {
    return this.http.post<Branch>(this.base, payload);
  }

  update(id: string, payload: UpsertBranchPayload): Observable<Branch> {
    return this.http.put<Branch>(`${this.base}/${id}`, {
      companyId: payload.companyId,
      nameAr: payload.nameAr,
      nameEn: payload.nameEn,
      code: payload.code,
      location: payload.location,
      isActive: payload.isActive
    });
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
}
