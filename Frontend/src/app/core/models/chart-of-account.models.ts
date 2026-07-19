export type AccountTypeCode = 1 | 2 | 3 | 4 | 5;

export const ACCOUNT_TYPES: { value: AccountTypeCode; labelKey: string }[] = [
  { value: 1, labelKey: 'fin.coa.type.asset' },
  { value: 2, labelKey: 'fin.coa.type.liability' },
  { value: 3, labelKey: 'fin.coa.type.equity' },
  { value: 4, labelKey: 'fin.coa.type.revenue' },
  { value: 5, labelKey: 'fin.coa.type.expense' }
];

export const ACCOUNT_CATEGORIES: { value: number; labelKey: string }[] = [
  { value: 1, labelKey: 'fin.coa.cat.currentAsset' },
  { value: 2, labelKey: 'fin.coa.cat.fixedAsset' },
  { value: 3, labelKey: 'fin.coa.cat.currentLiability' },
  { value: 4, labelKey: 'fin.coa.cat.longTermLiability' },
  { value: 5, labelKey: 'fin.coa.cat.equity' },
  { value: 6, labelKey: 'fin.coa.cat.operatingRevenue' },
  { value: 7, labelKey: 'fin.coa.cat.otherRevenue' },
  { value: 8, labelKey: 'fin.coa.cat.cogs' },
  { value: 9, labelKey: 'fin.coa.cat.operatingExpense' },
  { value: 10, labelKey: 'fin.coa.cat.otherExpense' }
];

export interface ChartAccount {
  id: string;
  accountNumber: string;
  nameAr: string;
  nameEn?: string | null;
  parentAccountId?: string | null;
  accountType: AccountTypeCode | number;
  accountCategory: number;
  currency: string;
  isPostingAllowed: boolean;
  isSummaryAccount: boolean;
  isSystemAccount: boolean;
  isActive: boolean;
  sortOrder: number;
  notes?: string | null;
  accountClassificationId?: string | null;
}

export interface AccountTreeNode extends ChartAccount {
  children: AccountTreeNode[];
}

export interface UpsertAccountPayload {
  accountNumber?: string;
  nameAr: string;
  nameEn?: string | null;
  accountType?: number;
  accountCategory: number;
  parentAccountId?: string | null;
  currency: string;
  isSummaryAccount: boolean;
  sortOrder: number;
  notes?: string | null;
  accountClassificationId?: string | null;
}

export interface AccountingSettings {
  id: string;
  tenantId: string;
  companyId?: string | null;
  accountNumberMaxLength: number;
  maxTreeLevels: number;
  levelLengthsCsv: string;
  levelSeparator: string;
  cashAccountId?: string | null;
  bankAccountId?: string | null;
  inventoryAccountId?: string | null;
  /** Goods received not invoiced (GRNI) clearing account. */
  grniAccountId?: string | null;
  cogsAccountId?: string | null;
  salesRevenueAccountId?: string | null;
  purchaseAccountId?: string | null;
  accountsReceivableAccountId?: string | null;
  accountsPayableAccountId?: string | null;
  vatInputAccountId?: string | null;
  vatOutputAccountId?: string | null;
  discountAccountId?: string | null;
  roundOffAccountId?: string | null;
  openingBalanceAccountId?: string | null;
  retainedEarningsAccountId?: string | null;
  payrollExpenseAccountId?: string | null;
  payrollLiabilityAccountId?: string | null;
  productionVarianceAccountId?: string | null;
  inventoryAdjustmentAccountId?: string | null;
  wasteAccountId?: string | null;
  deliveryRevenueAccountId?: string | null;
  deliveryExpenseAccountId?: string | null;
  kitchenConsumptionAccountId?: string | null;
  customerAdvancesAccountId?: string | null;
  supplierAdvancesAccountId?: string | null;
  exchangeDifferenceAccountId?: string | null;
  autoPostSales: boolean;
  autoPostPurchases: boolean;
  autoPostGoodsReceipt: boolean;
  autoPostGoodsIssue: boolean;
  autoPostStockTransfer: boolean;
  autoPostWaste: boolean;
  autoPostProduction: boolean;
  autoPostPayroll: boolean;
}

export interface AccountImportRow {
  accountNumber: string;
  nameAr: string;
  nameEn?: string | null;
  parentAccountNumber?: string | null;
  accountType: number;
  accountCategory: number;
  currency: string;
  isSummaryAccount: boolean;
  sortOrder: number;
  notes?: string | null;
}

export interface AccountImportPreview {
  totalRows: number;
  validRows: number;
  invalidRows: number;
  errors: string[];
  rows: AccountImportRow[];
}
