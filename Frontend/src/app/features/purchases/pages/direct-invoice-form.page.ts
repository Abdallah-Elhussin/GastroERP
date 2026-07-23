import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { catchError, of } from 'rxjs';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { InventoryService } from '../../../core/services/inventory.service';
import { InventoryRepository } from '../../../core/repositories/inventory.repository';
import { PurchaseInvoiceRepository } from '../../../core/repositories/purchase-invoice.repository';
import {
  CreatePurchaseInvoicePayload,
  DirectInvoiceLineDraft,
  PurchaseInvoiceDoc,
  PurchaseInvoiceLineInput,
  UpdatePurchaseInvoicePayload
} from '../../../core/models/purchase-invoice.models';
import {
  InventoryItemDefinition,
  InventoryUnit,
  SupplierSummary,
  Warehouse
} from '../../../core/models/inventory.models';
import { InventoryPageShellComponent } from '../../inventory/shared/inventory-page-shell.component';
import { AppDialogComponent } from '../../../shared/ui/app-dialog/app-dialog.component';

const DIRECT_KIND = 2;
const NATURE_INVENTORY = 1;

type ResultDialogState = {
  open: boolean;
  success: boolean;
  title: string;
  message: string;
  navigateToId?: string | null;
};

@Component({
  selector: 'app-direct-invoice-form-page',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, InventoryPageShellComponent, AppDialogComponent],
  templateUrl: './direct-invoice-form.page.html',
  styleUrl: './direct-invoice-form.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DirectInvoiceFormPage implements OnInit {
  private repo = inject(PurchaseInvoiceRepository);
  private inventoryRepo = inject(InventoryRepository);
  private inventory = inject(InventoryService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  resultDialog = signal<ResultDialogState>({
    open: false,
    success: true,
    title: '',
    message: ''
  });

  docId = signal<string | null>(null);
  invoiceNumber = signal('');
  invoiceDate = signal(this.todayIso());
  dueDate = signal<string | null>(null);
  status = signal('Draft');
  nature = signal<number>(NATURE_INVENTORY);
  paymentMode = signal<number>(0);
  settlementMethod = signal('');
  supplierId = signal<string | null>(null);
  warehouseId = signal<string | null>(null);
  currency = signal('SAR');
  exchangeRate = signal(1);
  supplierInvoiceNumber = signal('');
  externalReference = signal('');
  notes = signal('');
  headerDiscountAmount = signal(0);
  paidAmount = signal(0);
  remainingAmount = signal(0);
  paymentStatus = signal<number>(1);

  lines = signal<DirectInvoiceLineDraft[]>([]);
  selectedLineIndex = signal<number | null>(null);

  items = signal<InventoryItemDefinition[]>([]);
  units = signal<InventoryUnit[]>([]);
  warehouses = signal<Warehouse[]>([]);
  suppliers = signal<SupplierSummary[]>([]);

  breadcrumbs = [
    { labelKey: 'nav.purchases', path: '/purchases/dashboard' },
    { labelKey: 'pur.nav.directInvoices', path: '/purchases/direct-invoices' },
    { labelKey: 'pur.dpi.formBreadcrumb' }
  ];

  canManage = computed(() => this.auth.hasPermission('Inventory.Manage'));
  isDraft = computed(() => this.status() === 'Draft');
  isApproved = computed(() => this.status() === 'Approved');
  isPosted = computed(() => this.status() === 'Posted');
  isNew = computed(() => !this.docId());
  isInventoryNature = computed(() => this.nature() === NATURE_INVENTORY);
  pageTitle = computed(() => this.t('pur.dpi.docTitle'));
  statusLabel = computed(() => {
    switch (this.status()) {
      case 'Approved':
        return this.t('pur.dpi.status.approved');
      case 'Posted':
        return this.t('pur.dpi.status.posted');
      case 'Reversed':
        return this.t('pur.dpi.status.reversed');
      case 'Cancelled':
        return this.t('pur.dpi.status.cancelled');
      default:
        return this.t('pur.dpi.status.draft');
    }
  });
  paymentStatusLabel = computed(() => {
    switch (this.paymentStatus()) {
      case 2:
        return this.t('pur.dpi.paymentStatus.partiallyPaid');
      case 3:
        return this.t('pur.dpi.paymentStatus.fullyPaid');
      case 4:
        return this.t('pur.dpi.paymentStatus.fullyReturned');
      default:
        return this.t('pur.dpi.paymentStatus.unpaid');
    }
  });
  qtyTotal = computed(() => this.lines().reduce((s, l) => s + (Number(l.quantity) || 0), 0));
  lineNetTotal = computed(() => this.lines().reduce((s, l) => s + (Number(l.lineNet) || 0), 0));
  lineDiscountTotal = computed(() =>
    this.lines().reduce((s, l) => s + (Number(l.discountAmount) || 0), 0)
  );
  lineTaxTotal = computed(() => this.lines().reduce((s, l) => s + (Number(l.taxAmount) || 0), 0));
  subTotal = computed(() => this.lineNetTotal());
  taxAmount = computed(() => this.lineTaxTotal());
  netTotal = computed(() =>
    Math.max(0, this.subTotal() - (Number(this.headerDiscountAmount()) || 0) + this.taxAmount())
  );

  ngOnInit(): void {
    this.loadLookups();

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.docId.set(id);
      this.loadDoc(id);
    } else {
      this.loadNextInvoiceNumber();
      if (this.lines().length === 0) this.addLine();
    }
  }

  private loadLookups(): void {
    this.inventoryRepo
      .getItems(undefined, 1, 200)
      .pipe(catchError(() => of([] as InventoryItemDefinition[])))
      .subscribe(rows => {
        const active = rows.filter(i => i.isActive !== false);
        this.items.set(active);
        this.inventory.items.set(active);
      });

    this.inventoryRepo
      .getUnits()
      .pipe(catchError(() => of([] as InventoryUnit[])))
      .subscribe(rows => {
        this.units.set(rows);
        this.inventory.units.set(rows);
      });

    this.inventoryRepo
      .getWarehouseLookup()
      .pipe(catchError(() => of([] as Warehouse[])))
      .subscribe(rows => {
        // Prefer warehouses that can receive / purchase stock.
        const usable = rows.filter(
          w =>
            w.isActive !== false &&
            (w.allowReceiving !== false || w.allowPurchase !== false)
        );
        const list = usable.length > 0 ? usable : rows.filter(w => w.isActive !== false);
        this.warehouses.set(list);
        this.inventory.warehouses.set(list);

        const preferred =
          list.find(w => w.isDefault && w.isActive) ?? list.find(w => w.isActive) ?? list[0] ?? null;
        if (preferred) {
          if (!this.warehouseId()) this.warehouseId.set(preferred.id);
          const defaultId = this.warehouseId() || preferred.id;
          this.lines.set(
            this.lines().map(l =>
              l.lineWarehouseId ? l : { ...l, lineWarehouseId: defaultId }
            )
          );
        }
      });

    this.inventory
      .getSuppliers(1, 200)
      .pipe(catchError(() => of([] as SupplierSummary[])))
      .subscribe(s => this.suppliers.set(s));
  }

  private loadNextInvoiceNumber(): void {
    this.repo
      .getNextNumber(2)
      .pipe(catchError(() => of('')))
      .subscribe(n => {
        if (n) this.invoiceNumber.set(n);
      });
  }

  itemLabel(item: InventoryItemDefinition): string {
    const code = (item.sku || item.barcode || '').trim();
    return code ? `${code} — ${item.nameAr}` : item.nameAr;
  }

  warehouseLabel(w: Warehouse): string {
    const code = (w.code || '').trim();
    return code ? `${code} — ${w.nameAr}` : w.nameAr;
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  acknowledgeResult(): void {
    const navId = this.resultDialog().navigateToId;
    this.resultDialog.set({ open: false, success: true, title: '', message: '' });
    if (navId) {
      void this.router.navigate(['/purchases/direct-invoices', navId], { replaceUrl: true });
    }
  }

  onNatureChange(value: number): void {
    if (!this.isDraft()) return;
    this.nature.set(Number(value));
    if (Number(value) !== NATURE_INVENTORY) {
      this.warehouseId.set(null);
    }
  }

  onPaymentModeChange(value: number): void {
    if (!this.isDraft()) return;
    this.paymentMode.set(Number(value));
    if (Number(value) === 2) {
      this.settlementMethod.set('cash');
    } else if (Number(value) === 1) {
      this.settlementMethod.set('credit');
    } else {
      this.settlementMethod.set('');
    }
  }

  addLine(): void {
    if (!this.isDraft()) return;
    const draft: DirectInvoiceLineDraft = {
      inventoryItemId: '',
      unitId: '',
      quantity: 1,
      unitPrice: 0,
      discountPercent: 0,
      discountAmount: 0,
      taxPercent: 15,
      taxAmount: 0,
      lineNet: 0,
      lineTotal: 0,
      returnedQuantity: 0,
      remainingToReturn: 0,
      lineWarehouseId: this.warehouseId()
    };
    this.lines.set([...this.lines(), draft]);
    this.selectedLineIndex.set(this.lines().length - 1);
  }

  removeSelectedLine(): void {
    const idx = this.selectedLineIndex();
    if (idx == null || !this.isDraft()) return;
    this.lines.set(this.lines().filter((_, i) => i !== idx));
    this.selectedLineIndex.set(null);
  }

  recalculateAll(): void {
    if (!this.isDraft()) return;
    this.lines.set(this.lines().map(l => this.recalcLine(l)));
  }

  selectLine(index: number): void {
    this.selectedLineIndex.set(index);
  }

  onItemChange(index: number, itemId: string): void {
    if (!this.isDraft()) return;
    const item = this.items().find(i => i.id === itemId);
    const next = [...this.lines()];
    const row = { ...next[index], inventoryItemId: itemId };
    if (item) {
      row.itemNameAr = item.nameAr;
      row.unitId = item.defaultPurchaseUnitId || item.baseUnitId;
      row.unitNameAr = item.baseUnitNameAr;
      row.unitPrice = item.lastPurchaseUnitCost ?? item.averageUnitCost ?? 0;
    }
    next[index] = this.recalcLine(row);
    this.lines.set(next);
  }

  updateLine(index: number, patch: Partial<DirectInvoiceLineDraft>): void {
    if (!this.isDraft()) return;
    const next = [...this.lines()];
    const row = this.recalcLine({ ...next[index], ...patch });
    next[index] = row;
    this.lines.set(next);
  }

  save(): void {
    if (!this.canManage() || !this.isDraft()) return;
    if (!this.supplierId()) {
      this.error.set(this.t('pur.dpi.validation.supplier'));
      return;
    }
    if (this.paymentMode() !== 1 && this.paymentMode() !== 2) {
      this.error.set(this.t('pur.dpi.validation.invoiceType'));
      return;
    }
    if (!this.warehouseId() && this.lines().some(l => !l.lineWarehouseId)) {
      this.error.set(this.t('pur.dpi.validation.warehouse'));
      return;
    }
    const activeLines = this.lines().filter(l => l.inventoryItemId && Number(l.quantity) > 0);
    if (activeLines.length === 0) {
      this.error.set(this.t('pur.dpi.validation.lines'));
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    this.nature.set(NATURE_INVENTORY);
    const linePayload = this.buildLinePayload(activeLines);

    if (this.docId()) {
      const payload: UpdatePurchaseInvoicePayload = {
        invoiceDate: this.toDateOnly(this.invoiceDate()),
        paymentMode: this.paymentMode(),
        dueDate: this.dueDate() ? this.toDateOnly(this.dueDate()!) : null,
        supplierInvoiceNumber: this.supplierInvoiceNumber() || null,
        notes: this.notes() || null,
        warehouseId: this.warehouseId(),
        nature: NATURE_INVENTORY,
        exchangeRate: this.exchangeRate(),
        externalReference: this.externalReference() || null,
        discountAmount: this.headerDiscountAmount(),
        lines: linePayload
      };
      this.repo.update(this.docId()!, payload).subscribe({
        next: doc => {
          this.applyDoc(doc);
          this.saving.set(false);
          this.openResult(true, this.t('pur.dpi.result.savedTitle'), this.t('pur.dpi.result.savedMessage'));
        },
        error: err => {
          const reason = this.extractError(err, this.t('pur.dpi.saveFailed'));
          this.error.set(reason);
          this.saving.set(false);
          this.openResult(false, this.t('pur.dpi.result.failedTitle'), reason);
        }
      });
      return;
    }

    const createPayload: CreatePurchaseInvoicePayload = {
      kind: DIRECT_KIND,
      paymentMode: this.paymentMode(),
      supplierId: this.supplierId()!,
      invoiceDate: this.toDateOnly(this.invoiceDate()),
      currency: this.currency(),
      invoiceNumber: this.invoiceNumber() || null,
      warehouseId: this.warehouseId(),
      dueDate: this.dueDate() ? this.toDateOnly(this.dueDate()!) : null,
      supplierInvoiceNumber: this.supplierInvoiceNumber() || null,
      notes: this.notes() || null,
      nature: NATURE_INVENTORY,
      exchangeRate: this.exchangeRate(),
      externalReference: this.externalReference() || null,
      discountAmount: this.headerDiscountAmount(),
      lines: linePayload
    };

    this.repo.create(createPayload).subscribe({
      next: doc => {
        this.saving.set(false);
        this.invoiceNumber.set(doc.invoiceNumber || this.invoiceNumber());
        this.openResult(
          true,
          this.t('pur.dpi.result.savedTitle'),
          this.t('pur.dpi.result.savedMessage'),
          doc.id
        );
      },
      error: err => {
        const reason = this.extractError(err, this.t('pur.dpi.saveFailed'));
        this.error.set(reason);
        this.saving.set(false);
        this.openResult(false, this.t('pur.dpi.result.failedTitle'), reason);
      }
    });
  }

  approve(): void {
    const id = this.docId();
    if (!id || !this.canManage()) return;
    this.runAction(
      () => this.repo.approve(id),
      {
        successTitle: this.t('pur.dpi.result.approveSuccessTitle'),
        successMessage: this.t('pur.dpi.result.approveSuccessMessage'),
        failedTitle: this.t('pur.dpi.result.approveFailedTitle')
      }
    );
  }

  post(): void {
    const id = this.docId();
    if (!id || !this.canManage() || !this.isApproved()) return;
    const docNo = this.invoiceNumber() || id;
    this.runAction(
      () => this.repo.post(id),
      {
        successTitle: this.t('pur.dpi.result.postSuccessTitle'),
        successMessage: this.t('pur.dpi.result.postSuccessMessage').replace('{number}', docNo),
        failedTitle: this.t('pur.dpi.result.postFailedTitle')
      }
    );
  }

  unpost(): void {
    const id = this.docId();
    if (!id || !this.canManage()) return;
    this.runAction(() => this.repo.unpost(id));
  }

  cancelDoc(): void {
    const id = this.docId();
    if (!id || !this.canManage()) return;
    if (!confirm(this.t('pur.dpi.confirmCancel'))) return;
    this.runAction(() => this.repo.cancel(id));
  }

  createReturn(): void {
    const id = this.docId();
    if (!id || !this.isPosted()) return;
    void this.router.navigate(['/purchases/invoice-returns/new'], {
      queryParams: { purchaseInvoiceId: id }
    });
  }

  back(): void {
    void this.router.navigate(['/purchases/direct-invoices']);
  }

  unitName(unitId: string): string {
    return this.units().find(u => u.id === unitId)?.nameAr ?? '—';
  }

  private loadDoc(id: string): void {
    this.loading.set(true);
    this.repo.getById(id).subscribe({
      next: doc => {
        this.applyDoc(doc);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('pur.dpi.loadFailed'));
        this.loading.set(false);
      }
    });
  }

  private applyDoc(doc: PurchaseInvoiceDoc): void {
    this.docId.set(doc.id);
    this.invoiceNumber.set(doc.invoiceNumber);
    this.invoiceDate.set(this.toDateInput(doc.invoiceDate));
    this.dueDate.set(doc.dueDate ? this.toDateInput(doc.dueDate) : null);
    this.status.set(this.mapStatus(doc.status));
    this.nature.set(Number(doc.nature) || NATURE_INVENTORY);
    const mode = Number(doc.paymentMode) || 1;
    this.paymentMode.set(mode);
    this.settlementMethod.set(mode === 2 ? 'cash' : 'credit');
    this.supplierId.set(doc.supplierId);
    this.warehouseId.set(doc.warehouseId ?? null);
    this.currency.set(doc.currency || 'SAR');
    this.exchangeRate.set(doc.exchangeRate || 1);
    this.supplierInvoiceNumber.set(doc.supplierInvoiceNumber || '');
    this.externalReference.set(doc.externalReference || '');
    this.notes.set(doc.notes || '');
    this.headerDiscountAmount.set(doc.discountAmount || 0);
    this.paidAmount.set(doc.paidAmount || 0);
    this.remainingAmount.set(doc.remainingAmount || 0);
    this.paymentStatus.set(Number(doc.paymentStatus) || 1);
    this.lines.set(
      (doc.lines || []).map(l => {
        const item = this.items().find(i => i.id === l.inventoryItemId);
        return this.recalcLine({
          ...l,
          itemNameAr: item?.nameAr ?? l.itemNameAr,
          unitNameAr: this.unitName(l.unitId)
        });
      })
    );
  }

  private buildLinePayload(lines: DirectInvoiceLineDraft[]): PurchaseInvoiceLineInput[] {
    return lines.map(l => ({
      inventoryItemId: l.inventoryItemId,
      unitId: l.unitId,
      quantity: Number(l.quantity) || 0,
      unitPrice: Number(l.unitPrice) || 0,
      discountPercent: Number(l.discountPercent) || 0,
      discountAmount: Number(l.discountAmount) || 0,
      taxPercent: Number(l.taxPercent) || 0,
      taxAmount: Number(l.taxAmount) || 0,
      lineWarehouseId: l.lineWarehouseId || this.warehouseId() || null,
      batchNumber: l.batchNumber || null,
      serialNumber: l.serialNumber || null,
      productionDate: l.productionDate || null,
      expiryDate: l.expiryDate || null,
      costCenterId: l.costCenterId || null,
      description: l.description || null
    }));
  }

  private recalcLine(line: DirectInvoiceLineDraft): DirectInvoiceLineDraft {
    const qty = Number(line.quantity) || 0;
    const unitPrice = Number(line.unitPrice) || 0;
    const discountPercent = Math.max(0, Math.min(100, Number(line.discountPercent) || 0));
    // VAT rate is fixed/read-only at 15%.
    const taxPercent = 15;

    const lineGross = qty * unitPrice;
    const discountAmount =
      discountPercent > 0
        ? Math.round(((lineGross * discountPercent) / 100) * 10000) / 10000
        : Math.max(0, Number(line.discountAmount) || 0);

    const lineNet = Math.max(0, lineGross - discountAmount);
    const taxAmount = Math.round(((lineNet * taxPercent) / 100) * 10000) / 10000;
    const lineTotal = lineNet + taxAmount;

    return {
      ...line,
      discountPercent,
      discountAmount,
      taxPercent,
      taxAmount,
      lineNet,
      lineTotal,
      remainingToReturn: Math.max(0, qty - (Number(line.returnedQuantity) || 0))
    };
  }

  private mapStatus(status: string | number): string {
    if (typeof status === 'string' && isNaN(Number(status))) return status;
    switch (Number(status)) {
      case 0:
        return 'Draft';
      case 1:
        return 'Approved';
      case 2:
        return 'Posted';
      case 8:
        return 'Reversed';
      case 9:
        return 'Cancelled';
      default:
        return 'Draft';
    }
  }

  private runAction(
    action: () => import('rxjs').Observable<void>,
    feedback?: { successTitle: string; successMessage: string; failedTitle: string }
  ): void {
    this.saving.set(true);
    this.error.set(null);
    action().subscribe({
      next: () => {
        this.saving.set(false);
        if (this.docId()) this.loadDoc(this.docId()!);
        if (feedback) {
          this.openResult(true, feedback.successTitle, feedback.successMessage);
        }
      },
      error: err => {
        const reason = this.extractError(err, this.t('pur.dpi.actionFailed'));
        this.error.set(reason);
        this.saving.set(false);
        if (feedback) {
          this.openResult(false, feedback.failedTitle, reason);
        }
      }
    });
  }

  private openResult(
    success: boolean,
    title: string,
    message: string,
    navigateToId?: string | null
  ): void {
    this.resultDialog.set({ open: true, success, title, message, navigateToId: navigateToId ?? null });
  }

  private extractError(err: unknown, fallback: string): string {
    const body = (err as { error?: { error?: string; message?: string }; message?: string })?.error;
    if (body && typeof body === 'object') {
      if (typeof body.error === 'string' && body.error.trim()) return body.error.trim();
      if (typeof body.message === 'string' && body.message.trim()) return body.message.trim();
    }
    const msg = (err as { message?: string })?.message;
    if (typeof msg === 'string' && msg.trim() && !msg.startsWith('Http failure')) return msg.trim();
    return fallback;
  }

  private todayIso(): string {
    return new Date().toISOString().slice(0, 10);
  }

  private toDateInput(value: string): string {
    return value.slice(0, 10);
  }

  private toDateOnly(dateOnly: string): string {
    return dateOnly.slice(0, 10);
  }
}
