# GastroERP Backend Roadmap
# Phase 11B.0 — POS & Sales Domain Design
# Architecture Document (Design Only — No Code)

الإصدار: 1.0  
الحالة: معتمد للتنفيذ  
التاريخ: 2026-07-07

---

# 0. مقدمة

## 0.1 الهدف

تصميم السياق المقيد (Bounded Context) الكامل لـ **POS & Sales** لنظام GastroERP SaaS للمطاعم، متوافقاً مع:

- Clean Architecture
- Domain-Driven Design (DDD)
- CQRS
- Multi-Tenancy
- Offline-First (SQLite محلي + SQL Server سحابي)

## 0.2 الوضع الحالي

| مكتمل (Phase 11A) | غير موجود |
|-------------------|-----------|
| Identity, Organization, Menu, Inventory | أي كيانات POS/Sales |
| Database Hardening, Security, API Alignment | Order, Payment, Shift, Kitchen |

**Build Status:** 0 Errors / 0 Warnings

## 0.3 قرارات معمارية مبكرة

| القرار | الاختيار | المبرر |
|--------|----------|--------|
| تسمية الوردية | `CashierShift` | تجنب التعارض مع `WorkingShift` في Organization |
| تسمية الطلب | `SalesOrder` | وضوح في الكود؛ يُشار إليه تجارياً بـ Order |
| نموذج المال | `Money` VO للمدفوعات؛ `decimal + Currency` في snapshots | توحيد المدفوعات مع VO؛ snapshots تتبع نمط Menu |
| Kitchen Context | سياق فرعي داخل POS | متوافق مع `00-DOMAIN_MODEL_AND_CONTEXTS.md` |
| Offline Sync | Outbox Pattern + `Device.LastSyncAt` | يستفيد من البنية الموجودة في Device |
| مرجع Menu | Snapshot عند الإضافة | لا live reference — ACL pattern |

## 0.4 ما يُعاد استخدامه (لا تكرار)

| الموجود | الاستخدام في POS |
|---------|------------------|
| `Money`, `Address`, `PhoneNumber`, `GeoLocation` | مدفوعات، توصيل، حجوزات |
| `SalesChannel` (MenuEnums) | قناة البيع للطلب |
| `PriceLevel`, `ProductPriceLevel` | تسعير عبر ACL |
| `InventoryReservation` + `TransactionType.SalesConsumption` | حجز وخصم المخزون |
| `Recipe` → `ProductId` | استهلاك المكونات |
| `Device`, `BranchDevice` | الجهاز والمزامنة |
| `AppUser.PinCode`, `UserBranch` | تسجيل دخول الكاشير |
| `AuditableBaseEntity`, `ITenantEntity`, `IBranchEntity` | نمط الكيانات |
| `MessageCodes.OrderCreated`, `OrderCancelled` | رسائل النطاق |

---

# 1. Bounded Context Overview

## 1.1 المسؤوليات

سياق **Sales & POS** مسؤول عن:

- إنشاء وإدارة الطلبات (جميع قنوات البيع)
- تسعير الطلبات والخصومات والضرائب
- المدفوعات (كاملة، جزئية، مقسمة، استرداد)
- ورديات الكاشير والتسوية النقدية
- إدارة الطاولات وخطة الأرضية
- تذاكر المطبخ (KDS)
- الفواتير الضريبية (ZATCA-ready)
- التوصيل والحجوزات
- العروض والكوبونات

## 1.2 الحدود — ما ليس من مسؤولية POS

| خارج الحدود | السياق المالك |
|-------------|---------------|
| تعريف المنتجات والأسعار | Menu |
| حركات المخزون الفعلية | Inventory |
| المستخدمين والصلاحيات | Identity |
| الفروع والأجهزة | Organization |
| القيود المحاسبية | Finance (مستقبلي) |
| بيانات العملاء وبرامج الولاء | CRM (مستقبلي) |

## 1.3 خريطة السياق (Context Map)

```
┌──────────────────────────────────────────────────────────────────┐
│                   Sales & POS Context (NEW)                       │
│  ┌────────────┐ ┌─────────────┐ ┌──────────┐ ┌────────────────┐ │
│  │ SalesOrder │ │CashierShift │ │ Payment  │ │RestaurantTable │ │
│  └─────┬──────┘ └──────┬──────┘ └────┬─────┘ └────────────────┘ │
│        │               │             │                            │
│  ┌─────┴───────────────┴─────────────┴──────────────────────────┐│
│  │              Kitchen Sub-Context                              ││
│  │         KitchenTicket · KitchenStation                        ││
│  └──────────────────────────────────────────────────────────────┘│
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────────────────┐ │
│  │ CashRegister │ │   Invoice    │ │ DeliveryOrder · Reservation││
│  └──────────────┘ └──────────────┘ └──────────────────────────┘ │
└───────┬──────────┬──────────┬──────────┬─────────────────────────┘
        │          │          │          │
   ┌────┴───┐ ┌────┴────┐ ┌───┴────┐ ┌───┴──────┐
   │  Menu  │ │Inventory│ │  Org   │ │ Identity │
   │ (ACL)  │ │(Events) │ │ (Refs) │ │  (ACL)   │
   └────────┘ └─────────┘ └────────┘ └──────────┘
        │                              │
   ┌────┴────┐                    ┌────┴────┐
   │ Finance │                    │   CRM   │
   │(Future) │                    │(Future) │
   └─────────┘                    └─────────┘
```

## 1.4 أنماط التكامل

| السياق | النمط | الآلية |
|--------|-------|--------|
| Menu | ACL (Anti-Corruption Layer) | `IMenuPricingService` — snapshot عند إضافة OrderItem |
| Inventory | Domain Events + Application Orchestration | `OrderConfirmed` → Reserve؛ `OrderCompleted` → Consume |
| Organization | Shared Kernel (IDs فقط) | `TenantId`, `CompanyId`, `BranchId`, `DeviceId` |
| Identity | ACL | `CashierId`, `WaiterId`, `ManagerId` كـ GUID references |
| Finance | Integration Events (مستقبلي) | `OrderCompleted`, `InvoiceIssued` |
| CRM | Integration Events (مستقبلي) | `CustomerId` اختياري |

## 1.5 قواعد الحدود

1. **لا JOIN عبر السياقات** — مراجع بـ GUID فقط
2. **Menu data = snapshot** في OrderLine — لا live reference
3. **Inventory orchestration** عبر Application layer فقط
4. **POS هو مصدر الحقيقة** أثناء Offline
5. **كل كيان** يرث `AuditableBaseEntity` ويطبق `ITenantEntity`

---

# 2. Aggregate Roots

## 2.1 ملخص الـ Aggregates

