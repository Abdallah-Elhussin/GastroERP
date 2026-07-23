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
import { PurchaseInvoiceRepository } from '../../../core/repositories/purchase-invoice.repository';
import { GoodsReceiptRepository } from '../../../core/repositories/goods-receipt.repository';
import {
  CreatePurchaseInvoicePayload,
  PurchaseInvoiceDoc,
  UpdatePurchaseInvoicePayload
} from '../../../core/models/purchase-invoice.models';
import { GoodsReceiptDoc, GoodsReceiptLine } from '../../../core/models/goods-receipt.models';
import { InventoryPageShellComponent } from '../../inventory/shared/inventory-page-shell.component';
import { AppDialogComponent } from '../../../shared/ui/app-dialog/app-dialog.component';

/** PurchaseInvoiceKind.FromReceipt */
const FROM_RECEIPT_KIND = 1;
/** GoodsReceiptStatus.Posted */
const GRN_POSTED = 2;

type ResultDialogState = {
  open: boolean;
  success: boolean;
  title: string;
  message: string;
  navigateToId?: string | null;
};

interface InvoiceLineView {
  inventoryItemId: string;
  itemNameAr?: string | null;
  unitId: string;
  unitNameAr?: string | null;
  quantity: number;
  unitPrice: number;
  taxAmount: number;
  lineTotal: number;
  goodsReceiptLineId?: string | null;
  remainingToInvoice?: number;
}

