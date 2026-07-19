export interface ProductInquiryListItem {
  id: string;
  sku?: string | null;
  barcode?: string | null;
  nameAr: string;
  nameEn?: string | null;
  categoryNameAr?: string | null;
  itemTypeNameAr?: string | null;
  unitNameAr?: string | null;
  sellingPrice?: number | null;
  lastPurchasePrice?: number | null;
  totalOnHand: number;
  isActive: boolean;
  isInventory: boolean;
  itemKind: number;
}

export interface ProductInquiryDetail {
  id: string;
  general: {
    sku?: string | null;
    nameAr: string;
    nameEn?: string | null;
    barcode?: string | null;
    categoryNameAr?: string | null;
    itemTypeNameAr?: string | null;
    baseUnitNameAr?: string | null;
    isActive: boolean;
    isInventory: boolean;
    itemKind: string;
  };
  warehouses: Array<{
    warehouseId: string;
    warehouseNameAr: string;
    warehouseCode?: string | null;
    branchId?: string | null;
    onHand: number;
    reserved: number;
    available: number;
  }>;
  cost: {
    averageCost: number;
    lastPurchaseCost: number;
    standardCost?: number | null;
    costingMethod: string;
    canView: boolean;
  };
  sales: {
    lastSalePrice?: number | null;
    defaultPrice?: number | null;
    lastSaleAt?: string | null;
    lastCustomerName?: string | null;
    lastOrderNumber?: string | null;
  };
  purchase: {
    lastPurchasePrice?: number | null;
    lastSupplierName?: string | null;
    lastDocumentNumber?: string | null;
    lastPurchaseAt?: string | null;
  };
  prices: Array<{
    priceListNameAr: string;
    unitNameAr?: string | null;
    sellingPrice: number;
    startDate: string;
    endDate?: string | null;
    isActive: boolean;
  }>;
  reservations: Array<{
    id: string;
    warehouseNameAr: string;
    reservedQuantity: number;
    sourceDocument: string;
    status: string;
    expirationDate?: string | null;
  }>;
  batches: Array<{
    id: string;
    batchNumber: string;
    lotNumber?: string | null;
    expirationDate?: string | null;
    status: string;
    quantity: number;
  }>;
  recentMovements: Array<{
    movementId: string;
    occurredAt: string;
    transactionType: string;
    quantityChange: number;
    referenceDocumentNumber?: string | null;
    warehouseNameAr: string;
  }>;
  recipe: {
    hasRecipe: boolean;
    ingredientCount: number;
    recipeCost?: number | null;
    recipeNameAr?: string | null;
  };
  supplier: {
    primarySupplierName?: string | null;
    lastSupplierName?: string | null;
    leadTimeDays?: number | null;
    lastPrice?: number | null;
    canView: boolean;
  };
  branches: Array<{
    branchId: string;
    branchNameAr: string;
    quantity: number;
  }>;
  analytics: {
    totalOnHand: number;
    inventoryValue: number;
    averageMonthlySales: number;
    averageMonthlyConsumption: number;
    daysOfCover?: number | null;
    reorderLevel: number;
    stockStatus: string;
  };
}
