# خطة وحدة المبيعات الإدارية (Back Office Sales)
# GastroERP — Back Office Sales Module Plan

> **الحالة:** معتمدة للتنفيذ — يوليو 2026  
> **النطاق:** المبيعات الداخلية (Back Office) فقط  
> **خارج النطاق:** نقطة البيع (POS) وجميع ملحقاتها  
> **المعمارية:** Clean Architecture + DDD + CQRS + MediatR  

---

## 1. الهدف

بناء وحدة مبيعات احترافية للمبيعات الإدارية داخل الشركة (B2B / آجل / نقدي / اعتماد داخلي)، مستقلة تماماً عن POS.

- التحليل السابق للنظام القديم يُستخدم فقط لاستخراج **Business Rules** وسير العمل.
- **يُمنع** نقل كود أو Feature Folders أو God Services أو ViewModels من النظام القديم.
- أي تكامل مع POS يتم عبر **واجهات/خدمات مشتركة** فقط (أصناف، عملاء، مخزون، تسعير)، دون دمج منطق POS داخل هذه الوحدة.

---

## 2. حدود النطاق (Hard Boundary)

### داخل النطاق ✅

| الشاشة / القدرة | الاسم الإنجليزي |
|-----------------|-----------------|
| لوحة المبيعات | Sales Dashboard |
| عروض الأسعار | Quotation |
| أوامر البيع | Sales Orders (Administrative) |
| سندات التسليم | Delivery Notes |
| فواتير المبيعات | Sales Invoices |
| مرتجعات المبيعات | Sales Returns |
| إشعارات المدين | Debit Notes |
| العملاء (تجاري) | Customers / Commercial profile |
| التسعير والعروض | Pricing & Promotions (BO) |
| التقارير | Sales Reports |
| التحليلات | Analytics |

### خارج النطاق ❌ (وحدة POS مستقلة)

- POS Checkout، جلسات الكاشير، الأدراج، محطات البيع
- باركود الكاشير، شاشة الدفع السريع
- الطاولات، KDS، الطلبات السريعة، الطابعات الحرارية، أجهزة الدفع

### قاعدة ذهبية للكود

```text
❌ لا تستخدم Domain.Entities.Sales.SalesOrder (كيان POS) كأمر بيع إداري.
✅ أنشئ aggregates جديدة تحت سياق Back Office Sales (أسماء مقترحة أدناه).
```

---

## 3. تسمية الـ Aggregates المقترحة

لتجنب التعارض مع POS الحالي (`SalesOrder` / `OrderItem`):

| المفهوم | Aggregate مقترح | ملاحظات |
|---------|-----------------|---------|
| عرض سعر | `SalesQuotation` | قابل للتحويل لأمر بيع |
| أمر بيع إداري | `BackOfficeSalesOrder` | **ليس** `SalesOrder` الخاص بـ POS |
| سند تسليم | `SalesDeliveryNote` | يخصم/يحجز مخزون عند الترحيل |
| فاتورة مبيعات | `SalesInvoice` أو توسيع `Invoice` بمسار BO | يُفضّل مسار BO واضح المصدر |
| مرتجع مبيعات | `SalesReturn` | مرتبط بالفاتورة/التسليم |
| إشعار مدين | استخدام/توسعة `DebitNote` (Invoicing) | مسار BO فقط في الواجهة |

> قرار تصميم Phase 0: تثبيت الأسماء النهائية قبل أول Migration.

---

## 4. دورة المستند الموحدة

جميع مستندات الوحدة تتبع:

```text
Draft (0) → Approved (1) → Posted (2) → Unposted/Reversed (8) → Cancelled (9)
```

### قواعد إلزامية

