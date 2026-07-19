export type TaxpayerType = 1 | 2 | 3 | 4 | 5 | 6;
export type TaxRegistrationStatus = 1 | 2 | 3 | 4;

export const TAXPAYER_TYPES: { value: TaxpayerType; labelKey: string }[] = [
  { value: 1, labelKey: 'fin.tax.type.company' },
  { value: 2, labelKey: 'fin.tax.type.establishment' },
  { value: 3, labelKey: 'fin.tax.type.individual' },
  { value: 4, labelKey: 'fin.tax.type.government' },
  { value: 5, labelKey: 'fin.tax.type.association' },
  { value: 6, labelKey: 'fin.tax.type.organization' }
];

export const TAX_REG_STATUSES: { value: TaxRegistrationStatus; labelKey: string }[] = [
  { value: 1, labelKey: 'fin.tax.status.active' },
  { value: 2, labelKey: 'fin.tax.status.suspended' },
  { value: 3, labelKey: 'fin.tax.status.expired' },
  { value: 4, labelKey: 'fin.tax.status.cancelled' }
];

/** Common restaurant activity codes (ISIC-like). */
export const TAX_ACTIVITY_CODES: { code: string; nameAr: string; nameEn: string }[] = [
  { code: '561001', nameAr: 'أنشطة المطاعم', nameEn: 'Restaurants activities' },
  { code: '561002', nameAr: 'خدمات تقديم الوجبات المتنقلة', nameEn: 'Mobile food service' },
  { code: '563001', nameAr: 'أنشطة تقديم المشروبات', nameEn: 'Beverage serving activities' },
  { code: '562100', nameAr: 'أنشطة تقديم الطعام في المناسبات', nameEn: 'Event catering' }
];

export interface TaxRegistrationCertificate {
  id: string;
  version: number;
  fileName: string;
  storagePath: string;
  contentType?: string | null;
  documentNumber?: string | null;
  issueDate?: string | null;
  expiryDate?: string | null;
  notes?: string | null;
  isCurrent: boolean;
  uploadedAt: string;
}

export interface TaxRegistrationProfile {
  id: string;
  number: number;
  companyId: string;
  companyNameAr?: string | null;
  branchId?: string | null;
  branchNameAr?: string | null;
  vatNumber: string;
  branchVatNumber?: string | null;
  taxOffice?: string | null;
  taxpayerType: TaxpayerType | number;
  activityCode?: string | null;
  activityNameAr?: string | null;
  activityNameEn?: string | null;
  defaultTaxRate: number;
  registrationDate?: string | null;
  expiryDate?: string | null;
  status: TaxRegistrationStatus | number;
  notes?: string | null;
  isSystem: boolean;
  sortOrder: number;
  hasBeenUsed: boolean;
  createdAt: string;
  createdBy?: string | null;
  updatedAt?: string | null;
  updatedBy?: string | null;
  currentCertificate?: TaxRegistrationCertificate | null;
  certificates: TaxRegistrationCertificate[];
}

export interface UpsertTaxRegistrationPayload {
  companyId: string;
  branchId?: string | null;
  vatNumber: string;
  branchVatNumber?: string | null;
  taxOffice?: string | null;
  taxpayerType: number;
  activityCode?: string | null;
  activityNameAr?: string | null;
  activityNameEn?: string | null;
  defaultTaxRate: number;
  registrationDate?: string | null;
  expiryDate?: string | null;
  status: number;
  notes?: string | null;
  sortOrder: number;
  certificateDocumentNumber?: string | null;
  certificateIssueDate?: string | null;
  certificateExpiryDate?: string | null;
  certificateNotes?: string | null;
}

export interface TaxRegistrationListFilter {
  search?: string;
  companyId?: string | null;
  branchId?: string | null;
  status?: number | null;
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
