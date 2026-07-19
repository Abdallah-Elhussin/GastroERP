export type InventoryItemKind = 'raw' | 'manufactured';

export type WarehouseType =
  | 'Main'
  | 'POS'
  | 'Production'
  | 'RawMaterial'
  | 'FinishedGoods'
  | 'Returns'
  | 'Damaged'
  | 'Transit'
  | 'Kitchen'
  | 'Beverage'
  | 'DryStore'
  | 'Chiller'
  | 'Freezer'
  | 'Packaging'
  | 'Cleaning'
  | 'Waste';

export const WAREHOUSE_TYPES: WarehouseType[] = [
  'Main',
  'POS',
  'Production',
  'RawMaterial',
  'FinishedGoods',
  'Returns',
  'Damaged',
  'Transit',
  'Kitchen',
  'Beverage',
  'DryStore',
  'Chiller',
  'Freezer',
  'Packaging',
  'Cleaning',
  'Waste'
];

export interface WarehouseTypeDefinition {
  id: string;
  tenantId?: string;
  code: string;
  nameAr: string;
  nameEn?: string;
  description?: string;
  sortOrder: number;
  isSystem: boolean;
  isActive: boolean;
}

export interface BranchLookup {
  id: string;
  nameAr: string;
  nameEn?: string;
  code?: string;
}

export interface InventoryCategory {
  id: string;
  tenantId?: string;
  parentCategoryId?: string | null;
  code: string;
  nameAr: string;
  nameEn?: string;
  descriptionAr?: string;
  descriptionEn?: string;
  icon?: string;
  imageUrl?: string;
  color?: string;
  sortOrder: number;
  isActive: boolean;
  createdAt?: string;
}

export interface CreateInventoryCategoryPayload {
  nameAr: string;
  nameEn?: string;
  parentCategoryId?: string | null;
  code?: string;
  descriptionAr?: string;
  descriptionEn?: string;
  icon?: string;
  imageUrl?: string;
  color?: string;
  sortOrder?: number;
}

export interface UpdateInventoryCategoryPayload extends CreateInventoryCategoryPayload {}

export type InventoryUnitType = 'Measured' | 'Count' | number;
export type InventoryUnitClassification =
  | 'Weight'
  | 'Volume'
  | 'Length'
  | 'Count'
  | 'Packaging'
  | 'Other'
  | number;

export const INVENTORY_UNIT_TYPES = [
  { value: 1, key: 'Measured' as const, labelKey: 'inv.units.type.measured' },
  { value: 2, key: 'Count' as const, labelKey: 'inv.units.type.count' }
];

export const INVENTORY_UNIT_CLASSIFICATIONS = [
  { value: 1, key: 'Weight' as const, labelKey: 'inv.units.class.weight' },
  { value: 2, key: 'Volume' as const, labelKey: 'inv.units.class.volume' },
  { value: 3, key: 'Length' as const, labelKey: 'inv.units.class.length' },
  { value: 4, key: 'Count' as const, labelKey: 'inv.units.class.count' },
  { value: 5, key: 'Packaging' as const, labelKey: 'inv.units.class.packaging' },
  { value: 6, key: 'Other' as const, labelKey: 'inv.units.class.other' }
];

export interface InventoryUnit {
  id: string;
  tenantId?: string;
  code: string;
  nameAr: string;
  nameEn?: string;
  symbol: string;
  symbolAr?: string;
  decimalPlaces: number;
  baseUnitId?: string | null;
  conversionFactor: number;
  unitType: number;
  classification: number;
  sortOrder: number;
  isActive: boolean;
}

export interface CreateInventoryUnitPayload {
  nameAr: string;
  symbol: string;
  nameEn?: string;
  symbolAr?: string;
  code?: string;
  decimalPlaces?: number;
  baseUnitId?: string | null;
  conversionFactor?: number;
  unitType?: number;
  classification?: number;
  sortOrder?: number;
  isActive?: boolean;
}

export interface UpdateInventoryUnitPayload extends CreateInventoryUnitPayload {}

