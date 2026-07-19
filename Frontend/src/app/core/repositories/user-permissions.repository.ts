import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export enum UserPermissionEffect {
  Allow = 1,
  Deny = 2
}

export interface UserPermissionOverrideDto {
  permissionId: string;
  effect: UserPermissionEffect;
}

export interface UserPermissionsStateDto {
  userId: string;
  userName: string;
  fullName: string;
  rolePermissionIds: string[];
  overrides: UserPermissionOverrideDto[];
  effectivePermissionIds: string[];
  roleNames: string[];
}

@Injectable({ providedIn: 'root' })
export class UserPermissionsRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/identity/users`;

  getUserPermissionsState(userId: string): Observable<UserPermissionsStateDto> {
    return this.http.get<UserPermissionsStateDto>(`${this.base}/${userId}/permissions`);
  }

  replaceUserPermissions(userId: string, permissionIds: string[]): Observable<unknown> {
    return this.http.put(`${this.base}/${userId}/permissions`, permissionIds);
  }

  clearUserPermissionOverrides(userId: string): Observable<unknown> {
    return this.http.delete(`${this.base}/${userId}/permissions/overrides`);
  }
}
