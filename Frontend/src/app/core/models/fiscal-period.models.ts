export type FiscalPeriodStatus = 1 | 2 | 3;

export interface FiscalPeriodDetail {
  id: string;
  periodNumber: number;
  nameAr: string;
  nameEn: string;
  startDate: string;
  endDate: string;
  status: FiscalPeriodStatus | number;
}

export interface FiscalPeriod {
  id: string;
  fiscalYear: number;
  startMonth: number;
  name: string;
  startDate: string;
  endDate: string;
  notes?: string | null;
  periodPolicy: number;
  periodPolicyCode: string;
  status: FiscalPeriodStatus | number;
  statusCode: string;
  details: FiscalPeriodDetail[];
  createdAt: string;
  updatedAt?: string | null;
}

export interface UpsertFiscalPeriodPayload {
  fiscalYear?: number;
  startMonth: number;
  notes?: string | null;
  periodPolicy?: number;
  generateDetails?: boolean;
  details?: Array<{
    id?: string | null;
    periodNumber: number;
    nameAr: string;
    nameEn: string;
    startDate: string;
    endDate: string;
    status: number;
  }>;
}

export const FISCAL_STATUS = {
  Open: 1,
  Closed: 2,
  Locked: 3
} as const;
