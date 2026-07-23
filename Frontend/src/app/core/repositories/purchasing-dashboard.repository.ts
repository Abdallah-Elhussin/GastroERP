import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PurchasingDashboardSummary } from '../models/purchasing-dashboard.models';

@Injectable({ providedIn: 'root' })
export class PurchasingDashboardRepository {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/inventory/purchasing-dashboard`;

  getDashboard(): Observable<PurchasingDashboardSummary> {
    return this.http.get<PurchasingDashboardSummary>(this.base);
  }
}
