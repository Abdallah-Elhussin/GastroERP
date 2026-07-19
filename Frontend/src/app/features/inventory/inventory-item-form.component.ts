import {
  Component,
  ChangeDetectionStrategy,
  inject,
  signal,
  OnInit,
  computed
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../core/services/language.service';
import { InventoryService } from '../../core/services/inventory.service';
import {
  InventoryItemKind,
  WarehouseStockBalance
} from '../../core/models/inventory.models';

type ItemDetailTab =
  | 'units'
  | 'extra'
  | 'costs'
  | 'prices'
  | 'batches'
  | 'suppliers'
  | 'stock';

interface UnitBarcodeRow {
  id: string;
  unitId: string;
  unitName: string;
  barcode: string;
  barcodeType: string;
  factor: number;
  isBase: boolean;
  forSale: boolean;
  forPurchase: boolean;
  forInventory: boolean;
}

@Component({
  selector: 'app-inventory-item-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink, MatIconModule],
  templateUrl: './inventory-item-form.component.html',
  styleUrl: './inventory-item-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InventoryItemFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  langService = inject(LanguageService);
  inventoryService = inject(InventoryService);

  activeTab = signal<ItemDetailTab>('units');
  saving = signal(false);
  saveError = signal<string | null>(null);
  loading = signal(false);
  isEditMode = signal(false);
  itemId = signal<string | null>(null);
  averageCost = signal<number | null>(null);
  lastPurchaseCost = signal<number | null>(null);
  stock = signal<WarehouseStockBalance[]>([]);
  stockLoading = signal(false);

  trackBatch = signal(false);
  trackSerial = signal(false);
  trackExpiry = signal(false);
  showInMenu = signal(true);
  vatEnabled = signal(false);

  unitRows = signal<UnitBarcodeRow[]>([]);
  draftUnitId = signal('');
  draftBarcode = signal('');
  draftBarcodeType = signal('');
  draftFactor = signal(1);

  tabs: { id: ItemDetailTab; labelKey: string }[] = [
    { id: 'units', labelKey: 'inv.item.tab.units' },
    { id: 'extra', labelKey: 'inv.item.tab.extra' },
    { id: 'costs', labelKey: 'inv.item.tab.costs' },
    { id: 'prices', labelKey: 'inv.item.tab.prices' },
    { id: 'batches', labelKey: 'inv.item.tab.batches' },
    { id: 'suppliers', labelKey: 'inv.item.tab.suppliers' },
    { id: 'stock', labelKey: 'inv.item.tab.stock' }
  ];

  form = this.fb.group({
    nameAr: ['', Validators.required],
    nameEn: [''],
    categoryId: ['', Validators.required],
    itemKind: ['raw' as InventoryItemKind, Validators.required],
    imageUrl: [''],
    descriptionAr: [''],
    descriptionEn: [''],
    sku: [''],
    barcode: [''],
    baseUnitId: ['', Validators.required],
    defaultPurchaseUnitId: [''],
    defaultRecipeUnitId: [''],
    evaluationGroup: [''],
    reorderLevel: [0, [Validators.min(0)]],
    reorderQuantity: [0, [Validators.min(0)]]
  });

  displayTitle = computed(() => {
    const name = this.form.controls.nameAr.value?.trim();
    if (name) return name;
    return this.isEditMode() ? this.t('inventoryItem.editTitle') : this.t('inventoryItem.newTitle');
  });

  ngOnInit(): void {
    this.inventoryService.loadMasterData();
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isEditMode.set(true);
      this.itemId.set(id);
      this.loading.set(true);
      this.inventoryService.getItem(id).subscribe({
        next: item => {
          this.averageCost.set(item.averageUnitCost ?? null);
          this.lastPurchaseCost.set(item.lastPurchaseUnitCost ?? null);
          this.form.patchValue({
            nameAr: item.nameAr,
            nameEn: item.nameEn ?? '',
            categoryId: item.categoryId,
            itemKind: item.itemKind,
            imageUrl: item.imageUrl ?? '',
            descriptionAr: item.descriptionAr ?? '',
            descriptionEn: item.descriptionEn ?? '',
            sku: item.sku ?? '',
            barcode: item.barcode ?? '',
            baseUnitId: item.baseUnitId,
            defaultPurchaseUnitId: item.defaultPurchaseUnitId ?? '',
            defaultRecipeUnitId: item.defaultRecipeUnitId ?? '',
            reorderLevel: item.reorderLevel,
            reorderQuantity: item.reorderQuantity
          });
          this.seedUnitRows(item.baseUnitId, item.baseUnitNameAr, item.barcode ?? '');
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.saveError.set(this.t('inventoryItem.loadFailed'));
        }
      });
    }
  }

  selectTab(tab: ItemDetailTab): void {
    this.activeTab.set(tab);
    if (tab === 'stock' && this.itemId() && this.stock().length === 0) {
      this.loadStock();
    }
  }

  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => this.form.patchValue({ imageUrl: String(reader.result) });
    reader.readAsDataURL(file);
  }

  onBaseUnitChange(): void {
    const unitId = this.form.controls.baseUnitId.value || '';
    const unit = this.inventoryService.units().find(u => u.id === unitId);
    if (!unit) return;
    const barcode = this.form.controls.barcode.value || '';
    this.seedUnitRows(unit.id, this.unitLabel(unit), barcode);
  }

  addUnitRow(): void {
    const unitId = this.draftUnitId();
    const unit = this.inventoryService.units().find(u => u.id === unitId);
    if (!unit) return;
    const factor = Number(this.draftFactor()) || 1;
    if (factor <= 0) return;

    const exists = this.unitRows().some(r => r.unitId === unitId && !r.isBase);
    if (exists) return;

    this.unitRows.update(rows => [
      ...rows,
      {
        id: crypto.randomUUID(),
        unitId,
        unitName: this.unitLabel(unit),
        barcode: this.draftBarcode().trim(),
        barcodeType: this.draftBarcodeType().trim() || '—',
        factor,
        isBase: false,
        forSale: true,
        forPurchase: true,
        forInventory: false
      }
    ]);
    this.draftUnitId.set('');
    this.draftBarcode.set('');
    this.draftBarcodeType.set('');
    this.draftFactor.set(1);
  }

  removeUnitRow(id: string): void {
    this.unitRows.update(rows => rows.filter(r => r.id !== id || r.isBase));
  }

  loadStock(): void {
    const id = this.itemId();
    if (!id) return;
    this.stockLoading.set(true);
    this.inventoryService.getStockByWarehouse(id).subscribe({
      next: rows => {
        this.stock.set(rows ?? []);
        this.stockLoading.set(false);
      },
      error: () => {
        this.stock.set([]);
        this.stockLoading.set(false);
      }
    });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    const payload = {
      categoryId: v.categoryId!,
      nameAr: v.nameAr!.trim(),
      baseUnitId: v.baseUnitId!,
      nameEn: v.nameEn?.trim() || undefined,
      descriptionAr: v.descriptionAr?.trim() || undefined,
      descriptionEn: v.descriptionEn?.trim() || undefined,
      sku: v.sku?.trim() || undefined,
      barcode: v.barcode?.trim() || undefined,
      imageUrl: v.imageUrl?.trim() || undefined,
      itemKind: v.itemKind as InventoryItemKind,
      defaultPurchaseUnitId: v.defaultPurchaseUnitId || undefined,
      defaultRecipeUnitId: v.defaultRecipeUnitId || undefined,
      reorderLevel: Number(v.reorderLevel) || 0,
      reorderQuantity: Number(v.reorderQuantity) || 0
    };

    this.saving.set(true);
    this.saveError.set(null);

    if (this.isEditMode()) {
      this.inventoryService.updateItem(this.itemId()!, payload).subscribe({
        next: () => {
          this.saving.set(false);
          this.router.navigate(['/inventory/items']);
        },
        error: (err: { error?: { error?: string } }) => {
          this.saving.set(false);
          this.saveError.set(err?.error?.error ?? this.t('inventoryItem.saveFailed'));
        }
      });
      return;
    }

    this.inventoryService.createItem(payload).subscribe({
      next: created => {
        this.saving.set(false);
        void this.router.navigate(['/inventory/items', created.id]);
      },
      error: (err: { error?: { error?: string } }) => {
        this.saving.set(false);
        this.saveError.set(err?.error?.error ?? this.t('inventoryItem.saveFailed'));
      }
    });
  }

  categoryLabel(cat: { nameAr: string; nameEn?: string }): string {
    return this.langService.language() === 'ar' ? cat.nameAr : (cat.nameEn || cat.nameAr);
  }

  unitLabel(unit: { nameAr: string; nameEn?: string; symbol?: string }): string {
    const name = this.langService.language() === 'ar' ? unit.nameAr : (unit.nameEn || unit.nameAr);
    return unit.symbol ? `${name} (${unit.symbol})` : name;
  }

  t(key: string): string {
    return this.langService.t(key);
  }

  private seedUnitRows(baseUnitId: string, baseUnitName: string, barcode: string): void {
    const purchaseId = this.form.controls.defaultPurchaseUnitId.value;
    const recipeId = this.form.controls.defaultRecipeUnitId.value;
    const rows: UnitBarcodeRow[] = [
      {
        id: 'base',
        unitId: baseUnitId,
        unitName: baseUnitName,
        barcode: barcode || '—',
        barcodeType: barcode ? 'EAN' : '—',
        factor: 1,
        isBase: true,
        forSale: true,
        forPurchase: !purchaseId || purchaseId === baseUnitId,
        forInventory: true
      }
    ];

    const addAlt = (unitId: string | null | undefined, forPurchase: boolean, forSale: boolean) => {
      if (!unitId || unitId === baseUnitId) return;
      if (rows.some(r => r.unitId === unitId)) return;
      const unit = this.inventoryService.units().find(u => u.id === unitId);
      if (!unit) return;
      rows.push({
        id: unitId,
        unitId,
        unitName: this.unitLabel(unit),
        barcode: '—',
        barcodeType: '—',
        factor: 1,
        isBase: false,
        forSale,
        forPurchase,
        forInventory: false
      });
    };

    addAlt(purchaseId, true, false);
    addAlt(recipeId, false, true);
    this.unitRows.set(rows);
  }
}
