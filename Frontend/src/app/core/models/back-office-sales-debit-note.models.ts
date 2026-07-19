export type BackOfficeSalesDebitNoteStatus = 0 | 1 | 2 | 8 | 9;

export interface BackOfficeSalesDebitNoteLine {
  id?: string;
  description: string;
  quantity: number;
  unitPrice: number;
  taxPercent: number;
  taxAmount: number;
  lineNet: number;
  lineTotal: number;
}

export interface BackOfficeSalesDebitNote {
  id: string;
  debitNoteNumber: string;
  status: BackOfficeSalesDebitNoteStatus | number | string;
  customerId: string;
  invoiceId?: string | null;
  branchId?: string | null;
  debitDate: string;
  currency: string;
  notes?: string | null;
  subTotal: number;
  taxAmount: number;
  totalAmount: number;
  journalEntryId?: string | null;
  reversalJournalEntryId?: string | null;
  approvedAt?: string | null;
  postedAt?: string | null;
  lines: BackOfficeSalesDebitNoteLine[];
}

export interface CreateBackOfficeSalesDebitNoteLineInput {
  description: string;
  quantity: number;
  unitPrice: number;
  taxPercent?: number;
}

export interface CreateBackOfficeSalesDebitNotePayload {
  customerId: string;
  debitDate: string;
  debitNoteNumber?: string | null;
  currency?: string;
  invoiceId?: string | null;
  branchId?: string | null;
  notes?: string | null;
  lines: CreateBackOfficeSalesDebitNoteLineInput[];
}

export interface UpdateBackOfficeSalesDebitNotePayload {
  debitDate: string;
  invoiceId?: string | null;
  branchId?: string | null;
  notes?: string | null;
  lines: CreateBackOfficeSalesDebitNoteLineInput[];
}

export interface BackOfficeSalesDebitNoteListParams {
  page?: number;
  pageSize?: number;
  status?: number | null;
  customerId?: string | null;
  invoiceId?: string | null;
  search?: string;
  from?: string | null;
  to?: string | null;
}