| Aggregate Root | الأولوية | المبرر |
|----------------|----------|--------|
| **SalesOrder** | P0 | دورة حياة الطلب الكاملة — الجذر المركزي |
| **CashierShift** | P0 | ربط المدفوعات بالوردية والتسوية — لا مدفوعات بدون وردية مفتوحة |
| **Payment** | P0 | Split/Partial payments مستقلة — Invariant: لا دفع مزدوج |
| **CashRegister** | P1 | إدارة الخزينة والحركات النقدية الفعلية |
| **KitchenTicket** | P1 | KDS ومحطات التحضير — سياق فرعي |
| **RestaurantTable** | P1 | Dine-in وربط الطلبات بالطاولات |
| **FloorPlan** | P1 | تجميع مناطق الطعام والطاولات |
| **Invoice** | P2 | الفواتير الضريبية ZATCA — aggregate منفصل |
| **Reservation** | P2 | حجز الطاولات مسبقاً |
| **DeliveryOrder** | P2 | التوصيل وتتبع السائق |
| **Promotion** | P3 | العروض والخصومات التلقائية |
| **Coupon** | P3 | كوبونات الخصم |

## 2.2 SalesOrder (Aggregate Root)

**المسؤولية:** إدارة دورة حياة الطلب من الإنشاء حتى الإغلاق/الأرشفة.

**الكيانات المملوكة:**
- `OrderItem`
- `OrderLineModifier`
- `OrderDiscount`
- `OrderTax`
- `OrderStatusHistory`

**Invariants:**
- لا تعديل بعد `Completed`
- لا إلغاء بعد `Completed` (فقط Refund)
- المجموع = Σ(items) - discounts + taxes
- حجز مخزون مطلوب قبل `Confirmed`

## 2.3 CashierShift (Aggregate Root)

**المسؤولية:** وردية الكاشير — فتح، إغلاق، تسوية.

**الكيانات المملوكة:**
- `CashMovement`

**Invariants:**
- وردية واحدة مفتوحة لكل (Device + Cashier) في نفس الوقت
- لا مدفوعات بعد `Closing`
- Opening float مطلوب عند الفتح

## 2.4 Payment (Aggregate Root)

**المسؤولية:** معالجة المدفوعات والاسترداد.

**الكيانات المملوكة:**
- `PaymentAllocation`
- `Refund`

**Invariants:**
- لا دفع مزدوج لنفس الطلب
- Refund ≤ المدفوع
- Payment مرتبط بـ CashierShift مفتوحة

## 2.5 KitchenTicket (Aggregate Root)

**المسؤولية:** توجيه الطلبات لمحطات التحضير.

**الكيانات المملوكة:**
- `KitchenTicketItem`

**Invariants:**
- مرتبط بـ SalesOrder واحد
- لا تعديل بعد `Completed`

---

# 3. Entities

## 3.1 SalesOrder

```
SalesOrder : AuditableBaseEntity, ITenantEntity, ICompanyEntity, IBranchEntity
├── Id                          : Guid
├── TenantId                    : Guid
├── CompanyId                   : Guid
├── BranchId                    : Guid
├── OrderNumber                 : OrderNumber (VO)
├── SalesChannel                : SalesChannel (existing enum)
├── OrderType                   : OrderType (enum)
├── Status                      : OrderStatus (enum)
├── TableId                     : Guid? (DineIn)
├── CashierShiftId              : Guid?
├── CashierId                   : Guid (AppUser ref)
├── WaiterId                    : Guid? (AppUser ref)
├── CustomerId                  : Guid? (CRM future)
├── DeviceId                    : Guid
├── GuestCount                  : int?
├── Notes                       : string?
├── SubTotal                    : decimal
├── DiscountTotal               : decimal
├── TaxTotal                    : decimal
├── ServiceChargeTotal          : decimal
├── GrandTotal                  : decimal
├── Currency                    : string (ISO 4217)
├── PriceLevelId                : Guid? (snapshot ref)
├── ConfirmedAt                 : DateTimeOffset?
├── CompletedAt                 : DateTimeOffset?
├── CancelledAt                 : DateTimeOffset?
├── CancellationReason          : string?
├── SyncStatus                  : SyncStatus (enum)
├── LocalCreatedAt              : DateTimeOffset (offline)
├── Items                       : IReadOnlyCollection<OrderItem>
├── Discounts                   : IReadOnlyCollection<OrderDiscount>
├── Taxes                       : IReadOnlyCollection<OrderTax>
└── StatusHistory               : IReadOnlyCollection<OrderStatusHistory>
```

## 3.2 OrderItem

```
OrderItem : AuditableBaseEntity
├── Id                          : Guid
├── SalesOrderId                : Guid
├── LineNumber                  : int
├── ProductId                   : Guid (Menu ref — not navigated)
├── ComboMealId                 : Guid? (if combo)
├── ProductNameAr               : string (snapshot)
├── ProductNameEn               : string (snapshot)
├── Sku                         : string? (snapshot)
├── Quantity                    : Quantity (VO)
├── UnitPrice                   : decimal (snapshot)
├── LineDiscount                : decimal
├── LineTax                     : decimal
├── LineTotal                   : decimal
├── Currency                    : string
├── Notes                       : string? (kitchen notes)
├── KitchenStatus               : KitchenItemStatus (enum)
├── IsVoided                    : bool
├── VoidReason                  : string?
├── Modifiers                   : IReadOnlyCollection<OrderLineModifier>
```

## 3.3 OrderLineModifier

```
OrderLineModifier : AuditableBaseEntity
├── Id                          : Guid
├── OrderItemId                 : Guid
├── ModifierId                  : Guid (Menu ref — snapshot)
├── ModifierNameAr              : string (snapshot)
├── ModifierNameEn              : string (snapshot)
├── ExtraPrice                  : decimal (snapshot)
├── Quantity                    : int
```

## 3.4 OrderDiscount

```
OrderDiscount : AuditableBaseEntity
├── Id                          : Guid
├── SalesOrderId                : Guid
├── DiscountType                : DiscountType (enum)
├── Description                 : string
├── Amount                      : DiscountAmount (VO)
├── CouponId                    : Guid?
├── PromotionId                 : Guid?
├── AppliedBy                   : Guid (user ref)
```

## 3.5 OrderTax

```
OrderTax : AuditableBaseEntity
├── Id                          : Guid
├── SalesOrderId                : Guid
├── TaxNameAr                   : string
├── TaxNameEn                   : string
├── TaxRate                     : decimal (percentage)
├── TaxableAmount               : decimal
├── TaxAmount                   : TaxAmount (VO)
├── IsInclusive                 : bool
```

## 3.6 OrderStatusHistory

