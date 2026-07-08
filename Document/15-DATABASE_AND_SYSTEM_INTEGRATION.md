# 15-DATABASE_AND_SYSTEM_INTEGRATION.md

# Database & System Integration Implementation

بعد الانتهاء من طبقات **Domain**, **Application**, **Infrastructure**, و **Presentation (API)**، أصبحت جميع طبقات النظام مكتملة من الناحية المعمارية.

تهدف هذه المرحلة إلى ربط جميع الطبقات مع بعضها وتحويل النظام من هيكل معماري إلى نظام ERP يعمل فعلياً مع قاعدة البيانات.

---

# User Review Required

> [!IMPORTANT]
> هذه المرحلة لا تضيف Business Logic جديد، وإنما تركز على التكامل (Integration)، والتحقق من صحة النظام، وتحسين الأداء، والتأكد من جاهزية النظام للإنتاج.

---

# Proposed Changes

## المرحلة الأولى — مراجعة طبقة Persistence

### [VERIFY] Entity Framework Core Configurations

مراجعة جميع ملفات الـ Configuration والتأكد من:

- Primary Keys
- Foreign Keys
- Navigation Properties
- Cascade Delete Behaviors
- Restrict Delete Behaviors
- Composite Keys
- Unique Indexes
- Performance Indexes
- Decimal Precision
- Default Values
- Check Constraints
- Concurrency Tokens (RowVersion)
- Owned Types
- Value Converters
- Global Query Filters
- Soft Delete
- Multi-Tenant Filters

---

## المرحلة الثانية — Database Migration

### [NEW] Initial Migration

إنشاء أول Migration للنظام.

يشمل:

- إنشاء جميع الجداول
- إنشاء العلاقات
- إنشاء الفهارس
- إنشاء القيود
- إنشاء الـ Views إن وجدت
- إنشاء Stored Procedures (إذا احتاج النظام مستقبلاً)

ثم تنفيذ:

```
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## المرحلة الثالثة — Database Seeding

### [NEW] Seed Data

إضافة البيانات الأساسية للنظام.

يشمل:

- Default Tenant
- Super Admin
- Administrator Role
- Default Permissions
- Default Company
- Default Branch
- Default Warehouse
- Default Inventory Settings
- Default Menu Categories
- Default Units
- Default Currencies
- Default Languages
- Default Tax Rates

يجب أن يكون الـ Seed قابلاً لإعادة التشغيل بدون إنشاء بيانات مكررة.

---

## المرحلة الرابعة — Authentication Integration

ربط نظام تسجيل الدخول بالكامل.

يشمل:

- JWT Authentication
- Refresh Tokens
- Claims
- Roles
- Permissions
- Current User
- Tenant Resolution
- Branch Resolution

والتأكد من أن جميع الـ Endpoints تعتمد على:

```
[Authorize]
```

أو

```
[HasPermission(...)]
```

حسب الحاجة.

---

## المرحلة الخامسة — API Integration

ربط جميع Controllers مع Application Layer.

التأكد من:

- Commands تعمل
- Queries تعمل
- Validation يعمل
- Mapping يعمل
- Localization يعمل
- Result Pattern يعمل
- Exception Handling يعمل

بدون أي Business Logic داخل Controllers.

---

## المرحلة السادسة — Swagger Verification

مراجعة Swagger بالكامل.

يشمل:

- JWT Authentication
- API Versioning
- XML Documentation
- Grouping
- Request Examples
- Response Examples
- Status Codes

---

## المرحلة السابعة — End-to-End Testing

اختبار النظام كاملاً.

يشمل السيناريوهات التالية:

### Organization

- Create Tenant
- Create Company
- Create Branch
- Create Department
- Create Device

### Menu

- Create Category
- Create Product
- Create Modifier
- Create Combo Meal

### Inventory

- Create Warehouse
- Create Inventory Item
- Create Supplier
- Create Purchase Order
- Goods Receipt
- Stock Transfer
- Stock Adjustment
- Recipe

### Authentication

- Login
- Refresh Token
- Logout
- Change Password
- Switch Tenant

---

## المرحلة الثامنة — Performance Review

تحسين الأداء.

يشمل:

- AsNoTracking
- Split Queries
- Compiled Queries
- Pagination
- Caching
- إزالة N+1 Queries
- تحسين Includes
- مراجعة LINQ

---

## المرحلة التاسعة — Security Review

مراجعة الأمان.

يشمل:

- JWT Validation
- Permission Validation
- SQL Injection Protection
- XSS Protection
- CSRF Review
- Rate Limiting
- Security Headers
- HTTPS Enforcement
- File Upload Validation

---

## المرحلة العاشرة — Logging & Monitoring

التأكد من عمل:

- Audit Logging
- Business Logging
- Performance Logging
- Security Logging
- CorrelationId
- Health Checks

---

## المرحلة الحادية عشرة — Build Verification

تنفيذ:

```
dotnet clean
dotnet restore
dotnet build
```

والتأكد من:

- Zero Errors
- Zero Warnings

---

## المرحلة الثانية عشرة — Production Readiness

تجهيز النظام للإنتاج.

يشمل:

- appsettings.Development
- appsettings.Staging
- appsettings.Production
- Environment Variables
- Docker Support
- Docker Compose
- Backup Strategy
- Restore Strategy
- Secrets Management
- CI/CD Preparation

---

# Additional Requirements

أثناء تنفيذ هذه المرحلة يجب الالتزام بما يلي:

- يمنع وجود أي Business Logic داخل Controllers.
- يمنع استخدام النصوص الثابتة (Hardcoded Strings).
- جميع الرسائل تستخدم Localization Service.
- جميع العمليات تعتمد على Result Pattern.
- جميع العمليات الحساسة تسجل في Audit Log.
- جميع الاستعلامات تدعم Pagination وFiltering وSorting عند الحاجة.
- الالتزام الكامل بمبادئ Clean Architecture وSOLID وCQRS وDDD.
- الحفاظ على دعم Multi-Tenancy في جميع العمليات.
- التأكد من أن جميع الخدمات قابلة للاختبار (Testable) عبر Dependency Injection.

---

# Verification Plan

## Automated Verification

تنفيذ:

```bash
dotnet clean
dotnet restore
dotnet build

dotnet ef migrations add InitialCreate

dotnet ef database update

dotnet run --project GastroErp.Presentation
```

---

## Manual Verification

التحقق من:

- إنشاء Tenant جديد.
- إنشاء Company.
- إنشاء Branch.
- إنشاء Warehouse.
- إنشاء Inventory Item.
- إنشاء Purchase Order.
- استلام البضاعة.
- إنشاء Recipe.
- تنفيذ عمليات نقل وتسوية المخزون.
- تسجيل الدخول باستخدام JWT.
- اختبار جميع Endpoints عبر Swagger.
- التأكد من تطبيق الصلاحيات.
- التأكد من عمل الترجمة العربية والإنجليزية.
- مراجعة Audit Logs وسجل الحركات.
- التأكد من نجاح جميع السيناريوهات الأساسية دون أخطاء.

---

# Expected Outcome

بنهاية هذه المرحلة سيكون نظام GastroERP:

- مترابطاً بالكامل بين جميع الطبقات.
- يعمل فعلياً مع قاعدة البيانات.
- جاهزاً لإضافة الوحدات الجديدة مثل POS، Sales، Finance، HR، CRM دون الحاجة لإعادة هيكلة.
- مهيأً للاختبارات، والتشغيل، والنشر في بيئات التطوير والإنتاج.