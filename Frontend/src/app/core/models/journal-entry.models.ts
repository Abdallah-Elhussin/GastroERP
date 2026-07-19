export type JournalStatus = 1 | 2 | 3 | 4;
export type PostingSource = 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12;
export type JournalVoucherType = 1 | 2 | 3 | 4 | 5;

export const JOURNAL_STATUSES: { value: JournalStatus; labelKey: string }[] = [
  { value: 1, labelKey: 'fin.ops.je.status.draft' },
  { value: 4, labelKey: 'fin.ops.je.status.approved' },
  { value: 2, labelKey: 'fin.ops.je.status.posted' },
  { value: 3, labelKey: 'fin.ops.je.status.reversed' }
];

/** Selectable when creating/editing manual vouchers (Opening/Reversal are system-generated). */
export const JOURNAL_VOUCHER_TYPES: { value: JournalVoucherType; labelKey: string; selectable: boolean }[] = [
  { value: 1, labelKey: 'fin.ops.je.type.ordinary', selectable: true },
  { value: 2, labelKey: 'fin.ops.je.type.adjustment', selectable: true },
  { value: 3, labelKey: 'fin.ops.je.type.closing', selectable: true },
  { value: 4, labelKey: 'fin.ops.je.type.opening', selectable: false },
  { value: 5, labelKey: 'fin.ops.je.type.reversal', selectable: false }
];

export const POSTING_SOURCES: { value: PostingSource; labelKey: string }[] = [
  { value: 1, labelKey: 'fin.ops.je.source.manual' },
  { value: 2, labelKey: 'fin.ops.je.source.sales' },
  { value: 3, labelKey: 'fin.ops.je.source.payment' },
  { value: 4, labelKey: 'fin.ops.je.source.purchase' },
  { value: 5, labelKey: 'fin.ops.je.source.inventory' },
  { value: 6, labelKey: 'fin.ops.je.source.payroll' },
  { value: 7, labelKey: 'fin.ops.je.source.crm' },
  { value: 8, labelKey: 'fin.ops.je.source.openingBalance' },
  { value: 9, labelKey: 'fin.ops.je.source.receipt' },
  { value: 10, labelKey: 'fin.ops.je.source.paymentVoucher' },
  { value: 11, labelKey: 'fin.ops.je.source.debitNote' },
  { value: 12, labelKey: 'fin.ops.je.source.creditNote' }
];

export interface JournalLine {
  id?: string | null;
  chartOfAccountId: string;
  accountNumber?: string;
  accountName?: string;
  costCenterId?: string | null;
  analyticalAccountId?: string | null;
  debit: number;
  credit: number;
  currency?: string;
  exchangeRate?: number;
  description?: string | null;
  lineNumber?: number;
}

export interface JournalEntry {
  id: string;
  entryNumber: string;
  postingDate: string;
  fiscalPeriodId: string;
  description: string;
  reference?: string | null;
  voucherType?: JournalVoucherType | number;
  sourceModule: PostingSource | number;
  sourceDocumentId?: string | null;
  status: JournalStatus | number;
  postedAt?: string | null;
  totalDebit: number;
  totalCredit: number;
  companyId?: string | null;
  branchId?: string | null;
  createdBy?: string | null;
  createdAt?: string | null;
  lines?: JournalLine[];
}

export interface CreateJournalPayload {
  postingDate: string;
  description: string;
  sourceModule: number;
  branchId?: string | null;
  companyId?: string | null;
  reference?: string | null;
  voucherType?: number;
  fiscalPeriodId?: string | null;
  lines: JournalLine[];
}

export interface UpdateJournalPayload {
  postingDate: string;
  description: string;
  branchId?: string | null;
  companyId?: string | null;
  reference?: string | null;
  voucherType?: number;
  fiscalPeriodId?: string | null;
  lines: JournalLine[];
}

export interface JournalListFilter {
  search?: string;
  status?: number | null;
  sourceModule?: number | null;
  voucherType?: number | null;
  companyId?: string | null;
  branchId?: string | null;
  fiscalPeriodId?: string | null;
  fiscalYear?: number | null;
  entryNumber?: string | null;
  fromDate?: string | null;
  toDate?: string | null;
  page?: number;
  pageSize?: number;
}
