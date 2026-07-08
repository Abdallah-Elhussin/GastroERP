# 10-APPLICATION_LAYER_IMPLEMENTATION.md

# بناء طبقة Application - GastroERP

## الهدف

بعد الانتهاء من تصميم الـ Domain والـ Persistence، يبدأ العمل على طبقة الـ Application.

هذه الطبقة تمثل حالات الاستخدام (Use Cases) للنظام، وهي المسؤولة عن تنفيذ منطق التطبيق دون معرفة تفاصيل قاعدة البيانات أو واجهات المستخدم.

لن يتم إنشاء أي Controllers أو APIs قبل اكتمال هذه الطبقة.

---

# المبادئ المعتمدة

- Clean Architecture
- Domain Driven Design (DDD)
- CQRS
- MediatR
- FluentValidation
- AutoMapper
- Result Pattern
- Localization
- Dependency Injection
- SOLID Principles

---

# هيكل المشروع

RestaurantErp.Application

```
Application
│
├── Common
│   ├── Behaviors
│   ├── Exceptions
│   ├── Interfaces
│   ├── Localization
│   ├── Mapping
│   ├── Models
│   ├── Notifications
│   ├── Pagination
│   ├── Responses
│   ├── Security
│   ├── Services
│   └── Specifications
│
├── Features
│
│   ├── Organization
│   ├── Menu
│   ├── Inventory
│
└── DependencyInjection.cs
```

---

# داخل كل Feature

مثال Organization

```
Organization

Commands

Queries

DTOs

Validators

Mappings

Events

Notifications
```

ويطبق نفس الهيكل على Menu و Inventory.

---

# Result Pattern

إنشاء Result موحد للنظام.

```
Result

Result<T>

PagedResult<T>

ValidationResult

ErrorResult
```

جميع العمليات يجب أن تعيد Result.

ولا يتم Throw Exception إلا في حالات Domain فقط.

---

# Commands

لكل Aggregate Root يتم إنشاء Commands كاملة.

مثال

Tenant

Company

Branch

Department

AppUser

Device

Subscription

Menu

Product

Category

InventoryItem

Warehouse

Supplier

Recipe

PurchaseOrder

StockTransfer

StockCount

InventoryAdjustment

....

لكل Aggregate

Create

Update

Delete

Activate

Deactivate

Restore

Archive

حسب الحاجة.

---

# Queries

إنشاء Queries كاملة.

مثال

GetById

GetAll

GetPaged

Search

Filter

Lookup

Details

Summary

Statistics

Dashboard

حسب الحاجة.

---

# DTOs

إنشاء DTOs منفصلة.

CreateDto

UpdateDto

DetailsDto

ListDto

LookupDto

SummaryDto

PagedDto

ولا يتم إرجاع Entities مباشرة.

---

# Mapping

استخدام AutoMapper.

إنشاء Mapping Profiles لكل Feature.

OrganizationProfile

MenuProfile

InventoryProfile

...

---

# Validation

جميع Commands يتم التحقق منها بواسطة FluentValidation.

عدم كتابة Validation داخل Controllers.

جميع الرسائل تعتمد على Localization.

مثال

Validation.NameRequired

Validation.InvalidEmail

Validation.QuantityMustBePositive

Validation.PriceMustBePositive

...

---

# Behaviors

إنشاء Pipeline Behaviors

ValidationBehavior

LoggingBehavior

PerformanceBehavior

TransactionBehavior

AuthorizationBehavior

LocalizationBehavior

CachingBehavior (لاحقاً)

---

# Interfaces

إنشاء Interfaces اللازمة.

IApplicationDbContext

ICurrentUser

ITenantProvider

IDateTime

IEmailSender

ISmsSender

IPdfService

IQRCodeService

IBarcodeService

IBackgroundJob

ICacheService

IFileStorage

INotificationService

ILocalizationService

---

# Notifications

استخدام MediatR Notifications.

عند إنشاء Company

إرسال Notification.

عند إنشاء PurchaseOrder

إرسال Notification.

عند انتهاء الاشتراك

إرسال Notification.

عند انخفاض المخزون

إرسال Notification.

عند اكتمال الجرد

إرسال Notification.

---

# Authorization

إنشاء Authorization Policies.

Permissions

Roles

Claims

Branch Access

Tenant Access

حسب النظام الحالي.

---

# Pagination

إنشاء Pagination موحدة.

PageNumber

PageSize

Sorting

Filtering

Search

Dynamic Filters

---

# Search

دعم

Contains

StartsWith

Exact

Multiple Columns

Dynamic Search

---

# Specifications

إنشاء Specification Pattern.

حتى لا تتكرر شروط الاستعلام.

---

# Localization

جميع Validation

جميع Notifications

جميع Success Messages

جميع Exceptions

تعتمد على

ErrorCodes

MessageCodes

ar.json

en.json

ولا يسمح بوجود نص ثابت داخل Application.

---

# Logging

تسجيل جميع العمليات المهمة.

Create

Update

Delete

Login

Logout

Stock Movement

POS Operations

Purchasing

---

# Auditing

تمرير بيانات المستخدم الحالي.

Current User

Current Tenant

Current Branch

IP Address

Device

DateTime

---

# Dependency Injection

إنشاء DependencyInjection.cs

وتسجيل جميع الخدمات.

---

# المطلوب

1- إنشاء الهيكل الكامل لطبقة Application.

2- إنشاء Common Infrastructure.

3- إنشاء Result Pattern.

4- إنشاء Behaviors.

5- إنشاء Interfaces.

6- إنشاء Pagination.

7- إنشاء Mapping.

8- إنشاء Validation.

9- إنشاء الهيكل الكامل لكل Feature.

10- عدم إنشاء Controllers أو APIs في هذه المرحلة.

11- الالتزام الكامل بـ DDD و Clean Architecture.

12- عدم تعديل أي كود موجود في Domain أو Persistence إلا عند الضرورة وبعد توثيق السبب.