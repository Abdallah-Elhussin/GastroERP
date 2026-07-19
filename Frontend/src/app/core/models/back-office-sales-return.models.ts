export type BackOfficeSalesReturnStatus = 0 | 1 | 2 | 8 | 9;

export interface BackOfficeSalesReturnLine {
  id?: string;
  invoiceLineId: string;
  inventoryItemId?: string | null;
  unitId?: string | null;
  lineNature: number;
  description: string;
  quantity: number;
  unitPrice: number;
  unitCost: number;
  taxPercent: number;
  taxAmount: number;
  lineNet: number;
  lineTotal: number;
}

export interface BackOfficeSalesReturn {
  id: string;
  returnNumber: string;
  status: BackOfficeSalesReturnStatus | number | string;
  customerId: string;
  warehouseId?: string | null;
  invoiceId: string;
  branchId?: string | null;
  returnDate: string;
  notes?: string | null;
  discountAmount: number;
  subTotal: number;
  taxAmount: number;
  totalAmount: number;
  journalEntryId?: string | null;
  reversalJournalEntryId?: string | null;
  approvedAt?: string | null;
  postedAt?: string | null;
  lines: BackOfficeSalesReturnLine[];
}

export interface CreateBackOfficeSalesReturnLineInput {
  invoiceLineId: string;
  description: string;
  quantity: number;
  unitPrice: number;
  inventoryItemId?: string | null;
  unitId?: string | null;
  lineNature?: number;
  taxPercent?: number;
  unitCost?: number | null;
}

export interface CreateBackOfficeSalesReturnPayload {
  invoiceId: string;
  customerId: string;
  returnDate: string;
  returnNumber?: string | null;
  warehouseId?: string | null;
  branchId?: string | null;
  notes?: string | null;
  discountAmount?: number;
  lines: CreateBackOfficeSalesReturnLineInput[];
}

export interface UpdateBackOfficeSalesReturnPayload {
  returnDate: string;
  warehouseId?: string | null;
  branchId?: string | null;
  notes?: string | null;
  discountAmount?: number;
  lines: CreateBackOfficeSalesReturnLineInput[];
}

export interface BackOfficeSalesReturnListParams {
  page?: number;
  pageSize?: number;
  status?: number | null;
  customerId?: string | null;
  invoiceId?: string | null;
  search?: string;
  from?: string | null;
  to?: string | null;
}
