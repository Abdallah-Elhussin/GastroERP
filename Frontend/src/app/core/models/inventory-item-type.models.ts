/** Restaurant item type — drives sell/purchase/recipe/production behavior */
export type InventoryItemTypeCategoryKey =
  | 'RawMaterial'
  | 'PackagingMaterial'
  | 'FinishedProduct'
  | 'SemiFinishedProduct'
  | 'RecipeComponent'
  | 'MenuItem'
  | 'Bundle'
  | 'Service'
  | 'FixedAsset'
  | 'PromotionItem';

export const INVENTORY_ITEM_TYPE_CATEGORIES: { value: number; key: InventoryItemTypeCategoryKey; labelKey: string }[] = [
  { value: 1, key: 'RawMaterial', labelKey: 'inv.itemTypes.category.rawMaterial' },
  { value: 2, key: 'PackagingMaterial', labelKey: 'inv.itemTypes.category.packaging' },
  { value: 3, key: 'FinishedProduct', labelKey: 'inv.itemTypes.category.finished' },
  { value: 4, key: 'SemiFinishedProduct', labelKey: 'inv.itemTypes.category.semiFinished' },
  { value: 5, key: 'RecipeComponent', labelKey: 'inv.itemTypes.category.recipeComponent' },
  { value: 6, key: 'MenuItem', labelKey: 'inv.itemTypes.category.menuItem' },
  { value: 7, key: 'Bundle', labelKey: 'inv.itemTypes.category.bundle' },
  { value: 8, key: 'Service', labelKey: 'inv.itemTypes.category.service' },
  { value: 9, key: 'FixedAsset', labelKey: 'inv.itemTypes.category.fixedAsset' },
  { value: 10, key: 'PromotionItem', labelKey: 'inv.itemTypes.category.promotion' }
];

export interface InventoryItemType {
  id: string;
  code: string;
  nameAr: string;
  nameEn?: string | null;
  description?: string | null;
  category: number;
  categoryName: string;
  codeStart?: number | null;
  codeEnd?: number | null;
  isInventory: boolean;
  canSell: boolean;
  canPurchase: boolean;
  isRecipe: boolean;
  isProduction: boolean;
  allowNegativeStock: boolean;
  color: string;
  sortOrder: number;
  isSystem: boolean;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string | null;
}

export interface InventoryItemTypeQuery {
  search?: string;
  category?: number | null;
  isActive?: boolean | null;
  isInventory?: boolean | null;
  canSell?: boolean | null;
  canPurchase?: boolean | null;
  isRecipe?: boolean | null;
  isProduction?: boolean | null;
  sortBy?: string;
  sortDesc?: boolean;
  page?: number;
  pageSize?: number;
}

export interface InventoryItemTypePage {
  items: InventoryItemType[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateInventoryItemTypePayload {
  code: string;
  nameAr: string;
  nameEn?: string | null;
  description?: string | null;
  category: number;
  codeStart?: number | null;
  codeEnd?: number | null;
  isInventory: boolean;
  canSell: boolean;
  canPurchase: boolean;
  isRecipe: boolean;
  isProduction: boolean;
  allowNegativeStock: boolean;
  color?: string | null;
  sortOrder: number;
}

export interface UpdateInventoryItemTypePayload {
  nameAr: string;
  nameEn?: string | null;
  description?: string | null;
  category: number;
  codeStart?: number | null;
  codeEnd?: number | null;
  isInventory: boolean;
  canSell: boolean;
  canPurchase: boolean;
  isRecipe: boolean;
  isProduction: boolean;
  allowNegativeStock: boolean;
  color?: string | null;
  sortOrder: number;
  isActive: boolean;
}
