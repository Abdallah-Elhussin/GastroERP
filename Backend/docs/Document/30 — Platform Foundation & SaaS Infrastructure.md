# Phase 30 — Platform Foundation & SaaS Infrastructure

## الهدف

تجهيز منصة GastroERP لتكون SaaS احترافية وجاهزة للإنتاج، من خلال إعادة تنظيم البنية الأساسية (Platform Foundation)، ودعم قواعد البيانات متعددة البيئات، وإنشاء نظام Onboarding متكامل، وتطبيق RBAC كامل، بحيث تصبح الواجهة الأمامية قادرة على الاعتماد على واجهات API مستقرة وواضحة.

> **ملاحظة:** لا يتم بناء الواجهة الأمامية في هذه المرحلة. التركيز بالكامل على Backend وتجهيز المنصة.

---

# النطاق

تشمل هذه المرحلة:

- Environment Configuration
- Database Provider Abstraction
- Tenant Onboarding
- Company Registration
- RBAC Management
- Authentication Improvements
- Tenant Resolution
- Default Seed Improvements
- API Hardening
- Documentation
- Tests

---

# 1. Environment Configuration

## الهدف

عدم ربط النظام بقاعدة بيانات محلية أو مزود واحد.

---

### المطلوب

إعادة تنظيم ملفات الإعدادات:

```
appsettings.json

appsettings.Development.json

appsettings.Testing.json

appsettings.Staging.json

appsettings.Production.json
```

---

### دعم البيئات

Development

- SQL Server LocalDB
- SQL Server Express
- Docker SQL Server

Testing

- SQL Server
- PostgreSQL

Staging

- Azure SQL
- PostgreSQL

Production

- Azure SQL
- AWS RDS
- PostgreSQL

---

### Connection String Provider

إنشاء خدمة:

```
IConnectionStringResolver
```

تقوم باختيار Connection String حسب:

- Environment
- Tenant
- Database Provider

---

# 2. Database Provider Abstraction

عدم ربط المشروع بـ SQL Server فقط.

إنشاء طبقة تدعم مستقبلاً:

- SQL Server
- PostgreSQL
- Azure SQL

بدون تعديل Domain أو Application.

---

# 3. Tenant Registration (Onboarding)

إنشاء عملية تسجيل شركة جديدة بالكامل.

بدلاً من إنشاء Tenant يدوياً.

---

## Endpoint

```
POST

/api/v1/onboarding/register-company
```

---

## يقوم تلقائياً بـ

- إنشاء Tenant
- إنشاء Company
- إنشاء Branch
- إنشاء Warehouse
- إنشاء Administrator User
- إنشاء Administrator Role
- ربط المستخدم بالدور
- تشغيل Master Data Seed
- إنشاء Subscription
- إنشاء Trial License
- تسجيل Audit Log
- إصدار JWT

---

## Request

```json
{
  "companyName":"Restaurant One",
  "ownerName":"Ahmed",
  "email":"admin@restaurant.com",
  "password":"123456",
  "phone":"0500000000",
  "country":"SA",
  "subscription":"Trial"
}
```

---

## Response

```json
{
    "tenantId":"",
    "companyId":"",
    "userId":"",
    "token":"",
    "refreshToken":""
}
```

---

# 4. Tenant Resolution

إعادة تصميم طريقة معرفة الـ Tenant.

يدعم:

- JWT Claim
- Subdomain
- Header
- API Key
- مستقبلًا Domain Mapping

---

إنشاء:

```
ITenantResolver
```

---

مع Middleware مستقل.

---

# 5. Authentication Improvements

مراجعة كاملة لنظام Authentication.

يشمل:

- JWT
- Refresh Token
- Token Rotation
- Revoke Token
- Logout
- Multiple Sessions
- Session Tracking
- Device Tracking

---

# 6. RBAC

تطبيق نظام RBAC كامل.

---

## Entities

Permission

Role

RolePermission

UserRole

PermissionGroup

PermissionCategory

---

## Permissions

تنظيم الصلاحيات:

