# GastroERP - وحدة الهوية والتنظيم
# Organization Module - التوثيق الكامل

الإصدار: 1.0
الحالة: قيد التنفيذ

---

# الكيانات المشمولة في هذه الوحدة

1. Tenant (المستأجر)
2. SubscriptionPlan (باقة الاشتراك)
3. Subscription (الاشتراك)
4. Company (الشركة)
5. Branch (الفرع)
6. Department (القسم)
7. AppUser (المستخدم)
8. Role (الدور)
9. Permission (الصلاحية)
10. RolePermission (ربط الدور بالصلاحية)
11. UserRole (ربط المستخدم بالدور)
12. UserBranch (ربط المستخدم بالفرع)
13. Currency (العملة)
14. Language (اللغة)
15. Timezone (المنطقة الزمنية)
16. BusinessHour (ساعات العمل)
17. Holiday (العطل الرسمية)
18. Device (الجهاز)
19. BranchDevice (ربط الجهاز بالفرع)
20. WorkingShift (وردية العمل)
21. EmployeePosition (المسمى الوظيفي)

---

# 1. كيان Tenant (المستأجر)

## تحليل الأعمال

المستأجر هو الوحدة الجذرية في نظام SaaS متعدد المستأجرين.
يمثل شركة مطاعم كاملة (مثل: شركة مطاعم الريان).
كل بيانات النظام مرتبطة بالمستأجر لضمان العزل الكامل.

## قواعد العمل

- لا يمكن إنشاء مستأجر بدون اسم وـ Slug فريد
- الـ Slug غير قابل للتغيير بعد الإنشاء
- تعليق المستأجر يوقف وصول جميع مستخدميه
- كل مستأجر له اشتراك واحد نشط في أي وقت
- حذف المستأجر حذف منطقي فقط (Soft Delete)

## Aggregate

- Tenant هو Aggregate Root
- يحتوي داخله على Subscription الحالي (Active Subscription)

## Value Objects

- TenantBranding (LogoUrl, PrimaryColor, SecondaryColor)

## Domain Events

- TenantCreatedEvent
- TenantSuspendedEvent
- TenantActivatedEvent
- TenantDeletedEvent

## جدول SQL Server

```sql
CREATE TABLE Tenants (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    Name                NVARCHAR(200)       NOT NULL,
    Slug                NVARCHAR(100)       NOT NULL,
    Status              TINYINT             NOT NULL DEFAULT 1,
    DatabaseName        NVARCHAR(200)       NULL,
    LogoUrl             NVARCHAR(500)       NULL,
    PrimaryColor        NVARCHAR(7)         NULL,
    SecondaryColor      NVARCHAR(7)         NULL,
    DefaultCurrency     NVARCHAR(3)         NOT NULL DEFAULT 'SAR',
    DefaultLanguage     NVARCHAR(10)        NOT NULL DEFAULT 'ar',
    DefaultTimezone     NVARCHAR(100)       NOT NULL DEFAULT 'Arab Standard Time',
    IsDeleted           BIT                 NOT NULL DEFAULT 0,
    DeletedAt           DATETIMEOFFSET      NULL,
    DeletedBy           NVARCHAR(200)       NULL,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    UpdatedAt           DATETIMEOFFSET      NULL,
    UpdatedBy           NVARCHAR(200)       NULL,
    RowVersion          ROWVERSION          NOT NULL,
    CONSTRAINT PK_Tenants PRIMARY KEY (Id),
    CONSTRAINT UQ_Tenants_Slug UNIQUE (Slug)
);
CREATE INDEX IX_Tenants_Status ON Tenants(Status) WHERE IsDeleted = 0;
CREATE INDEX IX_Tenants_Slug ON Tenants(Slug);
```

## CQRS Checklist

- [ ] CreateTenantCommand + Handler
- [ ] SuspendTenantCommand + Handler
- [ ] ActivateTenantCommand + Handler
- [ ] UpdateTenantBrandingCommand + Handler
- [ ] GetTenantByIdQuery + Handler
- [ ] GetTenantBySlugQuery + Handler

---

# 2. كيان SubscriptionPlan (باقة الاشتراك)

## تحليل الأعمال

