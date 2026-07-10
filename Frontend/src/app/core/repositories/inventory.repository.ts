import { Observable } from 'rxjs';
import {
  CreateInventoryItemPayload,
  InventoryCategory,
  InventoryItemDefinition,
  InventoryUnit,
  UpdateInventoryItemPayload
} from '../models/inventory.models';

export abstract class InventoryRepository {
  abstract getItems(search?: string, page?: number, pageSize?: number): Observable<InventoryItemDefinition[]>;
  abstract getItemById(id: string): Observable<InventoryItemDefinition>;
  abstract createItem(payload: CreateInventoryItemPayload): Observable<InventoryItemDefinition>;
  abstract updateItem(id: string, payload: UpdateInventoryItemPayload): Observable<void>;
  abstract getCategories(): Observable<InventoryCategory[]>;
  abstract getUnits(): Observable<InventoryUnit[]>;
}
