export interface InventoryValuationGroup {
  id: string;
  tenantId?: string;
  code: string;
  nameAr: string;
  nameEn?: string | null;
  description?: string | null;
  costCenterId?: string | null;
  costCenterNameAr?: string | null;
  sortOrder: number;
  isSystem: boolean;
  isActive: boolean;
  createdAt?: string;
}

export interface CreateInventoryValuationGroupPayload {
  code: string;
  nameAr: string;
  nameEn?: string | null;
  description?: string | null;
  costCenterId?: string | null;
  sortOrder?: number;
}

export interface UpdateInventoryValuationGroupPayload {
  nameAr: string;
  nameEn?: string | null;
  description?: string | null;
  costCenterId?: string | null;
  sortOrder?: number;
  isActive?: boolean;
}

export interface CostCenterLookup {
  id: string;
  code?: string;
  nameAr: string;
  nameEn?: string;
}
