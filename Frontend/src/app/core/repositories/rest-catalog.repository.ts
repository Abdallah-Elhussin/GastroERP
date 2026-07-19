import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CatalogRepository } from './catalog.repository';
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
export class RestCatalogRepository extends CatalogRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/catalog`;

  getTypes(): Observable<ProductCatalogTypeDefinition[]> {
    return this.http.get<ProductCatalogTypeDefinition[]>(`${this.base}/types`);
  }

  getDefinitions(search?: string, page = 1, pageSize = 50): Observable<ProductCatalogDefinition[]> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search?.trim()) params = params.set('search', search.trim());
    return this.http.get<ProductCatalogDefinition[]>(`${this.base}/definitions`, { params });
  }

  getDefinitionById(id: string): Observable<ProductCatalogDefinition> {
    return this.http.get<ProductCatalogDefinition>(`${this.base}/definitions/${id}`);
  }

  getDefinitionByInventoryItemId(inventoryItemId: string): Observable<ProductCatalogDefinition> {
    return this.http.get<ProductCatalogDefinition>(`${this.base}/definitions/by-inventory-item/${inventoryItemId}`);
  }

  createDraft(payload: CreateCatalogDraftPayload): Observable<ProductCatalogDefinition> {
    return this.http.post<ProductCatalogDefinition>(`${this.base}/definitions`, payload);
  }

  updateGeneralInfo(id: string, payload: UpdateCatalogGeneralInfoPayload): Observable<ProductCatalogDefinition> {
    return this.http.put<ProductCatalogDefinition>(`${this.base}/definitions/${id}/general`, payload);
  }

  saveInventory(id: string, payload: SaveCatalogInventoryPayload): Observable<ProductCatalogDefinition> {
    return this.http.put<ProductCatalogDefinition>(`${this.base}/definitions/${id}/inventory`, payload);
  }

  saveRecipe(id: string, payload: SaveCatalogRecipePayload): Observable<ProductCatalogDefinition> {
    return this.http.put<ProductCatalogDefinition>(`${this.base}/definitions/${id}/recipe`, payload);
  }

  savePos(id: string, payload: SaveCatalogPosPayload): Observable<ProductCatalogDefinition> {
    return this.http.put<ProductCatalogDefinition>(`${this.base}/definitions/${id}/pos`, payload);
  }

  savePricing(id: string, payload: SaveCatalogPricingPayload): Observable<ProductCatalogDefinition> {
    return this.http.put<ProductCatalogDefinition>(`${this.base}/definitions/${id}/pricing`, payload);
  }

  saveExtensions(id: string, payload: SaveCatalogExtensionsPayload): Observable<ProductCatalogDefinition> {
    return this.http.put<ProductCatalogDefinition>(`${this.base}/definitions/${id}/extensions`, payload);
  }

  saveRelationships(id: string, payload: SaveCatalogRelationshipsPayload): Observable<ProductCatalogDefinition> {
    return this.http.put<ProductCatalogDefinition>(`${this.base}/definitions/${id}/relationships`, payload);
  }

  activate(id: string): Observable<ProductCatalogDefinition> {
    return this.http.post<ProductCatalogDefinition>(`${this.base}/definitions/${id}/activate`, {});
  }

  exportCsv(search?: string): Observable<Blob> {
    let params = new HttpParams();
    if (search?.trim()) params = params.set('search', search.trim());
    return this.http.get(`${this.base}/definitions/export`, { params, responseType: 'blob' });
  }

  importRows(rows: CatalogImportRow[]): Observable<number> {
    return this.http.post<number>(`${this.base}/definitions/import`, rows);
  }

  getAuditTimeline(id: string): Observable<CatalogAuditEntry[]> {
    return this.http.get<CatalogAuditEntry[]>(`${this.base}/definitions/${id}/audit`);
  }

  getPriceHistory(id: string): Observable<CatalogPriceHistoryEntry[]> {
    return this.http.get<CatalogPriceHistoryEntry[]>(`${this.base}/definitions/${id}/price-history`);
  }
}
