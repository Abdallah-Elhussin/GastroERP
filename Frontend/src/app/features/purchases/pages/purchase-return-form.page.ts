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
import { HttpClient, HttpParams } from '@angular/common/http';
import { MatIconModule } from '@angular/material/icon';
import { catchError, of } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { InventoryRepository } from '../../../core/repositories/inventory.repository';
import { GoodsReceiptRepository } from '../../../core/repositories/goods-receipt.repository';
import { PurchaseReturnRepository } from '../../../core/repositories/purchase-return.repository';
import {
  CreatePurchaseReturnPayload,
  PurchaseInvoiceForReturn,
  PurchaseReturnDoc,
  PurchaseReturnLine,
  PurchaseReturnReason,
  UpdatePurchaseReturnPayload
} from '../../../core/models/purchase-return.models';
import { GoodsReceiptDoc } from '../../../core/models/goods-receipt.models';
import { Warehouse } from '../../../core/models/inventory.models';
import { InventoryPageShellComponent } from '../../inventory/shared/inventory-page-shell.component';
import { AppDialogComponent } from '../../../shared/ui/app-dialog/app-dialog.component';

interface InvoiceOption {
  id: string;
  invoiceNumber: string;
  kind: number;
  status: number;
  supplierId: string;
  warehouseId?: string | null;
  totalAmount: number;
}

interface ReturnLineView extends PurchaseReturnLine {
  lineWarehouseId?: string | null;
  lineWarehouseNameAr?: string | null;
  isDisabled?: boolean;
  discountPercent?: number;
}

type ResultDialogState = {
  open: boolean;
  success: boolean;
  title: string;
  message: string;
  navigateToId?: string | null;
};

function previewReturnNumber(sequence = 1): string {
  const now = new Date();
  const period = `${now.getFullYear()}${String(now.getMonth() + 1).padStart(2, '0')}`;
  return `PR${period}${String(Math.max(1, sequence)).padStart(4, '0')}`;
}