تمثل باقات الاشتراك المتاحة للمستأجرين.
كل باقة تحدد الحدود القصوى للفروع والمستخدمين والأجهزة والوحدات.

## قواعد العمل

- الباقات النظامية لا يمكن حذفها
- لا يمكن إنشاء باقة بحدود سالبة
- الباقة يمكن إيقاف البيع عليها دون حذف المشتركين الحاليين

## جدول SQL Server

```sql
CREATE TABLE SubscriptionPlans (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    Name                NVARCHAR(100)       NOT NULL,
    NameAr              NVARCHAR(100)       NOT NULL,
    Description         NVARCHAR(500)       NULL,
    PlanType            TINYINT             NOT NULL,
    MonthlyPrice        DECIMAL(18,2)       NOT NULL,
    YearlyPrice         DECIMAL(18,2)       NOT NULL,
    Currency            NVARCHAR(3)         NOT NULL DEFAULT 'SAR',
    MaxBranches         INT                 NOT NULL,
    MaxUsers            INT                 NOT NULL,
    MaxDevices          INT                 NOT NULL,
    MaxProducts         INT                 NOT NULL DEFAULT -1,
    IsActive            BIT                 NOT NULL DEFAULT 1,
    IsSystem            BIT                 NOT NULL DEFAULT 0,
    SortOrder           INT                 NOT NULL DEFAULT 0,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    UpdatedAt           DATETIMEOFFSET      NULL,
    UpdatedBy           NVARCHAR(200)       NULL,
    RowVersion          ROWVERSION          NOT NULL,
    CONSTRAINT PK_SubscriptionPlans PRIMARY KEY (Id)
);
```

## CQRS Checklist

- [ ] CreateSubscriptionPlanCommand + Handler
- [ ] UpdateSubscriptionPlanCommand + Handler
- [ ] DeactivateSubscriptionPlanCommand + Handler
- [ ] GetAllSubscriptionPlansQuery + Handler
- [ ] GetSubscriptionPlanByIdQuery + Handler

---

# 3. كيان Subscription (الاشتراك)

## تحليل الأعمال

يمثل اشتراك مستأجر محدد في باقة معينة.
كل مستأجر له اشتراك واحد نشط في أي وقت.
عند انتهاء الاشتراك، يتم إطلاق حدث لإشعار النظام.

## قواعد العمل

- تاريخ النهاية يجب أن يكون بعد تاريخ البداية
- لا يمكن أن يكون للمستأجر أكثر من اشتراك نشط واحد
- تجديد الاشتراك يُنشئ سجلاً جديداً (لا يعدل القديم)
- الاشتراك المنتهي لا يمكن تفعيله مباشرة، بل يحتاج تجديداً

## Domain Events

- SubscriptionCreatedEvent
- SubscriptionRenewedEvent
- SubscriptionExpiredEvent
- SubscriptionCancelledEvent

## جدول SQL Server

```sql
CREATE TABLE Subscriptions (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    PlanId              UNIQUEIDENTIFIER    NOT NULL,
    Status              TINYINT             NOT NULL,
    BillingCycle        TINYINT             NOT NULL,
    StartDate           DATETIMEOFFSET      NOT NULL,
    EndDate             DATETIMEOFFSET      NOT NULL,
    MaxBranches         INT                 NOT NULL,
    MaxUsers            INT                 NOT NULL,
    MaxDevices          INT                 NOT NULL,
    PriceAmount         DECIMAL(18,2)       NOT NULL,
    PriceCurrency       NVARCHAR(3)         NOT NULL DEFAULT 'SAR',
    Notes               NVARCHAR(500)       NULL,
    IsDeleted           BIT                 NOT NULL DEFAULT 0,
    DeletedAt           DATETIMEOFFSET      NULL,
    DeletedBy           NVARCHAR(200)       NULL,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    UpdatedAt           DATETIMEOFFSET      NULL,
    UpdatedBy           NVARCHAR(200)       NULL,
    RowVersion          ROWVERSION          NOT NULL,
    CONSTRAINT PK_Subscriptions PRIMARY KEY (Id),
    CONSTRAINT FK_Subscriptions_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id) ON DELETE RESTRICT,
    CONSTRAINT FK_Subscriptions_Plans FOREIGN KEY (PlanId) REFERENCES SubscriptionPlans(Id) ON DELETE RESTRICT
);
CREATE INDEX IX_Subscriptions_TenantId ON Subscriptions(TenantId);
CREATE INDEX IX_Subscriptions_Status_EndDate ON Subscriptions(Status, EndDate);
```

