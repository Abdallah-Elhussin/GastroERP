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
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { IssueDestinationRepository } from '../../../core/repositories/issue-destination.repository';
import {
  ISSUE_DESTINATION_TYPES,
  IssueDestination,
  UpsertIssueDestinationPayload
} from '../../../core/models/issue-destination.models';
import { CostCenterLookup } from '../../../core/models/inventory-valuation-group.models';
import { AccountLookup } from '../../../core/models/opening-balance.models';
import { InventoryPageShellComponent } from '../shared/inventory-page-shell.component';
import { InventoryEmptyStateComponent } from '../shared/inventory-empty-state.component';
import { InventoryErrorStateComponent } from '../shared/inventory-error-state.component';
import { InventorySkeletonComponent } from '../shared/inventory-skeleton.component';

type FormTab = 'general' | 'accounting' | 'policies' | 'status';

@Component({
  selector: 'app-inventory-issue-destinations-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatTooltipModule,
    InventoryPageShellComponent,
    InventoryEmptyStateComponent,
    InventoryErrorStateComponent,
    InventorySkeletonComponent
  ],
  templateUrl: './inventory-issue-destinations.page.html',
  styleUrl: './inventory-issue-destinations.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryIssueDestinationsPage implements OnInit {
  private repo = inject(IssueDestinationRepository);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  rows = signal<IssueDestination[]>([]);
  selectedId = signal<string | null>(null);
  search = signal('');
  showModal = signal(false);
  formTab = signal<FormTab>('general');
  editingId = signal<string | null>(null);

  code = signal('');
  nameAr = signal('');
  nameEn = signal('');
  description = signal('');
  destinationType = signal(13);
  defaultGlAccountId = signal<string | null>(null);
  defaultCostCenterId = signal<string | null>(null);
  allowChangeAccountOnIssue = signal(true);
  requireEmployee = signal(false);
  requireProject = signal(false);
  requireCostCenter = signal(false);
  requireBranch = signal(false);
  requireReason = signal(false);
  requireApproval = signal(false);
  allowDirectIssue = signal(true);
  allowNegativeStock = signal(false);
  isActive = signal(true);
  sortOrder = signal(0);

  accounts = signal<AccountLookup[]>([]);
  costCenters = signal<CostCenterLookup[]>([]);
  types = ISSUE_DESTINATION_TYPES;

  breadcrumbs = [
    { labelKey: 'nav.inventory', path: '/inventory/dashboard' },
    { labelKey: 'inv.nav.issueDestinations' }
  ];

  canView = computed(() =>
    this.auth.hasPermission('Inventory.IssueDestinations.View') || this.auth.hasPermission('Inventory.View')
  );
  canCreate = computed(() =>
    this.auth.hasPermission('Inventory.IssueDestinations.Create') || this.auth.hasPermission('Inventory.Manage')
  );
  canEdit = computed(() =>
    this.auth.hasPermission('Inventory.IssueDestinations.Edit') || this.auth.hasPermission('Inventory.Manage')
  );
  canDelete = computed(() =>
    this.auth.hasPermission('Inventory.IssueDestinations.Delete') || this.auth.hasPermission('Inventory.Manage')
  );
  selected = computed(() => this.rows().find(r => r.id === this.selectedId()) ?? null);
  isEditing = computed(() => !!this.editingId());

  ngOnInit(): void {
    this.load();
    this.repo.getAccounts().subscribe({ next: a => this.accounts.set(a), error: () => this.accounts.set([]) });
    this.repo.getCostCenters().subscribe({ next: c => this.costCenters.set(c), error: () => this.costCenters.set([]) });
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo.getList({ activeOnly: false, search: this.search().trim() || undefined }).subscribe({
      next: rows => {
        this.rows.set(rows);
        this.loading.set(false);
        if (this.selectedId() && !rows.some(r => r.id === this.selectedId())) {
          this.selectedId.set(null);
        }
      },
      error: err => {
        this.error.set(err?.error?.error ?? this.t('inv.dest.loadFailed'));
        this.loading.set(false);
      }
    });
  }

  select(row: IssueDestination): void {
    this.selectedId.set(row.id);
  }

  openCreate(): void {
    if (!this.canCreate()) return;
    this.resetForm();
    this.showModal.set(true);
    this.formTab.set('general');
  }

  openEdit(): void {
    const row = this.selected();
    if (!row || !this.canEdit()) return;
    this.applyRow(row);
    this.showModal.set(true);
    this.formTab.set('general');
  }

  closeModal(): void {
    this.showModal.set(false);
    this.resetForm();
  }

  save(): void {
    if (!this.nameAr().trim()) {
      this.error.set(this.t('inv.dest.validation'));
      return;
    }
    if (!this.editingId() && !this.code().trim()) {
      this.error.set(this.t('inv.dest.validation'));
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    const payload: UpsertIssueDestinationPayload = {
      nameAr: this.nameAr(),
      nameEn: this.nameEn() || null,
      description: this.description() || null,
      destinationType: this.destinationType(),
      defaultGlAccountId: this.defaultGlAccountId(),
      defaultCostCenterId: this.defaultCostCenterId(),
      allowChangeAccountOnIssue: this.allowChangeAccountOnIssue(),
      requireEmployee: this.requireEmployee(),
      requireProject: this.requireProject(),
      requireCostCenter: this.requireCostCenter(),
      requireBranch: this.requireBranch(),
      requireReason: this.requireReason(),
      requireApproval: this.requireApproval(),
      allowDirectIssue: this.allowDirectIssue(),
      allowNegativeStock: this.allowNegativeStock(),
      sortOrder: this.sortOrder(),
      isActive: this.isActive()
    };

    const done = () => {
      this.saving.set(false);
      this.closeModal();
      this.load();
    };
    const fail = (err: any) => {
      this.error.set(err?.error?.error ?? this.t('inv.dest.saveFailed'));
      this.saving.set(false);
    };

    if (this.editingId()) {
      this.repo.update(this.editingId()!, payload).subscribe({ next: done, error: fail });
      return;
    }

    this.repo.create({ ...payload, code: this.code() }).subscribe({ next: done, error: fail });
  }

  remove(): void {
    const row = this.selected();
    if (!row || !this.canDelete()) return;
    if (row.isSystem) {
      this.error.set(this.t('inv.dest.systemNoDelete'));
      return;
    }
    if (!confirm(this.t('inv.dest.confirmDelete'))) return;
    this.repo.delete(row.id).subscribe({
      next: () => {
        this.selectedId.set(null);
        this.load();
      },
      error: err => this.error.set(err?.error?.error ?? this.t('inv.dest.saveFailed'))
    });
  }

  typeLabel(code: number): string {
    const found = this.types.find(t => t.value === code);
    return found ? this.t(found.labelKey) : String(code);
  }

  private resetForm(): void {
    this.editingId.set(null);
    this.code.set('');
    this.nameAr.set('');
    this.nameEn.set('');
    this.description.set('');
    this.destinationType.set(13);
    this.defaultGlAccountId.set(null);
    this.defaultCostCenterId.set(null);
    this.allowChangeAccountOnIssue.set(true);
    this.requireEmployee.set(false);
    this.requireProject.set(false);
    this.requireCostCenter.set(false);
    this.requireBranch.set(false);
    this.requireReason.set(false);
    this.requireApproval.set(false);
    this.allowDirectIssue.set(true);
    this.allowNegativeStock.set(false);
    this.isActive.set(true);
    this.sortOrder.set(0);
  }

  private applyRow(row: IssueDestination): void {
    this.editingId.set(row.id);
    this.code.set(row.code);
    this.nameAr.set(row.nameAr);
    this.nameEn.set(row.nameEn || '');
    this.description.set(row.description || '');
    this.destinationType.set(row.destinationTypeCode || 13);
    this.defaultGlAccountId.set(row.defaultGlAccountId ?? null);
    this.defaultCostCenterId.set(row.defaultCostCenterId ?? null);
    this.allowChangeAccountOnIssue.set(row.allowChangeAccountOnIssue);
    this.requireEmployee.set(row.requireEmployee);
    this.requireProject.set(row.requireProject);
    this.requireCostCenter.set(row.requireCostCenter);
    this.requireBranch.set(row.requireBranch);
    this.requireReason.set(row.requireReason);
    this.requireApproval.set(row.requireApproval);
    this.allowDirectIssue.set(row.allowDirectIssue);
    this.allowNegativeStock.set(row.allowNegativeStock);
    this.isActive.set(row.isActive);
    this.sortOrder.set(row.sortOrder);
  }
}
