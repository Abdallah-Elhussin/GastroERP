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
import { Observable, catchError, of } from 'rxjs';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { InventoryService } from '../../../core/services/inventory.service';
import { InventoryRepository } from '../../../core/repositories/inventory.repository';
import { PurchaseOrderRepository } from '../../../core/repositories/purchase-order.repository';
import {
  CreatePurchaseOrderPayload,
  PURCHASE_ORDER_STATUS,
  PurchaseOrderDto,
  PurchaseOrderLineDraft,
  PurchaseOrderLineInput,
  UpdatePurchaseOrderPayload
} from '../../../core/models/purchase-order.models';
import {
  InventoryItemDefinition,
  InventoryUnit,
  SupplierSummary,
  Warehouse
} from '../../../core/models/inventory.models';
import { InventoryPageShellComponent } from '../../inventory/shared/inventory-page-shell.component';

@Component({
  selector: 'app-purchase-order-form-page',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, InventoryPageShellComponent],
  templateUrl: './purchase-order-form.page.html',
  styleUrl: './purchase-order-form.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PurchaseOrderFormPage implements OnInit {
  private repo = inject(PurchaseOrderRepository);
  private inventoryRepo = inject(InventoryRepository);
  private inventory = inject(InventoryService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  acting = signal(false);
  error = signal<string | null>(null);

  docId = signal<string | null>(null);
  poNumber = signal('');
  orderDate = signal(this.todayIso());
  expectedDeliveryDate = signal<string | null>(null);
  statusCode = signal<number>(PURCHASE_ORDER_STATUS.Draft);
  supplierId = signal<string | null>(null);
  destinationWarehouseId = signal<string | null>(null);
  currency = signal('SAR');
  exchangeRate = signal(1);
  notes = signal('');

  remainingQuantity = signal(0);
  completionPercent = signal(0);

  lines = signal<PurchaseOrderLineDraft[]>([]);
  selectedLineIndex = signal<number | null>(null);

  items = signal<InventoryItemDefinition[]>([]);
  units = signal<InventoryUnit[]>([]);
  warehouses = signal<Warehouse[]>([]);
  suppliers = signal<SupplierSummary[]>([]);

  breadcrumbs = [
    { labelKey: 'nav.purchases', path: '/purchases/dashboard' },
    { labelKey: 'pur.nav.purchaseOrders', path: '/purchases/purchase-orders' },
    { labelKey: 'pur.po.formBreadcrumb' }
  ];

  canCreate = computed(() => this.auth.hasPermission('Purchase.Create') || this.auth.hasPermission('Inventory.Manage'));
  canApprove = computed(() => this.auth.hasPermission('Purchase.Approve') || this.auth.hasPermission('Inventory.Manage'));
  canCancel = computed(() => this.auth.hasPermission('Purchase.Cancel') || this.auth.hasPermission('Inventory.Manage'));

  isNew = computed(() => !this.docId());
  isDraft = computed(() => this.statusCode() === PURCHASE_ORDER_STATUS.Draft);
  canEdit = computed(() => this.isNew() || this.isDraft());

  canApproveDoc = computed(() =>
    [PURCHASE_ORDER_STATUS.Draft, PURCHASE_ORDER_STATUS.PendingApproval].includes(this.statusCode() as 1 | 9)
  );
  canCancelDoc = computed(
    () =>
      ![PURCHASE_ORDER_STATUS.Closed, PURCHASE_ORDER_STATUS.Cancelled, PURCHASE_ORDER_STATUS.FullyReceived].includes(
        this.statusCode() as 6 | 5 | 7
      ) && this.completionPercent() <= 0
  );
  canCreateGrnDoc = computed(
    () =>
      [PURCHASE_ORDER_STATUS.Approved, PURCHASE_ORDER_STATUS.SentToSupplier, PURCHASE_ORDER_STATUS.PartiallyReceived].includes(
        this.statusCode() as 2 | 3 | 4
      ) && this.remainingQuantity() > 0
  );

  pageTitle = computed(() => this.t('pur.po.docTitle'));
  statusLabel = computed(() => this.t(`pur.po.status.${this.statusKeySuffix(this.statusCode())}`));

  netTotal = computed(() => this.lines().reduce((s, l) => s + (Number(l.lineSubTotal) || 0), 0));
  taxTotal = computed(() => this.lines().reduce((s, l) => s + (Number(l.taxAmount) || 0), 0));
  grandTotal = computed(() => this.lines().reduce((s, l) => s + (Number(l.lineTotal) || 0), 0));

  statusSteps = [
    { code: PURCHASE_ORDER_STATUS.Draft, labelKey: 'pur.po.status.draft', display: '0' },
    { code: PURCHASE_ORDER_STATUS.Approved, labelKey: 'pur.po.status.approved', display: '1' },
    { code: PURCHASE_ORDER_STATUS.Closed, labelKey: 'pur.po.status.closed', display: '2' },
    { code: PURCHASE_ORDER_STATUS.Cancelled, labelKey: 'pur.po.status.cancelled', display: '9' }
  ] as const;

  ngOnInit(): void {
    this.inventoryRepo
      .getItems(undefined, 1, 200)
      .pipe(catchError(() => of([] as InventoryItemDefinition[])))
      .subscribe(rows => this.items.set(rows.filter(i => i.isActive !== false)));

    this.inventoryRepo
      .getUnits()
      .pipe(catchError(() => of([] as InventoryUnit[])))
      .subscribe(rows => this.units.set(rows));

    this.inventoryRepo
      .getWarehouseLookup()
      .pipe(catchError(() => of([] as Warehouse[])))
      .subscribe(rows => {
        const list = rows.filter(w => w.isActive !== false);
        this.warehouses.set(list);
        if (!this.destinationWarehouseId() && list.length > 0) {
          const preferred = list.find(w => w.isDefault) ?? list[0];
          this.destinationWarehouseId.set(preferred.id);
        }
      });

    this.inventory
      .getSuppliers(1, 200)
      .pipe(catchError(() => of([] as SupplierSummary[])))
      .subscribe(s => this.suppliers.set(s));

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.docId.set(id);
      this.loadDoc(id);
    } else {
      this.addLine();
    }
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  itemLabel(item: InventoryItemDefinition): string {
    const code = (item.sku || item.barcode || '').trim();
    return code ? `${code} — ${item.nameAr}` : item.nameAr;
  }

  isStepActive(code: number): boolean {
    const current = this.statusCode();
    if (code === PURCHASE_ORDER_STATUS.Cancelled) return current === PURCHASE_ORDER_STATUS.Cancelled;
    if (current === PURCHASE_ORDER_STATUS.Cancelled) return false;
    if (code === PURCHASE_ORDER_STATUS.Closed) {
      return (
        current === PURCHASE_ORDER_STATUS.Closed ||
        current === PURCHASE_ORDER_STATUS.FullyReceived
      );
    }
    if (code === PURCHASE_ORDER_STATUS.Approved) {
      return [
        PURCHASE_ORDER_STATUS.Approved,
        PURCHASE_ORDER_STATUS.SentToSupplier,
        PURCHASE_ORDER_STATUS.PartiallyReceived,
        PURCHASE_ORDER_STATUS.FullyReceived,
        PURCHASE_ORDER_STATUS.Closed
      ].includes(current as 2 | 3 | 4 | 5 | 7);
    }
    return current === PURCHASE_ORDER_STATUS.Draft || current === PURCHASE_ORDER_STATUS.PendingApproval;
  }

  addLine(): void {
    if (!this.canEdit()) return;
    const draft: PurchaseOrderLineDraft = {
      inventoryItemId: '',
      unitId: '',
      quantity: 1,
      unitPrice: 0,
      discountAmount: 0,
      taxAmount: 0,
      taxPercent: 15,
      taxStatus: 1,
      lineSubTotal: 0,
      lineTotal: 0,
      receivedQuantity: 0,
      invoicedQuantity: 0,
      remainingQuantity: 0,
      description: null,
      lineNotes: null
    };
    this.lines.set([...this.lines(), draft]);
    this.selectedLineIndex.set(this.lines().length - 1);
  }

  removeSelectedLine(): void {
    const idx = this.selectedLineIndex();
    if (idx == null || !this.canEdit()) return;
    this.lines.set(this.lines().filter((_, i) => i !== idx));
    this.selectedLineIndex.set(null);
  }

  selectLine(index: number): void {
    this.selectedLineIndex.set(index);
  }

  onItemChange(index: number, itemId: string): void {
    if (!this.canEdit()) return;
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

  updateLine(index: number, patch: Partial<PurchaseOrderLineDraft>): void {
    if (!this.canEdit()) return;
    const next = [...this.lines()];
    next[index] = this.recalcLine({ ...next[index], ...patch });
    this.lines.set(next);
  }

  recalculate(): void {
    this.lines.set(this.lines().map(l => this.recalcLine(l)));
  }

  save(): void {
    if (!this.canCreate() || !this.canEdit()) return;
    if (!this.supplierId()) {
      this.error.set(this.t('pur.po.validation.supplier'));
      return;
    }

    this.ensureWarehouse();
    if (!this.destinationWarehouseId()) {
      this.error.set(this.t('pur.po.validation.warehouse'));
      return;
    }

    this.recalculate();
    const activeLines = this.lines().filter(l => l.inventoryItemId && Number(l.quantity) > 0);
    if (activeLines.length === 0) {
      this.error.set(this.t('pur.po.validation.lines'));
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    const linePayload = this.buildLinePayload(activeLines);

    if (this.docId()) {
      const payload: UpdatePurchaseOrderPayload = {
        supplierId: this.supplierId()!,
        destinationWarehouseId: this.destinationWarehouseId()!,
        orderDate: this.toIsoDate(this.orderDate()),
        expectedDeliveryDate: this.expectedDeliveryDate() ? this.toIsoDate(this.expectedDeliveryDate()!) : null,
        currency: this.currency(),
        exchangeRate: this.exchangeRate(),
        orderType: 1,
        notes: this.notes() || null,
        lines: linePayload
      };
      this.repo.update(this.docId()!, payload).subscribe({
        next: doc => {
          this.applyDoc(doc);
          this.saving.set(false);
        },
        error: err => {
          this.error.set(err?.error?.error ?? this.t('pur.po.saveFailed'));
          this.saving.set(false);
        }
      });
      return;
    }

    const createPayload: CreatePurchaseOrderPayload = {
      supplierId: this.supplierId()!,
      destinationWarehouseId: this.destinationWarehouseId()!,
      orderDate: this.toIsoDate(this.orderDate()),
      expectedDeliveryDate: this.expectedDeliveryDate() ? this.toIsoDate(this.expectedDeliveryDate()!) : null,
      currency: this.currency(),
      exchangeRate: this.exchangeRate(),
      orderType: 1,
      notes: this.notes() || null,
      lines: linePayload
    };

    this.repo.create(createPayload).subscribe({
      next: doc => {
        this.saving.set(false);
        void this.router.navigate(['/purchases/purchase-orders', doc.id], { replaceUrl: true });
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('pur.po.saveFailed'));
        this.saving.set(false);
      }
    });
  }

  approve(): void {
    const id = this.docId();
    if (!id || !this.canApprove() || !this.canApproveDoc()) return;
    this.runAction(() => this.repo.approve(id));
  }

  cancelDoc(): void {
    const id = this.docId();
    if (!id || !this.canCancel() || !this.canCancelDoc()) return;
    if (!confirm(this.t('pur.po.confirmCancel'))) return;
    this.runAction(() => this.repo.cancel(id));
  }

  createGrn(): void {
    const id = this.docId();
    if (!id || !this.canCreateGrnDoc()) return;
    void this.router.navigate(['/purchases/goods-receipts/new'], { queryParams: { poId: id } });
  }

  back(): void {
    void this.router.navigate(['/purchases/purchase-orders']);
  }

  private ensureWarehouse(): void {
    if (this.destinationWarehouseId()) return;
    const first = this.warehouses()[0];
    if (first) this.destinationWarehouseId.set(first.id);
  }

  private loadDoc(id: string): void {
    this.loading.set(true);
    this.repo.getById(id).subscribe({
      next: doc => {
        this.applyDoc(doc);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('pur.po.loadFailed'));
        this.loading.set(false);
      }
    });
  }

  private applyDoc(doc: PurchaseOrderDto): void {
    this.docId.set(doc.id);
    this.poNumber.set(doc.poNumber);
    this.orderDate.set(this.toDateInput(doc.orderDate));
    this.expectedDeliveryDate.set(doc.expectedDeliveryDate ? this.toDateInput(doc.expectedDeliveryDate) : null);
    this.statusCode.set(Number(doc.statusCode ?? doc.status) || PURCHASE_ORDER_STATUS.Draft);
    this.supplierId.set(doc.supplierId);
    this.destinationWarehouseId.set(doc.destinationWarehouseId);
    this.currency.set(doc.currency || 'SAR');
    this.exchangeRate.set(doc.exchangeRate || 1);
    this.notes.set(doc.notes || '');
    this.remainingQuantity.set(doc.remainingQuantity || 0);
    this.completionPercent.set(doc.completionPercent || 0);
    this.lines.set(
      (doc.lines || []).map(l => {
        const sub = Math.max(0, Number(l.lineSubTotal) || Number(l.quantity) * Number(l.unitPrice) || 0);
        const tax = Number(l.taxAmount) || 0;
        const taxPercent = sub > 0 ? Math.round((tax / sub) * 10000) / 100 : 15;
        return {
          ...l,
          taxPercent,
          taxStatus: (tax > 0 ? 1 : 0) as 0 | 1
        };
      })
    );
  }

  private buildLinePayload(lines: PurchaseOrderLineDraft[]): PurchaseOrderLineInput[] {
    return lines.map(l => ({
      inventoryItemId: l.inventoryItemId,
      unitId: l.unitId,
      quantity: Number(l.quantity) || 0,
      unitPrice: Number(l.unitPrice) || 0,
      discountAmount: 0,
      taxAmount: Number(l.taxAmount) || 0,
      description: l.description || null,
      warehouseId: null,
      lineNotes: null
    }));
  }

  private recalcLine(line: PurchaseOrderLineDraft): PurchaseOrderLineDraft {
    const qty = Number(line.quantity) || 0;
    const unitPrice = Number(line.unitPrice) || 0;
    const lineSubTotal = Math.max(0, qty * unitPrice);
    const taxStatus = line.taxStatus ?? 1;
    let taxPercent = Number(line.taxPercent);
    if (Number.isNaN(taxPercent)) taxPercent = 15;
    if (taxStatus === 0) taxPercent = 0;
    const taxAmount = Math.round(((lineSubTotal * taxPercent) / 100) * 100) / 100;
    const lineTotal = lineSubTotal + taxAmount;
    return {
      ...line,
      discountAmount: 0,
      taxStatus,
      taxPercent,
      taxAmount,
      lineSubTotal,
      lineTotal
    };
  }

  private statusKeySuffix(code: number): string {
    switch (code) {
      case PURCHASE_ORDER_STATUS.Draft:
        return 'draft';
      case PURCHASE_ORDER_STATUS.Approved:
        return 'approved';
      case PURCHASE_ORDER_STATUS.SentToSupplier:
        return 'sentToSupplier';
      case PURCHASE_ORDER_STATUS.PartiallyReceived:
        return 'partiallyReceived';
      case PURCHASE_ORDER_STATUS.FullyReceived:
        return 'fullyReceived';
      case PURCHASE_ORDER_STATUS.Cancelled:
        return 'cancelled';
      case PURCHASE_ORDER_STATUS.Closed:
        return 'closed';
      case PURCHASE_ORDER_STATUS.Rejected:
        return 'rejected';
      case PURCHASE_ORDER_STATUS.PendingApproval:
        return 'pendingApproval';
      default:
        return 'draft';
    }
  }

  private runAction(action: () => Observable<void>): void {
    this.acting.set(true);
    this.error.set(null);
    action().subscribe({
      next: () => {
        this.acting.set(false);
        if (this.docId()) this.loadDoc(this.docId()!);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('pur.po.actionFailed'));
        this.acting.set(false);
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
