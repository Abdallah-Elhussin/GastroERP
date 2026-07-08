# GastroERP Backend Roadmap
# Phase 11B — Milestone 1
# POS Core Implementation Plan

الإصدار: 1.0  
الحالة: جاهز للتنفيذ  
التاريخ: 2026-07-07  
المرجع: `17-POS_SALES_DOMAIN_DESIGN.md`

---

# الحالة الحالية

> ✅ Phase 11A مكتمل — Identity, Organization, Menu, Inventory, Security  
> ✅ Phase 11B.0 مكتمل — تصميم نطاق POS & Sales  
> 🚧 Milestone 1 الحالي — POS Core (Orders)

**Build Target:** 0 Errors / 0 Warnings

---

# تعليمات عامة

أنت Lead Software Architect لمشروع GastroERP.

المشروع يتبع:

- Clean Architecture
- Domain Driven Design (DDD)
- CQRS + MediatR
- Repository Pattern + Unit of Work
- AutoMapper + FluentValidation
- Multi-Tenancy
- SQL Server + ASP.NET Core .NET 9

**لا تغيّر المعمارية.**  
**لا تُدخل أنماط تصميم جديدة.**  
**اتبع اتفاقيات الكود الموجودة.**  
**أعد استخدام كل ما هو موجود قبل إنشاء ملفات جديدة.**

---

# الهدف

تنفيذ **النواة الأساسية لـ POS** — الطلبات وعناصرها وحالاتها وواجهات API الأساسية.

**نطاق Milestone 1:**

| ضمن النطاق | خارج النطاق (Milestones لاحقة) |
|------------|-------------------------------|
| SalesOrder Aggregate | Payment processing |
| OrderItem + OrderLineModifier | CashierShift |
| OrderDiscount + OrderTax | CashRegister |
| OrderStatusHistory | KitchenTicket |
| Order lifecycle (Draft → Completed) | Invoice / ZATCA |
| Core POS APIs | DeliveryOrder |
| Menu pricing ACL (snapshot) | Reservation |
| Inventory reservation hook (Confirm) | FloorPlan / Tables |
| Permissions seed | Promotions / Coupons |

---

# المرحلة 1 — Domain Layer

## 1.1 الملفات المطلوبة

```
GastroErp.Domain/
├── Entities/Sales/
│   ├── SalesOrder.cs
│   ├── OrderItem.cs
│   ├── OrderLineModifier.cs
│   ├── OrderDiscount.cs
│   ├── OrderTax.cs
│   └── OrderStatusHistory.cs
├── Enums/SalesEnums.cs
├── Events/Sales/SalesEvents.cs
└── ValueObjects/SalesValueObjects.cs
```

## 1.2 SalesEnums.cs

إنشاء الملف مع الـ Enums التالية (Milestone 1 فقط):

- `OrderStatus` — Draft, Pending, Confirmed, Preparing, Ready, Served, Completed, Cancelled, Archived
- `OrderType` — DineIn, TakeAway, Delivery, DriveThru, QROrdering, Kiosk
- `DiscountType` — Percentage, FixedAmount
- `KitchenItemStatus` — Pending, Preparing, Ready, Served, Voided
- `SyncStatus` — Local, PendingSync, Synced, Conflict

> **ملاحظة:** `PaymentStatus`, `ShiftStatus`, etc. تُضاف في Milestones لاحقة.

## 1.3 SalesValueObjects.cs

| VO | الحقول | القواعد |
|----|--------|---------|
| `OrderNumber` | Prefix, Sequence, Formatted | immutable record |
| `TaxAmount` | Amount, Currency, Rate | Amount ≥ 0 |
| `DiscountAmount` | Amount, Currency, IsPercentage, Percentage? | Amount ≥ 0 |
| `Quantity` | Value, Unit? | Value > 0 |

## 1.4 SalesOrder Aggregate

**يرث:** `AuditableBaseEntity`, `ITenantEntity`, `ICompanyEntity`, `IBranchEntity`

**الحقول الأساسية:** حسب `17-POS_SALES_DOMAIN_DESIGN.md` §3.1

**السلوكيات (Methods):**

