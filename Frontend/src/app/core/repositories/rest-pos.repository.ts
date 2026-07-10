import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PosRepository } from './pos.repository';
import { Product, CartItem } from '../models/erp.models';

@Injectable({
  providedIn: 'root'
})
export class RestPosRepository extends PosRepository {
  private http = inject(HttpClient);
  private apiUrl = '/api/pos';

  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.apiUrl}/products`);
  }

  checkout(cart: CartItem[]): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/checkout`, { items: cart });
  }
}
