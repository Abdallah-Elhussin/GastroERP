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
import { BackOfficeSalesReturnRepository } from '../../../core/repositories/back-office-sales-return.repository';
import {
  BackOfficeSalesReturn,
  CreateBackOfficeSalesReturnLineInput
} from '../../../core/models/back-office-sales-return.models';
import { BackOfficeSalesInvoiceRepository } from '../../../core/repositories/back-office-sales-invoice.repository';
import {
  BackOfficeSalesInvoice,
  BackOfficeSalesInvoiceLine
} from '../../../core/models/back-office-sales-invoice.models';
import { Warehouse } from '../../../core/models/inventory.models';

interface LineDraft {
  invoiceLineId: string;
  description: string;
  quantity: number;
  unitPrice: number;
  taxPercent: number;
  inventoryItemId: string | null;
  unitId: string | null;
  maxQuantity: number;
}

@Component({
  selector: 'app-sales-return-form-page',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule],
  templateUrl: './sales-return-form.page.html',
  styleUrl: './sales-return-form.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SalesReturnFormPage implements OnInit {
  private repo = inject(BackOfficeSalesReturnRepository);
  private invoiceRepo = inject(BackOfficeSalesInvoiceRepository);
  private inventory = inject(InventoryService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  lang = inject(LanguageService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  docId = signal<string | null>(null);
  returnNumber = signal('');
  status = signal('Draft');
  invoiceId = signal<string | null>(null);
  customerId = signal<string | null>(null);
  warehouseId = signal<string | null>(null);
  returnDate = signal(new Date().toISOString().slice(0, 10));
  notes = signal('');
  lines = signal<LineDraft[]>([]);

  invoices = signal<BackOfficeSalesInvoice[]>([]);
  warehouses = signal<Warehouse[]>([]);
  selectedInvoice = signal<BackOfficeSalesInvoice | null>(null);
  invoiceLoading = signal(false);

  isDraft = computed(() => this.status() === 'Draft');
  isApproved = computed(() => this.status() === 'Approved');
  isPosted = computed(() => this.status() === 'Posted');
  isCancelled = computed(() => this.status() === 'Cancelled');
  isNew = computed(() => !this.docId());
  availableInvoiceLines = computed<BackOfficeSalesInvoiceLine[]>(() => {
    const invoice = this.selectedInvoice();
    if (!invoice) return [];
    return invoice.lines.filter(
      l => (l.remainingToReturn ?? l.quantity) > 0 || this.lines().some(x => x.invoiceLineId === l.id)
    );
  });
  total = computed(() =>
    this.lines().reduce((s, l) => {
      const net = (Number(l.quantity) || 0) * (Number(l.unitPrice) || 0);
      const tax = net * ((Number(l.taxPercent) || 0) / 100);
      return s + net + tax;
    }, 0)
  );

  ngOnInit(): void {
    this.inventory.loadWarehouses();
    this.invoiceRepo
      .getList({ pageSize: 500, status: 2 })
      .pipe(catchError(() => of([] as BackOfficeSalesInvoice[])))
      .subscribe(rows => this.invoices.set(rows));

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

  onInvoiceChange(invoiceId: string | null): void {
    this.invoiceId.set(invoiceId);
    this.selectedInvoice.set(null);
    this.lines.set([]);
    if (!invoiceId) return;
    this.invoiceLoading.set(true);
    this.invoiceRepo.getById(invoiceId).subscribe({
      next: invoice => {
        this.selectedInvoice.set(invoice);
        this.customerId.set(invoice.customerId);
        if (!this.warehouseId() && invoice.warehouseId) this.warehouseId.set(invoice.warehouseId);
        this.invoiceLoading.set(false);
      },
      error: () => {
        this.error.set(this.t('bos.ret.invoiceLoadFailed'));
        this.invoiceLoading.set(false);
      }
    });
  }

  onInvoiceLineChange(index: number, invoiceLineId: string): void {
    const line = this.selectedInvoice()?.lines.find(l => l.id === invoiceLineId);
    this.lines.update(list =>
      list.map((x, idx) => {
        if (idx !== index) return x;
        const remaining = line?.remainingToReturn ?? line?.quantity ?? 0;
        return {
          ...x,
          invoiceLineId,
          description: line?.description || x.description,
          inventoryItemId: line?.inventoryItemId || null,
          unitId: line?.unitId || null,
          unitPrice: line?.unitPrice || 0,
          taxPercent: line?.taxPercent || 0,
          maxQuantity: remaining,
          quantity: remaining || x.quantity
        };
      })
    );
  }

  addLine(): void {
    if (!this.isDraft()) return;
    this.lines.set([
      ...this.lines(),
      {
        invoiceLineId: '',
        description: '',
        quantity: 1,
        unitPrice: 0,
        taxPercent: 0,
        inventoryItemId: null,
        unitId: null,
        maxQuantity: 0
      }
    ]);
  }

  removeLine(index: number): void {
    this.lines.set(this.lines().filter((_, i) => i !== index));
  }

  updateLine(index: number, patch: Partial<LineDraft>): void {
    this.lines.update(list => list.map((x, idx) => (idx === index ? { ...x, ...patch } : x)));
  }

  back(): void {
    void this.router.navigate(['/sales/returns']);
  }

  save(): void {
    if (!this.isDraft()) return;
    if (!this.invoiceId() || !this.customerId()) {
      this.error.set(this.t('bos.ret.validation.invoice'));
      return;
    }
    const active = this.lines().filter(l => l.invoiceLineId && Number(l.quantity) > 0);
    if (active.length === 0) {
      this.error.set(this.t('bos.ret.validation.lines'));
      return;
    }

    const linePayload: CreateBackOfficeSalesReturnLineInput[] = active.map(l => ({
      invoiceLineId: l.invoiceLineId,
      description: l.description,
      quantity: Number(l.quantity),
      unitPrice: Number(l.unitPrice),
      taxPercent: Number(l.taxPercent) || 0,
      inventoryItemId: l.inventoryItemId,
      unitId: l.unitId
    }));

    this.saving.set(true);
    this.error.set(null);

    if (this.docId()) {
      this.repo
        .update(this.docId()!, {
          returnDate: this.returnDate(),
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
            this.error.set(err?.error?.error ?? this.t('bos.ret.saveFailed'));
            this.saving.set(false);
          }
        });
      return;
    }

    this.repo
      .create({
        invoiceId: this.invoiceId()!,
        customerId: this.customerId()!,
        returnDate: this.returnDate(),
        warehouseId: this.warehouseId() || null,
        notes: this.notes() || null,
        lines: linePayload
      })
      .subscribe({
        next: doc => {
          this.saving.set(false);
          void this.router.navigate(['/sales/returns', doc.id]);
        },
        error: err => {
          this.error.set(err?.error?.error ?? this.t('bos.ret.saveFailed'));
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
    if (!confirm(this.t('bos.ret.confirmCancel'))) return;
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
        this.error.set(err?.error?.error ?? this.t('bos.ret.loadFailed'));
        this.loading.set(false);
      }
    });
  }

  private apply(doc: BackOfficeSalesReturn): void {
    this.docId.set(doc.id);
    this.returnNumber.set(doc.returnNumber);
    this.status.set(this.mapStatus(doc.status));
    this.invoiceId.set(doc.invoiceId);
    this.customerId.set(doc.customerId);
    this.warehouseId.set(doc.warehouseId || null);
    this.returnDate.set(String(doc.returnDate).slice(0, 10));
    this.notes.set(doc.notes || '');
    this.lines.set(
      (doc.lines || []).map(l => ({
        invoiceLineId: l.invoiceLineId,
        description: l.description,
        quantity: l.quantity,
        unitPrice: l.unitPrice,
        taxPercent: l.taxPercent,
        inventoryItemId: l.inventoryItemId || null,
        unitId: l.unitId || null,
        maxQuantity: l.quantity
      }))
    );
    if (doc.invoiceId && !this.selectedInvoice()) {
      this.invoiceRepo.getById(doc.invoiceId).subscribe({ next: invoice => this.selectedInvoice.set(invoice) });
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
        this.error.set(err?.error?.error ?? this.t('bos.ret.actionFailed'));
        this.saving.set(false);
      }
    });
  }
}
