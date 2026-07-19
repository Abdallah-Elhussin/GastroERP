import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  SalesDashboardAlert,
  SalesDashboardFilters,
  SalesDashboardSummary
} from '../models/sales-dashboard.models';

interface BoNamed {
  label: string;
  value: number;
  count?: number;
}

interface BoDashboardResponse {
  generatedAtUtc: string;
  kpis: {
    salesToday: number;
    salesWeek: number;
    salesMonth: number;
    salesPeriod: number;
    salesTodayChangePercent: number;
    invoiceCount: number;
    draftCount: number;
    approvedCount: number;
    postedCount: number;
    averageInvoiceValue: number;
    creditOutstanding: number;
    cashSalesPeriod: number;
    creditSalesPeriod: number;
    activeCustomers: number;
  };
  salesByDay: BoNamed[];
  salesByCustomer: BoNamed[];
  salesByNature: BoNamed[];
  salesByPaymentMode: BoNamed[];
  recentInvoices: Array<{
    id: string;
    invoiceNumber: string;
    customerName?: string | null;
    totalAmount: number;
    status: number | string;
    paymentMode: number | string;
    invoiceDate: string;
    postedAt?: string | null;
  }>;
  alerts: SalesDashboardAlert[];
}

@Injectable({ providedIn: 'root' })
export class SalesDashboardRepository {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/back-office-sales/dashboard`;

  getDashboard(filters: SalesDashboardFilters = {}): Observable<SalesDashboardSummary> {
    let params = new HttpParams();
    if (filters.fromDate) params = params.set('fromDate', filters.fromDate);
    if (filters.toDate) params = params.set('toDate', filters.toDate);
    if (filters.branchId) params = params.set('branchId', filters.branchId);
    if (filters.customerId) params = params.set('customerId', filters.customerId);
    return this.http.get<BoDashboardResponse>(this.base, { params }).pipe(map(r => this.toSummary(r)));
  }

  private toSummary(r: BoDashboardResponse): SalesDashboardSummary {
    const k = r.kpis;
    return {
      generatedAtUtc: r.generatedAtUtc,
      kpis: {
        salesToday: k.salesToday,
        salesWeek: k.salesWeek,
        salesMonth: k.salesMonth,
        salesYear: k.salesPeriod,
        salesTodayChangePercent: k.salesTodayChangePercent,
        salesWeekChangePercent: 0,
        salesMonthChangePercent: 0,
        invoiceCount: k.invoiceCount,
        averageInvoiceValue: k.averageInvoiceValue,
        highestInvoice: 0,
        lowestInvoice: 0,
        newCustomers: 0,
        activeCustomers: k.activeCustomers,
        inactiveCustomers: 0,
        firstTimeBuyers: 0,
        returnsTotal: 0,
        returnsRatioPercent: 0,
        discountsTotal: 0,
        cancellationsTotal: 0,
        posInvoiceCount: k.postedCount,
        averageInvoiceMinutes: 0,
        averageItemsPerInvoice: 0,
        mostActivePosDevice: null,
        grossProfit: k.cashSalesPeriod,
        profitMarginPercent: 0,
        cogs: k.creditOutstanding,
        netProfit: k.creditSalesPeriod,
        salesGrowthPercent: k.salesTodayChangePercent,
        newCustomerRatioPercent: 0,
        returningCustomerRatioPercent: 0,
        discountRatioPercent: 0,
        bestBranchName: null,
        worstBranchName: null,
        bestCashierName: null,
        topSellingItemName: null,
        topSellingCategoryName: null,
        topPaymentMethodName: null
      },
      charts: {
        salesByDay: r.salesByDay ?? [],
        salesByBranch: [],
        salesByPosDevice: r.salesByNature ?? [],
        salesByCashier: [],
        topCustomers: r.salesByCustomer ?? [],
        topItems: [],
        salesByCategory: [],
        paymentMethods: r.salesByPaymentMode ?? [],
        salesByHour: []
      },
      recentOrders: (r.recentInvoices ?? []).map(i => ({
        id: i.id,
        orderNumber: i.invoiceNumber,
        customerName: i.customerName,
        grandTotal: i.totalAmount,
        paymentMethod: String(i.paymentMode),
        branchName: null,
        cashierName: null,
        status: String(i.status),
        occurredAt: i.postedAt || i.invoiceDate
      })),
      recentReturns: [],
      topCustomers: (r.salesByCustomer ?? []).map(c => ({
        customerId: null,
        customerName: c.label,
        totalSales: c.value,
        invoiceCount: c.count ?? 0,
        lastPurchaseAt: null
      })),
      topItems: [],
      alerts: r.alerts ?? []
    };
  }
}
