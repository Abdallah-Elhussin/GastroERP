import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateJournalPayload,
  JournalEntry,
  JournalListFilter,
  UpdateJournalPayload
} from '../models/journal-entry.models';
import {
  FiscalPeriodLookup,
  OrgBranchLookup,
  OrgCompanyLookup
} from '../models/financial-opening-balance.models';

@Injectable({ providedIn: 'root' })
export class JournalEntryRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/finance/journals`;

  getList(options: JournalListFilter = {}): Observable<JournalEntry[]> {
    let params = new HttpParams()
      .set('page', String(options.page ?? 1))
      .set('pageSize', String(options.pageSize ?? 200));
    if (options.search?.trim()) params = params.set('search', options.search.trim());
    if (options.status != null) params = params.set('status', String(options.status));
    if (options.sourceModule != null) params = params.set('sourceModule', String(options.sourceModule));
    if (options.voucherType != null) params = params.set('voucherType', String(options.voucherType));
    if (options.companyId) params = params.set('companyId', options.companyId);
    if (options.branchId) params = params.set('branchId', options.branchId);
    if (options.fiscalPeriodId) params = params.set('fiscalPeriodId', options.fiscalPeriodId);
    if (options.fiscalYear != null) params = params.set('fiscalYear', String(options.fiscalYear));
    if (options.entryNumber?.trim()) params = params.set('entryNumber', options.entryNumber.trim());
    if (options.fromDate) params = params.set('fromDate', options.fromDate);
    if (options.toDate) params = params.set('toDate', options.toDate);
    return this.http.get<JournalEntry[]>(this.base, { params }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<JournalEntry> {
    return this.http.get<JournalEntry>(`${this.base}/${id}`);
  }

  create(payload: CreateJournalPayload): Observable<JournalEntry> {
    return this.http.post<JournalEntry>(this.base, payload);
  }

  update(id: string, payload: UpdateJournalPayload): Observable<JournalEntry> {
    return this.http.put<JournalEntry>(`${this.base}/${id}`, payload);
  }

  approve(id: string): Observable<JournalEntry> {
    return this.http.post<JournalEntry>(`${this.base}/${id}/approve`, {});
  }

  post(id: string): Observable<JournalEntry> {
    return this.http.post<JournalEntry>(`${this.base}/${id}/post`, {});
  }

  reverse(id: string): Observable<JournalEntry> {
    return this.http.post<JournalEntry>(`${this.base}/${id}/reverse`, {});
  }

  delete(id: string): Observable<unknown> {
    return this.http.delete(`${this.base}/${id}`);
  }

  getCompanies(): Observable<OrgCompanyLookup[]> {
    const params = new HttpParams().set('page', '1').set('pageSize', '200');
    return this.http
      .get<OrgCompanyLookup[]>(`${environment.apiBaseUrl}/organization/companies`, { params })
      .pipe(map(r => r ?? []));
  }

  getBranches(): Observable<OrgBranchLookup[]> {
    const params = new HttpParams().set('page', '1').set('pageSize', '200');
    return this.http
      .get<OrgBranchLookup[]>(`${environment.apiBaseUrl}/organization/branches`, { params })
      .pipe(map(r => r ?? []));
  }

  getFiscalPeriods(): Observable<FiscalPeriodLookup[]> {
    return this.http
      .get<FiscalPeriodLookup[]>(`${environment.apiBaseUrl}/finance/fiscal-periods`)
      .pipe(map(r => (Array.isArray(r) ? r : [])));
  }
}
