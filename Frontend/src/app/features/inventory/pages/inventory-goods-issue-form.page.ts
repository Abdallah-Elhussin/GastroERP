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
import { GoodsIssueRepository } from '../../../core/repositories/goods-issue.repository';
import {
  CreateGoodsIssuePayload,
  GoodsIssueLine,
  IssueDestination,
  UpdateGoodsIssuePayload
} from '../../../core/models/goods-issue.models';
import { CostCenterLookup } from '../../../core/models/inventory-valuation-group.models';
import { InventoryItemDefinition, InventoryUnit, Warehouse } from '../../../core/models/inventory.models';
import { InventoryPageShellComponent } from '../shared/inventory-page-shell.component';

@Component({
  selector: 'app-inventory-goods-issue-form-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, MatIconModule, InventoryPageShellComponent],
  templateUrl: './inventory-goods-issue-form.page.html',
  styleUrl: './inventory-goods-issue-form.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryGoodsIssueFormPage implements OnInit {
  private repo = inject(GoodsIssueRepository);
  private inventory = inject(InventoryService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);

  docId = signal<string | null>(null);
  issueNumber = signal('');
  issueDate = signal(this.todayIso());
  approvalDate = signal<string | null>(null);
  status = signal('Draft');
  currency = signal('SAR');
  notes = signal('');
  defaultWarehouseId = signal<string | null>(null);
  issueDestinationId = signal<string | null>(null);
  lines = signal<GoodsIssueLine[]>([]);
  selectedLineIndex = signal<number | null>(null);

  items = signal<InventoryItemDefinition[]>([]);
  units = signal<InventoryUnit[]>([]);
  warehouses = signal<Warehouse[]>([]);
  destinations = signal<IssueDestination[]>([]);
  costCenters = signal<CostCenterLookup[]>([]);

  breadcrumbs = [
    { labelKey: 'nav.inventory', path: '/inventory/dashboard' },
    { labelKey: 'inv.nav.goodsIssue', path: '/inventory/goods-issues' },
    { labelKey: 'inv.gi.formBreadcrumb' }
  ];

  canManage = computed(() => this.auth.hasPermission('Inventory.Manage'));
  isDraft = computed(() => this.status() === 'Draft');
  isApproved = computed(() => this.status() === 'Approved');
  isPosted = computed(() => this.status() === 'Posted');
  isNew = computed(() => !this.docId());
  pageTitle = computed(() =>
    this.isNew() ? this.t('inv.gi.createTitle') : `${this.t('inv.gi.editTitle')} — ${this.issueNumber()}`
  );
  statusLabel = computed(() => {
    switch (this.status()) {
      case 'Approved':
        return this.t('inv.gi.status.approved');
      case 'Posted':
        return this.t('inv.gi.status.posted');
      case 'Cancelled':
        return this.t('inv.gi.status.cancelled');
      default:
        return this.t('inv.gi.status.draft');
    }
  });
  totalAmount = computed(() => this.lines().reduce((s, l) => s + l.quantity * l.unitCost, 0));

  ngOnInit(): void {
    this.inventory.loadWarehouses();
    this.inventory.loadItems();
    this.inventory.loadUnits();

    const pull = () => {
      this.warehouses.set([...this.inventory.warehouses()]);
      this.items.set([...this.inventory.items()]);
      this.units.set([...this.inventory.units()]);
      if (!this.defaultWarehouseId() && this.warehouses().length) {
        this.defaultWarehouseId.set(this.warehouses()[0].id);
      }
    };
    pull();
    const timer = setInterval(pull, 500);
    setTimeout(() => clearInterval(timer), 5000);

    this.repo.getDestinations().subscribe({
      next: d => this.destinations.set(d ?? []),
      error: () => this.destinations.set([])
    });
    this.repo.getCostCenters().subscribe({
      next: c => this.costCenters.set(c ?? []),
      error: () => this.costCenters.set([])
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.loadDoc(id);
    } else {
      this.repo.nextNumber().subscribe({
        next: n => this.issueNumber.set(typeof n === 'string' ? n : String(n)),
        error: () => {
          const stamp = new Date().toISOString().replace(/\D/g, '').slice(0, 14);
          this.issueNumber.set(`GI${stamp}`);
        }
      });
    }
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  addLine(): void {
    if (!this.isDraft()) return;
    const wh = this.defaultWarehouseId() ?? this.warehouses()[0]?.id;
    const item = this.items()[0];
    const unit = this.units()[0];
    if (!wh || !item || !unit) {
      this.error.set(this.t('inv.gi.validation.masters'));
      return;
    }
    this.lines.update(rows => [
      ...rows,
      {
        inventoryItemId: item.id,
        unitId: item.baseUnitId || unit.id,
        warehouseId: wh,
        quantity: 1,
        unitCost: 0,
        costCenterId: null,
        notes: null
      }
    ]);
    this.selectedLineIndex.set(this.lines().length - 1);
  }

  removeLine(): void {
    const idx = this.selectedLineIndex();
    if (idx == null || !this.isDraft()) return;
    this.lines.update(rows => rows.filter((_, i) => i !== idx));
    this.selectedLineIndex.set(null);
  }

  selectLine(index: number): void {
    this.selectedLineIndex.set(index);
  }

  patchLine(index: number, patch: Partial<GoodsIssueLine>): void {
    this.lines.update(rows => rows.map((row, i) => (i === index ? { ...row, ...patch } : row)));
  }

  onItemChange(index: number, itemId: string): void {
    const item = this.items().find(i => i.id === itemId);
    this.patchLine(index, {
      inventoryItemId: itemId,
      unitId: item?.baseUnitId || this.lines()[index].unitId
    });
  }

  save(): void {
    if (!this.canManage() || !this.isDraft()) return;
    if (!this.issueDestinationId()) {
      this.error.set(this.t('inv.gi.validation.destination'));
      return;
    }
    if (this.lines().length === 0) {
      this.error.set(this.t('inv.gi.validation.lines'));
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    const linePayload = this.lines().map(l => ({
      inventoryItemId: l.inventoryItemId,
      unitId: l.unitId,
      quantity: l.quantity,
      unitCost: l.unitCost,
      warehouseId: l.warehouseId,
      costCenterId: l.costCenterId,
      notes: l.notes
    }));

    if (this.docId()) {
      const payload: UpdateGoodsIssuePayload = {
        issueDate: new Date(this.issueDate()).toISOString(),
        warehouseId: this.defaultWarehouseId(),
        issueDestinationId: this.issueDestinationId(),
        currency: this.currency(),
        notes: this.notes() || null,
        lines: linePayload
      };
      this.repo.update(this.docId()!, payload).subscribe({
        next: doc => {
          this.applyDoc(doc);
          this.saving.set(false);
        },
        error: err => {
          this.error.set(err?.error?.error ?? this.t('inv.gi.saveFailed'));
          this.saving.set(false);
        }
      });
      return;
    }

    const payload: CreateGoodsIssuePayload = {
      issueNumber: this.issueNumber(),
      autoGenerateNumber: false,
      issueDate: new Date(this.issueDate()).toISOString(),
      warehouseId: this.defaultWarehouseId(),
      issueDestinationId: this.issueDestinationId(),
      currency: this.currency(),
      notes: this.notes() || null,
      lines: linePayload
    };
    this.repo.create(payload).subscribe({
      next: doc => {
        this.applyDoc(doc);
        this.saving.set(false);
        void this.router.navigate(['/inventory/goods-issues', doc.id], { replaceUrl: true });
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('inv.gi.saveFailed'));
        this.saving.set(false);
      }
    });
  }

  post(): void {
    if (!this.docId() || !this.canManage()) return;
    const id = this.docId()!;
    this.saving.set(true);
    this.error.set(null);

    const afterApprove = () =>
      this.repo.post(id).subscribe({
        next: () => this.loadDoc(id),
        error: err => {
          this.error.set(err?.error?.error ?? this.t('inv.gi.actionFailed'));
          this.saving.set(false);
        }
      });

    if (this.isDraft()) {
      this.repo.approve(id).subscribe({
        next: () => afterApprove(),
        error: err => {
          this.error.set(err?.error?.error ?? this.t('inv.gi.actionFailed'));
          this.saving.set(false);
        }
      });
      return;
    }

    if (this.isApproved()) {
      afterApprove();
      return;
    }
    this.saving.set(false);
  }

  cancel(): void {
    void this.router.navigate(['/inventory/goods-issues']);
  }

  private loadDoc(id: string): void {
    this.loading.set(true);
    this.repo.getById(id).subscribe({
      next: doc => {
        this.applyDoc(doc);
        this.loading.set(false);
        this.saving.set(false);
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('inv.gi.loadFailed'));
        this.loading.set(false);
        this.saving.set(false);
      }
    });
  }

  private applyDoc(doc: import('../../../core/models/goods-issue.models').GoodsIssueDoc): void {
    this.docId.set(doc.id);
    this.issueNumber.set(doc.issueNumber);
    this.issueDate.set(doc.issueDate?.slice(0, 10) || this.todayIso());
    this.approvalDate.set(doc.approvalDate ? doc.approvalDate.slice(0, 10) : null);
    this.status.set(doc.status);
    this.currency.set(doc.currency || 'SAR');
    this.notes.set(doc.notes || '');
    this.defaultWarehouseId.set(doc.warehouseId ?? null);
    this.issueDestinationId.set(doc.issueDestinationId ?? null);
    this.lines.set(
      (doc.lines ?? []).map(l => ({
        id: l.id,
        inventoryItemId: l.inventoryItemId,
        itemNameAr: l.itemNameAr,
        itemSku: l.itemSku,
        unitId: l.unitId,
        unitNameAr: l.unitNameAr,
        warehouseId: l.warehouseId,
        warehouseNameAr: l.warehouseNameAr,
        quantity: l.quantity,
        unitCost: l.unitCost,
        totalCost: l.totalCost,
        costCenterId: l.costCenterId,
        costCenterNameAr: l.costCenterNameAr,
        notes: l.notes
      }))
    );
  }

  private todayIso(): string {
    return new Date().toISOString().slice(0, 10);
  }
}