## CQRS Checklist

- [ ] CreateSubscriptionCommand + Handler
- [ ] RenewSubscriptionCommand + Handler
- [ ] CancelSubscriptionCommand + Handler
- [ ] GetActiveSubscriptionQuery + Handler
- [ ] GetSubscriptionHistoryQuery + Handler

---

# 4. كيان Company (الشركة)

## تحليل الأعمال

الشركة هي الكيان القانوني التابع للمستأجر.
مستأجر واحد يمكنه امتلاك عدة شركات (مثل: شركة مطاعم + شركة توصيل).
لكل شركة إعدادات ضريبية مستقلة.

## قواعد العمل

- لا يمكن إنشاء شركة بدون TenantId و Name و TaxNumber
- الرقم الضريبي فريد داخل نفس المستأجر
- لا يمكن حذف شركة لديها فروع نشطة
- لا يمكن حذف شركة لديها سجلات مالية

## Value Objects

- Address (العنوان الكامل)
- ContactInfo (ايميل + هاتف)

## Domain Events

- CompanyCreatedEvent
- CompanyDeactivatedEvent

## جدول SQL Server

```sql
CREATE TABLE Companies (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    Name                NVARCHAR(200)       NOT NULL,
    NameAr              NVARCHAR(200)       NULL,
    TaxNumber           NVARCHAR(50)        NOT NULL,
    VatNumber           NVARCHAR(50)        NULL,
    CommercialRegister  NVARCHAR(50)        NULL,
    Email               NVARCHAR(200)       NULL,
    PhoneNumber         NVARCHAR(20)        NULL,
    Website             NVARCHAR(300)       NULL,
    LogoUrl             NVARCHAR(500)       NULL,
    AddressStreet       NVARCHAR(300)       NULL,
    AddressCity         NVARCHAR(100)       NULL,
    AddressRegion       NVARCHAR(100)       NULL,
    AddressPostalCode   NVARCHAR(20)        NULL,
    AddressCountry      NVARCHAR(100)       NOT NULL DEFAULT 'Saudi Arabia',
    DefaultCurrencyId   UNIQUEIDENTIFIER    NULL,
    FiscalYearStartMonth TINYINT            NOT NULL DEFAULT 1,
    IsActive            BIT                 NOT NULL DEFAULT 1,
    IsDeleted           BIT                 NOT NULL DEFAULT 0,
    DeletedAt           DATETIMEOFFSET      NULL,
    DeletedBy           NVARCHAR(200)       NULL,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    UpdatedAt           DATETIMEOFFSET      NULL,
    UpdatedBy           NVARCHAR(200)       NULL,
    RowVersion          ROWVERSION          NOT NULL,
    CONSTRAINT PK_Companies PRIMARY KEY (Id),
    CONSTRAINT FK_Companies_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT UQ_Companies_TenantId_TaxNumber UNIQUE (TenantId, TaxNumber)
);
CREATE INDEX IX_Companies_TenantId ON Companies(TenantId) WHERE IsDeleted = 0;
```

## CQRS Checklist

- [ ] CreateCompanyCommand + Handler
- [ ] UpdateCompanyCommand + Handler
- [ ] DeactivateCompanyCommand + Handler
- [ ] GetCompaniesByTenantQuery + Handler
- [ ] GetCompanyByIdQuery + Handler

---

# 5. كيان Branch (الفرع)

## تحليل الأعمال

الفرع هو وحدة التشغيل الميدانية في النظام.
كل فرع له مستودعات، وطابعات، وأجهزة، وموظفون، ومنيو مستقل.
الفرع هو حد الـ Tenant في العمليات اليومية (Offline-First).

## قواعد العمل

