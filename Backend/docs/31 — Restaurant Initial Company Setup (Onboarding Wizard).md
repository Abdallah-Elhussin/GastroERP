# Phase 31 — Restaurant Initial Company Setup (Onboarding Wizard)

## الهدف

إنشاء **Restaurant Initial Setup Wizard** ليكون نقطة الدخول الوحيدة لإنشاء أي عميل جديد داخل GastroERP.

> **ملاحظة**
>
> GastroERP في هذا الإصدار مخصص **للمطاعم فقط**.
>
> لذلك **لا يتم سؤال المستخدم عن نوع النشاط**.
>
> يقوم النظام تلقائياً باعتبار النشاط:
>
> ```
> BusinessType = Restaurant
> ```
>
> ويتم استخدام جميع بيانات الـ Seed الخاصة بالمطاعم.

---

# أهداف المرحلة

بعد الضغط على **Create Company** يجب أن يصبح العميل قادراً على استخدام النظام مباشرة دون أي إعدادات إضافية.

يقوم النظام تلقائياً بإنشاء:

- Tenant
- Subscription
- Company
- Main Branch
- Main Warehouse
- Administrator
- Administrator Role
- Master Data
- Chart of Accounts
- Units
- VAT
- Fiscal Period
- Default Workflows
- Default Dashboards
- Reporting Templates
- Notifications

---

# خطوات الـ Wizard

---

# الخطوة الأولى
## إنشاء حساب المدير

### الحقول

| الحقل | مطلوب |
|--------|--------|
| Full Name | ✅ |
| Email | ✅ |
| Mobile | ✅ |
| Password | ✅ |
| Confirm Password | ✅ |

### Validation

- Email Unique
- Password Policy
- Mobile Optional حسب الدولة

---

# الخطوة الثانية
## بيانات الشركة

### البيانات الأساسية

| الحقل | مطلوب |
|--------|--------|
| الاسم العربي | ✅ |
| الاسم الإنجليزي | ✅ |
| الاسم التجاري | ✅ |
| شعار الشركة | اختياري |

---

## البيانات القانونية

| الحقل | مطلوب |
|--------|--------|
| السجل التجاري | ✅ |
| الرقم الضريبي | ✅ |
| شهادة الضريبة | اختياري |

---

## بيانات الاتصال

| الحقل | مطلوب |
|--------|--------|
| الهاتف | ✅ |
| البريد الإلكتروني | ✅ |
| الموقع الإلكتروني | اختياري |

---

## العنوان

| الحقل | مطلوب |
|--------|--------|
| الدولة | ✅ |
| المدينة | ✅ |
| المنطقة | اختياري |
| الحي | اختياري |
| الشارع | اختياري |
| الرمز البريدي | اختياري |
| الموقع على الخريطة | اختياري |

---

# الخطوة الثالثة
## الإعدادات العامة

### اللغة

- العربية
- الإنجليزية

---

### العملة الأساسية

مثال:

- SAR
- USD
- AED
- KWD
- QAR
- OMR
- BHD
- EGP
- SDG

---

### دعم العملات المتعددة

Checkbox

إذا كانت مفعلة

يختار المستخدم العملات الإضافية.

---

### المنطقة الزمنية

مثال

```
Asia/Riyadh
```

---

### بداية السنة المالية

مثال

```
January
```

---

### نوع التقويم

- Gregorian
- Hijri

---

# الخطوة الرابعة
## إنشاء الفرع الرئيسي

### الحقول

| الحقل | مطلوب |
|--------|--------|
| اسم الفرع | ✅ |
| الهاتف | ✅ |
| البريد الإلكتروني | اختياري |
| العنوان | ✅ |

---

# لا توجد خطوات إضافية

بمجرد الضغط على:

```
Create Company
```

يقوم النظام بتنفيذ جميع العمليات التالية تلقائياً.

---

# إنشاء Tenant

إنشاء مستأجر جديد.

---

# إنشاء Subscription

إنشاء اشتراك Trial.

---

# إنشاء Company

إنشاء الشركة.

---

# إنشاء Main Branch

إنشاء الفرع الرئيسي.

