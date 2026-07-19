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
import { CrmRepository } from '../../../core/repositories/crm.repository';
import { CrmCustomerSummary } from '../../../core/repositories/rest-crm.repository';
import { BackOfficeSalesDebitNoteRepository } from '../../../core/repositories/back-office-sales-debit-note.repository';
import {
  BackOfficeSalesDebitNote,
  CreateBackOfficeSalesDebitNoteLineInput
} from '../../../core/models/back-office-sales-debit-note.models';
import { BackOfficeSalesInvoiceRepository } from '../../../core/repositories/back-office-sales-invoice.repository';
import { BackOfficeSalesInvoice } from '../../../core/models/back-office-sales-invoice.models';

interface LineDraft {
  description: string;
  quantity: number;
  unitPrice: number;
  taxPercent: number;
}

@Component({
  selector: 'app-sales-debit-note-form-page',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule],
  templateUrl: './sales-debit-note-form.page.html',
  styleUrl: './sales-debit-note-form.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SalesDebitNoteFormPage implements OnInit {
  private repo = inject(BackOfficeSalesDebitNoteRepository);
  private invoiceRepo = inject(BackOfficeSalesInvoiceRepository);
  private crm = inject(CrmRepository);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  lang = inject(LanguageService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  docId = signal<string | null>(null);
  debitNoteNumber = signal('');
  status = signal('Draft');
  customerId = signal<string | null>(null);
  invoiceId = signal<string | null>(null);
  debitDate = signal(new Date().toISOString().slice(0, 10));
  currency = signal('SAR');
  notes = signal('');
  lines = signal<LineDraft[]>([]);

  customers = signal<CrmCustomerSummary[]>([]);
  invoices = signal<BackOfficeSalesInvoice[]>([]);

  isDraft = computed(() => this.status() === 'Draft');
  isApproved = computed(() => this.status() === 'Approved');
  isPosted = computed(() => this.status() === 'Posted');
  isCancelled = computed(() => this.status() === 'Cancelled');
  isNew = computed(() => !this.docId());
  total = computed(() =>
    this.lines().reduce((s, l) => {
      const net = (Number(l.quantity) || 0) * (Number(l.unitPrice) || 0);
      const tax = net * ((Number(l.taxPercent) || 0) / 100);
      return s + net + tax;
    }, 0)
  );

  ngOnInit(): void {
    this.crm
      .getCustomers(1, 500)
      .pipe(catchError(() => of([] as CrmCustomerSummary[])))
      .subscribe(rows => this.customers.set(rows));
    this.invoiceRepo
      .getList({ pageSize: 500 })
      .pipe(catchError(() => of([] as BackOfficeSalesInvoice[])))
      .subscribe(rows => this.invoices.set(rows));

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

  addLine(): void {
    if (!this.isDraft()) return;
    this.lines.set([...this.lines(), { description: '', quantity: 1, unitPrice: 0, taxPercent: 0 }]);
  }

  removeLine(index: number): void {
    if (!this.isDraft()) return;
    this.lines.set(this.lines().filter((_, i) => i !== index));
  }

  updateLine(index: number, patch: Partial<LineDraft>): void {
    this.lines.update(list => list.map((x, idx) => (idx === index ? { ...x, ...patch } : x)));
  }

  back(): void {
    void this.router.navigate(['/sales/debit-notes']);
  }

  save(): void {
    if (!this.isDraft()) return;
    if (!this.customerId()) {
      this.error.set(this.t('bos.dnte.validation.customer'));
      return;
    }
    const active = this.lines().filter(l => l.description.trim() && Number(l.quantity) > 0);
    if (active.length === 0) {
      this.error.set(this.t('bos.dnte.validation.lines'));
      return;
    }

    const linePayload: CreateBackOfficeSalesDebitNoteLineInput[] = active.map(l => ({
      description: l.description,
      quantity: Number(l.quantity),
      unitPrice: Number(l.unitPrice),
      taxPercent: Number(l.taxPercent) || 0
    }));

    this.saving.set(true);
    this.error.set(null);

    if (this.docId()) {
      this.repo
        .update(this.docId()!, {
          debitDate: this.debitDate(),
          invoiceId: this.invoiceId() || null,
          notes: this.notes() || null,
          lines: linePayload
        })
        .subscribe({
          next: doc => {
            this.apply(doc);
            this.saving.set(false);
          },
          error: err => {
            this.error.set(err?.error?.error ?? this.t('bos.dnte.saveFailed'));
            this.saving.set(false);
          }
        });
      return;
    }

    this.repo
      .create({
        customerId: this.customerId()!,
        debitDate: this.debitDate(),
        currency: this.currency(),
        invoiceId: this.invoiceId() || null,
        notes: this.notes() || null,
        lines: linePayload
      })
      .subscribe({
        next: doc => {
          this.saving.set(false);
          void this.router.navigate(['/sales/debit-notes', doc.id]);
        },
        error: err => {
          this.error.set(err?.error?.error ?? this.t('bos.dnte.saveFailed'));
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
    if (!confirm(this.t('bos.dnte.confirmCancel'))) return;
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
        this.error.set(err?.error?.error ?? this.t('bos.dnte.loadFailed'));
        this.loading.set(false);
      }
    });
  }

  private apply(doc: BackOfficeSalesDebitNote): void {
    this.docId.set(doc.id);
    this.debitNoteNumber.set(doc.debitNoteNumber);
    this.status.set(this.mapStatus(doc.status));
    this.customerId.set(doc.customerId);
    this.invoiceId.set(doc.invoiceId || null);
    this.debitDate.set(String(doc.debitDate).slice(0, 10));
    this.currency.set(doc.currency || 'SAR');
    this.notes.set(doc.notes || '');
    this.lines.set(
      (doc.lines || []).map(l => ({
        description: l.description,
        quantity: l.quantity,
        unitPrice: l.unitPrice,
        taxPercent: l.taxPercent
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
        this.error.set(err?.error?.error ?? this.t('bos.dnte.actionFailed'));
        this.saving.set(false);
      }
    });
  }
}
