import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateGoodsIssuePayload,
  GoodsIssueDoc,
  GoodsIssueListParams,
  IssueDestination,
  UpdateGoodsIssuePayload,
  UpsertIssueDestinationPayload
} from '../models/goods-issue.models';
import { CostCenterLookup } from '../models/inventory-valuation-group.models';

@Injectable({ providedIn: 'root' })
export class GoodsIssueRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/inventory/goods-issues`;
  private readonly destBase = `${environment.apiBaseUrl}/inventory/issue-destinations`;

  getList(params: GoodsIssueListParams = {}): Observable<GoodsIssueDoc[]> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 50);
    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.status != null) httpParams = httpParams.set('status', params.status);
    if (params.from) httpParams = httpParams.set('from', params.from);
    if (params.to) httpParams = httpParams.set('to', params.to);
    return this.http.get<GoodsIssueDoc[]>(this.base, { params: httpParams }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<GoodsIssueDoc> {
    return this.http.get<GoodsIssueDoc>(`${this.base}/${id}`);
  }

  nextNumber(): Observable<string> {
    return this.http.post<string>(`${this.base}/next-number`, {});
  }

  create(payload: CreateGoodsIssuePayload): Observable<GoodsIssueDoc> {
    return this.http.post<GoodsIssueDoc>(this.base, {
      tenantId: '00000000-0000-0000-0000-000000000000',
      ...payload
    });
  }

  update(id: string, payload: UpdateGoodsIssuePayload): Observable<GoodsIssueDoc> {
    return this.http.put<GoodsIssueDoc>(`${this.base}/${id}`, {
      tenantId: '00000000-0000-0000-0000-000000000000',
      ...payload
    });
  }

  approve(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/approve`, {});
  }

  unapprove(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/unapprove`, {});
  }

  post(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/post`, {});
  }

  cancel(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/cancel`, {});
  }

  getDestinations(activeOnly = true): Observable<IssueDestination[]> {
    const params = new HttpParams().set('activeOnly', activeOnly);
    return this.http.get<IssueDestination[]>(this.destBase, { params }).pipe(map(r => r ?? []));
  }

  createDestination(payload: UpsertIssueDestinationPayload & { code: string }): Observable<IssueDestination> {
    return this.http.post<IssueDestination>(this.destBase, {
      tenantId: '00000000-0000-0000-0000-000000000000',
      ...payload
    });
  }

  updateDestination(id: string, payload: UpsertIssueDestinationPayload): Observable<IssueDestination> {
    return this.http.put<IssueDestination>(`${this.destBase}/${id}`, {
      tenantId: '00000000-0000-0000-0000-000000000000',
      isActive: true,
      ...payload
    });
  }

  deleteDestination(id: string): Observable<void> {
    return this.http.delete<void>(`${this.destBase}/${id}`);
  }

  getCostCenters(): Observable<CostCenterLookup[]> {
    const params = new HttpParams().set('page', 1).set('pageSize', 200);
    return this.http
      .get<CostCenterLookup[]>(`${environment.apiBaseUrl}/finance/cost-centers`, { params })
      .pipe(map(rows => rows ?? []));
  }
}
