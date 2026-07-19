/** PostingSource from backend (`GastroErp.Domain.Enums.PostingSource`). */
export type GlPostingSource =
  | 1
  | 2
  | 3
  | 4
  | 5
  | 6
  | 7
  | 8
  | 9
  | 10
  | 11
  | 12;

export const GL_POSTING_SOURCES: { value: GlPostingSource; labelKey: string }[] = [
  { value: 8, labelKey: 'fin.ops.gl.source.openingBalance' },
  { value: 9, labelKey: 'fin.ops.gl.source.receipt' },
  { value: 10, labelKey: 'fin.ops.gl.source.payment' },
  { value: 1, labelKey: 'fin.ops.gl.source.manual' },
  { value: 2, labelKey: 'fin.ops.gl.source.sales' },
  { value: 4, labelKey: 'fin.ops.gl.source.purchase' },
  { value: 5, labelKey: 'fin.ops.gl.source.inventory' },
  { value: 11, labelKey: 'fin.ops.gl.source.debitNote' },
  { value: 12, labelKey: 'fin.ops.gl.source.creditNote' },
  { value: 3, labelKey: 'fin.ops.gl.source.paymentLegacy' },
  { value: 6, labelKey: 'fin.ops.gl.source.payroll' },
  { value: 7, labelKey: 'fin.ops.gl.source.crm' }
];

export interface GeneralLedgerLine {
  postingDate: string;
  entryNumber: string;
  description: string;
  debit: number;
  credit: number;
  runningBalance: number;
  journalEntryId?: string | null;
  sourceModule?: GlPostingSource | number | null;
  sourceDocumentId?: string | null;
  reference?: string | null;
  costCenterId?: string | null;
  costCenterNameAr?: string | null;
  costCenterNameEn?: string | null;
  chartOfAccountId?: string | null;
  accountNumber?: string | null;
  accountNameAr?: string | null;
  isOpeningBalance?: boolean;
}

export interface GeneralLedgerResult {
  openingBalance: number;
  totalDebit: number;
  totalCredit: number;
  closingBalance: number;
  totalCount: number;
  page: number;
  pageSize: number;
  lines: GeneralLedgerLine[];
}

export interface GeneralLedgerFilter {
  accountId?: string;
  companyId?: string;
  branchId?: string;
  fiscalPeriodId?: string;
  fiscalYear?: number;
  fromDate?: string;
  toDate?: string;
  costCenterId?: string;
  parentAccountId?: string;
  accountType?: number;
  currency?: string;
  sourceModule?: GlPostingSource | number;
  documentNumber?: string;
  search?: string;
  includeOpeningBalance?: boolean;
  page?: number;
  pageSize?: number;
}
