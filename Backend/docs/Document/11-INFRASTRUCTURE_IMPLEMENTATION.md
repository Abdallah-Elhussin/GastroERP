# 11-INFRASTRUCTURE_IMPLEMENTATION.md

# تنفيذ طبقة البنية التحتية (Infrastructure Layer)

## الهدف

بعد اكتمال Domain و Persistence و Application، تبدأ هذه المرحلة بتنفيذ جميع الخدمات الفعلية (Concrete Implementations) التي تعتمد عليها طبقة Application.

لن يتم إنشاء أي Controllers أو Endpoints قبل اكتمال هذه الطبقة.

---

# المبادئ

- Clean Architecture
- Dependency Injection
- SOLID
- Single Responsibility
- Configuration Based
- Testable Services

---

# هيكل المشروع

GastroErp.Infrastructure

```

Infrastructure

│

├── Authentication

├── Authorization

├── BackgroundJobs

├── Barcode

├── Cache

├── CurrentUser

├── DateTime

├── Email

├── FileStorage

├── Localization

├── Logging

├── Notifications

├── Pdf

├── Printing

├── QrCode

├── Services

├── Sms

├── Sync

├── Tenant

├── TimeZone

├── Security

├── Persistence

└── DependencyInjection.cs

```

---

# Current User

تنفيذ

ICurrentUser

يجب أن يوفر

- UserId
- TenantId
- BranchId
- DeviceId
- UserName
- Roles
- Permissions
- Language
- TimeZone

---

# Tenant Provider

تنفيذ

ITenantProvider

يقوم بتحديد الـ Tenant الحالي اعتماداً على

- JWT
- SubDomain
- Header
- API Key

ويجب أن يكون قابلاً للتوسعة.

---

# Date Time

تنفيذ

IDateTime

حتى لا يتم استخدام DateTime.UtcNow مباشرة داخل النظام.

---

# Localization

تنفيذ

ILocalizationService

يعتمد على

ar.json

en.json

ويقرأ جميع

ErrorCodes

MessageCodes

Validation Messages

Notification Messages

---

# File Storage

تنفيذ

IFileStorage

ويكون قابلاً للتبديل لاحقاً بين

Local Storage

Azure Blob

AWS S3

MinIO

---

# QR Code

تنفيذ

IQRCodeService

لدعم

- ZATCA
- Menu QR
- Order QR
- Payment QR

---

# Barcode

تنفيذ

IBarcodeService

لدعم

EAN13

Code128

QR

---

# Email

تنفيذ

IEmailSender

ويكون قابلاً لاستخدام

SMTP

أو أي مزود خارجي مستقبلاً.

---

# SMS

تنفيذ

ISmsSender

دون ربطه حالياً بمزود معين.

---

# PDF

تنفيذ

IPdfService

لدعم

Invoice

Receipt

Kitchen Ticket

Reports

---

# Printing

إنشاء

IPrinterService

لدعم

POS Printer

Kitchen Printer

Label Printer

Barcode Printer

---

# Notification Service

تنفيذ

INotificationService

لدعم

Email

SMS

Push

In-App

ويكون قابلاً للتوسعة.

---

# Cache

تنفيذ

ICacheService

والدعم الحالي

Memory Cache

مع إمكانية إضافة Redis لاحقاً.

---

# Background Jobs

تنفيذ

IBackgroundJobService

مع ترك التصميم قابلاً لاستخدام

Hangfire

أو Quartz

في المستقبل.

---

# Logging

إعداد Serilog.

تسجيل

Exceptions

Performance

Audit

Security

Business Events

---

# Security

تنفيذ

Password Hashing

Encryption

Random Generator

Token Generator

---

# Sync

إنشاء الهيكل الأساسي فقط لخدمة المزامنة.

يتضمن

SyncService

SyncQueue

ConflictResolver Interface

بدون تنفيذ كامل حالياً.

---

# Configuration

إعداد

Options Pattern

لكل خدمة.

مثال

EmailOptions

StorageOptions

LocalizationOptions

CacheOptions

SecurityOptions

---

# Dependency Injection

تسجيل جميع الخدمات داخل

DependencyInjection.cs

---

# المطلوب

1. تنفيذ جميع Interfaces الموجودة في Application.

2. عدم إضافة أي Business Logic داخل Infrastructure.

3. عدم إنشاء Controllers.

4. عدم إنشاء Use Cases.

5. الالتزام الكامل بـ Dependency Injection.

6. جميع الخدمات يجب أن تكون قابلة للاستبدال (Replaceable).

7. إعداد Logging و Localization بصورة احترافية.

8. تجهيز النظام لدعم SQL Server و SQLite دون تغيير في طبقة Application.

9. تجهيز خدمة الطباعة و QR و Barcode حتى وإن كانت بتنفيذ مبدئي.

10. توثيق أي قرار معماري جديد في ملف ADR إذا لزم الأمر.
