import { Component, ChangeDetectionStrategy, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { AppButtonComponent } from '../../shared/ui/app-button/app-button.component';
import { AppCardComponent } from '../../shared/ui/app-card/app-card.component';
import { AppTableComponent } from '../../shared/ui/app-table/app-table.component';
import { LanguageService } from '../../core/services/language.service';

interface MenuProduct {
  id: string;
  nameAr: string;
  nameEn: string;
  category: string;
  sku: string;
  dineIn: number;
  takeaway: number;
  delivery: number;
  isActive: boolean;
}

interface PriceLevelRow {
  id: string;
  nameAr: string;
  nameEn: string;
  channel: string;
  isDefault: boolean;
}

@Component({
  selector: 'app-menu-operations',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatIconModule, AppButtonComponent, AppCardComponent, AppTableComponent],
  templateUrl: './menu-operations.component.html',
  styleUrl: './menu-operations.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MenuOperationsComponent {
  private fb = inject(FormBuilder);
  langService = inject(LanguageService);

  activeTab = signal<'products' | 'pricing' | 'categories'>('products');
  showProductForm = signal(false);
  editingProductId = signal<string | null>(null);

  products = signal<MenuProduct[]>([
    { id: 'P-001', nameAr: 'قهوة عربية', nameEn: 'Arabic Coffee', category: 'مشروبات', sku: 'BEV-001', dineIn: 12, takeaway: 12, delivery: 14, isActive: true },
    { id: 'P-002', nameAr: 'برجر كلاسيك', nameEn: 'Classic Burger', category: 'أطباق رئيسية', sku: 'MAIN-001', dineIn: 28, takeaway: 26, delivery: 30, isActive: true },
    { id: 'P-003', nameAr: 'كنافة', nameEn: 'Kunafa', category: 'حلويات', sku: 'DES-001', dineIn: 22, takeaway: 22, delivery: 25, isActive: true }
  ]);

  priceLevels = signal<PriceLevelRow[]>([
    { id: 'PL-1', nameAr: 'محلي', nameEn: 'Dine In', channel: 'DineIn', isDefault: true },
    { id: 'PL-2', nameAr: 'سفري', nameEn: 'Takeaway', channel: 'TakeAway', isDefault: false },
    { id: 'PL-3', nameAr: 'توصيل', nameEn: 'Delivery', channel: 'Delivery', isDefault: false }
  ]);

  categories = signal<string[]>(['مشروبات', 'أطباق رئيسية', 'حلويات', 'إضافات']);

  productForm = this.fb.group({
    nameAr: ['', Validators.required],
    nameEn: ['', Validators.required],
    category: ['', Validators.required],
    sku: ['', Validators.required],
    dineIn: [0, [Validators.required, Validators.min(0)]],
    takeaway: [0, [Validators.required, Validators.min(0)]],
    delivery: [0, [Validators.required, Validators.min(0)]]
  });

  priceLevelForm = this.fb.group({
    nameAr: ['', Validators.required],
    nameEn: ['', Validators.required],
    channel: ['DineIn', Validators.required]
  });

  categoryForm = this.fb.group({ name: ['', Validators.required] });

  productColumns = computed(() => {
    this.langService.language();
    return [
      { key: 'sku', label: this.t('menu.col.sku'), sortable: true },
      { key: 'nameAr', label: this.t('menu.col.nameAr'), sortable: true },
      { key: 'nameEn', label: this.t('menu.col.nameEn'), sortable: true },
      { key: 'category', label: this.t('menu.col.category'), sortable: true },
      { key: 'dineIn', label: this.t('menu.col.dineIn'), sortable: true },
      { key: 'takeaway', label: this.t('menu.col.takeaway'), sortable: true },
      { key: 'delivery', label: this.t('menu.col.delivery'), sortable: true },
      { key: 'actions', label: this.t('menu.col.actions'), sortable: false }
    ];
  });

  openAddProduct(): void {
    this.editingProductId.set(null);
    this.productForm.reset({ dineIn: 0, takeaway: 0, delivery: 0 });
    this.showProductForm.set(true);
  }

  openEditProduct(product: MenuProduct): void {
    this.editingProductId.set(product.id);
    this.productForm.patchValue(product);
    this.showProductForm.set(true);
  }

  saveProduct(): void {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }

    const value = this.productForm.getRawValue();
    const payload: MenuProduct = {
      id: this.editingProductId() ?? `P-${Date.now()}`,
      nameAr: value.nameAr!,
      nameEn: value.nameEn!,
      category: value.category!,
      sku: value.sku!,
      dineIn: Number(value.dineIn),
      takeaway: Number(value.takeaway),
      delivery: Number(value.delivery),
      isActive: true
    };

    if (this.editingProductId()) {
      this.products.update(list => list.map(p => p.id === payload.id ? payload : p));
    } else {
      this.products.update(list => [...list, payload]);
    }

    this.showProductForm.set(false);
  }

  deleteProduct(id: string): void {
    this.products.update(list => list.filter(p => p.id !== id));
  }

  addPriceLevel(): void {
    if (this.priceLevelForm.invalid) return;
    const v = this.priceLevelForm.getRawValue();
    this.priceLevels.update(list => [...list, {
      id: `PL-${Date.now()}`,
      nameAr: v.nameAr!,
      nameEn: v.nameEn!,
      channel: v.channel!,
      isDefault: false
    }]);
    this.priceLevelForm.reset({ channel: 'DineIn' });
  }

  addCategory(): void {
    if (this.categoryForm.invalid) return;
    const name = this.categoryForm.value.name!.trim();
    if (!this.categories().includes(name)) {
      this.categories.update(list => [...list, name]);
    }
    this.categoryForm.reset();
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
