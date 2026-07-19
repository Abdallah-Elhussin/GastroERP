import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { FiscalPeriod, UpsertFiscalPeriodPayload } from '../models/fiscal-period.models';

@Injectable({ providedIn: 'root' })
export class FiscalPeriodRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/finance/fiscal-periods`;

  getList(options: { search?: string; status?: number | null } = {}): Observable<FiscalPeriod[]> {
    let params = new HttpParams();
    if (options.search?.trim()) params = params.set('search', options.search.trim());
    if (options.status != null) params = params.set('status', String(options.status));
    return this.http.get<FiscalPeriod[]>(this.base, { params }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<FiscalPeriod> {
    return this.http.get<FiscalPeriod>(`${this.base}/${id}`);
  }

  create(payload: UpsertFiscalPeriodPayload): Observable<FiscalPeriod> {
    return this.http.post<FiscalPeriod>(this.base, {
      fiscalYear: payload.fiscalYear,
      startMonth: payload.startMonth,
      notes: payload.notes,
      periodPolicy: payload.periodPolicy ?? 1,
      generateDetails: payload.generateDetails ?? true
    });
  }

  update(id: string, payload: UpsertFiscalPeriodPayload): Observable<FiscalPeriod> {
    return this.http.put<FiscalPeriod>(`${this.base}/${id}`, {
      startMonth: payload.startMonth,
      notes: payload.notes,
      details: payload.details
    });
  }

  delete(id: string): Observable<unknown> {
    return this.http.delete(`${this.base}/${id}`);
  }

  generateDetails(id: string): Observable<FiscalPeriod> {
    return this.http.post<FiscalPeriod>(`${this.base}/${id}/generate-details`, {});
  }
}