| القاعدة | التفاصيل |
|---------|----------|
| لا ترحيل قبل الاعتماد | `Post` يتطلب `Approved` |
| لا تعديل بعد الترحيل | التعديل فقط في `Draft` |
| لا حذف بعد الترحيل | Soft-cancel / reverse فقط |
| إلغاء الاعتماد | مسموح قبل الترحيل فقط (`Approved` → `Draft`) |
| عكس الترحيل | يعكس القيود + المخزون + الضرائب + الذمم |
| المرتجع | مرتبط بالمستند الأصلي؛ الكمية ≤ المتاح للإرجاع |

---

## 5. أنواع فواتير المبيعات

| النوع | التأثير على المخزون | القيد الأساسي |
|-------|---------------------|---------------|
| Inventory Sales | نعم (OUT + COGS) | إيراد + ضريبة + ذمم/نقد + COGS |
| Service Sales | لا | إيراد خدمات + ضريبة |
| Project Sales | حسب البنود | إيراد مشروع |
| Asset Sales | لا (أصل ثابت) | إيراد أصول / استبعاد |
| Mixed Invoice | حسب طبيعة كل بند | قيد مركّب |

---

## 6. المحاسبة (عند الترحيل فقط)

### بيع نقدي
```text
مدين: الصندوق / البنك
دائن: المبيعات
دائن: ضريبة المخرجات
```

### بيع آجل
```text
مدين: العميل (AR)
دائن: المبيعات
دائن: ضريبة المخرجات
```

### بيع مخزون (إضافي)
```text
مدين: تكلفة البضاعة المباعة (COGS)
دائن: المخزون
```

### مرتجع
عكس الإيراد والضريبة والمخزون وCOGS والذمم/النقد حسب الحالة الأصلية.

---

## 7. التكامل مع الوحدات

| الوحدة | التكامل |
|--------|---------|
| Inventory | حجز، تسليم، خصم، تكلفة، عكس عند Unpost |
| Finance | قيود يومية، ذمم، إيراد، ضريبة، COGS، فترة مالية |
| CRM | عملاء، تصنيفات، حد ائتمان، شروط دفع |
| Tax | VAT، إعفاءات، جاهزية فاتورة إلكترونية |
| Delivery | سند تسليم / شحن / تتبع (بدون POS) |
| Notifications | اعتماد، ترحيل، مرتجع |
| Shared Catalog | أصناف/منتجات مشتركة عبر Interfaces فقط |

---

## 8. Multi-Tenant والحقول المشتركة

كل مستند يدعم:

`TenantId`, `CompanyId`, `BranchId`, `WarehouseId?`, `CostCenterId?`, `Currency`, `ExchangeRate`, `FiscalPeriodId?`

Audit:

`CreatedBy/At`, `UpdatedBy/At`, `ApprovedBy/At`, `PostedBy/At`, `CancelledBy/At`, IP/Device/Session عند التوفر

---

## 9. CQRS و Domain Events

### Commands (لكل مستند رئيسي)
`Create`, `Update`, `Approve`, `Unapprove`, `Post`, `Unpost`, `Cancel`, `Delete` (+ `Print`, `Copy` حسب الحاجة)

### Queries
`GetById`, `Search/List`, `Dashboard`, `Reports`, `Statistics`

### Domain Events (أمثلة)
`SalesInvoiceCreated`, `SalesInvoiceApproved`, `SalesInvoicePosted`, `SalesInvoiceCancelled`,  
`SalesReturnPosted`, `StockReserved`, `StockReleased`, `CustomerBalanceChanged`

---

## 10. الصلاحيات (RBAC)

صلاحيات مستقلة لكل عملية على الأقل:

`Create`, `Update`, `Delete`, `Approve`, `Unapprove`, `Post`, `Unpost`, `Cancel`,  
`Print`, `Export`, `Reopen`, `EditPrices`, `EditDiscounts`, `EditTaxes`, `ViewDashboard`, `ViewReports`

Namespace مقترح: `BackOfficeSales.*` أو `Sales.BackOffice.*`  
(لا تخلط مع صلاحيات POS الحالية مثل `Sales.Complete` المرتبطة بطلبات الكاشير).

