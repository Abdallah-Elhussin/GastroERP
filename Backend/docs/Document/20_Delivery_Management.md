# GastroERP Backend Roadmap
# Phase 20 — Delivery Management Implementation Plan

الإصدار: 1.0  
الحالة: جاهز للتنفيذ  
التاريخ: 2026-07-07  
المرجع: Phase 19 مكتمل (Invoicing, Taxation & Fiscal Compliance)

---

# الحالة الحالية

> ✅ Phase 16 — POS Core  
> ✅ Phase 17 — Payments & Cash Management  
> ✅ Phase 18 — Kitchen, Dining & KDS  
> ✅ Phase 19 — Invoicing, Taxation & Fiscal Compliance  

**Build Target:** 0 Errors / 0 Warnings

**Pending Migrations (لا تُطبَّق بعد):**
- AddSalesOrderTables
- AddPaymentsAndCashManagement
- AddKitchenDiningAndKDS
- AddInvoicingTaxationAndFiscalCompliance

---

# الهدف

تنفيذ نظام إدارة التوصيل (Delivery Management) المتكامل مع:
- Sales Orders (Takeaway / Delivery order types)
- Kitchen (تذاكر التحضير للتوصيل)
- Payments (دفع عند الاستلام / مسبق)
- Invoicing (فاتورة التوصيل)
- Organization (الفروع، السائقين، الأجهزة)

---

# المرحلة 1 — Domain Layer

## 1.1 Entities الجديدة

```
GastroErp.Domain/Entities/Delivery/
├── DeliveryOrder.cs          (+ DeliveryOrderItem snapshot)
├── DeliveryZone.cs           (مناطق التوصيل)
├── DeliveryDriver.cs         (السائق / مندوب التوصيل)
├── DeliveryAssignment.cs     (تعيين طلب لسائق)
└── DeliveryTracking.cs       (حالات التتبع)
```

## 1.2 Enums الجديدة

- `DeliveryStatus` — Pending, Assigned, PickedUp, InTransit, Delivered, Failed, Cancelled
- `DeliveryPriority` — Normal, Express, Scheduled
- `DriverStatus` — Available, OnDelivery, OffDuty, Suspended
- `DeliveryPaymentMode` — Prepaid, CashOnDelivery, CardOnDelivery

(ملاحظة: `DeliveryStatus` مذكور في Phase 18 plan كمرحلة 19 — يُنشأ الآن)

## 1.3 Domain Events

- `DeliveryOrderCreatedEvent`
- `DeliveryAssignedEvent`
- `DeliveryPickedUpEvent`
- `DeliveryCompletedEvent`
- `DeliveryFailedEvent`
- `DriverStatusChangedEvent`

## 1.4 تعديل SalesOrder

- ربط `DeliveryOrderId` (اختياري)
- عند `OrderType.Delivery` → إنشاء DeliveryOrder تلقائياً
- عند Complete delivery → إكمال SalesOrder

---

# المرحلة 2 — Delivery Zones & Pricing

## 2.1 DeliveryZone Aggregate

- مرتبط بـ Branch
- Polygon أو Radius (مركز + نصف قطر)
- رسوم توصيل ثابتة / حسب المسافة
- وقت تقديري (ETA minutes)

## 2.2 APIs

```
GET/POST   /sales/delivery/zones
PUT        /sales/delivery/zones/{id}
GET        /sales/delivery/zones/{id}/fee?lat=&lng=
```

---

# المرحلة 3 — Drivers & Assignments

## 3.1 DeliveryDriver

- مرتبط بـ User / Employee
- حالة السائق (Available, OnDelivery...)
- Branch assignment
- مركبة / رقم لوحة (اختياري)

## 3.2 DeliveryAssignment

- ربط DeliveryOrder ↔ Driver
- AssignedAt, PickedUpAt, DeliveredAt
- GPS coordinates (اختياري للمرحلة الأولى: nullable fields)

