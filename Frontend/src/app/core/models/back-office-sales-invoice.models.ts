export type BackOfficeSalesDocumentStatus = 0 | 1 | 2 | 8 | 9;
export type BackOfficeSalesInvoiceNature = 1 | 2 | 3 | 4 | 5;
export type BackOfficeSalesPaymentMode = 1 | 2;
export type BackOfficeSalesLineNature = 1 | 2 | 3 | 4;
export type BackOfficeSalesPaymentStatus = 1 | 2 | 3;

export interface BackOfficeSalesInvoiceLine {
  id?: string;
  inventoryItemId?: string | null;
  productId?: string | null;
  unitId?: string | null;
  lineWarehouseId?: string | null;
  costCenterId?: string | null;
  lineNature: BackOfficeSalesLineNature | number;
  description: string;
  quantity: number;
  unitPrice: number;
  unitCost: number;
  discountPercent: number;
  discountAmount: number;
  taxPercent: number;
  taxAmount: number;
  lineNet: number;
  lineTotal: number;
  returnedQuantity?: number;
  remainingToReturn?: number;
}

export interface BackOfficeSalesInvoice {
  id: string;
  invoiceNumber: string;
  nature: BackOfficeSalesInvoiceNature | number;
  paymentMode: BackOfficeSalesPaymentMode | number;
  status: BackOfficeSalesDocumentStatus | number | string;
  customerId: string;
  branchId?: string | null;
  warehouseId?: string | null;
  costCenterId?: string | null;
  salesPersonId?: string | null;
  invoiceDate: string;
  dueDate?: string | null;
  currency: string;
  exchangeRate: number;
  externalReference?: string | null;
  notes?: string | null;
  discountAmount: number;
  subTotal: number;
  taxAmount: number;
  totalAmount: number;
  paidAmount: number;
  remainingAmount: number;
  paymentStatus: BackOfficeSalesPaymentStatus | number;
  journalEntryId?: string | null;
  postedAt?: string | null;
  lines: BackOfficeSalesInvoiceLine[];
}

export interface CreateBackOfficeSalesInvoiceLineInput {
  description: string;
  quantity: number;
  unitPrice: number;
  lineNature?: number;
  inventoryItemId?: string | null;
  productId?: string | null;
  unitId?: string | null;
  lineWarehouseId?: string | null;
  discountPercent?: number;
  discountAmount?: number;
  taxPercent?: number;
  taxAmount?: number;
  unitCost?: number | null;
}

export interface CreateBackOfficeSalesInvoicePayload {
  customerId: string;
  invoiceDate: string;
  paymentMode: number;
  nature?: number;
  invoiceNumber?: string | null;
  currency?: string;
  branchId?: string | null;
  warehouseId?: string | null;
  dueDate?: string | null;
  exchangeRate?: number;
  externalReference?: string | null;
  notes?: string | null;
  discountAmount?: number;
  lines: CreateBackOfficeSalesInvoiceLineInput[];
}

export interface UpdateBackOfficeSalesInvoicePayload {
  invoiceDate: string;
  paymentMode: number;
  nature?: number;
  dueDate?: string | null;
  warehouseId?: string | null;
  exchangeRate?: number;
  externalReference?: string | null;
  notes?: string | null;
  discountAmount?: number;
  lines: CreateBackOfficeSalesInvoiceLineInput[];
}

export interface BackOfficeSalesInvoiceListParams {
  page?: number;
  pageSize?: number;
  status?: number | null;
  customerId?: string | null;
  search?: string;
  from?: string | null;
  to?: string | null;
}
