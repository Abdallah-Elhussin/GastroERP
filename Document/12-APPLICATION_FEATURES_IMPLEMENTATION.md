خطة تنفيذ طبقة التطبيق (Application Features Implementation)

تهدف هذه الخطة إلى تنفيذ جميع حالات الاستخدام (Use Cases) الخاصة بالنظام داخل طبقة GastroErp.Application باستخدام نمط CQRS و MediatR، وتحويل الكيانات التي تم بناؤها في طبقة الـ Domain إلى عمليات أعمال حقيقية مع الالتزام الكامل بـ Clean Architecture و DDD.

User Review Required

[!IMPORTANT]
سيتم تنفيذ جميع الوحدات بنفس النمط لضمان توحيد بنية المشروع وسهولة صيانته وتطويره مستقبلاً.

لن يتم وضع أي Business Logic داخل الـ API أو Controllers، وستكون جميع العمليات داخل طبقة Application فقط.

سيتم الالتزام بجميع الوثائق والقرارات المعمارية (ADR) والتعليمات السابقة الخاصة بالمشروع.

Proposed Changes
1. تنفيذ جميع الوحدات (Modules)

سيتم تنفيذ جميع الوحدات بالترتيب التالي:

Organization
Menu
Inventory
Purchasing
POS
Kitchen (KDS)
CRM
Finance
HR
Reports
Administration
2. هيكلة كل وحدة (Feature Structure)

لكل وحدة يتم إنشاء الهيكل التالي:

Commands
Queries
DTOs
Validators
Mapping
Authorization
Events (عند الحاجة)
3. تنفيذ Commands

لكل عملية إنشاء أو تعديل أو حذف أو تفعيل أو إلغاء تفعيل سيتم إنشاء:

Request
Handler
FluentValidation Validator
AutoMapper Mapping
Authorization Check
Logging
Result Pattern
Localization Support

جميع أوامر الكتابة يجب أن تعتمد على MediatR ولا يسمح باستدعاء قاعدة البيانات أو الخدمات مباشرة من الـ API.

4. تنفيذ Queries

جميع الاستعلامات يجب أن تدعم:

Pagination
Searching
Filtering
Sorting
Projection إلى DTOs
CancellationToken

ولا يسمح بإرجاع الكيانات (Entities) مباشرة.

5. DTOs

سيتم إنشاء DTOs مستقلة للقراءة والكتابة.

يمنع إعادة أي Entity مباشرة إلى طبقة الـ API.

6. Validation

جميع عمليات التحقق تتم باستخدام FluentValidation فقط.

لا يسمح بكتابة Validation داخل Controllers.

جميع الرسائل تعتمد على:

ErrorCodes
MessageCodes
LocalizationService
7. AutoMapper

إنشاء Mapping Profiles مستقلة لكل وحدة.

يتم تحويل جميع Entities إلى DTOs والعكس باستخدام AutoMapper.

8. Authorization

تطبيق نظام الصلاحيات (RBAC) على جميع العمليات.

كل Command أو Query يحتاج لصلاحية مستقلة مرتبطة بـ Permission Code.

9. Logging

تسجيل جميع العمليات المهمة مع حفظ:

UserId
TenantId
BranchId
CorrelationId
ExecutionTime
10. Multi-Tenancy

جميع العمليات يجب أن تعتمد على:

TenantProvider
CurrentUser
Query Filters

لضمان عدم وصول أي مستأجر إلى بيانات مستأجر آخر.

11. Localization

دعم كامل للعربية والإنجليزية.

عدم كتابة أي رسائل ثابتة داخل الكود.

جميع الرسائل تعتمد على ملفات الترجمة وLocalizationService.

12. Result Pattern

جميع العمليات تعيد:

Result
Result<T>
PagedResult<T>

ولا يتم استخدام Exceptions إلا في حالات أخطاء الـ Domain غير المتوقعة.

13. Clean Architecture

الالتزام الكامل بالمبادئ التالية:

Clean Architecture
SOLID
CQRS
MediatR
DDD
Dependency Injection
Separation of Concerns
Single Responsibility Principle
Verification Plan
Automated Tests
التأكد من نجاح dotnet build بعد كل وحدة يتم تنفيذها.
التأكد من نجاح جميع Validators.
مراجعة جميع AutoMapper Profiles.
التأكد من عدم وجود أخطاء Compilation.
Manual Verification
مراجعة جميع Commands وQueries والتأكد من تطبيق CQRS بالشكل الصحيح.
التأكد من عدم وجود Business Logic داخل Controllers.
مراجعة دعم Multi-Tenancy في جميع العمليات.
مراجعة دعم Localization لجميع الرسائل.
مراجعة تسجيل العمليات (Logging).
التأكد من استخدام Result Pattern في جميع العمليات.
مراجعة هيكل المشروع والتأكد من الالتزام بجميع الوثائق والقرارات المعمارية السابقة وعدم الخروج عن المعايير المعتمدة للمشروع.