---

## 11. هيكل الطبقات (GastroERP)

```text
Domain/
  Entities/Sales/BackOffice/     # Aggregates جديدة فقط
  Enums/                         # حالات وطبيعة الفاتورة
  Events/Sales/BackOffice/

Application/
  Features/Sales/BackOffice/
    Quotations/
    Orders/
    DeliveryNotes/
    Invoices/
    Returns/
    DebitNotes/
    Dashboard/
    Reports/
    Customers/                   # أو توسيع CRM بملف تجاري
  Common/Interfaces/Sales/

Persistence/
  Configurations/Sales/BackOffice/
  Migrations/

Presentation/
  Controllers/Sales/BackOffice/
  ApiRoutes: /api/v1/back-office-sales/...   # مفضّل للفصل عن /sales (POS)

Frontend/
  features/back-office-sales/    # أو features/sales مع فصل واضح عن /pos
```

> **API:** يُفضّل بادئة منفصلة `/api/v1/back-office-sales` حتى لا تختلط مع `/api/v1/sales` الحالية الخاصة بـ POS.

---

## 12. خارطة التنفيذ بالمراحل

### Phase 0 — التأسيس (أسبوع قصير)
- [x] تثبيت أسماء Aggregates ومسارات API
- [x] Enum دورة المستند الموحدة + طبيعة الفاتورة
- [x] صلاحيات `BackOfficeSales.*` في `Permissions` + Catalog
- [x] مستند ADR قصير: فصل BO Sales عن POS (`ADR-001-BackOffice-Sales-vs-POS.md`)
- [x] تحديث فهرس الوحدات

**مخرجات:** حدود واضحة للكود + بداية Phase 2 (فاتورة).

---

### Phase 1 — العملاء التجاريون + لوحة أولية
- [x] توسيع ملف العميل: حد ائتمان، شروط دفع، الرقم الضريبي، حساب ذمة
- [x] CRM Controller (`/api/v1/crm/customers`) لربط الواجهة
- [x] Dashboard يعتمد بيانات **فواتير BO** (`/api/v1/back-office-sales/dashboard`)
- [x] شريط تنقل `/sales` بدون روابط POS تشغيلية داخل الوحدة
- [x] مزامنة صلاحيات جديدة (`BackOfficeSales.*` / `Crm.*`) + منح محاسب/مدير فرع

**مخرجات:** عميل جاهز للبيع الآجل + لوحة هيكل.

---

### Phase 2 — فاتورة المبيعات الإدارية (الأولوية التشغيلية)
- [x] Aggregate فاتورة BO (`BackOfficeSalesInvoice`)
- [x] دورة Draft → Approve → Post → Unpost → Cancel
- [x] أنواع: Inventory / Service / Asset / Mixed
- [x] ترحيل محاسبي + مخزون (عند البنود المخزنية) + ضريبة
- [x] شاشات قائمة + نموذج أولي تحت `/sales/invoices`
- [x] منع الترحيل قبل الاعتماد؛ منع التعديل بعد الترحيل
- [x] انتقاء عميل/صنف/مستودع/وحدة من قوائم الاختيار
- [x] مطابقة كاملة مع أمر البيع (Phase 3)

**مخرجات:** بيع إداري كامل في مستند واحد (نقدي/آجل).

---

### Phase 3 — أمر البيع + سند التسليم
- [x] `BackOfficeSalesOrder`: كميات، أسعار، اعتماد، تتبع تسليم/فوترة
- [x] `BackOfficeSalesDeliveryNote`: تسليم جزئي/كلي من الأمر
- [x] تحويل أمر → فاتورة (مطابقة كميات)
- [x] منع تجاوز الكميات المسلّمة/المفوترة
- [x] تحويل عرض سعر → أمر (Phase 4 مرتبط)

**مخرجات:** دورة أمر → تسليم → فاتورة.

---

