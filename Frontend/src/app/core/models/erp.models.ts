export interface Product {
  id: string;
  name: string;
  price: number;
  description: string;
  category: string;
  categoryKey: 'burgers' | 'sides' | 'drinks' | 'desserts';
  image: string;
}

export interface CartItem {
  id: string; // unique cart item instance ID (product.id + selected modifications)
  product: Product;
  quantity: number;
  customizations: string[];
  notes?: string;
}

export type KdsStatus = 'new' | 'preparing' | 'ready';

export interface KdsItem {
  name: string;
  quantity: number;
  notes?: string[];
}

export interface KdsStation {
  id: string;
  nameAr: string;
  nameEn?: string;
  stationType: number;
  sortOrder: number;
}

export interface KdsTicket {
  id: string;
  ticketNumber: string;
  tableNo: string;
  timer: number;
  items: KdsItem[];
  status: KdsStatus;
  orderTime: Date;
  stationId: string;
  stationType: number;
  stationNameAr?: string;
  stationNameEn?: string;
}

export interface LeaveBalance {
  type: string;
  taken: number;
  total: number;
}

export interface Employee {
  id: string;
  name: string;
  title: string;
  department: string;
  email: string;
  phone: string;
  address: string;
  hireDate: Date;
  tenure: string;
  performance: string;
  attendance: string;
  avatar: string;
  leaveBalances: LeaveBalance[];
  summary: string;
  performanceNotes: string;
}

export type InventoryStatus = 'in_stock' | 'low_stock' | 'out_of_stock';

export interface InventoryItem {
  id: string;
  name: string;
  batchNumber: string;
  location: string;
  stockLevel: string; // e.g., '150 kg', '12 kg'
  unitPrice: number;
  totalValue: number;
  status: InventoryStatus;
}

export interface BrandingConfig {
  name: string;
  position: 'Header' | 'Footer' | 'Sidebar';
  aspectRatio: 'Square (1:1)' | 'Wide (16:9)' | 'Banner';
  clickAction: 'Redirect to Home' | 'No Action';
  logoUrl: string;
  loginBgUrl: string;
  primaryColor?: string;
  accentColor?: string;
  fontFamily?: string;
}
