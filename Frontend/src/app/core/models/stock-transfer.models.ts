export type StockTransferStatus = 'Draft' | 'Approved' | 'InTransit' | 'Completed' | 'Cancelled' | string;

export interface StockTransferLine {
  id?: string;
  inventoryItemId: string;
  itemNameAr?: string | null;
  itemSku?: string | null;
  unitId: string;
  unitNameAr?: string | null;
  quantity: number;
  unitCost: number;
  lineTotal?: number;
  receivedQuantity?: number;
  batchNumber?: string | null;
}

export interface StockTransferDoc {
  id: string;
  tenantId: string;
  sourceWarehouseId: string;
  sourceWarehouseNameAr: string;
  destinationWarehouseId: string;
  destinationWarehouseNameAr: string;
  transferNumber: string;
  transferDate: string;
  transferType: string;
  transferTypeCode: number;
  status: StockTransferStatus;
  statusCode: number;
  notes?: string | null;
  lineCount: number;
  totalAmount: number;
  lines: StockTransferLine[];
  createdAt: string;
}

export interface StockTransferLineInput {
  inventoryItemId: string;
  unitId: string;
  quantity: number;
  unitCost?: number;
  batchNumber?: string | null;
}

export interface CreateStockTransferPayload {
  transferNumber?: string | null;
  autoGenerateNumber?: boolean;
  transferDate?: string | null;
  sourceWarehouseId: string;
  destinationWarehouseId: string;
  transferType?: number;
  notes?: string | null;
  lines?: StockTransferLineInput[];
}

export interface UpdateStockTransferPayload {
  transferDate: string;
  sourceWarehouseId: string;
  destinationWarehouseId: string;
  transferType?: number;
  notes?: string | null;
  lines?: StockTransferLineInput[];
}

export interface StockTransferListParams {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: number | null;
  from?: string | null;
  to?: string | null;
}