- كل فرع يتبع شركة واحدة فقط
- الفرع يرث الـ TenantId من الشركة
- لا يمكن حذف فرع له طلبات مفتوحة
- إيقاف الفرع يوقف نقاط البيع الخاصة به
- إحداثيات الموقع (Latitude/Longitude) مطلوبة للتوصيل

## Value Objects

- Address (العنوان الكامل)
- GeoLocation (Latitude, Longitude)
- BusinessHours (تضم قائمة BusinessHour)

## Domain Events

- BranchCreatedEvent
- BranchDeactivatedEvent
- BranchAddressChangedEvent

## جدول SQL Server

```sql
CREATE TABLE Branches (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    CompanyId           UNIQUEIDENTIFIER    NOT NULL,
    Name                NVARCHAR(200)       NOT NULL,
    NameAr              NVARCHAR(200)       NULL,
    Code                NVARCHAR(20)        NULL,
    BranchType          TINYINT             NOT NULL,
    Status              TINYINT             NOT NULL DEFAULT 1,
    PhoneNumber         NVARCHAR(20)        NULL,
    Email               NVARCHAR(200)       NULL,
    AddressStreet       NVARCHAR(300)       NULL,
    AddressCity         NVARCHAR(100)       NULL,
    AddressRegion       NVARCHAR(100)       NULL,
    AddressPostalCode   NVARCHAR(20)        NULL,
    AddressCountry      NVARCHAR(100)       NOT NULL DEFAULT 'Saudi Arabia',
    Latitude            DECIMAL(9,6)        NULL,
    Longitude           DECIMAL(9,6)        NULL,
    TimezoneId          UNIQUEIDENTIFIER    NULL,
    CurrencyId          UNIQUEIDENTIFIER    NULL,
    LanguageId          UNIQUEIDENTIFIER    NULL,
    AllowNegativeStock  BIT                 NOT NULL DEFAULT 0,
    AllowOfflineSales   BIT                 NOT NULL DEFAULT 1,
    IsDeleted           BIT                 NOT NULL DEFAULT 0,
    DeletedAt           DATETIMEOFFSET      NULL,
    DeletedBy           NVARCHAR(200)       NULL,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    UpdatedAt           DATETIMEOFFSET      NULL,
    UpdatedBy           NVARCHAR(200)       NULL,
    RowVersion          ROWVERSION          NOT NULL,
    CONSTRAINT PK_Branches PRIMARY KEY (Id),
    CONSTRAINT FK_Branches_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT FK_Branches_Companies FOREIGN KEY (CompanyId) REFERENCES Companies(Id)
);
CREATE INDEX IX_Branches_TenantId ON Branches(TenantId) WHERE IsDeleted = 0;
CREATE INDEX IX_Branches_CompanyId ON Branches(CompanyId) WHERE IsDeleted = 0;
```

## CQRS Checklist

- [ ] CreateBranchCommand + Handler
- [ ] UpdateBranchCommand + Handler
- [ ] DeactivateBranchCommand + Handler
- [ ] GetBranchesByCompanyQuery + Handler
- [ ] GetBranchByIdQuery + Handler

---

# 6. كيان Department (القسم)

## تحليل الأعمال

القسم هو تقسيم تنظيمي داخل الفرع أو الشركة.
مثل: قسم المطبخ، قسم الخدمة، قسم الحسابات.
يستخدم في تنظيم الموظفين والصلاحيات.

## جدول SQL Server

```sql
CREATE TABLE Departments (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    CompanyId           UNIQUEIDENTIFIER    NOT NULL,
    BranchId            UNIQUEIDENTIFIER    NULL,
    ParentDepartmentId  UNIQUEIDENTIFIER    NULL,
    Name                NVARCHAR(200)       NOT NULL,
    NameAr              NVARCHAR(200)       NULL,
    Code                NVARCHAR(20)        NULL,
    IsActive            BIT                 NOT NULL DEFAULT 1,
    IsDeleted           BIT                 NOT NULL DEFAULT 0,
    DeletedAt           DATETIMEOFFSET      NULL,
    DeletedBy           NVARCHAR(200)       NULL,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    UpdatedAt           DATETIMEOFFSET      NULL,
    UpdatedBy           NVARCHAR(200)       NULL,
    RowVersion          ROWVERSION          NOT NULL,
    CONSTRAINT PK_Departments PRIMARY KEY (Id),
    CONSTRAINT FK_Departments_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT FK_Departments_Companies FOREIGN KEY (CompanyId) REFERENCES Companies(Id),
    CONSTRAINT FK_Departments_Branches FOREIGN KEY (BranchId) REFERENCES Branches(Id),
    CONSTRAINT FK_Departments_Parent FOREIGN KEY (ParentDepartmentId) REFERENCES Departments(Id)
);
CREATE INDEX IX_Departments_TenantId ON Departments(TenantId) WHERE IsDeleted = 0;
CREATE INDEX IX_Departments_CompanyId ON Departments(CompanyId) WHERE IsDeleted = 0;
```

