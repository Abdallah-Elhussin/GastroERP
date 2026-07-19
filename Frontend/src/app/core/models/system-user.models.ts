export interface SystemUser {
  id: string;
  number: number;
  code?: string | null;
  userName: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  phoneNumber?: string | null;
  mobileNumber?: string | null;
  isActive: boolean;
  isPosUser: boolean;
  mustChangePassword: boolean;
  isLocked: boolean;
  lastLoginAt?: string | null;
  createdAt: string;
  branchId?: string | null;
  branchNameAr?: string | null;
  roleId?: string | null;
  roleName?: string | null;
  roleNameAr?: string | null;
}

export interface UpsertSystemUserPayload {
  userName: string;
  firstName: string;
  lastName?: string | null;
  email?: string | null;
  mobileNumber?: string | null;
  phoneNumber?: string | null;
  branchId: string;
  roleId: string;
  password?: string | null;
  isActive: boolean;
  isPosUser: boolean;
  mustChangePassword: boolean;
  isLocked: boolean;
  code?: string | null;
  preferredLanguage?: string;
}

export interface SystemUserListFilter {
  search?: string;
  branchId?: string | null;
  roleId?: string | null;
  isActive?: boolean | null;
  pageNumber?: number;
  pageSize?: number;
}

export interface UserLicenseStatus {
  currentUsers: number;
  maxUsers: number;
  isUnlimited: boolean;
  isTrial: boolean;
  label: string;
}

export interface RoleLookup {
  id: string;
  name: string;
  nameAr?: string | null;
  isActive?: boolean;
  isSystem?: boolean;
}

export interface BranchLookup {
  id: string;
  nameAr: string;
  nameEn?: string | null;
  companyId?: string;
  isActive?: boolean;
}
