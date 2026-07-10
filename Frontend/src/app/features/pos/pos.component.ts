import { Component, ChangeDetectionStrategy, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { DataService } from '../../core/services/data.service';
import { LanguageService } from '../../core/services/language.service';
import { POSLayoutComponent } from '../../shared/layouts/pos-layout/pos-layout.component';
import { CartItem, Product } from '../../core/models/erp.models';
import { SignalrService } from '../../core/services/signalr.service';
import { OfflineSyncService } from '../../core/services/offline-sync.service';
import {
  PosInvoiceStoreService,
  PosSuspendedOrder,
  PosInvoiceRecord,
  PosOrderType,
  DeliveryApp,
  DiscountType,
  PaymentMethod,
  PaymentBreakdown,
  PosInvoiceLine,
  PosInvoiceCustomer
} from '../../core/services/pos-invoice-store.service';
import { CompanyInvoiceProfileService, CompanyInvoiceProfile } from '../../core/services/company-invoice-profile.service';
import { buildZatcaTlvBase64 } from '../../core/utils/zatca-qr.util';
import { KitchenRepository } from '../../core/repositories/kitchen.repository';
import { MenuService } from '../../core/services/menu.service';

export type { PosOrderType, DeliveryApp, DiscountType, PaymentMethod, PaymentBreakdown, PosInvoiceLine, PosInvoiceCustomer };

export interface PosInvoice {
  invoiceNo: string;
  orderId: string;
  date: Date;
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

@Component({
  selector: 'app-pos',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatIconModule,
    POSLayoutComponent
  ],
  templateUrl: './pos.component.html',
  styleUrl: './pos.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PosComponent implements OnInit {
  dataService = inject(DataService);
  menuService = inject(MenuService);
  langService = inject(LanguageService);
  signalrService = inject(SignalrService);
  offlineSync = inject(OfflineSyncService);
  invoiceStore = inject(PosInvoiceStoreService);
  companyInvoiceProfile = inject(CompanyInvoiceProfileService);
  kitchenRepo = inject(KitchenRepository);

  categoryKeys = ['all', 'burgers', 'sides', 'drinks', 'desserts'] as const;
  activeCategoryKey = signal<string>('all');
  searchQuery = signal<string>('');

  orderType = signal<PosOrderType>('dineIn');
  tableNumber = signal('12');
  guestCount = signal(2);
  deliveryApp = signal<DeliveryApp>('hungerstation');
  deliveryOrderRef = signal('');

  discountType = signal<DiscountType>('percent');
  discountValue = signal(0);

  orderSentToKitchen = signal(false);
  currentOrderId = signal('');

  isSplitModalOpen = signal(false);
  splitCount = signal(2);
  activeSplitIndex = signal(1);

  isPaymentModalOpen = signal(false);
  pendingPayAmount = signal(0);
  paymentMethod = signal<PaymentMethod>('cash');
  cashPaidAmount = signal(0);
  lastPaymentBreakdown = signal<PaymentBreakdown | null>(null);

  isInvoiceModalOpen = signal(false);
  lastInvoice = signal<PosInvoice | null>(null);
  invoiceQrDataUrl = signal('');

  isInvoicesModalOpen = signal(false);
  invoicesTab = signal<'suspended' | 'paid'>('suspended');

  isCustomerModalOpen = signal(false);
  orderCustomer = signal<PosInvoiceCustomer>(this.createDefaultCustomer());
  customerDraft = signal<PosInvoiceCustomer>(this.createDefaultCustomer());

  paymentMethodOptions: PaymentMethod[] = ['cash', 'card', 'mixed'];

  cardPaidAmount = computed(() => {
    const total = this.pendingPayAmount();
    if (this.paymentMethod() === 'cash') return 0;
    if (this.paymentMethod() === 'card') return total;
    return Math.max(0, total - this.cashPaidAmount());
  });

  isPaymentValid = computed(() => {
    const total = this.pendingPayAmount();
    const method = this.paymentMethod();
    if (total <= 0) return false;
    if (method === 'cash' || method === 'card') return true;
    const cash = this.cashPaidAmount();
    return cash > 0 && cash < total;
  });

  orderTypeOptions: PosOrderType[] = ['dineIn', 'takeaway', 'delivery'];
  deliveryApps: DeliveryApp[] = ['hungerstation', 'jahez', 'talabat', 'careem', 'toYou'];
  tableOptions = Array.from({ length: 24 }, (_, i) => String(i + 1));

  categoriesDisplay = computed(() => {
    this.langService.language();
    return this.categoryKeys.map(k => ({ key: k, label: this.t('pos.cat.' + k) }));
  });

  orderTypeLabels = computed(() => {
    this.langService.language();
    return {
      dineIn: this.t('pos.dineIn'),
      takeaway: this.t('pos.takeaway'),
      delivery: this.t('pos.delivery')
    };
  });

  orderBadge = computed(() => {
    this.langService.language();
    const type = this.orderType();
    if (type === 'dineIn') {
      return `${this.t('pos.table')} ${this.tableNumber()}`;
    }
    if (type === 'takeaway') {
      return this.t('pos.takeaway');
    }
    return `${this.t('pos.app.' + this.deliveryApp())}${this.deliveryOrderRef() ? ' #' + this.deliveryOrderRef() : ''}`;
  });

  companyProfile = computed((): CompanyInvoiceProfile => this.companyInvoiceProfile.getProfile());

  customerDisplayName = computed(() => {
    this.langService.language();
    const c = this.orderCustomer();
    if (!c.name.trim() || c.name === this.t('pos.customer.defaultName')) {
      return this.t('pos.customer.defaultName');
    }
    return c.name;
  });

  discountAmount = computed(() => {
    const subtotal = this.dataService.cartSubtotal();
    const val = this.discountValue();
    if (val <= 0 || subtotal <= 0) return 0;
    if (this.discountType() === 'percent') {
      return Math.min(subtotal, subtotal * (Math.min(val, 100) / 100));
    }
    return Math.min(subtotal, val);
  });

  taxableSubtotal = computed(() => Math.max(0, this.dataService.cartSubtotal() - this.discountAmount()));
  orderTax = computed(() => this.taxableSubtotal() * 0.05);
  orderTotal = computed(() => this.taxableSubtotal() + this.orderTax());

  splitAmount = computed(() => {
    const count = Math.max(1, this.splitCount());
    return this.orderTotal() / count;
  });

  isCustomizing = signal(false);
  activeProduct = signal<Product | null>(null);
  selectedCustomizations = signal<string[]>([]);
  orderNotes = signal('');

  availableCustomizations: Record<string, string[]> = {
    burgers: ['جبن إضافي', 'بدون بصل', 'خبز خالي من الغلوتين', 'قطعة لحم مزدوجة'],
    sides: ['صلصة كمأة إضافية', 'بدون ملح', 'بارميزان إضافي'],
    drinks: ['ثلج إضافي', 'بدون سكر', 'بدون زينة']
  };

  filteredProducts = computed(() => {
    const categoryKey = this.activeCategoryKey();
    const query = this.searchQuery().toLowerCase();
    const source = this.menuService.menuProducts().length
      ? this.menuService.toPosProducts(this.menuService.menuProducts())
      : this.dataService.products;
    return source.filter(product => {
      const matchCategory = categoryKey === 'all' || product.categoryKey === categoryKey;
      const matchQuery = product.name.toLowerCase().includes(query) || product.description.toLowerCase().includes(query);
      return matchCategory && matchQuery;
    });
  });

  ngOnInit(): void {
    this.menuService.loadProducts();
  }

  selectProduct(product: Product): void {
    const customOptions = this.availableCustomizations[product.categoryKey];
    if (customOptions) {
      this.activeProduct.set(product);
      this.selectedCustomizations.set([]);
      this.orderNotes.set('');
      this.isCustomizing.set(true);
    } else {
      this.dataService.addToCart(product);
    }
  }

  toggleCustomization(option: string): void {
    this.selectedCustomizations.update(current =>
      current.includes(option) ? current.filter(opt => opt !== option) : [...current, option]
    );
  }

  addCustomizedItem(): void {
    const product = this.activeProduct();
    if (product) {
      this.dataService.addToCart(product, this.selectedCustomizations(), this.orderNotes());
      this.closeCustomizer();
    }
  }

  closeCustomizer(): void {
    this.isCustomizing.set(false);
    this.activeProduct.set(null);
  }

  setOrderType(type: PosOrderType): void {
    this.orderType.set(type);
    if (type === 'dineIn' && this.splitCount() < 2) {
      this.splitCount.set(Math.max(2, this.guestCount()));
    }
  }

  onGuestCountChange(value: number): void {
    const guests = Math.max(1, Math.min(20, value || 1));
    this.guestCount.set(guests);
    if (this.orderType() === 'dineIn') {
      this.splitCount.set(guests);
    }
  }

  toggleDiscountType(): void {
    this.discountType.update(t => (t === 'percent' ? 'fixed' : 'percent'));
    this.discountValue.set(0);
  }

  openSplitModal(): void {
    if (this.dataService.cart().length === 0) return;
    if (this.orderType() === 'dineIn') {
      this.splitCount.set(this.guestCount());
    }
    this.activeSplitIndex.set(1);
    this.isSplitModalOpen.set(true);
  }

  closeSplitModal(): void {
    this.isSplitModalOpen.set(false);
  }

  openPaymentModal(amount?: number): void {
    const total = amount ?? this.orderTotal();
    if (total <= 0 || this.dataService.cart().length === 0) return;
    this.pendingPayAmount.set(total);
    this.paymentMethod.set('cash');
    this.cashPaidAmount.set(total);
    this.isPaymentModalOpen.set(true);
    this.closeSplitModal();
  }

  closePaymentModal(): void {
    this.isPaymentModalOpen.set(false);
  }

  setPaymentMethod(method: PaymentMethod): void {
    this.paymentMethod.set(method);
    const total = this.pendingPayAmount();
    if (method === 'cash') {
      this.cashPaidAmount.set(total);
    } else if (method === 'card') {
      this.cashPaidAmount.set(0);
    } else {
      this.cashPaidAmount.set(Math.min(total * 0.5, total - 0.01));
    }
  }

  onCashAmountChange(value: number): void {
    const total = this.pendingPayAmount();
    this.cashPaidAmount.set(Math.max(0, Math.min(total, value || 0)));
  }

  paymentMethodLabel(method: PaymentMethod): string {
    return ({ cash: this.t('pos.payCash'), card: this.t('pos.payCard'), mixed: this.t('pos.payMixed') })[method];
  }

  paymentMethodInvoiceLabel(method: PaymentMethod): string {
    return this.paymentMethodLabel(method);
  }

  resolveTableLabel(): string {
    const type = this.orderType();
    if (type === 'dineIn') return `${this.t('pos.table')} ${this.tableNumber()}`;
    if (type === 'takeaway') return this.t('pos.takeaway');
    const app = this.t('pos.app.' + this.deliveryApp());
    const ref = this.deliveryOrderRef();
    return ref ? `${app} #${ref}` : app;
  }

  private ensureOrderId(): string {
    if (!this.currentOrderId()) {
      this.currentOrderId.set(`ORD-${Date.now()}`);
    }
    return this.currentOrderId();
  }

  private dispatchToKitchen(): void {
    const items = this.dataService.cart();
    if (items.length === 0) return;

    const orderId = this.ensureOrderId();
    const payload = {
      orderReference: orderId,
      tableLabel: this.resolveTableLabel(),
      orderType: this.orderType(),
      items: items.map(item => ({
        name: item.product.name,
        quantity: item.quantity,
        notes: [...item.customizations, ...(item.notes ? [item.notes] : [])].join(' | ') || undefined,
        categoryKey: item.product.categoryKey
      }))
    };

    this.kitchenRepo.dispatchFromPos(payload).subscribe({
      next: () => this.orderSentToKitchen.set(true),
      error: () => {
        this.signalrService.simulateEvent('order', {
          id: orderId.replace('ORD-', ''),
          tableNo: payload.tableLabel,
          timer: 0,
          status: 'new' as const,
          orderTime: new Date(),
          items: items.map(item => ({
            name: item.product.name,
            quantity: item.quantity,
            notes: item.customizations.concat(item.notes ? [item.notes] : [])
          }))
        });
        this.orderSentToKitchen.set(true);
      }
    });
  }

  /** إرسال للمطبخ فقط — لا يعني الدفع (محلي) */
  sendToKitchen(): void {
    if (this.dataService.cart().length === 0) return;

    if (this.orderType() !== 'dineIn') {
      alert(this.t('pos.payBeforeKitchen'));
      this.openPaymentModal();
      return;
    }

    this.dispatchToKitchen();
    alert(this.t('pos.sentKdsUnpaid'));
  }

  checkoutPay(amount?: number): void {
    this.openPaymentModal(amount);
  }

  private buildInvoiceLines(cart: CartItem[]): PosInvoiceLine[] {
    return cart.map(item => ({
      name: item.product.name,
      quantity: item.quantity,
      unitPrice: item.product.price,
      lineTotal: item.product.price * item.quantity,
      customizations: item.customizations,
      notes: item.notes
    }));
  }

  private buildInvoice(payment: PaymentBreakdown, total: number, cart: CartItem[]): PosInvoice {
    const orderId = this.ensureOrderId();
    const customer = { ...this.orderCustomer() };
    const isSimplified = !customer.taxNumber.trim();
    return {
      invoiceNo: `INV-${Date.now()}`,
      orderId,
      date: new Date(),
      branch: this.dataService.selectedBranch(),
      orderType: this.orderType(),
      tableLabel: this.resolveTableLabel(),
      lines: this.buildInvoiceLines(cart),
      subtotal: cart.reduce((s, i) => s + i.product.price * i.quantity, 0),
      discount: this.discountAmount(),
      tax: this.orderTax(),
      total,
      payment,
      sentToKitchen: this.orderSentToKitchen(),
      posted: true,
      customer,
      isSimplified
    };
  }

  createDefaultCustomer(): PosInvoiceCustomer {
    return {
      name: this.langService.tFor('pos.customer.defaultName', 'ar'),
      phone: '',
      taxNumber: '',
      email: '',
      address: ''
    };
  }

  tEn(key: string): string {
    return this.langService.tFor(key, 'en');
  }

  resolveInvoiceTitle(inv: PosInvoice): string {
    return inv.isSimplified ? this.t('pos.invoiceTitleSimplified') : this.t('pos.invoiceTitle');
  }

  resolveInvoiceTitleEn(inv: PosInvoice): string {
    return inv.isSimplified
      ? this.langService.tFor('pos.invoiceTitleSimplified', 'en')
      : this.langService.tFor('pos.invoiceTitle', 'en');
  }

  openCustomerModal(): void {
    this.customerDraft.set({ ...this.orderCustomer() });
    this.isCustomerModalOpen.set(true);
  }

  closeCustomerModal(): void {
    this.isCustomerModalOpen.set(false);
  }

  saveCustomerDraft(): void {
    const draft = this.customerDraft();
    this.orderCustomer.set({
      name: draft.name.trim() || this.createDefaultCustomer().name,
      phone: draft.phone.trim(),
      taxNumber: draft.taxNumber.trim(),
      email: draft.email.trim(),
      address: draft.address.trim()
    });
    this.closeCustomerModal();
  }

  resetCustomerToDefault(): void {
    const defaults = this.createDefaultCustomer();
    this.customerDraft.set({ ...defaults });
    this.orderCustomer.set({ ...defaults });
  }

  updateCustomerDraftField(field: keyof PosInvoiceCustomer, value: string): void {
    this.customerDraft.update(c => ({ ...c, [field]: value }));
  }

  /** الدفع → ترحيل → مطبخ (إن لم يُرسل) → فاتورة */
  confirmPayment(): void {
    if (!this.isPaymentValid()) {
      alert(this.t('pos.paymentInvalid'));
      return;
    }

    const total = this.pendingPayAmount();
    const method = this.paymentMethod();
    const cash = method === 'card' ? 0 : method === 'cash' ? total : this.cashPaidAmount();
    const card = method === 'cash' ? 0 : method === 'card' ? total : total - cash;
    const payment: PaymentBreakdown = { method, cash, card, total };

    const cartSnapshot = [...this.dataService.cart()];
    const orderId = this.ensureOrderId();

    if (!this.orderSentToKitchen()) {
      this.dispatchToKitchen();
    }

    const orderPayload = {
      id: orderId,
      cartItems: cartSnapshot,
      orderType: this.orderType(),
      tableNo: this.resolveTableLabel(),
      discount: this.discountAmount(),
      payment,
      total,
      posted: true,
      sentToKitchen: true,
      date: new Date()
    };

    this.offlineSync.addOrderToSyncQueue(orderPayload).catch(err => {
      console.error('Failed to add order to offline queue:', err);
    });

    const invoice = this.buildInvoice(payment, total, cartSnapshot);
    this.lastPaymentBreakdown.set(payment);
    this.lastInvoice.set(invoice);
    this.invoiceStore.addPaidInvoice(this.toInvoiceRecord(invoice));

    this.resetOrder();
    this.closePaymentModal();
    this.openInvoiceWithQr(invoice);
  }

  paySplitPortion(): void {
    this.openPaymentModal(this.splitAmount());
  }

  closeInvoiceModal(): void {
    this.isInvoiceModalOpen.set(false);
    this.lastInvoice.set(null);
    this.lastPaymentBreakdown.set(null);
    this.invoiceQrDataUrl.set('');
  }

  private openInvoiceWithQr(invoice: PosInvoice): void {
    this.lastInvoice.set(invoice);
    this.invoiceQrDataUrl.set('');
    this.isInvoiceModalOpen.set(true);
    void this.generateZatcaQrDataUrl(invoice).then(url => this.invoiceQrDataUrl.set(url));
  }

  async printInvoice(): Promise<void> {
    const inv = this.lastInvoice();
    if (!inv) return;
    await this.printInvoiceDocument(inv);
  }

  private async generateZatcaQrDataUrl(inv: PosInvoice): Promise<string> {
    const profile = this.companyProfile();
    const date = inv.date instanceof Date ? inv.date : new Date(inv.date);
    const tlv = buildZatcaTlvBase64({
      sellerName: profile.nameAr,
      vatNumber: profile.vatNumber,
      timestamp: date,
      invoiceTotal: inv.total,
      vatAmount: inv.tax
    });
    const QRCode = await import('qrcode');
    return QRCode.toDataURL(tlv, { errorCorrectionLevel: 'M', margin: 1, width: 168 });
  }

  private async printInvoiceDocument(inv: PosInvoice): Promise<void> {
    try {
      const qrDataUrl = await this.generateZatcaQrDataUrl(inv);
      document.getElementById('pos-invoice-print-root')?.remove();

      const wrapper = document.createElement('div');
      wrapper.id = 'pos-invoice-print-root';
      wrapper.innerHTML = this.buildPrintableInvoiceHtml(inv, qrDataUrl);
      document.body.appendChild(wrapper);
      document.body.classList.add('pos-print-active');

      let cleaned = false;
      const cleanup = (): void => {
        if (cleaned) return;
        cleaned = true;
        document.body.classList.remove('pos-print-active');
        wrapper.remove();
        window.removeEventListener('afterprint', cleanup);
      };

      window.addEventListener('afterprint', cleanup);

      requestAnimationFrame(() => {
        window.print();
        setTimeout(cleanup, 2500);
      });
    } catch (error) {
      console.error('[POS] Print failed:', error);
      alert(this.t('pos.printPopupBlocked'));
    }
  }

  private buildCompanyHeaderHtml(profile: CompanyInvoiceProfile): string {
    const logoHtml = profile.logoUrl
      ? `<div class="company-logo-wrapper"><img src="${this.escapeHtml(profile.logoUrl)}" alt="logo" class="company-logo" /></div>`
      : '';
    return `
    <div class="company-header">
      ${logoHtml}
      <div class="company-name-ar">${this.escapeHtml(profile.nameAr)}</div>
      <div class="company-name-en">${this.escapeHtml(profile.nameEn)}</div>
      <div class="company-info-lines">
        <div>${this.escapeHtml(profile.addressAr)} / ${this.escapeHtml(profile.addressEn)}</div>
        <div><strong>الرقم الضريبي / VAT:</strong> ${this.escapeHtml(profile.vatNumber)}</div>
        <div><strong>السجل التجاري / CR:</strong> ${this.escapeHtml(profile.crNumber)}</div>
        <div><strong>الهاتف / Phone:</strong> ${this.escapeHtml(profile.phone)}</div>
      </div>
    </div>`;
  }

  private buildCustomerSectionHtml(customer: PosInvoiceCustomer, isSimplified: boolean): string {
    const defaultCust = this.createDefaultCustomer();
    const isDefault = customer.name === defaultCust.name && !customer.phone && !customer.taxNumber && !customer.email && !customer.address;
    if (isDefault) {
      return '';
    }

    const row = (labelAr: string, labelEn: string, value: string) =>
      value
        ? `<div class="customer-line"><span class="customer-label-ar"><strong>${this.escapeHtml(labelAr)}:</strong></span> <span class="customer-label-en"><strong>${this.escapeHtml(labelEn)}:</strong></span> ${this.escapeHtml(value)}</div>`
        : '';

    return `
    <div class="customer-section">
      <div class="customer-heading">${this.escapeHtml(this.t('pos.customer.title'))} / ${this.escapeHtml(this.tEn('pos.customer.title'))}</div>
      ${row(this.t('pos.customer.name'), this.tEn('pos.customer.name'), customer.name)}
      ${row(this.t('pos.customer.phone'), this.tEn('pos.customer.phone'), customer.phone)}
      ${customer.taxNumber ? row(this.t('pos.customer.tax'), this.tEn('pos.customer.tax'), customer.taxNumber) : ''}
      ${row(this.t('pos.customer.email'), this.tEn('pos.customer.email'), customer.email)}
      ${row(this.t('pos.customer.address'), this.tEn('pos.customer.address'), customer.address)}
    </div>`;
  }

  private buildPrintableInvoiceHtml(inv: PosInvoice, qrDataUrl: string): string {
    const isRtl = this.langService.language() === 'ar';
    const dir = isRtl ? 'rtl' : 'ltr';
    const currency = this.t('common.currency');
    const payment = inv.payment ?? { method: 'cash' as PaymentMethod, cash: inv.total, card: 0, total: inv.total };
    const orderTypeLabel = this.orderTypeLabels()[inv.orderType] ?? inv.orderType;
    const lines = inv.lines ?? [];

    const linesHtml = lines.map(line => {
      const mods = line.customizations?.length
        ? `<div class="mods">${line.customizations.map(c => `+ ${this.escapeHtml(c)}`).join(' · ')}</div>`
        : '';
      const notes = line.notes ? `<div class="notes">${this.escapeHtml(line.notes)}</div>` : '';
      return `
        <tr>
          <td class="item-name">
            <strong>${this.escapeHtml(line.name)}</strong>
            ${mods}${notes}
          </td>
          <td class="num">${line.quantity}</td>
          <td class="num">${line.unitPrice.toFixed(2)}</td>
          <td class="num">${line.lineTotal.toFixed(2)}</td>
        </tr>`;
    }).join('');

    const discountRow = inv.discount > 0
      ? `<div class="row discount"><span>${this.escapeHtml(this.t('pos.discountAmount'))}</span><span>− ${inv.discount.toFixed(2)} ${currency}</span></div>`
      : '';

    const cashRow = payment.cash > 0
      ? `<div class="row"><span>${this.escapeHtml(this.t('pos.paidCash'))}</span><span>${payment.cash.toFixed(2)} ${currency}</span></div>`
      : '';

    const cardRow = payment.card > 0
      ? `<div class="row"><span>${this.escapeHtml(this.t('pos.paidCard'))}</span><span>${payment.card.toFixed(2)} ${currency}</span></div>`
      : '';

    const companyHeader = this.buildCompanyHeaderHtml(this.companyProfile());
    const customer = inv.customer ?? this.createDefaultCustomer();
    const isSimplified = inv.isSimplified ?? !customer.taxNumber.trim();
    const customerSection = this.buildCustomerSectionHtml(customer, isSimplified);
    const invoiceTitleAr = this.resolveInvoiceTitle(inv);
    const invoiceTitleEn = this.resolveInvoiceTitleEn(inv);

    return `
<style>
  @media print {
    @page {
      margin: 0;
      size: 80mm auto;
    }
  }
  .pos-invoice-print-sheet {
    font-family: 'Tajawal', 'Segoe UI', Tahoma, Arial, sans-serif;
    color: #000;
    background: #fff;
    padding: 4px 6px;
    line-height: 1.45;
    direction: ${dir};
    width: 80mm;
    max-width: 80mm;
    margin: 0 auto;
    box-sizing: border-box;
    font-size: 11px;
  }
  .pos-invoice-print-sheet * {
    box-sizing: border-box;
    background: transparent !important;
    background-color: transparent !important;
  }
  .pos-invoice-print-sheet .invoice {
    width: 100%;
    margin: 0;
    padding: 0;
  }
  .pos-invoice-print-sheet .company-header {
    display: flex;
    flex-direction: column;
    align-items: center;
    text-align: center;
    border-bottom: 1px dashed #000;
    padding-bottom: 8px;
    margin-bottom: 10px;
  }
  .pos-invoice-print-sheet .company-logo-wrapper {
    margin-bottom: 6px;
    text-align: center;
  }
  .pos-invoice-print-sheet .company-logo {
    width: 50px;
    height: 50px;
    object-fit: contain;
    border-radius: 4px;
  }
  .pos-invoice-print-sheet .company-name-ar {
    font-size: 14px;
    font-weight: 800;
    margin-bottom: 2px;
  }
  .pos-invoice-print-sheet .company-name-en {
    font-size: 12px;
    font-weight: 700;
    margin-bottom: 4px;
    text-transform: uppercase;
  }
  .pos-invoice-print-sheet .company-info-lines {
    font-size: 9px;
    line-height: 1.4;
    color: #111;
  }
  .pos-invoice-print-sheet .customer-section {
    border: 1px dashed #000;
    border-radius: 6px;
    padding: 8px 10px;
    margin-bottom: 10px;
    font-size: 10px;
  }
  .pos-invoice-print-sheet .customer-heading {
    font-weight: 800;
    margin-bottom: 6px;
    font-size: 11px;
    text-align: center;
    border-bottom: 1px solid #ddd;
    padding-bottom: 4px;
  }
  .pos-invoice-print-sheet .customer-line {
    margin-bottom: 3px;
  }
  .pos-invoice-print-sheet .invoice-title-row {
    text-align: center;
    margin-bottom: 12px;
    border-bottom: 1px dashed #000;
    padding-bottom: 8px;
  }
  .pos-invoice-print-sheet .title {
    font-size: 16px;
    font-weight: 800;
    margin: 0;
  }
  .pos-invoice-print-sheet .subtitle {
    font-size: 10px;
    color: #222;
    margin-top: 2px;
  }
  .pos-invoice-print-sheet .meta {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    font-size: 10px;
    margin-bottom: 12px;
    border-bottom: 1px dashed #000;
    padding-bottom: 8px;
  }
  .pos-invoice-print-sheet .badge {
    font-weight: 700;
    border: 1px solid #000;
    padding: 1px 5px;
    font-size: 9px;
    border-radius: 3px;
    white-space: nowrap;
  }
  .pos-invoice-print-sheet table {
    width: 100%;
    border-collapse: collapse;
    margin-bottom: 12px;
    font-size: 10.5px;
  }
  .pos-invoice-print-sheet th,
  .pos-invoice-print-sheet td {
    padding: 6px 4px;
    border-bottom: 1px dashed #ddd;
    vertical-align: top;
  }
  .pos-invoice-print-sheet th {
    font-size: 9px;
    font-weight: 800;
    border-bottom: 1px solid #000;
    border-top: 1px solid #000;
    text-transform: uppercase;
  }
  .pos-invoice-print-sheet .item-name {
    width: 45%;
  }
  .pos-invoice-print-sheet .mods {
    font-size: 9px;
    color: #333;
    margin-top: 2px;
  }
  .pos-invoice-print-sheet .notes {
    font-size: 9px;
    color: #444;
    font-style: italic;
    margin-top: 1px;
  }
  .pos-invoice-print-sheet .num {
    text-align: ${isRtl ? 'left' : 'right'};
    white-space: nowrap;
    font-variant-numeric: tabular-nums;
  }
  .pos-invoice-print-sheet .totals,
  .pos-invoice-print-sheet .payment {
    border-top: 1px dashed #000;
    padding: 8px 0;
    margin-bottom: 8px;
    font-size: 11px;
  }
  .pos-invoice-print-sheet .payment {
    border-bottom: 1px dashed #000;
    margin-bottom: 12px;
  }
  .pos-invoice-print-sheet .row {
    display: flex;
    justify-content: space-between;
    padding: 3px 0;
  }
  .pos-invoice-print-sheet .row.grand {
    border-top: 1px solid #000;
    margin-top: 6px;
    padding-top: 8px;
    font-size: 14px;
    font-weight: 800;
  }
  .pos-invoice-print-sheet .payment-title {
    font-weight: 800;
    margin-bottom: 4px;
  }
  .pos-invoice-print-sheet .qr-footer {
    margin-top: 14px;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 6px;
  }
  .pos-invoice-print-sheet .qr-footer img {
    width: 110px;
    height: 110px;
  }
  .pos-invoice-print-sheet .qr-label {
    font-size: 9px;
    font-weight: 700;
    color: #111;
    text-align: center;
  }
  .pos-invoice-print-sheet .footer {
    margin-top: 10px;
    font-size: 9px;
    color: #333;
    text-align: center;
    border-top: 1px dashed #ccc;
    padding-top: 6px;
  }
</style>
<div class="pos-invoice-print-sheet">
  <div class="invoice">
    ${companyHeader}

    <div class="invoice-title-row">
      <div class="title">${this.escapeHtml(invoiceTitleAr)}</div>
      <div class="subtitle">${this.escapeHtml(invoiceTitleEn)}</div>
    </div>

    ${customerSection}

    <div class="meta">
      <div>
        <div><strong>${this.escapeHtml(this.t('pos.invoiceNo'))}:</strong> ${this.escapeHtml(inv.invoiceNo)}</div>
        <div><strong>${this.escapeHtml(this.t('pos.invoiceDate'))}:</strong> ${this.escapeHtml(this.formatInvoiceDate(inv.date))}</div>
        <div><strong>${this.escapeHtml(this.t('pos.invoiceBranch'))}:</strong> ${this.escapeHtml(inv.branch)}</div>
      </div>
      <div class="badge">${this.escapeHtml(inv.tableLabel)} · ${this.escapeHtml(orderTypeLabel)}</div>
    </div>

    <table>
      <thead>
        <tr>
          <th>${this.escapeHtml(this.t('pos.invoiceItems'))}</th>
          <th class="num">${this.escapeHtml(this.t('pos.invoiceQty'))}</th>
          <th class="num">${this.escapeHtml(this.t('pos.invoiceUnit'))}</th>
          <th class="num">${this.escapeHtml(this.t('pos.invoiceLineTotal'))}</th>
        </tr>
      </thead>
      <tbody>${linesHtml}</tbody>
    </table>

    <div class="totals">
      <div class="row"><span>${this.escapeHtml(this.t('pos.subtotal'))}</span><span>${inv.subtotal.toFixed(2)} ${currency}</span></div>
      ${discountRow}
      <div class="row"><span>${this.escapeHtml(this.t('pos.tax'))}</span><span>${inv.tax.toFixed(2)} ${currency}</span></div>
      <div class="row grand"><span>${this.escapeHtml(this.t('pos.total'))}</span><span>${inv.total.toFixed(2)} ${currency}</span></div>
    </div>

    <div class="payment">
      <div class="payment-title">${this.escapeHtml(this.t('pos.paymentMethod'))}: ${this.escapeHtml(this.paymentMethodInvoiceLabel(payment.method))}</div>
      ${cashRow}
      ${cardRow}
    </div>

    <div class="qr-footer">
      <img src="${qrDataUrl}" alt="ZATCA QR" />
      <div class="qr-label">${this.escapeHtml(this.t('pos.invoice.zatcaQr'))}</div>
    </div>

    <div class="footer">GastroERP · ${this.escapeHtml(inv.orderId)}</div>
  </div>
</div>`;
  }

  private escapeHtml(value: string): string {
    return value
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  openInvoicesModal(tab: 'suspended' | 'paid' = 'suspended'): void {
    this.invoicesTab.set(tab);
    this.isInvoicesModalOpen.set(true);
  }

  closeInvoicesModal(): void {
    this.isInvoicesModalOpen.set(false);
  }

  suspendCurrentInvoice(): void {
    if (this.dataService.cart().length === 0) return;
    this.saveCurrentAsSuspended();
    this.resetOrder();
    alert(this.t('pos.suspendSuccess'));
  }

  recallSuspendedOrder(order: PosSuspendedOrder): void {
    if (this.dataService.cart().length > 0) {
      if (!confirm(this.t('pos.replaceCartConfirm'))) return;
      this.saveCurrentAsSuspended();
    }
    this.applySuspendedOrder(order);
    this.invoiceStore.removeSuspended(order.id);
    this.closeInvoicesModal();
    alert(this.t('pos.recallSuccess'));
  }

  deleteSuspendedOrder(id: string): void {
    this.invoiceStore.removeSuspended(id);
  }

  viewPaidInvoice(record: PosInvoiceRecord): void {
    this.closeInvoicesModal();
    this.openInvoiceWithQr(this.fromInvoiceRecord(record));
  }

  reprintPaidInvoice(record: PosInvoiceRecord): void {
    this.closeInvoicesModal();
    void this.printInvoiceDocument(this.fromInvoiceRecord(record));
  }

  private saveCurrentAsSuspended(): void {
    const cart = this.dataService.cart();
    if (cart.length === 0) return;

    this.invoiceStore.suspendOrder({
      label: this.resolveTableLabel(),
      cartItems: cart.map(item => ({
        ...item,
        product: { ...item.product },
        customizations: [...item.customizations]
      })),
      orderType: this.orderType(),
      tableNumber: this.tableNumber(),
      guestCount: this.guestCount(),
      deliveryApp: this.deliveryApp(),
      deliveryOrderRef: this.deliveryOrderRef(),
      discountType: this.discountType(),
      discountValue: this.discountValue(),
      orderSentToKitchen: this.orderSentToKitchen(),
      orderId: this.ensureOrderId(),
      itemCount: cart.reduce((n, i) => n + i.quantity, 0),
      total: this.orderTotal(),
      customer: { ...this.orderCustomer() }
    });
  }

  private applySuspendedOrder(order: PosSuspendedOrder): void {
    this.dataService.setCart(order.cartItems);
    this.orderType.set(order.orderType);
    this.tableNumber.set(order.tableNumber);
    this.guestCount.set(order.guestCount);
    this.deliveryApp.set(order.deliveryApp);
    this.deliveryOrderRef.set(order.deliveryOrderRef);
    this.discountType.set(order.discountType);
    this.discountValue.set(order.discountValue);
    this.orderSentToKitchen.set(order.orderSentToKitchen);
    this.currentOrderId.set(order.orderId);
    this.orderCustomer.set(order.customer ? { ...order.customer } : this.createDefaultCustomer());
  }

  private toInvoiceRecord(invoice: PosInvoice): PosInvoiceRecord {
    return { ...invoice, date: invoice.date.toISOString() };
  }

  private fromInvoiceRecord(record: PosInvoiceRecord): PosInvoice {
    const customer = record.customer ?? this.createDefaultCustomer();
    const isSimplified = record.isSimplified ?? !customer.taxNumber.trim();
    return { ...record, date: new Date(record.date), customer, isSimplified };
  }

  formatSuspendedDate(iso: string): string {
    return this.formatInvoiceDate(new Date(iso));
  }

  formatInvoiceDate(date: Date | string): string {
    const parsed = date instanceof Date ? date : new Date(date);
    if (Number.isNaN(parsed.getTime())) return '—';
    return new Intl.DateTimeFormat(this.langService.language() === 'ar' ? 'ar-SA' : 'en-GB', {
      dateStyle: 'medium',
      timeStyle: 'short'
    }).format(parsed);
  }

  private resetOrder(): void {
    this.dataService.clearCart();
    this.discountValue.set(0);
    this.deliveryOrderRef.set('');
    this.cashPaidAmount.set(0);
    this.paymentMethod.set('cash');
    this.orderSentToKitchen.set(false);
    this.currentOrderId.set('');
    this.orderCustomer.set(this.createDefaultCustomer());
  }

  formatMoney(value: number): string {
    return `${this.formatPriceAmount(value)} ${this.t('common.currency')}`;
  }

  formatPriceAmount(value: number): string {
    return value.toFixed(2);
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
