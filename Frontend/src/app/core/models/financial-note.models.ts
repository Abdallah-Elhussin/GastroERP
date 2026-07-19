export type FinancialNoteKind = 1 | 2; // Debit | Credit
export type FinancialNoteStatus = 1 | 2 | 3 | 4 | 5 | 6;
export type FinancialNoteReferenceType = 0 | 1 | 2 | 3 | 4 | 5;

export interface FinancialNoteLine {
  id?: string | null;
  notificationReasonId: string;
  reasonCode?: string | null;
  reasonNameAr?: string | null;
  offsetAccountId: string;
  offsetAccountNumber?: string | null;
  offsetAccountNameAr?: string | null;
  costCenterId?: string | null;
  costCenterNameAr?: string | null;
  analyticalAccountId?: string | null;
  analyticalAccountNameAr?: string | null;
  currency: string;
  exchangeRate: number;
  amount: number;
  amountInBase: number;
  description?: string | null;
}

export interface FinancialNote {
  id: string;
  number: number;
  documentNumber: string;
  noteKind: FinancialNoteKind;
  companyId: string;
  companyNameAr?: string | null;
  branchId: string;
  branchNameAr?: string | null;
  noteDate: string;
  fiscalPeriodId: string;
  fiscalYear?: number | null;
  partyType: number;
  partyId?: string | null;
  partyName?: string | null;
  mainAccountId: string;
  mainAccountNumber?: string | null;
  mainAccountNameAr?: string | null;
  currency: string;
  exchangeRate: number;
  referenceType: FinancialNoteReferenceType;
  referenceDocumentId?: string | null;
  referenceNumber?: string | null;
  description?: string | null;
  notes?: string | null;
  status: FinancialNoteStatus;
  journalEntryId?: string | null;
  journalEntryNumber?: string | null;
  linesCount: number;
  totalAmount: number;
  totalAmountInBase: number;
  createdAt: string;
  postedAt?: string | null;
  approvedAt?: string | null;
  lines: FinancialNoteLine[];
}

export interface UpsertFinancialNotePayload {
  noteKind: FinancialNoteKind;
  companyId: string;
  branchId: string;
  noteDate: string;
  fiscalPeriodId: string;
  partyType: number;
  mainAccountId: string;
  currency: string;
  exchangeRate: number;
  partyId?: string | null;
  partyName?: string | null;
  referenceType?: FinancialNoteReferenceType;
  referenceDocumentId?: string | null;
  referenceNumber?: string | null;
  description?: string | null;
  notes?: string | null;
  lines: {
    notificationReasonId: string;
    offsetAccountId: string;
    costCenterId?: string | null;
    analyticalAccountId?: string | null;
    currency: string;
    exchangeRate: number;
    amount: number;
    description?: string | null;
  }[];
}

export interface FinancialNoteListFilter {
  search?: string;
  companyId?: string | null;
  branchId?: string | null;
  noteKind?: number | null;
  status?: number | null;
  partyType?: number | null;
  fromDate?: string | null;
  toDate?: string | null;
  pageSize?: number;
}

export const NOTE_KINDS: { value: FinancialNoteKind; labelKey: string }[] = [
  { value: 1, labelKey: 'fin.ops.note.kind.debit' },
  { value: 2, labelKey: 'fin.ops.note.kind.credit' }
];

export const NOTE_STATUSES: { value: FinancialNoteStatus; labelKey: string }[] = [
  { value: 1, labelKey: 'fin.ops.note.status.draft' },
  { value: 2, labelKey: 'fin.ops.note.status.submitted' },
  { value: 3, labelKey: 'fin.ops.note.status.approved' },
  { value: 4, labelKey: 'fin.ops.note.status.posted' },
  { value: 5, labelKey: 'fin.ops.note.status.reversed' },
  { value: 6, labelKey: 'fin.ops.note.status.cancelled' }
];
