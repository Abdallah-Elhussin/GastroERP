import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal
} from '@angular/core';
import { CommonModule, NgTemplateOutlet } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';
import { ChartOfAccountRepository } from '../../../core/repositories/chart-of-account.repository';
import { AccountClassificationRepository } from '../../../core/repositories/account-classification.repository';
import { AccountClassification } from '../../../core/models/account-classification.models';
import {
  ACCOUNT_CATEGORIES,
  ACCOUNT_TYPES,
  AccountTreeNode,
  AccountTypeCode,
  ChartAccount,
  UpsertAccountPayload
} from '../../../core/models/chart-of-account.models';

@Component({
  selector: 'app-chart-of-accounts-page',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatTooltipModule, NgTemplateOutlet],
  templateUrl: './chart-of-accounts.page.html',
  styleUrl: './chart-of-accounts.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChartOfAccountsPage implements OnInit {
  private repo = inject(ChartOfAccountRepository);
  private classRepo = inject(AccountClassificationRepository);
  lang = inject(LanguageService);
  auth = inject(AuthService);

  loading = signal(false);
  saving = signal(false);
  error = signal<string | null>(null);
  tree = signal<AccountTreeNode[]>([]);
  classifications = signal<AccountClassification[]>([]);
  selectedId = signal<string | null>(null);
  search = signal('');
  activeType = signal<AccountTypeCode>(1);
  expanded = signal<Record<string, boolean>>({});
  editMode = signal(false);
  isCreate = signal(false);

  accountNumber = signal('');
  nameAr = signal('');
  nameEn = signal('');
  parentAccountId = signal<string | null>(null);
  accountType = signal<number>(1);
  accountCategory = signal(1);
  accountClassificationId = signal<string | null>(null);
  currency = signal('SAR');
  sortOrder = signal(0);
  isSummaryAccount = signal(false);
  isActive = signal(true);
  isSystemAccount = signal(false);
  isPostingAllowed = signal(true);
  notes = signal('');
  renumberValue = signal('');

  types = ACCOUNT_TYPES;
  categories = ACCOUNT_CATEGORIES;

  canView = computed(
    () =>
      this.auth.hasPermission('Accounting.View') ||
      this.auth.hasPermission('VIEW_FINANCE')
  );
  canCreate = computed(
    () =>
      this.auth.hasPermission('Accounting.Create') ||
      this.auth.hasPermission('Accounting.Update')
  );
  canEdit = computed(() => this.auth.hasPermission('Accounting.Update'));
  canDelete = computed(() => this.auth.hasPermission('Accounting.Delete'));

  selected = computed(() => {
    const id = this.selectedId();
    return id ? this.findNode(this.tree(), id) : null;
  });

  flatParents = computed(() => this.flatten(this.tree()).filter(n => n.id !== this.selectedId()));

  t = (key: string) => this.lang.t(key);

  filteredClassifications = computed(() =>
    this.classifications().filter(c => Number(c.accountType) === this.accountType())
  );

  ngOnInit(): void {
    this.classRepo.getList().subscribe({
      next: rows => this.classifications.set(rows),
      error: () => this.classifications.set([])
    });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.repo
      .getTree({
        accountType: this.activeType(),
        includeInactive: true,
        search: this.search()
      })
      .subscribe({
        next: nodes => {
          this.tree.set(nodes);
          this.expandAll(nodes);
          this.loading.set(false);
          const sel = this.selectedId();
          if (sel) {
            const node = this.findNode(nodes, sel);
            if (node) this.applyNode(node);
            else this.clearSelection();
          }
        },
        error: err => {
          this.error.set(err?.error?.detail || err?.message || this.t('fin.coa.loadError'));
          this.loading.set(false);
        }
      });
  }

  setType(type: AccountTypeCode): void {
    this.activeType.set(type);
    this.clearSelection();
    this.load();
  }

  select(node: AccountTreeNode): void {
    this.selectedId.set(node.id);
    this.editMode.set(false);
    this.isCreate.set(false);
    this.applyNode(node);
  }

  toggleExpand(id: string, event: Event): void {
    event.stopPropagation();
    this.expanded.update(m => ({ ...m, [id]: !m[id] }));
  }

  isExpanded(id: string): boolean {
    return this.expanded()[id] !== false;
  }

  openCreate(): void {
    if (!this.canCreate()) return;
    this.isCreate.set(true);
    this.editMode.set(true);
    this.selectedId.set(null);
    this.accountNumber.set('');
    this.nameAr.set('');
    this.nameEn.set('');
    this.parentAccountId.set(this.selected()?.id ?? null);
    this.accountType.set(this.activeType());
    this.accountCategory.set(this.defaultCategory(this.activeType()));
    this.accountClassificationId.set(null);
    this.currency.set('SAR');
    this.sortOrder.set(0);
    this.isSummaryAccount.set(false);
    this.isActive.set(true);
    this.isSystemAccount.set(false);
    this.isPostingAllowed.set(true);
    this.notes.set('');
    this.renumberValue.set('');
  }

  openEdit(): void {
    if (!this.canEdit() || !this.selected()) return;
    this.isCreate.set(false);
    this.editMode.set(true);
    this.renumberValue.set(this.accountNumber());
  }

  cancelEdit(): void {
    this.editMode.set(false);
    this.isCreate.set(false);
    const node = this.selected();
    if (node) this.applyNode(node);
  }

  save(): void {
    if (!this.editMode()) return;
    this.saving.set(true);
    this.error.set(null);

    if (this.isCreate()) {
      this.repo
        .create({
          accountNumber: this.accountNumber().trim(),
          nameAr: this.nameAr().trim(),
          nameEn: this.nameEn().trim() || null,
          accountType: this.accountType(),
          accountCategory: this.accountCategory(),
          parentAccountId: this.parentAccountId(),
          currency: this.currency(),
          isSummaryAccount: this.isSummaryAccount(),
          sortOrder: this.sortOrder(),
          notes: this.notes().trim() || null,
          accountClassificationId: this.accountClassificationId()
        })
        .subscribe({
          next: created => {
            this.saving.set(false);
            this.editMode.set(false);
            this.isCreate.set(false);
            this.selectedId.set(created.id);
            this.load();
          },
          error: err => this.fail(err)
        });
      return;
    }

    const id = this.selectedId();
    if (!id) return;

    const payload: UpsertAccountPayload = {
      nameAr: this.nameAr().trim(),
      nameEn: this.nameEn().trim() || null,
      accountCategory: this.accountCategory(),
      currency: this.currency(),
      isSummaryAccount: this.isSummaryAccount(),
      sortOrder: this.sortOrder(),
      notes: this.notes().trim() || null,
      accountClassificationId: this.accountClassificationId()
    };

    const ops: Array<ObservableLike> = [this.repo.update(id, payload)];

    const parentChanged = this.parentAccountId() !== (this.selected()?.parentAccountId ?? null);
    if (parentChanged) ops.push(this.repo.reparent(id, this.parentAccountId()));

    const newNumber = this.renumberValue().trim();
    if (newNumber && newNumber !== this.accountNumber() && !this.isSystemAccount()) {
      ops.push(this.repo.renumber(id, newNumber));
    }

    // chain sequentially
    const run = (i: number) => {
      if (i >= ops.length) {
        this.saving.set(false);
        this.editMode.set(false);
        this.load();
        return;
      }
      ops[i].subscribe({
        next: () => run(i + 1),
        error: err => this.fail(err)
      });
    };
    run(0);
  }

  remove(): void {
    const node = this.selected();
    if (!node || !this.canDelete() || node.isSystemAccount) return;
    if (!confirm(this.t('fin.coa.confirmDelete'))) return;
    this.saving.set(true);
    this.repo.delete(node.id).subscribe({
      next: () => {
        this.saving.set(false);
        this.clearSelection();
        this.load();
      },
      error: err => this.fail(err)
    });
  }

  activate(): void {
    const id = this.selectedId();
    if (!id || !this.canEdit()) return;
    this.repo.activate(id).subscribe({ next: () => this.load(), error: err => this.fail(err) });
  }

  deactivate(): void {
    const id = this.selectedId();
    if (!id || !this.canEdit()) return;
    this.repo.deactivate(id).subscribe({ next: () => this.load(), error: err => this.fail(err) });
  }

  downloadTemplate(): void {
    this.repo.downloadTemplate().subscribe(blob => this.downloadBlob(blob, 'chart-of-accounts-template.csv'));
  }

  exportExcel(): void {
    this.repo.exportCsv().subscribe(blob => this.downloadBlob(blob, 'chart-of-accounts.csv'));
  }

  onImportFile(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => {
      const text = String(reader.result ?? '');
      const rows = this.parseCsv(text);
      if (!rows.length) {
        this.error.set(this.t('fin.coa.importEmpty'));
        return;
      }
      this.saving.set(true);
      this.repo.import(rows, false).subscribe({
        next: preview => {
          if (preview.errors?.length) {
            this.error.set(preview.errors.slice(0, 5).join('\n'));
            this.saving.set(false);
            return;
          }
          if (!confirm(this.t('fin.coa.importConfirm').replace('{n}', String(preview.validRows)))) {
            this.saving.set(false);
            return;
          }
          this.repo.import(preview.rows, true).subscribe({
            next: () => {
              this.saving.set(false);
              this.load();
            },
            error: err => this.fail(err)
          });
        },
        error: err => this.fail(err)
      });
    };
    reader.readAsText(file);
    input.value = '';
  }

  onClassificationChange(id: string | null): void {
    this.accountClassificationId.set(id);
    const c = this.classifications().find(x => x.id === id);
    if (c) this.accountType.set(Number(c.accountType));
  }

  typeLabel(type: number): string {
    return this.t(ACCOUNT_TYPES.find(t => t.value === type)?.labelKey ?? 'fin.coa.type.asset');
  }

  categoryLabel(cat: number): string {
    return this.t(ACCOUNT_CATEGORIES.find(c => c.value === cat)?.labelKey ?? 'fin.coa.cat.currentAsset');
  }

  private applyNode(node: ChartAccount): void {
    this.accountNumber.set(node.accountNumber);
    this.nameAr.set(node.nameAr);
    this.nameEn.set(node.nameEn ?? '');
    this.parentAccountId.set(node.parentAccountId ?? null);
    this.accountType.set(Number(node.accountType));
    this.accountCategory.set(Number(node.accountCategory));
    this.accountClassificationId.set(node.accountClassificationId ?? null);
    this.currency.set(node.currency || 'SAR');
    this.sortOrder.set(node.sortOrder);
    this.isSummaryAccount.set(!!node.isSummaryAccount);
    this.isActive.set(!!node.isActive);
    this.isSystemAccount.set(!!node.isSystemAccount);
    this.isPostingAllowed.set(!!node.isPostingAllowed);
    this.notes.set(node.notes ?? '');
    this.renumberValue.set(node.accountNumber);
  }

  private clearSelection(): void {
    this.selectedId.set(null);
    this.editMode.set(false);
    this.isCreate.set(false);
  }

  private fail(err: unknown): void {
    this.saving.set(false);
    const e = err as { error?: { detail?: string; title?: string }; message?: string };
    this.error.set(e?.error?.detail || e?.error?.title || e?.message || this.t('fin.coa.saveError'));
  }

  private findNode(nodes: AccountTreeNode[], id: string): AccountTreeNode | null {
    for (const n of nodes) {
      if (n.id === id) return n;
      const child = this.findNode(n.children ?? [], id);
      if (child) return child;
    }
    return null;
  }

  private flatten(nodes: AccountTreeNode[]): AccountTreeNode[] {
    const out: AccountTreeNode[] = [];
    const walk = (list: AccountTreeNode[]) => {
      for (const n of list) {
        out.push(n);
        if (n.children?.length) walk(n.children);
      }
    };
    walk(nodes);
    return out;
  }

  private expandAll(nodes: AccountTreeNode[]): void {
    const map: Record<string, boolean> = { ...this.expanded() };
    const walk = (list: AccountTreeNode[]) => {
      for (const n of list) {
        if (map[n.id] === undefined) map[n.id] = true;
        if (n.children?.length) walk(n.children);
      }
    };
    walk(nodes);
    this.expanded.set(map);
  }

  private defaultCategory(type: AccountTypeCode): number {
    switch (type) {
      case 1: return 1;
      case 2: return 3;
      case 3: return 5;
      case 4: return 6;
      case 5: return 9;
      default: return 1;
    }
  }

  private downloadBlob(blob: Blob, name: string): void {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = name;
    a.click();
    URL.revokeObjectURL(url);
  }

  private parseCsv(text: string): import('../../../core/models/chart-of-account.models').AccountImportRow[] {
    const lines = text.replace(/^\uFEFF/, '').split(/\r?\n/).filter(l => l.trim());
    if (lines.length < 2) return [];
    const headers = this.splitCsvLine(lines[0]).map(h => h.trim().toLowerCase());
    const idx = (name: string) => headers.indexOf(name.toLowerCase());
    const rows = [];
    for (let i = 1; i < lines.length; i++) {
      const cols = this.splitCsvLine(lines[i]);
      const get = (name: string) => cols[idx(name)]?.trim() ?? '';
      const number = get('accountnumber');
      const nameAr = get('namear');
      if (!number || !nameAr) continue;
      rows.push({
        accountNumber: number,
        nameAr,
        nameEn: get('nameen') || null,
        parentAccountNumber: get('parentaccountnumber') || null,
        accountType: Number(get('accounttype') || this.activeType()),
        accountCategory: Number(get('accountcategory') || this.defaultCategory(this.activeType())),
        currency: get('currency') || 'SAR',
        isSummaryAccount: /^(1|true|yes)$/i.test(get('issummaryaccount')),
        sortOrder: Number(get('sortorder') || 0),
        notes: get('notes') || null
      });
    }
    return rows;
  }

  private splitCsvLine(line: string): string[] {
    const result: string[] = [];
    let cur = '';
    let inQuotes = false;
    for (let i = 0; i < line.length; i++) {
      const ch = line[i];
      if (ch === '"') {
        if (inQuotes && line[i + 1] === '"') {
          cur += '"';
          i++;
        } else inQuotes = !inQuotes;
      } else if (ch === ',' && !inQuotes) {
        result.push(cur);
        cur = '';
      } else cur += ch;
    }
    result.push(cur);
    return result;
  }
}

type ObservableLike = { subscribe: (handlers: { next: () => void; error: (err: unknown) => void }) => void };
