export type ProductCatalogTypeCode =
  | 'raw' | 'semi' | 'finished' | 'menu' | 'combo' | 'modifier' | 'bundle'
  | 'service' | 'voucher' | 'gift' | 'packaging' | 'asset' | 'expense';

export type ProductCatalogTypeId = number;
export type InventoryCostingMethodId = 1 | 2 | 3;

export interface ProductCatalogTypeDefinition {
  type: ProductCatalogTypeId;
  code: ProductCatalogTypeCode;
  nameAr: string;
  nameEn: string;
  prefix: string;
  requiresInventory: boolean;
  requiresRecipe: boolean;
  requiresProduct: boolean;
  requiresPricing: boolean;
  wizardSteps: string[];
}

export interface CatalogRecipeIngredient {
  inventoryItemId: string;
  itemNameAr?: string;
  unitId: string;
  unitNameAr?: string;
  quantity: number;
  wastePercentage: number;
}

export interface CatalogPriceLevelLine {
  priceLevelId: string;
  priceLevelName?: string;
  price: number;
}

export interface CatalogRelationship {
  targetCatalogId: string;
  relationshipType: string;
  targetNameAr?: string;
}

export interface CatalogAuditEntry {
  eventType: string;
  description: string;
  occurredAt: string;
  actor?: string;
}

export interface CatalogPriceHistoryEntry {
  id: string;
  previousPrice: number;
  currentPrice: number;
  currency: string;
  priceLevelName?: string;
  effectiveDate: string;
  actor?: string;
}

export interface ProductCatalogDefinition {
  id: string;
  tenantId: string;
  catalogType: ProductCatalogTypeId;
  code: string;
  sku?: string;
  barcode?: string;
  nameAr: string;
  nameEn?: string;
  shortDescriptionAr?: string;
  shortDescriptionEn?: string;
  longDescriptionAr?: string;
  longDescriptionEn?: string;
  keywords?: string;
  brand?: string;
  tagsJson?: string;
  primaryImageUrl?: string;
  status: number;
  wizardStepCompleted: number;
  menuCategoryId?: string;
  inventoryCategoryId?: string;
  inventoryItemId?: string;
  productId?: string;
  recipeId?: string;
  baseUnitId?: string;
  defaultPurchaseUnitId?: string;
  defaultRecipeUnitId?: string;
  minStock: number;
  maxStock: number;
  safetyStock: number;
  reorderLevel: number;
  reorderQuantity: number;
  costingMethod: InventoryCostingMethodId;
  trackBatch: boolean;
  trackSerial: boolean;
  trackExpiry: boolean;
  allowNegativeStock: boolean;
  recipeYield: number;
  recipeWastePercentage: number;
  recipePreparationTime: number;
  recipeInstructions?: string;
  recipeIngredients: CatalogRecipeIngredient[];
  prepTimeMinutes: number;
  isAvailableOnPos: boolean;
  isFeaturedOnPos: boolean;
  kitchenStationId?: string;
  basePrice: number;
  currency: string;
  priceLevels: CatalogPriceLevelLine[];
  supplierIds: string[];
  mediaUrls: string[];
  variantAttributesJson?: string;
  relatedProducts: CatalogRelationship[];
}

export interface CreateCatalogDraftPayload {
  catalogType: ProductCatalogTypeId;
  nameAr: string;
  nameEn?: string;
}

export interface UpdateCatalogGeneralInfoPayload {
  nameAr: string;
  nameEn?: string;
  shortDescriptionAr?: string;
  shortDescriptionEn?: string;
  longDescriptionAr?: string;
  longDescriptionEn?: string;
  keywords?: string;
  brand?: string;
  tagsJson?: string;
  sku?: string;
  barcode?: string;
  primaryImageUrl?: string;
  menuCategoryId?: string;
  inventoryCategoryId?: string;
}

export interface SaveCatalogInventoryPayload {
  baseUnitId: string;
  defaultPurchaseUnitId?: string;
  defaultRecipeUnitId?: string;
  minStock: number;
  maxStock: number;
  safetyStock: number;
  reorderLevel: number;
  reorderQuantity: number;
  costingMethod: InventoryCostingMethodId;
  trackBatch: boolean;
  trackSerial: boolean;
  trackExpiry: boolean;
  allowNegativeStock: boolean;
}

export interface SaveCatalogRecipePayload {
  yield: number;
  wastePercentage: number;
  preparationTime: number;
  instructions?: string;
  ingredients: CatalogRecipeIngredient[];
}

export interface SaveCatalogPosPayload {
  menuCategoryId: string;
  prepTimeMinutes: number;
  isAvailableOnPos: boolean;
  isFeaturedOnPos: boolean;
  kitchenStationId?: string;
}

export interface SaveCatalogPricingPayload {
  basePrice: number;
  currency: string;
  priceLevels: CatalogPriceLevelLine[];
}

export interface SaveCatalogExtensionsPayload {
  supplierIds: string[];
  mediaUrls: string[];
  variantAttributesJson?: string;
}

export interface SaveCatalogRelationshipsPayload {
  relatedProducts: CatalogRelationship[];
}

export interface CatalogImportRow {
  catalogType: ProductCatalogTypeId;
  nameAr: string;
  nameEn?: string;
  sku?: string;
  barcode?: string;
  basePrice: number;
}

export type CatalogWizardStep = 'type' | 'general' | 'inventory' | 'recipe' | 'pos' | 'pricing' | 'review';
