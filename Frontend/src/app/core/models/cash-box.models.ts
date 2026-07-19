export type CashBoxDeviceRole = 1 | 2 | 3 | 4 | 5;

export const CASH_BOX_DEVICE_ROLES: { value: CashBoxDeviceRole; labelKey: string }[] = [
  { value: 1, labelKey: 'fin.cash.device.pos' },
  { value: 2, labelKey: 'fin.cash.device.drawer' },
  { value: 3, labelKey: 'fin.cash.device.printer' },
  { value: 4, labelKey: 'fin.cash.device.display' },
  { value: 5, labelKey: 'fin.cash.device.barcode' }
];

export interface CashBoxUser {
  id?: string | null;
  userId: string;
  userName?: string | null;
  roleName?: string | null;
  isDefault: boolean;
  isManager: boolean;
  isCustodian: boolean;
}

export interface CashBoxDevice {
  id?: string | null;
  deviceId?: string | null;
  deviceName?: string | null;
  deviceRole: CashBoxDeviceRole | number;
  label?: string | null;
}

export interface CashBox {
  id: string;
  number: number;
  code: string;
  nameAr: string;
  nameEn?: string | null;
  companyId: string;
  companyNameAr?: string | null;
  branchId: string;
  branchNameAr?: string | null;
  locationName?: string | null;
  posDeviceId?: string | null;
  posDeviceName?: string | null;
  chartOfAccountId: string;
  accountNumber?: string | null;
  accountNameAr?: string | null;
  currencyId: string;
  currencyCode?: string | null;
  openingBalance: number;
  openingDate?: string | null;
  description?: string | null;
  isActive: boolean;
  allowReceive: boolean;
  allowPay: boolean;
  allowDeposit: boolean;
  allowWithdraw: boolean;
  allowTransfer: boolean;
  requireShiftBeforeUse: boolean;
  allowNegativeBalance: boolean;
  minBalance?: number | null;
  maxBalance?: number | null;
  currentBalance: number;
  currentUserId?: string | null;
  currentUserName?: string | null;
  lastOpenedAt?: string | null;
  lastClosedAt?: string | null;
  lastMovementAt?: string | null;
  lastCountAt?: string | null;
  isOpen: boolean;
  hasHadMovement: boolean;
  isSystem: boolean;
  sortOrder: number;
  createdAt: string;
  createdBy?: string | null;
  updatedAt?: string | null;
  updatedBy?: string | null;
  authorizedUsers: CashBoxUser[];
  devices: CashBoxDevice[];
}

export interface UpsertCashBoxPayload {
  nameAr: string;
  nameEn?: string | null;
  companyId: string;
  branchId: string;
  locationName?: string | null;
  posDeviceId?: string | null;
  chartOfAccountId: string;
  currencyId: string;
  openingBalance: number;
  openingDate?: string | null;
  description?: string | null;
  isActive: boolean;
  allowReceive: boolean;
  allowPay: boolean;
  allowDeposit: boolean;
  allowWithdraw: boolean;
  allowTransfer: boolean;
  requireShiftBeforeUse: boolean;
  allowNegativeBalance: boolean;
  minBalance?: number | null;
  maxBalance?: number | null;
  sortOrder: number;
  authorizedUsers: CashBoxUser[];
  devices: CashBoxDevice[];
}

export interface CashBoxListFilter {
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

export interface OrgDeviceLookup {
  id: string;
  nameAr: string;
  nameEn?: string | null;
  deviceType?: number;
  isActive?: boolean;
}

export interface UserLookup {
  id: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  userName?: string;
}
