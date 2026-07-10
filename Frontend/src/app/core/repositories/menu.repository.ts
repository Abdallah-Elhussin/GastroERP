import { Observable } from 'rxjs';
import { KitchenStation, MenuCategory, MenuProduct, PriceLevel } from '../models/menu.models';

export abstract class MenuRepository {
  abstract getCategories(): Observable<MenuCategory[]>;
  abstract getPriceLevels(): Observable<PriceLevel[]>;
  abstract getKitchenStations(): Observable<KitchenStation[]>;
  abstract getProducts(search?: string): Observable<MenuProduct[]>;
}