---

# إنشاء Main Warehouse

بشكل تلقائي.

```
Main Warehouse
```

---

# إنشاء المستخدم

Administrator

---

# إنشاء Role

Administrator

---

# ربط المستخدم بالدور

UserRoles

---

# إنشاء الفترة المالية

Fiscal Period

للسنة الحالية.

---

# إنشاء العملات

يقوم النظام بإضافة:

- العملة الأساسية
- العملات الإضافية المختارة

---

# إنشاء الضريبة

حسب الدولة.

مثال

السعودية

```
VAT 15%
```

---

# إنشاء الشجرة المحاسبية

لا يتم سؤال المستخدم.

يقوم النظام تلقائياً باستخدام:

```
Restaurant Default Chart Of Accounts
```

ويتم إنشاء:

- Account Classes
- Chart Of Accounts
- Default Accounts
- System Accounts

---

# إنشاء وحدات القياس

يتم زرع:

### الوزن

- kg
- g

### الحجم

- L
- ml

### العدد

- pc
- box
- tray
- portion
- bottle
- cup

---

# إنشاء المستودعات

افتراضياً

- Main Warehouse
- Kitchen Warehouse
- Dry Storage
- Cold Storage
- Freezer

---

# إنشاء أنواع الدفع

- Cash
- Visa
- MasterCard
- Mada
- Apple Pay
- STC Pay
- Bank Transfer

---

# إنشاء مستويات الأسعار

- Dine In
- Takeaway
- Delivery

---

# إنشاء تصنيفات المخزون

مثل

- Meat
- Chicken
- Seafood
- Vegetables
- Dairy
- Beverages
- Packaging
- Cleaning

---

# إنشاء أسباب الجرد

- Damage
- Waste
- Expired
- Adjustment

---

# إنشاء Workflow

يقوم النظام بزرع جميع Workflows الافتراضية.

مثل

- Leave Approval
- Purchase Approval
- Payroll Approval
- Refund Approval
- Discount Approval

---

# إنشاء Roles

بشكل تلقائي

- Administrator
- Branch Manager
- Cashier
- Waiter
- Kitchen
- Inventory
- Accountant
- HR

---

# إنشاء Permissions

ربط جميع الصلاحيات بالأدوار الافتراضية.

---

# إنشاء Dashboards

إنشاء Dashboards الافتراضية.

مثل

- Executive Dashboard
- Sales Dashboard
- Inventory Dashboard
- Finance Dashboard
- HR Dashboard

---

# إنشاء Reporting Templates

زرع التقارير الافتراضية.

---

# إنشاء Notification Templates

زرع قوالب الإشعارات.

---

# إنشاء Master Data

زرع جميع البيانات المشتركة للنظام.

مثل

- Currencies
- Units
- Payment Types
- Invoice Types
- Document Types
- Price Levels
- Adjustment Reasons
- Inventory Categories
- Tax Types

---

# شاشة النجاح

بعد اكتمال جميع العمليات

يعرض النظام:

✅ Company Created

✅ Administrator Created

✅ Main Branch Created

✅ Warehouse Created

✅ Chart Of Accounts Installed

✅ Units Installed

✅ VAT Installed

✅ Roles Installed

✅ Workflows Installed

✅ Dashboards Installed

✅ Trial Subscription Activated

ثم يظهر زر

```
Go To Dashboard
```

---

# متطلبات التنفيذ

- استخدام Transaction واحدة تغطي جميع عمليات الإنشاء.
- جميع عمليات الـ Seed يجب أن تكون Idempotent (لا تُكرر البيانات عند إعادة التنفيذ).
- إنشاء جميع البيانات من خلال Services وDomain Logic فقط، دون استخدام SQL مباشر.
- فصل بيانات الـ Master Data حسب الدولة والقوالب (Templates) لتسهيل التوسع مستقبلاً.
- الالتزام الكامل ببنية Clean Architecture وDDD وCQRS المستخدمة في GastroERP.
- يجب أن تكون جميع البيانات مرتبطة بالـ Tenant الجديد لضمان العزل الكامل بين المستأجرين.