```
OrderStatusHistory : BaseEntity (append-only, no soft delete)
├── Id                          : Guid
├── SalesOrderId                : Guid
├── FromStatus                  : OrderStatus
├── ToStatus                    : OrderStatus
├── ChangedAt                   : DateTimeOffset
├── ChangedBy                   : Guid
├── Reason                      : string?
├── DeviceId                    : Guid
```

## 3.7 Payment

```
Payment : AuditableBaseEntity, ITenantEntity, IBranchEntity
├── Id                          : Guid
├── TenantId                    : Guid
├── BranchId                    : Guid
├── CashierShiftId              : Guid
├── ReceiptNumber               : ReceiptNumber (VO)
├── PaymentMethod               : PaymentMethod (enum)
├── Status                      : PaymentStatus (enum)
├── Amount                      : Money (VO)
├── TipAmount                   : Money? (VO)
├── ReferenceNumber             : PaymentReference? (VO)
├── GatewayTransactionId        : string? (card payments)
├── ProcessedBy                   : Guid (AppUser ref)
├── ProcessedAt                 : DateTimeOffset
├── Allocations                 : IReadOnlyCollection<PaymentAllocation>
└── Refunds                     : IReadOnlyCollection<Refund>
```

## 3.8 PaymentAllocation

```
PaymentAllocation : AuditableBaseEntity
├── Id                          : Guid
├── PaymentId                   : Guid
├── SalesOrderId                : Guid
├── AllocatedAmount             : Money (VO)
```

## 3.9 Refund

```
Refund : AuditableBaseEntity
├── Id                          : Guid
├── PaymentId                   : Guid
├── SalesOrderId                : Guid
├── RefundAmount                : Money (VO)
├── RefundMethod                : PaymentMethod (enum)
├── Status                      : RefundStatus (enum)
├── Reason                      : string
├── ApprovedBy                  : Guid? (manager)
├── ProcessedAt                 : DateTimeOffset?
```

## 3.10 CashierShift

```
CashierShift : AuditableBaseEntity, ITenantEntity, IBranchEntity
├── Id                          : Guid
├── TenantId                    : Guid
├── BranchId                    : Guid
├── CashRegisterId              : Guid
├── DeviceId                    : Guid
├── CashierId                   : Guid (AppUser ref)
├── ShiftNumber                 : string (sequential per branch)
├── Status                      : ShiftStatus (enum)
├── OpeningFloat                : Money (VO)
├── ExpectedCash                : Money? (VO, calculated at close)
├── ActualCash                  : Money? (VO, counted at close)
├── Variance                    : Money? (VO)
├── OpenedAt                    : DateTimeOffset
├── ClosedAt                    : DateTimeOffset?
├── ReconciledAt                : DateTimeOffset?
├── ReconciledBy                : Guid?
├── Notes                       : string?
└── CashMovements               : IReadOnlyCollection<CashMovement>
```

## 3.11 CashRegister

```
CashRegister : AuditableBaseEntity, ITenantEntity, IBranchEntity
├── Id                          : Guid
├── TenantId                    : Guid
├── BranchId                    : Guid
├── NameAr                      : string
├── NameEn                      : string
├── Code                        : string (unique per branch)
├── IsActive                    : bool
├── CurrentBalance              : Money (VO)
└── DefaultOpeningFloat         : Money (VO)
```

## 3.12 CashMovement

```
CashMovement : BaseEntity (append-only)
├── Id                          : Guid
├── CashierShiftId              : Guid
├── MovementType                : CashMovementType (enum)
├── Amount                      : Money (VO)
├── Reason                      : string
├── ReferenceDocument           : string?
├── CreatedAt                   : DateTimeOffset
├── CreatedBy                   : Guid
```

## 3.13 FloorPlan

```
FloorPlan : AuditableBaseEntity, ITenantEntity, IBranchEntity
├── Id                          : Guid
├── TenantId                    : Guid
├── BranchId                    : Guid
├── NameAr                      : string
├── NameEn                      : string
├── IsActive                    : bool
├── SortOrder                   : int
└── DiningAreas                 : IReadOnlyCollection<DiningArea>
```

## 3.14 DiningArea

```
DiningArea : AuditableBaseEntity
├── Id                          : Guid
├── FloorPlanId                 : Guid
├── NameAr                      : string
├── NameEn                      : string
├── SortOrder                   : int
├── Capacity                    : int
└── Tables                      : IReadOnlyCollection<RestaurantTable>
```

## 3.15 RestaurantTable

```
RestaurantTable : AuditableBaseEntity
├── Id                          : Guid
├── DiningAreaId                : Guid
├── TableNumber                 : string
├── NameAr                      : string?
├── NameEn                      : string?
├── Capacity                    : int
├── Status                      : TableStatus (enum)
├── CurrentOrderId              : Guid?
├── PositionX                   : int? (floor plan layout)
├── PositionY                   : int?
├── Shape                       : TableShape (enum)
```

## 3.16 KitchenTicket

```
KitchenTicket : AuditableBaseEntity, ITenantEntity, IBranchEntity
├── Id                          : Guid
├── TenantId                    : Guid
├── BranchId                    : Guid
├── SalesOrderId                : Guid
├── TicketNumber                : string
├── KitchenStationId            : Guid
├── Status                      : KitchenTicketStatus (enum)
├── Priority                    : int
├── CreatedAt                   : DateTimeOffset
├── StartedAt                   : DateTimeOffset?
├── CompletedAt                 : DateTimeOffset?
├── EstimatedPrepMinutes        : int?
├── Items                       : IReadOnlyCollection<KitchenTicketItem>
```

## 3.17 KitchenTicketItem

```
KitchenTicketItem : AuditableBaseEntity
├── Id                          : Guid
├── KitchenTicketId             : Guid
├── OrderItemId                 : Guid
├── ProductNameAr               : string (snapshot)
├── ProductNameEn               : string (snapshot)
├── Quantity                    : int
├── ModifiersSummary            : string? (formatted snapshot)
├── Status                      : KitchenItemStatus (enum)
├── StartedAt                   : DateTimeOffset?
├── CompletedAt                 : DateTimeOffset?
```

## 3.18 KitchenStation

```
KitchenStation : AuditableBaseEntity, ITenantEntity, IBranchEntity
├── Id                          : Guid
├── TenantId                    : Guid
├── BranchId                    : Guid
├── NameAr                      : string
├── NameEn                      : string
├── StationType                 : KitchenStationType (enum)
├── DeviceId                    : Guid? (KDS device)
├── IsActive                    : bool
├── SortOrder                   : int
```

## 3.19 Invoice

