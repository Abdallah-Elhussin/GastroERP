export type BackOfficeSalesOrderStatus = 0 | 1 | 2 | 8 | 9;
export type BackOfficeSalesFulfillmentStatus = 0 | 1 | 2 | 3 | 4 | 5;

export interface BackOfficeSalesOrderLine {
  id?: string;
  inventoryItemId?: string | null;
  unitId?: string | null;
  lineNature: number;
  description: string;
  quantity: number;
  unitPrice: number;
  unitCost: number;
  discountAmount: number;
  taxPercent: number;
  taxAmount: number;
  lineNet: number;
  deliveredQuantity?: number;
  invoicedQuantity?: number;
  remainingToDeliver?: number;
  remainingToInvoice?: number;
}

export interface BackOfficeSalesOrder {
  id: string;
  orderNumber: string;
  status: BackOfficeSalesOrderStatus | number | string;
  fulfillmentStatus: BackOfficeSalesFulfillmentStatus | number | string;
  customerId: string;
  branchId?: string | null;
  warehouseId?: string | null;
  salesPersonId?: string | null;
  quotationId?: string | null;
  orderDate: string;
  expectedDeliveryDate?: string | null;
  currency: string;
  exchangeRate: number;
  notes?: string | null;
  discountAmount: number;
  subTotal: number;
  taxAmount: number;
  totalAmount: number;
  approvedAt?: string | null;
  lines: BackOfficeSalesOrderLine[];
}

export interface CreateBackOfficeSalesOrderLineInput {
  description: string;
  quantity: number;
  unitPrice: number;
  inventoryItemId?: string | null;
  unitId?: string | null;
  lineNature?: number;
  taxPercent?: number;
  discountAmount?: number;
  unitCost?: number | null;
}

export interface CreateBackOfficeSalesOrderPayload {
  customerId: string;
  orderDate: string;
  orderNumber?: string | null;
  currency?: string;
  branchId?: string | null;
  warehouseId?: string | null;
  salesPersonId?: string | null;
  quotationId?: string | null;
  expectedDeliveryDate?: string | null;
  exchangeRate?: number;
  notes?: string | null;
  discountAmount?: number;
  lines: CreateBackOfficeSalesOrderLineInput[];
}

export interface UpdateBackOfficeSalesOrderPayload {
  orderDate: string;
  warehouseId?: string | null;
  salesPersonId?: string | null;
  branchId?: string | null;
  expectedDeliveryDate?: string | null;
  notes?: string | null;
  discountAmount?: number;
  lines: CreateBackOfficeSalesOrderLineInput[];
}

export interface BackOfficeSalesOrderListParams {
  page?: number;
  pageSize?: number;
  status?: number | null;
  fulfillmentStatus?: number | null;
  customerId?: string | null;
  search?: string;
  from?: string | null;
  to?: string | null;
}

export interface ConvertOrderToInvoiceLineSelection {
  orderLineId: string;
  quantity: number;
}

export interface ConvertOrderToInvoicePayload {
  paymentMode: number;
  nature?: number;
  invoiceDate?: string | null;
  warehouseId?: string | null;
  costCenterId?: string | null;
  dueDate?: string | null;
  invoiceNumber?: string | null;
  selection?: ConvertOrderToInvoiceLineSelection[];
}

export interface ConvertOrderToInvoiceResult {
  invoiceId: string;
}
