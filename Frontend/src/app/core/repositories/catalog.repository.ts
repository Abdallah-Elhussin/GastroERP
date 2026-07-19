import { Observable } from 'rxjs';
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

export abstract class CatalogRepository {
  abstract getTypes(): Observable<ProductCatalogTypeDefinition[]>;
  abstract getDefinitions(search?: string, page?: number, pageSize?: number): Observable<ProductCatalogDefinition[]>;
  abstract getDefinitionById(id: string): Observable<ProductCatalogDefinition>;
  abstract getDefinitionByInventoryItemId(inventoryItemId: string): Observable<ProductCatalogDefinition>;
  abstract createDraft(payload: CreateCatalogDraftPayload): Observable<ProductCatalogDefinition>;
  abstract updateGeneralInfo(id: string, payload: UpdateCatalogGeneralInfoPayload): Observable<ProductCatalogDefinition>;
  abstract saveInventory(id: string, payload: SaveCatalogInventoryPayload): Observable<ProductCatalogDefinition>;
  abstract saveRecipe(id: string, payload: SaveCatalogRecipePayload): Observable<ProductCatalogDefinition>;
  abstract savePos(id: string, payload: SaveCatalogPosPayload): Observable<ProductCatalogDefinition>;
  abstract savePricing(id: string, payload: SaveCatalogPricingPayload): Observable<ProductCatalogDefinition>;
  abstract saveExtensions(id: string, payload: SaveCatalogExtensionsPayload): Observable<ProductCatalogDefinition>;
  abstract saveRelationships(id: string, payload: SaveCatalogRelationshipsPayload): Observable<ProductCatalogDefinition>;
  abstract activate(id: string): Observable<ProductCatalogDefinition>;
  abstract exportCsv(search?: string): Observable<Blob>;
  abstract importRows(rows: CatalogImportRow[]): Observable<number>;
  abstract getAuditTimeline(id: string): Observable<CatalogAuditEntry[]>;
  abstract getPriceHistory(id: string): Observable<CatalogPriceHistoryEntry[]>;
}
