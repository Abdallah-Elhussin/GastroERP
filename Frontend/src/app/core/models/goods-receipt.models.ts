export type GoodsReceiptStatusName =
  | 'Draft'
  | 'Approved'
  | 'Posted'
  | 'Reversed'
  | 'Cancelled'
  | string;

export type InspectionResultCode = 1 | 2 | 3;

export interface GoodsReceiptLine {
  id?: string;
  inventoryItemId: string;
  itemNameAr?: string | null;
  itemSku?: string | null;
  unitId: string;
  unitNameAr?: string | null;
  purchaseOrderLineId?: string | null;
  orderedQuantity: number;
  previouslyReceivedQuantity: number;
  remainingQuantity: number;
  receivedQuantity: number;
  acceptedQuantity: number;
  rejectedQuantity: number;
  unitCost: number;
  discountAmount: number;
  taxPercent: number;
  taxAmount: number;
  lineSubTotal: number;
  invoicedQuantity: number;
  batchNumber?: string | null;
  productionDate?: string | null;
  expiryDate?: string | null;
  storageLocation?: string | null;
  description?: string | null;
}

export interface GoodsReceiptDoc {
  id: string;
  tenantId: string;
  branchId?: string | null;
  purchaseOrderId?: string | null;
  poNumber: string;
  poCompletionPercent?: number | null;
  supplierId: string;
  supplierNameAr: string;
  warehouseId: string;
  warehouseNameAr: string;
  grnNumber: string;
  referenceNumber?: string | null;
  receiptDate: string;
  status: GoodsReceiptStatusName | number;
  unifiedStatusCode: number;
  source: number;
  currency: string;
  exchangeRate: number;
  receiptMethod?: string | null;
  receivedByName?: string | null;
  supplierRepName?: string | null;
  vehicleNumber?: string | null;
  waybillNumber?: string | null;
  notes?: string | null;
  inspectionResult: InspectionResultCode | number;
  inspectedBy?: string | null;
  inspectionDate?: string | null;
  qualityNotes?: string | null;
  rejectionReason?: string | null;
  qualityCertificateRef?: string | null;
  expiryCertificateRef?: string | null;
  journalEntryId?: string | null;
  lineCount: number;
  totalQuantity: number;
  totalValue: number;
  totalTax: number;
  grandTotal: number;
  isInvoiced: boolean;
  isPartiallyInvoiced: boolean;
  lines: GoodsReceiptLine[];
  createdAt: string;
}

export interface GoodsReceiptLineInput {
  inventoryItemId: string;
  unitId: string;
  receivedQuantity: number;
  unitCost: number;
  purchaseOrderLineId?: string | null;
  orderedQuantity?: number;
  previouslyReceivedQuantity?: number;
  acceptedQuantity?: number | null;
  rejectedQuantity?: number;
  discountAmount?: number;
  taxPercent?: number;
  taxAmount?: number;
  batchNumber?: string | null;
  productionDate?: string | null;
  expiryDate?: string | null;
  storageLocation?: string | null;
  description?: string | null;
}

export interface CreateGoodsReceiptPayload {
  warehouseId: string;
  grnNumber?: string | null;
  purchaseOrderId?: string | null;
  supplierId?: string | null;
  directReceipt?: boolean;
  receiptDate?: string | null;
  currency?: string;
  exchangeRate?: number;
  referenceNumber?: string | null;
  notes?: string | null;
  receiptMethod?: string | null;
  receivedByName?: string | null;
  supplierRepName?: string | null;
  vehicleNumber?: string | null;
  waybillNumber?: string | null;
  inspectionResult?: number;
  inspectedBy?: string | null;
  inspectionDate?: string | null;
  qualityNotes?: string | null;
  rejectionReason?: string | null;
  qualityCertificateRef?: string | null;
  expiryCertificateRef?: string | null;
  lines?: GoodsReceiptLineInput[];
}

export interface UpdateGoodsReceiptPayload {
  receiptDate: string;
  warehouseId: string;
  referenceNumber?: string | null;
  notes?: string | null;
  receiptMethod?: string | null;
  receivedByName?: string | null;
  supplierRepName?: string | null;
  vehicleNumber?: string | null;
  waybillNumber?: string | null;
  currency?: string;
  exchangeRate?: number;
  inspectionResult?: number;
  inspectedBy?: string | null;
  inspectionDate?: string | null;
  qualityNotes?: string | null;
  rejectionReason?: string | null;
  qualityCertificateRef?: string | null;
  expiryCertificateRef?: string | null;
  lines?: GoodsReceiptLineInput[];
}

export interface GoodsReceiptListParams {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: number | null;
  from?: string | null;
  to?: string | null;
  supplierId?: string | null;
}