```csharp
// Factory
static SalesOrder Create(tenantId, companyId, branchId, deviceId, cashierId, orderType, salesChannel, ...)

// Items
OrderItem AddItem(productSnapshot, quantity, modifiers, price)
void RemoveItem(orderItemId)
void VoidItem(orderItemId, reason, voidedBy)

// Discounts & Taxes
void ApplyDiscount(discountType, amount, appliedBy, couponId?)
void ApplyTax(taxName, rate, taxableAmount, isInclusive)
void RecalculateTotals()

// Lifecycle
void Submit(submittedBy)
void Confirm(confirmedBy)          // → OrderConfirmedEvent
void StartPreparing()
void MarkReady()
void MarkServed(servedBy)
void Complete(completedBy)           // → OrderCompletedEvent
void Cancel(reason, cancelledBy)   // → OrderCancelledEvent
void Archive()
void Reopen(reason, reopenedBy)    // Manager only

// Status
void RecordStatusChange(from, to, changedBy, deviceId, reason?)
bool CanTransitionTo(OrderStatus target)
```

**Invariants المطبّقة داخل Aggregate:**

- BR-S01: لا تعديل بعد Completed
- BR-S02: لا إلغاء بعد Completed
- BR-S03: transition matrix
- BR-S07: GrandTotal calculation
- BR-S11: price snapshot immutable
- BR-S12: void requires reason

## 1.5 OrderItem Entity

```csharp
internal OrderItem(...)  // constructor internal
void AddModifier(modifierSnapshot, quantity)
void Void(reason)
void UpdateKitchenStatus(KitchenItemStatus)
decimal CalculateLineTotal()
```

## 1.6 Domain Events

```csharp
// Events/Sales/SalesEvents.cs
record OrderCreatedEvent(Guid OrderId, Guid BranchId, Guid TenantId, OrderType OrderType, SalesChannel Channel, DateTimeOffset OccurredAt) : IDomainEvent;
record OrderSubmittedEvent(Guid OrderId, int ItemCount, decimal GrandTotal, DateTimeOffset OccurredAt) : IDomainEvent;
record OrderConfirmedEvent(Guid OrderId, Guid BranchId, Guid TenantId, DateTimeOffset OccurredAt) : IDomainEvent;
record OrderStatusChangedEvent(Guid OrderId, OrderStatus From, OrderStatus To, Guid ChangedBy, DateTimeOffset OccurredAt) : IDomainEvent;
record OrderCancelledEvent(Guid OrderId, string Reason, Guid CancelledBy, DateTimeOffset OccurredAt) : IDomainEvent;
record OrderCompletedEvent(Guid OrderId, decimal GrandTotal, string Currency, DateTimeOffset CompletedAt, DateTimeOffset OccurredAt) : IDomainEvent;
record OrderReopenedEvent(Guid OrderId, Guid ReopenedBy, string Reason, DateTimeOffset OccurredAt) : IDomainEvent;
record OrderItemVoidedEvent(Guid OrderId, Guid OrderItemId, string Reason, DateTimeOffset OccurredAt) : IDomainEvent;
```

## 1.7 ErrorCodes الجديدة

إضافة في `ErrorCodes.cs`:

```csharp
// Sales
public const string OrderNotFound = "SALES.ORDER_NOT_FOUND";
public const string OrderAlreadyClosed = "SALES.ORDER_ALREADY_CLOSED";
public const string OrderCannotBeCancelled = "SALES.ORDER_CANNOT_BE_CANCELLED";
public const string InvalidStatusTransition = "SALES.INVALID_STATUS_TRANSITION";
public const string OrderHasNoItems = "SALES.ORDER_HAS_NO_ITEMS";
public const string ItemNotFound = "SALES.ITEM_NOT_FOUND";
public const string ItemAlreadyVoided = "SALES.ITEM_ALREADY_VOIDED";
public const string VoidReasonRequired = "SALES.VOID_REASON_REQUIRED";
public const string TableRequired = "SALES.TABLE_REQUIRED";
public const string DeliveryAddressRequired = "SALES.DELIVERY_ADDRESS_REQUIRED";
public const string OfflineSalesNotAllowed = "SALES.OFFLINE_SALES_NOT_ALLOWED";
public const string ReopenWindowExpired = "SALES.REOPEN_WINDOW_EXPIRED";
public const string InsufficientStock = "SALES.INSUFFICIENT_STOCK";
public const string ProductNotAvailable = "SALES.PRODUCT_NOT_AVAILABLE";
```

## 1.8 MessageCodes

تفعيل الموجود + إضافة:

