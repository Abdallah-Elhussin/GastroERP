export interface MenuCategory {
  id: string;
  nameAr: string;
  nameEn?: string;
  isActive: boolean;
}

export interface PriceLevel {
  id: string;
  nameAr: string;
  nameEn?: string;
  isDefault: boolean;
  isActive: boolean;
}

export interface KitchenStation {
  id: string;
  nameAr: string;
  nameEn?: string;
  branchId?: string;
}

export interface MenuProduct {
  id: string;
  categoryId: string;
  categoryNameAr: string;
  nameAr: string;
  nameEn?: string;
  descriptionAr?: string;
  basePrice: number;
  currency: string;
  prepTimeMinutes: number;
  isAvailable: boolean;
  isFeatured: boolean;
  sku?: string;
}
