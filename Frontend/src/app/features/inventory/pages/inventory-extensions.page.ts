import { Component, ChangeDetectionStrategy, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../../core/services/language.service';
import { InventoryService } from '../../../core/services/inventory.service';
import {
  INVENTORY_ATTRIBUTE_DATA_TYPES,
  InventoryAttribute,
  InventoryBrand,
  InventoryManufacturer,
  InventoryPriceList,
  InventoryTaxGroup
} from '../../../core/models/inventory.models';
import { InventoryPageShellComponent } from '../shared/inventory-page-shell.component';
import { InventorySkeletonComponent } from '../shared/inventory-skeleton.component';
import { InventoryErrorStateComponent } from '../shared/inventory-error-state.component';
import { InventoryEmptyStateComponent } from '../shared/inventory-empty-state.component';

type ExtTab = 'brands' | 'manufacturers' | 'attributes' | 'priceLists' | 'taxGroups';

@Component({
  selector: 'app-inventory-extensions-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    InventoryPageShellComponent,
    InventorySkeletonComponent,
    InventoryErrorStateComponent,
    InventoryEmptyStateComponent
  ],
  templateUrl: './inventory-extensions.page.html',
  styleUrl: './inventory-extensions.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryExtensionsPage implements OnInit {
  lang = inject(LanguageService);
  inventory = inject(InventoryService);

  tabs: { id: ExtTab; labelKey: string }[] = [
    { id: 'brands', labelKey: 'inv.nav.brands' },
    { id: 'manufacturers', labelKey: 'inv.nav.manufacturers' },
    { id: 'attributes', labelKey: 'inv.nav.attributes' },
    { id: 'priceLists', labelKey: 'inv.nav.priceLists' },
    { id: 'taxGroups', labelKey: 'inv.nav.taxGroups' }
  ];

  activeTab = signal<ExtTab>('brands');
  loading = signal(true);
  error = signal<string | null>(null);
  saving = signal(false);
  formError = signal<string | null>(null);
  showForm = signal(false);
  editingId = signal<string | null>(null);

  brands = signal<InventoryBrand[]>([]);
  manufacturers = signal<InventoryManufacturer[]>([]);
  attributes = signal<InventoryAttribute[]>([]);
  priceLists = signal<InventoryPriceList[]>([]);
  taxGroups = signal<InventoryTaxGroup[]>([]);

  dataTypes = INVENTORY_ATTRIBUTE_DATA_TYPES;
  form: Record<string, unknown> = {};
  valueDraft = '';

  breadcrumbs = [
    { labelKey: 'nav.inventory', path: '/inventory/dashboard' },
    { labelKey: 'inv.nav.extensions' }
  ];

  ngOnInit(): void {
    this.reload();
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  setTab(tab: ExtTab): void {
    this.activeTab.set(tab);
    this.closeForm();
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.error.set(null);
    const tab = this.activeTab();
    const fail = (err: { error?: { error?: string }; message?: string }) => {
      this.error.set(err?.error?.error ?? err?.message ?? this.t('inv.extensions.loadError'));
      this.loading.set(false);
    };

    if (tab === 'brands') {
      this.inventory.getBrands().subscribe({
        next: rows => { this.brands.set(rows ?? []); this.loading.set(false); },
        error: fail
      });
    } else if (tab === 'manufacturers') {
      this.inventory.getManufacturers().subscribe({
        next: rows => { this.manufacturers.set(rows ?? []); this.loading.set(false); },
        error: fail
      });
    } else if (tab === 'attributes') {
      this.inventory.getAttributes().subscribe({
        next: rows => { this.attributes.set(rows ?? []); this.loading.set(false); },
        error: fail
      });
    } else if (tab === 'priceLists') {
      this.inventory.getPriceLists().subscribe({
        next: rows => { this.priceLists.set(rows ?? []); this.loading.set(false); },
        error: fail
      });
    } else {
      this.inventory.getTaxGroups().subscribe({
        next: rows => { this.taxGroups.set(rows ?? []); this.loading.set(false); },
        error: fail
      });
    }
  }

  openCreate(): void {
    this.editingId.set(null);
    this.formError.set(null);
    const tab = this.activeTab();
    if (tab === 'brands' || tab === 'manufacturers') {
      this.form = { nameAr: '', nameEn: '', code: '', country: '' };
    } else if (tab === 'attributes') {
      this.form = { nameAr: '', nameEn: '', code: '', dataType: 1 };
    } else if (tab === 'priceLists') {
      this.form = { nameAr: '', nameEn: '', code: '', currency: 'SAR', validFrom: '', validTo: '' };
    } else {
      this.form = { nameAr: '', nameEn: '', description: '' };
    }
    this.showForm.set(true);
  }

  openEdit(row: InventoryBrand | InventoryManufacturer | InventoryAttribute | InventoryPriceList | InventoryTaxGroup): void {
    this.editingId.set(row.id);
    this.formError.set(null);
    const tab = this.activeTab();
    if (tab === 'brands') {
      const b = row as InventoryBrand;
      this.form = { nameAr: b.nameAr, nameEn: b.nameEn ?? '', code: b.code };
    } else if (tab === 'manufacturers') {
      const m = row as InventoryManufacturer;
      this.form = { nameAr: m.nameAr, nameEn: m.nameEn ?? '', code: m.code, country: m.country ?? '' };
    } else if (tab === 'attributes') {
      const a = row as InventoryAttribute;
      this.form = {
        nameAr: a.nameAr,
        nameEn: a.nameEn ?? '',
        code: a.code,
        dataType: typeof a.dataType === 'number' ? a.dataType : (INVENTORY_ATTRIBUTE_DATA_TYPES.find(d => d.key === a.dataType)?.value ?? 1)
      };
    } else if (tab === 'priceLists') {
      const p = row as InventoryPriceList;
      this.form = {
        nameAr: p.nameAr,
        nameEn: p.nameEn ?? '',
        code: p.code,
        currency: p.currency,
        validFrom: p.validFrom?.slice(0, 10) ?? '',
        validTo: p.validTo?.slice(0, 10) ?? ''
      };
    } else {
      const g = row as InventoryTaxGroup;
      this.form = { nameAr: g.nameAr, nameEn: g.nameEn ?? '', description: g.description ?? '' };
    }
    this.showForm.set(true);
  }

  closeForm(): void {
    this.showForm.set(false);
  }

  save(): void {
    const nameAr = String(this.form['nameAr'] ?? '').trim();
    if (!nameAr) {
      this.formError.set(this.t('inv.field.nameAr'));
      return;
    }
    this.saving.set(true);
    this.formError.set(null);
    const id = this.editingId();
    const done = () => {
      this.saving.set(false);
      this.showForm.set(false);
      this.reload();
    };
    const fail = (err: { error?: { error?: string } }) => {
      this.saving.set(false);
      this.formError.set(err?.error?.error ?? this.t('inv.saveFailed'));
    };

    const tab = this.activeTab();
    if (tab === 'brands') {
      const payload = { nameAr, nameEn: String(this.form['nameEn'] || '') || undefined, code: String(this.form['code'] || '') || undefined };
      if (id) {
        this.inventory.updateBrand(id, payload).subscribe({ next: done, error: fail });
      } else {
        this.inventory.createBrand(payload).subscribe({ next: done, error: fail });
      }
    } else if (tab === 'manufacturers') {
      const payload = {
        nameAr,
        nameEn: String(this.form['nameEn'] || '') || undefined,
        code: String(this.form['code'] || '') || undefined,
        country: String(this.form['country'] || '') || undefined
      };
      if (id) {
        this.inventory.updateManufacturer(id, payload).subscribe({ next: done, error: fail });
      } else {
        this.inventory.createManufacturer(payload).subscribe({ next: done, error: fail });
      }
    } else if (tab === 'attributes') {
      const payload = {
        nameAr,
        nameEn: String(this.form['nameEn'] || '') || undefined,
        code: String(this.form['code'] || '') || undefined,
        dataType: Number(this.form['dataType'] ?? 1)
      };
      if (id) {
        this.inventory.updateAttribute(id, payload).subscribe({ next: done, error: fail });
      } else {
        this.inventory.createAttribute(payload).subscribe({ next: done, error: fail });
      }
    } else if (tab === 'priceLists') {
      const payload = {
        nameAr,
        nameEn: String(this.form['nameEn'] || '') || undefined,
        code: String(this.form['code'] || '') || undefined,
        currency: String(this.form['currency'] || 'SAR'),
        validFrom: String(this.form['validFrom'] || '') || null,
        validTo: String(this.form['validTo'] || '') || null
      };
      if (id) {
        this.inventory.updatePriceList(id, payload).subscribe({ next: done, error: fail });
      } else {
        this.inventory.createPriceList(payload).subscribe({ next: done, error: fail });
      }
    } else {
      const payload = {
        nameAr,
        nameEn: String(this.form['nameEn'] || '') || undefined,
        description: String(this.form['description'] || '') || undefined
      };
      if (id) {
        this.inventory.updateTaxGroup(id, payload).subscribe({ next: done, error: fail });
      } else {
        this.inventory.createTaxGroup(payload).subscribe({ next: done, error: fail });
      }
    }
  }

  toggleActive(row: { id: string; isActive: boolean }): void {
    const tab = this.activeTab();
    const req$ =
      tab === 'brands'
        ? (row.isActive ? this.inventory.deactivateBrand(row.id) : this.inventory.activateBrand(row.id))
        : tab === 'manufacturers'
          ? (row.isActive ? this.inventory.deactivateManufacturer(row.id) : this.inventory.activateManufacturer(row.id))
          : tab === 'attributes'
            ? (row.isActive ? this.inventory.deactivateAttribute(row.id) : this.inventory.activateAttribute(row.id))
            : tab === 'priceLists'
              ? (row.isActive ? this.inventory.deactivatePriceList(row.id) : this.inventory.activatePriceList(row.id))
              : null;
    req$?.subscribe({ next: () => this.reload() });
  }

  addListValue(attr: InventoryAttribute): void {
    const v = this.valueDraft.trim();
    if (!v) return;
    this.inventory.addAttributeValue(attr.id, v).subscribe({
      next: () => { this.valueDraft = ''; this.reload(); }
    });
  }

  removeListValue(attr: InventoryAttribute, valueId: string): void {
    this.inventory.removeAttributeValue(attr.id, valueId).subscribe({ next: () => this.reload() });
  }

  dataTypeLabel(value: InventoryAttribute['dataType']): string {
    const key = typeof value === 'number'
      ? (INVENTORY_ATTRIBUTE_DATA_TYPES.find(d => d.value === value)?.key ?? 'Text')
      : value;
    return this.t(`inv.attr.type.${key}`);
  }
}