---

# 7. كيان AppUser (المستخدم)

## تحليل الأعمال

المستخدم هو أي شخص يدخل إلى النظام.
المستخدم مرتبط بمستأجر واحد ويمكنه الوصول لعدة فروع.
جميع عمليات المستخدم تُسجَّل في سجل التدقيق.

## قواعد العمل

- كلمة المرور لا تُخزَّن كنص صريح أبداً
- قفل الحساب تلقائياً بعد 5 محاولات فاشلة
- البريد الإلكتروني فريد داخل نفس المستأجر
- المستخدم المُوقَف لا يمكنه تسجيل الدخول
- إيقاف المستخدم لا يحذف بياناته أو سجلاته

## Domain Events

- UserCreatedEvent
- UserDeactivatedEvent
- UserLockedEvent
- UserPasswordChangedEvent

## جدول SQL Server

```sql
CREATE TABLE AppUsers (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    Email               NVARCHAR(200)       NOT NULL,
    PasswordHash        NVARCHAR(500)       NOT NULL,
    FirstName           NVARCHAR(100)       NOT NULL,
    LastName            NVARCHAR(100)       NOT NULL,
    PhoneNumber         NVARCHAR(20)        NULL,
    AvatarUrl           NVARCHAR(500)       NULL,
    PinCode             NVARCHAR(10)        NULL,
    PreferredLanguage   NVARCHAR(10)        NULL DEFAULT 'ar',
    IsActive            BIT                 NOT NULL DEFAULT 1,
    IsEmailVerified     BIT                 NOT NULL DEFAULT 0,
    LastLoginAt         DATETIMEOFFSET      NULL,
    FailedLoginCount    TINYINT             NOT NULL DEFAULT 0,
    LockedUntil         DATETIMEOFFSET      NULL,
    MustChangePassword  BIT                 NOT NULL DEFAULT 0,
    IsDeleted           BIT                 NOT NULL DEFAULT 0,
    DeletedAt           DATETIMEOFFSET      NULL,
    DeletedBy           NVARCHAR(200)       NULL,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    UpdatedAt           DATETIMEOFFSET      NULL,
    UpdatedBy           NVARCHAR(200)       NULL,
    RowVersion          ROWVERSION          NOT NULL,
    CONSTRAINT PK_AppUsers PRIMARY KEY (Id),
    CONSTRAINT FK_AppUsers_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT UQ_AppUsers_TenantId_Email UNIQUE (TenantId, Email)
);
CREATE INDEX IX_AppUsers_TenantId ON AppUsers(TenantId) WHERE IsDeleted = 0;
CREATE INDEX IX_AppUsers_Email ON AppUsers(Email);
```

---

# 8. كيان Role (الدور)

## جدول SQL Server

```sql
CREATE TABLE Roles (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NULL,
    Name                NVARCHAR(100)       NOT NULL,
    NameAr              NVARCHAR(100)       NULL,
    Description         NVARCHAR(300)       NULL,
    IsSystem            BIT                 NOT NULL DEFAULT 0,
    IsActive            BIT                 NOT NULL DEFAULT 1,
    IsDeleted           BIT                 NOT NULL DEFAULT 0,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    UpdatedAt           DATETIMEOFFSET      NULL,
    UpdatedBy           NVARCHAR(200)       NULL,
    RowVersion          ROWVERSION          NOT NULL,
    CONSTRAINT PK_Roles PRIMARY KEY (Id),
    CONSTRAINT FK_Roles_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id)
);
CREATE INDEX IX_Roles_TenantId ON Roles(TenantId);
```

