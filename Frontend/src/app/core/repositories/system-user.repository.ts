import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  BranchLookup,
  RoleLookup,
  SystemUser,
  SystemUserListFilter,
  UpsertSystemUserPayload,
  UserLicenseStatus
} from '../models/system-user.models';

@Injectable({ providedIn: 'root' })
export class SystemUserRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/identity/users`;
  private readonly rolesBase = `${environment.apiBaseUrl}/identity/roles`;

  getList(options: SystemUserListFilter = {}): Observable<SystemUser[]> {
    let params = new HttpParams()
      .set('pageNumber', String(options.pageNumber ?? 1))
      .set('pageSize', String(options.pageSize ?? 200));
    if (options.search?.trim()) params = params.set('searchTerm', options.search.trim());
    if (options.branchId) params = params.set('branchId', options.branchId);
    if (options.roleId) params = params.set('roleId', options.roleId);
    if (options.isActive != null) params = params.set('isActive', String(options.isActive));
    return this.http.get<SystemUser[]>(this.base, { params }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<SystemUser> {
    return this.http.get<SystemUser>(`${this.base}/${id}`);
  }

  getLicenseStatus(): Observable<UserLicenseStatus> {
    return this.http.get<UserLicenseStatus>(`${this.base}/license-status`);
  }

  create(payload: UpsertSystemUserPayload): Observable<string> {
    return this.http.post<string>(this.base, payload);
  }

  update(id: string, payload: UpsertSystemUserPayload): Observable<unknown> {
    return this.http.put(`${this.base}/${id}`, payload);
  }

  delete(id: string): Observable<unknown> {
    return this.http.delete(`${this.base}/${id}`);
  }

  lock(id: string): Observable<unknown> {
    return this.http.post(`${this.base}/${id}/lock`, {});
  }

  unlock(id: string): Observable<unknown> {
    return this.http.post(`${this.base}/${id}/unlock`, {});
  }

  resetPassword(id: string, newPassword: string): Observable<unknown> {
    return this.http.post(`${this.base}/${id}/reset-password`, { newPassword });
  }

  getRoles(): Observable<RoleLookup[]> {
    return this.http.get<RoleLookup[]>(this.rolesBase).pipe(map(r => r ?? []));
  }

  getBranches(): Observable<BranchLookup[]> {
    const params = new HttpParams().set('page', '1').set('pageSize', '200');
    return this.http
      .get<BranchLookup[]>(`${environment.apiBaseUrl}/organization/branches`, { params })
      .pipe(map(r => r ?? []));
  }
}