export interface Warehouse {
  id: string;
  tenantId?: string;
  branchId?: string | null;
  companyId?: string | null;
  nameAr: string;
  nameEn?: string;
  code?: string;
  address?: string;
  phone?: string;
  email?: string;
  notes?: string;
  warehouseType: WarehouseType | number;
  warehouseTypeId?: string | null;
  warehouseTypeNameAr?: string | null;
  parentWarehouseId?: string | null;
  parentWarehouseNameAr?: string | null;
  branchNameAr?: string | null;
  managerUserId?: string | null;
  responsibleEmployeeId?: string | null;
  allowPurchase: boolean;
  allowSales: boolean;
  allowTransfer: boolean;
  allowInventoryCount: boolean;
  allowManufacturing: boolean;
  allowNegativeStock: boolean;
  allowReservation: boolean;
  allowReceiving: boolean;
  allowIssue: boolean;
  allowAdjustment: boolean;
  isPosWarehouse: boolean;
  isDefault: boolean;
  isSystem: boolean;
  useBins: boolean;
  isActive: boolean;
  zoneCount: number;
  createdAt?: string;
}

export interface CreateWarehousePayload {
  nameAr: string;
  nameEn?: string;
  code?: string;
  branchId?: string | null;
  companyId?: string | null;
  warehouseType?: WarehouseType | number;
  warehouseTypeId?: string | null;
  parentWarehouseId?: string | null;
  address?: string;
  phone?: string;
  email?: string;
  notes?: string;
  managerUserId?: string | null;
  responsibleEmployeeId?: string | null;
  allowPurchase?: boolean;
  allowSales?: boolean;
  allowTransfer?: boolean;
  allowInventoryCount?: boolean;
  allowManufacturing?: boolean;
  allowNegativeStock?: boolean;
  allowReservation?: boolean;
  allowReceiving?: boolean;
  allowIssue?: boolean;
  allowAdjustment?: boolean;
  isPosWarehouse?: boolean;
  isDefault?: boolean;
  useBins?: boolean;
  isActive?: boolean;
}

export interface UpdateWarehousePayload extends CreateWarehousePayload {}

export interface InventoryItemDefinition {
  id: string;
  tenantId: string;
  categoryId: string;
  categoryNameAr: string;
  nameAr: string;
  nameEn?: string;
  descriptionAr?: string;
  descriptionEn?: string;
  sku?: string;
  barcode?: string;
  imageUrl?: string;
  itemKind: InventoryItemKind;
  baseUnitId: string;
  baseUnitNameAr: string;
  defaultPurchaseUnitId?: string;
  defaultRecipeUnitId?: string;
  reorderLevel: number;
  reorderQuantity: number;
  averageUnitCost?: number;
  lastPurchaseUnitCost?: number;
  isActive: boolean;
}

export interface CreateInventoryItemPayload {
  categoryId: string;
  nameAr: string;
  baseUnitId: string;
  nameEn?: string;
  descriptionAr?: string;
  descriptionEn?: string;
  sku?: string;
  barcode?: string;
  imageUrl?: string;
  itemKind: InventoryItemKind;
  defaultPurchaseUnitId?: string;
  defaultRecipeUnitId?: string;
  reorderLevel?: number;
  reorderQuantity?: number;
}

export interface UpdateInventoryItemPayload extends CreateInventoryItemPayload {}

export interface WarehouseStockBalance {
  warehouseId: string;
  warehouseNameAr: string;
  warehouseCode?: string;
  onHand: number;
  reserved: number;
  available: number;
  ordered: number;
  incoming: number;
}

export interface ItemStockMovement {
  movementId: string;
  transactionId: string;
  occurredAt: string;
  transactionType: string;
  referenceDocumentNumber?: string;
  warehouseId: string;
  warehouseNameAr: string;
  quantityChange: number;
  unitCost: number;
  totalCost: number;
}

export interface ItemPurchaseHistory {
  purchaseOrderId: string;
  poNumber: string;
  supplierId: string;
  supplierNameAr: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
  orderDate: string;
  status: string;
}

export interface ItemSalesHistory {
  salesOrderId: string;
  orderNumber: string;
  customerId?: string;
  customerName?: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
  orderDate: string;
  status: string;
}

/** Phase E — inventory operations */
export interface InventoryLedgerEntry {
  id: string;
  tenantId: string;
  transactionType: string;
  referenceDocumentNumber: string;
  referenceDocumentId: string;
  transactionDate: string;
  notes?: string;
  movementCount: number;
}

export interface StockTransferRecord {
  id: string;
  tenantId: string;
  sourceWarehouseId: string;
  sourceWarehouseNameAr: string;
  destinationWarehouseId: string;
  destinationWarehouseNameAr: string;
  transferNumber: string;
  transferDate: string;
  status: string;
  notes?: string;
  createdAt: string;
}

export interface CreateStockTransferPayload {
  sourceWarehouseId: string;
  destinationWarehouseId: string;
  transferNumber: string;
  notes?: string;
  inventoryItemId: string;
  unitId: string;
  quantity: number;
  complete?: boolean;
}