```
Invoice : AuditableBaseEntity, ITenantEntity, ICompanyEntity, IBranchEntity
├── Id                          : Guid
├── TenantId                    : Guid
├── CompanyId                   : Guid
├── BranchId                    : Guid
├── SalesOrderId                : Guid
├── InvoiceNumber               : InvoiceNumber (VO)
├── Status                      : InvoiceStatus (enum)
├── IssueDate                   : DateTime
├── DueDate                     : DateTime?
├── SubTotal                    : Money (VO)
├── TaxTotal                    : Money (VO)
├── GrandTotal                  : Money (VO)
├── BuyerName                   : string?
├── BuyerVatNumber              : string?
├── ZatcaUuid                   : string? (ZATCA integration)
├── ZatcaHash                   : string?
├── ZatcaQrCode                 : string?
├── Lines                       : IReadOnlyCollection<InvoiceLine>
```

## 3.20 InvoiceLine

```
InvoiceLine : AuditableBaseEntity
├── Id                          : Guid
├── InvoiceId                   : Guid
├── LineNumber                  : int
├── DescriptionAr               : string
├── DescriptionEn               : string
├── Quantity                    : decimal
├── UnitPrice                   : Money (VO)
├── TaxRate                     : decimal
├── TaxAmount                   : TaxAmount (VO)
├── LineTotal                   : Money (VO)
```

## 3.21 Reservation

```
Reservation : AuditableBaseEntity, ITenantEntity, IBranchEntity
├── Id                          : Guid
├── TenantId                    : Guid
├── BranchId                    : Guid
├── TableId                     : Guid?
├── CustomerName                : string
├── CustomerPhone               : PhoneNumber (VO)
├── GuestCount                  : int
├── ReservationDate             : DateTime
├── DurationMinutes             : int
├── Status                      : ReservationStatus (enum)
├── Notes                       : string?
├── ConfirmedAt                 : DateTimeOffset?
├── SalesOrderId                : Guid? (when seated)
```

## 3.22 DeliveryOrder

```
DeliveryOrder : AuditableBaseEntity, ITenantEntity, IBranchEntity
├── Id                          : Guid
├── TenantId                    : Guid
├── BranchId                    : Guid
├── SalesOrderId                : Guid
├── DeliveryAddress             : Address (VO)
├── DeliveryLocation            : GeoLocation? (VO)
├── CustomerPhone               : PhoneNumber (VO)
├── CustomerName                : string
├── DeliveryFee                 : Money (VO)
├── EstimatedDeliveryMinutes    : int?
├── Status                      : DeliveryStatus (enum)
├── DriverId                    : Guid? (Employee ref)
├── AssignedAt                  : DateTimeOffset?
├── PickedUpAt                  : DateTimeOffset?
├── DeliveredAt                 : DateTimeOffset?
└── DriverAssignments           : IReadOnlyCollection<DriverAssignment>
```

## 3.23 DriverAssignment

```
DriverAssignment : AuditableBaseEntity
├── Id                          : Guid
├── DeliveryOrderId             : Guid
├── DriverId                    : Guid (Employee ref)
├── AssignedAt                  : DateTimeOffset
├── Status                      : DriverAssignmentStatus (enum)
```

## 3.24 Promotion

```
Promotion : AuditableBaseEntity, ITenantEntity
├── Id                          : Guid
├── TenantId                    : Guid
├── NameAr                      : string
├── NameEn                      : string
├── PromotionType               : PromotionType (enum)
├── DiscountValue               : decimal
├── DiscountType                : DiscountType (enum)
├── StartDate                   : DateTime
├── EndDate                     : DateTime?
├── IsActive                    : bool
├── ApplicableChannels          : SalesChannel (flags)
├── MinimumOrderAmount          : decimal?
├── MaximumDiscountAmount       : decimal?
```

## 3.25 Coupon

```
Coupon : AuditableBaseEntity, ITenantEntity
├── Id                          : Guid
├── TenantId                    : Guid
├── Code                        : string (unique per tenant)
├── PromotionId                 : Guid?
├── DiscountType                : DiscountType (enum)
├── DiscountValue               : decimal
├── MaxUses                     : int?
├── UsedCount                   : int
├── ValidFrom                   : DateTime
├── ValidTo                     : DateTime?
├── IsActive                    : bool
```

## 3.26 PaymentStatusHistory

```
PaymentStatusHistory : BaseEntity (append-only)
├── Id                          : Guid
├── PaymentId                   : Guid
├── FromStatus                  : PaymentStatus
├── ToStatus                    : PaymentStatus
├── ChangedAt                   : DateTimeOffset
├── ChangedBy                   : Guid
├── Reason                      : string?
```

---

# 4. Value Objects

| Value Object | جديد/موجود | المالك | Immutable | الوصف |
|--------------|------------|--------|-----------|-------|
| `Money` | موجود | Payment, CashierShift, Invoice | نعم | Amount + ISO Currency |
| `Address` | موجود | DeliveryOrder | نعم | عنوان التوصيل ثنائي اللغة |
| `PhoneNumber` | موجود | DeliveryOrder, Reservation | نعم | E.164 |
| `GeoLocation` | موجود | DeliveryOrder | نعم | إحداثيات التوصيل |
| `TaxAmount` | **جديد** | OrderTax, InvoiceLine | نعم | مبلغ الضريبة + العملة |
| `DiscountAmount` | **جديد** | OrderDiscount | نعم | مبلغ الخصم + العملة + النوع (نسبة/ثابت) |
| `Quantity` | **جديد** | OrderItem | نعم | كمية موجبة مع وحدة اختيارية |
| `OrderNumber` | **جديد** | SalesOrder | نعم | رقم تسلسلي لكل فرع (مثال: BR-2026-00001) |
| `ReceiptNumber` | **جديد** | Payment | نعم | رقم إيصال لكل فرع |
| `InvoiceNumber` | **جديد** | Invoice | نعم | رقم فاتورة ZATCA-compliant |
| `PaymentReference` | **جديد** | Payment | نعم | مرجع بوابة الدفع أو التحويل |

### 4.1 OrderNumber

```
OrderNumber
├── Prefix    : string (branch code)
├── Sequence  : long
├── Formatted : string => "{Prefix}-{Year}-{Sequence:D5}"
```

### 4.2 TaxAmount

```
TaxAmount
├── Amount    : decimal (≥ 0)
├── Currency  : string (ISO 4217, 3 chars)
├── Rate      : decimal (percentage)
```

### 4.3 DiscountAmount

```
DiscountAmount
├── Amount       : decimal (≥ 0)
├── Currency     : string
├── IsPercentage : bool
├── Percentage   : decimal? (if IsPercentage)
```

---

# 5. Enumerations

## 5.1 موجودة — لا تكرار

