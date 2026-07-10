export type InventoryItemKind = 'raw' | 'manufactured';

export interface InventoryCategory {
  id: string;
  nameAr: string;
  nameEn?: string;
  color?: string;
  isActive: boolean;
}

export interface InventoryUnit {
  id: string;
  nameAr: string;
  nameEn?: string;
  symbol?: string;
  isActive: boolean;
}

export interface InventoryItemDefinition {
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
  itemKind: InventoryItemKind;
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

export interface CreateInventoryItemPayload {
  categoryId: string;
  nameAr: string;
  baseUnitId: string;
  nameEn?: string;
  descriptionAr?: string;
  descriptionEn?: string;
  sku?: string;
  barcode?: string;
  imageUrl?: string;
  itemKind: InventoryItemKind;
  defaultPurchaseUnitId?: string;
  defaultRecipeUnitId?: string;
  reorderLevel?: number;
  reorderQuantity?: number;
}

export interface UpdateInventoryItemPayload extends CreateInventoryItemPayload {}