```csharp
public const string OrderCreated = "MSG.ORDER_CREATED";
public const string OrderCancelled = "MSG.ORDER_CANCELLED";
public const string OrderCompleted = "MSG.ORDER_COMPLETED";
public const string OrderConfirmed = "MSG.ORDER_CONFIRMED";
public const string OrderSubmitted = "MSG.ORDER_SUBMITTED";
public const string OrderItemVoided = "MSG.ORDER_ITEM_VOIDED";
```

---

# المرحلة 2 — Persistence Layer

## 2.1 الملفات

```
GastroErp.Persistence/
├── Configurations/Sales/
│   └── SalesConfigurations.cs
└── Migrations/
    └── {timestamp}_AddSalesOrderTables.cs
```

## 2.2 الجداول

| الجدول | المفتاح | Indexes |
|--------|---------|---------|
| `SalesOrders` | Id | (TenantId, BranchId, Status), (BranchId, OrderNumber) UNIQUE, DeviceId |
| `OrderItems` | Id | SalesOrderId, ProductId |
| `OrderLineModifiers` | Id | OrderItemId |
| `OrderDiscounts` | Id | SalesOrderId |
| `OrderTaxes` | Id | SalesOrderId |
| `OrderStatusHistories` | Id | SalesOrderId, ChangedAt |

## 2.3 EF Configuration Rules

اتبع نمط `MenuConfigurations.cs`:

- `HasPrecision(18, 4)` لكل decimal
- `HasMaxLength` لكل string
- `IsRowVersion()` للتفاؤل
- `HasQueryFilter(!IsDeleted)` للكيانات القابلة للحذف
- `OrderStatusHistory` **بدون** soft delete (append-only)
- Value Objects كـ owned types أو columns مباشرة:
  - `OrderNumber` → `OrderNumberPrefix` + `OrderNumberSequence` + computed `OrderNumberFormatted`
  - `Quantity` → `Quantity` column

## 2.4 العلاقات

```
SalesOrder 1──* OrderItem         (Cascade Restrict)
OrderItem  1──* OrderLineModifier (Cascade Restrict)
SalesOrder 1──* OrderDiscount     (Cascade Restrict)
SalesOrder 1──* OrderTax          (Cascade Restrict)
SalesOrder 1──* OrderStatusHistory (Cascade Restrict)
```

> **لا Foreign Keys** لـ Menu/Product — references بـ GUID فقط.

## 2.5 ApplicationDbContext

```csharp
public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
// EF يكتشف children عبر navigation أو explicit DbSets حسب النمط الموجود
```

تسجيل Configurations في `OnModelCreating` أو `ApplyConfigurationsFromAssembly`.

## 2.6 Migration

```bash
dotnet ef migrations add AddSalesOrderTables --project GastroErp.Persistence --startup-project GastroErp.Presentation
```

---

# المرحلة 3 — Application Layer

## 3.1 هيكل الملفات

```
GastroErp.Application/
├── Features/Sales/
│   ├── Commands/
│   │   ├── SalesCommands.cs
│   │   └── SalesCommandHandlers.cs
│   ├── Queries/
│   │   ├── SalesQueries.cs
│   │   └── SalesQueryHandlers.cs
│   ├── DTOs/
│   │   └── SalesDtos.cs
│   ├── Validators/
│   │   └── SalesValidators.cs
│   ├── Mapping/
│   │   └── SalesMappingProfile.cs
│   └── Services/
│       ├── IMenuPricingService.cs
│       ├── MenuPricingService.cs
│       ├── IOrderNumberGenerator.cs
│       └── OrderNumberGenerator.cs
```

## 3.2 Commands

```csharp
// SalesCommands.cs
public record CreateOrderCommand(CreateOrderDto Dto) : IRequest<Result<OrderDto>>;
public record AddOrderItemCommand(Guid OrderId, AddOrderItemDto Dto) : IRequest<Result<OrderItemDto>>;
public record RemoveOrderItemCommand(Guid OrderId, Guid ItemId) : IRequest<Result>;
public record VoidOrderItemCommand(Guid OrderId, Guid ItemId, VoidOrderItemDto Dto) : IRequest<Result>;
public record ApplyOrderDiscountCommand(Guid OrderId, ApplyDiscountDto Dto) : IRequest<Result>;
public record SubmitOrderCommand(Guid OrderId) : IRequest<Result>;
public record ConfirmOrderCommand(Guid OrderId) : IRequest<Result>;
public record UpdateOrderStatusCommand(Guid OrderId, UpdateOrderStatusDto Dto) : IRequest<Result>;
public record CompleteOrderCommand(Guid OrderId) : IRequest<Result>;
public record CancelOrderCommand(Guid OrderId, CancelOrderDto Dto) : IRequest<Result>;
public record ReopenOrderCommand(Guid OrderId, ReopenOrderDto Dto) : IRequest<Result>;
```

