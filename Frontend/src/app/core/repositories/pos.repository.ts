import { Observable } from 'rxjs';
import { Product, CartItem } from '../models/erp.models';

export abstract class PosRepository {
  abstract getProducts(): Observable<Product[]>;
  abstract checkout(cart: CartItem[]): Observable<void>;
}
