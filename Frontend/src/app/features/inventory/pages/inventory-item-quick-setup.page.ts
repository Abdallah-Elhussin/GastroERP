import { Component, ChangeDetectionStrategy, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../../core/services/language.service';
import { InventoryService } from '../../../core/services/inventory.service';
import {
  InventoryCategory,
  InventoryItemKind,
  InventoryUnit
} from '../../../core/models/inventory.models';
import { InventoryPageShellComponent } from '../shared/inventory-page-shell.component';

type QuickStep = 1 | 2 | 3 | 4;

@Component({
  selector: 'app-inventory-item-quick-setup-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, MatIconModule, InventoryPageShellComponent],
  templateUrl: './inventory-item-quick-setup.page.html',
  styleUrl: './inventory-item-quick-setup.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryItemQuickSetupPage implements OnInit {
  langService = inject(LanguageService);
  inventoryService = inject(InventoryService);
  router = inject(Router);

  step = signal<QuickStep>(1);
  saving = signal(false);
  error = signal<string | null>(null);
  lookupOpen = signal(false);

  categoryId = signal('');
  nameAr = signal('');
  baseUnitId = signal('');
  classificationId = signal('');

  categoryQuery = signal('');
  unitQuery = signal('');
  classificationQuery = signal('');

  breadcrumbs = [
    { labelKey: 'nav.inventory', path: '/inventory/dashboard' },
    { labelKey: 'inv.nav.items', path: '/inventory/items' },
    { labelKey: 'inv.nav.newItem' }
  ];

  stepTitle = computed(() => {
    switch (this.step()) {
      case 1: return this.t('inv.quick.itemCategory');
      case 2: return this.t('inv.quick.itemName');
      case 3: return this.t('inv.quick.baseUnit');
      case 4: return this.t('inv.quick.classification');
    }
  });

  stepHint = computed(() => {
    switch (this.step()) {
      case 1: return this.t('inv.quick.step.categoryHint');
      case 2: return this.t('inv.quick.step.nameHint');
      case 3: return this.t('inv.quick.step.unitHint');
      case 4: return this.t('inv.quick.step.classificationHint');
    }
  });

  categoryOptions = computed(() =>
    this.filterCats(this.inventoryService.categories(), this.categoryQuery())
  );

  unitOptions = computed(() =>
    this.filterUnits(this.inventoryService.units(), this.unitQuery())
  );

  classificationOptions = computed(() => {
    const all = this.inventoryService.categories().filter(c => c.isActive);
    const roots = all.filter(c => !c.parentCategoryId);
    return this.filterCats(roots.length ? roots : all, this.classificationQuery());
  });

  selectedCategoryLabel = computed(() => this.labelCat(this.categoryId()));
  selectedUnitLabel = computed(() => this.labelUnit(this.baseUnitId()));
  selectedClassificationLabel = computed(() => this.labelCat(this.classificationId()));

  canNext = computed(() => {
    switch (this.step()) {
      case 1: return !!this.categoryId();
      case 2: return this.nameAr().trim().length > 0;
      case 3: return !!this.baseUnitId();
      case 4: return !!this.classificationId();
    }
  });

  ngOnInit(): void {
    this.inventoryService.loadMasterData();
  }

  pickCategory(cat: InventoryCategory): void {
    this.categoryId.set(cat.id);
    this.categoryQuery.set(this.catLabel(cat));
    this.lookupOpen.set(false);
  }

  pickUnit(unit: InventoryUnit): void {
    this.baseUnitId.set(unit.id);
    this.unitQuery.set(this.unitLabel(unit));
    this.lookupOpen.set(false);
  }

  pickClassification(cat: InventoryCategory): void {
    this.classificationId.set(cat.id);
    this.classificationQuery.set(this.catLabel(cat));
    this.lookupOpen.set(false);
  }

  back(): void {
    const s = this.step();
    if (s === 1) {
      void this.router.navigate(['/inventory/items']);
      return;
    }
    this.error.set(null);
    this.lookupOpen.set(false);
    this.step.set((s - 1) as QuickStep);
  }

  next(): void {
    if (!this.canNext()) {
      this.error.set(this.t('inv.quick.step.required'));
      return;
    }
    this.error.set(null);
    this.lookupOpen.set(false);

    if (this.step() < 4) {
      this.step.set((this.step() + 1) as QuickStep);
      return;
    }
    this.finish();
  }

  catLabel(cat: InventoryCategory): string {
    return this.langService.language() === 'ar' ? cat.nameAr : (cat.nameEn || cat.nameAr);
  }

  unitLabel(unit: InventoryUnit): string {
    return this.langService.language() === 'ar' ? unit.nameAr : (unit.nameEn || unit.nameAr);
  }

  t(key: string): string {
    return this.langService.t(key);
  }

  private finish(): void {
    const classification = this.inventoryService
      .categories()
      .find(c => c.id === this.classificationId());

    const itemKind: InventoryItemKind =
      classification && /مصنع|finished|product|منتج/i.test(this.catLabel(classification))
        ? 'manufactured'
        : 'raw';

    this.saving.set(true);
    this.error.set(null);

    this.inventoryService.createItem({
      categoryId: this.categoryId(),
      nameAr: this.nameAr().trim(),
      baseUnitId: this.baseUnitId(),
      itemKind,
      descriptionAr: classification
        ? `${this.t('inv.quick.classification')}: ${this.catLabel(classification)}`
        : undefined
    }).subscribe({
      next: created => {
        this.saving.set(false);
        void this.router.navigate(['/inventory/items', created.id]);
      },
      error: (err: { error?: { error?: string } }) => {
        this.saving.set(false);
        this.error.set(err?.error?.error ?? this.t('inventoryItem.saveFailed'));
      }
    });
  }

  private labelCat(id: string): string {
    const cat = this.inventoryService.categories().find(c => c.id === id);
    return cat ? this.catLabel(cat) : '';
  }

  private labelUnit(id: string): string {
    const unit = this.inventoryService.units().find(u => u.id === id);
    return unit ? this.unitLabel(unit) : '';
  }

  private filterCats(rows: InventoryCategory[], q: string): InventoryCategory[] {
    const active = rows.filter(c => c.isActive);
    const query = q.trim().toLowerCase();
    if (!query) return active.slice(0, 14);
    return active.filter(c => this.catLabel(c).toLowerCase().includes(query)).slice(0, 14);
  }

  private filterUnits(rows: InventoryUnit[], q: string): InventoryUnit[] {
    const active = rows.filter(u => u.isActive);
    const query = q.trim().toLowerCase();
    if (!query) return active.slice(0, 14);
    return active.filter(u => this.unitLabel(u).toLowerCase().includes(query)).slice(0, 14);
  }
}
