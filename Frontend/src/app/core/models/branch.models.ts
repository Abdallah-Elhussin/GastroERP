export interface Branch {
  id: string;
  tenantId: string;
  companyId: string;
  nameAr: string;
  nameEn?: string | null;
  code?: string | null;
  branchType: number;
  status: number;
  phoneNumber?: string | null;
  email?: string | null;
  addressStreetAr?: string | null;
  addressStreetEn?: string | null;
  cityAr?: string | null;
  cityEn?: string | null;
  allowNegativeStock: boolean;
  allowOfflineSales: boolean;
  createdAt: string;
  updatedAt?: string | null;
  companyNameAr?: string | null;
  isActive: boolean;
}

export interface UpsertBranchPayload {
  companyId: string;
  nameAr: string;
  nameEn?: string | null;
  code?: string | null;
  location?: string | null;
  isActive: boolean;
}

export interface BranchListFilter {
  search?: string;
  companyId?: string | null;
  isActive?: boolean | null;
  pageSize?: number;
}

export interface OrgCompanyLookup {
  id: string;
  nameAr: string;
  nameEn?: string | null;
  isActive?: boolean;
}
