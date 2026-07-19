export type BackOfficeSalesQuotationStatus = 0 | 1 | 2 | 8 | 9;

export interface BackOfficeSalesQuotationLine {
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
  lineTotal: number;
}

export interface BackOfficeSalesQuotation {
  id: string;
  quotationNumber: string;
  status: BackOfficeSalesQuotationStatus | number | string;
  customerId: string;
  branchId?: string | null;
  warehouseId?: string | null;
  salesPersonId?: string | null;
  quotationDate: string;
  validUntil?: string | null;
  currency: string;
  exchangeRate: number;
  notes?: string | null;
  discountAmount: number;
  subTotal: number;
  taxAmount: number;
  totalAmount: number;
  convertedOrderId?: string | null;
  approvedAt?: string | null;
  isExpired: boolean;
  lines: BackOfficeSalesQuotationLine[];
}

export interface CreateBackOfficeSalesQuotationLineInput {
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

export interface CreateBackOfficeSalesQuotationPayload {
  customerId: string;
  quotationDate: string;
  quotationNumber?: string | null;
  currency?: string;
  branchId?: string | null;
  warehouseId?: string | null;
  salesPersonId?: string | null;
  validUntil?: string | null;
  exchangeRate?: number;
  notes?: string | null;
  discountAmount?: number;
  lines: CreateBackOfficeSalesQuotationLineInput[];
}

export interface UpdateBackOfficeSalesQuotationPayload {
  quotationDate: string;
  warehouseId?: string | null;
  salesPersonId?: string | null;
  branchId?: string | null;
  validUntil?: string | null;
  notes?: string | null;
  discountAmount?: number;
  lines: CreateBackOfficeSalesQuotationLineInput[];
}

export interface BackOfficeSalesQuotationListParams {
  page?: number;
  pageSize?: number;
  status?: number | null;
  customerId?: string | null;
  search?: string;
  from?: string | null;
  to?: string | null;
}

export interface ConvertQuotationToOrderPayload {
  orderDate?: string | null;
  expectedDeliveryDate?: string | null;
  orderNumber?: string | null;
}

export interface ConvertQuotationToOrderResult {
  orderId: string;
}
