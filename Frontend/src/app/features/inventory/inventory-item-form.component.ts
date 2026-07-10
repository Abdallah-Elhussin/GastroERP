import { Component, ChangeDetectionStrategy, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '../../core/services/language.service';
import { InventoryService } from '../../core/services/inventory.service';
import { InventoryItemKind } from '../../core/models/inventory.models';

@Component({
  selector: 'app-inventory-item-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, MatButtonModule, MatIconModule],
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

  activeTab = signal<'basic' | 'identifiers' | 'costing'>('basic');
  saving = signal(false);
  saveError = signal<string | null>(null);
  isEditMode = signal(false);
  itemId = signal<string | null>(null);
  averageCost = signal<number | null>(null);
  lastPurchaseCost = signal<number | null>(null);

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
    reorderLevel: [0, [Validators.min(0)]],
    reorderQuantity: [0, [Validators.min(0)]]
  });

  ngOnInit(): void {
    this.inventoryService.loadMasterData();
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isEditMode.set(true);
      this.itemId.set(id);
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
        },
        error: () => this.saveError.set(this.t('inventoryItem.loadFailed'))
      });
    }
  }

  setTab(tab: 'basic' | 'identifiers' | 'costing'): void {
    this.activeTab.set(tab);
  }

  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => this.form.patchValue({ imageUrl: String(reader.result) });
    reader.readAsDataURL(file);
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
          this.router.navigate(['/inventory']);
        },
        error: (err: { error?: { error?: string } }) => {
          this.saving.set(false);
          this.saveError.set(err?.error?.error ?? this.t('inventoryItem.saveFailed'));
        }
      });
      return;
    }

    this.inventoryService.createItem(payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.router.navigate(['/inventory']);
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
}
