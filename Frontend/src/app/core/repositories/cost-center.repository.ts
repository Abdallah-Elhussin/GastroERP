import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CostCenter, UpsertCostCenterPayload } from '../models/cost-center.models';

@Injectable({ providedIn: 'root' })
export class CostCenterRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/finance/cost-centers`;

  getList(options: { search?: string; status?: number | null; pageSize?: number } = {}): Observable<CostCenter[]> {
    let params = new HttpParams()
      .set('page', '1')
      .set('pageSize', String(options.pageSize ?? 200));
    if (options.search?.trim()) params = params.set('search', options.search.trim());
    if (options.status != null) params = params.set('status', String(options.status));
    return this.http.get<CostCenter[]>(this.base, { params }).pipe(map(r => r ?? []));
  }

  getById(id: string): Observable<CostCenter> {
    return this.http.get<CostCenter>(`${this.base}/${id}`);
  }

  create(payload: UpsertCostCenterPayload): Observable<CostCenter> {
    return this.http.post<CostCenter>(this.base, payload);
  }

  update(id: string, payload: UpsertCostCenterPayload): Observable<CostCenter> {
    return this.http.put<CostCenter>(`${this.base}/${id}`, payload);
  }

  activate(id: string): Observable<unknown> {
    return this.http.post(`${this.base}/${id}/activate`, {});
  }

  deactivate(id: string): Observable<unknown> {
    return this.http.post(`${this.base}/${id}/deactivate`, {});
  }

  delete(id: string): Observable<unknown> {
    return this.http.delete(`${this.base}/${id}`);
  }

  exportCsv(search?: string): Observable<Blob> {
    let params = new HttpParams();
    if (search?.trim()) params = params.set('search', search.trim());
    return this.http.get(`${this.base}/export`, { params, responseType: 'blob' });
  }
}
