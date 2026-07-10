import { Injectable, signal, computed, inject } from '@angular/core';
import { Product, CartItem, Employee, InventoryItem, BrandingConfig } from '../models/erp.models';
import { KdsService } from './kds.service';
import { SignalrService } from './signalr.service';

@Injectable({
  providedIn: 'root'
})
export class DataService {
  // Global branch settings
  branches = ['وسط المدينة', 'المنطقة الشمالية', 'طريق السريع'];
  selectedBranch = signal<string>('وسط المدينة');
  
  // Inject services
  signalrService = inject(SignalrService);

  // 1. POS Menu Products
  products: Product[] = [
    {
      id: 'p1',
      name: 'برجر واغيو الذهبي',
      price: 24.00,
      description: 'قطعة لحم واغيو فاخرة مع جبن معتق، بصل مكرمل، كمأة، ورقائق ذهبية.',
      category: 'برجر',
      categoryKey: 'burgers',
      image: 'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=600&auto=format&fit=crop&q=80'
    },
    {
      id: 'p2',
      name: 'بطاطس الكمأة',
      price: 10.00,
      description: 'بطاطس مقرمشة مع زيت الكمأة الأسود وجبن البارميزان والبقدونس.',
      category: 'مقبلات',
      categoryKey: 'sides',
      image: 'https://images.unsplash.com/photo-1573080496219-bb080dd4f877?w=600&auto=format&fit=crop&q=80'
    },
    {
      id: 'p3',
      name: 'كوكتيل أولد فاشن',
      price: 18.00,
      description: 'كوكتيل كلاسيكي من ويسكي الجاودة مع المرارة وقشر البرتقال.',
      category: 'مشروبات',
      categoryKey: 'drinks',
      image: 'https://images.unsplash.com/photo-1514362545857-3bc16c4c7d1b?w=600&auto=format&fit=crop&q=80'
    },
    {
      id: 'p4',
      name: 'اسكالوبس مقلي',
      price: 32.00,
      description: 'اسكالوبس بحري طازج مع صلصة الزبدة وهريس الجزر الأبيض.',
      category: 'حلويات',
      categoryKey: 'desserts',
      image: 'https://images.unsplash.com/photo-1532636875304-0c8fe1197e1d?w=600&auto=format&fit=crop&q=80'
    },
    {
      id: 'p5',
      name: 'تارت التوت الحرفي',
      price: 8.00,
      description: 'عجينة هشة مع كريمة الكاسترد وتوت الغابة الطازج.',
      category: 'حلويات',
      categoryKey: 'desserts',
      image: 'https://images.unsplash.com/photo-1587314168485-3236d6710814?w=600&auto=format&fit=crop&q=80'
    }
  ];

  // 2. POS Cart State
  cart = signal<CartItem[]>([]);

  // Computed Cart Calculations
  cartSubtotal = computed(() => {
    return this.cart().reduce((sum, item) => sum + (item.product.price * item.quantity), 0);
  });
  
  cartTax = computed(() => {
    return this.cartSubtotal() * 0.05; // 5% TAX
  });
  
  cartTotal = computed(() => {
    return this.cartSubtotal() + this.cartTax();
  });

  // Cart Helper Methods
  addToCart(product: Product, customizations: string[] = [], notes: string = ''): void {
    const id = `${product.id}-${customizations.join('-')}-${notes}`;
    this.cart.update(currentCart => {
      const existing = currentCart.find(item => item.id === id);
      if (existing) {
        return currentCart.map(item => item.id === id ? { ...item, quantity: item.quantity + 1 } : item);
      } else {
        return [...currentCart, { id, product, quantity: 1, customizations, notes }];
      }
    });
  }

  removeFromCart(cartItemId: string): void {
    this.cart.update(currentCart => currentCart.filter(item => item.id !== cartItemId));
  }

  updateQuantity(cartItemId: string, delta: number): void {
    this.cart.update(currentCart => {
      return currentCart.map(item => {
        if (item.id === cartItemId) {
          const newQty = item.quantity + delta;
          return newQty > 0 ? { ...item, quantity: newQty } : null;
        }
        return item;
      }).filter((item): item is CartItem => item !== null);
    });
  }

  clearCart(): void {
    this.cart.set([]);
  }

  setCart(items: CartItem[]): void {
    this.cart.set(items.map(item => ({
      ...item,
      product: { ...item.product },
      customizations: [...item.customizations]
    })));
  }

  // 3. KDS — delegated to KdsService
  private kdsService = inject(KdsService);
  kdsTickets = this.kdsService.tickets;