---

# 9. كيان Permission (الصلاحية)

## جدول SQL Server

```sql
CREATE TABLE Permissions (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    Module              NVARCHAR(100)       NOT NULL,
    Name                NVARCHAR(100)       NOT NULL,
    DisplayName         NVARCHAR(200)       NOT NULL,
    DisplayNameAr       NVARCHAR(200)       NULL,
    Description         NVARCHAR(300)       NULL,
    CONSTRAINT PK_Permissions PRIMARY KEY (Id),
    CONSTRAINT UQ_Permissions_Name UNIQUE (Name)
);
```

---

# 10. كيان RolePermission (ربط الدور بالصلاحية)

```sql
CREATE TABLE RolePermissions (
    RoleId          UNIQUEIDENTIFIER    NOT NULL,
    PermissionId    UNIQUEIDENTIFIER    NOT NULL,
    CONSTRAINT PK_RolePermissions PRIMARY KEY (RoleId, PermissionId),
    CONSTRAINT FK_RP_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_RP_Permissions FOREIGN KEY (PermissionId) REFERENCES Permissions(Id) ON DELETE CASCADE
);
```

---

# 11. كيان UserRole (ربط المستخدم بالدور)

```sql
CREATE TABLE UserRoles (
    UserId          UNIQUEIDENTIFIER    NOT NULL,
    RoleId          UNIQUEIDENTIFIER    NOT NULL,
    TenantId        UNIQUEIDENTIFIER    NOT NULL,
    AssignedAt      DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    AssignedBy      NVARCHAR(200)       NULL,
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UR_Users FOREIGN KEY (UserId) REFERENCES AppUsers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UR_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
);
```

---

# 12. كيان UserBranch (ربط المستخدم بالفرع)

```sql
CREATE TABLE UserBranches (
    UserId          UNIQUEIDENTIFIER    NOT NULL,
    BranchId        UNIQUEIDENTIFIER    NOT NULL,
    TenantId        UNIQUEIDENTIFIER    NOT NULL,
    IsDefault       BIT                 NOT NULL DEFAULT 0,
    GrantedAt       DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    GrantedBy       NVARCHAR(200)       NULL,
    CONSTRAINT PK_UserBranches PRIMARY KEY (UserId, BranchId),
    CONSTRAINT FK_UB_Users FOREIGN KEY (UserId) REFERENCES AppUsers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UB_Branches FOREIGN KEY (BranchId) REFERENCES Branches(Id)
);
```

---

# 13. كيان Currency (العملة)

```sql
CREATE TABLE Currencies (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    Code                NVARCHAR(3)         NOT NULL,
    Name                NVARCHAR(100)       NOT NULL,
    NameAr              NVARCHAR(100)       NULL,
    Symbol              NVARCHAR(10)        NOT NULL,
    DecimalPlaces       TINYINT             NOT NULL DEFAULT 2,
    IsActive            BIT                 NOT NULL DEFAULT 1,
    CONSTRAINT PK_Currencies PRIMARY KEY (Id),
    CONSTRAINT UQ_Currencies_Code UNIQUE (Code)
);
```

---

# 14. كيان Language (اللغة)

```sql
CREATE TABLE Languages (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    Code                NVARCHAR(10)        NOT NULL,
    Name                NVARCHAR(100)       NOT NULL,
    NativeName          NVARCHAR(100)       NOT NULL,
    IsRtl               BIT                 NOT NULL DEFAULT 0,
    IsActive            BIT                 NOT NULL DEFAULT 1,
    CONSTRAINT PK_Languages PRIMARY KEY (Id),
    CONSTRAINT UQ_Languages_Code UNIQUE (Code)
);
```

---

# 15. كيان Timezone (المنطقة الزمنية)

```sql
CREATE TABLE Timezones (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    SystemId            NVARCHAR(100)       NOT NULL,
    DisplayName         NVARCHAR(200)       NOT NULL,
    UtcOffset           NVARCHAR(10)        NOT NULL,
    IsActive            BIT                 NOT NULL DEFAULT 1,
    CONSTRAINT PK_Timezones PRIMARY KEY (Id),
    CONSTRAINT UQ_Timezones_SystemId UNIQUE (SystemId)
);
```

