export type FinancialOpeningBalanceStatus = 1 | 2 | 3;

export const OB_STATUSES: { value: FinancialOpeningBalanceStatus; labelKey: string }[] = [
  { value: 1, labelKey: 'fin.ops.ob.status.draft' },
  { value: 2, labelKey: 'fin.ops.ob.status.posted' },
  { value: 3, labelKey: 'fin.ops.ob.status.reversed' }
];

export interface FinancialOpeningBalanceLine {
  id?: string | null;
  chartOfAccountId: string;
  accountNumber?: string | null;
  accountNameAr?: string | null;
  costCenterId?: string | null;
  costCenterNameAr?: string | null;
  debit: number;
  credit: number;
  currency: string;
  description?: string | null;
}

export interface FinancialOpeningBalance {
  id: string;
  number: number;
  documentNumber: string;
  companyId: string;
  companyNameAr?: string | null;
  branchId?: string | null;
  branchNameAr?: string | null;
  openingDate: string;
  fiscalPeriodId: string;
  fiscalYear?: number | null;
  description?: string | null;
  status: FinancialOpeningBalanceStatus | number;
  equityAccountId?: string | null;
  journalEntryId?: string | null;
  journalEntryNumber?: string | null;
  linesCount: number;
  totalDebit: number;
  totalCredit: number;
  createdAt: string;
  postedAt?: string | null;
  lines: FinancialOpeningBalanceLine[];
}

export interface UpsertFinancialOpeningBalancePayload {
  companyId: string;
  branchId?: string | null;
  openingDate: string;
  fiscalPeriodId: string;
  description?: string | null;
  equityAccountId?: string | null;
  lines: FinancialOpeningBalanceLine[];
}

export interface FinancialOpeningBalanceListFilter {
  search?: string;
  companyId?: string | null;
  branchId?: string | null;
  fiscalPeriodId?: string | null;
  status?: number | null;
  pageSize?: number;
}

export interface OrgCompanyLookup {
  id: string;
  nameAr: string;
  isActive?: boolean;
}

export interface OrgBranchLookup {
  id: string;
  companyId: string;
  nameAr: string;
  isActive?: boolean;
}

export interface FiscalPeriodLookup {
  id: string;
  fiscalYear: number;
  name: string;
  status: number;
}