```
Organization.*

Identity.*

Inventory.*

Sales.*

Kitchen.*

Finance.*

Workflow.*

HR.*

Reporting.*

Settings.*

Administration.*
```

---

## APIs

Roles

Permissions

Assign Role

Assign Permission

Remove Permission

Permission Matrix

User Permissions

Role Permissions

---

# 7. Authorization

كل Endpoint يجب أن يحتوي على:

Policy

Permission

Swagger Description

Authorization Attribute

---

عدم السماح لأي Endpoint بدون حماية.

---

# 8. Seed Improvements

تحديث Master Seed.

يشمل:

إنشاء تلقائي:

Administrator Role

Manager Role

Cashier Role

Accountant Role

HR Manager

Kitchen Manager

Inventory Manager

Viewer

---

إضافة جميع Permissions.

---

إنشاء Admin User.

---

ربط المستخدم بالدور.

---

# 9. API Hardening

مراجعة جميع Controllers.

التأكد من:

Validation

Authorization

Permissions

Problem Details

HTTP Status Codes

Pagination

Filtering

Sorting

Versioning

---

# 10. Health Checks

إضافة:

Authentication Health

Authorization Health

Tenant Resolution Health

RBAC Health

Database Provider Health

---

# 11. Background Jobs

إضافة:

Expired Session Cleanup

Refresh Token Cleanup

Inactive Tenant Check

Trial Expiry Reminder

Subscription Expiry Reminder

---

# 12. Audit

تسجيل:

Login

Logout

Register Company

Assign Role

Assign Permission

Create Tenant

Create Company

Refresh Token

---

# 13. Tests

إضافة:

Unit Tests

Integration Tests

Authorization Tests

RBAC Tests

Tenant Tests

Authentication Tests

Onboarding Tests

---

# 14. Documentation

تحديث:

README

Documentation Index

Architecture

Authentication

Authorization

RBAC

Tenant Management

Onboarding

Environment Configuration

Deployment Guide

---

# 15. Build

يجب أن تكون النتيجة النهائية:

```
0 Errors

0 Warnings
```

---

# معايير القبول

تعتبر المرحلة مكتملة عند تحقق جميع النقاط التالية:

- دعم البيئات المختلفة (Development / Testing / Staging / Production).
- إمكانية تغيير قاعدة البيانات دون تعديل منطق الأعمال.
- إنشاء شركة جديدة يؤدي تلقائياً إلى إنشاء Tenant والبيانات الأساسية.
- دعم تسجيل الدخول وإدارة الجلسات وRefresh Tokens.
- تطبيق RBAC كامل على جميع واجهات API.
- إنشاء الأدوار والصلاحيات تلقائياً عند Seed.
- حماية جميع الـ Endpoints بالسياسات والصلاحيات المناسبة.
- نجاح جميع Health Checks.
- نجاح جميع Unit وIntegration Tests.
- تحديث التوثيق بالكامل.
- نجاح Build بدون أي أخطاء أو تحذيرات.

---

# ملاحظات هندسية

- يمنع كسر مبادئ Clean Architecture أو DDD.
- يمنع وضع منطق الأعمال داخل Controllers أو Middleware.
- جميع العمليات يجب أن تستخدم CQRS وMediatR وFluentValidation.
- جميع الخدمات يجب تسجيلها في Dependency Injection.
- جميع الكيانات الداعمة للمستأجرين يجب أن تطبق العزل باستخدام `TenantId`.
- لا يتم تطبيق Migrations تلقائياً، وإنما يتم إنشاؤها فقط وفق سياسة المشروع.
- يجب الحفاظ على التوافق مع جميع المراحل السابقة (HR، Workflow، Reporting، Finance، وغيرها).

---

# النتيجة المتوقعة

بعد إكمال هذه المرحلة يصبح GastroERP منصة SaaS احترافية ومهيأة للإنتاج، مع بنية Backend مستقرة وآمنة، ودعم كامل لتعدد المستأجرين (Multi-Tenancy)، وإدارة الشركات، والأدوار والصلاحيات، والبيئات المختلفة، لتكون جاهزة لبناء واجهة Angular واختبار جميع الوحدات بكفاءة.