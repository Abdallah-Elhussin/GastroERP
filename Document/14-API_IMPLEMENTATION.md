# خطة تنفيذ طبقة العرض (API Layer)

تهدف هذه المرحلة إلى إنشاء طبقة Presentation الخاصة بنظام GastroERP وربطها مع طبقة Application باستخدام MediatR مع الالتزام الكامل بمبادئ Clean Architecture.

---

# الهدف

بناء واجهات REST API احترافية تدعم جميع وحدات النظام وتكون جاهزة للتطبيقات التالية:

- Angular Web
- POS Desktop
- POS Tablet
- Mobile App
- Kitchen Display System (KDS)
- Customer App
- Delivery App
- Third Party Integration

---

# المبادئ العامة

يجب أن تكون طبقة API مسؤولة فقط عن:

- استقبال الطلب
- التحقق من هوية المستخدم
- استدعاء MediatR
- إعادة Result المناسب

ولا تحتوي إطلاقاً على Business Logic.

---

# Proposed Changes

## 1. إنشاء الهيكل العام

إنشاء المجلدات التالية:

API/

Controllers/

Middlewares/

Filters/

Authorization/

Authentication/

Localization/

Swagger/

Versioning/

HealthChecks/

Configuration/

Extensions/

Contracts/

Responses/

Requests/

OpenApi/

Common/

---

## 2. Controllers

إنشاء Controllers لكل Module.

OrganizationController

CompanyController

BranchController

DepartmentController

DeviceController

MenuController

CategoryController

ProductController

ModifierController

ComboController

InventoryController

WarehouseController

SupplierController

PurchaseController

RecipeController

StockController

---

## 3. API Versioning

اعتماد Versioning منذ البداية.

مثال:

/api/v1/organization

/api/v1/menu

/api/v1/inventory

ليكون النظام جاهزاً للإصدارات المستقبلية.

---

## 4. Swagger

إعداد Swagger بالكامل.

يشمل:

JWT Authentication

Examples

XML Documentation

Operation Filters

Schema Filters

Response Types

Localization

Grouping

---

## 5. Authentication

ربط JWT Authentication.

Bearer Token

Refresh Token

Claims

Roles

Permissions

Policies

---

## 6. Authorization

إنشاء Permission Based Authorization.

بدلاً من Roles فقط.

مثل:

Organization.View

Organization.Create

Organization.Update

Organization.Delete

Inventory.Manage

Menu.Publish

Sales.Create

Kitchen.View

Reports.View

---

## 7. Exception Middleware

إنشاء Global Exception Middleware.

المهام:

تحويل جميع Exceptions إلى Result موحد.

Localization

Logging

Problem Details

HTTP Status Codes

---

## 8. Validation Middleware

ربط FluentValidation.

أي Request غير صالح يرجع:

ValidationResult

بدون الوصول إلى Handler.

---

## 9. Localization

ربط Accept-Language Header.

مثال:

ar

en

ويتم اختيار اللغة تلقائياً.

---

## 10. Result Wrapper

جميع Endpoints ترجع:

Result

Result<T>

PagedResult<T>

ولا يتم إرجاع Entities مباشرة.

---

## 11. Health Checks

إضافة Health Checks.

Database

Cache

Storage

Localization

Background Jobs

---

## 12. Rate Limiting

إعداد Rate Limiting.

لحماية النظام.

---

## 13. CORS

إعداد CORS.

Development

Testing

Production

---

## 14. Compression

إضافة:

Response Compression

Gzip

Brotli

---

## 15. Security Headers

إضافة:

X-Content-Type

XSS Protection

Content Security Policy

Frame Options

HSTS

---

## 16. Logging

ربط Serilog.

Logging لكل Request.

Execution Time

User

Tenant

IP

Machine

CorrelationId

---

## 17. Correlation ID

إنشاء CorrelationId Middleware.

لتتبع جميع العمليات.

---

## 18. API Documentation

توثيق جميع Endpoints باستخدام XML Comments.

يشمل:

Summary

Remarks

Response Codes

Examples

Authorization

---

## 19. Verification Plan

التأكد من:

✓ Build Success

✓ Swagger يعمل

✓ JWT يعمل

✓ Authorization يعمل

✓ Localization تعمل

✓ Validation تعمل

✓ جميع Controllers تستدعي MediatR فقط

✓ لا يوجد Business Logic داخل Controllers

✓ جميع Endpoints ترجع Result Pattern

✓ جميع الأخطاء تمر عبر Global Exception Middleware

✓ جميع الطلبات يتم تسجيلها في Serilog

✓ جميع الخدمات مسجلة في Dependency Injection

✓ عدم وجود أي Hardcoded Messages

✓ الالتزام الكامل بـ Clean Architecture