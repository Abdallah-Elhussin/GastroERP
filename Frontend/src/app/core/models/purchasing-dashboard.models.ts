export interface PurchasingDashboardSummary {
  openPurchaseOrders: number;
  latePurchaseOrders: number;
  totalPurchaseOrders: number;
  uninvoicedReceipts: number;
  pendingReceipts: number;
  totalReceipts: number;
  unpaidInvoices: number;
  draftInvoices: number;
  totalInvoices: number;
  totalSuppliers: number;
  activeSuppliers: number;
  overCreditSuppliers: number;
  alerts: PurchasingDashboardAlert[];
  recentActivities: PurchasingDashboardActivity[];
}

export interface PurchasingDashboardAlert {
  code: string;
  severity: string;
  messageEn: string;
  messageAr: string;
  path?: string | null;
}

export interface PurchasingDashboardActivity {
  id: string;
  kind: string;
  reference: string;
  occurredAt: string;
  notes?: string | null;
  path?: string | null;
}