  bumpTicket(ticketId: string): void {
    const ticket = this.kdsTickets().find(t => t.id === ticketId);
    if (ticket) this.kdsService.bumpTicket(ticket);
  }

  // 4. Employees State (HR Module)
  employees: Employee[] = [
    {
      id: 'e1',
      name: 'جوليان ستيرلينغ',
      title: 'رئيس الطهاة التنفيذي',
      department: 'عمليات المطبخ',
      email: 'j.sterling@sterlinghospitality.com',
      phone: '+966 50 123 4567',
      address: 'الرياض، المملكة العربية السعودية',
      hireDate: new Date('2023-04-15'),
      tenure: '٣.٢ سنة',
      performance: 'A+',
      attendance: '٩٨٪',
      avatar: 'https://images.unsplash.com/photo-1577219491135-ce391730fb2c?w=150&auto=format&fit=crop&q=80',
      summary: 'محترف طهي بخبرة تتجاوز ١٥ عاماً في مطاعم حاصلة على نجمة ميشلان.',
      leaveBalances: [
        { type: 'إجازة مدفوعة', taken: 12, total: 30 },
        { type: 'إجازة مرضية', taken: 3, total: 10 }
      ],
      performanceNotes: 'أظهر قيادة استثنائية وخفّض الهدر بنسبة ١٢٪ في الربع الأخير.'
    }
  ];

  // 5. Inventory Items State
  inventoryItems = signal<InventoryItem[]>([
    {
      id: 'i1',
      name: 'لحم واغيو بالكمأة',
      batchNumber: 'دفعة #W-23829',
      location: 'التخزين البارد الرئيسي',
      stockLevel: '150 كجم',
      unitPrice: 32.00,
      totalValue: 4800.00,
      status: 'in_stock'
    },
    {
      id: 'i2',
      name: 'سمك القاروس الطازج',
      batchNumber: 'دفعة #S-10294',
      location: 'مجمد الأسماك',
      stockLevel: '5 كجم',
      unitPrice: 45.00,
      totalValue: 225.00,
      status: 'low_stock'
    },
    {
      id: 'i3',
      name: 'خليط السلطة العضوية',
      batchNumber: 'دفعة #G-00293',
      location: 'ثلاجة المشي ١',
      stockLevel: '0 كيس',
      unitPrice: 4.50,
      totalValue: 0.00,
      status: 'out_of_stock'
    }
  ]);

  updateStock(itemId: string, newLevel: string, newStatus: 'in_stock' | 'low_stock' | 'out_of_stock'): void {
    this.inventoryItems.update(currentItems => {
      return currentItems.map(item => {
        if (item.id === itemId) {
          const numericLevel = parseFloat(newLevel);
          const totalValue = isNaN(numericLevel) ? item.totalValue : numericLevel * item.unitPrice;
          return { ...item, stockLevel: newLevel, status: newStatus, totalValue };
        }
        return item;
      });
    });
  }

  // 6. Branding & Identity State
  branding = signal<BrandingConfig>({
    name: 'GastroERP',
    position: 'Header',
    aspectRatio: 'Square (1:1)',
    clickAction: 'Redirect to Home',
    logoUrl: 'https://images.unsplash.com/photo-1543007630-9710e4a00a20?w=150&auto=format&fit=crop&q=80',
    loginBgUrl: 'https://images.unsplash.com/photo-1544025162-d76694265947?w=1200&auto=format&fit=crop&q=80',
    primaryColor: '#845ec2',
    accentColor: '#ff9671',
    fontFamily: 'Tajawal'
  });

  updateBranding(config: Partial<BrandingConfig>): void {
    this.branding.update(current => {
      const next = { ...current, ...config };
      localStorage.setItem('gastro_branding_config', JSON.stringify(next));
      return next;
    });
  }

  // Timer interval simulation for KDS order tickets
  constructor() {
    const cached = localStorage.getItem('gastro_branding_config');
    if (cached) {
      try {
        this.branding.set(JSON.parse(cached));
      } catch (e) {}
    }

    // Real-Time inventory updates
    this.signalrService.inventoryUpdated$.subscribe(data => {
      this.inventoryItems.update(currentItems => {
        return currentItems.map(item => {
          if (item.id === data.itemId) {
            return {
              ...item,
              stockLevel: data.stockLevel,
              status: data.status as 'in_stock' | 'low_stock' | 'out_of_stock'
            };
          }
          return item;
        });
      });
    });

    // Inventory list is managed by InventoryService (inventory feature module).
  }
}
