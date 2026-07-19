import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  GeneralLedgerSetting,
  GeneralLedgerSettingListFilter,
  OrgBranchLookup,
  OrgCompanyLookup,
  UpsertGeneralLedgerSettingPayload
} from '../models/general-ledger-setting.models';

@Injectable({ providedIn: 'root' })
export class GeneralLedgerSettingRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/finance/general-ledger-settings`;

  getList(options: GeneralLedgerSettingListFilter = {}): Observable<GeneralLedgerSetting[]> {
    let params = new HttpParams()
      .set('page', '1')
      .set('pageSize', String(options.pageSize ?? 200));
    if (options.search?.trim()) params = params.set('search', options.search.trim());
    if (options.companyId) params = params.set('companyId', options.companyId);
    if (options.branchId) params = params.set('branchId', options.branchId);
    return this.http.get<GeneralLedgerSetting[]>(this.base, { params }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<GeneralLedgerSetting> {
    return this.http.get<GeneralLedgerSetting>(`${this.base}/${id}`);
  }

  create(payload: UpsertGeneralLedgerSettingPayload): Observable<GeneralLedgerSetting> {
    return this.http.post<GeneralLedgerSetting>(this.base, payload);
  }

  update(id: string, payload: UpsertGeneralLedgerSettingPayload): Observable<GeneralLedgerSetting> {
    return this.http.put<GeneralLedgerSetting>(`${this.base}/${id}`, payload);
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

  getBranches(companyId?: string | null): Observable<OrgBranchLookup[]> {
    let params = new HttpParams().set('page', '1').set('pageSize', '200');
    if (companyId) params = params.set('companyId', companyId);
    return this.http
      .get<OrgBranchLookup[]>(`${environment.apiBaseUrl}/organization/branches`, { params })
      .pipe(map(r => r ?? []));
  }
}