export interface StockAdjustmentRecord {
  id: string;
  tenantId: string;
  warehouseId: string;
  warehouseNameAr: string;
  inventoryItemId: string;
  itemNameAr: string;
  adjustmentNumber: string;
  quantityAdjusted: number;
  reasonId?: string;
  notes?: string;
  adjustmentDate: string;
  createdAt: string;
}

export interface CreateStockAdjustmentPayload {
  warehouseId: string;
  inventoryItemId: string;
  adjustmentNumber: string;
  quantityAdjusted: number;
  unitId: string;
  unitCost: number;
  reasonId?: string;
  notes?: string;
  confirm?: boolean;
}

export interface WasteRecord {
  id: string;
  tenantId: string;
  warehouseId: string;
  warehouseNameAr: string;
  inventoryItemId: string;
  itemNameAr: string;
  wasteNumber: string;
  quantity: number;
  unitCost: number;
  reasonId?: string;
  notes?: string;
  wasteDate: string;
  createdAt: string;
}

export interface CreateWastePayload {
  warehouseId: string;
  inventoryItemId: string;
  unitId: string;
  wasteNumber: string;
  quantity: number;
  unitCost: number;
  reasonId?: string;
  notes?: string;
  confirm?: boolean;
}

export interface GoodsReceiptRecord {
  id: string;
  tenantId: string;
  purchaseOrderId?: string | null;
  poNumber: string;
  warehouseId: string;
  warehouseNameAr: string;
  grnNumber: string;
  receiptDate: string;
  notes?: string;
  lineCount: number;
  createdAt: string;
}

export interface CreateGoodsReceiptPayload {
  purchaseOrderId: string;
  warehouseId: string;
  grnNumber: string;
  notes?: string;
  inventoryItemId: string;
  unitId: string;
  receivedQuantity: number;
  unitCost: number;
  purchaseOrderLineId?: string;
  confirm?: boolean;
}

export interface StockCountRecord {
  id: string;
  tenantId: string;
  warehouseId: string;
  countNumber: string;
  countDate: string;
  status: string | number;
  notes?: string;
}

export interface CreateStockCountPayload {
  warehouseId: string;
  countNumber: string;
  notes?: string;
  inventoryItemId: string;
  unitId: string;
  expectedQuantity: number;
  actualQuantity: number;
}

export interface PurchaseReturnRecord {
  id: string;
  tenantId: string;
  supplierId: string;
  warehouseId: string;
  goodsReceiptId?: string | null;
  returnNumber: string;
  returnDate: string;
  reason?: string;
  isCompleted: boolean;
}

export interface CreatePurchaseReturnPayload {
  supplierId: string;
  warehouseId: string;
  returnNumber: string;
  goodsReceiptId?: string;
  reason?: string;
  inventoryItemId: string;
  unitId: string;
  returnQuantity: number;
  unitCost: number;
  approve?: boolean;
}

export interface PurchaseOrderSummary {
  id: string;
  tenantId: string;
  supplierId: string;
  supplierNameAr: string;
  destinationWarehouseId: string;
  warehouseNameAr: string;
  poNumber: string;
  orderDate: string;
  status: string | number;
  totalAmount: number;
  currency: string;
  lineCount: number;
}

export interface SupplierSummary {
  id: string;
  nameAr: string;
  nameEn?: string;
  isActive: boolean;
}

/** Phase F — inventory dashboard */
export interface InventoryDashboardSummary {
  totalItems: number;
  activeItems: number;
  inactiveItems: number;
  categoryCount: number;
  lowStockWatchlist: number;
  warehouseCount: number;
  activeWarehouses: number;
  openTransfers: number;
  openStockCounts: number;
  activeReservations: number;
  draftGoodsReceipts: number;
  uncompletedWaste: number;
  warehouses: InventoryDashboardWarehouse[];
  recentActivities: InventoryDashboardActivity[];
  alerts: InventoryDashboardAlert[];
  topMovers: InventoryDashboardTopMover[];
  categoryDistribution: InventoryDashboardCategorySlice[];
}

export interface InventoryDashboardTopMover {
  inventoryItemId: string;
  nameAr: string;
  nameEn?: string;
  inQuantity: number;
  outQuantity: number;
}

export interface InventoryDashboardCategorySlice {
  categoryId?: string | null;
  nameAr: string;
  nameEn?: string;
  itemCount: number;
}

