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
import { InventoryService } from '../../../core/services/inventory.service';
import { GoodsReceiptRepository } from '../../../core/repositories/goods-receipt.repository';
import { PurchaseReturnRepository } from '../../../core/repositories/purchase-return.repository';
import {
  CreatePurchaseReturnPayload,
  PurchaseReturnDoc,
  PurchaseReturnLine,
  PurchaseReturnReason,
  UpdatePurchaseReturnPayload
} from '../../../core/models/purchase-return.models';
import { GoodsReceiptDoc } from '../../../core/models/goods-receipt.models';
import { Warehouse } from '../../../core/models/inventory.models';
import { InventoryPageShellComponent } from '../../inventory/shared/inventory-page-shell.component';

interface InvoiceOption {
  id: string;
  invoiceNumber: string;
  kind: number;
  status: number;
  supplierId: string;
  warehouseId?: string | null;
  totalAmount: number;
}

@Component({
  selector: 'app-purchase-return-form-page',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, InventoryPageShellComponent],
  templateUrl: './purchase-return-form.page.html',
  styleUrl: './purchase-return-form.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PurchaseReturnFormPage implements OnInit {
  private repo = inject(PurchaseReturnRepository);
  private grnRepo = inject(GoodsReceiptRepository);
  private inventory = inject(InventoryService);
  private http = inject(HttpClient);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);

  docId = signal<string | null>(null);
  returnNumber = signal('');
  returnDate = signal(this.todayIso());
  status = signal('Draft');
  unifiedStatusCode = signal(0);
  returnType = signal<number>(1);
  currency = signal('SAR');
  notes = signal('');
  referenceNumber = signal('');
  reasonNotes = signal('');
  returnReasonId = signal<string | null>(null);
  warehouseId = signal<string | null>(null);
  warehouseNameAr = signal('');
  supplierId = signal<string | null>(null);
  supplierNameAr = signal('');
  goodsReceiptId = signal<string | null>(null);
  goodsReceiptNumber = signal('');
  purchaseInvoiceId = signal<string | null>(null);
  purchaseInvoiceNumber = signal('');

  lines = signal<PurchaseReturnLine[]>([]);
  selectedLineIndex = signal<number | null>(null);

  reasons = signal<PurchaseReturnReason[]>([]);
  warehouses = signal<Warehouse[]>([]);
  goodsReceipts = signal<GoodsReceiptDoc[]>([]);
  invoices = signal<InvoiceOption[]>([]);

  isDirectRoute = signal(false);

  breadcrumbs = computed(() => {
    const listKey = this.isDirectRoute() ? 'pur.nav.directReturns' : 'pur.nav.purchaseReturns';
    const listPath = this.isDirectRoute()
      ? '/purchases/direct-returns'
      : '/purchases/purchase-returns';
    return [
      { labelKey: 'nav.purchases', path: '/purchases/dashboard' },
      { labelKey: listKey, path: listPath },
      { labelKey: 'pur.pr.formBreadcrumb' }
    ];
  });

  canManage = computed(() => this.auth.hasPermission('Inventory.Manage'));
  isDraft = computed(() => this.status() === 'Draft');
  isPosted = computed(() => this.status() === 'Posted');
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
  grandTotal = computed(() => this.lines().reduce((s, l) => s + (Number(l.lineTotal) || 0), 0));
  listBasePath = computed(() =>
    this.isDirectRoute() ? '/purchases/direct-returns' : '/purchases/purchase-returns'
  );

  ngOnInit(): void {
    const path = this.route.snapshot.routeConfig?.path ?? '';
    const parentPath = this.route.parent?.snapshot.routeConfig?.path ?? '';
    const url = this.router.url;
    const isDirect =
      url.includes('/direct-returns') ||
      path.startsWith('direct-returns') ||
      parentPath.startsWith('direct-returns') ||
      this.route.snapshot.data['defaultReturnType'] === 3;
    this.isDirectRoute.set(isDirect);

    const qpType = Number(this.route.snapshot.queryParamMap.get('returnType'));
    if (isDirect) {
      this.returnType.set(3);
    } else if (qpType === 1 || qpType === 2 || qpType === 3) {
      this.returnType.set(qpType);
    }

    this.inventory.loadWarehouses();
    const pullWh = () => this.warehouses.set([...this.inventory.warehouses()]);
    pullWh();
    setTimeout(pullWh, 400);

    this.loadReasons();
    this.loadSourceOptions();

    const qpInvoiceId = this.route.snapshot.queryParamMap.get('purchaseInvoiceId');
    if (qpInvoiceId && !this.route.snapshot.paramMap.get('id')) {
      this.purchaseInvoiceId.set(qpInvoiceId);
      this.returnType.set(isDirect ? 3 : 2);
      this.loadSourceOptions();
      // Prefill lines from posted invoice when opening "create return" from DPI.
      this.repo.previewFromInvoice(qpInvoiceId).subscribe({
        next: doc => this.applyPreview(doc),
        error: () => undefined
      });
    }

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.docId.set(id);
      this.loadDoc(id);
    }
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  onReturnTypeChange(value: number): void {
    if (!this.isDraft() || this.docId()) return;
    this.returnType.set(Number(value));
    this.goodsReceiptId.set(null);
    this.goodsReceiptNumber.set('');
    this.purchaseInvoiceId.set(null);
    this.purchaseInvoiceNumber.set('');
    this.supplierId.set(null);
    this.supplierNameAr.set('');
    this.warehouseId.set(null);
    this.warehouseNameAr.set('');
    this.lines.set([]);
    this.loadSourceOptions();
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
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('pur.pr.loadFailed'));
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
    this.repo.previewFromInvoice(invId).subscribe({
      next: preview => {
        this.applyPreview(preview);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('pur.pr.loadFailed'));
        this.loading.set(false);
      }
    });
  }

  save(): void {
    if (!this.canManage() || !this.isDraft()) return;
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
    const activeLines = this.lines().filter(l => Number(l.returnQuantity) > 0);
    if (activeLines.length === 0) {
      this.error.set(this.t('pur.pr.validation.lines'));
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
        reasonNotes: this.reasonNotes() || null,
        referenceNumber: this.referenceNumber() || null,
        notes: this.notes() || null,
        lines: linePayload
      };
      this.repo.update(this.docId()!, payload).subscribe({
        next: doc => {
          this.applyDoc(doc);
          this.saving.set(false);
        },
        error: err => {
          this.error.set(err?.error?.error ?? this.t('pur.pr.saveFailed'));
          this.saving.set(false);
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
      reasonNotes: this.reasonNotes() || null,
      referenceNumber: this.referenceNumber() || null,
      notes: this.notes() || null,
      currency: this.currency(),
      lines: linePayload
    };

    this.repo.create(createPayload).subscribe({
      next: doc => {
        this.saving.set(false);
        void this.router.navigate([this.listBasePath(), doc.id], { replaceUrl: true });
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('pur.pr.saveFailed'));
        this.saving.set(false);
      }
    });
  }

  approve(): void {
    const id = this.docId();
    if (!id || !this.canManage()) return;
    this.runAction(() => this.repo.approve(id));
  }

  post(): void {
    const id = this.docId();
    if (!id || !this.canManage()) return;
    this.runAction(() => this.repo.post(id));
  }

  unpost(): void {
    const id = this.docId();
    if (!id || !this.canManage()) return;
    this.runAction(() => this.repo.unpost(id));
  }

  cancelDoc(): void {
    const id = this.docId();
    if (!id || !this.canManage()) return;
    if (!confirm(this.t('pur.pr.confirmCancel'))) return;
    this.runAction(() => this.repo.cancel(id));
  }

  back(): void {
    void this.router.navigate([this.listBasePath()]);
  }

  selectLine(index: number): void {
    this.selectedLineIndex.set(index);
  }

  updateLine(index: number, patch: Partial<PurchaseReturnLine>): void {
    if (!this.isDraft()) return;
    const next = [...this.lines()];
    const row = { ...next[index], ...patch };
    let qty = Number(row.returnQuantity) || 0;
    const available = Number(row.availableToReturn) || qty;
    if (qty > available) {
      qty = available;
      row.returnQuantity = available;
    }
    const cost = Number(row.unitCost) || 0;
    const discount = Number(row.discountAmount) || 0;
    const taxPercent = Number(row.taxPercent) || 0;
    const origQty = Number(row.originalQuantity) || 0;
    row.lineSubTotal = Math.max(0, qty * cost - discount);
    if (taxPercent > 0) {
      row.taxAmount = Math.round(row.lineSubTotal * taxPercent) / 100;
    } else if (origQty > 0 && 'returnQuantity' in patch) {
      const baseTax = Number(next[index].taxAmount) || 0;
      const baseQty = Number(next[index].returnQuantity) || origQty;
      row.taxAmount = baseQty > 0 ? Math.round((baseTax * qty) / baseQty * 100) / 100 : 0;
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

    let params = new HttpParams().set('page', 1).set('pageSize', 100).set('status', 2);
    if (this.returnType() === 3) {
      params = params.set('kind', 2);
    } else if (this.returnType() === 2) {
      params = params.set('kind', 1);
    }
    this.http
      .get<InvoiceOption[]>(`${environment.apiBaseUrl}/inventory/purchase-invoices`, { params })
      .pipe(catchError(() => of([] as InvoiceOption[])))
      .subscribe(rows => this.invoices.set(rows ?? []));
  }

  private applyPreview(doc: PurchaseReturnDoc): void {
    this.returnType.set(Number(doc.returnType) || this.returnType());
    this.supplierId.set(doc.supplierId);
    this.supplierNameAr.set(doc.supplierNameAr || '');
    this.warehouseId.set(doc.warehouseId);
    this.warehouseNameAr.set(doc.warehouseNameAr || '');
    this.goodsReceiptId.set(doc.goodsReceiptId ?? null);
    this.goodsReceiptNumber.set(doc.goodsReceiptNumber || '');
    this.purchaseInvoiceId.set(doc.purchaseInvoiceId ?? null);
    this.purchaseInvoiceNumber.set(doc.purchaseInvoiceNumber || '');
    this.currency.set(doc.currency || 'SAR');
    if (doc.notes) this.notes.set(doc.notes);
    this.lines.set((doc.lines || []).map(l => ({ ...l })));
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
    this.reasonNotes.set(doc.reasonNotes || '');
    this.returnReasonId.set(doc.returnReasonId ?? null);
    this.warehouseId.set(doc.warehouseId);
    this.warehouseNameAr.set(doc.warehouseNameAr || '');
    this.supplierId.set(doc.supplierId);
    this.supplierNameAr.set(doc.supplierNameAr || '');
    this.goodsReceiptId.set(doc.goodsReceiptId ?? null);
    this.goodsReceiptNumber.set(doc.goodsReceiptNumber || '');
    this.purchaseInvoiceId.set(doc.purchaseInvoiceId ?? null);
    this.purchaseInvoiceNumber.set(doc.purchaseInvoiceNumber || '');
    this.lines.set((doc.lines || []).map(l => ({ ...l })));
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

  private runAction(action: () => import('rxjs').Observable<void>): void {
    this.saving.set(true);
    this.error.set(null);
    action().subscribe({
      next: () => {
        this.saving.set(false);
        if (this.docId()) this.loadDoc(this.docId()!);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('pur.pr.actionFailed'));
        this.saving.set(false);
      }
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
