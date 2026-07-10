import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { InventoryRepository } from './inventory.repository';
import {
  CreateInventoryItemPayload,
  InventoryCategory,
  InventoryItemDefinition,
  InventoryItemKind,
  InventoryUnit,
  UpdateInventoryItemPayload
} from '../models/inventory.models';

interface ApiInventoryItem {
  id: string;
  tenantId: string;
  categoryId: string;
  categoryNameAr: string;
  nameAr: string;
  nameEn?: string;
  descriptionAr?: string;
  descriptionEn?: string;
  sku?: string;
  barcode?: string;
  imageUrl?: string;
  itemKind: number;
  baseUnitId: string;
  baseUnitNameAr: string;
  defaultPurchaseUnitId?: string;
  defaultRecipeUnitId?: string;
  reorderLevel: number;
  reorderQuantity: number;
  averageUnitCost?: number;
  lastPurchaseUnitCost?: number;
  isActive: boolean;
}

interface ApiCategory {
  id: string;
  tenantId: string;
  nameAr: string;
  nameEn?: string;
  color?: string;
  isActive: boolean;
}

interface ApiUnit {
  id: string;
  tenantId: string;
  nameAr: string;
  nameEn?: string;
  symbol?: string;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class RestInventoryRepository extends InventoryRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/inventory`;

  getItems(search?: string, page = 1, pageSize = 100): Observable<InventoryItemDefinition[]> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    if (search?.trim()) {
      params = params.set('search', search.trim());
    }
    return this.http.get<ApiInventoryItem[]>(`${this.base}/items`, { params }).pipe(
      map(rows => rows.map(mapApiItem))
    );
  }

  getItemById(id: string): Observable<InventoryItemDefinition> {
    return this.http.get<ApiInventoryItem>(`${this.base}/items/${id}`).pipe(
      map(mapApiItem)
    );
  }

  createItem(payload: CreateInventoryItemPayload): Observable<InventoryItemDefinition> {
    return this.http.post<ApiInventoryItem>(`${this.base}/items`, mapToApiPayload(payload)).pipe(
      map(mapApiItem)
    );
  }

  updateItem(id: string, payload: UpdateInventoryItemPayload): Observable<void> {
    return this.http.put<void>(`${this.base}/items/${id}`, mapToUpdatePayload(payload));
  }

  getCategories(): Observable<InventoryCategory[]> {
    const params = new HttpParams().set('page', 1).set('pageSize', 100);
    return this.http.get<ApiCategory[]>(`${this.base}/categories`, { params }).pipe(
      map(rows => rows.map(c => ({
        id: c.id,
        nameAr: c.nameAr,
        nameEn: c.nameEn,
        color: c.color,
        isActive: c.isActive
      })))
    );
  }

  getUnits(): Observable<InventoryUnit[]> {
    return this.http.get<ApiUnit[]>(`${this.base}/units`).pipe(
      map(rows => rows.map(u => ({
        id: u.id,
        nameAr: u.nameAr,
        nameEn: u.nameEn,
        symbol: u.symbol,
        isActive: u.isActive
      })))
    );
  }
}

function mapApiItemKind(value: number): InventoryItemKind {
  return value === 2 ? 'manufactured' : 'raw';
}

function mapToApiItemKind(value: InventoryItemKind): number {
  return value === 'manufactured' ? 2 : 1;
}

function mapApiItem(raw: ApiInventoryItem): InventoryItemDefinition {
  return {
    id: raw.id,
    tenantId: raw.tenantId,
    categoryId: raw.categoryId,
    categoryNameAr: raw.categoryNameAr,
    nameAr: raw.nameAr,
    nameEn: raw.nameEn,
    descriptionAr: raw.descriptionAr,
    descriptionEn: raw.descriptionEn,
    sku: raw.sku,
    barcode: raw.barcode,
    imageUrl: raw.imageUrl,
    itemKind: mapApiItemKind(raw.itemKind),
    baseUnitId: raw.baseUnitId,
    baseUnitNameAr: raw.baseUnitNameAr,
    defaultPurchaseUnitId: raw.defaultPurchaseUnitId,
    defaultRecipeUnitId: raw.defaultRecipeUnitId,
    reorderLevel: Number(raw.reorderLevel),
    reorderQuantity: Number(raw.reorderQuantity),
    averageUnitCost: raw.averageUnitCost != null ? Number(raw.averageUnitCost) : undefined,
    lastPurchaseUnitCost: raw.lastPurchaseUnitCost != null ? Number(raw.lastPurchaseUnitCost) : undefined,
    isActive: raw.isActive
  };
}

function mapToApiPayload(payload: CreateInventoryItemPayload): Record<string, unknown> {
  return {
    tenantId: '00000000-0000-0000-0000-000000000000',
    categoryId: payload.categoryId,
    nameAr: payload.nameAr,
    baseUnitId: payload.baseUnitId,
    nameEn: payload.nameEn || null,
    descriptionAr: payload.descriptionAr || null,
    descriptionEn: payload.descriptionEn || null,
    sku: payload.sku || null,
    barcode: payload.barcode || null,
    imageUrl: payload.imageUrl || null,
    itemKind: mapToApiItemKind(payload.itemKind),
    defaultPurchaseUnitId: payload.defaultPurchaseUnitId || null,
    defaultRecipeUnitId: payload.defaultRecipeUnitId || null,
    reorderLevel: payload.reorderLevel ?? 0,
    reorderQuantity: payload.reorderQuantity ?? 0
  };
}

function mapToUpdatePayload(payload: UpdateInventoryItemPayload): Record<string, unknown> {
  return {
    nameAr: payload.nameAr,
    nameEn: payload.nameEn || null,
    descriptionAr: payload.descriptionAr || null,
    descriptionEn: payload.descriptionEn || null,
    sku: payload.sku || null,
    barcode: payload.barcode || null,
    imageUrl: payload.imageUrl || null,
    itemKind: mapToApiItemKind(payload.itemKind),
    categoryId: payload.categoryId,
    baseUnitId: payload.baseUnitId,
    defaultPurchaseUnitId: payload.defaultPurchaseUnitId || null,
    defaultRecipeUnitId: payload.defaultRecipeUnitId || null,
    reorderLevel: payload.reorderLevel ?? 0,
    reorderQuantity: payload.reorderQuantity ?? 0
  };
}
