import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ProductInquiryDetail, ProductInquiryListItem } from '../models/product-inquiry.models';

@Injectable({ providedIn: 'root' })
export class ProductInquiryRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/inventory/product-inquiry`;

  getList(opts: {
    search?: string;
    activeOnly?: boolean;
    inventoryOnly?: boolean;
    categoryId?: string | null;
    itemTypeId?: string | null;
    sortBy?: string;
    sortDesc?: boolean;
    page?: number;
    pageSize?: number;
  } = {}): Observable<ProductInquiryListItem[]> {
    let params = new HttpParams()
      .set('page', String(opts.page ?? 1))
      .set('pageSize', String(opts.pageSize ?? 100))
      .set('activeOnly', String(opts.activeOnly ?? true))
      .set('inventoryOnly', String(opts.inventoryOnly ?? false));
    if (opts.search?.trim()) params = params.set('search', opts.search.trim());
    if (opts.categoryId) params = params.set('categoryId', opts.categoryId);
    if (opts.itemTypeId) params = params.set('itemTypeId', opts.itemTypeId);
    if (opts.sortBy) params = params.set('sortBy', opts.sortBy);
    if (opts.sortDesc) params = params.set('sortDesc', 'true');
    return this.http.get<ProductInquiryListItem[]>(this.base, { params }).pipe(map(rows => rows ?? []));
  }

  getDetail(id: string): Observable<ProductInquiryDetail> {
    return this.http.get<ProductInquiryDetail>(`${this.base}/${id}`);
  }
}
