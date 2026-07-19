export type BackOfficeSalesDeliveryNoteStatus = 0 | 1 | 2 | 8 | 9;

export interface BackOfficeSalesDeliveryNoteLine {
  id?: string;
  orderLineId: string;
  inventoryItemId?: string | null;
  unitId?: string | null;
  description: string;
  quantity: number;
  unitCost: number;
  lineCost: number;
}

export interface BackOfficeSalesDeliveryNote {
  id: string;
  deliveryNumber: string;
  status: BackOfficeSalesDeliveryNoteStatus | number | string;
  customerId: string;
  warehouseId: string;
  orderId: string;
  branchId?: string | null;
  deliveryDate: string;
  notes?: string | null;
  totalCost: number;
  journalEntryId?: string | null;
  reversalJournalEntryId?: string | null;
  approvedAt?: string | null;
  postedAt?: string | null;
  lines: BackOfficeSalesDeliveryNoteLine[];
}

export interface CreateBackOfficeSalesDeliveryNoteLineInput {
  orderLineId: string;
  description: string;
  quantity: number;
  inventoryItemId?: string | null;
  unitId?: string | null;
  unitCost?: number;
}

export interface CreateBackOfficeSalesDeliveryNotePayload {
  orderId: string;
  customerId: string;
  warehouseId: string;
  deliveryDate: string;
  deliveryNumber?: string | null;
  branchId?: string | null;
  notes?: string | null;
  lines: CreateBackOfficeSalesDeliveryNoteLineInput[];
}

export interface UpdateBackOfficeSalesDeliveryNotePayload {
  deliveryDate: string;
  warehouseId?: string | null;
  branchId?: string | null;
  notes?: string | null;
  lines: CreateBackOfficeSalesDeliveryNoteLineInput[];
}

export interface BackOfficeSalesDeliveryNoteListParams {
  page?: number;
  pageSize?: number;
  status?: number | null;
  customerId?: string | null;
  orderId?: string | null;
  warehouseId?: string | null;
  search?: string;
  from?: string | null;
  to?: string | null;
}
