export type ReceiptMethod =
  | 1 // Cash
  | 2 // BankTransfer
  | 3 // Cheque
  | 4 // CreditCard
  | 5 // DebitCard
  | 6 // Wallet
  | 7; // Other

export type ReceiptPartyType = 1 | 2 | 3; // Customer | General | Supplier

export type ReceiptVoucherStatus =
  | 1 // Draft
  | 2 // Submitted
  | 3 // Approved
  | 4 // Posted
  | 5 // Reversed
  | 6; // Cancelled

export interface ReceiptVoucherLine {
  id?: string | null;
  chartOfAccountId: string;
  accountNumber?: string | null;
  accountNameAr?: string | null;
  costCenterId?: string | null;
  costCenterNameAr?: string | null;
  analyticalAccountId?: string | null;
  analyticalAccountNameAr?: string | null;
  currency: string;
  exchangeRate: number;
  amount: number;
  amountInBase: number;
  description?: string | null;
}

export interface ReceiptVoucher {
  id: string;
  number: number;
  documentNumber: string;
  companyId: string;
  companyNameAr?: string | null;
  branchId: string;
  branchNameAr?: string | null;
  voucherDate: string;
  fiscalPeriodId: string;
  fiscalYear?: number | null;
  receiptMethod: ReceiptMethod;
  cashBoxId?: string | null;
  cashBoxNameAr?: string | null;
  bankId?: string | null;
  bankNameAr?: string | null;
  partyType: ReceiptPartyType;
  partyId?: string | null;
  partyName?: string | null;
  currency: string;
  exchangeRate: number;
  costCenterId?: string | null;
  costCenterNameAr?: string | null;
  reference?: string | null;
  chequeNumber?: string | null;
  chequeDate?: string | null;
  description?: string | null;
  notes?: string | null;
  status: ReceiptVoucherStatus;
  journalEntryId?: string | null;
  journalEntryNumber?: string | null;
  linesCount: number;
  totalAmount: number;
  totalAmountInBase: number;
  createdAt: string;
  postedAt?: string | null;
  approvedAt?: string | null;
  lines: ReceiptVoucherLine[];
}

export interface UpsertReceiptVoucherPayload {
  companyId: string;
  branchId: string;
  voucherDate: string;
  fiscalPeriodId: string;
  receiptMethod: ReceiptMethod;
  partyType: ReceiptPartyType;
  cashBoxId?: string | null;
  bankId?: string | null;
  partyId?: string | null;
  partyName?: string | null;
  currency: string;
  exchangeRate: number;
  costCenterId?: string | null;
  reference?: string | null;
  description?: string | null;
  notes?: string | null;
  chequeNumber?: string | null;
  chequeDate?: string | null;
  lines: {
    chartOfAccountId: string;
    costCenterId?: string | null;
    analyticalAccountId?: string | null;
    currency: string;
    exchangeRate: number;
    amount: number;
    description?: string | null;
  }[];
}

export interface ReceiptVoucherListFilter {
  search?: string;
  companyId?: string | null;
  branchId?: string | null;
  fiscalPeriodId?: string | null;
  status?: number | null;
  receiptMethod?: number | null;
  cashBoxId?: string | null;
  bankId?: string | null;
  currency?: string | null;
  fromDate?: string | null;
  toDate?: string | null;
  pageSize?: number;
}

export const RECEIPT_METHODS: { value: ReceiptMethod; labelKey: string }[] = [
  { value: 1, labelKey: 'fin.ops.rv.method.cash' },
  { value: 2, labelKey: 'fin.ops.rv.method.bankTransfer' },
  { value: 3, labelKey: 'fin.ops.rv.method.cheque' },
  { value: 4, labelKey: 'fin.ops.rv.method.creditCard' },
  { value: 5, labelKey: 'fin.ops.rv.method.debitCard' },
  { value: 6, labelKey: 'fin.ops.rv.method.wallet' },
  { value: 7, labelKey: 'fin.ops.rv.method.other' }
];

export const RECEIPT_STATUSES: { value: ReceiptVoucherStatus; labelKey: string }[] = [
  { value: 1, labelKey: 'fin.ops.rv.status.draft' },
  { value: 2, labelKey: 'fin.ops.rv.status.submitted' },
  { value: 3, labelKey: 'fin.ops.rv.status.approved' },
  { value: 4, labelKey: 'fin.ops.rv.status.posted' },
  { value: 5, labelKey: 'fin.ops.rv.status.reversed' },
  { value: 6, labelKey: 'fin.ops.rv.status.cancelled' }
];

export const RECEIPT_PARTY_TYPES: { value: ReceiptPartyType; labelKey: string }[] = [
  { value: 1, labelKey: 'fin.ops.rv.party.customer' },
  { value: 2, labelKey: 'fin.ops.rv.party.general' },
  { value: 3, labelKey: 'fin.ops.rv.party.supplier' }
];
