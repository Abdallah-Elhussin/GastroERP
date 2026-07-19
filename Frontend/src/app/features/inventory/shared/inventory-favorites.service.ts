import { Injectable, signal, computed } from '@angular/core';

const STORAGE_KEY = 'gastro_inventory_favorites';

export interface InventoryFavorite {
  path: string;
  labelKey: string;
  icon: string;
}

@Injectable({ providedIn: 'root' })
export class InventoryFavoritesService {
  private readonly favoritesSignal = signal<InventoryFavorite[]>(this.load());

  favorites = computed(() => this.favoritesSignal());

  isFavorite(path: string): boolean {
    return this.favoritesSignal().some(f => f.path === path);
  }

  toggle(item: InventoryFavorite): void {
    const current = this.favoritesSignal();
    const exists = current.some(f => f.path === item.path);
    const next = exists
      ? current.filter(f => f.path !== item.path)
      : [...current, item];
    this.favoritesSignal.set(next);
    this.persist(next);
  }

  private load(): InventoryFavorite[] {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? (JSON.parse(raw) as InventoryFavorite[]) : [];
    } catch {
      return [];
    }
  }

  private persist(items: InventoryFavorite[]): void {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(items));
    } catch {
      /* ignore quota */
    }
  }
}
