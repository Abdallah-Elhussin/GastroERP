# GastroERP Backend Roadmap
# Phase 18 — Kitchen, Dining & KDS Implementation Plan

الإصدار: 1.0  
الحالة: جاهز للتنفيذ  
التاريخ: 2026-07-07  
المرجع: `17-POS_SALES_DOMAIN_DESIGN.md` + Phase 17 مكتمل

---

# الحالة الحالية

> ✅ Phase 16 — POS Core (SalesOrder, lifecycle, APIs)  
> ✅ Phase 17 — Payments & Cash Management  
> 🚧 Phase 18 — Kitchen, Dining & KDS

**Build Target:** 0 Errors / 0 Warnings

---

# الهدف

تنفيذ نظام المطبخ (KDS)، إدارة الطاولات، خطة الأرضية، والحجوزات — متكاملاً مع SalesOrder و Payments الموجودين.

---

# المرحلة 1 — Domain Layer

## 1.1 Entities الجديدة

```
GastroErp.Domain/Entities/Sales/
├── KitchenStation.cs
├── KitchenTicket.cs          (+ KitchenTicketItem)
├── FloorPlan.cs              (+ DiningArea, RestaurantTable)
└── Reservation.cs
```

## 1.2 Enums الجديدة (SalesEnums.cs)

- `KitchenTicketStatus` — Pending, InProgress, Ready, Completed, Cancelled
- `KitchenStationType` — Hot, Cold, Grill, Fry, Bar, Dessert, Expo, General
- `TableStatus` — Available, Occupied, Reserved, Cleaning, OutOfService
- `TableShape` — Square, Round, Rectangle, Bar
- `TableReservationStatus` — Pending, Confirmed, Seated, Completed, Cancelled, NoShow
- `DeliveryStatus` — (للمرحلة 19)

## 1.3 Domain Events

- `KitchenTicketCreatedEvent`, `KitchenTicketStartedEvent`, `KitchenTicketCompletedEvent`
- `KitchenItemReadyEvent`
- `TableStatusChangedEvent`
- `ReservationConfirmedEvent`, `ReservationCancelledEvent`

## 1.4 تعديل SalesOrder

- ربط `TableId` بـ `RestaurantTable` عبر Application layer
- عند Confirm → إنشاء KitchenTickets تلقائياً
- عند Complete/Cancel → تحديث حالة الطاولة

---

# المرحلة 2 — Kitchen Display System (KDS)

## 2.1 KitchenStation Aggregate

- مرتبط بـ Branch + Device (KDS)
- نوع المحطة (Hot, Cold, Grill...)
- ربط بـ Product categories أو recipe routing rules

## 2.2 KitchenTicket Aggregate

- مرتبط بـ SalesOrder
- تقسيم حسب KitchenStation
- KitchenTicketItem لكل OrderItem
- دورة حياة: Pending → InProgress → Ready → Completed

## 2.3 Kitchen Routing Service

```csharp
IKitchenRoutingService.RouteOrderAsync(SalesOrder order)
```

- يقرأ Recipe/Category لتحديد المحطة
- ينشئ ticket لكل محطة
- Fallback إلى General station

## 2.4 APIs

```
GET    /sales/kitchen/tickets              → قائمة التذاكر النشطة
GET    /sales/kitchen/tickets/{id}         → تفاصيل التذكرة
PATCH  /sales/kitchen/tickets/{id}/start   → بدء التحضير
PATCH  /sales/kitchen/tickets/{id}/ready   → جاهز
PATCH  /sales/kitchen/tickets/{id}/complete→ مكتمل
PATCH  /sales/kitchen/items/{id}/ready     → عنصر جاهز

GET    /sales/kitchen/stations             → المحطات
POST   /sales/kitchen/stations             → إنشاء محطة
PUT    /sales/kitchen/stations/{id}        → تعديل
```

---

# المرحلة 3 — Floor Plan & Tables