## 3.3 Queries

```csharp
// SalesQueries.cs
public record GetOrderByIdQuery(Guid Id) : IRequest<Result<OrderDetailDto>>;
public record GetOrdersQuery(OrderFilterDto Filter) : IRequest<Result<PagedResult<OrderSummaryDto>>>;
public record GetActiveOrdersByBranchQuery(Guid BranchId) : IRequest<Result<List<OrderSummaryDto>>>;
public record GetOrdersByTableQuery(Guid TableId) : IRequest<Result<List<OrderSummaryDto>>>;
public record GetOrderStatusHistoryQuery(Guid OrderId) : IRequest<Result<List<OrderStatusHistoryDto>>>;
```

## 3.4 DTOs

```csharp
// ─── Request DTOs ───
public record CreateOrderDto(
    Guid BranchId,
    Guid DeviceId,
    OrderType OrderType,
    SalesChannel SalesChannel,
    Guid? TableId,
    int? GuestCount,
    string? Notes
);

public record AddOrderItemDto(
    Guid ProductId,
    decimal Quantity,
    string? Notes,
    List<AddOrderLineModifierDto>? Modifiers
);

public record AddOrderLineModifierDto(Guid ModifierId, int Quantity);

public record ApplyDiscountDto(DiscountType Type, decimal Value, string? Description);

public record CancelOrderDto(string Reason);
public record ReopenOrderDto(string Reason);
public record VoidOrderItemDto(string Reason);
public record UpdateOrderStatusDto(OrderStatus TargetStatus);

public record OrderFilterDto(
    Guid? BranchId,
    OrderStatus? Status,
    OrderType? OrderType,
    SalesChannel? SalesChannel,
    DateTimeOffset? FromDate,
    DateTimeOffset? ToDate,
    string? SearchTerm,
    int Page = 1,
    int PageSize = 20
);

// ─── Response DTOs ───
public record OrderDto(...);
public record OrderDetailDto(...);       // includes items, discounts, taxes, history
public record OrderSummaryDto(...);     // list view
public record OrderItemDto(...);
public record OrderLineModifierDto(...);
public record OrderDiscountDto(...);
public record OrderTaxDto(...);
public record OrderStatusHistoryDto(...);
```

## 3.5 Validators (FluentValidation)

| Validator | القواعد |
|-----------|---------|
| `CreateOrderCommandValidator` | BranchId, DeviceId required; TableId required if DineIn |
| `AddOrderItemCommandValidator` | ProductId, Quantity > 0 |
| `ApplyOrderDiscountCommandValidator` | Value > 0; Percentage ≤ 100 |
| `CancelOrderCommandValidator` | Reason required, min 3 chars |
| `VoidOrderItemCommandValidator` | Reason required |
| `OrderFilterDtoValidator` | PageSize ≤ 100 |

## 3.6 AutoMapper Profile

```csharp
public class SalesMappingProfile : Profile
{
    public SalesMappingProfile()
    {
        CreateMap<SalesOrder, OrderDto>();
        CreateMap<SalesOrder, OrderDetailDto>();
        CreateMap<SalesOrder, OrderSummaryDto>();
        CreateMap<OrderItem, OrderItemDto>();
        CreateMap<OrderLineModifier, OrderLineModifierDto>();
        CreateMap<OrderDiscount, OrderDiscountDto>();
        CreateMap<OrderTax, OrderTaxDto>();
        CreateMap<OrderStatusHistory, OrderStatusHistoryDto>();
    }
}
```

## 3.7 ACL Services

### IMenuPricingService

```csharp
public interface IMenuPricingService
{
    Task<ProductPriceSnapshot> GetProductPriceAsync(
        Guid productId, Guid branchId, SalesChannel channel, Guid? priceLevelId, CancellationToken ct);

    Task<bool> IsProductAvailableAsync(Guid productId, Guid branchId, CancellationToken ct);
}

public record ProductPriceSnapshot(
    Guid ProductId,
    string NameAr, string NameEn,
    string? Sku,
    decimal UnitPrice,
    string Currency,
    List<ModifierPriceSnapshot> AvailableModifiers
);

public record ModifierPriceSnapshot(
    Guid ModifierId, string NameAr, string NameEn, decimal ExtraPrice
);
```

