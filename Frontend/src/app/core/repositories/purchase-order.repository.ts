import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreatePurchaseOrderPayload,
  PurchaseOrderDashboardDto,
  PurchaseOrderDto,
  PurchaseOrderListParams,
  PurchaseOrderPage,
  UpdatePurchaseOrderPayload
} from '../models/purchase-order.models';

type RawPurchaseOrderPage = PurchaseOrderDto[] | { items: PurchaseOrderDto[]; totalCount?: number };

@Injectable({ providedIn: 'root' })
export class PurchaseOrderRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/inventory/purchases`;

  getDashboard(): Observable<PurchaseOrderDashboardDto> {
    return this.http.get<PurchaseOrderDashboardDto>(`${this.base}/dashboard`);
  }

  /**
   * Backend returns HandlePagedResult: a plain array body plus
   * X-Pagination-* response headers (see BaseApiController.HandlePagedResult).
   * This also tolerates a { items, totalCount } envelope in case that ever changes.
   */
  getList(params: PurchaseOrderListParams = {}): Observable<PurchaseOrderPage> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 50);
    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.status != null) httpParams = httpParams.set('status', params.status);
    if (params.supplierId) httpParams = httpParams.set('supplierId', params.supplierId);
    if (params.warehouseId) httpParams = httpParams.set('warehouseId', params.warehouseId);
    if (params.from) httpParams = httpParams.set('from', params.from);
    if (params.to) httpParams = httpParams.set('to', params.to);

    return this.http
      .get<RawPurchaseOrderPage>(this.base, { params: httpParams, observe: 'response' })
      .pipe(map(res => unwrapPage(res)));
  }

  getById(id: string): Observable<PurchaseOrderDto> {
    return this.http.get<PurchaseOrderDto>(`${this.base}/${id}`);
  }

  create(payload: CreatePurchaseOrderPayload): Observable<PurchaseOrderDto> {
    return this.http.post<PurchaseOrderDto>(this.base, payload);
  }

  update(id: string, payload: UpdatePurchaseOrderPayload): Observable<PurchaseOrderDto> {
    return this.http.put<PurchaseOrderDto>(`${this.base}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  copy(id: string): Observable<PurchaseOrderDto> {
    return this.http.post<PurchaseOrderDto>(`${this.base}/${id}/copy`, {});
  }

  approve(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/approve`, {});
  }

  cancel(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/cancel`, {});
  }

  close(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/close`, {});
  }

  send(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/send`, {});
  }
}

function unwrapPage(res: HttpResponse<RawPurchaseOrderPage>): PurchaseOrderPage {
  const body = res.body;
  let items: PurchaseOrderDto[];
  let totalCount: number;

  if (Array.isArray(body)) {
    items = body;
    totalCount = Number(res.headers.get('X-Pagination-TotalCount') ?? items.length);
  } else {
    items = body?.items ?? [];
    totalCount = Number(body?.totalCount ?? res.headers.get('X-Pagination-TotalCount') ?? items.length);
  }

  const pageNumber = Number(res.headers.get('X-Pagination-PageNumber') ?? 1);
  const pageSize = Number(res.headers.get('X-Pagination-PageSize') ?? (items.length || 50));
  const totalPages = Number(res.headers.get('X-Pagination-TotalPages') ?? 1);

  return { items, totalCount, pageNumber, pageSize, totalPages };
}
