import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PermissionDto, RoleDto, UpsertRolePayload } from '../models/role-permissions.models';

@Injectable({ providedIn: 'root' })
export class RolePermissionsRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/identity/roles`;

  getRoles(): Observable<RoleDto[]> {
    return this.http.get<RoleDto[]>(this.base).pipe(map(r => r ?? []));
  }

  createRole(payload: UpsertRolePayload): Observable<string> {
    return this.http.post<string>(this.base, payload);
  }

  updateRole(id: string, payload: UpsertRolePayload): Observable<unknown> {
    return this.http.put(`${this.base}/${id}`, payload);
  }

  deleteRole(id: string): Observable<unknown> {
    return this.http.delete(`${this.base}/${id}`);
  }

  copyRole(id: string, name?: string, nameAr?: string): Observable<string> {
    return this.http.post<string>(`${this.base}/${id}/copy`, { name, nameAr });
  }

  getPermissions(): Observable<PermissionDto[]> {
    return this.http.get<PermissionDto[]>(`${this.base}/permissions`).pipe(map(r => r ?? []));
  }

  getRolePermissionIds(roleId: string): Observable<string[]> {
    return this.http.get<string[]>(`${this.base}/${roleId}/permissions`).pipe(map(r => r ?? []));
  }

  replaceRolePermissions(roleId: string, permissionIds: string[]): Observable<unknown> {
    return this.http.put(`${this.base}/${roleId}/permissions`, permissionIds);
  }
}