**التنفيذ:** يقرأ من Menu DbContext — يحسب السعر حسب الأولوية:
1. MenuItem.OverridePrice
2. ProductPriceLevel
3. Product.BasePrice

### IOrderNumberGenerator

```csharp
public interface IOrderNumberGenerator
{
    Task<OrderNumber> GenerateAsync(Guid branchId, CancellationToken ct);
}
```

**التنفيذ:** `{BranchCode}-{Year}-{Sequence:D5}` — sequence per branch per year.

## 3.8 Command Handler Patterns

اتبع نمط `ProductHandlers.cs`:

```csharp
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    // 1. Validate branch access (current user)
    // 2. Validate device belongs to branch
    // 3. Generate OrderNumber
    // 4. SalesOrder.Create(...)
    // 5. _unitOfWork.SalesOrders.Add(order)
    // 6. await _unitOfWork.SaveChangesAsync()
    // 7. return Result.Success(_mapper.Map<OrderDto>(order))
}
```

### ConfirmOrderCommandHandler — Inventory Hook

```csharp
// 1. Load order
// 2. order.Confirm(userId)
// 3. if (inventorySetting.AutoReserveStock)
//      await _inventoryReservationService.ReserveForOrderAsync(order, ct)
// 4. SaveChanges
```

> `IInventoryReservationService` يُنشأ كـ interface في Application، يُنفَّذ في Infrastructure أو Inventory feature — يستخدم `InventoryReservation` الموجود.

## 3.9 Repository

```csharp
// في GastroErp.Application/Common/Interfaces/ أو Infrastructure
public interface ISalesOrderRepository
{
    Task<SalesOrder?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<SalesOrder?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct);
    Task<PagedList<SalesOrder>> GetPagedAsync(OrderFilter filter, CancellationToken ct);
    Task<List<SalesOrder>> GetActiveByBranchAsync(Guid branchId, CancellationToken ct);
    void Add(SalesOrder order);
    void Update(SalesOrder order);
}
```

---

# المرحلة 4 — Presentation Layer

## 4.1 ApiRoutes

```csharp
public static class Sales
{
    public const string Orders = $"{Root}/sales/orders";
}
```

## 4.2 Controller

```
GastroErp.Presentation/Controllers/Sales/OrderController.cs
```

```csharp
[ApiController]
[ApiVersion("1.0")]
[Route(ApiRoutes.Sales.Orders)]
[Authorize]
public class OrderController : ApiControllerBase
{
    // POST   /                     → CreateOrder
    // GET    /                     → GetOrders (filtered, paged)
    // GET    /{id}                 → GetOrderById
    // GET    /{id}/history         → GetOrderStatusHistory
    // GET    /branch/{branchId}/active → GetActiveOrdersByBranch
    // POST   /{id}/items           → AddOrderItem
    // DELETE /{id}/items/{itemId}  → RemoveOrderItem
    // POST   /{id}/items/{itemId}/void → VoidOrderItem
    // POST   /{id}/discounts       → ApplyOrderDiscount
    // POST   /{id}/submit          → SubmitOrder
    // POST   /{id}/confirm         → ConfirmOrder
    // PATCH  /{id}/status          → UpdateOrderStatus
    // POST   /{id}/complete        → CompleteOrder
    // POST   /{id}/cancel          → CancelOrder
    // POST   /{id}/reopen          → ReopenOrder
}
```

## 4.3 Permissions

إضافة في `Permissions.cs`:

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
```

**Authorization على Endpoints:**

| Endpoint | Permission |
|----------|------------|
| GET * | `Sales.View` |
| POST / (create) | `Sales.Create` |
| POST items, discounts, submit, confirm, status | `Sales.Update` |
| POST complete | `Sales.Complete` |
| POST cancel | `Sales.Cancel` |
| POST reopen | `Sales.Reopen` |
| POST void | `Sales.VoidItem` |

## 4.4 Permission Seed

إضافة في `PermissionSeeder` (أو الملف الموجود):

```csharp
new("Sales", "View", "عرض الطلبات"),
new("Sales", "Create", "إنشاء طلب"),
new("Sales", "Update", "تعديل طلب"),
new("Sales", "Cancel", "إلغاء طلب"),
new("Sales", "Complete", "إغلاق طلب"),
new("Sales", "Reopen", "إعادة فتح طلب"),
new("Sales", "VoidItem", "إلغاء عنصر"),
```

---

# المرحلة 5 — DI Registration

```csharp
// Application DI
services.AddScoped<IMenuPricingService, MenuPricingService>();
services.AddScoped<IOrderNumberGenerator, OrderNumberGenerator>();
services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();

