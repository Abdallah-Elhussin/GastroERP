import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateProductPricesBatchPayload,
  InventoryItemLookup,
  ProductPrice,
  ProductUnitPricingRow,
  ProductCostType,
  SalesPriceList,
  UpdateProductPricePayload
} from '../models/product-pricing.models';

@Injectable({ providedIn: 'root' })
export class ProductPricingRepository {
  private http = inject(HttpClient);
  private readonly pricesBase = `${environment.apiBaseUrl}/sales/product-prices`;
  private readonly listsBase = `${environment.apiBaseUrl}/sales/price-lists`;
  private readonly itemsBase = `${environment.apiBaseUrl}/inventory/items`;

  getPrices(opts: {
    search?: string;
    productId?: string | null;
    branchId?: string | null;
    priceListId?: string | null;
    unitId?: string | null;
    isActive?: boolean | null;
    asOfDate?: string | null;
    sortBy?: string;
    sortDesc?: boolean;
    page?: number;
    pageSize?: number;
  } = {}): Observable<ProductPrice[]> {
    let params = new HttpParams()
      .set('page', String(opts.page ?? 1))
      .set('pageSize', String(opts.pageSize ?? 100));
    if (opts.search?.trim()) params = params.set('search', opts.search.trim());
    if (opts.productId) params = params.set('productId', opts.productId);
    if (opts.branchId) params = params.set('branchId', opts.branchId);
    if (opts.priceListId) params = params.set('priceListId', opts.priceListId);
    if (opts.unitId) params = params.set('unitId', opts.unitId);
    if (opts.isActive != null) params = params.set('isActive', String(opts.isActive));
    if (opts.asOfDate) params = params.set('asOfDate', opts.asOfDate);
    if (opts.sortBy) params = params.set('sortBy', opts.sortBy);
    if (opts.sortDesc) params = params.set('sortDesc', 'true');
    return this.http.get<ProductPrice[]>(this.pricesBase, { params }).pipe(map(rows => rows ?? []));
  }

  getById(id: string): Observable<ProductPrice> {
    return this.http.get<ProductPrice>(`${this.pricesBase}/${id}`);
  }

  getProductUnits(productId: string, costType: ProductCostType = 1): Observable<ProductUnitPricingRow[]> {
    const params = new HttpParams().set('costType', String(costType));
    return this.http
      .get<ProductUnitPricingRow[]>(`${this.pricesBase}/product-units/${productId}`, { params })
      .pipe(map(rows => rows ?? []));
  }

  createBatch(payload: CreateProductPricesBatchPayload): Observable<ProductPrice[]> {
    return this.http.post<ProductPrice[]>(`${this.pricesBase}/batch`, {
      tenantId: '00000000-0000-0000-0000-000000000000',
      ...payload
    });
  }

  update(id: string, payload: UpdateProductPricePayload): Observable<void> {
    return this.http.put<void>(`${this.pricesBase}/${id}`, {
      tenantId: '00000000-0000-0000-0000-000000000000',
      ...payload
    });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.pricesBase}/${id}`);
  }

  getPriceLists(activeOnly = true): Observable<SalesPriceList[]> {
    const params = new HttpParams().set('activeOnly', String(activeOnly));
    return this.http.get<SalesPriceList[]>(this.listsBase, { params }).pipe(map(rows => rows ?? []));
  }

  searchItems(search?: string): Observable<InventoryItemLookup[]> {
    let params = new HttpParams().set('page', '1').set('pageSize', '50');
    if (search?.trim()) params = params.set('search', search.trim());
    return this.http.get<InventoryItemLookup[]>(this.itemsBase, { params }).pipe(map(rows => rows ?? []));
  }
}
