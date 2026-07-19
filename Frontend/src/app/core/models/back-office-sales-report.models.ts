export interface BackOfficeSalesReportNamedValue {
  label: string;
  value: number;
  count: number;
}

export interface BackOfficeSalesReportSummary {
  grossSales: number;
  taxAmount: number;
  totalReturns: number;
  totalDebitNotes: number;
  netSales: number;
  postedInvoiceCount: number;
  postedReturnCount: number;
  postedDebitNoteCount: number;
}

export interface BackOfficeSalesReportDocumentCounts {
  invoices: Record<string, number>;
  returns: Record<string, number>;
  debitNotes: Record<string, number>;
  orders: Record<string, number>;
  quotations: Record<string, number>;
  deliveryNotes: Record<string, number>;
}

export interface BackOfficeSalesReport {
  generatedAtUtc: string;
  from: string;
  to: string;
  summary: BackOfficeSalesReportSummary;
  salesByCustomer: BackOfficeSalesReportNamedValue[];
  salesByItem: BackOfficeSalesReportNamedValue[];
  salesByDay: BackOfficeSalesReportNamedValue[];
  documentCounts: BackOfficeSalesReportDocumentCounts;
}

export interface BackOfficeSalesReportParams {
  from?: string | null;
  to?: string | null;
  customerId?: string | null;
  branchId?: string | null;
  topCustomers?: number;
  topItems?: number;
}
