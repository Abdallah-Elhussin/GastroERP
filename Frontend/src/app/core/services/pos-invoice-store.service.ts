import { Injectable, signal, computed } from '@angular/core';
import { CartItem } from '../models/erp.models';

export type PosOrderType = 'dineIn' | 'takeaway' | 'delivery';
export type DeliveryApp = 'hungerstation' | 'jahez' | 'talabat' | 'careem' | 'toYou';
export type DiscountType = 'percent' | 'fixed';
export type PaymentMethod = 'cash' | 'card' | 'mixed';

export interface PaymentBreakdown {
  method: PaymentMethod;
  cash: number;
  card: number;
  total: number;
}

export interface PosInvoiceLine {
  name: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
  customizations: string[];
  notes?: string;
}

export interface PosInvoiceCustomer {
  name: string;
  phone: string;
  taxNumber: string;
  email: string;
  address: string;
}

export interface PosInvoiceRecord {
  invoiceNo: string;
  orderId: string;
  date: string;
  branch: string;
  orderType: PosOrderType;
  tableLabel: string;
  lines: PosInvoiceLine[];
  subtotal: number;
  discount: number;
  tax: number;
  total: number;
  payment: PaymentBreakdown;
  sentToKitchen: boolean;
  posted: boolean;
  customer: PosInvoiceCustomer;
  isSimplified: boolean;
}

export interface PosSuspendedOrder {
  id: string;
  label: string;
  suspendedAt: string;
  cartItems: CartItem[];
  orderType: PosOrderType;
  tableNumber: string;
  guestCount: number;
  deliveryApp: DeliveryApp;
  deliveryOrderRef: string;
  discountType: DiscountType;
  discountValue: number;
  orderSentToKitchen: boolean;
  orderId: string;
  itemCount: number;
  total: number;
  customer?: PosInvoiceCustomer;
}

const SUSPENDED_KEY = 'gastro_pos_suspended';
const PAID_KEY = 'gastro_pos_paid_invoices';
const MAX_PAID = 100;

@Injectable({ providedIn: 'root' })
export class PosInvoiceStoreService {
  private suspended = signal<PosSuspendedOrder[]>(this.loadSuspended());
  private paidInvoices = signal<PosInvoiceRecord[]>(this.loadPaid());

  suspendedOrders = this.suspended.asReadonly();
  paidInvoicesList = this.paidInvoices.asReadonly();

  suspendedCount = computed(() => this.suspended().length);
  paidCount = computed(() => this.paidInvoices().length);

  constructor() {
    // loaded from localStorage in signal initializers
  }

  suspendOrder(order: Omit<PosSuspendedOrder, 'id' | 'suspendedAt'>): PosSuspendedOrder {
    const record: PosSuspendedOrder = {
      ...order,
      id: `SUS-${Date.now()}`,
      suspendedAt: new Date().toISOString()
    };
    this.suspended.update(list => [record, ...list]);
    this.persistSuspended();
    return record;
  }

  removeSuspended(id: string): void {
    this.suspended.update(list => list.filter(o => o.id !== id));
    this.persistSuspended();
  }

  addPaidInvoice(invoice: PosInvoiceRecord): void {
    this.paidInvoices.update(list => [invoice, ...list].slice(0, MAX_PAID));
    this.persistPaid();
  }

  private loadSuspended(): PosSuspendedOrder[] {
    return this.readJson<PosSuspendedOrder[]>(SUSPENDED_KEY, []);
  }

  private loadPaid(): PosInvoiceRecord[] {
    return this.readJson<PosInvoiceRecord[]>(PAID_KEY, []);
  }

  private persistSuspended(): void {
    localStorage.setItem(SUSPENDED_KEY, JSON.stringify(this.suspended()));
  }

  private persistPaid(): void {
    localStorage.setItem(PAID_KEY, JSON.stringify(this.paidInvoices()));
  }

  private readJson<T>(key: string, fallback: T): T {
    try {
      const raw = localStorage.getItem(key);
      return raw ? JSON.parse(raw) : fallback;
    } catch {
      return fallback;
    }
  }
}
