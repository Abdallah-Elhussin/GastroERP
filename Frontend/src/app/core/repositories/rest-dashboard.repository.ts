import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { DashboardRepository } from './dashboard.repository';
import {
  EnterpriseDashboardActivities,
  EnterpriseDashboardCustomers,
  EnterpriseDashboardDelivery,
  EnterpriseDashboardFilter,
  EnterpriseDashboardFinance,
  EnterpriseDashboardHr,
  EnterpriseDashboardInventory,
  EnterpriseDashboardKitchen,
  EnterpriseDashboardOverview,
  EnterpriseDashboardProducts,
  EnterpriseDashboardSales
} from '../models/enterprise-dashboard.models';

@Injectable({ providedIn: 'root' })
export class RestDashboardRepository extends DashboardRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/dashboard`;

  getOverview(filter: EnterpriseDashboardFilter): Observable<EnterpriseDashboardOverview | null> {
    return this.http
      .get<any>(`${this.base}/overview`, { params: this.toParams(filter) })
      .pipe(
        map(r => normalizeOverview(r)),
        catchError(() => of(null))
      );
  }

  getSales(filter: EnterpriseDashboardFilter): Observable<EnterpriseDashboardSales | null> {
    return this.http
      .get<any>(`${this.base}/sales`, { params: this.toParams(filter) })
      .pipe(
        map(r => unwrapData<EnterpriseDashboardSales>(r)),
        catchError(() => of(null))
      );
  }

  getProducts(filter: EnterpriseDashboardFilter): Observable<EnterpriseDashboardProducts | null> {
    return this.http
      .get<any>(`${this.base}/products`, { params: this.toParams(filter) })
      .pipe(
        map(r => unwrapData<EnterpriseDashboardProducts>(r)),
        catchError(() => of(null))
      );
  }

  getCustomers(filter: EnterpriseDashboardFilter): Observable<EnterpriseDashboardCustomers | null> {
    return this.http
      .get<any>(`${this.base}/customers`, { params: this.toParams(filter) })
      .pipe(
        map(r => unwrapData<EnterpriseDashboardCustomers>(r)),
        catchError(() => of(null))
      );
  }

  getInventory(filter: EnterpriseDashboardFilter): Observable<EnterpriseDashboardInventory | null> {
    return this.http
      .get<any>(`${this.base}/inventory`, { params: this.toParams(filter) })
      .pipe(
        map(r => unwrapData<EnterpriseDashboardInventory>(r)),
        catchError(() => of(null))
      );
  }

  getFinance(filter: EnterpriseDashboardFilter): Observable<EnterpriseDashboardFinance | null> {
    return this.http
      .get<any>(`${this.base}/finance`, { params: this.toParams(filter) })
      .pipe(
        map(r => unwrapData<EnterpriseDashboardFinance>(r)),
        catchError(() => of(null))
      );
  }

  getKitchen(filter: EnterpriseDashboardFilter): Observable<EnterpriseDashboardKitchen | null> {
    return this.http
      .get<any>(`${this.base}/kitchen`, { params: this.toParams(filter) })
      .pipe(
        map(r => unwrapData<EnterpriseDashboardKitchen>(r)),
        catchError(() => of(null))
      );
  }

  getDelivery(filter: EnterpriseDashboardFilter): Observable<EnterpriseDashboardDelivery | null> {
    return this.http
      .get<any>(`${this.base}/delivery`, { params: this.toParams(filter) })
      .pipe(
        map(r => unwrapData<EnterpriseDashboardDelivery>(r)),
        catchError(() => of(null))
      );
  }

  getHr(filter: EnterpriseDashboardFilter): Observable<EnterpriseDashboardHr | null> {
    return this.http
      .get<any>(`${this.base}/hr`, { params: this.toParams(filter) })
      .pipe(
        map(r => unwrapData<EnterpriseDashboardHr>(r)),
        catchError(() => of(null))
      );
  }

  getActivities(filter: EnterpriseDashboardFilter): Observable<EnterpriseDashboardActivities | null> {
    return this.http
      .get<any>(`${this.base}/activities`, { params: this.toParams(filter) })
      .pipe(
        map(r => unwrapData<EnterpriseDashboardActivities>(r)),
        catchError(() => of(null))
      );
  }

  private toParams(filter: EnterpriseDashboardFilter): HttpParams {
    let p = new HttpParams();
    if (filter.period != null) p = p.set('period', filter.period);
    if (filter.fromDate) p = p.set('fromDate', filter.fromDate);
    if (filter.toDate) p = p.set('toDate', filter.toDate);
    if (filter.branchId) p = p.set('branchId', filter.branchId);
    if (filter.warehouseId) p = p.set('warehouseId', filter.warehouseId);
    if (filter.cashierId) p = p.set('cashierId', filter.cashierId);
    if (filter.companyId) p = p.set('companyId', filter.companyId);
    if (filter.currency) p = p.set('currency', filter.currency);
    return p;
  }
}

function unwrapData<T>(res: any): T | null {
  if (!res) return null;
  // Prefer unwrapped payload when present and non-null.
  if (res.data != null && typeof res.data === 'object') return res.data as T;
  return res as T;
}

function normalizeOverview(raw: any): EnterpriseDashboardOverview | null {
  const o = unwrapData<any>(raw);
  if (!o) return null;
  const header = o.header ?? o.Header;
  if (!header) return null;
  return {
    header: {
      companyName: header.companyName ?? header.CompanyName ?? '',
      branchName: header.branchName ?? header.BranchName ?? null,
      userName: header.userName ?? header.UserName ?? '',
      serverTime: header.serverTime ?? header.ServerTime ?? new Date().toISOString(),
      lastSyncedAt: header.lastSyncedAt ?? header.LastSyncedAt ?? new Date().toISOString(),
      currency: header.currency ?? header.Currency ?? 'SAR'
    },
    kpis: o.kpis ?? o.Kpis ?? [],
    notifications: o.notifications ?? o.Notifications ?? [],
    insights: o.insights ?? o.Insights ?? [],
    quickActions: o.quickActions ?? o.QuickActions ?? []
  };
}
