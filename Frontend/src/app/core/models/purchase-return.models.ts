export type PurchaseReturnTypeCode = 1 | 2 | 3;
export type PurchasingDocumentStatusCode = 0 | 1 | 2 | 8 | 9;

export interface PurchaseReturnLine {
  id?: string;
  inventoryItemId: string;
  itemNameAr?: string | null;
  itemSku?: string | null;
  unitId: string;
  unitNameAr?: string | null;
  goodsReceiptLineId?: string | null;
  purchaseInvoiceLineId?: string | null;
  originalQuantity: number;
  previouslyReturnedQuantity: number;
  availableToReturn: number;
  returnQuantity: number;
  unitCost: number;
  discountAmount: number;
  taxPercent: number;
  taxAmount: number;
  lineSubTotal: number;
  lineTotal: number;
  batchNumber?: string | null;
  expiryDate?: string | null;
  lineReason?: string | null;
  notes?: string | null;
  productTemperature?: number | null;
  destroyItem: boolean;
}

export interface PurchaseReturnDoc {
  id: string;
  tenantId: string;
  branchId?: string | null;
  supplierId: string;
  supplierNameAr: string;
  warehouseId: string;
  warehouseNameAr: string;
  returnNumber: string;
  returnDate: string;
  returnType: PurchaseReturnTypeCode | number;
  status: PurchasingDocumentStatusCode | number | string;
  unifiedStatusCode: number;
  goodsReceiptId?: string | null;
  goodsReceiptNumber?: string | null;
  purchaseInvoiceId?: string | null;
  purchaseInvoiceNumber?: string | null;
  returnReasonId?: string | null;
  returnReasonNameAr?: string | null;
  reasonNotes?: string | null;
  referenceNumber?: string | null;
  notes?: string | null;
  currency: string;
  subTotal: number;
  taxAmount: number;
  totalAmount: number;
  journalEntryId?: string | null;
  creditNoteJournalEntryId?: string | null;
  isCompleted: boolean;
  lineCount: number;
  lines: PurchaseReturnLine[];
  createdAt: string;
}

export interface PurchaseReturnReason {
  id: string;
  code: string;
  nameAr: string;
  nameEn?: string | null;
  sortOrder: number;
  isActive: boolean;
}

export interface PurchaseReturnLineInput {
  inventoryItemId: string;
  unitId: string;
  originalQuantity: number;
  previouslyReturnedQuantity: number;
  returnQuantity: number;
  unitCost: number;
  discountAmount?: number;
  taxPercent?: number;
  taxAmount?: number;
  goodsReceiptLineId?: string | null;
  purchaseInvoiceLineId?: string | null;
  batchNumber?: string | null;
  expiryDate?: string | null;
  lineReason?: string | null;
  notes?: string | null;
  productTemperature?: number | null;
  destroyItem?: boolean;
}

export interface CreatePurchaseReturnPayload {
  returnType: number;
  warehouseId: string;
  returnNumber?: string | null;
  goodsReceiptId?: string | null;
  purchaseInvoiceId?: string | null;
  returnDate?: string | null;
  returnReasonId?: string | null;
  reasonNotes?: string | null;
  referenceNumber?: string | null;
  notes?: string | null;
  currency?: string;
  lines?: PurchaseReturnLineInput[];
}

export interface UpdatePurchaseReturnPayload {
  returnDate: string;
  returnReasonId?: string | null;
  reasonNotes?: string | null;
  referenceNumber?: string | null;
  notes?: string | null;
  lines?: PurchaseReturnLineInput[];
}

export interface PurchaseReturnListParams {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: number | null;
  returnType?: number | null;
  /** AfterInvoice + Direct only (purchase / direct invoice returns). */
  invoiceBasedOnly?: boolean;
  from?: string | null;
  to?: string | null;
  supplierId?: string | null;
}

/** Response of GET .../invoice-for-return/{id} */
export interface PurchaseInvoiceForReturnHeader {
  id: string;
  invoiceNumber: string;
  kind: number;
  status: number;
  paymentMode: number;
  nature: number;
  supplierId: string;
  supplierNameAr: string;
  warehouseId?: string | null;
  warehouseNameAr?: string | null;
  costCenterId?: string | null;
  costCenterNameAr?: string | null;
  invoiceDate: string;
  dueDate?: string | null;
  currency: string;
  exchangeRate: number;
  supplierInvoiceNumber?: string | null;
  externalReference?: string | null;
  notes?: string | null;
  apAccountId?: string | null;
  apAccountNameAr?: string | null;
  discountAmount: number;
  subTotal: number;
  taxAmount: number;
  totalAmount: number;
  canCreateReturn: boolean;
  blockReason?: string | null;
  blockReasonCode?: string | null;
}

export interface PurchaseInvoiceForReturnLine {
  purchaseInvoiceLineId: string;
  inventoryItemId: string;
  itemNameAr?: string | null;
  itemSku?: string | null;
  description?: string | null;
  unitId: string;
  unitNameAr?: string | null;
  warehouseId?: string | null;
  warehouseNameAr?: string | null;
  originalQuantity: number;
  previouslyReturnedQuantity: number;
  remainingQuantity: number;
  returnQuantity: number;
  unitPrice: number;
  discountPercent: number;
  discountAmount: number;
  taxPercent: number;
  taxAmount: number;
  lineSubTotal: number;
  lineTotal: number;
  isDisabled: boolean;
}

export interface PurchaseInvoiceForReturnTax {
  taxPercent: number;
  taxableAmount: number;
  taxAmount: number;
}

export interface PurchaseInvoiceForReturn {
  header: PurchaseInvoiceForReturnHeader;
  items: PurchaseInvoiceForReturnLine[];
  taxes: PurchaseInvoiceForReturnTax[];
  totalRemainingQuantity: number;
  invoiceTotalAmount: number;
}
