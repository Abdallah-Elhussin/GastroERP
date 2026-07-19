/** PurchaseInvoiceKind: FromReceipt=1, Direct=2 */
export type PurchaseInvoiceKindCode = 1 | 2;

/** PurchaseInvoicePaymentMode: Credit=1, Cash=2 */
export type PurchaseInvoicePaymentModeCode = 1 | 2;

/** DirectPurchaseNature: Inventory=1, Services=2, FixedAssets=3 */
export type DirectPurchaseNatureCode = 1 | 2 | 3;

/** PurchasingDocumentStatus: Draft=0, Approved=1, Posted=2, Reversed=8, Cancelled=9 */
export type PurchasingDocumentStatusCode = 0 | 1 | 2 | 8 | 9;

/** PurchaseInvoicePaymentStatus: Unpaid=1, PartiallyPaid=2, FullyPaid=3, FullyReturned=4 */
export type PurchaseInvoicePaymentStatusCode = 1 | 2 | 3 | 4;

export interface PurchaseInvoiceLine {
  id?: string;
  inventoryItemId: string;
  itemNameAr?: string | null;
  itemSku?: string | null;
  unitId: string;
  unitNameAr?: string | null;
  quantity: number;
  unitPrice: number;
  discountPercent: number;
  discountAmount: number;
  taxPercent: number;
  taxAmount: number;
  lineNet: number;
  lineTotal: number;
  goodsReceiptLineId?: string | null;
  purchaseOrderLineId?: string | null;
  lineWarehouseId?: string | null;
  costCenterId?: string | null;
  batchNumber?: string | null;
  serialNumber?: string | null;
  productionDate?: string | null;
  expiryDate?: string | null;
  description?: string | null;
  returnedQuantity: number;
  remainingToReturn: number;
}

export interface PurchaseInvoiceDoc {
  id: string;
  invoiceNumber: string;
  kind: PurchaseInvoiceKindCode | number;
  paymentMode: PurchaseInvoicePaymentModeCode | number;
  nature: DirectPurchaseNatureCode | number;
  status: PurchasingDocumentStatusCode | number | string;
  supplierId: string;
  branchId?: string | null;
  purchaseOrderId?: string | null;
  goodsReceiptId?: string | null;
  warehouseId?: string | null;
  costCenterId?: string | null;
  invoiceDate: string;
  dueDate?: string | null;
  currency: string;
  exchangeRate: number;
  supplierInvoiceNumber?: string | null;
  externalReference?: string | null;
  notes?: string | null;
  discountAmount: number;
  subTotal: number;
  taxAmount: number;
  totalAmount: number;
  paidAmount: number;
  remainingAmount: number;
  paymentStatus: PurchaseInvoicePaymentStatusCode | number;
  journalEntryId?: string | null;
  reversalJournalEntryId?: string | null;
  postedAt?: string | null;
  lines: PurchaseInvoiceLine[];
}

export interface PurchaseInvoiceLineInput {
  inventoryItemId: string;
  unitId: string;
  quantity: number;
  unitPrice: number;
  taxAmount?: number;
  goodsReceiptLineId?: string | null;
  purchaseOrderLineId?: string | null;
  description?: string | null;
  discountPercent?: number;
  discountAmount?: number;
  taxPercent?: number;
  batchNumber?: string | null;
  serialNumber?: string | null;
  productionDate?: string | null;
  expiryDate?: string | null;
  lineWarehouseId?: string | null;
  costCenterId?: string | null;
}

export interface CreatePurchaseInvoicePayload {
  kind: number;
  paymentMode: number;
  supplierId: string;
  invoiceDate: string;
  invoiceNumber?: string | null;
  currency?: string;
  warehouseId?: string | null;
  purchaseOrderId?: string | null;
  goodsReceiptId?: string | null;
  dueDate?: string | null;
  supplierInvoiceNumber?: string | null;
  notes?: string | null;
  nature?: number;
  exchangeRate?: number;
  externalReference?: string | null;
  costCenterId?: string | null;
  branchId?: string | null;
  discountAmount?: number;
  lines?: PurchaseInvoiceLineInput[];
}

export interface UpdatePurchaseInvoicePayload {
  invoiceDate: string;
  paymentMode: number;
  dueDate?: string | null;
  supplierInvoiceNumber?: string | null;
  notes?: string | null;
  warehouseId?: string | null;
  nature?: number;
  exchangeRate?: number;
  externalReference?: string | null;
  costCenterId?: string | null;
  branchId?: string | null;
  discountAmount?: number;
  lines?: PurchaseInvoiceLineInput[];
}

export interface PurchaseInvoiceListParams {
  page?: number;
  pageSize?: number;
  kind?: number | null;
  status?: number | null;
  supplierId?: string | null;
  warehouseId?: string | null;
  paymentMode?: number | null;
  nature?: number | null;
  search?: string;
  from?: string | null;
  to?: string | null;
}

/** Editable line row used by the direct invoice form. */
export interface DirectInvoiceLineDraft extends PurchaseInvoiceLine {
  itemNameAr?: string | null;
  unitNameAr?: string | null;
}
