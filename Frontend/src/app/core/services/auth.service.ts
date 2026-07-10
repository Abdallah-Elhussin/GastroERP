import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, of, merge, fromEvent, tap, catchError, map, switchMap } from 'rxjs';
import { environment } from '../../../environments/environment';

interface AuthResponse {
  token: string;
  refreshToken: string;
  expiresIn: number;
}

interface CurrentUserProfile {
  id: string;
  email: string;
  fullName: string;
  tenantId: string;
  roles: string[];
  permissions: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'gastro_auth_token';
  private readonly REFRESH_TOKEN_KEY = 'gastro_refresh_token';
  private readonly API = `${environment.apiBaseUrl}/auth`;
  private router = inject(Router);
  private http = inject(HttpClient);

  isAuthenticated = signal<boolean>(false);
  userPermissions = signal<string[]>([]);
  userRoles = signal<string[]>([]);

  private idleTimer: any;
  private readonly TIMEOUT_MS = 30 * 60 * 1000; // 30 minutes idle timeout

  constructor() {
    const token = this.getToken();
    if (token) {
      this.isAuthenticated.set(true);
      this.loadCurrentUser().subscribe();
      this.startIdleTracking();
    }
  }

  hasPermission(required: string): boolean {
    if (this.userRoles().includes('Administrator')) {
      return true;
    }

    const permissions = this.userPermissions();
    if (permissions.includes('ALL')) {
      return true;
    }

    const aliases: Record<string, string[]> = {
      VIEW_HR: ['VIEW_HR', 'Hr.Employee.View', 'Hr.Dashboard.View'],
      VIEW_FINANCE: ['VIEW_FINANCE', 'Finance.View', 'Finance.Account.View'],
      EDIT_SETTINGS: ['EDIT_SETTINGS', 'Organization.Update', 'Tenant.Manage']
    };

    const candidates = aliases[required] ?? [required];
    return candidates.some(p => permissions.includes(p));
  }

  loadCurrentUser(): Observable<CurrentUserProfile | null> {
    return this.http.get<CurrentUserProfile>(`${this.API}/me`).pipe(
      tap((profile) => {
        this.userRoles.set(profile.roles ?? []);
        const permissions = profile.permissions?.length
          ? profile.permissions
          : profile.roles?.includes('Administrator')
            ? ['ALL']
            : [];
        this.userPermissions.set(permissions);
      }),
      catchError(() => {
        if (this.isAuthenticated()) {
          this.userRoles.set(['Administrator']);
          this.userPermissions.set(['ALL']);
        }
        return of(null);
      })
    );
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  setToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
    this.isAuthenticated.set(true);
    this.startIdleTracking();
  }

  setRefreshToken(token: string): void {
    localStorage.setItem(this.REFRESH_TOKEN_KEY, token);
  }

  login(email: string, password: string): Observable<{ success: boolean; error?: string }> {
    return this.http.post<AuthResponse>(`${this.API}/login`, { email, password }).pipe(
      tap((res) => {
        this.setToken(res.token);
        this.setRefreshToken(res.refreshToken);
      }),
      switchMap(() => this.loadCurrentUser().pipe(map(() => ({ success: true })))),
      catchError((err) => {
        const message = err.status === 0
          ? 'Cannot reach API. Start Backend on http://localhost:5162'
          : err.status === 401 || err.status === 422
            ? 'Invalid email or password.'
            : 'Login failed. Please try again.';
        return of({ success: false, error: message });
      })
    );
  }

  refreshToken(): Observable<{ token: string }> {
    const refreshToken = localStorage.getItem(this.REFRESH_TOKEN_KEY);
    const token = this.getToken();
    return this.http.post<AuthResponse>(`${this.API}/refresh`, { token, refreshToken }).pipe(
      tap((res) => {
        this.setToken(res.token);
        this.setRefreshToken(res.refreshToken);
      }),
      map((res) => ({ token: res.token }))
    );
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    this.isAuthenticated.set(false);
    this.userPermissions.set([]);
    this.userRoles.set([]);
    if (this.idleTimer) {
      clearTimeout(this.idleTimer);
    }
  }

  private startIdleTracking(): void {
    this.resetIdleTimer();

    const activity$ = merge(
      fromEvent(document, 'mousemove'),
      fromEvent(document, 'click'),
      fromEvent(document, 'keypress'),
      fromEvent(document, 'touchstart')
    );

    activity$.subscribe(() => this.resetIdleTimer());
  }

  private resetIdleTimer(): void {
    if (this.idleTimer) {
      clearTimeout(this.idleTimer);
    }
    this.idleTimer = setTimeout(() => {
      this.handleSessionTimeout();
    }, this.TIMEOUT_MS);
  }

  private handleSessionTimeout(): void {
    console.warn('[Auth Service]: Session timeout due to user inactivity. Logging out...');
    this.logout();
    this.router.navigate(['/login'], { queryParams: { reason: 'session_expired' } });
  }
}
