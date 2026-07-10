import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError, switchMap } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { TelemetryService } from '../services/telemetry.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const telemetry = inject(TelemetryService);
  const token = authService.getToken();

  let authReq = req;
  if (token) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // Catch unauthorized 401 and try rotating token
      if (error.status === 401) {
        return authService.refreshToken().pipe(
          switchMap((res) => {
            authService.setToken(res.token);
            const retryReq = req.clone({
              setHeaders: {
                Authorization: `Bearer ${res.token}`
              }
            });
            return next(retryReq);
          }),
          catchError((refreshErr) => {
            authService.logout();
            return throwError(() => new Error('Session expired. Please log in again.'));
          })
        );
      }

      // Global HTTP Error handler for other status codes
      let errorMessage = 'An unknown network error occurred.';
      if (error.error instanceof ErrorEvent) {
        errorMessage = `Client Error: ${error.error.message}`;
      } else {
        switch (error.status) {
          case 403:
            errorMessage = 'Access denied. You do not have permissions for this action.';
            router.navigate(['/error'], { queryParams: { code: '403' } });
            break;
          case 404:
            errorMessage = 'Requested API resource not found.';
            break;
          case 500:
            errorMessage = 'Internal server error occurred on Gastro backend.';
            break;
        }
      }

      telemetry.logError(errorMessage, error);
      return throwError(() => new Error(errorMessage));
    })
  );
};