---

# 16. كيان BusinessHour (ساعات العمل)

```sql
CREATE TABLE BusinessHours (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    BranchId            UNIQUEIDENTIFIER    NOT NULL,
    DayOfWeek           TINYINT             NOT NULL,
    OpenTime            TIME                NOT NULL,
    CloseTime           TIME                NOT NULL,
    IsClosed            BIT                 NOT NULL DEFAULT 0,
    CONSTRAINT PK_BusinessHours PRIMARY KEY (Id),
    CONSTRAINT FK_BH_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT FK_BH_Branches FOREIGN KEY (BranchId) REFERENCES Branches(Id),
    CONSTRAINT UQ_BusinessHours_Branch_Day UNIQUE (BranchId, DayOfWeek)
);
CREATE INDEX IX_BusinessHours_BranchId ON BusinessHours(BranchId);
```

---

# 17. كيان Holiday (العطلة الرسمية)

```sql
CREATE TABLE Holidays (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    CompanyId           UNIQUEIDENTIFIER    NOT NULL,
    Name                NVARCHAR(200)       NOT NULL,
    NameAr              NVARCHAR(200)       NULL,
    Date                DATE                NOT NULL,
    IsRecurringYearly   BIT                 NOT NULL DEFAULT 0,
    IsDeleted           BIT                 NOT NULL DEFAULT 0,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    CONSTRAINT PK_Holidays PRIMARY KEY (Id),
    CONSTRAINT FK_Holidays_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT FK_Holidays_Companies FOREIGN KEY (CompanyId) REFERENCES Companies(Id)
);
CREATE INDEX IX_Holidays_CompanyId_Date ON Holidays(CompanyId, Date) WHERE IsDeleted = 0;
```

---

# 18. كيان Device (الجهاز)

## تحليل الأعمال

الجهاز يمثل كل جهاز مسجل في النظام (كاشير، طابعة، شاشة مطبخ).
لكل جهاز رمز تفعيل وحد أقصى بحسب الاشتراك.
عند الوصول للإنترنت، يقوم الجهاز بمزامنة بياناته المحلية.

## جدول SQL Server

```sql
CREATE TABLE Devices (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    Name                NVARCHAR(200)       NOT NULL,
    DeviceType          TINYINT             NOT NULL,
    SerialNumber        NVARCHAR(100)       NULL,
    MacAddress          NVARCHAR(50)        NULL,
    ActivationCode      NVARCHAR(50)        NOT NULL,
    IsActivated         BIT                 NOT NULL DEFAULT 0,
    ActivatedAt         DATETIMEOFFSET      NULL,
    LastSyncAt          DATETIMEOFFSET      NULL,
    IsOnline            BIT                 NOT NULL DEFAULT 0,
    IsActive            BIT                 NOT NULL DEFAULT 1,
    IsDeleted           BIT                 NOT NULL DEFAULT 0,
    DeletedAt           DATETIMEOFFSET      NULL,
    DeletedBy           NVARCHAR(200)       NULL,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    UpdatedAt           DATETIMEOFFSET      NULL,
    UpdatedBy           NVARCHAR(200)       NULL,
    RowVersion          ROWVERSION          NOT NULL,
    CONSTRAINT PK_Devices PRIMARY KEY (Id),
    CONSTRAINT FK_Devices_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT UQ_Devices_ActivationCode UNIQUE (ActivationCode)
);
CREATE INDEX IX_Devices_TenantId ON Devices(TenantId) WHERE IsDeleted = 0;
```

---

# 19. كيان BranchDevice (ربط الجهاز بالفرع)

```sql
CREATE TABLE BranchDevices (
    BranchId            UNIQUEIDENTIFIER    NOT NULL,
    DeviceId            UNIQUEIDENTIFIER    NOT NULL,
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    AssignedAt          DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    AssignedBy          NVARCHAR(200)       NULL,
    CONSTRAINT PK_BranchDevices PRIMARY KEY (BranchId, DeviceId),
    CONSTRAINT FK_BD_Branches FOREIGN KEY (BranchId) REFERENCES Branches(Id),
    CONSTRAINT FK_BD_Devices FOREIGN KEY (DeviceId) REFERENCES Devices(Id)
);
```