| Enum | الملف | ملاحظة |
|------|-------|--------|
| `SalesChannel` | MenuEnums.cs | DineIn, TakeAway, Delivery, Kiosk, Online |
| `ReservationStatus` | InventoryEnums.cs | لحجز المخزون فقط — اسم مختلف |
| `DeviceType` | OrganizationEnums.cs | POSTerminal, KitchenDisplay, etc. |

## 5.2 جديدة — SalesEnums.cs

```csharp
public enum OrderStatus
{
    Draft = 1,
    Pending = 2,
    Confirmed = 3,
    Preparing = 4,
    Ready = 5,
    Served = 6,
    Completed = 7,
    Cancelled = 8,
    Archived = 9
}

public enum OrderType
{
    DineIn = 1,
    TakeAway = 2,
    Delivery = 3,
    DriveThru = 4,
    QROrdering = 5,
    Kiosk = 6
}

public enum PaymentStatus
{
    Pending = 1,
    Authorized = 2,
    Captured = 3,
    Completed = 4,
    Refunded = 5,
    PartiallyRefunded = 6,
    Cancelled = 7,
    Failed = 8
}

public enum PaymentMethod
{
    Cash = 1,
    CreditCard = 2,
    DebitCard = 3,
    MobileWallet = 4,
    BankTransfer = 5,
    GiftCard = 6,
    LoyaltyPoints = 7,
    Other = 99
}

public enum ShiftStatus
{
    Open = 1,
    Active = 2,
    Closing = 3,
    Closed = 4,
    Reconciled = 5
}

public enum KitchenTicketStatus
{
    Pending = 1,
    InProgress = 2,
    Ready = 3,
    Completed = 4,
    Cancelled = 5
}

public enum KitchenItemStatus
{
    Pending = 1,
    Preparing = 2,
    Ready = 3,
    Served = 4,
    Voided = 5
}

public enum KitchenStationType
{
    Hot = 1,
    Cold = 2,
    Grill = 3,
    Fry = 4,
    Bar = 5,
    Dessert = 6,
    Expo = 7,
    General = 99
}

public enum InvoiceStatus
{
    Draft = 1,
    Issued = 2,
    Cancelled = 3,
    CreditNote = 4
}

public enum RefundStatus
{
    Pending = 1,
    Approved = 2,
    Processed = 3,
    Rejected = 4
}

public enum ReservationStatus
{
  Pending = 1,
  Confirmed = 2,
  Seated = 3,
  Completed = 4,
  Cancelled = 5,
  NoShow = 6
}

public enum DeliveryStatus
{
    Pending = 1,
    Assigned = 2,
    PickedUp = 3,
    InTransit = 4,
    Delivered = 5,
    Failed = 6,
    Cancelled = 7
}

public enum DriverAssignmentStatus
{
    Assigned = 1,
    Accepted = 2,
    Rejected = 3,
    Completed = 4
}

public enum TableStatus
{
    Available = 1,
    Occupied = 2,
    Reserved = 3,
    Cleaning = 4,
    OutOfService = 5
}

public enum TableShape
{
    Square = 1,
    Round = 2,
    Rectangle = 3,
    Bar = 4
}

public enum DiscountType
{
    Percentage = 1,
    FixedAmount = 2,
    BuyXGetY = 3,
    Combo = 4
}

public enum PromotionType
{
    OrderDiscount = 1,
    ItemDiscount = 2,
    FreeItem = 3,
    BundleDeal = 4
}

public enum CashMovementType
{
    Sale = 1,
    Refund = 2,
    FloatIn = 3,
    FloatOut = 4,
    PettyCash = 5,
    Variance = 6,
    Tip = 7
}

public enum SyncStatus
{
    Local = 1,
    PendingSync = 2,
    Synced = 3,
    Conflict = 4
}
```

---

# 6. Domain Events

## 6.1 Sales Events (Events/Sales/SalesEvents.cs)

| Event | المُطلِق | Payload الرئيسي |
|-------|---------|-----------------|
| `OrderCreatedEvent` | SalesOrder | OrderId, BranchId, TenantId, OrderType, SalesChannel |
| `OrderSubmittedEvent` | SalesOrder | OrderId, ItemCount, GrandTotal |
| `OrderConfirmedEvent` | SalesOrder | OrderId, BranchId, Items[] (for inventory) |
| `OrderStatusChangedEvent` | SalesOrder | OrderId, FromStatus, ToStatus, ChangedBy |
| `OrderCancelledEvent` | SalesOrder | OrderId, Reason, CancelledBy |
| `OrderCompletedEvent` | SalesOrder | OrderId, GrandTotal, Currency, CompletedAt |
| `OrderReopenedEvent` | SalesOrder | OrderId, ReopenedBy, Reason |
| `OrderItemVoidedEvent` | SalesOrder | OrderId, OrderItemId, Reason |

## 6.2 Payment Events

| Event | المُطلِق | Payload الرئيسي |
|-------|---------|-----------------|
| `PaymentInitiatedEvent` | Payment | PaymentId, OrderId, Amount, Method |
| `PaymentCompletedEvent` | Payment | PaymentId, OrderId, Amount, ShiftId |
| `PaymentFailedEvent` | Payment | PaymentId, Reason |
| `PaymentRefundedEvent` | Payment | PaymentId, RefundId, Amount |
| `RefundApprovedEvent` | Refund | RefundId, PaymentId, ApprovedBy |

## 6.3 Shift Events

| Event | المُطلِق | Payload الرئيسي |
|-------|---------|-----------------|
| `ShiftOpenedEvent` | CashierShift | ShiftId, CashierId, DeviceId, OpeningFloat |
| `ShiftClosingEvent` | CashierShift | ShiftId, ExpectedCash |
| `ShiftClosedEvent` | CashierShift | ShiftId, ActualCash, Variance |
| `ShiftReconciledEvent` | CashierShift | ShiftId, ReconciledBy |
| `CashMovementRecordedEvent` | CashierShift | ShiftId, Type, Amount |

## 6.4 Kitchen Events

| Event | المُطلِق | Payload الرئيسي |
|-------|---------|-----------------|
| `KitchenTicketCreatedEvent` | KitchenTicket | TicketId, OrderId, StationId |
| `KitchenTicketStartedEvent` | KitchenTicket | TicketId, StartedAt |
| `KitchenTicketCompletedEvent` | KitchenTicket | TicketId, CompletedAt, PrepMinutes |
| `KitchenItemReadyEvent` | KitchenTicketItem | TicketId, ItemId |

## 6.5 Invoice Events

| Event | المُطلِق | Payload الرئيسي |
|-------|---------|-----------------|
| `InvoiceIssuedEvent` | Invoice | InvoiceId, OrderId, InvoiceNumber, GrandTotal |
| `InvoiceCancelledEvent` | Invoice | InvoiceId, Reason |

## 6.6 Integration Events (Cross-Context)

