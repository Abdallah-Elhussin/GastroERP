export interface SalesDashboardNamedValue {
  label: string;
  value: number;
  count?: number;
}

export interface SalesDashboardHourlyCell {
  dayOfWeek: number;
  hour: number;
  value: number;
  count: number;
}

export interface SalesDashboardKpis {
  salesToday: number;
  salesWeek: number;
  salesMonth: number;
  salesYear: number;
  salesTodayChangePercent: number;
  salesWeekChangePercent: number;
  salesMonthChangePercent: number;
  invoiceCount: number;
  averageInvoiceValue: number;
  highestInvoice: number;
  lowestInvoice: number;
  newCustomers: number;
  activeCustomers: number;
  inactiveCustomers: number;
  firstTimeBuyers: number;
  returnsTotal: number;
  returnsRatioPercent: number;
  discountsTotal: number;
  cancellationsTotal: number;
  posInvoiceCount: number;
  averageInvoiceMinutes: number;
  averageItemsPerInvoice: number;
  mostActivePosDevice?: string | null;
  grossProfit: number;
  profitMarginPercent: number;
  cogs: number;
  netProfit: number;
  salesGrowthPercent: number;
  newCustomerRatioPercent: number;
  returningCustomerRatioPercent: number;
  discountRatioPercent: number;
  bestBranchName?: string | null;
  worstBranchName?: string | null;
  bestCashierName?: string | null;
  topSellingItemName?: string | null;
  topSellingCategoryName?: string | null;
  topPaymentMethodName?: string | null;
}

export interface SalesDashboardCharts {
  salesByDay: SalesDashboardNamedValue[];
  salesByBranch: SalesDashboardNamedValue[];
  salesByPosDevice: SalesDashboardNamedValue[];
  salesByCashier: SalesDashboardNamedValue[];
  topCustomers: SalesDashboardNamedValue[];
  topItems: SalesDashboardNamedValue[];
  salesByCategory: SalesDashboardNamedValue[];
  paymentMethods: SalesDashboardNamedValue[];
  salesByHour: SalesDashboardHourlyCell[];
}

export interface SalesDashboardRecentOrder {
  id: string;
  orderNumber: string;
  customerName?: string | null;
  grandTotal: number;
  paymentMethod?: string | null;
  branchName?: string | null;
  cashierName?: string | null;
  status: string;
  occurredAt: string;
}

export interface SalesDashboardRecentReturn {
  id: string;
  creditNoteNumber: string;
  originalInvoiceNumber?: string | null;
  customerName?: string | null;
  totalAmount: number;
  reason: string;
  issuedAt?: string | null;
}

export interface SalesDashboardTopCustomer {
  customerId?: string | null;
  customerName: string;
  totalSales: number;
  invoiceCount: number;
  lastPurchaseAt?: string | null;
}

export interface SalesDashboardTopItem {
  productId: string;
  productName: string;
  quantity: number;
  revenue: number;
  profit: number;
}

export interface SalesDashboardAlert {
  code: string;
  severity: string;
  messageEn: string;
  messageAr: string;
  path?: string | null;
}

export interface SalesDashboardSummary {
  generatedAtUtc: string;
  kpis: SalesDashboardKpis;
  charts: SalesDashboardCharts;
  recentOrders: SalesDashboardRecentOrder[];
  recentReturns: SalesDashboardRecentReturn[];
  topCustomers: SalesDashboardTopCustomer[];
  topItems: SalesDashboardTopItem[];
  alerts: SalesDashboardAlert[];
}

export interface SalesDashboardFilters {
  fromDate?: string | null;
  toDate?: string | null;
  companyId?: string | null;
  branchId?: string | null;
  cashierId?: string | null;
  deviceId?: string | null;
  customerId?: string | null;
  paymentMethod?: number | null;
  orderStatus?: number | null;
}