@Component({
  selector: 'app-purchase-invoice-form-page',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, InventoryPageShellComponent, AppDialogComponent],
  templateUrl: './purchase-invoice-form.page.html',
  styleUrl: './purchase-invoice-form.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PurchaseInvoiceFormPage implements OnInit {
  private repo = inject(PurchaseInvoiceRepository);
  private grnRepo = inject(GoodsReceiptRepository);
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
  paymentMode = signal(1);
  supplierId = signal<string | null>(null);
  supplierNameAr = signal('');
  warehouseId = signal<string | null>(null);
  warehouseNameAr = signal('');
  goodsReceiptId = signal<string | null>(null);
  goodsReceiptNumber = signal('');
  purchaseOrderId = signal<string | null>(null);
  currency = signal('SAR');
  exchangeRate = signal(1);
  supplierInvoiceNumber = signal('');
  notes = signal('');

  lines = signal<InvoiceLineView[]>([]);
  availableReceipts = signal<GoodsReceiptDoc[]>([]);

  breadcrumbs = [
    { labelKey: 'nav.purchases', path: '/purchases/dashboard' },
    { labelKey: 'pur.nav.purchaseInvoices', path: '/purchases/purchase-invoices' },
    { labelKey: 'pur.pi.formBreadcrumb' }
  ];

  canManage = computed(() => this.auth.hasPermission('Inventory.Manage'));
  isDraft = computed(() => this.status() === 'Draft');
  isApproved = computed(() => this.status() === 'Approved');
  isPosted = computed(() => this.status() === 'Posted');
  isNew = computed(() => !this.docId());
  pageTitle = computed(() =>
    this.isNew()
      ? this.t('pur.pi.createTitle')
      : `${this.t('pur.pi.docTitle')} — ${this.invoiceNumber()}`
  );
  statusLabel = computed(() => {
    switch (this.status()) {
      case 'Approved':
        return this.t('pur.pi.status.approved');
      case 'Posted':
        return this.t('pur.pi.status.posted');
      case 'Reversed':
        return this.t('pur.pi.status.reversed');
      case 'Cancelled':
        return this.t('pur.pi.status.cancelled');
      default:
        return this.t('pur.pi.status.draft');
    }
  });
  subTotal = computed(() =>
    this.lines().reduce((s, l) => s + Number(l.quantity) * Number(l.unitPrice), 0)
  );
  taxTotal = computed(() => this.lines().reduce((s, l) => s + (Number(l.taxAmount) || 0), 0));
  grandTotal = computed(() => this.subTotal() + this.taxTotal());

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.docId.set(id);
      this.loadDoc(id);
      return;
    }

    this.loadNextNumber();
    this.loadAvailableReceipts();

    const grnId = this.route.snapshot.queryParamMap.get('goodsReceiptId');
    if (grnId) {
      this.goodsReceiptId.set(grnId);
      this.loadFromReceipt(grnId);
    }
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  onReceiptChange(grnId: string | null): void {
    this.goodsReceiptId.set(grnId);
    if (!grnId || !this.isDraft() || this.docId()) return;
    this.loadFromReceipt(grnId);
  }

  save(): void {
    if (!this.canManage() || !this.isDraft()) return;
    if (!this.goodsReceiptId()) {
      this.error.set(this.t('pur.pi.validation.grn'));
      return;
    }
    if (!this.supplierId()) {
      this.error.set(this.t('pur.pi.validation.supplier'));
      return;
    }
    if (this.lines().length === 0) {
      this.error.set(this.t('pur.pi.validation.lines'));
      return;
    }

    this.saving.set(true);
    this.error.set(null);

    if (this.docId()) {
      const payload: UpdatePurchaseInvoicePayload = {
        invoiceDate: this.toDateOnly(this.invoiceDate()),
        paymentMode: this.paymentMode(),
        dueDate: this.dueDate() ? this.toDateOnly(this.dueDate()!) : null,
        supplierInvoiceNumber: this.supplierInvoiceNumber() || null,
        notes: this.notes() || null,
        exchangeRate: this.exchangeRate(),
        currency: this.currency(),
        warehouseId: this.warehouseId(),
        lines: this.lines().map(l => ({
          inventoryItemId: l.inventoryItemId,
          unitId: l.unitId,
          quantity: Number(l.quantity) || 0,
          unitPrice: Number(l.unitPrice) || 0,
          taxAmount: Number(l.taxAmount) || 0,
          goodsReceiptLineId: l.goodsReceiptLineId || null
        }))
      };
      this.repo.update(this.docId()!, payload).subscribe({
        next: doc => {
          this.applyDoc(doc);
          this.saving.set(false);
          this.openResult(true, this.t('pur.pi.result.savedTitle'), this.t('pur.pi.result.savedMessage'));
        },
        error: err => {
          const reason = this.extractError(err, this.t('pur.pi.saveFailed'));
          this.error.set(reason);
          this.saving.set(false);
          this.openResult(false, this.t('pur.pi.result.failedTitle'), reason);
        }
      });
      return;
    }

    const createPayload: CreatePurchaseInvoicePayload = {
      kind: FROM_RECEIPT_KIND,
      paymentMode: this.paymentMode(),
      supplierId: this.supplierId()!,
      invoiceDate: this.toDateOnly(this.invoiceDate()),
      invoiceNumber: this.invoiceNumber() || null,
      currency: this.currency(),
      warehouseId: this.warehouseId(),
      purchaseOrderId: this.purchaseOrderId(),
      goodsReceiptId: this.goodsReceiptId(),
      dueDate: this.dueDate() ? this.toDateOnly(this.dueDate()!) : null,
      supplierInvoiceNumber: this.supplierInvoiceNumber() || null,
      notes: this.notes() || null,
      exchangeRate: this.exchangeRate(),
      lines: []
    };

    this.repo.create(createPayload).subscribe({
      next: doc => {
        this.saving.set(false);
        this.openResult(
          true,
          this.t('pur.pi.result.savedTitle'),
          this.t('pur.pi.result.savedMessage'),
          doc.id
        );
      },
      error: err => {
        const reason = this.extractError(err, this.t('pur.pi.saveFailed'));
        this.error.set(reason);
        this.saving.set(false);
        this.openResult(false, this.t('pur.pi.result.failedTitle'), reason);
      }
    });
  }

  approve(): void {
    const id = this.docId();
    if (!id || !this.canManage() || !this.isDraft()) return;
    this.runAction(
      () => this.repo.approve(id),
      {
        successTitle: this.t('pur.pi.result.approveSuccessTitle'),
        successMessage: this.t('pur.pi.result.approveSuccessMessage'),
        failedTitle: this.t('pur.pi.result.approveFailedTitle')
      }
    );
  }

  post(): void {
    const id = this.docId();
    if (!id || !this.canManage() || !this.isApproved()) return;
    this.runAction(
      () => this.repo.post(id),
      {
        successTitle: this.t('pur.pi.result.postSuccessTitle'),
        successMessage: this.t('pur.pi.result.postSuccessMessage').replace(
          '{number}',
          this.invoiceNumber() || id
        ),
        failedTitle: this.t('pur.pi.result.postFailedTitle')
      }
    );
  }

  unpost(): void {
    const id = this.docId();
    if (!id || !this.canManage()) return;
    this.runAction(() => this.repo.unpost(id));
  }

  createReturn(): void {
    const id = this.docId();
    if (!id || !this.isPosted()) return;
    void this.router.navigate(['/purchases/invoice-returns/new'], {
      queryParams: { purchaseInvoiceId: id }
    });
  }

  acknowledgeResult(): void {
    const navId = this.resultDialog().navigateToId;
    this.resultDialog.set({ open: false, success: true, title: '', message: '' });
    if (navId && navId !== this.docId()) {
      void this.router.navigate(['/purchases/purchase-invoices', navId], { replaceUrl: true });
    }
  }

  back(): void {
    void this.router.navigate(['/purchases/purchase-invoices']);
  }

  private loadNextNumber(): void {
    this.repo.getNextNumber(1).pipe(catchError(() => of(''))).subscribe(n => {
      if (n) this.invoiceNumber.set(n);
    });
  }

  private loadAvailableReceipts(): void {
    this.grnRepo
      .getList({ page: 1, pageSize: 100, status: GRN_POSTED })
      .pipe(catchError(() => of([] as GoodsReceiptDoc[])))
      .subscribe(rows => {
        this.availableReceipts.set(rows.filter(r => !r.isInvoiced));
      });
  }

  private loadFromReceipt(grnId: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.grnRepo.getById(grnId).subscribe({
      next: grn => {
        if (this.statusNameOfGrn(grn) !== 'Posted') {
          this.error.set(this.t('pur.pi.validation.grnMustBePosted'));
          this.loading.set(false);
          return;
        }
        this.applyReceipt(grn);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('pur.pi.loadFailed'));
        this.loading.set(false);
      }
    });
  }

  private applyReceipt(grn: GoodsReceiptDoc): void {
    this.goodsReceiptId.set(grn.id);
    this.goodsReceiptNumber.set(grn.grnNumber);
    this.supplierId.set(grn.supplierId);
    this.supplierNameAr.set(grn.supplierNameAr);
    this.warehouseId.set(grn.warehouseId);
    this.warehouseNameAr.set(grn.warehouseNameAr);
    this.purchaseOrderId.set(grn.purchaseOrderId ?? null);
    this.currency.set(grn.currency || 'SAR');
    this.exchangeRate.set(grn.exchangeRate || 1);

    const invoiceLines = (grn.lines || [])
      .map(l => {
        const qty = remainingToInvoice(l);
        if (qty <= 0) return null;
        const unitPrice = Number(l.unitCost) || 0;
        const taxAmount = Number(l.taxAmount) || 0;
        return {
          inventoryItemId: l.inventoryItemId,
          itemNameAr: l.itemNameAr,
          unitId: l.unitId,
          unitNameAr: l.unitNameAr,
          quantity: qty,
          unitPrice,
          taxAmount,
          lineTotal: qty * unitPrice + taxAmount,
          goodsReceiptLineId: l.id ?? null,
          remainingToInvoice: qty
        } as InvoiceLineView;
      })
      .filter((x): x is InvoiceLineView => x != null);

    this.lines.set(invoiceLines);
    if (invoiceLines.length === 0) {
      this.error.set(this.t('pur.pi.validation.nothingToInvoice'));
    }
  }

  private loadDoc(id: string): void {
    this.loading.set(true);
    this.repo.getById(id).subscribe({
      next: doc => {
        this.applyDoc(doc);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('pur.pi.loadFailed'));
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
    this.paymentMode.set(Number(doc.paymentMode) || 1);
    this.supplierId.set(doc.supplierId);
    this.warehouseId.set(doc.warehouseId ?? null);
    this.goodsReceiptId.set(doc.goodsReceiptId ?? null);
    this.purchaseOrderId.set(doc.purchaseOrderId ?? null);
    this.currency.set(doc.currency || 'SAR');
    this.exchangeRate.set(doc.exchangeRate || 1);
    this.supplierInvoiceNumber.set(doc.supplierInvoiceNumber || '');
    this.notes.set(doc.notes || '');
    this.lines.set(
      (doc.lines || []).map(l => ({
        inventoryItemId: l.inventoryItemId,
        itemNameAr: l.itemNameAr,
        unitId: l.unitId,
        unitNameAr: l.unitNameAr,
        quantity: l.quantity,
        unitPrice: l.unitPrice,
        taxAmount: l.taxAmount,
        lineTotal: l.lineTotal,
        goodsReceiptLineId: l.goodsReceiptLineId
      }))
    );

    if (doc.goodsReceiptId) {
      this.grnRepo
        .getById(doc.goodsReceiptId)
        .pipe(catchError(() => of(null)))
        .subscribe(grn => {
          if (!grn) return;
          this.goodsReceiptNumber.set(grn.grnNumber);
          this.supplierNameAr.set(grn.supplierNameAr);
          this.warehouseNameAr.set(grn.warehouseNameAr);
        });
    }
  }

  private mapStatus(status: string | number): string {
    if (typeof status === 'string' && isNaN(Number(status))) return status;
    switch (Number(status)) {
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

  private statusNameOfGrn(grn: GoodsReceiptDoc): string {
    if (typeof grn.status === 'string' && isNaN(Number(grn.status))) return grn.status;
    const code = typeof grn.status === 'number' ? grn.status : grn.unifiedStatusCode;
    switch (Number(code)) {
      case 2:
        return 'Posted';
      case 4:
        return 'Approved';
      case 1:
        return 'Draft';
      default:
        return grn.unifiedStatusCode === 2 ? 'Posted' : 'Draft';
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
        if (feedback) this.openResult(true, feedback.successTitle, feedback.successMessage);
      },
      error: err => {
        const reason = this.extractError(err, this.t('pur.pi.actionFailed'));
        this.error.set(reason);
        this.saving.set(false);
        if (feedback) this.openResult(false, feedback.failedTitle, reason);
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
    const body = (err as { error?: { error?: string; message?: string } })?.error;
    if (body && typeof body === 'object') {
      if (typeof body.error === 'string' && body.error.trim()) return body.error.trim();
      if (typeof body.message === 'string' && body.message.trim()) return body.message.trim();
    }
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

function remainingToInvoice(line: GoodsReceiptLine): number {
  if (typeof line.remainingQuantity === 'number' && line.invoicedQuantity != null) {
    return Math.max(0, Number(line.acceptedQuantity) - Number(line.invoicedQuantity));
  }
  const accepted = Number(line.acceptedQuantity) || 0;
  const invoiced = Number(line.invoicedQuantity) || 0;
  return Math.max(0, accepted - invoiced);
}
