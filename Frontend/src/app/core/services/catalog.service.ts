import { Injectable, inject, signal } from '@angular/core';
import { catchError, of, take, tap } from 'rxjs';
import { CatalogRepository } from '../repositories/catalog.repository';
import {
  CatalogAuditEntry,
  CatalogImportRow,
  CatalogPriceHistoryEntry,
  CreateCatalogDraftPayload,
  ProductCatalogDefinition,
  ProductCatalogTypeDefinition,
  SaveCatalogExtensionsPayload,
  SaveCatalogInventoryPayload,
  SaveCatalogPosPayload,
  SaveCatalogPricingPayload,
  SaveCatalogRecipePayload,
  SaveCatalogRelationshipsPayload,
  UpdateCatalogGeneralInfoPayload
} from '../models/catalog.models';

@Injectable({ providedIn: 'root' })
export class CatalogService {
  private repo = inject(CatalogRepository);

  types = signal<ProductCatalogTypeDefinition[]>([]);
  definitions = signal<ProductCatalogDefinition[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  loadTypes(): void {
    this.repo.getTypes().pipe(
      catchError(() => of([] as ProductCatalogTypeDefinition[])),
      tap(t => this.types.set(t))
    ).subscribe();
  }

  whenTypesReady() {
    if (this.types().length > 0) return of(this.types());
    return this.repo.getTypes().pipe(
      catchError(() => of([] as ProductCatalogTypeDefinition[])),
      tap(t => this.types.set(t)),
      take(1)
    );
  }

  loadDefinitions(search?: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo.getDefinitions(search).pipe(
      catchError(err => {
        this.error.set(err?.error?.error ?? 'Failed to load catalog.');
        return of([] as ProductCatalogDefinition[]);
      }),
      tap(items => {
        this.definitions.set(items);
        this.loading.set(false);
      })
    ).subscribe();
  }

  getDefinition(id: string) {
    return this.repo.getDefinitionById(id);
  }

  getDefinitionByInventoryItemId(inventoryItemId: string) {
    return this.repo.getDefinitionByInventoryItemId(inventoryItemId);
  }

  createDraft(payload: CreateCatalogDraftPayload) {
    return this.repo.createDraft(payload);
  }

  updateGeneralInfo(id: string, payload: UpdateCatalogGeneralInfoPayload) {
    return this.repo.updateGeneralInfo(id, payload);
  }

  saveInventory(id: string, payload: SaveCatalogInventoryPayload) {
    return this.repo.saveInventory(id, payload);
  }

  saveRecipe(id: string, payload: SaveCatalogRecipePayload) {
    return this.repo.saveRecipe(id, payload);
  }

  savePos(id: string, payload: SaveCatalogPosPayload) {
    return this.repo.savePos(id, payload);
  }

  savePricing(id: string, payload: SaveCatalogPricingPayload) {
    return this.repo.savePricing(id, payload);
  }

  saveExtensions(id: string, payload: SaveCatalogExtensionsPayload) {
    return this.repo.saveExtensions(id, payload);
  }

  saveRelationships(id: string, payload: SaveCatalogRelationshipsPayload) {
    return this.repo.saveRelationships(id, payload);
  }

  activate(id: string) {
    return this.repo.activate(id);
  }

  exportCsv(search?: string) {
    return this.repo.exportCsv(search);
  }

  importRows(rows: CatalogImportRow[]) {
    return this.repo.importRows(rows);
  }

  getAuditTimeline(id: string) {
    return this.repo.getAuditTimeline(id);
  }

  getPriceHistory(id: string) {
    return this.repo.getPriceHistory(id);
  }
}
