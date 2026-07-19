export interface AccountMainClassification {
  id: string;
  code: string;
  nameAr: string;
  nameEn: string;
  accountType: number;
  sortOrder: number;
  isActive: boolean;
}

export interface AccountClassification {
  id: string;
  number: number;
  code: string;
  nameAr: string;
  nameEn: string;
  mainClassificationId: string;
  mainClassificationNameAr: string;
  mainClassificationCode: string;
  accountType: number;
  isDefault: boolean;
  isSystem: boolean;
  isActive: boolean;
  sortOrder: number;
  createdAt: string;
  updatedAt?: string | null;
  createdBy?: string | null;
}

export interface UpsertAccountClassificationPayload {
  nameAr: string;
  nameEn: string;
  mainClassificationId: string;
  code?: string | null;
}
