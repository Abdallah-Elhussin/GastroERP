import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SettingsRepository } from './settings.repository';

@Injectable({
  providedIn: 'root'
})
export class RestSettingsRepository extends SettingsRepository {
  private http = inject(HttpClient);
  private apiUrl = '/api/settings';

  getSettings(): Observable<any> {
    return this.http.get<any>(this.apiUrl);
  }

  updateSettings(settings: any): Observable<void> {
    return this.http.post<void>(this.apiUrl, settings);
  }

  getRoles(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/roles`);
  }

  updateRolePermission(roleName: string, permission: string, granted: boolean): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/roles/permissions`, { roleName, permission, granted });
  }

  getAuditLogs(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/audit-logs`);
  }
}
