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
import { BackOfficeSalesInvoiceRepository } from '../../../core/repositories/back-office-sales-invoice.repository';
import {
  BackOfficeSalesInvoice,
  CreateBackOfficeSalesInvoiceLineInput
} from '../../../core/models/back-office-sales-invoice.models';
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
  selector: 'app-sales-invoice-form-page',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule],
  templateUrl: './sales-invoice-form.page.html',
  styleUrl: './sales-invoice-form.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SalesInvoiceFormPage implements OnInit {
  private repo = inject(BackOfficeSalesInvoiceRepository);
  private inventory = inject(InventoryService);
  private crm = inject(CrmRepository);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  lang = inject(LanguageService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  docId = signal<string | null>(null);
  invoiceNumber = signal('');
  status = signal('Draft');
  customerId = signal<string | null>(null);
  warehouseId = signal<string | null>(null);
  invoiceDate = signal(new Date().toISOString().slice(0, 10));
  dueDate = signal('');
  paymentMode = signal(1);
  nature = signal(1);
  currency = signal('SAR');
  notes = signal('');
  lines = signal<LineDraft[]>([]);

  customers = signal<CrmCustomerSummary[]>([]);
  warehouses = signal<Warehouse[]>([]);
  items = signal<InventoryItemDefinition[]>([]);
  units = signal<InventoryUnit[]>([]);

  isDraft = computed(() => this.status() === 'Draft');
  isApproved = computed(() => this.status() === 'Approved');
  isPosted = computed(() => this.status() === 'Posted');
  isNew = computed(() => !this.docId());
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

  onCustomerChange(id: string | null): void {
    this.customerId.set(id);
    const c = this.customers().find(x => x.id === id);
    if (!c || !this.isDraft()) return;
    if (c.currency) this.currency.set(c.currency);
    if ((c.paymentDueDays ?? 0) > 0 && this.paymentMode() === 1) {
      const d = new Date(this.invoiceDate());
      d.setDate(d.getDate() + (c.paymentDueDays ?? 0));
      this.dueDate.set(d.toISOString().slice(0, 10));
    }
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

  updateLine(index: number, patch: Partial<LineDraft>): void {
    this.lines.update(list => list.map((x, idx) => (idx === index ? { ...x, ...patch } : x)));
  }

  removeLine(index: number): void {
    if (!this.isDraft()) return;
    this.lines.set(this.lines().filter((_, i) => i !== index));
  }

  back(): void {
    void this.router.navigate(['/sales/invoices']);
  }

  save(): void {
    if (!this.isDraft()) return;
    if (!this.customerId()) {
      this.error.set(this.t('bos.inv.validation.customer'));
      return;
    }
    const active = this.lines().filter(l => l.description.trim() && Number(l.quantity) > 0);
    if (active.length === 0) {
      this.error.set(this.t('bos.inv.validation.lines'));
      return;
    }
    if (this.nature() === 1 && !this.warehouseId()) {
      this.error.set(this.t('bos.inv.validation.warehouse'));
      return;
    }

    const linePayload: CreateBackOfficeSalesInvoiceLineInput[] = active.map(l => ({
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
          invoiceDate: this.invoiceDate(),
          paymentMode: this.paymentMode(),
          nature: this.nature(),
          dueDate: this.dueDate() || null,
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
            this.error.set(err?.error?.error ?? this.t('bos.inv.saveFailed'));
            this.saving.set(false);
          }
        });
      return;
    }

    this.repo
      .create({
        customerId: this.customerId()!,
        invoiceDate: this.invoiceDate(),
        paymentMode: this.paymentMode(),
        nature: this.nature(),
        currency: this.currency(),
        warehouseId: this.warehouseId() || null,
        dueDate: this.dueDate() || null,
        notes: this.notes() || null,
        lines: linePayload
      })
      .subscribe({
        next: doc => {
          this.saving.set(false);
          void this.router.navigate(['/sales/invoices', doc.id]);
        },
        error: err => {
          this.error.set(err?.error?.error ?? this.t('bos.inv.saveFailed'));
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

  private load(id: string): void {
    this.loading.set(true);
    this.repo.getById(id).subscribe({
      next: doc => {
        this.apply(doc);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('bos.inv.loadFailed'));
        this.loading.set(false);
      }
    });
  }

  private apply(doc: BackOfficeSalesInvoice): void {
    this.docId.set(doc.id);
    this.invoiceNumber.set(doc.invoiceNumber);
    this.status.set(this.mapStatus(doc.status));
    this.customerId.set(doc.customerId);
    this.warehouseId.set(doc.warehouseId || null);
    this.invoiceDate.set(String(doc.invoiceDate).slice(0, 10));
    this.dueDate.set(doc.dueDate ? String(doc.dueDate).slice(0, 10) : '');
    this.paymentMode.set(Number(doc.paymentMode) || 1);
    this.nature.set(Number(doc.nature) || 1);
    this.currency.set(doc.currency || 'SAR');
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
      case 8:
        return 'Reversed';
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
        this.error.set(err?.error?.error ?? this.t('bos.inv.actionFailed'));
        this.saving.set(false);
      }
    });
  }
}
