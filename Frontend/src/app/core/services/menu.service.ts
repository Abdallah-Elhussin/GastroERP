import { Injectable, inject, signal } from '@angular/core';
import { catchError, of, tap } from 'rxjs';
import { MenuRepository } from '../repositories/menu.repository';
import { KitchenStation, MenuCategory, MenuProduct, PriceLevel } from '../models/menu.models';
import { Product } from '../models/erp.models';

@Injectable({ providedIn: 'root' })
export class MenuService {
  private repo = inject(MenuRepository);

  categories = signal<MenuCategory[]>([]);
  priceLevels = signal<PriceLevel[]>([]);
  kitchenStations = signal<KitchenStation[]>([]);
  menuProducts = signal<MenuProduct[]>([]);
  loading = signal(false);

  loadMasterData(): void {
    this.repo.getCategories().pipe(catchError(() => of([])), tap(c => this.categories.set(c))).subscribe();
    this.repo.getPriceLevels().pipe(catchError(() => of([])), tap(p => this.priceLevels.set(p))).subscribe();
    this.repo.getKitchenStations().pipe(catchError(() => of([])), tap(k => this.kitchenStations.set(k))).subscribe();
  }

  loadProducts(search?: string): void {
    this.loading.set(true);
    this.repo.getProducts(search).pipe(
      catchError(() => of([] as MenuProduct[])),
      tap(items => {
        this.menuProducts.set(items.filter(p => p.isAvailable));
        this.loading.set(false);
      })
    ).subscribe();
  }

  toPosProducts(items: MenuProduct[]): Product[] {
    return items.map(p => ({
      id: p.id,
      name: p.nameAr,
      price: p.basePrice,
      description: p.descriptionAr ?? '',
      category: p.categoryNameAr,
      categoryKey: this.categoryKey(p.categoryId),
      image: ''
    }));
  }

  private categoryKey(categoryId: string): Product['categoryKey'] {
    const keys: Product['categoryKey'][] = ['burgers', 'sides', 'drinks', 'desserts'];
    const hash = categoryId.split('').reduce((acc, c) => acc + c.charCodeAt(0), 0);
    return keys[hash % keys.length];
  }
}