## 3.1 FloorPlan Aggregate

```
FloorPlan
  └── DiningArea (منطقة)
        └── RestaurantTable (طاولة)
```

## 3.2 RestaurantTable

- رقم، سعة، شكل، موقع (X,Y) على الخطة
- Status: Available, Occupied, Reserved, Cleaning
- CurrentOrderId

## 3.3 APIs

```
GET    /sales/floor-plans                  → الخطط
POST   /sales/floor-plans                  → إنشاء
GET    /sales/floor-plans/{id}/tables      → الطاولات
POST   /sales/tables/{id}/occupy           → شغل طاولة
POST   /sales/tables/{id}/release          → تحرير
PATCH  /sales/tables/{id}/status           → تغيير الحالة
```

## 3.4 تكامل مع SalesOrder

- DineIn يتطلب TableId (موجود)
- عند Create Order → Table.Occupy(orderId)
- عند Complete/Cancel → Table.Release()

---

# المرحلة 4 — Reservations

## 4.1 Reservation Aggregate

- TableId, CustomerName, Phone, GuestCount
- ReservationDate, DurationMinutes
- Status lifecycle
- ربط بـ SalesOrder عند Seated

## 4.2 APIs

```
GET    /sales/reservations                 → قائمة الحجوزات
POST   /sales/reservations                 → حجز جديد
POST   /sales/reservations/{id}/confirm    → تأكيد
POST   /sales/reservations/{id}/seat       → جلوس (ينشئ Order)
POST   /sales/reservations/{id}/cancel     → إلغاء
```

---

# المرحلة 5 — CQRS & Application

| Commands | Queries |
|----------|---------|
| CreateKitchenStation | GetKitchenTickets |
| CreateKitchenTicket (auto) | GetKitchenTicketById |
| StartKitchenTicket | GetActiveTicketsByStation |
| CompleteKitchenTicket | GetKitchenStations |
| CreateFloorPlan | GetFloorPlans |
| AddDiningArea | GetTablesByArea |
| AddRestaurantTable | GetTableById |
| OccupyTable / ReleaseTable | GetReservations |
| CreateReservation | GetReservationById |
| ConfirmReservation | |
| SeatReservation | |

---

# المرحلة 6 — Permissions

```csharp
Kitchen.View, Kitchen.Manage
Table.View, Table.Manage
Reservation.View, Reservation.Create, Reservation.Manage
FloorPlan.View, FloorPlan.Manage
```

---

# المرحلة 7 — Persistence

جداول جديدة:
- KitchenStations, KitchenTickets, KitchenTicketItems
- FloorPlans, DiningAreas, RestaurantTables
- Reservations

Migration واحدة: `AddKitchenDiningAndKDS`

---

# المرحلة 8 — ترتيب التنفيذ

```
Step 1:  Domain — Kitchen enums + KitchenStation + KitchenTicket
Step 2:  Domain — FloorPlan + DiningArea + RestaurantTable
Step 3:  Domain — Reservation
Step 4:  Persistence — EF configs + migration
Step 5:  Application — Kitchen routing service
Step 6:  Application — Kitchen CQRS + APIs
Step 7:  Application — Floor plan CQRS + APIs
Step 8:  Application — Reservation CQRS + APIs
Step 9:  Integration — SalesOrder confirm → kitchen tickets
Step 10: Integration — Table occupy/release
Step 11: Presentation — Controllers
Step 12: Build verify
```

---

# معايير القبول

- [ ] Kitchen tickets تُنشأ تلقائياً عند Confirm order
- [ ] KDS APIs تعمل (start, ready, complete)
- [ ] Floor plan + tables CRUD
- [ ] Table occupy/release مع orders
- [ ] Reservations CRUD + seat flow
- [ ] Build = 0 Errors / 0 Warnings
- [ ] Migration compiled (not applied)

---

نهاية خطة Phase 18
