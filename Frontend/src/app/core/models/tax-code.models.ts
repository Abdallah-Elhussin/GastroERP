export type TaxAppliesTo = 1 | 2 | 3;
export type TaxCalculationMethod = 1 | 2 | 3;

export const TAX_APPLIES_TO: { value: TaxAppliesTo; labelKey: string }[] = [
  { value: 1, labelKey: 'fin.taxCode.applies.sales' },
  { value: 2, labelKey: 'fin.taxCode.applies.purchases' },
  { value: 3, labelKey: 'fin.taxCode.applies.both' }
];

export const TAX_CALC_METHODS: { value: TaxCalculationMethod; labelKey: string }[] = [
  { value: 1, labelKey: 'fin.taxCode.method.standard' },
  { value: 2, labelKey: 'fin.taxCode.method.exempt' },
  { value: 3, labelKey: 'fin.taxCode.method.zeroRated' }
];

export interface TaxCodeRate {
  id?: string | null;
  fromDate: string;
  toDate?: string | null;
  rate: number;
}

export interface TaxCode {
  id: string;
  number: number;
  companyId: string;
  companyNameAr?: string | null;
  branchId?: string | null;
  branchNameAr?: string | null;
  code: string;
  nameAr: string;
  nameEn?: string | null;
  appliesTo: TaxAppliesTo | number;
  calculationMethod: TaxCalculationMethod | number;
  salesAccountId?: string | null;
  salesAccountNumber?: string | null;
  salesAccountNameAr?: string | null;
  purchaseAccountId?: string | null;
  purchaseAccountNumber?: string | null;
  purchaseAccountNameAr?: string | null;
  priceIncludesTax: boolean;
  isActive: boolean;
  hasBeenUsed: boolean;
  currentRate?: number | null;
  createdAt: string;
  createdBy?: string | null;
  updatedAt?: string | null;
  updatedBy?: string | null;
  rates: TaxCodeRate[];
}

export interface UpsertTaxCodePayload {
  companyId: string;
  branchId?: string | null;
  code: string;
  nameAr: string;
  nameEn?: string | null;
  appliesTo: number;
  calculationMethod: number;
  salesAccountId?: string | null;
  purchaseAccountId?: string | null;
  priceIncludesTax: boolean;
  isActive: boolean;
  rates: TaxCodeRate[];
}

export interface TaxCodeListFilter {
  search?: string;
  companyId?: string | null;
  branchId?: string | null;
  appliesTo?: number | null;
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
  isActive?: boolean;
}
