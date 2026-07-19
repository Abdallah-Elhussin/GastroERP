export type OpeningBalanceStatus = 'Draft' | 'Approved' | 'Posted' | string;

export interface OpeningBalanceLine {
  id?: string;
  inventoryItemId: string;
  itemNameAr?: string | null;
  itemSku?: string | null;
  warehouseId: string;
  warehouseNameAr?: string | null;
  unitId: string;
  unitNameAr?: string | null;
  quantity: number;
  unitCost: number;
  batchNumber?: string | null;
  expiryDate?: string | null;
  serialNumber?: string | null;
}

export interface OpeningBalanceDoc {
  id: string;
  tenantId: string;
  warehouseId?: string | null;
  warehouseNameAr?: string | null;
  documentNumber: string;
  documentDate: string;
  approvalDate?: string | null;
  notes?: string | null;
  status: OpeningBalanceStatus;
  statusCode: number;
  entryMethod: string;
  displayMethod: string;
  costingMethod: string;
  weightedAverageScope: string;
  useExpiryDate: boolean;
  useBatchNumbers: boolean;
  useSerialNumbers: boolean;
  contraAccountId?: string | null;
  contraAccountName?: string | null;
  costCenterId?: string | null;
  costCenterNameAr?: string | null;
  isApproved: boolean;
  isPosted: boolean;
  lineCount: number;
  lines: OpeningBalanceLine[];
  createdAt: string;
}

export interface OpeningBalanceLineInput {
  inventoryItemId: string;
  unitId: string;
  quantity: number;
  unitCost: number;
  warehouseId?: string | null;
  batchNumber?: string | null;
  expiryDate?: string | null;
  serialNumber?: string | null;
}

export interface CreateOpeningBalancePayload {
  documentNumber?: string | null;
  autoGenerateNumber?: boolean;
  documentDate?: string | null;
  warehouseId?: string | null;
  notes?: string | null;
  entryMethod?: number;
  displayMethod?: number;
  costingMethod?: number;
  weightedAverageScope?: number;
  useExpiryDate?: boolean;
  useBatchNumbers?: boolean;
  useSerialNumbers?: boolean;
  contraAccountId?: string | null;
  costCenterId?: string | null;
  lines?: OpeningBalanceLineInput[];
}

export interface UpdateOpeningBalancePayload {
  documentDate: string;
  warehouseId?: string | null;
  notes?: string | null;
  entryMethod?: number;
  displayMethod?: number;
  costingMethod?: number;
  weightedAverageScope?: number;
  useExpiryDate?: boolean;
  useBatchNumbers?: boolean;
  useSerialNumbers?: boolean;
  contraAccountId?: string | null;
  costCenterId?: string | null;
  lines?: OpeningBalanceLineInput[];
}

export interface AccountLookup {
  id: string;
  accountNumber?: string;
  code?: string;
  nameAr: string;
  nameEn?: string | null;
}
