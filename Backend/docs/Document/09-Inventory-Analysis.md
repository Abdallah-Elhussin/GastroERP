# Inventory Module Analysis

## الهدف

إدارة جميع المواد الخام والمخزون والإنتاج بحيث يمكن للنظام حساب تكلفة كل منتج وخصم المواد الخام تلقائياً عند البيع.

---

# Aggregate Roots

InventoryItem

Warehouse

PurchaseOrder

Recipe

StockCount

StockTransfer

Supplier

GoodsReceipt

WasteRecord

---

# Entities

InventoryCategory

InventoryItem

InventoryUnit

UnitConversion

Recipe

RecipeItem

Warehouse

WarehouseBin

WarehouseItem

StockMovement

StockTransaction

StockReservation

Supplier

SupplierContact

PurchaseOrder

PurchaseOrderLine

GoodsReceipt

GoodsReceiptLine

PurchaseReturn

PurchaseReturnLine

StockAdjustment

StockAdjustmentLine

StockTransfer

StockTransferLine

StockCount

StockCountLine

WasteRecord

WasteItem

InventoryCost

InventoryBatch

ExpiryTracking

LotNumber

InventorySetting

---

# Relationships

Category

↓

InventoryItem

↓

RecipeItem

↓

Recipe

↓

Product

----------------------------

Warehouse

↓

WarehouseItem

↓

StockMovement

↓

InventoryTransaction

----------------------------

Supplier

↓

PurchaseOrder

↓

PurchaseOrderLine

↓

GoodsReceipt

↓

GoodsReceiptLine

----------------------------

Warehouse

↓

StockTransfer

↓

StockTransferLine

----------------------------

Warehouse

↓

StockCount

↓

StockCountLine

----------------------------

InventoryItem

↓

WasteRecord

↓

WasteItem

---

# Business Rules

- المادة الخام يمكن استخدامها في أكثر من وصفة.
- الوصفة تحتوي على أكثر من مادة خام.
- كل حركة مخزون غير قابلة للتعديل بعد اعتمادها.
- كل استلام يزيد المخزون.
- كل بيع يخصم من المخزون.
- الهدر ينقص المخزون.
- الجرد يصحح الرصيد.
- التحويل ينقل الكميات بين مستودعين.
- يسمح بتتبع التشغيلات والباركود وتاريخ الانتهاء.

End Of File