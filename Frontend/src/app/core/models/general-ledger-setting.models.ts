export type ClosingMethod = 1 | 2 | 3 | 4;

export const CLOSING_METHODS: { value: ClosingMethod; code: string; labelKey: string }[] = [
  { value: 1, code: 'SINGLE_SUMMARY', labelKey: 'fin.gl.closing.single' },
  { value: 2, code: 'DIRECT_TO_RETAINED_EARNINGS', labelKey: 'fin.gl.closing.retained' },
  { value: 3, code: 'BY_PROFIT_CENTER', labelKey: 'fin.gl.closing.profitCenter' },
  { value: 4, code: 'BY_BRANCH', labelKey: 'fin.gl.closing.branch' }
];

export interface GeneralLedgerSetting {
  id: string;
  number: number;
  companyId: string;
  companyNameAr?: string | null;
  branchId: string;
  branchNameAr?: string | null;
  voucherNumberLength: number;
  decimalPlaces: number;
  showDateInReports: boolean;
  showPostingIndicator: boolean;
  autoPostReceiptChecks: boolean;
  autoPostPaymentChecks: boolean;
  useBudgetPerCurrency: boolean;
  useAnalyticalAccounts: boolean;
  allowZeroEffectEntries: boolean;
  requireJournalType: boolean;
  allowManualTaxEntries: boolean;
  requireReferenceNumber: boolean;
  closingMethod: ClosingMethod | number;
  closingMethodCode: string;
  isSystem: boolean;
  createdAt: string;
  createdBy?: string | null;
  updatedAt?: string | null;
  updatedBy?: string | null;
}

export interface UpsertGeneralLedgerSettingPayload {
  companyId: string;
  branchId: string;
  voucherNumberLength: number;
  decimalPlaces: number;
  showDateInReports: boolean;
  showPostingIndicator: boolean;
  autoPostReceiptChecks: boolean;
  autoPostPaymentChecks: boolean;
  useBudgetPerCurrency: boolean;
  useAnalyticalAccounts: boolean;
  allowZeroEffectEntries: boolean;
  requireJournalType: boolean;
  allowManualTaxEntries: boolean;
  requireReferenceNumber: boolean;
  closingMethod: number;
}

export interface GeneralLedgerSettingListFilter {
  search?: string;
  companyId?: string | null;
  branchId?: string | null;
  pageSize?: number;
}

export interface OrgCompanyLookup {
  id: string;
  nameAr: string;
  nameEn?: string | null;
  isActive?: boolean;
}

export interface OrgBranchLookup {
  id: string;
  companyId: string;
  nameAr: string;
  nameEn?: string | null;
  code?: string | null;
}
