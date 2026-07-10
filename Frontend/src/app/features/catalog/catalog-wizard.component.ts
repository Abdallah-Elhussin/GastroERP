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
  CatalogAuditEntry,
  CatalogRecipeIngredient,
  CatalogWizardStep,
  InventoryCostingMethodId,
  ProductCatalogDefinition,
  ProductCatalogTypeDefinition
} from '../../core/models/catalog.models';
import { InventoryItemDefinition, InventoryUnit } from '../../core/models/inventory.models';

@Component({
  selector: 'app-catalog-wizard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, MatIconModule],
  templateUrl: './catalog-wizard.component.html',
  styleUrl: './catalog-wizard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CatalogWizardComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  langService = inject(LanguageService);
  catalogService = inject(CatalogService);
  inventoryService = inject(InventoryService);
  menuService = inject(MenuService);

  catalogId = signal<string | null>(null);
  loadedDefinition = signal<ProductCatalogDefinition | null>(null);
  selectedType = signal<ProductCatalogTypeDefinition | null>(null);
  activeStep = signal<CatalogWizardStep>('type');
  saving = signal(false);
  error = signal<string | null>(null);
  generatedCode = signal<string | null>(null);
  auditTimeline = signal<CatalogAuditEntry[]>([]);

  generalForm = this.fb.group({
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
    primaryImageUrl: [''],
    menuCategoryId: [''],
    inventoryCategoryId: ['']
  });

  inventoryForm = this.fb.group({
    baseUnitId: ['', Validators.required],
    defaultPurchaseUnitId: [''],
    defaultRecipeUnitId: [''],
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

  recipeForm = this.fb.group({
    yield: [1, [Validators.required, Validators.min(0.01)]],
    wastePercentage: [0, [Validators.min(0), Validators.max(99)]],
    preparationTime: [0, [Validators.min(0)]],
    instructions: [''],
    ingredients: this.fb.array([])
  });

  posForm = this.fb.group({
    menuCategoryId: ['', Validators.required],
    prepTimeMinutes: [0, [Validators.min(0)]],
    isAvailableOnPos: [true],
    isFeaturedOnPos: [false],
    kitchenStationId: ['']
  });

  pricingForm = this.fb.group({
    basePrice: [0, [Validators.required, Validators.min(0)]],
    currency: ['SAR', Validators.required]
  });

  extensionsForm = this.fb.group({
    supplierIds: [''],
    mediaUrls: [''],
    variantAttributesJson: ['']
  });

  relationshipsForm = this.fb.group({
    relatedCatalogId: [''],
    relationshipType: ['crossSell']
  });

  relatedProducts = signal<{ targetCatalogId: string; relationshipType: string }[]>([]);
  priceLevelPrices = signal<Record<string, number>>({});

  costingMethods: InventoryCostingMethodId[] = [1, 2, 3];
  relationshipTypes = ['crossSell', 'upsell', 'alternative'];

  steps: CatalogWizardStep[] = ['type', 'general', 'inventory', 'recipe', 'pos', 'pricing', 'review'];

  visibleSteps = computed(() => {
    const type = this.selectedType();
    if (!type) return ['type', 'general'] as CatalogWizardStep[];
    return this.steps.filter(s => type.wizardSteps.includes(s));
  });

  stepIndex = computed(() => this.visibleSteps().indexOf(this.activeStep()));

  get ingredients(): FormArray {
    return this.recipeForm.get('ingredients') as FormArray;
  }

  ngOnInit(): void {
    this.inventoryService.loadMasterData();
    this.inventoryService.loadItems();
    this.menuService.loadMasterData();
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.catalogId.set(id);
      this.catalogService.whenTypesReady().subscribe(() => {
        this.catalogService.getDefinition(id).subscribe({
          next: def => this.applyLoadedDefinition(def),
          error: () => this.error.set(this.t('catalog.loadFailed'))
        });
        this.catalogService.getAuditTimeline(id).pipe().subscribe({
          next: entries => this.auditTimeline.set(entries),
          error: () => this.auditTimeline.set([])
        });
      });
    } else {
      this.catalogService.loadTypes();
    }
  }

  selectType(type: ProductCatalogTypeDefinition): void {
    this.selectedType.set(type);
  }

  isStepActive(step: CatalogWizardStep): boolean {
    return this.activeStep() === step;
  }

  isStepDone(step: CatalogWizardStep): boolean {
    return this.visibleSteps().indexOf(step) < this.stepIndex();
  }

  canGoNext(): boolean {
    const step = this.activeStep();
    if (step === 'type') return !!this.selectedType();
    if (step === 'general') return this.generalForm.valid;
    if (step === 'inventory') return this.inventoryForm.valid && !!this.generalForm.value.inventoryCategoryId;
    if (step === 'recipe') return this.recipeForm.valid;
    if (step === 'pos') return this.posForm.valid;
    if (step === 'pricing') return this.pricingForm.valid;
    return true;
  }

  next(): void {
    const step = this.activeStep();
    if (step === 'type') return this.createOrAdvanceFromType();
    if (step === 'general') return this.saveGeneralAndAdvance();
    if (step === 'inventory') return this.saveInventoryAndAdvance();
    if (step === 'recipe') return this.saveRecipeAndAdvance();
    if (step === 'pos') return this.savePosAndAdvance();
    if (step === 'pricing') return this.savePricingAndAdvance();
    this.advanceStep();
  }

  back(): void {
    const idx = this.stepIndex();
    if (idx > 0) this.activeStep.set(this.visibleSteps()[idx - 1]);
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

  addRelationship(): void {
    const v = this.relationshipsForm.getRawValue();
    if (!v.relatedCatalogId?.trim()) return;
    this.relatedProducts.update(list => [...list, {
      targetCatalogId: v.relatedCatalogId!.trim(),
      relationshipType: v.relationshipType ?? 'crossSell'
    }]);
    this.relationshipsForm.patchValue({ relatedCatalogId: '' });
  }

  removeRelationship(index: number): void {
    this.relatedProducts.update(list => list.filter((_, i) => i !== index));
  }

  setPriceLevelPrice(levelId: string, price: number): void {
    this.priceLevelPrices.update(map => ({ ...map, [levelId]: price }));
  }

  unitLabel(unit: InventoryUnit): string {
    return this.langService.language() === 'ar' ? unit.nameAr : (unit.nameEn ?? unit.nameAr);
  }

  itemLabel(item: InventoryItemDefinition): string {
    return this.langService.language() === 'ar' ? item.nameAr : (item.nameEn ?? item.nameAr);
  }

  costingMethodLabel(method: InventoryCostingMethodId): string {
    return this.t(`catalog.costing.${method}`);
  }

  private applyLoadedDefinition(def: ProductCatalogDefinition): void {
    this.loadedDefinition.set(def);
    this.generatedCode.set(def.code);
    this.selectedType.set(this.catalogService.types().find(t => t.type === def.catalogType) ?? null);
    this.activeStep.set('general');
    this.generalForm.patchValue({
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
      primaryImageUrl: def.primaryImageUrl ?? '',
      menuCategoryId: def.menuCategoryId ?? '',
      inventoryCategoryId: def.inventoryCategoryId ?? ''
    });
    this.inventoryForm.patchValue({
      baseUnitId: def.baseUnitId ?? '',
      defaultPurchaseUnitId: def.defaultPurchaseUnitId ?? '',
      defaultRecipeUnitId: def.defaultRecipeUnitId ?? '',
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
    this.posForm.patchValue({
      menuCategoryId: def.menuCategoryId ?? '',
      prepTimeMinutes: def.prepTimeMinutes ?? 0,
      isAvailableOnPos: def.isAvailableOnPos ?? true,
      isFeaturedOnPos: def.isFeaturedOnPos ?? false,
      kitchenStationId: def.kitchenStationId ?? ''
    });
    this.pricingForm.patchValue({ basePrice: def.basePrice ?? 0, currency: def.currency ?? 'SAR' });
    const levelMap: Record<string, number> = {};
    (def.priceLevels ?? []).forEach(p => { levelMap[p.priceLevelId] = p.price; });
    this.priceLevelPrices.set(levelMap);
    this.extensionsForm.patchValue({
      supplierIds: (def.supplierIds ?? []).join(', '),
      mediaUrls: (def.mediaUrls ?? []).join('\n'),
      variantAttributesJson: def.variantAttributesJson ?? ''
    });
    this.relatedProducts.set((def.relatedProducts ?? []).map(r => ({
      targetCatalogId: r.targetCatalogId,
      relationshipType: r.relationshipType
    })));
  }

  private createOrAdvanceFromType(): void {
    const type = this.selectedType();
    if (!type) return;
    if (this.catalogId()) {
      this.activeStep.set('general');
      return;
    }
    this.saving.set(true);
    this.catalogService.createDraft({
      catalogType: type.type,
      nameAr: this.t('catalog.draftName'),
      nameEn: 'Draft'
    }).subscribe({
      next: def => {
        this.catalogId.set(def.id);
        this.generatedCode.set(def.code);
        this.generalForm.patchValue({ nameAr: def.nameAr, nameEn: def.nameEn ?? '' });
        this.saving.set(false);
        this.activeStep.set('general');
      },
      error: err => {
        this.saving.set(false);
        this.error.set(err?.error?.error ?? this.t('catalog.saveFailed'));
      }
    });
  }

  private saveGeneralAndAdvance(): void {
    if (this.generalForm.invalid || !this.catalogId()) {
      this.generalForm.markAllAsTouched();
      return;
    }
    const v = this.generalForm.getRawValue();
    this.saving.set(true);
    this.catalogService.updateGeneralInfo(this.catalogId()!, {
      nameAr: v.nameAr!.trim(),
      nameEn: v.nameEn?.trim() || undefined,
      shortDescriptionAr: v.shortDescriptionAr?.trim() || undefined,
      shortDescriptionEn: v.shortDescriptionEn?.trim() || undefined,
      longDescriptionAr: v.longDescriptionAr?.trim() || undefined,
      longDescriptionEn: v.longDescriptionEn?.trim() || undefined,
      keywords: v.keywords?.trim() || undefined,
      brand: v.brand?.trim() || undefined,
      sku: v.sku?.trim() || undefined,
      barcode: v.barcode?.trim() || undefined,
      primaryImageUrl: v.primaryImageUrl?.trim() || undefined,
      menuCategoryId: v.menuCategoryId || undefined,
      inventoryCategoryId: v.inventoryCategoryId || undefined
    }).subscribe({
      next: def => { this.loadedDefinition.set(def); this.afterSaveAdvance(); },
      error: err => this.handleSaveError(err)
    });
  }

  private saveInventoryAndAdvance(): void {
    if (this.inventoryForm.invalid || !this.catalogId()) {
      this.inventoryForm.markAllAsTouched();
      return;
    }
    if (!this.generalForm.value.inventoryCategoryId && this.selectedType()?.requiresInventory) {
      this.error.set(this.t('catalog.inventoryCategoryRequired'));
      return;
    }
    const v = this.inventoryForm.getRawValue();
    this.saving.set(true);
    this.error.set(null);
    this.catalogService.saveInventory(this.catalogId()!, {
      baseUnitId: v.baseUnitId!,
      defaultPurchaseUnitId: v.defaultPurchaseUnitId || undefined,
      defaultRecipeUnitId: v.defaultRecipeUnitId || undefined,
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
    }).subscribe({
      next: def => { this.loadedDefinition.set(def); this.afterSaveAdvance(); },
      error: err => this.handleSaveError(err)
    });
  }

  private saveRecipeAndAdvance(): void {
    if (this.recipeForm.invalid || !this.catalogId()) {
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
    this.saving.set(true);
    this.catalogService.saveRecipe(this.catalogId()!, {
      yield: Number(v.yield),
      wastePercentage: Number(v.wastePercentage) || 0,
      preparationTime: Number(v.preparationTime) || 0,
      instructions: v.instructions?.trim() || undefined,
      ingredients
    }).subscribe({
      next: def => { this.loadedDefinition.set(def); this.afterSaveAdvance(); },
      error: err => this.handleSaveError(err)
    });
  }

  private savePosAndAdvance(): void {
    if (this.posForm.invalid || !this.catalogId()) {
      this.posForm.markAllAsTouched();
      return;
    }
    const v = this.posForm.getRawValue();
    this.saving.set(true);
    this.catalogService.savePos(this.catalogId()!, {
      menuCategoryId: v.menuCategoryId!,
      prepTimeMinutes: Number(v.prepTimeMinutes) || 0,
      isAvailableOnPos: !!v.isAvailableOnPos,
      isFeaturedOnPos: !!v.isFeaturedOnPos,
      kitchenStationId: v.kitchenStationId || undefined
    }).subscribe({
      next: def => { this.loadedDefinition.set(def); this.afterSaveAdvance(); },
      error: err => this.handleSaveError(err)
    });
  }

  private savePricingAndAdvance(): void {
    if (this.pricingForm.invalid || !this.catalogId()) {
      this.pricingForm.markAllAsTouched();
      return;
    }
    const v = this.pricingForm.getRawValue();
    const prices = this.priceLevelPrices();
    const priceLevels = this.menuService.priceLevels()
      .filter(l => prices[l.id] != null)
      .map(l => ({ priceLevelId: l.id, priceLevelName: l.nameAr, price: prices[l.id] }));
    this.saving.set(true);
    this.catalogService.savePricing(this.catalogId()!, {
      basePrice: Number(v.basePrice) || 0,
      currency: v.currency ?? 'SAR',
      priceLevels
    }).subscribe({
      next: def => { this.loadedDefinition.set(def); this.afterSaveAdvance(); },
      error: err => this.handleSaveError(err)
    });
  }

  finish(): void {
    if (!this.catalogId()) return;
    this.saving.set(true);
    const ext = this.extensionsForm.getRawValue();
    const supplierIds = ext.supplierIds?.split(',').map(s => s.trim()).filter(Boolean) ?? [];
    const mediaUrls = ext.mediaUrls?.split('\n').map(s => s.trim()).filter(Boolean) ?? [];
    const related = this.relatedProducts();
    const id = this.catalogId()!;

    this.catalogService.saveExtensions(id, {
      supplierIds,
      mediaUrls,
      variantAttributesJson: ext.variantAttributesJson?.trim() || undefined
    }).subscribe({
      next: () => {
        this.catalogService.saveRelationships(id, {
          relatedProducts: related.map(r => ({ targetCatalogId: r.targetCatalogId, relationshipType: r.relationshipType }))
        }).subscribe({
          next: () => {
            this.catalogService.activate(id).subscribe({
              next: () => {
                this.saving.set(false);
                this.router.navigate(['/catalog']);
              },
              error: err => this.handleSaveError(err)
            });
          },
          error: err => this.handleSaveError(err)
        });
      },
      error: err => this.handleSaveError(err)
    });
  }

  private afterSaveAdvance(): void {
    this.saving.set(false);
    const nextStep = this.visibleSteps()[this.stepIndex() + 1];
    this.activeStep.set(nextStep ?? 'review');
  }

  private handleSaveError(err: { error?: { error?: string } }): void {
    this.saving.set(false);
    this.error.set(err?.error?.error ?? this.t('catalog.saveFailed'));
  }

  private advanceStep(): void {
    const idx = this.stepIndex();
    const next = this.visibleSteps()[idx + 1];
    if (next) this.activeStep.set(next);
  }

  typeLabel(type: ProductCatalogTypeDefinition): string {
    return this.langService.language() === 'ar' ? type.nameAr : type.nameEn;
  }

  stepLabel(step: CatalogWizardStep): string {
    return this.t(`catalog.step.${step}`);
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
