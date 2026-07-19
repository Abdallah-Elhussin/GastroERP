/** Shared finance voucher detail line (سندات قبض/صرف وغيرها). */
export interface FinanceVoucherLineFormValue {
  id?: string | null;
  chartOfAccountId: string;
  costCenterId: string;
  analyticalAccountId: string;
  currency: string;
  exchangeRate: number;
  amount: number;
  description: string;
}

export interface FinanceVoucherAccountOption {
  id: string;
  accountNumber: string;
  nameAr: string;
  nameEn?: string | null;
}

export interface FinanceVoucherCostCenterOption {
  id: string;
  code?: string | null;
  nameAr: string;
}

export interface FinanceVoucherCurrencyOption {
  code: string;
  nameAr: string;
  currentExchangeRate: number;
  isCompanyCurrency: boolean;
  decimalPlaces?: number;
}

export interface FinanceVoucherDetailsTotals {
  lineCount: number;
  totalAmount: number;
  totalLocal: number;
  totalForeign: number;
  hasForeign: boolean;
}