## 3.3 APIs

```
GET/POST   /sales/delivery/drivers
PATCH      /sales/delivery/drivers/{id}/status
POST       /sales/delivery/orders/{id}/assign
POST       /sales/delivery/orders/{id}/pickup
POST       /sales/delivery/orders/{id}/complete
POST       /sales/delivery/orders/{id}/fail
```

---

# المرحلة 4 — Delivery Order Lifecycle

```
Pending → Assigned → PickedUp → InTransit → Delivered
    ↓         ↓          ↓           ↓
 Cancelled  Cancelled  Failed      Failed
```

## تكامل Kitchen

- عند Confirm order نوع Delivery → Kitchen tickets كالمعتاد
- عند Ready → إشعار للسائق / لوحة التوصيل

## تكامل Payment

- COD: Payment عند `Delivered`
- Prepaid: Payment قبل التوصيل (موجود)

## تكامل Invoice

- فاتورة تُنشأ عند Complete order (موجود في Phase 19)
- توصيل COD → فاتورة + دفع عند التسليم

---

# المرحلة 5 — CQRS & Application

| Commands | Queries |
|----------|---------|
| CreateDeliveryZone | GetDeliveryZones |
| UpdateDeliveryZone | GetDeliveryZoneById |
| CreateDeliveryDriver | GetDrivers |
| UpdateDriverStatus | GetAvailableDrivers |
| CreateDeliveryOrder (auto) | GetDeliveryOrders |
| AssignDelivery | GetDeliveryById |
| PickUpDelivery | GetActiveDeliveriesByDriver |
| CompleteDelivery | GetDeliveryTracking |
| FailDelivery | |
| CancelDelivery | |

## Services

- `DeliveryFeeCalculationService`
- `DeliveryAssignmentService`
- `DeliveryEtaService`
- `DeliveryNumberGenerator`

---

# المرحلة 6 — Permissions

```csharp
Delivery.View, Delivery.Manage
DeliveryZone.View, DeliveryZone.Manage
Driver.View, Driver.Manage
```

---

# المرحلة 7 — Persistence

جداول جديدة:
- DeliveryZones, DeliveryDrivers
- DeliveryOrders, DeliveryAssignments
- DeliveryTrackingEvents

Migration واحدة: `AddDeliveryManagement`

---

# المرحلة 8 — REST API Summary

```
GET/POST   /sales/delivery/orders
GET        /sales/delivery/orders/{id}
POST       /sales/delivery/orders/{id}/assign
POST       /sales/delivery/orders/{id}/pickup
POST       /sales/delivery/orders/{id}/complete
POST       /sales/delivery/orders/{id}/fail
POST       /sales/delivery/orders/{id}/cancel

GET/POST   /sales/delivery/zones
GET/POST   /sales/delivery/drivers
```

---

# المرحلة 9 — ترتيب التنفيذ

```
Step 1:  Domain — enums + DeliveryZone + DeliveryDriver
Step 2:  Domain — DeliveryOrder + Assignment + Tracking
Step 3:  Persistence — EF configs + migration
Step 4:  Application — fee/assignment services
Step 5:  Application — Delivery CQRS + APIs
Step 6:  Integration — SalesOrder Create (Delivery type)
Step 7:  Integration — Kitchen Ready → delivery queue
Step 8:  Integration — COD payment on complete
Step 9:  Presentation — Controllers + permissions
Step 10: Build verify
```

---

# معايير القبول

- [ ] طلب Delivery يُنشأ تلقائياً من SalesOrder
- [ ] تعيين سائق ودورة حياة كاملة
- [ ] حساب رسوم التوصيل حسب المنطقة
- [ ] COD payment عند التسليم
- [ ] تكامل Kitchen (Ready → pickup)
- [ ] Build = 0 Errors / 0 Warnings
- [ ] Migration compiled (not applied)

---

نهاية خطة Phase 20
