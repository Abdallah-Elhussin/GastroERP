import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { IssueDestination, UpsertIssueDestinationPayload } from '../models/issue-destination.models';
import { CostCenterLookup } from '../models/inventory-valuation-group.models';
import { AccountLookup } from '../models/opening-balance.models';

@Injectable({ providedIn: 'root' })
export class IssueDestinationRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/inventory/issue-destinations`;

  getList(options: { activeOnly?: boolean; search?: string; destinationType?: number | null } = {}): Observable<IssueDestination[]> {
    let params = new HttpParams().set('activeOnly', options.activeOnly ?? false);
    if (options.search) params = params.set('search', options.search);
    if (options.destinationType != null) params = params.set('destinationType', options.destinationType);
    return this.http.get<IssueDestination[]>(this.base, { params }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<IssueDestination> {
    return this.http.get<IssueDestination>(`${this.base}/${id}`);
  }

  create(payload: UpsertIssueDestinationPayload & { code: string }): Observable<IssueDestination> {
    return this.http.post<IssueDestination>(this.base, {
      tenantId: '00000000-0000-0000-0000-000000000000',
      ...payload
    });
  }

  update(id: string, payload: UpsertIssueDestinationPayload): Observable<IssueDestination> {
    return this.http.put<IssueDestination>(`${this.base}/${id}`, {
      tenantId: '00000000-0000-0000-0000-000000000000',
      isActive: true,
      ...payload
    });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  activate(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/activate`, {});
  }

  deactivate(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/deactivate`, {});
  }

  getCostCenters(): Observable<CostCenterLookup[]> {
    const params = new HttpParams().set('page', 1).set('pageSize', 200);
    return this.http
      .get<CostCenterLookup[]>(`${environment.apiBaseUrl}/finance/cost-centers`, { params })
      .pipe(map(rows => rows ?? []));
  }

  getAccounts(): Observable<AccountLookup[]> {
    const params = new HttpParams().set('page', 1).set('pageSize', 200).set('isActive', 'true');
    return this.http
      .get<AccountLookup[]>(`${environment.apiBaseUrl}/finance/accounts`, { params })
      .pipe(map(rows => rows ?? []));
  }
}