// AutoMapper
// يُكتشف تلقائياً عبر ApplyConfigurationsFromAssembly إذا SalesMappingProfile في نفس Assembly

// FluentValidation
// يُكتشف تلقائياً
```

---

# المرحلة 6 — ترتيب التنفيذ

```
Step 1:  Domain — Enums + ValueObjects + Events
Step 2:  Domain — SalesOrder + children entities
Step 3:  Domain — ErrorCodes + MessageCodes
Step 4:  Persistence — EF Configurations
Step 5:  Persistence — Migration + verify
Step 6:  Application — DTOs
Step 7:  Application — ACL Services (MenuPricing, OrderNumber)
Step 8:  Application — Repository
Step 9:  Application — Commands + Handlers
Step 10: Application — Queries + Handlers
Step 11: Application — Validators
Step 12: Application — AutoMapper Profile
Step 13: Presentation — ApiRoutes + Permissions
Step 14: Presentation — OrderController
Step 15: DI Registration
Step 16: Build verify (0 errors, 0 warnings)
Step 17: Manual API test
```

---

# المرحلة 7 — اختبار يدوي (Test Plan)

## 7.1 سيناريوهات أساسية

| # | السيناريو | الخطوات | النتيجة المتوقعة |
|---|-----------|---------|-----------------|
| T1 | إنشاء طلب DineIn | POST /orders + tableId | Status=Draft, OrderNumber generated |
| T2 | إضافة عناصر | POST /orders/{id}/items × 3 | Items with price snapshots |
| T3 | إضافة modifiers | POST with modifiers | Modifier snapshots |
| T4 | خصم | POST /orders/{id}/discounts | DiscountTotal updated |
| T5 | Submit | POST /orders/{id}/submit | Status=Pending |
| T6 | Confirm | POST /orders/{id}/confirm | Status=Confirmed, inventory reserved |
| T7 | Lifecycle | PATCH status → Preparing → Ready → Served | Status history recorded |
| T8 | Complete | POST /orders/{id}/complete | Status=Completed |
| T9 | Cancel | Create + Cancel | Status=Cancelled, reservations released |
| T10 | Void item | Void one item | Item voided, totals recalculated |
| T11 | Validation | Confirm empty order | 400 OrderHasNoItems |
| T12 | Validation | Modify completed order | 400 OrderAlreadyClosed |
| T13 | Pagination | GET /orders?page=1&pageSize=10 | Paged result |
| T14 | Branch filter | GET /orders?branchId=... | Filtered results |
| T15 | Permissions | Call without Sales.Create | 403 Forbidden |

## 7.2 اختبار التكامل

| # | التكامل | التحقق |
|---|---------|--------|
| I1 | Menu Pricing ACL | Price matches ProductPriceLevel |
| I2 | Inventory Reservation | Reservation created on Confirm |
| I3 | Multi-tenancy | Cannot access other tenant's orders |
| I4 | Branch scoping | User sees only authorized branches |

---

# المرحلة 8 — معايير القبول (Definition of Done)

- [ ] Build = 0 Errors, 0 Warnings
- [ ] 6 Domain entities + 5 enums + 4 VOs + 8 events
- [ ] 1 Migration applied successfully
- [ ] 12 Commands + 5 Queries implemented
- [ ] 12 Validators
- [ ] 1 AutoMapper Profile
- [ ] 1 Controller with 14 endpoints
- [ ] 7 Permissions seeded
- [ ] Menu Pricing ACL working
- [ ] Inventory reservation hook on Confirm
- [ ] Order lifecycle Draft → Completed working
- [ ] Status history audit trail
- [ ] Multi-tenancy enforced
- [ ] All 15 test scenarios pass

---

# Milestones لاحقة

| Milestone | النطاق | التبعية |
|-----------|--------|---------|
| **M2 — Payments & Shift** | Payment, CashierShift, CashRegister | M1 |
| **M3 — Floor & Kitchen** | FloorPlan, Table, KitchenTicket, KDS APIs | M1 |
| **M4 — Invoice & Delivery** | Invoice (ZATCA), DeliveryOrder | M1, M2 |
| **M5 — Promotions & CRM** | Coupon, Promotion, Reservation, Customer | M1 |

---

نهاية خطة التنفيذ — Phase 11B Milestone 1