| Event | المستمع | الغرض |
|-------|---------|-------|
| `OrderConfirmedEvent` | Inventory | إنشاء InventoryReservation |
| `OrderCompletedEvent` | Inventory | Fulfill reservation + SalesConsumption |
| `OrderCancelledEvent` | Inventory | إلغاء الحجوزات |
| `OrderCompletedEvent` | Finance (future) | Journal Entry |
| `InvoiceIssuedEvent` | Finance/ZATCA (future) | إرسال للهيئة |

---

# 7. Business Rules

## 7.1 قواعد الطلب (SalesOrder)

| ID | القاعدة | التحقق | رسالة الخطأ |
|----|---------|--------|-------------|
| BR-S01 | لا تعديل طلب بعد Completed | `Status < Completed` | `ErrorCodes.OrderAlreadyClosed` |
| BR-S02 | لا إلغاء بعد Completed | `Status != Completed` | `ErrorCodes.OrderCannotBeCancelled` |
| BR-S03 | طلب ملغى لا يعود Pending | transition matrix | `ErrorCodes.InvalidStatusTransition` |
| BR-S04 | حجز مخزون قبل Confirm | inventory check via ACL | `ErrorCodes.InsufficientStock` |
| BR-S05 | DineIn يتطلب TableId | `OrderType != DineIn \|\| TableId != null` | `ErrorCodes.TableRequired` |
| BR-S06 | Delivery يتطلب DeliveryOrder | `OrderType != Delivery \|\| has delivery` | `ErrorCodes.DeliveryAddressRequired` |
| BR-S07 | GrandTotal = SubTotal - Discounts + Taxes + ServiceCharge | calculation invariant | internal |
| BR-S08 | Offline sales تحتاج Branch.AllowOfflineSales | branch check | `ErrorCodes.OfflineSalesNotAllowed` |
| BR-S09 | Reopen يتطلب صلاحية Manager | permission check | `ErrorCodes.Unauthorized` |
| BR-S10 | Reopen خلال نافذة زمنية (24h default) | time check | `ErrorCodes.ReopenWindowExpired` |
| BR-S11 | السعر snapshot وقت الإضافة | immutable after add | `ErrorCodes.ItemPriceLocked` |
| BR-S12 | Void item يتطلب سبب | `VoidReason != null` | `ErrorCodes.VoidReasonRequired` |

## 7.2 قواعد المدفوعات (Payment)

| ID | القاعدة | التحقق | رسالة الخطأ |
|----|---------|--------|-------------|
| BR-P01 | لا دفع مزدوج | `order.PaidAmount < order.GrandTotal` | `ErrorCodes.OrderAlreadyPaid` |
| BR-P02 | Refund ≤ المدفوع | `refundAmount <= paidAmount` | `ErrorCodes.RefundExceedsPaid` |
| BR-P03 | وردية مغلقة لا تقبل مدفوعات | `shift.Status < Closing` | `ErrorCodes.ShiftClosed` |
| BR-P04 | Split payments مسموحة | multiple allocations | — |
| BR-P05 | Partial payments مسموحة | `paidAmount < grandTotal` → Pending | — |
| BR-P06 | Refund يتطلب صلاحية | `payments.refund` permission | `ErrorCodes.Unauthorized` |
| BR-P07 | Refund كبير يتطلب Manager approval | amount > threshold | `ErrorCodes.ManagerApprovalRequired` |
| BR-P08 | Tip ≥ 0 | `tipAmount >= 0` | `ErrorCodes.InvalidAmount` |

## 7.3 قواعد الوردية (CashierShift)

| ID | القاعدة | التحقق | رسالة الخطأ |
|----|---------|--------|-------------|
| BR-H01 | وردية واحدة مفتوحة per Device+Cashier | uniqueness check | `ErrorCodes.ShiftAlreadyOpen` |
| BR-H02 | Opening float ≥ 0 | `openingFloat >= 0` | `ErrorCodes.InvalidAmount` |
| BR-H03 | لا طلبات جديدة بعد Closing | `status < Closing` | `ErrorCodes.ShiftClosing` |
| BR-H04 | Variance يُسجّل في CashMovement | auto on close | — |
| BR-H05 | Variance كبير يتطلب Manager | `abs(variance) > threshold` | `ErrorCodes.ManagerApprovalRequired` |
| BR-H06 | Reconcile يتطلب Closed status | `status == Closed` | `ErrorCodes.InvalidStatusTransition` |

## 7.4 قواعد المطبخ (Kitchen)

| ID | القاعدة | التحقق |
|----|---------|--------|
| BR-K01 | Ticket مرتبط بطلب واحد | `SalesOrderId` required |
| BR-K02 | لا تعديل بعد Completed | status check |
| BR-K03 | Prep time يُسجل للـ KPI | `CompletedAt - StartedAt` |
| BR-K04 | التوجيه حسب Recipe station config | routing service |

## 7.5 قواعد الطاولات (RestaurantTable)

| ID | القاعدة | التحقق |
|----|---------|--------|
| BR-T01 | طاولة Occupied لها طلب واحد نشط | `CurrentOrderId` unique |
| BR-T02 | حجز يتطلب طاولة Available | `table.Status == Available` |
| BR-T03 | Release table عند Complete/Cancel | auto status update |

## 7.6 قواعد المخزون (Integration)

| ID | القاعدة | التحقق |
|----|---------|--------|
| BR-I01 | Reserve عند Confirm | `InventoryReservation` per recipe item |
| BR-I02 | Consume عند Complete | `TransactionType.SalesConsumption` |
| BR-I03 | Release عند Cancel | cancel reservations |
| BR-I04 | Negative stock = Branch.AllowNegativeStock ∩ InventorySetting | branch + setting check |
| BR-I05 | SourceDocument = OrderNumber | traceability |

---

# 8. Relationships

## 8.1 مخطط العلاقات

```
Tenant (Org)
  └── Company (Org)
        └── Branch (Org)
              ├── Device (Org) ────────────── CashierShift
              ├── CashRegister ────────────── CashierShift
              ├── FloorPlan
              │     └── DiningArea
              │           └── RestaurantTable ──?── SalesOrder
              ├── KitchenStation ──────────── KitchenTicket
              │
              ├── SalesOrder (1) ──── (*) OrderItem
              │     │                      └── (*) OrderLineModifier
              │     ├── (*) OrderDiscount
              │     ├── (*) OrderTax
              │     ├── (*) OrderStatusHistory
              │     ├── (0..1) DeliveryOrder
              │     ├── (0..*) KitchenTicket
              │     └── (0..1) Invoice
              │
              ├── Payment (1) ──── (*) PaymentAllocation ──── SalesOrder
              │     └── (*) Refund
              │
              └── CashierShift (1) ──── (*) CashMovement
                    └── (*) Payment

Promotion (Tenant) ──?── Coupon
Reservation ──?── RestaurantTable
Reservation ──?── SalesOrder (when seated)
```

