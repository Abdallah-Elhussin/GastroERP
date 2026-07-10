import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { DashboardRepository } from './dashboard.repository';

@Injectable({
  providedIn: 'root'
})
export class RestDashboardRepository extends DashboardRepository {
  private http = inject(HttpClient);
  private apiUrl = '/api/v1/reporting/dashboards';

  getSalesOverview(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/sales-overview`).pipe(
      catchError(() => of(null))
    );
  }

  getWidgetPermissions(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/widget-permissions`).pipe(
      catchError(() => of({ permissions: [] }))
    );
  }
}
