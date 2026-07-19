import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BackOfficeSalesReport, BackOfficeSalesReportParams } from '../models/back-office-sales-report.models';

@Injectable({ providedIn: 'root' })
export class BackOfficeSalesReportRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/back-office-sales/reports`;

  getReport(params: BackOfficeSalesReportParams = {}): Observable<BackOfficeSalesReport> {
    let httpParams = new HttpParams();
    if (params.from) httpParams = httpParams.set('from', params.from);
    if (params.to) httpParams = httpParams.set('to', params.to);
    if (params.customerId) httpParams = httpParams.set('customerId', params.customerId);
    if (params.branchId) httpParams = httpParams.set('branchId', params.branchId);
    if (params.topCustomers != null) httpParams = httpParams.set('topCustomers', params.topCustomers);
    if (params.topItems != null) httpParams = httpParams.set('topItems', params.topItems);
    return this.http.get<BackOfficeSalesReport>(this.base, { params: httpParams });
  }
}