---

# 20. كيان WorkingShift (وردية العمل)

```sql
CREATE TABLE WorkingShifts (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    BranchId            UNIQUEIDENTIFIER    NOT NULL,
    Name                NVARCHAR(200)       NOT NULL,
    NameAr              NVARCHAR(200)       NULL,
    StartTime           TIME                NOT NULL,
    EndTime             TIME                NOT NULL,
    IsOvernight         BIT                 NOT NULL DEFAULT 0,
    IsActive            BIT                 NOT NULL DEFAULT 1,
    IsDeleted           BIT                 NOT NULL DEFAULT 0,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    UpdatedAt           DATETIMEOFFSET      NULL,
    UpdatedBy           NVARCHAR(200)       NULL,
    RowVersion          ROWVERSION          NOT NULL,
    CONSTRAINT PK_WorkingShifts PRIMARY KEY (Id),
    CONSTRAINT FK_WS_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT FK_WS_Branches FOREIGN KEY (BranchId) REFERENCES Branches(Id)
);
CREATE INDEX IX_WorkingShifts_BranchId ON WorkingShifts(BranchId) WHERE IsDeleted = 0;
```

---

# 21. كيان EmployeePosition (المسمى الوظيفي)

```sql
CREATE TABLE EmployeePositions (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    CompanyId           UNIQUEIDENTIFIER    NOT NULL,
    DepartmentId        UNIQUEIDENTIFIER    NULL,
    Name                NVARCHAR(200)       NOT NULL,
    NameAr              NVARCHAR(200)       NULL,
    Description         NVARCHAR(300)       NULL,
    IsActive            BIT                 NOT NULL DEFAULT 1,
    IsDeleted           BIT                 NOT NULL DEFAULT 0,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    UpdatedAt           DATETIMEOFFSET      NULL,
    UpdatedBy           NVARCHAR(200)       NULL,
    RowVersion          ROWVERSION          NOT NULL,
    CONSTRAINT PK_EmployeePositions PRIMARY KEY (Id),
    CONSTRAINT FK_EP_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT FK_EP_Companies FOREIGN KEY (CompanyId) REFERENCES Companies(Id),
    CONSTRAINT FK_EP_Departments FOREIGN KEY (DepartmentId) REFERENCES Departments(Id)
);
CREATE INDEX IX_EmployeePositions_CompanyId ON EmployeePositions(CompanyId) WHERE IsDeleted = 0;
```

---

# ملخص الكيانات والملفات

| # | الكيان | النوع | المجلد |
|---|--------|--------|--------|
| 1 | Tenant | Aggregate Root | Organization/ |
| 2 | SubscriptionPlan | Aggregate Root | Organization/ |
| 3 | Subscription | Entity داخل Tenant | Organization/ |
| 4 | Company | Aggregate Root | Organization/ |
| 5 | Branch | Aggregate Root | Organization/ |
| 6 | Department | Aggregate Root | Organization/ |
| 7 | AppUser | Aggregate Root | Identity/ |
| 8 | Role | Aggregate Root | Identity/ |
| 9 | Permission | Entity | Identity/ |
| 10 | RolePermission | Entity ربط | Identity/ |
| 11 | UserRole | Entity ربط | Identity/ |
| 12 | UserBranch | Entity ربط | Identity/ |
| 13 | Currency | Value Config | Settings/ |
| 14 | Language | Value Config | Settings/ |
| 15 | Timezone | Value Config | Settings/ |
| 16 | BusinessHour | Entity داخل Branch | Organization/ |
| 17 | Holiday | Aggregate Root | Organization/ |
| 18 | Device | Aggregate Root | Organization/ |
| 19 | BranchDevice | Entity ربط | Organization/ |
| 20 | WorkingShift | Aggregate Root | Organization/ |
| 21 | EmployeePosition | Aggregate Root | Organization/ |

---

الحالة: الوثيقة مكتملة — الكود قيد الإنشاء