export interface InventoryDashboardWarehouse {
  id: string;
  nameAr: string;
  nameEn?: string;
  code?: string;
  warehouseType: WarehouseType | number;
  isActive: boolean;
}

export interface InventoryDashboardActivity {
  id: string;
  kind: string;
  reference: string;
  occurredAt: string;
  notes?: string;
}

export interface InventoryDashboardAlert {
  code: string;
  severity: 'info' | 'warning' | 'danger' | string;
  messageEn: string;
  messageAr: string;
  path?: string;
}

/** Phase H — reservations */
export interface InventoryReservationRecord {
  id: string;
  tenantId: string;
  warehouseId: string;
  inventoryItemId: string;
  inventoryItemNameAr: string;
  reservedQuantity: number;
  sourceDocument: string;
  status: string | number;
  expirationDate?: string | null;
}

export interface CreateReservationPayload {
  warehouseId: string;
  inventoryItemId: string;
  reservedQuantity: number;
  sourceDocument: string;
  expirationDate?: string | null;
}

/** Phase I — inventory settings */
export type InventoryCostingMethod = 'FIFO' | 'WeightedAverage' | 'StandardCost' | number;

export const INVENTORY_COSTING_METHODS = [
  { value: 1, key: 'FIFO' as const, enabled: false },
  { value: 2, key: 'WeightedAverage' as const, enabled: true },
  { value: 3, key: 'StandardCost' as const, enabled: false }
];

export type InventoryDocumentSeriesType =
  | 'GoodsReceipt'
  | 'GoodsIssue'
  | 'StockTransfer'
  | 'InventoryAdjustment'
  | 'InventoryCount'
  | 'Waste'
  | 'OpeningBalance'
  | 'Reservation'
  | 'ProductionIssue'
  | 'ProductionReceipt'
  | number;

export const INVENTORY_DOCUMENT_SERIES_TYPES: { value: number; key: InventoryDocumentSeriesType; labelKey: string }[] = [
  { value: 1, key: 'GoodsReceipt', labelKey: 'inv.settings.doc.goodsReceipt' },
  { value: 2, key: 'GoodsIssue', labelKey: 'inv.settings.doc.goodsIssue' },
  { value: 3, key: 'StockTransfer', labelKey: 'inv.settings.doc.stockTransfer' },
  { value: 4, key: 'InventoryAdjustment', labelKey: 'inv.settings.doc.adjustment' },
  { value: 5, key: 'InventoryCount', labelKey: 'inv.settings.doc.count' },
  { value: 6, key: 'Waste', labelKey: 'inv.settings.doc.waste' },
  { value: 7, key: 'OpeningBalance', labelKey: 'inv.settings.doc.opening' },
  { value: 8, key: 'Reservation', labelKey: 'inv.settings.doc.reservation' },
  { value: 9, key: 'ProductionIssue', labelKey: 'inv.settings.doc.productionIssue' },
  { value: 10, key: 'ProductionReceipt', labelKey: 'inv.settings.doc.productionReceipt' }
];

export interface InventoryDocumentNumberSeries {
  id?: string;
  documentType: number;
  prefix: string;
  numberLength: number;
  nextNumber: number;
  autoIncrement: boolean;
}

export interface InventorySetting {
  id: string;
  tenantId: string;
  companyId?: string | null;
  branchId?: string | null;
  defaultWarehouseId?: string | null;
  defaultUnitId?: string | null;
  defaultCurrencyCode?: string | null;
  autoGenerateItemCode: boolean;
  enableMultiWarehouse: boolean;
  enableWarehouseHierarchy: boolean;
  enableBatchTracking: boolean;
  enableSerialTracking: boolean;
  enableExpiryTracking: boolean;
  enableBarcode: boolean;
  enableQrCode: boolean;
  costingMethod: InventoryCostingMethod;
  costPrecision: number;
  roundCost: boolean;
  autoRecalculateCost: boolean;
  allowNegativeStock: boolean;
  checkAvailableQuantity: boolean;
  enableReservation: boolean;
  autoReleaseReservation: boolean;
  freezeDuringCount: boolean;
  allowZeroCost: boolean;
  allowNegativeCost: boolean;
  validateWarehouseBeforePosting: boolean;
  autoIssueRecipe: boolean;
  requireApprovalBeforePosting: boolean;
  autoPostAfterApproval: boolean;
  allowUnpost: boolean;
  createReverseEntry: boolean;
  lockPostedDocuments: boolean;
  allowEditDraft: boolean;
  allowDeleteDraft: boolean;
  enablePurchasingIntegration: boolean;
  enablePosIntegration: boolean;
  enableProductionIntegration: boolean;
  enableAccountingIntegration: boolean;
  enableKitchenIntegration: boolean;
  enableDeliveryIntegration: boolean;
  lowStockAlert: boolean;
  outOfStockAlert: boolean;
  nearExpiryAlert: boolean;
  expiredItemsAlert: boolean;
  cycleCountReminder: boolean;
  emailNotifications: boolean;
  pushNotifications: boolean;
  enableMultiCompany: boolean;
  enableMultiBranch: boolean;
  enableWarehouseZones: boolean;
  enableShelves: boolean;
  enableBins: boolean;
  enableRfid: boolean;
  enableMobileScanner: boolean;
  isActive: boolean;
  updatedAt: string;
  documentSeries: InventoryDocumentNumberSeries[];
}