## 8.2 جدول العلاقات

| من | إلى | النوع | الملكية | ملاحظة |
|----|-----|-------|---------|--------|
| SalesOrder | OrderItem | 1:N | Order يملك | Cascade delete (soft) |
| SalesOrder | OrderDiscount | 1:N | Order يملك | |
| SalesOrder | OrderTax | 1:N | Order يملك | |
| SalesOrder | PaymentAllocation | 1:N | Payment يملك | عبر Payment |
| SalesOrder | KitchenTicket | 1:N | Ticket يملك | Reference by ID |
| SalesOrder | Invoice | 1:1 | Invoice يملك | |
| SalesOrder | DeliveryOrder | 1:1 | Delivery يملك | |
| SalesOrder | RestaurantTable | N:1 | Reference | لا navigation |
| Payment | CashierShift | N:1 | Reference | |
| CashierShift | CashRegister | N:1 | Reference | |
| CashierShift | Device | N:1 | Reference | |
| FloorPlan | DiningArea | 1:N | FloorPlan يملك | |
| DiningArea | RestaurantTable | 1:N | DiningArea يملك | |
| Coupon | Promotion | N:1 | Reference | |

## 8.3 Aggregate Boundaries

- **داخل SalesOrder:** OrderItem, OrderLineModifier, OrderDiscount, OrderTax, OrderStatusHistory
- **خارج SalesOrder (references):** Payment, KitchenTicket, Invoice, DeliveryOrder, RestaurantTable
- **داخل Payment:** PaymentAllocation, Refund, PaymentStatusHistory
- **داخل CashierShift:** CashMovement
- **داخل KitchenTicket:** KitchenTicketItem
- **داخل FloorPlan:** DiningArea → RestaurantTable
- **داخل Invoice:** InvoiceLine

---

# 9. Integration Diagram

## 9.1 تدفق إنشاء طلب

```
[POS Device]
    │
    ▼
CreateOrderCommand
    │
    ├──► IMenuPricingService.GetPrice(productId, priceLevelId, channel)
    │         └── Menu Context (ACL) → snapshot في OrderItem
    │
    ├──► SalesOrder.Create() → OrderCreatedEvent
    │
    └──► Response: OrderDto

[Submit Order]
    │
    ▼
SubmitOrderCommand
    │
    ├──► SalesOrder.Submit() → OrderSubmittedEvent
    │
    └──► KitchenTicketService.CreateTickets(order) → KitchenTicketCreatedEvent
```

## 9.2 تدفق التأكيد والمخزون

```
ConfirmOrderCommand
    │
    ├──► IInventoryReservationService.ReserveForOrder(order)
    │         └── Inventory Context → InventoryReservation (SourceDocument = OrderNumber)
    │
    ├──► SalesOrder.Confirm() → OrderConfirmedEvent
    │
    └──► Kitchen: tickets → InProgress
```

## 9.3 تدفق الإكمال والخصم

```
CompleteOrderCommand
    │
    ├──► SalesOrder.Complete() → OrderCompletedEvent
    │
    ├──► IInventoryConsumptionService.ConsumeForOrder(order)
    │         └── InventoryReservation.MarkAsFulfilled()
    │         └── InventoryTransaction (SalesConsumption)
    │
    ├──► RestaurantTable.Release() (if DineIn)
    │
    └──► [Future] Finance: JournalEntry
```

## 9.4 تدفق الدفع

```
ProcessPaymentCommand
    │
    ├──► Validate: CashierShift.IsOpen
    ├──► Validate: Order.NotFullyPaid
    │
    ├──► Payment.Create() → PaymentCompletedEvent
    ├──► PaymentAllocation to SalesOrder
    │
    ├──► If fully paid → SalesOrder.MarkAsPaid()
    │
    └──► CashMovement in Shift
```

## 9.5 ACL Services (Application Layer)

| Interface | الغرض |
|-----------|-------|
| `IMenuPricingService` | جلب السعر والمنتج للـ snapshot |
| `IMenuAvailabilityService` | التحقق من توفر المنتج |
| `IInventoryReservationService` | حجز المخزون للطلب |
| `IInventoryConsumptionService` | خصم المخزون عند الإكمال |
| `ITableAvailabilityService` | توفر الطاولات |
| `IOrderNumberGenerator` | توليد OrderNumber تسلسلي |
| `IReceiptNumberGenerator` | توليد ReceiptNumber |
| `IInvoiceNumberGenerator` | توليد InvoiceNumber ZATCA |

---

# 10. Lifecycle Diagrams

## 10.1 Order Lifecycle

```
                    ┌─────────┐
                    │  Draft  │ ← إنشاء طلب جديد
                    └────┬────┘
                         │ submit()
                    ┌────▼────┐
                    │ Pending │ ← في انتظار التأكيد
                    └────┬────┘
                         │ confirm() + inventory reserve
                    ┌────▼─────┐
         ┌──────────│ Confirmed│
         │          └────┬─────┘
         │               │ send to kitchen
         │          ┌────▼─────┐
         │          │ Preparing│
         │          └────┬─────┘
         │               │ items ready
         │          ┌────▼────┐
         │          │  Ready  │
         │          └────┬────┘
         │               │ serve()
         │          ┌────▼────┐
         │          │ Served  │
         │          └────┬────┘
         │               │ complete() + payment
         │          ┌────▼─────┐
         │          │ Completed│ ← نهائي (يمكن Reopen/Refund)
         │          └────┬─────┘
         │               │ archive()
         │          ┌────▼────┐
         │          │ Archived│
         │          └─────────┘
         │
         │ cancel() من أي حالة قبل Completed
         │
    ┌────▼─────┐
    │ Cancelled│
    └──────────┘

Completed → reopen() [Manager, within 24h] → Served
Completed → refund() [partial/full] → Refund flow
```

### Transition Matrix

| From \ To | Pending | Confirmed | Preparing | Ready | Served | Completed | Cancelled |
|-----------|---------|-----------|-----------|-------|--------|-----------|-----------|
| Draft | ✓ | | | | | | ✓ |
| Pending | | ✓ | | | | | ✓ |
| Confirmed | | | ✓ | | | | ✓ |
| Preparing | | | | ✓ | | | ✓ |
| Ready | | | | | ✓ | | ✓ |
| Served | | | | | | ✓ | |
| Completed | | | | | ✓(reopen) | | |

## 10.2 Shift Lifecycle

