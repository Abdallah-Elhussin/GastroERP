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
import { InventoryService } from '../../../core/services/inventory.service';
import { BackOfficeSalesDeliveryNoteRepository } from '../../../core/repositories/back-office-sales-delivery-note.repository';
import {
  BackOfficeSalesDeliveryNote,
  CreateBackOfficeSalesDeliveryNoteLineInput
} from '../../../core/models/back-office-sales-delivery-note.models';
import { BackOfficeSalesOrderRepository } from '../../../core/repositories/back-office-sales-order.repository';
import { BackOfficeSalesOrder, BackOfficeSalesOrderLine } from '../../../core/models/back-office-sales-order.models';
import { Warehouse } from '../../../core/models/inventory.models';

interface LineDraft {
  orderLineId: string;
  description: string;
  quantity: number;
  unitCost: number;
  inventoryItemId: string | null;
  unitId: string | null;
  maxQuantity: number;
}

@Component({
  selector: 'app-sales-delivery-note-form-page',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule],
  templateUrl: './sales-delivery-note-form.page.html',
  styleUrl: './sales-delivery-note-form.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SalesDeliveryNoteFormPage implements OnInit {
  private repo = inject(BackOfficeSalesDeliveryNoteRepository);
  private orderRepo = inject(BackOfficeSalesOrderRepository);
  private inventory = inject(InventoryService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  lang = inject(LanguageService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  docId = signal<string | null>(null);
  deliveryNumber = signal('');
  status = signal('Draft');
  orderId = signal<string | null>(null);
  customerId = signal<string | null>(null);
  warehouseId = signal<string | null>(null);
  deliveryDate = signal(new Date().toISOString().slice(0, 10));
  notes = signal('');
  lines = signal<LineDraft[]>([]);

  orders = signal<BackOfficeSalesOrder[]>([]);
  warehouses = signal<Warehouse[]>([]);
  selectedOrder = signal<BackOfficeSalesOrder | null>(null);
  orderLoading = signal(false);

  isDraft = computed(() => this.status() === 'Draft');
  isApproved = computed(() => this.status() === 'Approved');
  isPosted = computed(() => this.status() === 'Posted');
  isCancelled = computed(() => this.status() === 'Cancelled');
  isNew = computed(() => !this.docId());
  availableOrderLines = computed<BackOfficeSalesOrderLine[]>(() => {
    const order = this.selectedOrder();
    if (!order) return [];
    return order.lines.filter(l => (l.remainingToDeliver ?? 0) > 0 || this.lines().some(x => x.orderLineId === l.id));
  });
  total = computed(() => this.lines().reduce((s, l) => s + (Number(l.quantity) || 0) * (Number(l.unitCost) || 0), 0));

  ngOnInit(): void {
    this.inventory.loadWarehouses();
    this.orderRepo
      .getList({ pageSize: 500, status: 1 })
      .pipe(catchError(() => of([] as BackOfficeSalesOrder[])))
      .subscribe(rows => this.orders.set(rows));

    const pull = () => this.warehouses.set([...this.inventory.warehouses()]);
    pull();
    setTimeout(pull, 600);
    setTimeout(pull, 1500);

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.docId.set(id);
      this.load(id);
    }
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  onOrderChange(orderId: string | null): void {
    this.orderId.set(orderId);
    this.selectedOrder.set(null);
    this.lines.set([]);
    if (!orderId) return;
    this.orderLoading.set(true);
    this.orderRepo.getById(orderId).subscribe({
      next: order => {
        this.selectedOrder.set(order);
        this.customerId.set(order.customerId);
        if (!this.warehouseId() && order.warehouseId) this.warehouseId.set(order.warehouseId);
        this.orderLoading.set(false);
      },
      error: () => {
        this.error.set(this.t('bos.dn.orderLoadFailed'));
        this.orderLoading.set(false);
      }
    });
  }

  onOrderLineChange(index: number, orderLineId: string): void {
    const line = this.selectedOrder()?.lines.find(l => l.id === orderLineId);
    this.lines.update(list =>
      list.map((x, idx) => {
        if (idx !== index) return x;
        return {
          ...x,
          orderLineId,
          description: line?.description || x.description,
          inventoryItemId: line?.inventoryItemId || null,
          unitId: line?.unitId || null,
          unitCost: line?.unitCost || 0,
          maxQuantity: line?.remainingToDeliver ?? 0,
          quantity: line?.remainingToDeliver ?? x.quantity
        };
      })
    );
  }

  addLine(): void {
    if (!this.isDraft()) return;
    this.lines.set([
      ...this.lines(),
      { orderLineId: '', description: '', quantity: 1, unitCost: 0, inventoryItemId: null, unitId: null, maxQuantity: 0 }
    ]);
  }

  removeLine(index: number): void {
    this.lines.set(this.lines().filter((_, i) => i !== index));
  }

  updateLine(index: number, patch: Partial<LineDraft>): void {
    this.lines.update(list => list.map((x, idx) => (idx === index ? { ...x, ...patch } : x)));
  }

  back(): void {
    void this.router.navigate(['/sales/delivery-notes']);
  }

  save(): void {
    if (!this.isDraft()) return;
    if (!this.orderId() || !this.customerId()) {
      this.error.set(this.t('bos.dn.validation.order'));
      return;
    }
    if (!this.warehouseId()) {
      this.error.set(this.t('bos.dn.validation.warehouse'));
      return;
    }
    const active = this.lines().filter(l => l.orderLineId && Number(l.quantity) > 0);
    if (active.length === 0) {
      this.error.set(this.t('bos.dn.validation.lines'));
      return;
    }

    const linePayload: CreateBackOfficeSalesDeliveryNoteLineInput[] = active.map(l => ({
      orderLineId: l.orderLineId,
      description: l.description,
      quantity: Number(l.quantity),
      inventoryItemId: l.inventoryItemId,
      unitId: l.unitId,
      unitCost: Number(l.unitCost) || 0
    }));

    this.saving.set(true);
    this.error.set(null);

    if (this.docId()) {
      this.repo
        .update(this.docId()!, {
          deliveryDate: this.deliveryDate(),
          warehouseId: this.warehouseId() || null,
          notes: this.notes() || null,
          lines: linePayload
        })
        .subscribe({
          next: doc => {
            this.apply(doc);
            this.saving.set(false);
          },
          error: err => {
            this.error.set(err?.error?.error ?? this.t('bos.dn.saveFailed'));
            this.saving.set(false);
          }
        });
      return;
    }

    this.repo
      .create({
        orderId: this.orderId()!,
        customerId: this.customerId()!,
        warehouseId: this.warehouseId()!,
        deliveryDate: this.deliveryDate(),
        notes: this.notes() || null,
        lines: linePayload
      })
      .subscribe({
        next: doc => {
          this.saving.set(false);
          void this.router.navigate(['/sales/delivery-notes', doc.id]);
        },
        error: err => {
          this.error.set(err?.error?.error ?? this.t('bos.dn.saveFailed'));
          this.saving.set(false);
        }
      });
  }

  approve(): void {
    const id = this.docId();
    if (!id) return;
    this.runAction(() => this.repo.approve(id));
  }

  post(): void {
    const id = this.docId();
    if (!id) return;
    this.runAction(() => this.repo.post(id));
  }

  unpost(): void {
    const id = this.docId();
    if (!id) return;
    this.runAction(() => this.repo.unpost(id));
  }

  cancel(): void {
    const id = this.docId();
    if (!id) return;
    if (!confirm(this.t('bos.dn.confirmCancel'))) return;
    this.runAction(() => this.repo.cancel(id));
  }

  private load(id: string): void {
    this.loading.set(true);
    this.repo.getById(id).subscribe({
      next: doc => {
        this.apply(doc);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('bos.dn.loadFailed'));
        this.loading.set(false);
      }
    });
  }

  private apply(doc: BackOfficeSalesDeliveryNote): void {
    this.docId.set(doc.id);
    this.deliveryNumber.set(doc.deliveryNumber);
    this.status.set(this.mapStatus(doc.status));
    this.orderId.set(doc.orderId);
    this.customerId.set(doc.customerId);
    this.warehouseId.set(doc.warehouseId || null);
    this.deliveryDate.set(String(doc.deliveryDate).slice(0, 10));
    this.notes.set(doc.notes || '');
    this.lines.set(
      (doc.lines || []).map(l => ({
        orderLineId: l.orderLineId,
        description: l.description,
        quantity: l.quantity,
        unitCost: l.unitCost,
        inventoryItemId: l.inventoryItemId || null,
        unitId: l.unitId || null,
        maxQuantity: l.quantity
      }))
    );
    if (doc.orderId && !this.selectedOrder()) {
      this.orderRepo.getById(doc.orderId).subscribe({ next: order => this.selectedOrder.set(order) });
    }
  }

  private mapStatus(status: string | number): string {
    if (typeof status === 'string' && isNaN(Number(status))) return status;
    switch (Number(status)) {
      case 1:
        return 'Approved';
      case 2:
        return 'Posted';
      case 9:
        return 'Cancelled';
      default:
        return 'Draft';
    }
  }

  private runAction(action: () => import('rxjs').Observable<void>): void {
    this.saving.set(true);
    action().subscribe({
      next: () => {
        this.saving.set(false);
        if (this.docId()) this.load(this.docId()!);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('bos.dn.actionFailed'));
        this.saving.set(false);
      }
    });
  }
}
