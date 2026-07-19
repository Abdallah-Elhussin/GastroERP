export interface BankAccountDetail {
  id?: string | null;
  currencyId: string;
  currencyCode?: string | null;
  accountNumber: string;
  iban?: string | null;
  minBalance?: number | null;
  maxBalance?: number | null;
  minTransaction?: number | null;
  maxTransaction?: number | null;
  dailyTransferLimit?: number | null;
  allowExceedLimits: boolean;
  allowWithdraw: boolean;
  allowDeposit: boolean;
  allowTransfer: boolean;
  isDefault: boolean;
  isActive: boolean;
  sortOrder: number;
}

export interface Bank {
  id: string;
  number: number;
  nameAr: string;
  nameEn?: string | null;
  code?: string | null;
  swiftCode?: string | null;
  defaultIban?: string | null;
  companyId: string;
  companyNameAr?: string | null;
  branchId: string;
  branchNameAr?: string | null;
  chartOfAccountId: string;
  accountNumber?: string | null;
  accountNameAr?: string | null;
  baseCurrencyId: string;
  baseCurrencyCode?: string | null;
  isActive: boolean;
  deactivatedAt?: string | null;
  deactivationReason?: string | null;
  isSystem: boolean;
  sortOrder: number;
  accountsCount: number;
  accounts: BankAccountDetail[];
}

export interface UpsertBankPayload {
  nameAr: string;
  nameEn?: string | null;
  code?: string | null;
  swiftCode?: string | null;
  defaultIban?: string | null;
  companyId: string;
  branchId: string;
  chartOfAccountId: string;
  baseCurrencyId: string;
  isActive: boolean;
  deactivatedAt?: string | null;
  deactivationReason?: string | null;
  sortOrder: number;
  accounts: BankAccountDetail[];
}

export interface BankListFilter {
  search?: string;
  companyId?: string | null;
  branchId?: string | null;
  currencyId?: string | null;
  isActive?: boolean | null;
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
