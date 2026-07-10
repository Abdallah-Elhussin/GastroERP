import { Injectable, inject, signal } from '@angular/core';
import { catchError, of, tap } from 'rxjs';
import { InventoryRepository } from '../repositories/inventory.repository';
import {
  CreateInventoryItemPayload,
  InventoryCategory,
  InventoryItemDefinition,
  InventoryUnit,
  UpdateInventoryItemPayload
} from '../models/inventory.models';

@Injectable({
  providedIn: 'root'
})
export class InventoryService {
  private repo = inject(InventoryRepository);

  items = signal<InventoryItemDefinition[]>([]);
  categories = signal<InventoryCategory[]>([]);
  units = signal<InventoryUnit[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  loadMasterData(): void {
    this.repo.getCategories().pipe(
      catchError(() => of([] as InventoryCategory[])),
      tap(c => this.categories.set(c))
    ).subscribe();

    this.repo.getUnits().pipe(
      catchError(() => of([] as InventoryUnit[])),
      tap(u => this.units.set(u))
    ).subscribe();
  }

  loadItems(search?: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo.getItems(search).pipe(
      catchError(err => {
        this.error.set(err?.error?.error ?? 'Failed to load inventory items.');
        return of([] as InventoryItemDefinition[]);
      }),
      tap(items => {
        this.items.set(items);
        this.loading.set(false);
      })
    ).subscribe();
  }

  getItem(id: string) {
    return this.repo.getItemById(id);
  }

  createItem(payload: CreateInventoryItemPayload) {
    return this.repo.createItem(payload);
  }

  updateItem(id: string, payload: UpdateInventoryItemPayload) {
    return this.repo.updateItem(id, payload);
  }
}
