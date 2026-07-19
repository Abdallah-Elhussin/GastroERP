/**
 * PurchaseOrderStatus (backend byte enum, GastroErp.Domain.Enums.PurchaseOrderStatus):
 * Draft=1, Approved=2, SentToSupplier=3, PartiallyReceived=4, FullyReceived=5,
 * Cancelled=6, Closed=7, Rejected=8, PendingApproval=9
 */
export type PurchaseOrderStatusCode = 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9;

export const PURCHASE_ORDER_STATUS = {
  Draft: 1,
  Approved: 2,
  SentToSupplier: 3,
  PartiallyReceived: 4,
  FullyReceived: 5,
  Cancelled: 6,
  Closed: 7,
  Rejected: 8,
  PendingApproval: 9
} as const;

export type PurchaseOrderStatusName = keyof typeof PURCHASE_ORDER_STATUS;

export interface PurchaseOrderLine {
  id?: string;
  inventoryItemId: string;
  itemNameAr?: string | null;
  itemSku?: string | null;
  unitId: string;
  unitNameAr?: string | null;
  warehouseId?: string | null;
  quantity: number;
  unitPrice: number;
  discountAmount: number;
  taxAmount: number;
  lineSubTotal: number;
  lineTotal: number;
  receivedQuantity: number;
  invoicedQuantity: number;
  remainingQuantity: number;
  description?: string | null;
  lineNotes?: string | null;
}

export interface PurchaseOrderDto {
  id: string;
  tenantId: string;
  supplierId: string;
  supplierNameAr?: string | null;
  destinationWarehouseId: string;
  warehouseNameAr?: string | null;
  branchId?: string | null;
  costCenterId?: string | null;
  responsibleEmployeeId?: string | null;
  poNumber: string;
  orderType: number;
  orderDate: string;
  expectedDeliveryDate?: string | null;
  status: PurchaseOrderStatusCode | number;
  statusCode: number;
  currency: string;
  exchangeRate: number;
  paymentMethod?: string | null;
  paymentTerms?: string | null;
  externalReference?: string | null;
  notes?: string | null;
  totalAmount: number;
  completionPercent: number;
  remainingQuantity: number;
  lineCount: number;
  lastReceiptDate?: string | null;
  createdAt: string;
  lines: PurchaseOrderLine[];
}

export interface PurchaseOrderLineInput {
  inventoryItemId: string;
  unitId: string;
  quantity: number;
  unitPrice: number;
  discountAmount?: number;
  taxAmount?: number;
  description?: string | null;
  warehouseId?: string | null;
  lineNotes?: string | null;
}

export interface CreatePurchaseOrderPayload {
  supplierId: string;
  destinationWarehouseId: string;
  poNumber?: string | null;
  orderDate?: string | null;
  expectedDeliveryDate?: string | null;
  currency?: string;
  exchangeRate?: number;
  orderType?: number;
  branchId?: string | null;
  costCenterId?: string | null;
  responsibleEmployeeId?: string | null;
  paymentMethod?: string | null;
  paymentTerms?: string | null;
  externalReference?: string | null;
  notes?: string | null;
  lines?: PurchaseOrderLineInput[];
}

export interface UpdatePurchaseOrderPayload {
  supplierId: string;
  destinationWarehouseId: string;
  orderDate: string;
  expectedDeliveryDate?: string | null;
  currency: string;
  exchangeRate: number;
  orderType: number;
  branchId?: string | null;
  costCenterId?: string | null;
  responsibleEmployeeId?: string | null;
  paymentMethod?: string | null;
  paymentTerms?: string | null;
  externalReference?: string | null;
  notes?: string | null;
  lines: PurchaseOrderLineInput[];
}

export interface PurchaseOrderListParams {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: number | null;
  supplierId?: string | null;
  warehouseId?: string | null;
  from?: string | null;
  to?: string | null;
}

export interface PurchaseOrderPage {
  items: PurchaseOrderDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface PurchaseOrderDashboardDto {
  ordersToday: number;
  approvedCount: number;
  awaitingReceiptCount: number;
  closedCount: number;
  overdueCount: number;
  totalValue: number;
}

/** Editable line row used by the purchase order form (adds display-only fields). */
export interface PurchaseOrderLineDraft extends PurchaseOrderLine {
  itemNameAr?: string | null;
  unitNameAr?: string | null;
  /** UI-only: used to derive taxAmount from line subtotal. */
  taxPercent?: number;
  /** UI-only: 1 = taxable, 0 = exempt. */
  taxStatus?: 0 | 1;
}
