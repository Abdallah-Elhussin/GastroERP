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
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { InventoryService } from '../../../core/services/inventory.service';
import { GoodsReceiptRepository } from '../../../core/repositories/goods-receipt.repository';
import {
  CreateGoodsReceiptPayload,
  GoodsReceiptLine,
  UpdateGoodsReceiptPayload
} from '../../../core/models/goods-receipt.models';
import {
  InventoryItemDefinition,
  InventoryUnit,
  PurchaseOrderSummary,
  SupplierSummary,
  Warehouse
} from '../../../core/models/inventory.models';
import { InventoryPageShellComponent } from '../../inventory/shared/inventory-page-shell.component';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-goods-receipt-form-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, MatIconModule, InventoryPageShellComponent],
  templateUrl: './goods-receipt-form.page.html',
  styleUrl: './goods-receipt-form.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GoodsReceiptFormPage implements OnInit {
  private repo = inject(GoodsReceiptRepository);
  private inventory = inject(InventoryService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);

  docId = signal<string | null>(null);
  grnNumber = signal('');
  receiptDate = signal(this.todayIso());
  status = signal('Draft');
  unifiedStatusCode = signal(0);
  source = signal(1);
  currency = signal('SAR');
  exchangeRate = signal(1);
  notes = signal('');
  referenceNumber = signal('');
  receiptMethod = signal('');
  receivedByName = signal('');
  supplierRepName = signal('');
  vehicleNumber = signal('');
  waybillNumber = signal('');
  warehouseId = signal<string | null>(null);
  supplierId = signal<string | null>(null);
  purchaseOrderId = signal<string | null>(null);
  poNumber = signal('');
  poCompletionPercent = signal<number | null>(null);
  supplierNameAr = signal('');
  supplierLocked = signal(false);

  inspectionResult = signal(1);
  inspectedBy = signal('');
  inspectionDate = signal(this.todayIso());
  qualityNotes = signal('');
  rejectionReason = signal('');
  qualityCertificateRef = signal('');
  expiryCertificateRef = signal('');

  lines = signal<GoodsReceiptLine[]>([]);
  selectedLineIndex = signal<number | null>(null);

  items = signal<InventoryItemDefinition[]>([]);
  units = signal<InventoryUnit[]>([]);
  warehouses = signal<Warehouse[]>([]);
  purchaseOrders = signal<PurchaseOrderSummary[]>([]);
  suppliers = signal<SupplierSummary[]>([]);

  breadcrumbs = [
    { labelKey: 'nav.purchases', path: '/purchases/dashboard' },
    { labelKey: 'pur.nav.goodsReceipts', path: '/purchases/goods-receipts' },
    { labelKey: 'pur.grn.formBreadcrumb' }
  ];

  canManage = computed(() => this.auth.hasPermission('Inventory.Manage'));
  isDraft = computed(() => this.status() === 'Draft');
  isApproved = computed(() => this.status() === 'Approved');
  isPosted = computed(() => this.status() === 'Posted');
  isNew = computed(() => !this.docId());
  pageTitle = computed(() =>
    this.isNew()
      ? this.t('pur.grn.createTitle')
      : `${this.t('pur.grn.docTitle')} — ${this.grnNumber()}`
  );
  statusLabel = computed(() => {
    switch (this.status()) {
      case 'Approved':
        return this.t('pur.grn.status.approved');
      case 'Posted':
        return this.t('pur.grn.status.posted');
      case 'Reversed':
        return this.t('pur.grn.status.reversed');
      case 'Cancelled':
        return this.t('pur.grn.status.cancelled');
      default:
        return this.t('pur.grn.status.draft');
    }
  });
  grandTotal = computed(() =>
    this.lines().reduce((s, l) => s + Math.max(0, l.acceptedQuantity * l.unitCost - (l.discountAmount || 0)) + (l.taxAmount || 0), 0)
  );

  ngOnInit(): void {
    this.inventory.loadWarehouses();
    this.inventory.loadItems();
    this.inventory.loadUnits();
    this.inventory.getPurchaseOrders().pipe(catchError(() => of([] as PurchaseOrderSummary[]))).subscribe(p => this.purchaseOrders.set(p));
    this.inventory.getSuppliers().pipe(catchError(() => of([] as SupplierSummary[]))).subscribe(s => this.suppliers.set(s));

    const pull = () => {
      this.warehouses.set([...this.inventory.warehouses()]);
      this.items.set([...this.inventory.items()]);
      this.units.set([...this.inventory.units()]);
    };
    pull();
    setTimeout(pull, 400);

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.docId.set(id);
      this.loadDoc(id);
    }
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  loadFromPo(): void {
    const poId = this.purchaseOrderId();
    if (!poId) {
      this.error.set(this.t('pur.grn.validation.po'));
      return;
    }
    this.loading.set(true);
    this.error.set(null);
    this.repo.previewFromPo(poId).subscribe({
      next: preview => {
        this.applyPreview(preview);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('pur.grn.loadFailed'));
        this.loading.set(false);
      }
    });
  }

  save(): void {
    if (!this.canManage() || !this.isDraft()) return;
    if (!this.warehouseId()) {
      this.error.set(this.t('pur.grn.validation.warehouse'));
      return;
    }
    if (!this.purchaseOrderId() && !this.supplierId()) {
      this.error.set(this.t('pur.grn.validation.supplierOrPo'));
      return;
    }
    if (this.lines().length === 0) {
      this.error.set(this.t('pur.grn.validation.lines'));
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    const linePayload = this.lines().map(l => ({
      inventoryItemId: l.inventoryItemId,
      unitId: l.unitId,
      receivedQuantity: Number(l.receivedQuantity) || 0,
      unitCost: Number(l.unitCost) || 0,
      purchaseOrderLineId: l.purchaseOrderLineId || null,
      orderedQuantity: Number(l.orderedQuantity) || 0,
      previouslyReceivedQuantity: Number(l.previouslyReceivedQuantity) || 0,
      acceptedQuantity: Number(l.acceptedQuantity) || 0,
      rejectedQuantity: Number(l.rejectedQuantity) || 0,
      discountAmount: Number(l.discountAmount) || 0,
      taxPercent: Number(l.taxPercent) || 0,
      taxAmount: Number(l.taxAmount) || 0,
      batchNumber: l.batchNumber || null,
      productionDate: l.productionDate || null,
      expiryDate: l.expiryDate || null,
      storageLocation: l.storageLocation || null,
      description: l.description || null
    }));

    if (this.docId()) {
      const payload: UpdateGoodsReceiptPayload = {
        receiptDate: this.toIsoDate(this.receiptDate()),
        warehouseId: this.warehouseId()!,
        referenceNumber: this.referenceNumber() || null,
        notes: this.notes() || null,
        receiptMethod: this.receiptMethod() || null,
        receivedByName: this.receivedByName() || null,
        supplierRepName: this.supplierRepName() || null,
        vehicleNumber: this.vehicleNumber() || null,
        waybillNumber: this.waybillNumber() || null,
        currency: this.currency(),
        exchangeRate: this.exchangeRate(),
        inspectionResult: this.inspectionResult(),
        inspectedBy: this.inspectedBy() || null,
        inspectionDate: this.toIsoDate(this.inspectionDate()),
        qualityNotes: this.qualityNotes() || null,
        rejectionReason: this.rejectionReason() || null,
        qualityCertificateRef: this.qualityCertificateRef() || null,
        expiryCertificateRef: this.expiryCertificateRef() || null,
        lines: linePayload
      };
      this.repo.update(this.docId()!, payload).subscribe({
        next: doc => {
          this.applyDoc(doc);
          this.saving.set(false);
        },
        error: err => {
          this.error.set(err?.error?.error ?? this.t('pur.grn.saveFailed'));
          this.saving.set(false);
        }
      });
      return;
    }

    const createPayload: CreateGoodsReceiptPayload = {
      warehouseId: this.warehouseId()!,
      purchaseOrderId: this.purchaseOrderId() || null,
      supplierId: this.supplierId() || null,
      directReceipt: !this.purchaseOrderId(),
      receiptDate: this.toIsoDate(this.receiptDate()),
      currency: this.currency(),
      exchangeRate: this.exchangeRate(),
      referenceNumber: this.referenceNumber() || null,
      notes: this.notes() || null,
      receiptMethod: this.receiptMethod() || null,
      receivedByName: this.receivedByName() || null,
      supplierRepName: this.supplierRepName() || null,
      vehicleNumber: this.vehicleNumber() || null,
      waybillNumber: this.waybillNumber() || null,
      inspectionResult: this.inspectionResult(),
      inspectedBy: this.inspectedBy() || null,
      inspectionDate: this.toIsoDate(this.inspectionDate()),
      qualityNotes: this.qualityNotes() || null,
      rejectionReason: this.rejectionReason() || null,
      qualityCertificateRef: this.qualityCertificateRef() || null,
      expiryCertificateRef: this.expiryCertificateRef() || null,
      lines: linePayload
    };

    this.repo.create(createPayload).subscribe({
      next: doc => {
        this.saving.set(false);
        void this.router.navigate(['/purchases/goods-receipts', doc.id], { replaceUrl: true });
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('pur.grn.saveFailed'));
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
    if (!confirm(this.t('pur.grn.confirmCancel'))) return;
    this.runAction(() => this.repo.cancel(id));
  }

  createInvoice(): void {
    const id = this.docId();
    if (!id || !this.isPosted()) return;
    void this.router.navigate(['/purchases/purchase-invoices'], { queryParams: { goodsReceiptId: id } });
  }

  back(): void {
    void this.router.navigate(['/purchases/goods-receipts']);
  }

  selectLine(index: number): void {
    this.selectedLineIndex.set(index);
  }

  updateLine(index: number, patch: Partial<GoodsReceiptLine>): void {
    if (!this.isDraft()) return;
    const next = [...this.lines()];
    const row = { ...next[index], ...patch };
    if ('receivedQuantity' in patch && patch.acceptedQuantity == null) {
      row.acceptedQuantity = Number(row.receivedQuantity) - Number(row.rejectedQuantity || 0);
    }
    row.lineSubTotal = Math.max(0, Number(row.acceptedQuantity) * Number(row.unitCost) - Number(row.discountAmount || 0));
    next[index] = row;
    this.lines.set(next);
  }

  removeSelectedLine(): void {
    const idx = this.selectedLineIndex();
    if (idx == null || !this.isDraft()) return;
    const next = this.lines().filter((_, i) => i !== idx);
    this.lines.set(next);
    this.selectedLineIndex.set(null);
  }

  lineTone(line: GoodsReceiptLine): string {
    if (line.orderedQuantity <= 0) return 'neutral';
    const rem = Math.max(0, line.orderedQuantity - line.previouslyReceivedQuantity - line.receivedQuantity);
    if (rem <= 0) return 'complete';
    if (line.previouslyReceivedQuantity > 0 || line.receivedQuantity > 0) return 'partial';
    return 'none';
  }

  private loadDoc(id: string): void {
    this.loading.set(true);
    this.repo.getById(id).subscribe({
      next: doc => {
        this.applyDoc(doc);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('pur.grn.loadFailed'));
        this.loading.set(false);
      }
    });
  }

  private applyPreview(doc: {
    purchaseOrderId?: string | null;
    poNumber: string;
    poCompletionPercent?: number | null;
    supplierId: string;
    supplierNameAr: string;
    warehouseId: string;
    currency: string;
    notes?: string | null;
    lines: GoodsReceiptLine[];
  }): void {
    this.purchaseOrderId.set(doc.purchaseOrderId ?? null);
    this.poNumber.set(doc.poNumber);
    this.poCompletionPercent.set(doc.poCompletionPercent ?? null);
    this.supplierId.set(doc.supplierId);
    this.supplierNameAr.set(doc.supplierNameAr);
    this.supplierLocked.set(true);
    this.warehouseId.set(doc.warehouseId);
    this.currency.set(doc.currency || 'SAR');
    if (doc.notes) this.notes.set(doc.notes);
    this.lines.set(doc.lines.map(l => ({ ...l })));
    this.source.set(1);
  }

  private applyDoc(doc: import('../../../core/models/goods-receipt.models').GoodsReceiptDoc): void {
    this.docId.set(doc.id);
    this.grnNumber.set(doc.grnNumber);
    this.receiptDate.set(this.toDateInput(doc.receiptDate));
    this.status.set(this.mapStatus(doc.status, doc.unifiedStatusCode));
    this.unifiedStatusCode.set(doc.unifiedStatusCode);
    this.source.set(doc.source);
    this.currency.set(doc.currency || 'SAR');
    this.exchangeRate.set(doc.exchangeRate || 1);
    this.notes.set(doc.notes || '');
    this.referenceNumber.set(doc.referenceNumber || '');
    this.receiptMethod.set(doc.receiptMethod || '');
    this.receivedByName.set(doc.receivedByName || '');
    this.supplierRepName.set(doc.supplierRepName || '');
    this.vehicleNumber.set(doc.vehicleNumber || '');
    this.waybillNumber.set(doc.waybillNumber || '');
    this.warehouseId.set(doc.warehouseId);
    this.supplierId.set(doc.supplierId);
    this.supplierNameAr.set(doc.supplierNameAr);
    this.purchaseOrderId.set(doc.purchaseOrderId ?? null);
    this.poNumber.set(doc.poNumber || '');
    this.poCompletionPercent.set(doc.poCompletionPercent ?? null);
    this.supplierLocked.set(!!doc.purchaseOrderId);
    this.inspectionResult.set(Number(doc.inspectionResult) || 1);
    this.inspectedBy.set(doc.inspectedBy || '');
    this.inspectionDate.set(doc.inspectionDate ? this.toDateInput(doc.inspectionDate) : this.todayIso());
    this.qualityNotes.set(doc.qualityNotes || '');
    this.rejectionReason.set(doc.rejectionReason || '');
    this.qualityCertificateRef.set(doc.qualityCertificateRef || '');
    this.expiryCertificateRef.set(doc.expiryCertificateRef || '');
    this.lines.set((doc.lines || []).map(l => ({ ...l })));
  }

  private mapStatus(status: string | number, unified: number): string {
    if (typeof status === 'string' && isNaN(Number(status))) return status;
    const n = Number(status);
    switch (n) {
      case 1:
        return 'Draft';
      case 4:
        return 'Approved';
      case 2:
        return 'Posted';
      case 8:
        return 'Reversed';
      case 3:
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
        this.error.set(err?.error?.error ?? this.t('pur.grn.actionFailed'));
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