export type UpsertInventorySettingPayload = Omit<InventorySetting, 'id' | 'tenantId' | 'updatedAt' | 'isActive'> & {
  branchId?: string | null;
};

/** Phase J — warehouse structure */
export interface WarehouseBin {
  id: string;
  shelfId: string;
  nameAr: string;
  nameEn?: string;
  code?: string;
  isActive: boolean;
}

export interface WarehouseShelfDetail {
  id: string;
  zoneId: string;
  nameAr: string;
  nameEn?: string;
  code?: string;
  isActive: boolean;
  bins: WarehouseBin[];
}

export interface WarehouseZoneDetail {
  id: string;
  warehouseId: string;
  nameAr: string;
  nameEn?: string;
  code?: string;
  isActive: boolean;
  shelves: WarehouseShelfDetail[];
}

export interface WarehouseDetail extends Warehouse {
  zones: WarehouseZoneDetail[];
}

export interface AddWarehouseLocationPayload {
  nameAr: string;
  nameEn?: string;
  code?: string;
}

/** Phase J — brands / manufacturers / attributes / price lists */
export interface InventoryBrand {
  id: string;
  tenantId: string;
  code: string;
  nameAr: string;
  nameEn?: string;
  isActive: boolean;
}

export interface UpsertInventoryBrandPayload {
  nameAr: string;
  nameEn?: string;
  code?: string;
}

export interface InventoryManufacturer {
  id: string;
  tenantId: string;
  code: string;
  nameAr: string;
  nameEn?: string;
  country?: string;
  isActive: boolean;
}

export interface UpsertInventoryManufacturerPayload {
  nameAr: string;
  nameEn?: string;
  code?: string;
  country?: string;
}

export type InventoryAttributeDataType = 'Text' | 'Number' | 'Boolean' | 'List' | number;

export const INVENTORY_ATTRIBUTE_DATA_TYPES = [
  { value: 1, key: 'Text' as const },
  { value: 2, key: 'Number' as const },
  { value: 3, key: 'Boolean' as const },
  { value: 4, key: 'List' as const }
];

export interface InventoryAttributeValue {
  id: string;
  attributeId: string;
  valueAr: string;
  valueEn?: string;
  sortOrder: number;
}

export interface InventoryAttribute {
  id: string;
  tenantId: string;
  code: string;
  nameAr: string;
  nameEn?: string;
  dataType: InventoryAttributeDataType;
  isActive: boolean;
  values: InventoryAttributeValue[];
}

export interface UpsertInventoryAttributePayload {
  nameAr: string;
  nameEn?: string;
  code?: string;
  dataType: number;
}

export interface InventoryPriceListLine {
  id: string;
  priceListId: string;
  inventoryItemId: string;
  inventoryItemNameAr?: string;
  unitId?: string | null;
  unitPrice: number;
}

export interface InventoryPriceList {
  id: string;
  tenantId: string;
  code: string;
  nameAr: string;
  nameEn?: string;
  currency: string;
  validFrom?: string | null;
  validTo?: string | null;
  isActive: boolean;
  lineCount: number;
  lines: InventoryPriceListLine[];
}

export interface UpsertInventoryPriceListPayload {
  nameAr: string;
  nameEn?: string;
  code?: string;
  currency?: string;
  validFrom?: string | null;
  validTo?: string | null;
}

export interface UpsertInventoryPriceListLinePayload {
  inventoryItemId: string;
  unitPrice: number;
  unitId?: string | null;
}

/** Reuses Invoicing TaxGroup API under inventory UX */
export interface InventoryTaxGroup {
  id: string;
  nameAr: string;
  nameEn?: string;
  isActive: boolean;
  description?: string;
  rates: { id: string; taxRateId: string; sortOrder: number }[];
}