@Component({
  selector: 'app-purchase-return-form-page',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, InventoryPageShellComponent, AppDialogComponent],
  templateUrl: './purchase-return-form.page.html',
  styleUrl: './purchase-return-form.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PurchaseReturnFormPage implements OnInit {
  private repo = inject(PurchaseReturnRepository);
  private grnRepo = inject(GoodsReceiptRepository);
  private inventoryRepo = inject(InventoryRepository);
  private http = inject(HttpClient);
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
  approveConfirmOpen = signal(false);

  docId = signal<string | null>(null);
  returnNumber = signal('');
  returnDate = signal(this.todayIso());
  status = signal('Draft');
  unifiedStatusCode = signal(0);
  returnType = signal<number>(1);
  currency = signal('SAR');
  notes = signal('');
  referenceNumber = signal('');
  returnReasonId = signal<string | null>(null);
  warehouseId = signal<string | null>(null);
  warehouseNameAr = signal('');
  warehouseLocked = signal(false);
  supplierId = signal<string | null>(null);
  supplierNameAr = signal('');
  goodsReceiptId = signal<string | null>(null);
  goodsReceiptNumber = signal('');
  purchaseInvoiceId = signal<string | null>(null);
  purchaseInvoiceNumber = signal('');

  lines = signal<ReturnLineView[]>([]);
  selectedLineIndex = signal<number | null>(null);
  sourceLoaded = signal(false);

  reasons = signal<PurchaseReturnReason[]>([]);
  warehouses = signal<Warehouse[]>([]);
  goodsReceipts = signal<GoodsReceiptDoc[]>([]);
  invoices = signal<InvoiceOption[]>([]);

  isDirectRoute = signal(false);
  /** Invoice returns screen: FromReceipt + Direct purchase invoices. */
  isInvoiceReturnsMode = signal(false);

  breadcrumbs = computed(() => {
    const invoiceMode = this.isInvoiceReturnsMode();
    const listKey = invoiceMode ? 'pur.nav.invoiceReturns' : 'pur.nav.purchaseReturns';
    const listPath = invoiceMode ? '/purchases/invoice-returns' : '/purchases/purchase-returns';
    return [
      { labelKey: 'nav.purchases', path: '/purchases/dashboard' },
      { labelKey: listKey, path: listPath },
      { labelKey: 'pur.pr.formBreadcrumb' }
    ];
  });

  canManage = computed(() =>
    this.auth.hasPermission('Inventory.Manage') || this.auth.hasPermission('Purchases.Manage')
  );
  /** Editable draft only — after approval the document is locked. */
  isEditableDraft = computed(() => this.status() === 'Draft');
  isApproved = computed(() => this.status() === 'Approved');
  isPosted = computed(() => this.status() === 'Posted');
  canApprove = computed(() => !!this.docId() && this.status() === 'Draft');
  canPost = computed(() => !!this.docId() && this.status() === 'Approved');
  canCancel = computed(() => !!this.docId() && this.status() === 'Draft');
  isNew = computed(() => !this.docId());
  pageTitle = computed(() =>
    this.isNew()
      ? this.t('pur.pr.createTitle')
      : `${this.t('pur.pr.docTitle')} — ${this.returnNumber()}`
  );
  statusLabel = computed(() => {
    switch (this.status()) {
      case 'Approved':
        return this.t('pur.pr.status.approved');
      case 'Posted':
        return this.t('pur.pr.status.posted');
      case 'Reversed':
        return this.t('pur.pr.status.reversed');
      case 'Cancelled':
        return this.t('pur.pr.status.cancelled');
      default:
        return this.t('pur.pr.status.draft');
    }
  });
  showCreditNote = computed(() => this.returnType() === 2 || this.returnType() === 3);
  usesGrnSource = computed(() => this.returnType() === 1);
  usesInvoiceSource = computed(() => this.returnType() === 2 || this.returnType() === 3);
  hasInvoiceSource = computed(() => !!this.purchaseInvoiceId());
  linesEnabled = computed(() =>
    this.usesGrnSource() ? !!this.goodsReceiptId() : this.hasInvoiceSource() && this.sourceLoaded()
  );
  canSaveDraft = computed(() => {
    if (!this.canManage() || !this.isEditableDraft() || this.saving()) return false;
    if (this.usesInvoiceSource() && !this.purchaseInvoiceId()) return false;
    if (this.usesGrnSource() && !this.goodsReceiptId()) return false;
    return true;
  });
  grandTotal = computed(() => this.lines().reduce((s, l) => s + (Number(l.lineTotal) || 0), 0));
  listBasePath = computed(() =>
    this.isInvoiceReturnsMode() ? '/purchases/invoice-returns' : '/purchases/purchase-returns'
  );
  warehouseDisplayName = computed(() => {
    const name = this.warehouseNameAr();
    if (name) return name;
    const id = this.warehouseId();
    const match = this.warehouses().find(w => w.id === id);
    return match ? this.warehouseLabel(match) : '—';
  });

  ngOnInit(): void {
    const path = this.route.snapshot.routeConfig?.path ?? '';
    const parentPath = this.route.parent?.snapshot.routeConfig?.path ?? '';
    const url = this.router.url;
    const data = this.route.snapshot.data;
    const invoiceMode =
      data['invoiceReturnsMode'] === true ||
      url.includes('/invoice-returns') ||
      url.includes('/direct-returns') ||
      path.startsWith('invoice-returns') ||
      path.startsWith('direct-returns') ||
      parentPath.startsWith('invoice-returns') ||
      parentPath.startsWith('direct-returns');
    this.isInvoiceReturnsMode.set(invoiceMode);
    this.isDirectRoute.set(invoiceMode); // keeps template "locked type" behavior

    const qpType = Number(this.route.snapshot.queryParamMap.get('returnType'));
    if (invoiceMode) {
      // Default AfterInvoice until a Direct invoice is selected.
      this.returnType.set(qpType === 3 ? 3 : 2);
    } else if (qpType === 1 || qpType === 2 || qpType === 3) {
      this.returnType.set(qpType);
    }

    this.loadWarehouses();
    this.loadReasons();
    this.loadSourceOptions();

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.docId.set(id);
      this.loadDoc(id);
    } else {
      this.loadNextReturnNumber();
    }

    const qpInvoiceId = this.route.snapshot.queryParamMap.get('purchaseInvoiceId');
    if (qpInvoiceId && !id) {
      this.purchaseInvoiceId.set(qpInvoiceId);
      if (!invoiceMode) this.returnType.set(2);
      this.loadSourceOptions();
      this.loadFromInvoice();
    }
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  warehouseLabel(w: Warehouse): string {
    const code = (w.code || '').trim();
    return code ? `${code} — ${w.nameAr}` : w.nameAr;
  }

  onWarehouseChange(id: string | null): void {
    this.warehouseId.set(id);
    const match = this.warehouses().find(w => w.id === id);
    this.warehouseNameAr.set(match?.nameAr || '');
  }

  onReturnTypeChange(value: number): void {
    if (!this.isEditableDraft() || this.docId()) return;
    this.returnType.set(Number(value));
    this.goodsReceiptId.set(null);
    this.goodsReceiptNumber.set('');
    this.purchaseInvoiceId.set(null);
    this.purchaseInvoiceNumber.set('');
    this.supplierId.set(null);
    this.supplierNameAr.set('');
    this.warehouseId.set(null);
    this.warehouseNameAr.set('');
    this.warehouseLocked.set(false);
    this.lines.set([]);
    this.sourceLoaded.set(false);
    this.error.set(null);
    this.loadSourceOptions();
  }

  onInvoiceChange(invoiceId: string | null): void {
    if (!this.isEditableDraft() || this.docId()) return;
    this.purchaseInvoiceId.set(invoiceId);
    this.lines.set([]);
    this.sourceLoaded.set(false);
    this.supplierId.set(null);
    this.supplierNameAr.set('');
    this.warehouseId.set(null);
    this.warehouseNameAr.set('');
    this.warehouseLocked.set(false);
    this.purchaseInvoiceNumber.set('');
    this.error.set(null);
    if (!invoiceId) {
      this.error.set(this.t('pur.pr.validation.source'));
      return;
    }
    this.loadFromInvoice();
  }

  loadFromSource(): void {
    if (this.usesGrnSource()) {
      this.loadFromGrn();
      return;
    }
    this.loadFromInvoice();
  }

  loadFromGrn(): void {
    const grnId = this.goodsReceiptId();
    if (!grnId) {
      this.error.set(this.t('pur.pr.validation.source'));
      return;
    }
    this.loading.set(true);
    this.error.set(null);
    this.repo.previewFromGrn(grnId).subscribe({
      next: preview => {
        this.applyPreview(preview);
        this.sourceLoaded.set(true);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('pur.pr.loadFailed'));
        this.sourceLoaded.set(false);
        this.loading.set(false);
      }
    });
  }

  loadFromInvoice(): void {
    const invId = this.purchaseInvoiceId();
    if (!invId) {
      this.error.set(this.t('pur.pr.validation.source'));
      return;
    }
    this.loading.set(true);
    this.error.set(null);
    this.repo.getInvoiceForReturn(invId).subscribe({
      next: data => {
        this.applyInvoiceForReturn(data);
        this.loading.set(false);
      },
      error: err => {
        const msg =
          err?.error?.error ??
          err?.error?.message ??
          this.t('pur.pr.loadFailed');
        this.error.set(msg);
        this.lines.set([]);
        this.sourceLoaded.set(false);
        this.loading.set(false);
      }
    });
  }

  save(): void {
    if (!this.canSaveDraft()) {
      if (this.usesInvoiceSource() && !this.purchaseInvoiceId()) {
        this.error.set(this.t('pur.pr.validation.source'));
      }
      return;
    }
    if (!this.warehouseId()) {
      this.error.set(this.t('pur.pr.validation.warehouse'));
      return;
    }
    if (this.usesGrnSource() && !this.goodsReceiptId()) {
      this.error.set(this.t('pur.pr.validation.source'));
      return;
    }
    if (this.usesInvoiceSource() && !this.purchaseInvoiceId()) {
      this.error.set(this.t('pur.pr.validation.source'));
      return;
    }
    if (!this.returnReasonId()) {
      this.error.set(this.t('pur.pr.validation.reason'));
      return;
    }
    const activeLines = this.lines().filter(l => !l.isDisabled && Number(l.returnQuantity) > 0);
    if (activeLines.length === 0) {
      this.error.set(this.t('pur.pr.validation.lines'));
      return;
    }
    const over = activeLines.find(l => Number(l.returnQuantity) > Number(l.availableToReturn));
    if (over) {
      this.error.set(
        this.t('pur.pr.validation.qtyExceeds').replace('{max}', String(over.availableToReturn))
      );
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    const linePayload = activeLines.map(l => ({
      inventoryItemId: l.inventoryItemId,
      unitId: l.unitId,
      originalQuantity: Number(l.originalQuantity) || 0,
      previouslyReturnedQuantity: Number(l.previouslyReturnedQuantity) || 0,
      returnQuantity: Number(l.returnQuantity) || 0,
      unitCost: Number(l.unitCost) || 0,
      discountAmount: Number(l.discountAmount) || 0,
      taxPercent: Number(l.taxPercent) || 0,
      taxAmount: Number(l.taxAmount) || 0,
      goodsReceiptLineId: l.goodsReceiptLineId || null,
      purchaseInvoiceLineId: l.purchaseInvoiceLineId || null,
      batchNumber: l.batchNumber || null,
      expiryDate: l.expiryDate || null,
      lineReason: l.lineReason || null,
      notes: l.notes || null,
      productTemperature: l.productTemperature ?? null,
      destroyItem: !!l.destroyItem
    }));

    if (this.docId()) {
      const payload: UpdatePurchaseReturnPayload = {
        returnDate: this.toIsoDate(this.returnDate()),
        returnReasonId: this.returnReasonId(),
        reasonNotes: null,
        referenceNumber: this.referenceNumber() || null,
        notes: this.notes() || null,
        lines: linePayload
      };
      this.repo.update(this.docId()!, payload).subscribe({
        next: doc => {
          this.applyDoc(doc);
          this.saving.set(false);
          this.error.set(null);
        },
        error: err => {
          this.saving.set(false);
          this.error.set(err?.error?.error ?? this.t('pur.pr.saveFailed'));
        }
      });
      return;
    }

    const createPayload: CreatePurchaseReturnPayload = {
      returnType: this.returnType(),
      warehouseId: this.warehouseId()!,
      returnNumber: this.returnNumber() || null,
      goodsReceiptId: this.usesGrnSource() ? this.goodsReceiptId() : null,
      purchaseInvoiceId: this.usesInvoiceSource() ? this.purchaseInvoiceId() : null,
      returnDate: this.toIsoDate(this.returnDate()),
      returnReasonId: this.returnReasonId(),
      reasonNotes: null,
      referenceNumber: this.referenceNumber() || null,
      notes: this.notes() || null,
      currency: this.currency(),
      lines: linePayload
    };

    this.repo.create(createPayload).subscribe({
      next: doc => {
        this.saving.set(false);
        this.error.set(null);
        void this.router.navigate([this.listBasePath(), doc.id], { replaceUrl: true });
      },
      error: err => {
        this.saving.set(false);
        this.error.set(err?.error?.error ?? this.t('pur.pr.saveFailed'));
      }
    });
  }

  post(): void {
    const id = this.docId();
    if (!id || !this.canManage() || !this.canPost()) return;
    this.runAction(
      () => this.repo.post(id),
      {
        successTitle: this.t('pur.pr.result.postSuccessTitle'),
        successMessage: this.t('pur.pr.result.postSuccessMessage').replace(
          '{number}',
          this.returnNumber() || id
        )
      }
    );
  }

  openApproveConfirm(): void {
    if (!this.canApprove() || !this.canManage()) return;
    this.approveConfirmOpen.set(true);
  }

  closeApproveConfirm(): void {
    this.approveConfirmOpen.set(false);
  }

  confirmApprove(): void {
    const id = this.docId();
    if (!id || !this.canManage()) return;
    this.approveConfirmOpen.set(false);
    this.runAction(
      () => this.repo.approve(id),
      {
        successTitle: this.t('pur.pr.result.approveSuccessTitle'),
        successMessage: this.t('pur.pr.result.approveSuccessMessage')
      }
    );
  }

  cancelDoc(): void {
    const id = this.docId();
    if (!id || !this.canManage() || !this.canCancel()) return;
    if (!confirm(this.t('pur.pr.confirmCancel'))) return;
    this.runAction(() => this.repo.cancel(id));
  }

  acknowledgeResult(): void {
    const navId = this.resultDialog().navigateToId;
    this.resultDialog.set({ open: false, success: true, title: '', message: '' });
    if (navId && navId !== this.docId()) {
      void this.router.navigate([this.listBasePath(), navId], { replaceUrl: true });
    }
  }

  back(): void {
    void this.router.navigate([this.listBasePath()]);
  }

  selectLine(index: number): void {
    this.selectedLineIndex.set(index);
  }

  updateLine(index: number, patch: Partial<ReturnLineView>): void {
    if (!this.isEditableDraft()) return;
    const next = [...this.lines()];
    const row = { ...next[index], ...patch };
    if (row.isDisabled) return;

    let qty = Number(row.returnQuantity) || 0;
    if (qty < 0) qty = 0;
    const available = Number(row.availableToReturn) || 0;
    if (qty > available) {
      this.error.set(
        this.t('pur.pr.validation.qtyExceeds').replace('{max}', String(available))
      );
      qty = available;
    } else if (this.error()?.includes('كمية المرتجع') || this.error()?.includes('exceed')) {
      this.error.set(null);
    }
    row.returnQuantity = qty;

    const cost = Number(row.unitCost) || 0;
    const discountPercent = Number(row.discountPercent) || 0;
    const origQty = Number(row.originalQuantity) || 0;
    const origDiscount = Number(row.discountAmount) || 0;
    const discount =
      discountPercent > 0
        ? Math.round(((qty * cost * discountPercent) / 100) * 10000) / 10000
        : origQty > 0
          ? Math.round((origDiscount * (qty / origQty)) * 10000) / 10000
          : 0;
    row.discountAmount = discount;

    const taxPercent = Number(row.taxPercent) || 0;
    row.lineSubTotal = Math.max(0, qty * cost - discount);
    if (taxPercent > 0) {
      row.taxAmount = Math.round(((row.lineSubTotal * taxPercent) / 100) * 10000) / 10000;
    } else if (origQty > 0) {
      const baseTax = Number(next[index].taxAmount) || 0;
      const baseQty = Number(next[index].returnQuantity) || 0;
      row.taxAmount =
        baseQty > 0
          ? Math.round((baseTax * (qty / baseQty)) * 10000) / 10000
          : origQty > 0
            ? Math.round((baseTax * (qty / origQty)) * 10000) / 10000
            : 0;
    } else {
      row.taxAmount = 0;
    }
    row.lineTotal = row.lineSubTotal + (Number(row.taxAmount) || 0);
    next[index] = row;
    this.lines.set(next);
  }

  returnTypeLabel(): string {
    switch (this.returnType()) {
      case 1:
        return this.t('pur.pr.type.beforeInvoice');
      case 2:
        return this.t('pur.pr.type.afterInvoice');
      case 3:
        return this.t('pur.pr.type.direct');
      default:
        return '';
    }
  }

  invoiceOptionLabel(inv: InvoiceOption): string {
    const kindLabel =
      Number(inv.kind) === 2
        ? this.t('pur.pr.invoiceKind.direct')
        : this.t('pur.pr.invoiceKind.fromReceipt');
    return `${inv.invoiceNumber} — ${kindLabel}`;
  }

  private loadDoc(id: string): void {
    this.loading.set(true);
    this.repo.getById(id).subscribe({
      next: doc => {
        this.applyDoc(doc);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('pur.pr.loadFailed'));
        this.loading.set(false);
      }
    });
  }

  private loadNextReturnNumber(): void {
    this.returnNumber.set(previewReturnNumber(1));
    this.repo
      .getNextNumber()
      .pipe(catchError(() => of('')))
      .subscribe(n => {
        this.returnNumber.set(n && /^PR\d{10}$/.test(n) ? n : previewReturnNumber(1));
      });
  }

  private loadWarehouses(): void {
    this.inventoryRepo
      .getWarehouseLookup()
      .pipe(catchError(() => of([] as Warehouse[])))
      .subscribe(rows => {
        const usable = rows.filter(
          w =>
            w.isActive !== false &&
            (w.allowReceiving !== false || w.allowPurchase !== false)
        );
        const list = usable.length > 0 ? usable : rows.filter(w => w.isActive !== false);
        this.warehouses.set(list);

        if (!this.warehouseId()) {
          const preferred =
            list.find(w => w.isDefault && w.isActive) ?? list.find(w => w.isActive) ?? list[0] ?? null;
          if (preferred && !this.sourceLoaded()) {
            this.warehouseId.set(preferred.id);
            this.warehouseNameAr.set(preferred.nameAr || '');
          }
        } else {
          this.ensureWarehouseInList(this.warehouseId()!, this.warehouseNameAr());
        }
      });
  }

  private ensureWarehouseInList(id: string, nameAr?: string | null): void {
    if (!id) return;
    if (this.warehouses().some(w => w.id === id)) return;
    this.warehouses.update(list => [
      ...list,
      {
        id,
        code: '',
        nameAr: nameAr || id,
        nameEn: undefined,
        warehouseType: 'Main',
        allowPurchase: true,
        allowSales: false,
        allowTransfer: true,
        allowInventoryCount: true,
        allowManufacturing: false,
        allowNegativeStock: false,
        allowReservation: false,
        allowReceiving: true,
        allowIssue: true,
        allowAdjustment: true,
        isPosWarehouse: false,
        isDefault: false,
        isSystem: false,
        useBins: false,
        isActive: true,
        zoneCount: 0
      }
    ]);
  }

  private loadReasons(): void {
    this.repo
      .getReasons(true)
      .pipe(catchError(() => of([] as PurchaseReturnReason[])))
      .subscribe(r => {
        if (r.length > 0) {
          this.reasons.set(r);
          return;
        }
        this.repo
          .seedReasons()
          .pipe(catchError(() => of([] as PurchaseReturnReason[])))
          .subscribe(seeded => this.reasons.set(seeded));
      });
  }

  private loadSourceOptions(): void {
    if (this.usesGrnSource()) {
      this.grnRepo
        .getList({ page: 1, pageSize: 100, status: 2 })
        .pipe(catchError(() => of([] as GoodsReceiptDoc[])))
        .subscribe(rows => this.goodsReceipts.set(rows));
      return;
    }

    // Invoice returns mode: load both FromReceipt (kind=1) and Direct (kind=2).
    let params = new HttpParams().set('page', 1).set('pageSize', 100).set('status', 2);
    if (!this.isInvoiceReturnsMode()) {
      if (this.returnType() === 3) params = params.set('kind', 2);
      else if (this.returnType() === 2) params = params.set('kind', 1);
    }
    this.http
      .get<InvoiceOption[]>(`${environment.apiBaseUrl}/inventory/purchase-invoices`, { params })
      .pipe(catchError(() => of([] as InvoiceOption[])))
      .subscribe(rows => this.invoices.set(rows ?? []));
  }

  private applyInvoiceForReturn(data: PurchaseInvoiceForReturn): void {
    const h = data.header;
    if (!h.canCreateReturn) {
      const code = (h.blockReasonCode || '').toLowerCase();
      if (code.includes('cancel')) this.error.set(this.t('pur.pr.invoiceCancelled'));
      else if (code.includes('nothing') || code.includes('return'))
        this.error.set(this.t('pur.pr.nothingToReturn'));
      else       this.error.set(h.blockReason || this.t('pur.pr.invoiceBlocked'));
      this.lines.set([]);
      this.sourceLoaded.set(false);
      this.supplierId.set(h.supplierId);
      this.supplierNameAr.set(h.supplierNameAr || '');
      this.purchaseInvoiceNumber.set(h.invoiceNumber || '');
      this.returnType.set(Number(h.kind) === 2 ? 3 : 2);
      return;
    }

    this.error.set(null);
    this.supplierId.set(h.supplierId);
    this.supplierNameAr.set(h.supplierNameAr || '');
    this.purchaseInvoiceId.set(h.id);
    this.purchaseInvoiceNumber.set(h.invoiceNumber || '');
    // Kind: FromReceipt=1 → AfterInvoice=2; Direct=2 → Direct=3
    this.returnType.set(Number(h.kind) === 2 ? 3 : 2);
    this.currency.set(h.currency || 'SAR');
    if (h.warehouseId) {
      this.warehouseId.set(h.warehouseId);
      this.warehouseNameAr.set(h.warehouseNameAr || '');
      this.ensureWarehouseInList(h.warehouseId, h.warehouseNameAr);
      this.warehouseLocked.set(true);
    } else {
      this.warehouseLocked.set(false);
    }
    if (h.externalReference || h.supplierInvoiceNumber) {
      this.referenceNumber.set(h.externalReference || h.supplierInvoiceNumber || '');
    }
    if (h.notes) this.notes.set(h.notes);

    const mapped: ReturnLineView[] = (data.items || []).map(i => ({
      inventoryItemId: i.inventoryItemId,
      itemNameAr: i.itemNameAr,
      itemSku: i.itemSku,
      unitId: i.unitId,
      unitNameAr: i.unitNameAr,
      purchaseInvoiceLineId: i.purchaseInvoiceLineId,
      originalQuantity: i.originalQuantity,
      previouslyReturnedQuantity: i.previouslyReturnedQuantity,
      availableToReturn: i.remainingQuantity,
      returnQuantity: 0,
      unitCost: i.unitPrice,
      discountAmount: 0,
      discountPercent: i.discountPercent,
      taxPercent: i.taxPercent,
      taxAmount: 0,
      lineSubTotal: 0,
      lineTotal: 0,
      notes: i.description,
      destroyItem: false,
      lineWarehouseId: i.warehouseId,
      lineWarehouseNameAr: i.warehouseNameAr,
      isDisabled: i.isDisabled || i.remainingQuantity <= 0
    }));

    this.lines.set(mapped);
    this.sourceLoaded.set(true);
  }

  private applyPreview(doc: PurchaseReturnDoc): void {
    this.returnType.set(Number(doc.returnType) || this.returnType());
    this.supplierId.set(doc.supplierId);
    this.supplierNameAr.set(doc.supplierNameAr || '');
    this.warehouseId.set(doc.warehouseId);
    this.warehouseNameAr.set(doc.warehouseNameAr || '');
    this.ensureWarehouseInList(doc.warehouseId, doc.warehouseNameAr);
    this.warehouseLocked.set(!!doc.warehouseId);
    this.goodsReceiptId.set(doc.goodsReceiptId ?? null);
    this.goodsReceiptNumber.set(doc.goodsReceiptNumber || '');
    this.purchaseInvoiceId.set(doc.purchaseInvoiceId ?? null);
    this.purchaseInvoiceNumber.set(doc.purchaseInvoiceNumber || '');
    this.currency.set(doc.currency || 'SAR');
    if (doc.notes) this.notes.set(doc.notes);
    this.lines.set(
      (doc.lines || []).map(l => ({
        ...l,
        returnQuantity: Number(l.returnQuantity) || 0,
        isDisabled: Number(l.availableToReturn) <= 0
      }))
    );
    this.sourceLoaded.set(true);
  }

  private applyDoc(doc: PurchaseReturnDoc): void {
    this.docId.set(doc.id);
    this.returnNumber.set(doc.returnNumber);
    this.returnDate.set(this.toDateInput(doc.returnDate));
    this.status.set(this.mapStatus(doc.status, doc.unifiedStatusCode));
    this.unifiedStatusCode.set(doc.unifiedStatusCode);
    this.returnType.set(Number(doc.returnType) || 1);
    this.currency.set(doc.currency || 'SAR');
    this.notes.set(doc.notes || '');
    this.referenceNumber.set(doc.referenceNumber || '');
    this.returnReasonId.set(doc.returnReasonId ?? null);
    this.warehouseId.set(doc.warehouseId);
    this.warehouseNameAr.set(doc.warehouseNameAr || '');
    this.ensureWarehouseInList(doc.warehouseId, doc.warehouseNameAr);
    this.warehouseLocked.set(true);
    this.supplierId.set(doc.supplierId);
    this.supplierNameAr.set(doc.supplierNameAr || '');
    this.goodsReceiptId.set(doc.goodsReceiptId ?? null);
    this.goodsReceiptNumber.set(doc.goodsReceiptNumber || '');
    this.purchaseInvoiceId.set(doc.purchaseInvoiceId ?? null);
    this.purchaseInvoiceNumber.set(doc.purchaseInvoiceNumber || '');
    this.lines.set((doc.lines || []).map(l => ({ ...l })));
    this.sourceLoaded.set(true);
  }

  private mapStatus(status: string | number, unified: number): string {
    if (typeof status === 'string' && isNaN(Number(status))) return status;
    const n = Number(status);
    switch (n) {
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
        switch (unified) {
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
  }

  private runAction(
    action: () => import('rxjs').Observable<void>,
    messages?: { successTitle: string; successMessage: string }
  ): void {
    this.saving.set(true);
    this.error.set(null);
    action().subscribe({
      next: () => {
        this.saving.set(false);
        if (this.docId()) this.loadDoc(this.docId()!);
        if (messages) {
          this.showResult(true, messages.successTitle, messages.successMessage);
        }
      },
      error: err => {
        this.saving.set(false);
        const msg = err?.error?.error ?? this.t('pur.pr.actionFailed');
        this.error.set(msg);
        this.showResult(false, this.t('pur.pr.result.failedTitle'), msg);
      }
    });
  }

  private showResult(
    success: boolean,
    title: string,
    message: string,
    navigateToId?: string | null
  ): void {
    this.resultDialog.set({
      open: true,
      success,
      title,
      message,
      navigateToId: navigateToId ?? null
    });
  }

  private todayIso(): string {
    return new Date().toISOString().slice(0, 10);
  }

  private toDateInput(value: string): string {
    return value.slice(0, 10);
  }

  private toIsoDate(dateOnly: string): string {
    return new Date(`${dateOnly}T12:00:00.000Z`).toISOString();
  }
}