### Phase 4 — عرض السعر + المرتجعات + المدين
- [x] `BackOfficeSalesQuotation` + تحويل لأمر بيع
- [x] `BackOfficeSalesReturn` مرتبط بفاتورة؛ عكس محاسبي/مخزني
- [x] إشعارات المدين (`BackOfficeSalesDebitNote`) لمسار BO
- [x] قواعد: لا إرجاع أكبر من المباعة

**مخرجات:** دورة كاملة عرض → أمر → تسليم → فاتورة → مرتجع.

---

### Phase 5 — التقارير والتحليلات والتدقيق
- [x] تقارير: ملخص فترة، عميل، صنف، يومي، عدّادات الحالات
- [x] أعلى عملاء/أصناف
- [x] Audit Log على ترحيل التسليم/المرتجع/المدين (IAuditLogger)
- [ ] مرفقات، طباعة، نسخ، تصدير PDF/Excel/CSV (لاحق — خارج نطاق التشغيل الأساسي)
- [x] أداء: AsNoTracking، Pagination، تجنب N+1 في التقارير

---

## 13. معايير القبول العامة (Definition of Done)

لكل مستند رئيسي:

1. Commands + Queries + FluentValidation + Handler رفيع
2. قواعد الحالة داخل Domain (لا في Controller)
3. ترحيل محاسبي/مخزني فقط عند Post
4. Unpost يعكس الحركات
5. صلاحيات لكل عملية
6. اختبارات على الأقل لمسار Approve → Post → Unpost
7. شاشة قائمة + نموذج RTL عربي/إنجليزي
8. لا اعتماد على كيانات/خدمات POS الداخلية

---

## 14. ما يُعاد استخدامه من المنصة (Shared فقط)

يُسمح بإعادة استخدام **القدرات المشتركة** وليس منطق POS:

- `IInventoryMovementPipeline`
- `IJournalPostingService` / إعدادات الحسابات
- كتالوج الأصناف/المنتجات
- كيان العميل (مع توسيع تجاري)
- Tax codes / registrations
- أنماط UI المشتركة (shell، جداول، صلاحيات)

يُمنع:

- استدعاء `OrderController` / دورة مطبخ / ورديات / مدفوعات POS كجزء من مسار BO
- توسيع `SalesOrder` (POS) بحالات إدارية (Approved/Posted الإدارية)

---

## 15. ترتيب التنفيذ الفوري المقترح

```text
Phase 0 (حدود + صلاحيات + ADR)
    → Phase 2 (فاتورة BO)     ← أعلى قيمة تشغيلية
    → Phase 1 بالتوازي (عميل ائتماني + Dashboard من فواتير BO)
    → Phase 3 (أمر + تسليم)
    → Phase 4 (عرض + مرتجع + مدين)
    → Phase 5 (تقارير + Audit + تصدير)
```

---

## 16. سجل القرارات

| التاريخ | القرار |
|---------|--------|
| 2026-07-19 | إكمال Phases 3–5: أمر، تسليم، عرض، مرتجع، مدين، تقارير |

| 2026-07-18 | التحليل القديم = Business Rules فقط؛ لا نقل كود |
| 2026-07-18 | دورة مستند موحدة: Draft → Approved → Posted → Unposted → Cancelled |
| 2026-07-18 | هذا المستند هو مصدر الحقيقة للخطة حتى يُستبدل بـ ADR أحدث |

---

## 17. مراجع ذات صلة

- POS (مستقل): `16 - POS & Sales Domain Design.md`, `17-POS_SALES_DOMAIN_DESIGN.md`
- CRM: `21 -CRM & Loyalty Management.md`
- Finance: `22-Finance & Accounting Integration.md`
- Inventory architecture: `/docs/inventory-architecture/`
- فهرس الوحدات: [Modules/README.md](./README.md)

---

**المالك:** فريق معمارية GastroERP  
**آخر تحديث:** 18 يوليو 2026
