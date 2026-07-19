import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CostCenterLookup,
  CreateInventoryValuationGroupPayload,
  InventoryValuationGroup,
  UpdateInventoryValuationGroupPayload
} from '../models/inventory-valuation-group.models';

@Injectable({ providedIn: 'root' })
export class InventoryValuationGroupRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/inventory/valuation-groups`;

  getPaged(opts: {
    search?: string;
    isActive?: boolean | null;
    costCenterId?: string | null;
    sortBy?: string;
    sortDesc?: boolean;
    page?: number;
    pageSize?: number;
  } = {}): Observable<InventoryValuationGroup[]> {
    let params = new HttpParams()
      .set('page', String(opts.page ?? 1))
      .set('pageSize', String(opts.pageSize ?? 100));
    if (opts.search?.trim()) params = params.set('search', opts.search.trim());
    if (opts.isActive != null) params = params.set('isActive', String(opts.isActive));
    if (opts.costCenterId) params = params.set('costCenterId', opts.costCenterId);
    if (opts.sortBy) params = params.set('sortBy', opts.sortBy);
    if (opts.sortDesc) params = params.set('sortDesc', 'true');
    return this.http.get<InventoryValuationGroup[]>(this.base, { params });
  }

  create(payload: CreateInventoryValuationGroupPayload): Observable<InventoryValuationGroup> {
    return this.http.post<InventoryValuationGroup>(this.base, {
      tenantId: '00000000-0000-0000-0000-000000000000',
      ...payload
    });
  }

  update(id: string, payload: UpdateInventoryValuationGroupPayload): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, {
      tenantId: '00000000-0000-0000-0000-000000000000',
      ...payload
    });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  getCostCenters(): Observable<CostCenterLookup[]> {
    const params = new HttpParams().set('page', 1).set('pageSize', 200);
    return this.http
      .get<CostCenterLookup[]>(`${environment.apiBaseUrl}/finance/cost-centers`, { params })
      .pipe(map(rows => rows ?? []));
  }
}
