export type DashboardPeriod = 0 | 1 | 2 | 3 | 4;

export interface EnterpriseDashboardFilter {
  period?: DashboardPeriod;
  fromDate?: string | null;
  toDate?: string | null;
  branchId?: string | null;
  warehouseId?: string | null;
  cashierId?: string | null;
  companyId?: string | null;
  currency?: string | null;
}

export interface DashboardKpi {
  key: string;
  label: string;
  value: number;
  unit?: string | null;
  changePercent?: number | null;
  isHigherBetter: boolean;
  sparkline: number[];
  updatedAt: string;
}

export interface DashboardNamedValue {
  name: string;
  value: number;
  percent?: number | null;
}

export interface DashboardSeriesPoint {
  label: string;
  sales: number;
  profit: number;
  discounts: number;
  tax: number;
}

export interface DashboardTableRow {
  id: string;
  name: string;
  quantity: number;
  revenue: number;
  profit?: number | null;
  percent?: number | null;
  lastActivity?: string | null;
  currentQty?: number | null;
  minQty?: number | null;
  suggestedQty?: number | null;
}

export interface DashboardHeader {
  companyName: string;
  branchName?: string | null;
  userName: string;
  serverTime: string;
  lastSyncedAt: string;
  currency: string;
}

export interface DashboardNotification {
  severity: string;
  code: string;
  message: string;
  at: string;
  link?: string | null;
}

export interface DashboardInsight {
  category: string;
  title: string;
  detail: string;
  actionHint?: string | null;
}

export interface DashboardQuickAction {
  key: string;
  label: string;
  route: string;
  icon: string;
}

export interface EnterpriseDashboardOverview {
  header: DashboardHeader;
  kpis: DashboardKpi[];
  notifications: DashboardNotification[];
  insights: DashboardInsight[];
  quickActions: DashboardQuickAction[];
}

export interface EnterpriseDashboardSales {
  trend: DashboardSeriesPoint[];
  revenueSources: DashboardNamedValue[];
  paymentMethods: DashboardNamedValue[];
}

export interface EnterpriseDashboardProducts {
  topSelling: DashboardTableRow[];
  worstSelling: DashboardTableRow[];
}

export interface EnterpriseDashboardCustomers {
  topCustomers: DashboardTableRow[];
}

export interface EnterpriseDashboardInventory {
  inventoryValue: number;
  lowStockCount: number;
  lowStockItems: DashboardTableRow[];
}

export interface EnterpriseDashboardFinance {
  snapshot: {
    bankBalance: number;
    cashBalance: number;
    receivables: number;
    payables: number;
    profit: number;
  };
}

export interface EnterpriseDashboardKitchen {
  status: {
    pending: number;
    preparing: number;
    ready: number;
    served: number;
    delayed: number;
    avgPrepMinutes: number;
  };
}

export interface EnterpriseDashboardDelivery {
  status: {
    inProgress: number;
    delivered: number;
    delayed: number;
    avgDeliveryMinutes: number;
  };
}

export interface EnterpriseDashboardHr {
  snapshot: {
    present: number;
    absent: number;
    late: number;
    workedHours: number;
  };
}

export interface DashboardActivity {
  type: string;
  title: string;
  reference?: string | null;
  at: string;
  userName?: string | null;
}

export interface EnterpriseDashboardActivities {
  items: DashboardActivity[];
}

export type DashboardWidgetId =
  | 'kpis'
  | 'sales'
  | 'revenueSources'
  | 'payments'
  | 'topItems'
  | 'worstItems'
  | 'topCustomers'
  | 'lowInventory'
  | 'kitchen'
  | 'delivery'
  | 'hr'
  | 'finance'
  | 'activities'
  | 'notifications'
  | 'quickActions'
  | 'insights';

export interface DashboardWidgetLayout {
  id: DashboardWidgetId;
  titleKey: string;
  visible: boolean;
  size: 'full' | 'half' | 'third';
}