```
┌──────┐
│ Open │ ← openShift(cashier, device, openingFloat)
└──┬───┘
   │ first transaction
┌──▼───┐
│Active│ ← تقبل طلبات ومدفوعات
└──┬───┘
   │ initiateClose()
┌──▼────┐
│Closing│ ← لا طلبات جديدة، إنهاء المعلقة
└──┬────┘
   │ close(actualCash)
┌──▼────┐
│ Closed│ ← variance recorded
└──┬────┘
   │ reconcile(manager)
┌──▼───────┐
│Reconciled│ ← نهائي
└──────────┘
```

### Reconciliation Rules

```
ExpectedCash = OpeningFloat
             + Σ(cash payments)
             - Σ(cash refunds)
             + Σ(cash movements in)
             - Σ(cash movements out)

Variance = ActualCash - ExpectedCash

If |Variance| > Branch.VarianceThreshold → Manager approval required
```

## 10.3 Payment Lifecycle

```
┌─────────┐
│ Pending │ ← initiate payment
└────┬────┘
     │
     ├── [Card] ──► Authorized ──► Captured ──► Completed
     │
     └── [Cash] ──────────────────────────────► Completed
                                                    │
                                    ┌───────────────┤
                                    │               │
                               Refunded      PartiallyRefunded
                                    │
                               Cancelled (before capture)
                               Failed (gateway error)
```

---

# 11. Risks & Mitigations

| # | الخطر | التأثير | الاحتمال | التخفيف |
|---|-------|---------|----------|---------|
| R1 | تضارب أسماء Session/Shift | ارتباك | منخفض | اعتماد `CashierShift` صراحةً |
| R2 | Money vs decimal inconsistency | أخطاء حسابية | متوسط | Money للمدفوعات؛ decimal في snapshots مع Currency |
| R3 | Offline sync conflicts | فقد بيانات | عالي | Event versioning + `SyncStatus.Conflict` + manual resolution UI |
| R4 | Inventory race conditions | بيع بدون مخزون | متوسط | Reservation قبل Confirm + optimistic concurrency (RowVersion) |
| R5 | ZATCA compliance | رفض فواتير | متوسط | Invoice aggregate منفصل + integration point جاهز |
| R6 | Performance مع طلبات كثيرة | بطء POS | متوسط | Indexes على OrderNumber, BranchId+Status, DeviceId+Shift |
| R7 | Split payment complexity | أخطاء تسوية | متوسط | PaymentAllocation aggregate + invariant checks |
| R8 | Kitchen routing errors | طلبات ضائعة | منخفض | KitchenStation config + fallback to General station |

---

# 12. Future Extension Points

| النقطة | الوصف | الجاهزية |
|--------|-------|----------|
| **Finance Integration** | `OrderCompletedEvent` → Journal Entry | Event defined |
| **ZATCA E-Invoicing** | `Invoice` aggregate + `ZatcaUuid/Hash/QrCode` | Fields ready |
| **CRM / Customer** | `CustomerId` على SalesOrder | Field nullable |
| **Loyalty Points** | `PaymentMethod.LoyaltyPoints` | Enum ready |
| **Multi-Currency** | `Money` VO + `Currency` system entity | VO ready |
| **QR Ordering** | `OrderType.QROrdering` | Enum ready |
| **Drive-Thru** | `OrderType.DriveThru` | Enum ready |
| **Tips Pooling** | `TipAmount` على Payment | Field ready |
| **Gift Cards** | `PaymentMethod.GiftCard` | Enum ready |
| **Kitchen Display API** | `KitchenTicket` events | Events defined |
| **Reporting** | Status history + prep time fields | Audit trail ready |
| **Webhook Notifications** | Domain events → Integration events | Pattern established |

---

# 13. Permissions (مقترحة)

```csharp
public static class Sales
{
    public const string View = "Sales.View";
    public const string Create = "Sales.Create";
    public const string Update = "Sales.Update";
    public const string Cancel = "Sales.Cancel";
    public const string Complete = "Sales.Complete";
    public const string Reopen = "Sales.Reopen";
    public const string VoidItem = "Sales.VoidItem";
}

public static class Payment
{
    public const string Process = "Payment.Process";
    public const string Refund = "Payment.Refund";
    public const string RefundApprove = "Payment.RefundApprove";
}

public static class Shift
{
    public const string Open = "Shift.Open";
    public const string Close = "Shift.Close";
    public const string Reconcile = "Shift.Reconcile";
}

public static class Kitchen
{
    public const string View = "Kitchen.View";
    public const string Manage = "Kitchen.Manage";
}

public static class Table
{
    public const string View = "Table.View";
    public const string Manage = "Table.Manage";
}

public static class Invoice
{
    public const string View = "Invoice.View";
    public const string Issue = "Invoice.Issue";
    public const string Cancel = "Invoice.Cancel";
}
```

---

# 14. Final Validation

| الفحص | الحالة |
|-------|--------|
| لا تكرار كيانات Menu/Inventory/Organization | ✅ |
| متوافق مع `00-DOMAIN_MODEL_AND_CONTEXTS.md` | ✅ |
| متوافق مع `03-BUSINESS_RULES.md` | ✅ |
| `AuditableBaseEntity` + multi-tenancy | ✅ |
| Offline-first عبر Device + SyncStatus | ✅ |
| ZATCA-ready (Invoice aggregate) | ✅ |
| Bilingual (ADR-000) | ✅ |
| DDD compliant (aggregate boundaries) | ✅ |
| Domain events للتكامل | ✅ |
| ACL pattern للسياقات الخارجية | ✅ |

---

# 15. هيكل الملفات المقترح (للتنفيذ)

```
GastroErp.Domain/
├── Entities/Sales/
│   ├── SalesOrder.cs
│   ├── OrderItem.cs
│   ├── OrderLineModifier.cs
│   ├── OrderDiscount.cs
│   ├── OrderTax.cs
│   ├── OrderStatusHistory.cs
│   ├── Payment.cs
│   ├── PaymentAllocation.cs
│   ├── Refund.cs
│   ├── PaymentStatusHistory.cs
│   ├── CashierShift.cs
│   ├── CashRegister.cs
│   ├── CashMovement.cs
│   ├── FloorPlan.cs
│   ├── DiningArea.cs
│   ├── RestaurantTable.cs
│   ├── KitchenTicket.cs
│   ├── KitchenTicketItem.cs
│   ├── KitchenStation.cs
│   ├── Invoice.cs
│   ├── InvoiceLine.cs
│   ├── Reservation.cs
│   ├── DeliveryOrder.cs
│   ├── DriverAssignment.cs
│   ├── Promotion.cs
│   └── Coupon.cs
├── Enums/SalesEnums.cs
├── Events/Sales/SalesEvents.cs
└── ValueObjects/SalesValueObjects.cs
```

---

نهاية وثيقة التصميم — Phase 11B.0
