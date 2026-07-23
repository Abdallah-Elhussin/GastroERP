/** Supplier payment methods for UI: Cash=1, Credit=2 only. */
export type SupplierPaymentMethodKind = 1 | 2 | 3 | 4 | 5;

export const SUPPLIER_PAYMENT_CASH: SupplierPaymentMethodKind = 1;
export const SUPPLIER_PAYMENT_CREDIT: SupplierPaymentMethodKind = 2;

/** Only نقدي / آجل — other enum values kept for legacy rows. */
export const PAYMENT_METHOD_KINDS: Array<{ value: SupplierPaymentMethodKind; labelKey: string }> = [
  { value: SUPPLIER_PAYMENT_CASH, labelKey: 'pur.sup.pm.cash' },
  { value: SUPPLIER_PAYMENT_CREDIT, labelKey: 'pur.sup.pm.credit' }
];

export interface SupplierListItem {
  id: string;
  code: string;
  nameAr: string;
  nameEn?: string | null;
  accountNumber?: string | null;
  apAccountId?: string | null;
  category: number;
  city?: string | null;
  country?: string | null;
  taxNumber?: string | null;
  creditLimit: number;
  currentBalance: number;
  lastPurchaseDate?: string | null;
  lastPaymentDate?: string | null;
  isActive: boolean;
  isBlacklisted: boolean;
  isOverCreditLimit: boolean;
}

export interface SupplierPaymentMethod {
  id?: string;
  kind: SupplierPaymentMethodKind;
  bankName?: string | null;
  iban?: string | null;
  swift?: string | null;
  accountNumber?: string | null;
  beneficiaryName?: string | null;
  currency: string;
  isDefault: boolean;
  notes?: string | null;
}

export interface SupplierDetail {
  id: string;
  tenantId: string;
  companyId?: string | null;
  branchId?: string | null;
  code: string;
  nameAr: string;
  nameEn?: string | null;
  supplierType: number;
  category: number;
  taxNumber?: string | null;
  contactPerson?: string | null;
  contactJobTitle?: string | null;
  phone?: string | null;
  mobile?: string | null;
  email?: string | null;
  website?: string | null;
  city?: string | null;
  region?: string | null;
  country?: string | null;
  postalCode?: string | null;
  address?: string | null;
  apAccountId?: string | null;
  accountNumber?: string | null;
  currency: string;
  defaultPaymentMethod: SupplierPaymentMethodKind;
  paymentDueDays: number;
  paymentTerms?: string | null;
  creditLimit: number;
  openingBalance: number;
  leadTimeDays: number;
  isPreferred: boolean;
  rating: number;
  isActive: boolean;
  isBlacklisted: boolean;
  blacklistReason?: string | null;
  notes?: string | null;
  currentBalance: number;
  paymentMethods: SupplierPaymentMethod[];
  createdAt?: string;
}

export interface CreateSupplierPayload {
  nameAr: string;
  apAccountId: string;
  nameEn?: string | null;
  code?: string | null;
  currency?: string;
  taxNumber?: string | null;
  phone?: string | null;
  email?: string | null;
  city?: string | null;
  country?: string | null;
  address?: string | null;
  notes?: string | null;
  paymentDueDays?: number;
  creditLimit?: number;
  defaultPaymentMethod?: SupplierPaymentMethodKind;
}

export interface UpsertSupplierMasterPayload {
  nameAr: string;
  nameEn?: string | null;
  supplierType: number;
  category: number;
  companyId?: string | null;
  branchId?: string | null;
  taxNumber?: string | null;
  commercialRegister?: string | null;
  establishmentNumber?: string | null;
  taxRegistrationCountry?: string | null;
  taxType?: string | null;
  defaultTaxPercent: number;
  taxCertificateExpiry?: string | null;
  commercialRegisterExpiry?: string | null;
  contactPerson?: string | null;
  contactJobTitle?: string | null;
  phone?: string | null;
  mobile?: string | null;
  email?: string | null;
  website?: string | null;
  city?: string | null;
  region?: string | null;
  country?: string | null;
  postalCode?: string | null;
  address?: string | null;
  apAccountId: string;
  discountAccountId?: string | null;
  purchaseReturnAccountId?: string | null;
  exchangeDifferenceAccountId?: string | null;
  currency: string;
  defaultPaymentMethod: SupplierPaymentMethodKind;
  paymentDueDays: number;
  paymentTerms?: string | null;
  creditLimit: number;
  openingBalance: number;
  openingBalanceDate?: string | null;
  vatEvaluation: number;
  leadTimeDays: number;
  isPreferred: boolean;
  rating: number;
  notes?: string | null;
  paymentMethods?: Array<{
    kind: SupplierPaymentMethodKind;
    bankName?: string | null;
    iban?: string | null;
    swift?: string | null;
    accountNumber?: string | null;
    beneficiaryName?: string | null;
    currency?: string;
    isDefault?: boolean;
    notes?: string | null;
  }>;
}

export const PAYMENT_TERM_PRESETS = [
  { value: 'Cash', labelKey: 'pur.sup.term.cash', dueDays: 0 },
  { value: '15 Days', labelKey: 'pur.sup.term.15', dueDays: 15 },
  { value: '30 Days', labelKey: 'pur.sup.term.30', dueDays: 30 },
  { value: '60 Days', labelKey: 'pur.sup.term.60', dueDays: 60 }
] as const;