import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MenuRepository } from './menu.repository';
import { KitchenStation, MenuCategory, MenuProduct, PriceLevel } from '../models/menu.models';

@Injectable({ providedIn: 'root' })
export class RestMenuRepository extends MenuRepository {
  private http = inject(HttpClient);
  private readonly menuBase = `${environment.apiBaseUrl}/menu`;
  private readonly salesBase = `${environment.apiBaseUrl}/sales`;

  getCategories(): Observable<MenuCategory[]> {
    return this.http.get<MenuCategory[]>(`${this.menuBase}/categories`, {
      params: new HttpParams().set('pageSize', '200')
    });
  }

  getPriceLevels(): Observable<PriceLevel[]> {
    return this.http.get<PriceLevel[]>(`${this.menuBase}/price-levels`, {
      params: new HttpParams().set('pageSize', '100')
    });
  }

  getKitchenStations(): Observable<KitchenStation[]> {
    return this.http.get<KitchenStation[]>(`${this.salesBase}/kitchen`);
  }

  getProducts(search?: string): Observable<MenuProduct[]> {
    let params = new HttpParams().set('pageSize', '500');
    if (search?.trim()) params = params.set('search', search.trim());
    return this.http.get<MenuProduct[]>(`${this.menuBase}/products`, { params });
  }
}
