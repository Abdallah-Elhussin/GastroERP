export interface Currency {
  id: string;
  number: number;
  code: string;
  nameAr: string;
  nameEn: string;
  symbol?: string | null;
  decimalPlaces: number;
  subUnitNameAr?: string | null;
  subUnitNameEn?: string | null;
  currentExchangeRate: number;
  isCompanyCurrency: boolean;
  isForeignCurrency: boolean;
  status: number;
  isActive: boolean;
  isSystem: boolean;
  sortOrder: number;
  lastExchangeRateAt?: string | null;
  lastExchangeRateBy?: string | null;
}

export interface UpsertCurrencyPayload {
  code?: string;
  nameAr: string;
  nameEn: string;
  symbol?: string | null;
  decimalPlaces: number;
  subUnitNameAr?: string | null;
  subUnitNameEn?: string | null;
  currentExchangeRate?: number;
  isCompanyCurrency?: boolean;
  isActive: boolean;
  sortOrder: number;
}

export interface CurrencyExchangeRate {
  id: string;
  number: number;
  currencyId: string;
  currencyCode: string;
  currencyNameAr: string;
  rate: number;
  startDate: string;
  endDate?: string | null;
  isActive: boolean;
  isOpen: boolean;
  changeReason?: string | null;
  createdBy?: string | null;
  createdAt: string;
  updatedBy?: string | null;
  updatedAt?: string | null;
}

export interface UpsertExchangeRatePayload {
  currencyId?: string;
  rate: number;
  startDate: string;
  endDate?: string | null;
  isActive: boolean;
  changeReason?: string | null;
  autoClosePreviousOpen?: boolean;
}

export const CURRENCY_DECIMAL_OPTIONS = [0, 2, 3, 4] as const;
