import { Observable } from 'rxjs';

export abstract class SettingsRepository {
  abstract getSettings(): Observable<any>;
  abstract updateSettings(settings: any): Observable<void>;
  abstract getRoles(): Observable<any[]>;
  abstract updateRolePermission(roleName: string, permission: string, granted: boolean): Observable<void>;
  abstract getAuditLogs(): Observable<any[]>;
}
