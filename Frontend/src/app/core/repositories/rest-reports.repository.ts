import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ReportsRepository } from './reports.repository';

@Injectable({
  providedIn: 'root'
})
export class RestReportsRepository extends ReportsRepository {
  private http = inject(HttpClient);
  private apiUrl = '/api/reports';

  getReportsKPIs(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/kpis`);
  }

  getPivotSalesData(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/pivot-sales`);
  }

  scheduleReport(payload: any): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/schedule`, payload);
  }
}
