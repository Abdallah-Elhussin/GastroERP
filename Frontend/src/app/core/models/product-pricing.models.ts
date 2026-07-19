export type PricingMethod = 1 | 2 | 3 | 4;
export type ProductCostType = 1 | 2 | 3;
export type SalesChannel = 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9;

export interface SalesPriceList {
  id: string;
  tenantId: string;
  code: string;
  nameAr: string;
  nameEn?: string | null;
  description?: string | null;
  defaultSalesChannel?: SalesChannel | null;
  sortOrder: number;
  isDefault: boolean;
  isSystem: boolean;
  isActive: boolean;
  createdAt: string;
}

export interface ProductPrice {
  id: string;
  tenantId: string;
  productId: string;
  productNameAr?: string | null;
  productSku?: string | null;
  branchId?: string | null;
  branchNameAr?: string | null;
  priceListId: string;
  priceListNameAr?: string | null;
  salesChannel: SalesChannel;
  unitId: string;
  unitNameAr?: string | null;
  unitFactor: number;
  pricingMethod: PricingMethod;
  costType: ProductCostType;
  cost: number;
  profitMargin: number;
  profitAmount: number;
  sellingPrice: number;
  minimumPrice?: number | null;
  maximumDiscount?: number | null;
  startDate: string;
  endDate?: string | null;
  priority: number;
  currencyId?: string | null;
  isDefault: boolean;
  isActive: boolean;
  notes?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

export interface ProductUnitPricingRow {
  unitId: string;
  unitNameAr: string;
  unitNameEn?: string | null;
  factor: number;
  cost: number;
  isBaseUnit: boolean;
}

export interface ProductPriceUnitLinePayload {
  unitId: string;
  unitFactor: number;
  cost: number;
  profitMargin: number;
  profitAmount: number;
  sellingPrice: number;
  minimumPrice?: number | null;
  save?: boolean;
}

export interface CreateProductPricesBatchPayload {
  productId: string;
  priceListId: string;
  pricingMethod: PricingMethod;
  costType: ProductCostType;
  startDate: string;
  lines: ProductPriceUnitLinePayload[];
  branchId?: string | null;
  salesChannel?: SalesChannel;
  maximumDiscount?: number | null;
  endDate?: string | null;
  priority?: number;
  isDefault?: boolean;
  notes?: string | null;
}

export interface UpdateProductPricePayload {
  branchId?: string | null;
  priceListId: string;
  salesChannel: SalesChannel;
  unitId: string;
  pricingMethod: PricingMethod;
  costType: ProductCostType;
  cost: number;
  profitMargin: number;
  profitAmount: number;
  sellingPrice: number;
  minimumPrice?: number | null;
  maximumDiscount?: number | null;
  startDate: string;
  endDate?: string | null;
  priority: number;
  currencyId?: string | null;
  isDefault: boolean;
  isActive: boolean;
  notes?: string | null;
}

export interface InventoryItemLookup {
  id: string;
  nameAr: string;
  nameEn?: string | null;
  sku?: string | null;
  isActive: boolean;
}
