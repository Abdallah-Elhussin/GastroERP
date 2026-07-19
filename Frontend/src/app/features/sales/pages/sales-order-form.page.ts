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
import { CrmRepository } from '../../../core/repositories/crm.repository';
import { CrmCustomerSummary } from '../../../core/repositories/rest-crm.repository';
import { BackOfficeSalesOrderRepository } from '../../../core/repositories/back-office-sales-order.repository';
import {
  BackOfficeSalesOrder,
  CreateBackOfficeSalesOrderLineInput
} from '../../../core/models/back-office-sales-order.models';
import {
  InventoryItemDefinition,
  InventoryUnit,
  Warehouse
} from '../../../core/models/inventory.models';

interface LineDraft {
  description: string;
  quantity: number;
  unitPrice: number;
  taxPercent: number;
  inventoryItemId: string;
  unitId: string;
  lineNature: number;
  unitCost: number;
}

@Component({
  selector: 'app-sales-order-form-page',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule],
  templateUrl: './sales-order-form.page.html',
  styleUrl: './sales-order-form.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SalesOrderFormPage implements OnInit {
  private repo = inject(BackOfficeSalesOrderRepository);
  private inventory = inject(InventoryService);
  private crm = inject(CrmRepository);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  lang = inject(LanguageService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  docId = signal<string | null>(null);
  orderNumber = signal('');
  status = signal('Draft');
  customerId = signal<string | null>(null);
  warehouseId = signal<string | null>(null);
  orderDate = signal(new Date().toISOString().slice(0, 10));
  expectedDeliveryDate = signal('');
  notes = signal('');
  lines = signal<LineDraft[]>([]);

  customers = signal<CrmCustomerSummary[]>([]);
  warehouses = signal<Warehouse[]>([]);
  items = signal<InventoryItemDefinition[]>([]);
  units = signal<InventoryUnit[]>([]);

  showConvertModal = signal(false);
  convertPaymentMode = signal(1);
  convertWarehouseId = signal<string | null>(null);
  convertDueDate = signal('');
  converting = signal(false);
  convertError = signal<string | null>(null);

  isDraft = computed(() => this.status() === 'Draft');
  isApproved = computed(() => this.status() === 'Approved');
  isPosted = computed(() => this.status() === 'Posted');
  isCancelled = computed(() => this.status() === 'Cancelled');
  isNew = computed(() => !this.docId());
  canConvert = computed(() => (this.isApproved() || this.isPosted()) && !!this.docId());
  total = computed(() =>
    this.lines().reduce((s, l) => {
      const net = (Number(l.quantity) || 0) * (Number(l.unitPrice) || 0);
      const tax = net * ((Number(l.taxPercent) || 0) / 100);
      return s + net + tax;
    }, 0)
  );

  ngOnInit(): void {
    this.inventory.loadWarehouses();
    this.inventory.loadItems();
    this.inventory.loadUnits();
    this.crm
      .getCustomers(1, 500)
      .pipe(catchError(() => of([] as CrmCustomerSummary[])))
      .subscribe(rows => this.customers.set(rows));

    const pull = () => {
      this.warehouses.set([...this.inventory.warehouses()]);
      this.items.set([...this.inventory.items()]);
      this.units.set([...this.inventory.units()]);
    };
    pull();
    setTimeout(pull, 600);
    setTimeout(pull, 1500);

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.docId.set(id);
      this.load(id);
    } else {
      this.addLine();
    }
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  onItemChange(index: number, itemId: string): void {
    const item = this.items().find(i => i.id === itemId);
    this.lines.update(list =>
      list.map((x, idx) => {
        if (idx !== index) return x;
        return {
          ...x,
          inventoryItemId: itemId,
          description: item?.nameAr || x.description,
          unitId: item?.baseUnitId || x.unitId
        };
      })
    );
  }

  addLine(): void {
    if (!this.isDraft()) return;
    this.lines.set([
      ...this.lines(),
      {
        description: '',
        quantity: 1,
        unitPrice: 0,
        taxPercent: 15,
        inventoryItemId: '',
        unitId: '',
        lineNature: 1,
        unitCost: 0
      }
    ]);
  }

  removeLine(index: number): void {
    if (!this.isDraft()) return;
    this.lines.set(this.lines().filter((_, i) => i !== index));
  }

  updateLine(index: number, patch: Partial<LineDraft>): void {
    this.lines.update(list => list.map((x, idx) => (idx === index ? { ...x, ...patch } : x)));
  }

  back(): void {
    void this.router.navigate(['/sales/orders']);
  }

  save(): void {
    if (!this.isDraft()) return;
    if (!this.customerId()) {
      this.error.set(this.t('bos.ord.validation.customer'));
      return;
    }
    const active = this.lines().filter(l => l.description.trim() && Number(l.quantity) > 0);
    if (active.length === 0) {
      this.error.set(this.t('bos.ord.validation.lines'));
      return;
    }

    const linePayload: CreateBackOfficeSalesOrderLineInput[] = active.map(l => ({
      description: l.description,
      quantity: Number(l.quantity),
      unitPrice: Number(l.unitPrice),
      taxPercent: Number(l.taxPercent) || 0,
      lineNature: Number(l.lineNature) || 1,
      inventoryItemId: l.inventoryItemId || null,
      unitId: l.unitId || null,
      unitCost: Number(l.unitCost) || 0
    }));

    this.saving.set(true);
    this.error.set(null);

    if (this.docId()) {
      this.repo
        .update(this.docId()!, {
          orderDate: this.orderDate(),
          warehouseId: this.warehouseId() || null,
          expectedDeliveryDate: this.expectedDeliveryDate() || null,
          notes: this.notes() || null,
          lines: linePayload
        })
        .subscribe({
          next: doc => {
            this.apply(doc);
            this.saving.set(false);
          },
          error: err => {
            this.error.set(err?.error?.error ?? this.t('bos.ord.saveFailed'));
            this.saving.set(false);
          }
        });
      return;
    }

    this.repo
      .create({
        customerId: this.customerId()!,
        orderDate: this.orderDate(),
        warehouseId: this.warehouseId() || null,
        expectedDeliveryDate: this.expectedDeliveryDate() || null,
        notes: this.notes() || null,
        lines: linePayload
      })
      .subscribe({
        next: doc => {
          this.saving.set(false);
          void this.router.navigate(['/sales/orders', doc.id]);
        },
        error: err => {
          this.error.set(err?.error?.error ?? this.t('bos.ord.saveFailed'));
          this.saving.set(false);
        }
      });
  }

  approve(): void {
    const id = this.docId();
    if (!id) return;
    this.runAction(() => this.repo.approve(id));
  }

  close(): void {
    const id = this.docId();
    if (!id) return;
    this.runAction(() => this.repo.close(id));
  }

  cancel(): void {
    const id = this.docId();
    if (!id) return;
    if (!confirm(this.t('bos.ord.confirmCancel'))) return;
    this.runAction(() => this.repo.cancel(id));
  }

  openConvert(): void {
    this.convertWarehouseId.set(this.warehouseId());
    this.convertDueDate.set('');
    this.convertPaymentMode.set(1);
    this.convertError.set(null);
    this.showConvertModal.set(true);
  }

  closeConvert(): void {
    this.showConvertModal.set(false);
  }

  confirmConvert(): void {
    const id = this.docId();
    if (!id) return;
    this.converting.set(true);
    this.convertError.set(null);
    this.repo
      .convertToInvoice(id, {
        paymentMode: this.convertPaymentMode(),
        warehouseId: this.convertWarehouseId() || null,
        dueDate: this.convertDueDate() || null
      })
      .subscribe({
        next: result => {
          this.converting.set(false);
          this.showConvertModal.set(false);
          void this.router.navigate(['/sales/invoices', result.invoiceId]);
        },
        error: err => {
          this.convertError.set(err?.error?.error ?? this.t('bos.ord.convert.failed'));
          this.converting.set(false);
        }
      });
  }

  private load(id: string): void {
    this.loading.set(true);
    this.repo.getById(id).subscribe({
      next: doc => {
        this.apply(doc);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('bos.ord.loadFailed'));
        this.loading.set(false);
      }
    });
  }

  private apply(doc: BackOfficeSalesOrder): void {
    this.docId.set(doc.id);
    this.orderNumber.set(doc.orderNumber);
    this.status.set(this.mapStatus(doc.status));
    this.customerId.set(doc.customerId);
    this.warehouseId.set(doc.warehouseId || null);
    this.orderDate.set(String(doc.orderDate).slice(0, 10));
    this.expectedDeliveryDate.set(doc.expectedDeliveryDate ? String(doc.expectedDeliveryDate).slice(0, 10) : '');
    this.notes.set(doc.notes || '');
    this.lines.set(
      (doc.lines || []).map(l => ({
        description: l.description,
        quantity: l.quantity,
        unitPrice: l.unitPrice,
        taxPercent: l.taxPercent,
        inventoryItemId: l.inventoryItemId || '',
        unitId: l.unitId || '',
        lineNature: Number(l.lineNature) || 1,
        unitCost: l.unitCost || 0
      }))
    );
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
        this.error.set(err?.error?.error ?? this.t('bos.ord.actionFailed'));
        this.saving.set(false);
      }
    });
  }
}
