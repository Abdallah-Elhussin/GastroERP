export type CostCenterTypeCode = 1 | 2 | 3 | 4 | 5 | 6;

export const COST_CENTER_TYPES: { value: CostCenterTypeCode; labelKey: string; code: string }[] = [
  { value: 1, labelKey: 'fin.cc.type.operational', code: 'operational' },
  { value: 2, labelKey: 'fin.cc.type.administrative', code: 'administration' },
  { value: 3, labelKey: 'fin.cc.type.production', code: 'production' },
  { value: 4, labelKey: 'fin.cc.type.service', code: 'service' },
  { value: 5, labelKey: 'fin.cc.type.branch', code: 'branch' },
  { value: 6, labelKey: 'fin.cc.type.project', code: 'project' }
];

export interface CostCenter {
  id: string;
  number: number;
  branchId: string;
  departmentId?: string | null;
  parentCostCenterId?: string | null;
  code: string;
  nameAr: string;
  nameEn?: string | null;
  description?: string | null;
  costCenterType: CostCenterTypeCode | number;
  costCenterTypeCode: string;
  status: number;
  isActive: boolean;
  isSystem: boolean;
  sortOrder: number;
  linkedAccountsCount: number;
  useInPurchases: boolean;
  useInInventory: boolean;
  useInProduction: boolean;
  useInSales: boolean;
  useInPayroll: boolean;
  useInAssets: boolean;
  useInMaintenance: boolean;
  useInJournals: boolean;
  allowedAccountIds: string[];
}

export interface UpsertCostCenterPayload {
  nameAr: string;
  nameEn?: string | null;
  code?: string | null;
  branchId?: string | null;
  parentCostCenterId?: string | null;
  departmentId?: string | null;
  costCenterType: number;
  description?: string | null;
  sortOrder?: number;
  useInPurchases: boolean;
  useInInventory: boolean;
  useInProduction: boolean;
  useInSales: boolean;
  useInPayroll: boolean;
  useInAssets: boolean;
  useInMaintenance: boolean;
  useInJournals: boolean;
  allowedAccountIds: string[];
}
