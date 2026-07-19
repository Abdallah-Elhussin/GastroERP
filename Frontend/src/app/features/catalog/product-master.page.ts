import { Component, ChangeDetectionStrategy, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../core/services/language.service';
import { CatalogService } from '../../core/services/catalog.service';
import { InventoryService } from '../../core/services/inventory.service';
import { MenuService } from '../../core/services/menu.service';
import {
  CatalogRecipeIngredient,
  InventoryCostingMethodId,
  ProductCatalogDefinition,
  ProductCatalogTypeDefinition
} from '../../core/models/catalog.models';
import { InventoryItemDefinition, InventoryUnit } from '../../core/models/inventory.models';
import { InventoryPageShellComponent } from '../inventory/shared/inventory-page-shell.component';
import { InventorySkeletonComponent } from '../inventory/shared/inventory-skeleton.component';
import { InventoryErrorStateComponent } from '../inventory/shared/inventory-error-state.component';

export type ProductMasterTab =
  | 'basic'
  | 'categories'
  | 'units'
  | 'prices'
  | 'taxes'
  | 'images'
  | 'inventory'
  | 'purchase'
  | 'sales'
  | 'manufacturing'
  | 'restaurant'
  | 'logistics'
  | 'accounting'
  | 'additional';

interface ProductMasterExtras {
  taxes?: { taxGroupCode?: string; taxInclusive?: boolean; notes?: string };
  logistics?: {
    weightKg?: number;
    lengthCm?: number;
    widthCm?: number;
    heightCm?: number;
    hsCode?: string;
    packingUnit?: string;
  };
  accounting?: {
    inventoryAccountCode?: string;
    cogsAccountCode?: string;
    salesAccountCode?: string;
    purchaseAccountCode?: string;
  };
  additional?: {
    notes?: string;
    isSellable?: boolean;
    isPurchasable?: boolean;
    isProducible?: boolean;
    customFieldsJson?: string;
  };
}

const ALL_TABS: ProductMasterTab[] = [
  'basic', 'categories', 'units', 'prices', 'taxes', 'images',
  'inventory', 'purchase', 'sales', 'manufacturing', 'restaurant',
  'logistics', 'accounting', 'additional'
];

@Component({
  selector: 'app-product-master-page',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatIconModule,
    InventoryPageShellComponent,
    InventorySkeletonComponent,
    InventoryErrorStateComponent
  ],
  templateUrl: './product-master.page.html',
  styleUrl: './product-master.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductMasterPage implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  lang = inject(LanguageService);
  catalogService = inject(CatalogService);
  inventoryService = inject(InventoryService);
  menuService = inject(MenuService);

  catalogId = signal<string | null>(null);
  definition = signal<ProductCatalogDefinition | null>(null);
  selectedType = signal<ProductCatalogTypeDefinition | null>(null);
  activeTab = signal<ProductMasterTab>('basic');
  loading = signal(true);
  saving = signal(false);
  activating = signal(false);
  error = signal<string | null>(null);
  success = signal<string | null>(null);
  pickingType = signal(false);

  priceLevelPrices = signal<Record<string, number>>({});
  costingMethods: InventoryCostingMethodId[] = [1, 2, 3];

  basicForm = this.fb.group({
    nameAr: ['', Validators.required],
    nameEn: [''],
    shortDescriptionAr: [''],
    shortDescriptionEn: [''],
    longDescriptionAr: [''],
    longDescriptionEn: [''],
    keywords: [''],
    brand: [''],
    sku: [''],
    barcode: [''],
    primaryImageUrl: ['']
  });

  categoriesForm = this.fb.group({
    inventoryCategoryId: [''],
    menuCategoryId: ['']
  });

  unitsForm = this.fb.group({
    baseUnitId: ['', Validators.required],
    defaultPurchaseUnitId: [''],
    defaultRecipeUnitId: ['']
  });

  inventoryForm = this.fb.group({
    minStock: [0, [Validators.min(0)]],
    maxStock: [0, [Validators.min(0)]],
    safetyStock: [0, [Validators.min(0)]],
    reorderLevel: [0, [Validators.min(0)]],
    reorderQuantity: [0, [Validators.min(0)]],
    costingMethod: [2 as InventoryCostingMethodId, Validators.required],
    trackBatch: [false],
    trackSerial: [false],
    trackExpiry: [false],
    allowNegativeStock: [false]
  });

  pricingForm = this.fb.group({
    basePrice: [0, [Validators.required, Validators.min(0)]],
    currency: ['SAR', Validators.required]
  });

  recipeForm = this.fb.group({
    yield: [1, [Validators.required, Validators.min(0.01)]],
    wastePercentage: [0, [Validators.min(0), Validators.max(99)]],
    preparationTime: [0, [Validators.min(0)]],
    instructions: [''],
    ingredients: this.fb.array([])
  });

  salesForm = this.fb.group({
    menuCategoryId: [''],
    isAvailableOnPos: [true],
    isFeaturedOnPos: [false]
  });

  restaurantForm = this.fb.group({
    prepTimeMinutes: [0, [Validators.min(0)]],
    kitchenStationId: ['']
  });

  purchaseForm = this.fb.group({
    supplierIds: ['']
  });

  imagesForm = this.fb.group({
    primaryImageUrl: [''],
    mediaUrls: ['']
  });

  taxesForm = this.fb.group({
    taxGroupCode: [''],
    taxInclusive: [false],
    notes: ['']
  });

  logisticsForm = this.fb.group({
    weightKg: [0],
    lengthCm: [0],
    widthCm: [0],
    heightCm: [0],
    hsCode: [''],
    packingUnit: ['']
  });

  accountingForm = this.fb.group({
    inventoryAccountCode: [''],
    cogsAccountCode: [''],
    salesAccountCode: [''],
    purchaseAccountCode: ['']
  });

  additionalForm = this.fb.group({
    notes: [''],
    isSellable: [true],
    isPurchasable: [true],
    isProducible: [false],
    customFieldsJson: [''],
    variantAttributesJson: ['']
  });

  visibleTabs = computed(() => {
    const type = this.selectedType();
    if (!type) return ALL_TABS.filter(t => t === 'basic' || t === 'categories');
    return ALL_TABS.filter(tab => this.isTabVisible(tab, type));
  });

  breadcrumbs = computed(() => [
    { labelKey: 'nav.inventory', path: '/inventory/dashboard' },
    { labelKey: 'nav.catalog.engine', path: '/catalog' },
    { labelKey: 'pm.title' }
  ]);

  get ingredients(): FormArray {
    return this.recipeForm.get('ingredients') as FormArray;
  }

  ngOnInit(): void {
    this.inventoryService.loadMasterData();
    this.inventoryService.loadItems();
    this.menuService.loadMasterData();
    this.catalogService.loadTypes();

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.catalogId.set(id);
      this.catalogService.whenTypesReady().subscribe(() => {
        this.catalogService.getDefinition(id).subscribe({
          next: def => {
            this.applyDefinition(def);
            this.loading.set(false);
          },
          error: () => {
            this.error.set(this.t('catalog.loadFailed'));
            this.loading.set(false);
          }
        });
      });
    } else {
      this.pickingType.set(true);
      this.loading.set(false);
      this.catalogService.loadTypes();
    }
  }

  isTabVisible(tab: ProductMasterTab, type: ProductCatalogTypeDefinition): boolean {
    switch (tab) {
      case 'units':
      case 'inventory':
      case 'purchase':
        return type.requiresInventory;
      case 'manufacturing':
        return type.requiresRecipe;
      case 'sales':
      case 'restaurant':
        return type.requiresProduct;
      case 'prices':
        return type.requiresPricing || type.requiresProduct;
      default:
        return true;
    }
  }

  selectTab(tab: ProductMasterTab): void {
    if (!this.visibleTabs().includes(tab)) return;
    this.activeTab.set(tab);
    this.success.set(null);
    this.error.set(null);
  }

  startWithType(type: ProductCatalogTypeDefinition): void {
    this.selectedType.set(type);
    this.saving.set(true);
    this.error.set(null);
    this.catalogService.createDraft({
      catalogType: type.type,
      nameAr: this.t('catalog.draftName'),
      nameEn: 'Draft'
    }).subscribe({
      next: def => {
        this.saving.set(false);
        void this.router.navigate(['/catalog/master', def.id], { replaceUrl: true });
        this.catalogId.set(def.id);
        this.pickingType.set(false);
        this.applyDefinition(def);
      },
      error: err => {
        this.saving.set(false);
        this.error.set(err?.error?.error ?? this.t('catalog.saveFailed'));
      }
    });
  }

  saveActiveTab(): void {
    const tab = this.activeTab();
    switch (tab) {
      case 'basic':
      case 'categories':
        return this.saveGeneral();
      case 'units':
      case 'inventory':
        return this.saveInventory();
      case 'prices':
        return this.savePricing();
      case 'manufacturing':
        return this.saveRecipe();
      case 'sales':
      case 'restaurant':
        return this.savePos();
      case 'images':
      case 'purchase':
      case 'taxes':
      case 'logistics':
      case 'accounting':
      case 'additional':
        return this.saveExtensions();
    }
  }

  activate(): void {
    const id = this.catalogId();
    if (!id) return;
    this.activating.set(true);
    this.error.set(null);
    this.catalogService.activate(id).subscribe({
      next: def => {
        this.activating.set(false);
        this.applyDefinition(def);
        this.success.set(this.t('pm.activated'));
      },
      error: err => {
        this.activating.set(false);
        this.error.set(err?.error?.error ?? this.t('catalog.saveFailed'));
      }
    });
  }

  addIngredient(): void {
    this.ingredients.push(this.fb.group({
      inventoryItemId: ['', Validators.required],
      unitId: ['', Validators.required],
      quantity: [1, [Validators.required, Validators.min(0.01)]],
      wastePercentage: [0, [Validators.min(0), Validators.max(99)]]
    }));
  }

  removeIngredient(index: number): void {
    this.ingredients.removeAt(index);
  }

  setPriceLevelPrice(levelId: string, price: number): void {
    this.priceLevelPrices.update(map => ({ ...map, [levelId]: price }));
  }

  unitLabel(unit: InventoryUnit): string {
    return this.lang.language() === 'ar' ? unit.nameAr : (unit.nameEn ?? unit.nameAr);
  }

  itemLabel(item: InventoryItemDefinition): string {
    return this.lang.language() === 'ar' ? item.nameAr : (item.nameEn ?? item.nameAr);
  }

  typeLabel(type: ProductCatalogTypeDefinition): string {
    return this.lang.language() === 'ar' ? type.nameAr : type.nameEn;
  }

  tabLabel(tab: ProductMasterTab): string {
    return this.t(`pm.tab.${tab}`);
  }

  costingMethodLabel(method: InventoryCostingMethodId): string {
    return this.t(`catalog.costing.${method}`);
  }

  t(key: string): string {
    return this.lang.t(key);
  }

  private applyDefinition(def: ProductCatalogDefinition): void {
    this.definition.set(def);
    this.selectedType.set(this.catalogService.types().find(t => t.type === def.catalogType) ?? null);
    this.basicForm.patchValue({
      nameAr: def.nameAr,
      nameEn: def.nameEn ?? '',
      shortDescriptionAr: def.shortDescriptionAr ?? '',
      shortDescriptionEn: def.shortDescriptionEn ?? '',
      longDescriptionAr: def.longDescriptionAr ?? '',
      longDescriptionEn: def.longDescriptionEn ?? '',
      keywords: def.keywords ?? '',
      brand: def.brand ?? '',
      sku: def.sku ?? '',
      barcode: def.barcode ?? '',
      primaryImageUrl: def.primaryImageUrl ?? ''
    });
    this.categoriesForm.patchValue({
      inventoryCategoryId: def.inventoryCategoryId ?? '',
      menuCategoryId: def.menuCategoryId ?? ''
    });
    this.unitsForm.patchValue({
      baseUnitId: def.baseUnitId ?? '',
      defaultPurchaseUnitId: def.defaultPurchaseUnitId ?? '',
      defaultRecipeUnitId: def.defaultRecipeUnitId ?? ''
    });
    this.inventoryForm.patchValue({
      minStock: def.minStock ?? 0,
      maxStock: def.maxStock ?? 0,
      safetyStock: def.safetyStock ?? 0,
      reorderLevel: def.reorderLevel ?? 0,
      reorderQuantity: def.reorderQuantity ?? 0,
      costingMethod: (def.costingMethod || 2) as InventoryCostingMethodId,
      trackBatch: def.trackBatch ?? false,
      trackSerial: def.trackSerial ?? false,
      trackExpiry: def.trackExpiry ?? false,
      allowNegativeStock: def.allowNegativeStock ?? false
    });
    this.pricingForm.patchValue({
      basePrice: def.basePrice ?? 0,
      currency: def.currency ?? 'SAR'
    });
    const levelMap: Record<string, number> = {};
    (def.priceLevels ?? []).forEach(p => { levelMap[p.priceLevelId] = p.price; });
    this.priceLevelPrices.set(levelMap);

    this.recipeForm.patchValue({
      yield: def.recipeYield || 1,
      wastePercentage: def.recipeWastePercentage ?? 0,
      preparationTime: def.recipePreparationTime ?? 0,
      instructions: def.recipeInstructions ?? ''
    });
    this.ingredients.clear();
    (def.recipeIngredients ?? []).forEach(ing => this.ingredients.push(this.fb.group({
      inventoryItemId: [ing.inventoryItemId, Validators.required],
      unitId: [ing.unitId, Validators.required],
      quantity: [ing.quantity, [Validators.required, Validators.min(0.01)]],
      wastePercentage: [ing.wastePercentage ?? 0]
    })));

    this.salesForm.patchValue({
      menuCategoryId: def.menuCategoryId ?? '',
      isAvailableOnPos: def.isAvailableOnPos ?? true,
      isFeaturedOnPos: def.isFeaturedOnPos ?? false
    });
    this.restaurantForm.patchValue({
      prepTimeMinutes: def.prepTimeMinutes ?? 0,
      kitchenStationId: def.kitchenStationId ?? ''
    });
    this.purchaseForm.patchValue({
      supplierIds: (def.supplierIds ?? []).join(', ')
    });
    this.imagesForm.patchValue({
      primaryImageUrl: def.primaryImageUrl ?? '',
      mediaUrls: (def.mediaUrls ?? []).join('\n')
    });

    const extras = this.parseExtras(def.variantAttributesJson);
    this.taxesForm.patchValue({
      taxGroupCode: extras.taxes?.taxGroupCode ?? '',
      taxInclusive: extras.taxes?.taxInclusive ?? false,
      notes: extras.taxes?.notes ?? ''
    });
    this.logisticsForm.patchValue({
      weightKg: extras.logistics?.weightKg ?? 0,
      lengthCm: extras.logistics?.lengthCm ?? 0,
      widthCm: extras.logistics?.widthCm ?? 0,
      heightCm: extras.logistics?.heightCm ?? 0,
      hsCode: extras.logistics?.hsCode ?? '',
      packingUnit: extras.logistics?.packingUnit ?? ''
    });
    this.accountingForm.patchValue({
      inventoryAccountCode: extras.accounting?.inventoryAccountCode ?? '',
      cogsAccountCode: extras.accounting?.cogsAccountCode ?? '',
      salesAccountCode: extras.accounting?.salesAccountCode ?? '',
      purchaseAccountCode: extras.accounting?.purchaseAccountCode ?? ''
    });
    this.additionalForm.patchValue({
      notes: extras.additional?.notes ?? '',
      isSellable: extras.additional?.isSellable ?? true,
      isPurchasable: extras.additional?.isPurchasable ?? true,
      isProducible: extras.additional?.isProducible ?? false,
      customFieldsJson: extras.additional?.customFieldsJson ?? '',
      variantAttributesJson: this.stripKnownExtras(def.variantAttributesJson)
    });

    const tabs = this.visibleTabs();
    if (!tabs.includes(this.activeTab())) {
      this.activeTab.set(tabs[0] ?? 'basic');
    }
  }

  private saveGeneral(): void {
    if (!this.catalogId()) return;
    if (this.basicForm.invalid) {
      this.basicForm.markAllAsTouched();
      return;
    }
    const b = this.basicForm.getRawValue();
    const c = this.categoriesForm.getRawValue();
    const img = this.imagesForm.getRawValue();
    this.runSave(this.catalogService.updateGeneralInfo(this.catalogId()!, {
      nameAr: b.nameAr!.trim(),
      nameEn: b.nameEn?.trim() || undefined,
      shortDescriptionAr: b.shortDescriptionAr?.trim() || undefined,
      shortDescriptionEn: b.shortDescriptionEn?.trim() || undefined,
      longDescriptionAr: b.longDescriptionAr?.trim() || undefined,
      longDescriptionEn: b.longDescriptionEn?.trim() || undefined,
      keywords: b.keywords?.trim() || undefined,
      brand: b.brand?.trim() || undefined,
      sku: b.sku?.trim() || undefined,
      barcode: b.barcode?.trim() || undefined,
      primaryImageUrl: (img.primaryImageUrl || b.primaryImageUrl)?.trim() || undefined,
      menuCategoryId: c.menuCategoryId || undefined,
      inventoryCategoryId: c.inventoryCategoryId || undefined
    }));
  }

  private saveInventory(): void {
    if (!this.catalogId()) return;
    if (this.unitsForm.invalid || this.inventoryForm.invalid) {
      this.unitsForm.markAllAsTouched();
      this.inventoryForm.markAllAsTouched();
      return;
    }
    if (!this.categoriesForm.value.inventoryCategoryId && this.selectedType()?.requiresInventory) {
      this.error.set(this.t('catalog.inventoryCategoryRequired'));
      this.activeTab.set('categories');
      return;
    }
    const u = this.unitsForm.getRawValue();
    const v = this.inventoryForm.getRawValue();
    this.runSave(this.catalogService.saveInventory(this.catalogId()!, {
      baseUnitId: u.baseUnitId!,
      defaultPurchaseUnitId: u.defaultPurchaseUnitId || undefined,
      defaultRecipeUnitId: u.defaultRecipeUnitId || undefined,
      minStock: Number(v.minStock) || 0,
      maxStock: Number(v.maxStock) || 0,
      safetyStock: Number(v.safetyStock) || 0,
      reorderLevel: Number(v.reorderLevel) || 0,
      reorderQuantity: Number(v.reorderQuantity) || 0,
      costingMethod: Number(v.costingMethod) as InventoryCostingMethodId,
      trackBatch: !!v.trackBatch,
      trackSerial: !!v.trackSerial,
      trackExpiry: !!v.trackExpiry,
      allowNegativeStock: !!v.allowNegativeStock
    }));
  }

  private savePricing(): void {
    if (!this.catalogId() || this.pricingForm.invalid) {
      this.pricingForm.markAllAsTouched();
      return;
    }
    const v = this.pricingForm.getRawValue();
    const prices = this.priceLevelPrices();
    const priceLevels = this.menuService.priceLevels()
      .filter(l => prices[l.id] != null)
      .map(l => ({ priceLevelId: l.id, priceLevelName: l.nameAr, price: prices[l.id] }));
    this.runSave(this.catalogService.savePricing(this.catalogId()!, {
      basePrice: Number(v.basePrice) || 0,
      currency: v.currency ?? 'SAR',
      priceLevels
    }));
  }

  private saveRecipe(): void {
    if (!this.catalogId() || this.recipeForm.invalid) {
      this.recipeForm.markAllAsTouched();
      return;
    }
    const v = this.recipeForm.getRawValue();
    const rawIngredients = (v.ingredients ?? []) as Array<{
      inventoryItemId: string;
      unitId: string;
      quantity: number;
      wastePercentage?: number;
    }>;
    const ingredients: CatalogRecipeIngredient[] = rawIngredients.map(i => ({
      inventoryItemId: i.inventoryItemId,
      unitId: i.unitId,
      quantity: Number(i.quantity),
      wastePercentage: Number(i.wastePercentage) || 0
    }));
    this.runSave(this.catalogService.saveRecipe(this.catalogId()!, {
      yield: Number(v.yield),
      wastePercentage: Number(v.wastePercentage) || 0,
      preparationTime: Number(v.preparationTime) || 0,
      instructions: v.instructions?.trim() || undefined,
      ingredients
    }));
  }

  private savePos(): void {
    if (!this.catalogId()) return;
    const sales = this.salesForm.getRawValue();
    const rest = this.restaurantForm.getRawValue();
    const menuCategoryId = sales.menuCategoryId || this.categoriesForm.value.menuCategoryId;
    if (!menuCategoryId) {
      this.error.set(this.t('catalog.selectMenuCategory'));
      this.activeTab.set('categories');
      return;
    }
    this.runSave(this.catalogService.savePos(this.catalogId()!, {
      menuCategoryId,
      prepTimeMinutes: Number(rest.prepTimeMinutes) || 0,
      isAvailableOnPos: !!sales.isAvailableOnPos,
      isFeaturedOnPos: !!sales.isFeaturedOnPos,
      kitchenStationId: rest.kitchenStationId || undefined
    }));
  }

  private saveExtensions(): void {
    if (!this.catalogId()) return;
    const purchase = this.purchaseForm.getRawValue();
    const images = this.imagesForm.getRawValue();
    const taxes = this.taxesForm.getRawValue();
    const logistics = this.logisticsForm.getRawValue();
    const accounting = this.accountingForm.getRawValue();
    const additional = this.additionalForm.getRawValue();

    const extras: ProductMasterExtras = {
      taxes: {
        taxGroupCode: taxes.taxGroupCode?.trim() || undefined,
        taxInclusive: !!taxes.taxInclusive,
        notes: taxes.notes?.trim() || undefined
      },
      logistics: {
        weightKg: Number(logistics.weightKg) || 0,
        lengthCm: Number(logistics.lengthCm) || 0,
        widthCm: Number(logistics.widthCm) || 0,
        heightCm: Number(logistics.heightCm) || 0,
        hsCode: logistics.hsCode?.trim() || undefined,
        packingUnit: logistics.packingUnit?.trim() || undefined
      },
      accounting: {
        inventoryAccountCode: accounting.inventoryAccountCode?.trim() || undefined,
        cogsAccountCode: accounting.cogsAccountCode?.trim() || undefined,
        salesAccountCode: accounting.salesAccountCode?.trim() || undefined,
        purchaseAccountCode: accounting.purchaseAccountCode?.trim() || undefined
      },
      additional: {
        notes: additional.notes?.trim() || undefined,
        isSellable: !!additional.isSellable,
        isPurchasable: !!additional.isPurchasable,
        isProducible: !!additional.isProducible,
        customFieldsJson: additional.customFieldsJson?.trim() || undefined
      }
    };

    let userJson: Record<string, unknown> = {};
    if (additional.variantAttributesJson?.trim()) {
      try {
        userJson = JSON.parse(additional.variantAttributesJson) as Record<string, unknown>;
      } catch {
        this.error.set(this.t('pm.invalidJson'));
        return;
      }
    }

    const supplierIds = purchase.supplierIds?.split(',').map(s => s.trim()).filter(Boolean) ?? [];
    const mediaUrls = images.mediaUrls?.split('\n').map(s => s.trim()).filter(Boolean) ?? [];
    const variantAttributesJson = JSON.stringify({ ...userJson, __pm: extras });

    this.saving.set(true);
    this.error.set(null);
    this.success.set(null);

    // Keep primary image in sync when saving images tab
    const primary = images.primaryImageUrl?.trim();
    const afterExt = (def: ProductCatalogDefinition) => {
      this.saving.set(false);
      this.applyDefinition(def);
      this.success.set(this.t('pm.saved'));
    };

    const saveExt = () => this.catalogService.saveExtensions(this.catalogId()!, {
      supplierIds,
      mediaUrls,
      variantAttributesJson
    }).subscribe({
      next: afterExt,
      error: err => this.handleSaveError(err)
    });

    if (this.activeTab() === 'images' && primary) {
      const b = this.basicForm.getRawValue();
      const c = this.categoriesForm.getRawValue();
      this.catalogService.updateGeneralInfo(this.catalogId()!, {
        nameAr: b.nameAr!.trim(),
        nameEn: b.nameEn?.trim() || undefined,
        shortDescriptionAr: b.shortDescriptionAr?.trim() || undefined,
        shortDescriptionEn: b.shortDescriptionEn?.trim() || undefined,
        longDescriptionAr: b.longDescriptionAr?.trim() || undefined,
        longDescriptionEn: b.longDescriptionEn?.trim() || undefined,
        keywords: b.keywords?.trim() || undefined,
        brand: b.brand?.trim() || undefined,
        sku: b.sku?.trim() || undefined,
        barcode: b.barcode?.trim() || undefined,
        primaryImageUrl: primary,
        menuCategoryId: c.menuCategoryId || undefined,
        inventoryCategoryId: c.inventoryCategoryId || undefined
      }).subscribe({
        next: () => saveExt(),
        error: err => this.handleSaveError(err)
      });
      return;
    }

    saveExt();
  }

  private runSave(obs: ReturnType<CatalogService['updateGeneralInfo']>): void {
    this.saving.set(true);
    this.error.set(null);
    this.success.set(null);
    obs.subscribe({
      next: def => {
        this.saving.set(false);
        this.applyDefinition(def);
        this.success.set(this.t('pm.saved'));
      },
      error: err => this.handleSaveError(err)
    });
  }

  private handleSaveError(err: { error?: { error?: string } }): void {
    this.saving.set(false);
    this.error.set(err?.error?.error ?? this.t('catalog.saveFailed'));
  }

  private parseExtras(raw?: string): ProductMasterExtras {
    if (!raw?.trim()) return {};
    try {
      const parsed = JSON.parse(raw) as { __pm?: ProductMasterExtras } & ProductMasterExtras;
      return parsed.__pm ?? parsed;
    } catch {
      return {};
    }
  }

  private stripKnownExtras(raw?: string): string {
    if (!raw?.trim()) return '';
    try {
      const parsed = JSON.parse(raw) as Record<string, unknown>;
      delete parsed['__pm'];
      delete parsed['taxes'];
      delete parsed['logistics'];
      delete parsed['accounting'];
      delete parsed['additional'];
      return Object.keys(parsed).length ? JSON.stringify(parsed, null, 2) : '';
    } catch {
      return raw;
    }
  }
}
